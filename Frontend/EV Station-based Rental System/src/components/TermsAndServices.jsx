import React from 'react'
import '../styles/TermsAndServices.css'

export default function TermsAndServices({ isOpen, onClose }) {
  if (!isOpen) return null

  return (
    <div className="terms-overlay">
      <div className="terms-modal">
        <div className="terms-header">
          <h2 className="terms-title">Terms &amp; Services</h2>
          <button 
            className="terms-close-btn" 
            onClick={onClose}
            aria-label="Close terms and services"
          >
            ✕
          </button>
        </div>
        
        <div className="terms-content">
          <section className="terms-section">
            <h3>1. Preparation Time</h3>
            <p>
              Customers have a maximum of 15 minutes to arrive at the pickup point after the specified time. 
              If late by more than 15 minutes, the order will be cancelled and a fee may be charged.
            </p>
          </section>

          <section className="terms-section">
            <h3>2. Rental Duration</h3>
            <p>
              The pickup and return times must be at least 3 hours apart. 
              The rental period starts from the time you pick up the vehicle at the designated pickup point and ends when you return it at the designated return point.
            </p>
          </section>

          <section className="terms-section">
            <h3>3. Deposit and Payment</h3>
            <p>
              The deposit will be held throughout the rental period. 
              If the vehicle is damaged or lost, the deposit will be used to cover the damages.
            </p>
          </section>

          <section className="terms-section">
            <h3>4. Liability and Insurance</h3>
            <p>
              The renter is responsible for all damages, loss, or traffic violations. 
              The vehicle has basic insurance. Insurance details will be provided at pickup.
            </p>
          </section>

          <section className="terms-section">
            <h3>5. Cancellation Policy</h3>
            <p>
              Customers can cancel for free if cancelled before 24 hours. 
              Cancellation after 24 hours will forfeit 50% of the rental fee. Cancellation after pickup will forfeit 100% of the fee.
            </p>
          </section>

          <section className="terms-section">
            <h3>6. Traffic Rules</h3>
            <p>
              The renter must comply with all traffic laws and road safety regulations. 
              Any violations will be paid by the renter.
            </p>
          </section>

          <section className="terms-section">
            <h3>7. Maintenance and Cleanliness</h3>
            <p>
              The vehicle must be returned in clean condition and with a full fuel tank. 
              If not, additional maintenance or cleaning fees will be charged.
            </p>
          </section>

          <section className="terms-section">
            <h3>8. Customer Support</h3>
            <p>
              If you have any questions or issues during the rental period, please contact our customer support team immediately.
            </p>
          </section>
        </div>

        <div className="terms-footer">
          <button 
            className="btn btn-secondary" 
            onClick={onClose}
          >
            Close
          </button>
        </div>
      </div>
    </div>
  )
}
