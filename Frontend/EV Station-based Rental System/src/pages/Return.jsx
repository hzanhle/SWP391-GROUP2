import React, { useState } from 'react'
import Navbar from '../components/Navbar'
import Footer from '../components/Footer'
import CTA from '../components/CTA'
import DocumentUploader from '../components/DocumentUploader'
import FeedbackForm from '../components/FeedbackForm'
import * as bookingApi from '../api/booking'

export default function Return() {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState(null)
  const [showFeedback, setShowFeedback] = useState(false)

  const handleSubmit = async (e) => {
    e.preventDefault()
    try {
      setLoading(true)
      setError(null)

      // Determine orderId: try query param, then active_order, then pending_booking
      const params = new URLSearchParams(window.location.search)
      const orderIdParam = params.get('orderId')
      const activeOrder = localStorage.getItem('active_order')
      const pending = JSON.parse(localStorage.getItem('pending_booking') || '{}')
      const orderId = Number(orderIdParam || activeOrder || pending.orderId)

      if (!orderId) {
        setError('Không tìm thấy mã đơn hàng để hoàn tất trả xe')
        return
      }

      const token = localStorage.getItem('auth.token')
      await bookingApi.completeRental(orderId, token)

      // Show immediate feedback popup
      setShowFeedback(true)
    } catch (err) {
      console.error('Error completing rental:', err)
      setError(err.message || 'Lỗi khi hoàn thành trả xe')
    } finally {
      setLoading(false)
    }
  }

  const handleSubmitFeedback = async ({ rating, comments }) => {
    try {
      const user = JSON.parse(localStorage.getItem('auth.user') || '{}')
      const userId = Number(user.userId || user.UserId || 0)
      const orderId = Number(localStorage.getItem('active_order') || JSON.parse(localStorage.getItem('pending_booking') || '{}').orderId || 0)
      const vehicleId = Number(JSON.parse(localStorage.getItem('pending_booking') || '{}').vehicleInfo?.vehicleId || 0)

      const token = localStorage.getItem('auth.token')
      await bookingApi.submitFeedback({
        UserId: userId,
        OrderId: orderId,
        VehicleId: vehicleId,
        VehicleRating: rating,
        Comments: comments,
      }, token)

      setShowFeedback(false)
      // Navigate to booking list/detail
      window.location.hash = 'booking'
    } catch (err) {
      console.error('Error submitting feedback:', err)
      setError(err.message || 'Lỗi khi gửi đánh giá')
      throw err
    }
  }

  return (
    <div data-figma-layer="Return Vehicle Page">
      <Navbar />
      <main>
        <section id="return" className="section page-offset" aria-labelledby="return-title">
          <div className="container">
            <div className="section-header">
              <h1 id="return-title" className="section-title">Trả xe</h1>
              <p className="section-subtitle">Kiểm tra tình trạng và hoàn tất thanh toán nếu phát sinh.</p>
            </div>

            {error && <div className="error-message" style={{ marginBottom: '1rem' }}>{error}</div>}

            <div className="card">
              <form className="card-body" onSubmit={handleSubmit}>
                <div className="docs-grid">
                  <div className="doc-card">
                    <h3 className="card-title">Ảnh sau khi sử dụng</h3>
                    <div className="doc-uploaders">
                      <DocumentUploader label="Mặt trước" />
                      <DocumentUploader label="Mặt sau" />
                      <DocumentUploader label="Bên trái" />
                      <DocumentUploader label="Bên phải" />
                    </div>
                  </div>
                  <div className="doc-card">
                    <h3 className="card-title">Checklist</h3>
                    <label className="row"><input type="checkbox" required /> Không trầy xước mới</label>
                    <label className="row"><input type="checkbox" required /> Mức pin theo hợp đồng</label>
                    <label className="row"><input type="checkbox" required /> Phụ kiện đầy đủ</label>
                  </div>
                </div>
                <div className="row-between">
                  <a className="nav-link" href="#booking">Quay lại</a>
                  <CTA as="button" type="submit" disabled={loading}>{loading ? 'Đang xử lý...' : 'Xác nhận trả xe'}</CTA>
                </div>
              </form>
            </div>
          </div>
        </section>
      </main>

      <FeedbackForm open={showFeedback} onClose={() => setShowFeedback(false)} onSubmit={handleSubmitFeedback} />

      <Footer />
    </div>
  )
}
