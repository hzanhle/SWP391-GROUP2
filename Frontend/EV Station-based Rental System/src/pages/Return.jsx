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
  const [successMessage, setSuccessMessage] = useState('')
  const [showFeedback, setShowFeedback] = useState(false)
  const [frontImage, setFrontImage] = useState(null)
  const [backImage, setBackImage] = useState(null)
  const [leftImage, setLeftImage] = useState(null)
  const [rightImage, setRightImage] = useState(null)
  const [hasDamage, setHasDamage] = useState(false)
  const [damageDescription, setDamageDescription] = useState('')
  const [conditionChecks, setConditionChecks] = useState({
    exterior: false,
    battery: false,
    accessories: false,
    cleanliness: false,
  })

  const handleSubmit = async (e) => {
    e.preventDefault()
    
    // Collect all images
    const images = [frontImage, backImage, leftImage, rightImage].filter(img => img !== null)
    
    if (images.length === 0) {
      setError('Please upload at least one vehicle photo')
      return
    }

    if (!hasDamage && Object.values(conditionChecks).some(value => !value)) {
      setError('Please confirm the vehicle is in good condition before completing the return.')
      return
    }

    try {
      setLoading(true)
      setError(null)
      setSuccessMessage('')

      // Determine orderId: try hash param, then active_order, then pending_booking
      const hash = window.location.hash.substring(1)
      const hashParams = new URLSearchParams(hash.split('?')[1] || '')
      const orderIdParam = hashParams.get('orderId')
      const activeOrder = localStorage.getItem('active_order')
      const pending = JSON.parse(localStorage.getItem('pending_booking') || '{}')
      const orderId = Number(orderIdParam || activeOrder || pending.orderId)

      if (!orderId) {
        setError('Order code for return not found')
        return
      }

      const token = localStorage.getItem('auth.token')

      const positiveNotes = []
      if (conditionChecks.exterior) positiveNotes.push('Exterior checked - no new scratches')
      if (conditionChecks.battery) positiveNotes.push('Battery level matches contract requirement')
      if (conditionChecks.accessories) positiveNotes.push('All accessories present')
      if (conditionChecks.cleanliness) positiveNotes.push('Interior cleaned and tidy')
      const conditionNotes = positiveNotes.join('; ')

      await bookingApi.completeRental(orderId, images, hasDamage, damageDescription, conditionNotes, token)

      // Show immediate feedback popup
      setShowFeedback(true)
      setSuccessMessage('Return completed successfully! Your deposit refund is being processed automatically.')
    } catch (err) {
      console.error('Error completing rental:', err)
      setError(err.message || 'Error completing rental return')
      setSuccessMessage('')
    } finally {
      setLoading(false)
    }
  }

  const handleSubmitFeedback = async ({ rating, comments }) => {
    try {
      const user = JSON.parse(localStorage.getItem('auth.user') || '{}')
      const userId = Number(user.userId || user.UserId || 0)
      const orderId = Number(localStorage.getItem('active_order') || JSON.parse(localStorage.getItem('pending_booking') || '{}').orderId || 0)
      const token = localStorage.getItem('auth.token')

      let vehicleId = 0
      try {
        const orderResponse = await bookingApi.getOrderById(orderId, token)
        const orderData = orderResponse?.data?.data ?? orderResponse?.data ?? {}
        vehicleId = Number(
          orderData?.Vehicle?.VehicleId ??
          orderData?.VehicleId ??
          orderData?.Vehicle?.Id ??
          0
        )
      } catch (err) {
        console.warn('Unable to fetch order data for vehicle id, falling back to local storage', err)
        vehicleId = Number(JSON.parse(localStorage.getItem('pending_booking') || '{}').vehicleInfo?.vehicleId || 0)
      }

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
      setError(err.message || 'Error sending rating')
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
              <h1 id="return-title" className="section-title">Return Vehicle</h1>
              <p className="section-subtitle">Check condition and complete payment if applicable.</p>
            </div>

            {error && <div className="error-message mb-4">{error}</div>}
            {successMessage && !error && (
              <div className="card card-body badge green" style={{ marginBottom: '1.5rem' }}>
                {successMessage}
              </div>
            )}

            <div className="card">
              <form className="card-body" onSubmit={handleSubmit}>
                <div className="docs-grid">
                  <div className="doc-card">
                    <h3 className="card-title">Photos After Use</h3>
                    <p className="card-subtext" style={{ marginBottom: '1rem' }}>Please upload at least one photo of the vehicle</p>
                    <div className="doc-uploaders">
                      <DocumentUploader label="Front" value={frontImage} onChange={setFrontImage} accept="image/*" />
                      <DocumentUploader label="Back" value={backImage} onChange={setBackImage} accept="image/*" />
                      <DocumentUploader label="Left" value={leftImage} onChange={setLeftImage} accept="image/*" />
                      <DocumentUploader label="Right" value={rightImage} onChange={setRightImage} accept="image/*" />
                    </div>
                  </div>
                  <div className="doc-card">
                    <h3 className="card-title">Vehicle Condition</h3>
                    <div className="stack mt-4">
                      <label className="row">
                        <input
                          type="checkbox"
                          checked={conditionChecks.exterior}
                          onChange={(e) => setConditionChecks(prev => ({ ...prev, exterior: e.target.checked }))}
                        />
                        Exterior inspected – no new scratches or dents
                      </label>
                      <label className="row">
                        <input
                          type="checkbox"
                          checked={conditionChecks.battery}
                          onChange={(e) => setConditionChecks(prev => ({ ...prev, battery: e.target.checked }))}
                        />
                        Battery level meets contract requirement
                      </label>
                      <label className="row">
                        <input
                          type="checkbox"
                          checked={conditionChecks.accessories}
                          onChange={(e) => setConditionChecks(prev => ({ ...prev, accessories: e.target.checked }))}
                        />
                        All required accessories (helmet, charger, documents) are present
                      </label>
                      <label className="row">
                        <input
                          type="checkbox"
                          checked={conditionChecks.cleanliness}
                          onChange={(e) => setConditionChecks(prev => ({ ...prev, cleanliness: e.target.checked }))}
                        />
                        Vehicle is clean and ready for the next customer
                      </label>
                      <label className="row">
                        <input 
                          type="checkbox" 
                          checked={hasDamage}
                          onChange={(e) => setHasDamage(e.target.checked)}
                        />
                        Vehicle has damage
                      </label>
                      {hasDamage && (
                        <div>
                          <label className="label">Damage Description</label>
                          <textarea
                            className="input"
                            rows="3"
                            value={damageDescription}
                            onChange={(e) => setDamageDescription(e.target.value)}
                            placeholder="Describe any damage to the vehicle..."
                          />
                        </div>
                      )}
                    </div>
                  </div>
                </div>
                <div className="row-between">
                  <a className="nav-link" href="#booking">Back</a>
                  <CTA as="button" type="submit" disabled={loading}>{loading ? 'Processing...' : 'Confirm Return'}</CTA>
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
