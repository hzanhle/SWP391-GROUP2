import React, { useState } from 'react'
import Navbar from '../components/Navbar'
import Footer from '../components/Footer'
import CTA from '../components/CTA'
import DocumentUploader from '../components/DocumentUploader'
import * as bookingApi from '../api/booking'

export default function CheckIn() {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState(null)
  const [agreed, setAgreed] = useState(false)
  const [success, setSuccess] = useState(false)

  const handleSubmit = async (e) => {
    e.preventDefault()

    if (!agreed) {
      setError('Vui lòng xác nhận rằng bạn đã kiểm tra xe')
      return
    }

    try {
      setLoading(true)
      setError(null)

      const params = new URLSearchParams(window.location.search)
      const orderIdParam = params.get('orderId')
      const activeOrder = localStorage.getItem('active_order')
      const pending = JSON.parse(localStorage.getItem('pending_booking') || '{}')
      const orderId = Number(orderIdParam || activeOrder || pending.orderId)

      if (!orderId || isNaN(orderId)) {
        setError('Không tìm thấy mã đơn hàng để check-in')
        return
      }

      const token = localStorage.getItem('auth.token')
      if (!token) {
        setError('Vui lòng đăng nhập lại')
        return
      }

      await bookingApi.startRental(orderId, token)

      setSuccess(true)
      localStorage.setItem('active_order', String(orderId))
      localStorage.removeItem('pending_booking')

      setTimeout(() => {
        window.location.hash = 'booking'
      }, 2000)
    } catch (err) {
      console.error('Error during check-in:', err)
      setError(err.message || 'Lỗi khi check-in. Vui lòng thử lại.')
    } finally {
      setLoading(false)
    }
  }

  if (success) {
    return (
      <div data-figma-layer="Check-in Success Page">
        <Navbar />
        <main>
          <section className="section">
            <div className="container">
              <div className="card">
                <div className="card-body" style={{ textAlign: 'center', padding: '4rem' }}>
                  <div style={{ fontSize: '4rem', marginBottom: '1rem' }}>✅</div>
                  <h2 className="card-title" style={{ color: '#2a6817', marginBottom: '1rem' }}>Check-in thành công!</h2>
                  <p className="card-subtext" style={{ marginBottom: '2rem', fontSize: '1.6rem' }}>
                    Bạn đã nhận xe thành công. Chuyến đi vui vẻ!
                  </p>
                  <p className="card-subtext">Đang chuyển hướng...</p>
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
    <div data-figma-layer="Check-in Page">
      <Navbar />
      <main>
        <section id="check-in" className="section" aria-labelledby="checkin-title">
          <div className="container">
            <div className="section-header">
              <h1 id="checkin-title" className="section-title">Check-in nhận xe</h1>
              <p className="section-subtitle">Chụp ảnh và xác nhận tình trạng trước khi nhận xe.</p>
            </div>

            {error && (
              <div className="error-message error-visible" style={{ marginBottom: '1rem' }}>
                <span>{error}</span>
              </div>
            )}

            <div className="card">
              <form className="card-body" onSubmit={handleSubmit}>
                <div className="docs-grid">
                  <div className="doc-card">
                    <h3 className="card-title">Ảnh tình trạng xe</h3>
                    <div className="doc-uploaders">
                      <DocumentUploader label="Mặt trước" />
                      <DocumentUploader label="Mặt sau" />
                      <DocumentUploader label="Bên trái" />
                      <DocumentUploader label="Bên phải" />
                    </div>
                  </div>
                  <div className="doc-card">
                    <h3 className="card-title">Xác nhận</h3>
                    <p className="card-subtext">Tôi xác nhận đã kiểm tra xe và đồng ý điều khoản thuê.</p>
                    <label className="row">
                      <input
                        type="checkbox"
                        checked={agreed}
                        onChange={(e) => setAgreed(e.target.checked)}
                        required
                        aria-label="Agree"
                      />
                      Tôi đồng ý
                    </label>
                  </div>
                </div>
                <div className="row-between">
                  <a className="nav-link" href="#booking">Quay lại</a>
                  <CTA as="button" type="submit" disabled={loading || !agreed}>
                    {loading ? 'Đang xử lý...' : 'Hoàn tất check-in'}
                  </CTA>
                </div>
              </form>
            </div>
          </div>
        </section>
      </main>
      <Footer />
    </div>
  )
}
