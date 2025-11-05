import React, { useState } from 'react'
import CTA from '../components/CTA'
import TermsAndServices from '../components/TermsAndServices'

export default function BookingStep3_Schedule({
  selectedStation,
  selectedModel,
  pickupDate,
  dropoffDate,
  onPickupDateChange,
  onDropoffDateChange,
  preview,
  previewLoading,
  previewError,
  bookingLoading,
  bookingError,
  onPreview,
  onConfirmBooking,
  termsAccepted,
  setTermsAccepted,
}) {
  const [showTerms, setShowTerms] = useState(false)

  const pickupTime = pickupDate ? new Date(pickupDate) : null
  const dropoffTime = dropoffDate ? new Date(dropoffDate) : null
  const hoursDiff = pickupTime && dropoffTime ? (dropoffTime - pickupTime) / (1000 * 60 * 60) : 0
  const isValidTimeRange = hoursDiff >= 3

  return (
    <div className="booking-grid">
      <div className="booking-notes-container">
        <div className="booking-note booking-note-warning">
          <span className="booking-note-icon">���️</span>
          <div className="booking-note-content">
            <p className="booking-note-text"><strong>Note:</strong> Pickup and return times must be at least <strong>3 hours</strong> apart</p>
          </div>
        </div>
        <div className="booking-note booking-note-info">
          <span className="booking-note-icon">⚠️</span>
          <div className="booking-note-content">
            <p className="booking-note-text"><strong>Restriction:</strong> You have a maximum of <strong>15 minutes</strong> to arrive at the pickup point after the specified time</p>
          </div>
        </div>
      </div>

      <div className="field">
        <label htmlFor="pickup-date" className="label">
          <span>Pickup Time</span>
        </label>
        <input
          id="pickup-date"
          type="datetime-local"
          className="input"
          value={pickupDate}
          onChange={(e) => onPickupDateChange(e.target.value)}
        />
      </div>
      <div className="field">
        <label htmlFor="dropoff-date" className="label">
          <span>Return Time</span>
        </label>
        <input
          id="dropoff-date"
          type="datetime-local"
          className="input"
          value={dropoffDate}
          onChange={(e) => onDropoffDateChange(e.target.value)}
        />
        {pickupDate && dropoffDate && !isValidTimeRange && (
          <div className="field-error-message">
            <span className="error-icon">⚠️</span> Return time must be at least 3 hours after pickup
          </div>
        )}
      </div>

      {previewError && (
        <div className="error-message error-visible grid-span-full">
          <span>{previewError}</span>
        </div>
      )}

      {!preview && !previewLoading && (
        <div className="field grid-span-full text-center">
          <CTA
            as="button"
            onClick={onPreview}
            disabled={!selectedModel || !pickupDate || !dropoffDate || !isValidTimeRange}
          >
            View Cost Estimate
          </CTA>
        </div>
      )}

      {previewLoading && (
        <div className="field grid-span-full text-center">
          <p>Calculating...</p>
        </div>
      )}

      {preview && (
        <div className="summary grid-span-full">
          <h3 className="card-title">Order Summary</h3>
          <div style={{ display: 'grid', gap: '1rem', marginTop: '1rem' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between' }}>
              <span className="card-subtext">Location:</span>
              <span className="card-subtext" style={{ fontWeight: 'bold' }}>{selectedStation?.name}</span>
            </div>
            <div style={{ display: 'flex', justifyContent: 'space-between' }}>
              <span className="card-subtext">Vehicle:</span>
              <span className="card-subtext" style={{ fontWeight: 'bold' }}>{selectedModel?.manufacturer} {selectedModel?.modelName}</span>
            </div>
            <div style={{ display: 'flex', justifyContent: 'space-between' }}>
              <span className="card-subtext">Rental Price:</span>
              <span className="card-subtext" style={{ fontWeight: 'bold' }}>${preview.totalRentalCost?.toFixed(2)}</span>
            </div>
            <div style={{ display: 'flex', justifyContent: 'space-between' }}>
              <span className="card-subtext">Deposit:</span>
              <span className="card-subtext" style={{ fontWeight: 'bold' }}>${preview.depositCost?.toFixed(2)}</span>
            </div>
            <div style={{ display: 'flex', justifyContent: 'space-between' }}>
              <span className="card-subtext">Service Fee:</span>
              <span className="card-subtext" style={{ fontWeight: 'bold' }}>${preview.serviceFee?.toFixed(2)}</span>
            </div>
            <hr style={{ margin: '1rem 0' }} />
            <div style={{ display: 'flex', justifyContent: 'space-between' }}>
              <h4 style={{ color: '#ff4d30', fontSize: '1.8rem', margin: 0 }}>Tổng cộng:</h4>
              <h4 style={{ color: '#ff4d30', fontSize: '1.8rem', margin: 0 }}>${preview.totalPaymentCost?.toFixed(2)}</h4>
            </div>
          </div>
        </div>
      )}

      {/* Terms & Conditions checkbox - required before proceeding to payment */}
      {preview && (
        <div className="field grid-span-full" style={{ marginTop: '1rem' }}>
          <label className="row" style={{ alignItems: 'center', gap: '0.75rem' }}>
            <input
              type="checkbox"
              checked={!!termsAccepted}
              onChange={(e) => setTermsAccepted?.(e.target.checked)}
              aria-label="Đồng ý điều khoản"
            />
            <span style={{ fontSize: '1rem' }}>
              I have read and agree with{' '}
              <button
                type="button"
                onClick={() => setShowTerms(true)}
                style={{
                  background: 'none',
                  border: 'none',
                  color: '#0066cc',
                  cursor: 'pointer',
                  textDecoration: 'underline',
                  fontSize: '1rem',
                  padding: 0,
                  fontFamily: 'inherit',
                }}
                onMouseOver={(e) => (e.target.style.color = '#0052a3')}
                onMouseOut={(e) => (e.target.style.color = '#0066cc')}
              >
                Terms &amp; Services
              </button>
            </span>
          </label>
        </div>
      )}

      {bookingError && (
        <div className="error-message error-visible grid-span-full">
          <span>{bookingError}</span>
        </div>
      )}

      <TermsAndServices isOpen={showTerms} onClose={() => setShowTerms(false)} />
    </div>
  )
}
