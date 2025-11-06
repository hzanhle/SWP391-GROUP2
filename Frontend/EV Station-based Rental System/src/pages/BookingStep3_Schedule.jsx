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
          <div className="summary-grid">
            <div className="summary-row">
              <span className="card-subtext">Location:</span>
              <span className="card-subtext summary-strong">{selectedStation?.name}</span>
            </div>
            <div className="summary-row">
              <span className="card-subtext">Vehicle:</span>
              <span className="card-subtext summary-strong">{selectedModel?.manufacturer} {selectedModel?.modelName}</span>
            </div>
            <div className="summary-row">
              <span className="card-subtext">Rental Price:</span>
              <span className="card-subtext summary-strong">${preview.totalRentalCost?.toFixed(2)}</span>
            </div>
            <div className="summary-row">
              <span className="card-subtext">Deposit:</span>
              <span className="card-subtext summary-strong">${preview.depositCost?.toFixed(2)}</span>
            </div>
            <div className="summary-row">
              <span className="card-subtext">Service Fee:</span>
              <span className="card-subtext summary-strong">${preview.serviceFee?.toFixed(2)}</span>
            </div>
            <hr className="divider" />
            <div className="summary-row total-row">
              <h4 className="total-label">Total:</h4>
              <h4 className="total-value">${preview.totalPaymentCost?.toFixed(2)}</h4>
            </div>
          </div>
        </div>
      )}

      {/* Terms & Conditions checkbox - required before proceeding to payment */}
      {preview && (
        <div className="field grid-span-full mt-4">
          <label className="row">
            <input
              type="checkbox"
              checked={!!termsAccepted}
              onChange={(e) => setTermsAccepted?.(e.target.checked)}
              aria-label="Đồng ý điều khoản"
            />
            <span className="text-14">
              I have read and agree with{' '}
              <button
                type="button"
                onClick={() => setShowTerms(true)}
                className="btn-reset link-underline text-14"
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
