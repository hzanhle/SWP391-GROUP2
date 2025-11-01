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
      setError('Please confirm that you have inspected the vehicle')
      return
    }

    try {
      setLoading(true)
      setError(null)

      // Parse orderId from hash URL (e.g., #check-in?orderId=123)
      const hash = window.location.hash.substring(1)
      const hashParams = new URLSearchParams(hash.split('?')[1] || '')
      const orderIdParam = hashParams.get('orderId')

      const activeOrder = localStorage.getItem('active_order')
      const pending = JSON.parse(localStorage.getItem('pending_booking') || '{}')
      const orderId = Number(orderIdParam || activeOrder || pending.orderId)

      if (!orderId || isNaN(orderId)) {
        setError('Order code for check-in not found')
        return
      }

      const token = localStorage.getItem('auth.token')
      if (!token) {
        setError('Please log in again')
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
      setError(err.message || 'Error during check-in. Please try again.')
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
                  <h2 className="card-title" style={{ color: '#2a6817', marginBottom: '1rem' }}>Check-in Successful!</h2>
                  <p className="card-subtext" style={{ marginBottom: '2rem', fontSize: '1.6rem' }}>
                    You have successfully received the vehicle. Enjoy your trip!
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
        <section id="check-in" className="section page-offset" aria-labelledby="checkin-title">
          <div className="container">
            <div className="section-header">
              <h1 id="checkin-title" className="section-title">Check-in to Receive Vehicle</h1>
              <p className="section-subtitle">Take photos and confirm vehicle condition before receiving.</p>
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
                    <h3 className="card-title">Vehicle Condition Photos</h3>
                    <div className="doc-uploaders">
                      <DocumentUploader label="Front" />
                      <DocumentUploader label="Back" />
                      <DocumentUploader label="Left" />
                      <DocumentUploader label="Right" />
                    </div>
                  </div>
                  <div className="doc-card">
                    <h3 className="card-title">Confirmation</h3>
                    <p className="card-subtext">I confirm that I have inspected the vehicle and agree to the rental terms.</p>
                    <label className="row">
                      <input
                        type="checkbox"
                        checked={agreed}
                        onChange={(e) => setAgreed(e.target.checked)}
                        required
                        aria-label="Agree"
                      />
                      I Agree
                    </label>
                  </div>
                </div>
                <div className="row-between">
                  <a className="nav-link" href="#booking">Back</a>
                  <CTA as="button" type="submit" disabled={loading || !agreed}>
                    {loading ? 'Processing...' : 'Complete Check-in'}
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
