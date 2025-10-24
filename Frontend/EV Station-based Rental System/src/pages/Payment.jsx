import React, { useState, useEffect } from 'react'
import Navbar from '../components/Navbar'
import Footer from '../components/Footer'
import CTA from '../components/CTA'
import * as bookingApi from '../api/booking'

export default function Payment() {
  const [booking, setBooking] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
  const [paymentProcessing, setPaymentProcessing] = useState(false)
  const [paymentError, setPaymentError] = useState(null)
  const [token, setToken] = useState(null)
  const [paymentStatus, setPaymentStatus] = useState(null)

  useEffect(() => {
    async function init() {
      try {
        const authToken = localStorage.getItem('auth.token')
        if (!authToken) {
          setError('Vui lòng đăng nhập')
          window.location.hash = 'login'
          return
        }

        const pendingBooking = localStorage.getItem('pending_booking')
        if (!pendingBooking) {
          setError('Không tìm thấy thông tin đơn hàng')
          window.location.hash = 'booking-new'
          return
        }

        const bookingData = JSON.parse(pendingBooking)
        setBooking(bookingData)
        setToken(authToken)

        // Check URL params for payment callback
        const params = new URLSearchParams(window.location.search)
        const paymentSuccess = params.get('success')
        const orderId = params.get('orderId')

        if (paymentSuccess === 'true' && orderId) {
          setPaymentStatus('success')
        } else if (paymentSuccess === 'false' && orderId) {
          setPaymentStatus('failed')
          setError('Thanh toán thất bại. Vui lòng thử lại.')
        }

        setLoading(false)
      } catch (err) {
        console.error('Error initializing payment:', err)
        setError(err.message || 'Lỗi khi tải trang thanh toán')
        setLoading(false)
      }
    }

    init()
  }, [])

  async function handleCreatePayment() {
    if (!booking) {
      setPaymentError('Không tìm thấy thông tin đơn hàng')
      return
    }

    try {
      setPaymentProcessing(true)
      setPaymentError(null)

      // Get VNPay payment URL
      const paymentRes = await bookingApi.createVNPayURL(booking.orderId, token)

      if (paymentRes.data && paymentRes.data.paymentUrl) {
        // Redirect to VNPay
        window.location.href = paymentRes.data.paymentUrl
      } else {
        setPaymentError('Không thể tạo URL thanh toán')
      }
    } catch (err) {
      console.error('Error creating payment:', err)
      setPaymentError(err.message || 'Lỗi khi tạo thanh toán. Vui lòng thử lại.')
    } finally {
      setPaymentProcessing(false)
    }
  }

  function handleConfirmSuccess() {
    // Clear the pending booking
    localStorage.removeItem('pending_booking')
    // Navigate to booking details
    window.location.hash = 'booking'
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
          <section className="section">
            <div className="container">
              <div className="text-center" style={{ padding: '4rem 0' }}>
                <p style={{ fontSize: '1.8rem' }}>Đang tải...</p>
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
          <section className="section">
            <div className="container">
              <div className="card">
                <div className="card-body" style={{ textAlign: 'center', padding: '4rem' }}>
                  <div style={{ fontSize: '4rem', marginBottom: '1rem' }}>✅</div>
                  <h2 className="card-title" style={{ color: '#2a6817', marginBottom: '1rem' }}>Thanh toán thành công!</h2>
                  <p className="card-subtext" style={{ marginBottom: '2rem', fontSize: '1.6rem' }}>
                    Đơn hàng của bạn đã được xác nhận. <br />
                    Mã đơn hàng: <strong>#{booking?.orderId}</strong>
                  </p>
                  <p className="card-subtext" style={{ marginBottom: '2rem' }}>
                    Tổng tiền: <strong style={{ fontSize: '1.8rem', color: '#ff4d30' }}>
                      ${booking?.totalAmount?.toFixed(2)}
                    </strong>
                  </p>
                  <CTA as="button" onClick={handleConfirmSuccess} variant="primary">
                    Xem chi tiết đơn hàng
                  </CTA>
                </div>
              </div>
            </div>
          </section>
        </main>
        <Footer />
      </div>
    )
  }

  if (paymentStatus === 'failed') {
    return (
      <div data-figma-layer="Payment Failed Page">
        <Navbar />
        <main>
          <section className="section">
            <div className="container">
              <div className="card">
                <div className="card-body" style={{ textAlign: 'center', padding: '4rem' }}>
                  <div style={{ fontSize: '4rem', marginBottom: '1rem' }}>❌</div>
                  <h2 className="card-title" style={{ color: '#d32f2f', marginBottom: '1rem' }}>Thanh toán thất bại</h2>
                  <p className="card-subtext" style={{ marginBottom: '2rem', fontSize: '1.6rem' }}>
                    {error}
                  </p>
                  <p className="card-subtext" style={{ marginBottom: '2rem' }}>
                    Mã đơn hàng: <strong>#{booking?.orderId}</strong>
                  </p>
                  <div style={{ display: 'flex', gap: '1rem', justifyContent: 'center' }}>
                    <CTA as="button" onClick={handleRetry} variant="primary">
                      Thử lại thanh toán
                    </CTA>
                    <CTA as="a" href="#booking-new" variant="secondary">
                      Đặt xe mới
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
        <section className="section">
          <div className="container">
            <div className="section-header">
              <h1 className="section-title">Thanh toán</h1>
              <p className="section-subtitle">Hoàn tất thanh toán để xác nhận đơn hàng của bạn.</p>
            </div>

            {error && (
              <div className="error-message" style={{ display: 'flex' }}>
                <span>{error}</span>
              </div>
            )}

            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '2rem', marginBottom: '2rem' }}>
              {/* Order Summary */}
              <div className="card">
                <div className="card-body">
                  <h3 className="card-title">Tóm tắt đơn hàng</h3>
                  
                  {booking && (
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '1.5rem', marginTop: '1.5rem' }}>
                      <div>
                        <h4 style={{ fontSize: '1.4rem', color: '#666', marginBottom: '0.5rem' }}>Mã đơn hàng</h4>
                        <p style={{ fontSize: '1.6rem', fontWeight: 'bold', color: '#ff4d30' }}>#{booking.orderId}</p>
                      </div>

                      <div>
                        <h4 style={{ fontSize: '1.4rem', color: '#666', marginBottom: '0.5rem' }}>Xe</h4>
                        <p className="card-subtext">{booking.vehicleInfo?.model}</p>
                        <p className="card-subtext">Màu: {booking.vehicleInfo?.color}</p>
                      </div>

                      <div>
                        <h4 style={{ fontSize: '1.4rem', color: '#666', marginBottom: '0.5rem' }}>Điểm thuê</h4>
                        <p className="card-subtext">{booking.vehicleInfo?.station}</p>
                      </div>

                      <div>
                        <h4 style={{ fontSize: '1.4rem', color: '#666', marginBottom: '0.5rem' }}>Thời gian</h4>
                        <p className="card-subtext">
                          Nhận: {new Date(booking.dates?.from).toLocaleString('vi-VN')}
                        </p>
                        <p className="card-subtext">
                          Trả: {new Date(booking.dates?.to).toLocaleString('vi-VN')}
                        </p>
                      </div>

                      <hr style={{ margin: '1rem 0' }} />

                      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                        <h4 style={{ fontSize: '1.8rem', color: '#ff4d30' }}>Tổng thanh toán:</h4>
                        <h2 style={{ fontSize: '2.4rem', color: '#ff4d30', margin: 0 }}>
                          ${booking.totalAmount?.toFixed(2)}
                        </h2>
                      </div>

                      {booking.expiresAt && (
                        <p style={{ 
                          fontSize: '1.4rem', 
                          color: '#d32f2f',
                          backgroundColor: '#ffebee',
                          padding: '1rem',
                          borderRadius: '0.5rem',
                          marginTop: '1rem'
                        }}>
                          ⏰ Thanh toán trước: {new Date(booking.expiresAt).toLocaleString('vi-VN')}
                        </p>
                      )}
                    </div>
                  )}
                </div>
              </div>

              {/* Payment Method */}
              <div className="card">
                <div className="card-body">
                  <h3 className="card-title">Phương thức thanh toán</h3>
                  
                  <div style={{ marginTop: '2rem' }}>
                    <div style={{
                      padding: '2rem',
                      border: '2px solid #ff4d30',
                      borderRadius: '0.8rem',
                      textAlign: 'center',
                      backgroundColor: '#fff5f0'
                    }}>
                      <div style={{ fontSize: '3rem', marginBottom: '1rem' }}>🏦</div>
                      <h4 style={{ fontSize: '1.8rem', marginBottom: '0.5rem' }}>Ví điện tử VNPay</h4>
                      <p className="card-subtext">Thanh toán nhanh chóng và an toàn</p>
                      <p className="card-subtext" style={{ marginTop: '1rem', color: '#2a6817' }}>
                        ✓ Bảo mật 100%
                      </p>
                    </div>

                    {paymentError && (
                      <div className="error-message" style={{ display: 'flex', marginTop: '1.5rem' }}>
                        <span>{paymentError}</span>
                      </div>
                    )}

                    <div style={{ marginTop: '2rem' }}>
                      <CTA 
                        as="button" 
                        onClick={handleCreatePayment}
                        disabled={paymentProcessing}
                        variant="primary"
                      >
                        {paymentProcessing ? 'Đang xử lý...' : 'Thanh toán qua VNPay'}
                      </CTA>
                      <CTA 
                        as="a" 
                        href="#booking-new"
                        variant="secondary"
                        style={{ marginTop: '1rem', display: 'block', textAlign: 'center' }}
                      >
                        Huỷ bỏ
                      </CTA>
                    </div>
                  </div>

                  <div style={{
                    marginTop: '2rem',
                    padding: '1.5rem',
                    backgroundColor: '#f5f5f5',
                    borderRadius: '0.5rem',
                    fontSize: '1.4rem',
                    color: '#666'
                  }}>
                    <h4 style={{ marginBottom: '1rem', fontSize: '1.6rem', color: '#333' }}>ℹ️ Lưu ý</h4>
                    <ul style={{ marginLeft: '1.5rem', lineHeight: '1.8' }}>
                      <li>Bạn sẽ được chuyển hướng đến trang thanh toán VNPay</li>
                      <li>Vui lòng không đóng trình duyệt khi đang thanh toán</li>
                      <li>Sau khi thanh toán thành công, hệ thống sẽ tự động xác nhận đơn hàng</li>
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
