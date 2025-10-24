import React, { useState, useEffect } from 'react'
import Footer from '../components/Footer'
import api from '../api/client'

export default function VerifyEmail() {
  const [submitting, setSubmitting] = useState(false)
  const [verified, setVerified] = useState(false)
  const [error, setError] = useState('')
  const [email, setEmail] = useState('')

  useEffect(() => {
    try {
      const saved = localStorage.getItem('pendingVerificationEmail') || ''
      setEmail(saved)
    } catch {}
  }, [])

  async function handleSubmit(e) {
    e.preventDefault()
    if (submitting) return
    setSubmitting(true)
    setError('')

    const form = e.currentTarget
    const code = form.code.value.trim()

    try {
      if (!email) throw new Error('Không tìm thấy email cần xác minh. Vui lòng đăng ký lại.')
      await api.verifyRegistrationOtp(email, code)
      setVerified(true)
      try { localStorage.removeItem('pendingVerificationEmail') } catch {}
      window.alert('Xác minh thành công! Bạn có thể đăng nhập.')
      window.location.hash = 'login'
    } catch (err) {
      const msg = (err?.data && (err.data.message || err.data.Message)) || err?.message || 'Xác minh thất bại'
      setError(msg)
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div data-figma-layer="Verify Email Page">
      <main>
        <section className="auth-section">
          <div className="auth-page-hero">
            <div className="auth-page-hero__overlay"></div>
            <div className="container">
              <div className="auth-page-hero__content">
                <h1 className="auth-page-hero__title">Verify Email</h1>
                <p className="auth-page-hero__subtitle">Complete your account verification</p>
              </div>
            </div>
          </div>

          <div className="container">
            <div className="auth-form-wrapper">
              <div className="auth-form-card">
                <h2>Verify Your Email</h2>
                <p className="auth-form-subtitle">Enter the verification code sent to {email || 'your email'}</p>

                <form onSubmit={handleSubmit} noValidate>
                  {error && (
                    <div role="alert" className="auth-error-message">
                      <i className="fa-solid fa-circle-exclamation"></i>
                      {error}
                    </div>
                  )}

                  <div className="form-group">
                    <label htmlFor="code">Verification Code</label>
                    <input
                      id="code"
                      name="code"
                      type="text"
                      placeholder="Enter 6-digit code"
                      inputMode="numeric"
                      pattern="^[0-9]{4,8}$"
                      required
                    />
                  </div>

                  <div className="form-actions">
                    <a href="#signup" className="forgot-link">Back to Sign Up</a>
                    <button type="submit" className="auth-submit-btn" disabled={submitting}>
                      {submitting ? 'Verifying...' : 'Verify Email'}
                    </button>
                  </div>
                </form>

                <div className="auth-divider">
                  <span>or</span>
                </div>

                <div className="auth-footer-text">
                  <span>Already verified?</span>
                  <a href="#login" className="auth-link">Sign in</a>
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
