import React, { useState, useEffect } from 'react'
import Navbar from '../components/Navbar'
import Footer from '../components/Footer'
import CTA from '../components/CTA'
import * as bookingApi from '../api/booking'

export default function BookingDetail() {
  const [order, setOrder] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)

  useEffect(() => {
    const fetchOrderDetails = async () => {
      try {
        setLoading(true)
        // Extract query params from hash (e.g., #booking?orderId=123)
        const hash = window.location.hash.substring(1)
        const hashParams = new URLSearchParams(hash.split('?')[1] || '')
        const orderIdParam = hashParams.get('orderId')
        const pendingBooking = JSON.parse(localStorage.getItem('pending_booking') || '{}')
        const activeOrder = localStorage.getItem('active_order')
        const orderId = Number(orderIdParam || pendingBooking.orderId || activeOrder)

        console.log('[BookingDetail] Hash:', hash, 'OrderIdParam:', orderIdParam, 'PendingOrderId:', pendingBooking.orderId, 'ActiveOrder:', activeOrder, 'FinalOrderId:', orderId)

        if (!orderId || isNaN(orderId)) {
          setError('Order code not found')
          return
        }

        const authToken = localStorage.getItem('auth.token')
        if (!authToken) {
          setError('Please log in')
          window.location.hash = 'login'
          return
        }

        const { data } = await bookingApi.getOrderById(orderId, authToken)
        setOrder(data)
        setError(null)
      } catch (err) {
        console.error('Error fetching order details:', err)
        setError(err.message || 'Unable to load order details')
      } finally {
        setLoading(false)
      }
    }

    fetchOrderDetails()

    // Listen for hash changes to refetch when user navigates to different order
    const handleHashChange = () => {
      fetchOrderDetails()
    }
    window.addEventListener('hashchange', handleHashChange)
    return () => window.removeEventListener('hashchange', handleHashChange)
  }, [])

  const getStatusBadgeClass = (status) => {
    const statusMap = {
      'Pending': 'yellow',
      'Confirmed': 'blue',
      'InProgress': 'green',
      'Completed': 'green',
      'Cancelled': 'red',
    }
    return statusMap[status] || 'gray'
  }

  const getNextActions = (status) => {
    const currentStatus = status || order?.status || order?.Status
    const orderId = order?.orderId || order?.OrderId

    if (currentStatus === 'Confirmed' || currentStatus === 'Pending') {
      return (
        <>
          <CTA as="a" href={`#check-in?orderId=${orderId}`} variant="primary">Check-in to Receive Vehicle</CTA>
          <CTA as="a" href="#booking-new" variant="secondary">Book New Vehicle</CTA>
        </>
      )
    }

    if (currentStatus === 'InProgress') {
      return (
        <>
          <CTA as="a" href={`#return?orderId=${orderId}`} variant="primary">Return Vehicle</CTA>
          <CTA as="a" href="#booking-new" variant="secondary">Book New Vehicle</CTA>
        </>
      )
    }
    if (currentStatus === 'Completed') {
      return (
        <>
          <CTA as="a" href="#feedback" variant="primary">Send Feedback</CTA>
          <CTA as="a" href="#booking-new" variant="secondary">Book New Vehicle</CTA>
        </>
      )
    }
    return (
      <CTA as="a" href="#booking-new" variant="primary">Book New Vehicle</CTA>
    )
  }

  if (loading) {
    return (
      <div data-figma-layer="Booking Detail Page">
        <Navbar />
        <main>
          <section className="section page-offset">
            <div className="container">
              <div className="text-center pad-y-4">
                <p>Loading...</p>
              </div>
            </div>
          </section>
        </main>
        <Footer />
      </div>
    )
  }

  if (error) {
    return (
      <div data-figma-layer="Booking Detail Page">
        <Navbar />
        <main>
          <section className="section page-offset">
            <div className="container">
              <div className="card">
                <div className="card-body">
                  <div className="error-message">
                    <span>{error}</span>
                  </div>
                  <CTA as="a" href="#booking" className="mt-4">Back to List</CTA>
                </div>
              </div>
            </div>
          </section>
        </main>
        <Footer />
      </div>
    )
  }

  if (!order) {
    return (
      <div data-figma-layer="Booking Detail Page">
        <Navbar />
        <main>
          <section className="section page-offset">
            <div className="container">
              <div className="card">
                <div className="card-body text-center">
                  <p>Order information not found</p>
                  <CTA as="a" href="#booking" className="mt-4">Back</CTA>
                </div>
              </div>
            </div>
          </section>
        </main>
        <Footer />
      </div>
    )
  }

  const status = order?.status || order?.Status || 'Unknown'
  const orderId = order?.orderId || order?.OrderId
  const fromDate = new Date(order?.fromDate || order?.FromDate)
  const toDate = new Date(order?.toDate || order?.ToDate)

  return (
    <div data-figma-layer="Booking Detail Page">
      <Navbar />
      <main>
        <section id="booking" className="section page-offset" aria-labelledby="booking-detail-title">
          <div className="container">
            <div className="section-header">
              <h1 id="booking-detail-title" className="section-title">Booking Details</h1>
              <p className="section-subtitle">Track status and next actions.</p>
            </div>

            <div className="card">
              <div className="card-body">
                <div className="row-between mb-6">
                  <h3 className="card-title">Order ID: #{orderId}</h3>
                  <span className={`badge ${getStatusBadgeClass(status)}`}>{status}</span>
                </div>

                <div className="two-col-grid mb-8">
                  <div>
                    <p className="muted-small mb-0">Pickup Time</p>
                    <p className="value-strong">
                      {fromDate.toLocaleString('vi-VN')}
                    </p>
                  </div>
                  <div>
                    <p className="muted-small mb-0">Return Time</p>
                    <p className="value-strong">
                      {toDate.toLocaleString('vi-VN')}
                    </p>
                  </div>
                </div>

                {order?.vehicle && (
                  <div className="two-col-grid mb-8">
                    <div>
                      <p className="muted-small mb-0">Vehicle</p>
                      <p className="value-strong">
                        {order.vehicle.model || order.Vehicle?.Model || 'N/A'}
                      </p>
                    </div>
                    <div>
                      <p className="muted-small mb-0">Color</p>
                      <p className="value-strong">
                        {order.vehicle.color || order.Vehicle?.Color || 'N/A'}
                      </p>
                    </div>
                  </div>
                )}

                <div className="two-col-grid mb-8">
                  <div>
                    <p className="muted-small mb-0">Total Amount</p>
                    <p className="value-strong value-accent">
                      ${Number(order?.totalCost || order?.TotalCost || 0).toFixed(2)}
                    </p>
                  </div>
                  <div>
                    <p className="muted-small mb-0">Payment Method</p>
                    <p className="value-strong">
                      {order?.paymentMethod || order?.PaymentMethod || 'VNPay'}
                    </p>
                  </div>
                </div>

                <hr className="divider-lg" />

                <div className="row" aria-label="Next actions">
                  {getNextActions(status)}
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
