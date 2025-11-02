import React, { useState, useEffect } from 'react'
import Navbar from '../components/Navbar'
import Footer from '../components/Footer'
import CTA from '../components/CTA'
import * as bookingApi from '../api/booking'

export default function History() {
  const [orders, setOrders] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)

  useEffect(() => {
    const fetchHistory = async () => {
      try {
        setLoading(true)
        const authUser = localStorage.getItem('auth.user')
        const authToken = localStorage.getItem('auth.token')

        if (!authUser || !authToken) {
          setError('Please log in to view history')
          return
        }

        const user = JSON.parse(authUser)
        const userId = Number(user?.userId || user?.UserId || user?.id || user?.Id)

        if (!userId || isNaN(userId)) {
          setError('Unable to determine user ID')
          return
        }

        const { data } = await bookingApi.getOrdersByUserId(userId, authToken)
        const ordersList = Array.isArray(data) ? data : []
        setOrders(ordersList)
        setError(null)
      } catch (err) {
        console.error('Error fetching booking history:', err)
        setError(err.message || 'Unable to load rental history')
      } finally {
        setLoading(false)
      }
    }

    fetchHistory()
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

  const getActionButton = (order) => {
    const status = order.status || order.Status
    const orderId = order.orderId || order.OrderId

    if (status === 'Pending') {
      return (
        <CTA
          as="button"
          onClick={() => {
            localStorage.removeItem('activeOrder')
            localStorage.setItem('pending_booking', JSON.stringify(order))
            window.location.hash = 'payment'
          }}
          variant="primary"
        >
          Pay Now
        </CTA>
      )
    }

    if (status === 'Confirmed') {
      return (
        <CTA
          as="a"
          href={`#check-in?orderId=${orderId}`}
          variant="primary"
        >
          Check-in
        </CTA>
      )
    }

    if (status === 'InProgress') {
      return (
        <CTA
          as="a"
          href={`#return?orderId=${orderId}`}
          variant="primary"
        >
          Return Vehicle
        </CTA>
      )
    }

    if (status === 'Completed') {
      return (
        <CTA
          as="a"
          href="#feedback"
          variant="primary"
        >
          Rate
        </CTA>
      )
    }

    return (
      <CTA
        as="button"
        onClick={() => {
          const orderId = order.orderId || order.OrderId
          console.log('[History] Viewing order:', orderId)
          localStorage.removeItem('activeOrder')
          localStorage.setItem('pending_booking', JSON.stringify(order))
          window.location.hash = `booking?orderId=${orderId}`
        }}
        variant="secondary"
      >
        View
      </CTA>
    )
  }

  return (
    <div data-figma-layer="History Page">
      <Navbar />
      <main>
        <section id="history" className="section page-offset" aria-labelledby="history-title">
          <div className="container">
            <div className="section-header">
              <h1 id="history-title" className="section-title">Rental History</h1>
              <p className="section-subtitle">Review your rentals and expenses.</p>
            </div>

            {loading && (
              <div className="card">
                <div className="card-body text-center">
                  <p>Loading history...</p>
                </div>
              </div>
            )}

            {error && (
              <div className="card">
                <div className="card-body">
                  <div className="error-message" style={{ marginBottom: 0 }}>
                    <span>{error}</span>
                  </div>
                </div>
              </div>
            )}

            {!loading && !error && orders.length === 0 && (
              <div className="card">
                <div className="card-body text-center">
                  <p className="card-subtext">You have no rental history yet.</p>
                  <CTA as="a" href="#booking-new" style={{ marginTop: '1rem' }}>Book Now</CTA>
                </div>
              </div>
            )}

            {!loading && !error && orders.length > 0 && (
              <div className="card">
                <div className="card-body">
                  <ul className="history-list" role="list">
                    {orders.map((order) => (
                      <li key={order.orderId || order.OrderId} className="history-item">
                        <div className="row-between">
                          <div>
                            <h3 className="card-title">#{order.orderId || order.OrderId}</h3>
                            <p className="card-subtext">
                              {new Date(order.fromDate || order.FromDate).toLocaleDateString('vi-VN')} •
                              {order.vehicle?.model || order.Vehicle?.Model || 'N/A'} •
                              ${Number(order.totalCost || order.TotalCost || 0).toFixed(2)}
                            </p>
                          </div>
                          <div className="row">
                            <span className={`badge ${getStatusBadgeClass(order.status || order.Status)}`}>
                              {order.status || order.Status || 'Unknown'}
                            </span>
                            {getActionButton(order)}
                          </div>
                        </div>
                      </li>
                    ))}
                  </ul>
                </div>
              </div>
            )}

          </div>
        </section>
      </main>
      <Footer />
    </div>
  )
}
