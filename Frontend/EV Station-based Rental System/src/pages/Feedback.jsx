import React, { useState, useEffect } from 'react'
import Navbar from '../components/Navbar'
import Footer from '../components/Footer'
import CTA from '../components/CTA'
import * as bookingApi from '../api/booking'
import * as feedbackApi from '../api/feedback'

export default function Feedback() {
  const [orders, setOrders] = useState([])
  const [selectedOrder, setSelectedOrder] = useState(null)
  const [feedback, setFeedback] = useState(null)
  const [rating, setRating] = useState(5)
  const [comments, setComments] = useState('')
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState(null)
  const [success, setSuccess] = useState(false)

  useEffect(() => {
    const fetchOrders = async () => {
      try {
        setLoading(true)
        const authUser = localStorage.getItem('auth.user')
        const authToken = localStorage.getItem('auth.token')

        if (!authUser || !authToken) {
          setError('Please log in to view feedback')
          return
        }

        const user = JSON.parse(authUser)
        const userId = Number(user?.userId || user?.UserId || user?.id || user?.Id)

        if (!userId || isNaN(userId)) {
          setError('Unable to determine user ID')
          return
        }

        const { data } = await bookingApi.getOrdersByUserId(userId, authToken)
        const completedOrders = (Array.isArray(data) ? data : []).filter(
          order => order.status === 'Completed' || order.Status === 'Completed'
        )
        setOrders(completedOrders)
        setError(null)
      } catch (err) {
        console.error('Error fetching orders:', err)
        setError(err.message || 'Unable to load order list')
      } finally {
        setLoading(false)
      }
    }

    fetchOrders()
  }, [])

  const handleSelectOrder = async (order) => {
    setSelectedOrder(order)
    setRating(5)
    setComments('')
    setSuccess(false)

    try {
      const orderId = order.orderId || order.OrderId
      const authToken = localStorage.getItem('auth.token')
      const { data } = await feedbackApi.getFeedbackByOrder(orderId, authToken)
      setFeedback(data)
    } catch (err) {
      console.warn('No feedback found for this order:', err)
      setFeedback(null)
    }
  }

  const handleSubmitFeedback = async (e) => {
    e.preventDefault()

    if (!selectedOrder) {
      setError('Please select an order')
      return
    }

    if (!comments.trim()) {
      setError('Please enter a comment')
      return
    }

    try {
      setSubmitting(true)
      setError(null)

      const authUser = localStorage.getItem('auth.user')
      const authToken = localStorage.getItem('auth.token')
      const user = JSON.parse(authUser)
      const userId = Number(user?.userId || user?.UserId || user?.id || user?.Id)
      const orderId = selectedOrder.orderId || selectedOrder.OrderId
      const vehicleId = selectedOrder.vehicleId || selectedOrder.VehicleId || 0

      await feedbackApi.submitFeedback({
        userId,
        orderId,
        vehicleId,
        vehicleRating: rating,
        comments,
      }, authToken)

      setSuccess(true)
      setRating(5)
      setComments('')

      setTimeout(() => {
        setSuccess(false)
        handleSelectOrder(selectedOrder)
      }, 2000)
    } catch (err) {
      console.error('Error submitting feedback:', err)
      setError(err.message || 'Error sending feedback. Please try again.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div data-figma-layer="Feedback Page">
      <Navbar />
      <main>
        <section id="feedback" className="section page-offset" aria-labelledby="feedback-title">
          <div className="container">
            <div className="section-header">
              <h1 id="feedback-title" className="section-title">Order Feedback</h1>
              <p className="section-subtitle">Share your experience about your rental trip.</p>
            </div>

            {loading && (
              <div className="card">
                <div className="card-body text-center">
                  <p>Loading...</p>
                </div>
              </div>
            )}

            {error && !loading && (
              <div className="card">
                <div className="card-body">
                  <div className="error-message">
                    <span>{error}</span>
                  </div>
                </div>
              </div>
            )}

            {!loading && !error && orders.length === 0 && (
              <div className="card">
                <div className="card-body text-center">
                  <p className="card-subtext">You have not completed any orders.</p>
                </div>
              </div>
            )}

            {!loading && !error && orders.length > 0 && (
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '2rem' }}>
                <div className="card">
                  <div className="card-body">
                    <h3 className="card-title">Order List</h3>
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem', marginTop: '1rem' }}>
                      {orders.map((order) => (
                        <button
                          key={order.orderId || order.OrderId}
                          onClick={() => handleSelectOrder(order)}
                          style={{
                            padding: '1rem',
                            backgroundColor: selectedOrder?.orderId === order.orderId || selectedOrder?.OrderId === order.OrderId ? '#fff5f0' : '#f9f9f9',
                            border: selectedOrder?.orderId === order.orderId || selectedOrder?.OrderId === order.OrderId ? '2px solid #ff4d30' : '1px solid #ddd',
                            borderRadius: '0.5rem',
                            cursor: 'pointer',
                            textAlign: 'left',
                          }}
                        >
                          <p style={{ margin: 0, fontWeight: 'bold', fontSize: '1.4rem' }}>
                            #{order.orderId || order.OrderId}
                          </p>
                          <p style={{ margin: '0.5rem 0 0 0', color: '#666', fontSize: '1.2rem' }}>
                            {new Date(order.fromDate || order.FromDate).toLocaleDateString('vi-VN')}
                          </p>
                          <p style={{ margin: '0.25rem 0 0 0', color: '#999', fontSize: '1rem' }}>
                            {order.vehicle?.model || order.Vehicle?.Model || 'N/A'}
                          </p>
                        </button>
                      ))}
                    </div>
                  </div>
                </div>

                <div>
                  {selectedOrder ? (
                    <div className="card">
                      <div className="card-body">
                        <h3 className="card-title">Feedback for Order #{selectedOrder.orderId || selectedOrder.OrderId}</h3>

                        {success && (
                          <div style={{
                            padding: '1rem',
                            backgroundColor: '#d4edda',
                            color: '#155724',
                            borderRadius: '0.5rem',
                            marginBottom: '1rem',
                            textAlign: 'center',
                          }}>
                            ✅ Feedback sent successfully!
                          </div>
                        )}

                        {feedback && (
                          <div style={{
                            padding: '1rem',
                            backgroundColor: '#e7f3ff',
                            borderRadius: '0.5rem',
                            marginBottom: '1rem',
                          }}>
                            <p style={{ margin: 0, color: '#0066cc', fontSize: '1.2rem' }}>
                              ⭐ Rating: {feedback.vehicleRating || feedback.VehicleRating}/5
                            </p>
                            <p style={{ margin: '0.5rem 0 0 0', color: '#666' }}>
                              {feedback.comments || feedback.Comments}
                            </p>
                          </div>
                        )}

                        {!feedback && (
                          <form onSubmit={handleSubmitFeedback}>
                            <div style={{ marginBottom: '1.5rem' }}>
                              <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500' }}>
                                Vehicle Rating (1-5 stars)
                              </label>
                              <div style={{ display: 'flex', gap: '0.5rem' }}>
                                {[1, 2, 3, 4, 5].map((star) => (
                                  <button
                                    key={star}
                                    type="button"
                                    onClick={() => setRating(star)}
                                    style={{
                                      fontSize: '2.4rem',
                                      backgroundColor: 'transparent',
                                      border: 'none',
                                      cursor: 'pointer',
                                      opacity: star <= rating ? 1 : 0.3,
                                    }}
                                  >
                                    ⭐
                                  </button>
                                ))}
                              </div>
                            </div>

                            <div style={{ marginBottom: '1.5rem' }}>
                              <label htmlFor="comments" style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500' }}>
                                Comments
                              </label>
                              <textarea
                                id="comments"
                                value={comments}
                                onChange={(e) => setComments(e.target.value)}
                                placeholder="Share your experience about your rental trip..."
                                style={{
                                  width: '100%',
                                  padding: '0.75rem',
                                  borderRadius: '0.5rem',
                                  border: '1px solid #ddd',
                                  fontFamily: 'inherit',
                                  fontSize: '1.4rem',
                                  minHeight: '120px',
                                }}
                              />
                            </div>

                            {error && (
                              <div className="error-message" style={{ marginBottom: '1rem' }}>
                                <span>{error}</span>
                              </div>
                            )}

                            <CTA
                              as="button"
                              type="submit"
                              disabled={submitting}
                            >
                              {submitting ? 'Sending...' : 'Send Feedback'}
                            </CTA>
                          </form>
                        )}

                        {feedback && (
                          <p className="card-subtext">You have already sent feedback for this order.</p>
                        )}
                      </div>
                    </div>
                  ) : (
                    <div className="card">
                      <div className="card-body text-center">
                        <p className="card-subtext">Select an order to send feedback</p>
                      </div>
                    </div>
                  )}
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
