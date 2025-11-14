import React, { useState, useEffect } from 'react'
import Navbar from '../components/Navbar'
import Footer from '../components/Footer'
import CTA from '../components/CTA'
import * as bookingApi from '../api/booking'
import * as signalR from '@microsoft/signalr'
import FeedbackForm from '../components/FeedbackForm'

export default function Payment() {
  const [booking, setBooking] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
  const [paymentProcessing, setPaymentProcessing] = useState(false)
  const [paymentError, setPaymentError] = useState(null)
  const [token, setToken] = useState(null)
  const [paymentStatus, setPaymentStatus] = useState(null)
  const [contractDetails, setContractDetails] = useState(null)
  const [contractUrl, setContractUrl] = useState('')
  const [showFeedback, setShowFeedback] = useState(false)
  const [selectedMethod, setSelectedMethod] = useState('VNPay')

  useEffect(() => {
    let connection = null

    async function init() {
      try {
        const authToken = localStorage.getItem('auth.token')
        if (!authToken) {
          setError('Please log in')
          window.location.hash = 'login'
          return
        }

        const pendingBooking = localStorage.getItem('pending_booking')
        console.log('[Payment] Retrieved pending_booking:', pendingBooking)

        if (!pendingBooking) {
          console.error('[Payment] No pending_booking found in localStorage')
          setError('Order information not found. Please start over.')
          setTimeout(() => {
            window.location.hash = 'booking-new'
          }, 1000)
          return
        }

        let bookingData
        try {
          bookingData = JSON.parse(pendingBooking)
          console.log('[Payment] Parsed booking data:', bookingData)
        } catch (parseErr) {
          console.error('[Payment] Error parsing booking data:', parseErr)
          setError('Order data is invalid. Please start over.')
          setTimeout(() => {
            window.location.hash = 'booking-new'
          }, 1000)
          return
        }

        setBooking(bookingData)
        setToken(authToken)

        // Setup SignalR connection to listen for PaymentSuccess
        try {
          const base = (import.meta.env.VITE_BOOKING_API_URL || '').replace(/\/$/, '')
          const hubUrl = `${base}/orderTimerHub`
          connection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl, { accessTokenFactory: () => authToken })
            .withAutomaticReconnect()
            .build()

          connection.on('PaymentSuccess', async (data) => {
            try {
              console.log('[Payment] SignalR PaymentSuccess received', data)
              const incomingOrderId = Number(data?.OrderId || data?.orderId)
              const transactionId = data?.TransactionId || data?.transactionId
              if (!incomingOrderId || incomingOrderId !== bookingData.orderId) {
                console.log('[Payment] Ignoring PaymentSuccess for different order')
                return
              }

              console.log('[Payment] ⭐ Verifying payment status before creating contract...')

              try {
                // ✅ CRITICAL: Fetch order and verify payment is ACTUALLY completed
                const orderResponse = await bookingApi.getOrderById(incomingOrderId, authToken)
                const orderStatus = orderResponse.data?.Status
                // Payments is a collection, get the deposit payment
                const payments = orderResponse.data?.Payments || []
                const payment = Array.isArray(payments) && payments.length > 0 
                  ? payments.find(p => p.Type === 'Deposit' || p.type === 'Deposit') || payments[0]
                  : null

                console.log('[Payment] Order status:', orderStatus)
                console.log('[Payment] Payment status:', payment?.Status || payment?.status)

                // BOTH conditions must be true: order is Confirmed AND payment status is Completed
                const isOrderConfirmed = orderStatus === 'Confirmed'
                const isPaymentCompleted = payment && (payment.Status === 'Completed' || payment.status === 'Completed')
                const isPaymentConfirmed = isOrderConfirmed && isPaymentCompleted

                if (!isPaymentConfirmed) {
                  console.warn('[Payment] ❌ SignalR received but payment NOT fully confirmed!')
                  console.warn('[Payment]   - Order Status:', orderStatus, '(expected: Confirmed)')
                  console.warn('[Payment]   - Payment Status:', payment?.Status, '(expected: Completed)')
                  // ❌ DO NOT create contract - payment not confirmed
                  return
                }

                console.log('[Payment] ✅ Payment confirmed! Creating contract for order', incomingOrderId)

                // Create contract on backend
                const userJson = localStorage.getItem('auth.user') || '{}'
                const user = JSON.parse(userJson)

                const contractData = {
                  OrderId: incomingOrderId,
                  PaidAt: new Date().toISOString(),
                  CustomerName: user.fullName || user.fullname || user.name || 'Guest',
                  CustomerEmail: user.email || '',
                  CustomerPhone: user.phone || user.phoneNumber || '',
                  CustomerIdCard: user.idCard || user.id_card || user.identityNumber || '',
                  CustomerAddress: user.address || '',
                  CustomerDateOfBirth: user.dateOfBirth || user.dob || '',
                  VehicleModel: bookingData.vehicleInfo?.model || 'N/A',
                  LicensePlate: bookingData.vehicleInfo?.licensePlate || 'N/A',
                  VehicleColor: bookingData.vehicleInfo?.color || 'N/A',
                  VehicleType: bookingData.vehicleInfo?.type || 'N/A',
                  FromDate: new Date(bookingData.dates?.from).toISOString(),
                  ToDate: new Date(bookingData.dates?.to).toISOString(),
                  TotalRentalCost: Number(bookingData.totalRentalCost || 0),
                  DepositAmount: Number(bookingData.depositCost || 0),
                  ServiceFee: Number(bookingData.serviceFee || 0),
                  TotalPaymentAmount: Number(bookingData.totalCost || 0),
                  TransactionId: transactionId || '',
                  PaymentMethod: selectedMethod,
                  PaymentDate: new Date().toISOString(),
                }

                console.log('[Payment] Contract data prepared:', contractData)

                try {
                  const res = await bookingApi.createContract(contractData, authToken)
                  console.log('[Payment] Contract creation response:', res)

                  if (res && res.data) {
                    const contractUrl = res.data.downloadUrl || res.data.DownloadUrl || ''
                    setContractDetails(res.data)
                    setContractUrl(contractUrl)
                    console.log('[Payment] Contract URL received:', contractUrl)
                    localStorage.removeItem('pending_booking')
                    localStorage.setItem('active_order', String(incomingOrderId))
                    setPaymentStatus('success')
                  } else {
                    console.warn('[Payment] No contract URL in response')
                    setPaymentStatus('success')
                  }
                } catch (contractErr) {
                  console.error('[Payment] Contract creation failed via SignalR:', contractErr)
                  // Contract creation failed, but payment succeeded - still show success
                  // The contract may be created via the VNPay callback redirect instead
                  setPaymentStatus('success')
                }
              } catch (verifyErr) {
                console.error('[Payment] Error verifying payment status:', verifyErr)
                // Cannot verify payment - do not create contract to be safe
                console.warn('[Payment] ⚠️ Skipping contract creation due to verification error')
              }
            } catch (err) {
              console.error('[Payment] Error handling PaymentSuccess event:', err)
            }
          })

          await connection.start()
          // Join group for this order so we only receive events for it
          try {
            await connection.invoke('JoinOrderGroup', String(bookingData.orderId))
            console.log('[Payment] Joined SignalR order group', bookingData.orderId)
          } catch (joinErr) {
            console.warn('Could not join SignalR group', joinErr)
          }
        } catch (hubErr) {
          console.warn('SignalR not available or failed to connect:', hubErr)
        }

        // Check URL params for payment callback
        // When returning from VNPay, actively create contract instead of waiting for SignalR
        const params = new URLSearchParams(window.location.search)
        const paymentSuccess = params.get('success')
        const urlOrderId = params.get('orderId')
        const urlTransactionId = params.get('transactionId')  // ✅ Extract from VNPay redirect

        if (paymentSuccess === 'false' && urlOrderId) {
          setPaymentStatus('failed')
          setError('Payment failed. Please try again.')
        } else if (paymentSuccess === 'true' && urlOrderId) {
          // Returning from payment gateway (VNPay or PayOS) - create contract immediately instead of waiting for SignalR
          console.log('[Payment] Payment callback received with success=true, creating contract...')

          try {
            // First, fetch the order to verify payment was completed
            const order = await bookingApi.getOrderById(Number(urlOrderId), authToken)
            console.log('[Payment] Order status on return:', order.data)

            // ✅ CRITICAL: Verify payment is actually CONFIRMED before proceeding
            // Order.Status becomes "Confirmed" after payment completes
            const orderStatus = order.data?.Status
            // Payments is a collection, get the deposit payment (first one or the one with Type=Deposit)
            const payments = order.data?.Payments || []
            const payment = Array.isArray(payments) && payments.length > 0 
              ? payments.find(p => p.Type === 'Deposit' || p.type === 'Deposit') || payments[0]
              : null

            console.log('[Payment] Order status:', orderStatus)
            console.log('[Payment] Payments array:', payments)
            console.log('[Payment] Payment object:', payment)
            console.log('[Payment] Payment status:', payment?.Status || payment?.status)

            // Check if order is confirmed (payment succeeded)
            // BOTH conditions must be true: order is Confirmed AND payment status is Completed
            const isOrderConfirmed = orderStatus === 'Confirmed'
            const isPaymentCompleted = payment && (payment.Status === 'Completed' || payment.status === 'Completed')
            const isPaymentConfirmed = isOrderConfirmed && isPaymentCompleted

            if (!isPaymentConfirmed) {
              console.warn('[Payment] ❌ Payment not fully confirmed yet')
              console.warn('[Payment]   - Order Status:', orderStatus, '(expected: Confirmed)')
              console.warn('[Payment]   - Payment Status:', payment?.Status || payment?.status, '(expected: Completed)')
              console.warn('[Payment]   - Payment found:', !!payment)
              // Don't show success if order isn't confirmed
              // Just show the normal payment page instead
              setLoading(false)
              return
            }

            // Check if contract already exists
            if (order.data && order.data.OnlineContract && order.data.OnlineContract.ContractFilePath) {
              // Contract already exists - show success immediately
              const contractUrl = order.data.OnlineContract.ContractFilePath
              setContractUrl(contractUrl)
              setContractDetails({ downloadUrl: contractUrl })
              console.log('[Payment] Contract already exists on return:', contractUrl)
              setPaymentStatus('success')
            } else {
              // Contract doesn't exist yet - create it NOW instead of waiting
              console.log('[Payment] Contract not found, creating contract...')

              const userJson = localStorage.getItem('auth.user') || '{}'
              const user = JSON.parse(userJson)

              console.log('[Payment] User data from localStorage:', {
                name: user.fullName || user.fullname || user.name,
                email: user.email,
                phone: user.phone || user.phoneNumber,
                idCard: user.idCard || user.id_card || user.identityNumber,
              })

              // Use booking data from localStorage OR extract from Order API response
              const bookingToUse = bookingData || {
                orderId: Number(urlOrderId),
                vehicleInfo: {},
                dates: {
                  from: order.data?.FromDate,
                  to: order.data?.ToDate,
                },
                totalRentalCost: order.data?.TotalCost || 0,
                depositCost: order.data?.DepositAmount || 0,
                serviceFee: 0, // Not available in Order model directly
                totalCost: order.data?.TotalCost || 0,
              }

              // Calculate service fee if not available
              const rentalCost = Number(bookingToUse.totalRentalCost || 0)
              const depositCost = Number(bookingToUse.depositCost || 0)
              const serviceFee = Number(bookingToUse.serviceFee || 0)
              const totalCost = Number(bookingToUse.totalCost || (rentalCost + depositCost + serviceFee))

              // Validate required financial data
              if (totalCost <= 0) {
                console.error('[Payment] Invalid total cost:', totalCost)
                setError('Error: Unable to determine order cost')
                setLoading(false)
                return
              }

              const contractData = {
                OrderId: Number(urlOrderId),
                PaidAt: new Date().toISOString(),
                CustomerName: user.fullName || user.fullname || user.name || 'Guest',
                CustomerEmail: user.email || '',
                CustomerPhone: user.phone || user.phoneNumber || '',
                CustomerIdCard: user.idCard || user.id_card || user.identityNumber || '',
                CustomerAddress: user.address || '',
                CustomerDateOfBirth: user.dateOfBirth || user.dob || '',
                VehicleModel: bookingToUse.vehicleInfo?.model || order.data?.Vehicle?.Model || 'N/A',
                LicensePlate: bookingToUse.vehicleInfo?.licensePlate || order.data?.Vehicle?.LicensePlate || 'N/A',
                VehicleColor: bookingToUse.vehicleInfo?.color || order.data?.Vehicle?.Color || 'N/A',
                VehicleType: bookingToUse.vehicleInfo?.type || order.data?.Vehicle?.Type || 'N/A',
                FromDate: bookingToUse.dates?.from ? new Date(bookingToUse.dates.from).toISOString() : new Date().toISOString(),
                ToDate: bookingToUse.dates?.to ? new Date(bookingToUse.dates.to).toISOString() : new Date().toISOString(),
                TotalRentalCost: rentalCost,
                DepositAmount: depositCost,
                ServiceFee: serviceFee,
                TotalPaymentAmount: totalCost,
                TransactionId: urlTransactionId || '',  // ✅ Use from payment gateway URL callback
                PaymentMethod: payment?.PaymentMethod || payment?.paymentMethod || selectedMethod || 'PayOS',  // ✅ Get from payment record, fallback to selectedMethod
                PaymentDate: new Date().toISOString(),
              }

              console.log('[Payment] Submitting contract creation with data:', contractData)

              // Validate contract data before sending
              const missingFields = []
              if (!contractData.CustomerName) missingFields.push('CustomerName')
              if (!contractData.CustomerEmail) missingFields.push('CustomerEmail')
              if (!contractData.CustomerPhone) missingFields.push('CustomerPhone')
              // ✅ CustomerIdCard is now optional
              // ✅ TransactionId is now optional - VNPay callback may not provide it

              if (missingFields.length > 0) {
                console.error('[Payment] Missing required fields:', missingFields)
                setError(`Lỗi: Thiếu d��� li��u bắt buộc: ${missingFields.join(', ')}`)
                setLoading(false)
                return
              }

              try {
                const res = await bookingApi.createContract(contractData, authToken)
                console.log('[Payment] Contract creation response:', res)

                if (res && res.data) {
                  const contractUrl = res.data.downloadUrl || res.data.DownloadUrl || ''
                  setContractDetails(res.data)
                  setContractUrl(contractUrl)
                  console.log('[Payment] Contract created successfully:', contractUrl)
                  localStorage.removeItem('pending_booking')
                  localStorage.setItem('active_order', String(Number(urlOrderId)))
                } else {
                  console.warn('[Payment] No contract URL in response')
                }

                setPaymentStatus('success')
              } catch (contractErr) {
                console.error('[Payment] Contract creation error:', contractErr)
                setError(`Lỗi tạo hợp đồng: ${contractErr.message}`)
                setLoading(false)
                return
              }
            }

            // ✅ Clear URL params to prevent re-triggering on page refresh
            window.history.replaceState({}, document.title, window.location.pathname)
          } catch (err) {
            console.error('[Payment] Error handling payment callback:', err)
            // Don't show success if there's an error
            setLoading(false)
          }
        }

        setLoading(false)
      } catch (err) {
        console.error('Error initializing payment:', err)
        setError(err.message || 'Error loading payment page')
        setLoading(false)
      }
    }

    init()

    return () => {
      try {
        if (connection) {
          const pendingBooking = JSON.parse(localStorage.getItem('pending_booking') || '{}')
          const orderId = pendingBooking?.orderId
          if (orderId && connection.invoke) connection.invoke('LeaveOrderGroup', String(orderId)).catch(() => {})
          connection.stop().catch(() => {})
        }
      } catch (e) {
        // ignore
      }
    }
  }, [])

  async function handleCreatePayment() {
    if (!booking) {
      setPaymentError('Order information not found')
      return
    }

    try {
      setPaymentProcessing(true)
      setPaymentError(null)

      if (selectedMethod === 'VNPay') {
        const paymentRes = await bookingApi.createVNPayURL(booking.orderId, token)
        if (paymentRes.data && paymentRes.data.paymentUrl) {
          window.location.href = paymentRes.data.paymentUrl
        } else {
          setPaymentError('Unable to create VNPay payment URL')
        }
      } else {
        const paymentRes = await bookingApi.createPayOSLink(booking.orderId, token)
        if (paymentRes.data && paymentRes.data.paymentUrl) {
          window.location.href = paymentRes.data.paymentUrl
        } else {
          setPaymentError('Unable to create PayOS payment link')
        }
      }
    } catch (err) {
      console.error('Error creating payment:', err)
      setPaymentError(err.message || 'Error creating payment. Please try again.')
    } finally {
      setPaymentProcessing(false)
    }
  }

  function handleConfirmSuccess() {
    // Navigate to booking details
    window.location.hash = 'booking'
  }

  async function handleStartTrip() {
    try {
      if (!booking) return
      const authToken = token || localStorage.getItem('auth.token')
      const orderId = booking.orderId
      await bookingApi.startRental(orderId, authToken)
      // After starting rental, navigate to booking detail
      window.location.hash = 'booking'
    } catch (err) {
      console.error('Error starting trip:', err)
      setPaymentError(err.message || 'Unable to start rental')
    }
  }

  async function handleSubmitFeedback({ rating, comments }) {
    try {
      const authToken = token || localStorage.getItem('auth.token')
      const userId = Number(JSON.parse(localStorage.getItem('auth.user') || '{}')?.userId || 0)
      const orderId = booking?.orderId
      const vehicleId = booking?.vehicleInfo?.vehicleId || 0
      if (!orderId || !userId) throw new Error('Missing order or user id')
      await bookingApi.submitFeedback({
        UserId: userId,
        userId: userId,
        OrderId: orderId,
        orderId: orderId,
        VehicleId: vehicleId,
        vehicleId: vehicleId,
        VehicleRating: rating,
        vehicleRating: rating,
        Comments: comments,
        comments: comments,
      }, authToken)
      setShowFeedback(false)
      // After feedback, navigate to booking list
      window.location.hash = 'booking'
    } catch (err) {
      console.error('Error submitting feedback:', err)
      throw err
    }
  }

  function handleRetry() {
    setPaymentStatus(null)
    setError(null)
  }

  if (loading) {
    return (
      <div data-figma-layer="Payment Page">
        <Navbar />
        <main>
          <section className="section page-offset">
            <div className="container" role="status" aria-busy="true">
              <div className="two-col-grid">
                <div className="card">
                  <div className="card-body">
                    <div className="skeleton skeleton-line"></div>
                    <div className="skeleton skeleton-line"></div>
                    <div className="skeleton skeleton-card"></div>
                  </div>
                </div>
                <div className="card">
                  <div className="card-body">
                    <div className="skeleton skeleton-line"></div>
                    <div className="skeleton skeleton-pill"></div>
                    <div className="skeleton skeleton-line"></div>
                  </div>
                </div>
              </div>
            </div>
          </section>
        </main>
        <Footer />
      </div>
    )
  }

  if (paymentStatus === 'success') {
    return (
      <div data-figma-layer="Payment Success Page">
        <Navbar />
        <main>
          <section className="section page-offset">
            <div className="container">
              <div className="card">
                <div className="card-body center-text pad-4">
                  <div className="icon-xl">✅</div>
                  <h2 className="card-title text-success mb-4">Payment Successful!</h2>
                  <p className="card-subtext mb-8 text-16">
                    Your order has been confirmed. <br />
                    Order ID: <strong>#{booking?.orderId}</strong>
                  </p>
                  <p className="card-subtext mb-4">
                    Total Amount: <strong className="total-value large value-accent">${booking?.totalCost?.toFixed(2)}</strong>
                  </p>

                  {contractUrl ? (
                    <div className="stack">
                      <div className="contract-container">
                        <h3 className="contract-title">📄 Rental Contract</h3>
                        <iframe
                          src={`${contractUrl}#toolbar=0&navpanes=0&scrollbar=0`}
                          className="contract-frame"
                          title="Contract PDF"
                        />
                        <div className="contract-actions">
                          <CTA as="a" href={contractUrl} download target="_blank" rel="noreferrer" className="text-center" variant="secondary">⬇️ Download</CTA>
                          <CTA as="a" href={contractUrl} target="_blank" rel="noreferrer" className="text-center" variant="secondary">👁️ View Full</CTA>
                        </div>
                      </div>
                      <CTA as="button" onClick={handleStartTrip} variant="primary">Start Rental</CTA>
                      <CTA as="button" onClick={() => setShowFeedback(true)} variant="ghost">Leave Rating After Return</CTA>
                    </div>
                  ) : (
                    <div>
                      <p className="card-subtext">Contract is being created �� you will receive an email with download link when complete.</p>
                      <CTA as="button" onClick={handleConfirmSuccess} variant="primary">View Order Details</CTA>
                    </div>
                  )}

                </div>
              </div>
            </div>
          </section>
        </main>
        <FeedbackForm open={showFeedback} onClose={() => setShowFeedback(false)} onSubmit={handleSubmitFeedback} />
        <Footer />
      </div>
    )
  }

  if (paymentStatus === 'failed') {
    return (
      <div data-figma-layer="Payment Failed Page">
        <Navbar />
        <main>
          <section className="section page-offset">
            <div className="container">
              <div className="card">
                <div className="card-body center-text pad-4">
                  <div className="icon-xl mb-4">❌</div>
                  <h2 className="card-title text-error mb-4">Payment Failed</h2>
                  <p className="card-subtext mb-8 text-16">
                    {error}
                  </p>
                  <p className="card-subtext mb-4">
                    Order ID: <strong>#{booking?.orderId}</strong>
                  </p>
                  <div className="row center-justify">
                    <CTA as="button" onClick={handleRetry} variant="primary">
                      Retry Payment
                    </CTA>
                    <CTA as="a" href="#booking-new" variant="secondary">
                      Book New Vehicle
                    </CTA>
                  </div>
                </div>
              </div>
            </div>
          </section>
        </main>
        <Footer />
      </div>
    )
  }

  return (
    <div data-figma-layer="Payment Page">
      <Navbar />
      <main>
        <section className="section page-offset">
          <div className="container">
            <div className="section-header">
              <h1 className="section-title">Payment</h1>
              <p className="section-subtitle">Complete payment to confirm your order.</p>
            </div>

            {error && (
              <div className="error-message error-visible">
                <span>{error}</span>
              </div>
            )}

            <div className="two-col-grid mb-8">
              <div className="card">
                <div className="card-body">
                  <h3 className="card-title">Order Summary</h3>
                  
                  {booking ? (
                    <div className="stack mt-4">
                      <div>
                        <h4 className="muted-title">Order ID</h4>
                        <p className="order-id">{booking.orderId ? `#${booking.orderId}` : 'N/A'}</p>
                      </div>

                      <div>
                        <h4 className="muted-title">Vehicle</h4>
                        <p className="card-subtext mb-3">{booking.vehicleInfo?.model || 'N/A'}</p>
                        <p className="card-subtext">Color: {booking.vehicleInfo?.color || 'N/A'}</p>
                      </div>

                      <div>
                        <h4 className="muted-title">Station</h4>
                        <p className="card-subtext">{booking.vehicleInfo?.station || 'N/A'}</p>
                      </div>

                      <div>
                        <h4 className="muted-title">Time</h4>
                        {booking.dates?.from && (
                          <p className="card-subtext">
                            Pickup: {new Date(booking.dates.from).toLocaleString('vi-VN')}
                          </p>
                        )}
                        {booking.dates?.to && (
                          <p className="card-subtext">
                            Return: {new Date(booking.dates.to).toLocaleString('vi-VN')}
                          </p>
                        )}
                      </div>

                      <hr className="divider-lg" />

                      <div className="breakdown-box">
                        <div className="summary-row">
                          <span className="price-label">Rental Fee:</span>
                          <span className="price-value">{(booking.totalRentalCost || 0).toLocaleString('vi-VN')} ₫</span>
                        </div>
                        {(booking.depositCost || 0) > 0 && (
                          <div className="summary-row">
                            <span className="price-label">Deposit:</span>
                            <span className="price-value">{(booking.depositCost || 0).toLocaleString('vi-VN')} ₫</span>
                          </div>
                        )}
                        {(booking.serviceFee || 0) > 0 && (
                          <div className="summary-row">
                            <span className="price-label">Service Fee:</span>
                            <span className="price-value">{(booking.serviceFee || 0).toLocaleString('vi-VN')} ₫</span>
                          </div>
                        )}
                      </div>

                      <div className="summary-row total-row">
                        <h4 className="total-label">Total Payment:</h4>
                        <h2 className="total-value large">
                          {(booking.totalCost || 0).toLocaleString('vi-VN')} ₫
                        </h2>
                      </div>

                      {booking.expiresAt && (
                        <p className="deadline-box">
                          ⏰ Pay before: {new Date(booking.expiresAt).toLocaleString('vi-VN')}
                        </p>
                      )}
                    </div>
                  ) : (
                    <div className="state-wrapper">
                      <p className="state-message">No order data. Please try again.</p>
                    </div>
                  )}
                </div>
              </div>

              <div className="card">
                <div className="card-body">
                  <h3 className="card-title">Payment Method</h3>

                  <div className="mt-8">
                    <div className="stack">
                      <button
                        type="button"
                        className="payment-method-card"
                        aria-pressed={selectedMethod === 'VNPay'}
                        onClick={() => setSelectedMethod('VNPay')}
                      >
                        <div className="icon-xl">🏦</div>
                        <h4 className="payment-title">VNPay</h4>
                        <p className="card-subtext">Fast and secure payment</p>
                        {selectedMethod === 'VNPay' && <p className="card-subtext text-success mt-4">✓ Selected</p>}
                      </button>

                      <button
                        type="button"
                        className="payment-method-card"
                        aria-pressed={selectedMethod === 'PayOS'}
                        onClick={() => setSelectedMethod('PayOS')}
                      >
                        <div className="icon-xl">💳</div>
                        <h4 className="payment-title">PayOS</h4>
                        <p className="card-subtext">Card and wallet payments</p>
                        {selectedMethod === 'PayOS' && <p className="card-subtext text-success mt-4">✓ Selected</p>}
                      </button>
                    </div>

                    {paymentError && (
                      <div className="error-message error-visible mt-6">
                        <span>{paymentError}</span>
                      </div>
                    )}

                    <div className="mt-8">
                      <CTA
                        as="button"
                        onClick={handleCreatePayment}
                        disabled={paymentProcessing}
                        variant="primary"
                      >
                        {paymentProcessing ? 'Processing...' : (selectedMethod === 'VNPay' ? 'Pay with VNPay' : 'Pay with PayOS')}
                      </CTA>
                      <CTA
                        as="a"
                        href="#booking-new"
                        variant="secondary"
                        className="block-link"
                      >
                        Cancel
                      </CTA>
                    </div>
                  </div>

                  <div className="note-box mt-8">
                    <h4 className="note-title">ℹ️ Note</h4>
                    <ul className="note-list">
                      <li>You will be redirected to the {selectedMethod} payment page</li>
                      <li>Please do not close your browser while paying</li>
                      <li>After successful payment, the system will automatically confirm your order</li>
                    </ul>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </section>
      </main>
      <Footer />
    </div>
  )
}
