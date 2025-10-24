import React, { useState } from 'react'
import Footer from '../components/Footer'
import api from '../api/client'

export default function ForgotPassword() {
  const [submitting, setSubmitting] = useState(false)
  const [sent, setSent] = useState(false)
  const [error, setError] = useState('')
  const [email, setEmail] = useState('')

  async function handleRequest(e) {
    e.preventDefault()
    if (submitting) return
    setSubmitting(true)
    setError('')

    const form = e.currentTarget
    const value = form.email.value.trim()

    try {
      const { data } = await api.sendPasswordResetOtp(value)
      setEmail(value)
      setSent(true)
      window.alert((data && (data.message || data.Message)) || 'OTP sent. Please check your email for verification code.')
    } catch (err) {
      const msg = (err?.data && (err.data.message || err.data.Message)) || err?.message || 'Failed to send OTP'
      setError(msg)
    } finally {
      setSubmitting(false)
    }
  }

  async function handleVerifyAndReset(e) {
    e.preventDefault()
    if (submitting) return
    setSubmitting(true)
    setError('')

    const form = e.currentTarget
    const otp = form.otp.value.trim()
    const newPassword = form.newPassword.value
    const confirmPassword = form.confirmPassword.value

    try {
      await api.verifyPasswordResetOtp(email, otp)
      await api.resetPassword({ email, otp, newPassword, confirmPassword })
      window.alert('Password changed successfully! Please login again.')
      window.location.hash = '#login'
    } catch (err) {
      const msg = (err?.data && (err.data.message || err.data.Message)) || err?.message || 'Failed to verify or reset password'
      setError(msg)
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div data-figma-layer="Forgot Password Page">
      <main>
        <section className="auth-section">
          <div className="auth-page-hero">
            <div className="auth-page-hero__overlay"></div>
            <div className="container">
              <div className="auth-page-hero__content">
                <h1 className="auth-page-hero__title">Reset Password</h1>
                <p className="auth-page-hero__subtitle">Regain access to your account</p>
              </div>
            </div>
          </div>

          <div className="container">
            <div className="auth-form-wrapper">
              <div className="auth-form-card">
                {!sent ? (
                  <>
                    <h2>Enter Your Email</h2>
                    <p className="auth-form-subtitle">We'll send you a code to reset your password</p>

                    <form onSubmit={handleRequest} noValidate>
                      {error && (
                        <div role="alert" className="auth-error-message">
                          <i className="fa-solid fa-circle-exclamation"></i>
                          {error}
                        </div>
                      )}

                      <div className="form-group">
                        <label htmlFor="email">Email Address</label>
                        <input
                          id="email"
                          name="email"
                          type="email"
                          placeholder="your@example.com"
                          autoComplete="email"
                          required
                        />
                      </div>

                      <div className="form-actions">
                        <a href="#login" className="forgot-link">Back to Sign In</a>
                        <button type="submit" className="auth-submit-btn" disabled={submitting}>
                          {submitting ? 'Sending...' : 'Send Code'}
                        </button>
                      </div>
                    </form>
                  </>
                ) : (
                  <>
                    <h2>Verify & Reset</h2>
                    <p className="auth-form-subtitle">Enter the code we sent to {email}</p>

                    <form onSubmit={handleVerifyAndReset} noValidate>
                      {error && (
                        <div role="alert" className="auth-error-message">
                          <i className="fa-solid fa-circle-exclamation"></i>
                          {error}
                        </div>
                      )}

                      <div className="form-group">
                        <label htmlFor="otp">Verification Code</label>
                        <input
                          id="otp"
                          name="otp"
                          type="text"
                          placeholder="Enter 6-digit code"
                          inputMode="numeric"
                          pattern="^[0-9]{6}$"
                          required
                        />
                      </div>

                      <div className="form-group">
                        <label htmlFor="newPassword">New Password</label>
                        <input
                          id="newPassword"
                          name="newPassword"
                          type="password"
                          placeholder="••••••••"
                          autoComplete="new-password"
                          minLength={6}
                          required
                        />
                      </div>

                      <div className="form-group">
                        <label htmlFor="confirmPassword">Confirm Password</label>
                        <input
                          id="confirmPassword"
                          name="confirmPassword"
                          type="password"
                          placeholder="••••••••"
                          autoComplete="new-password"
                          minLength={6}
                          required
                        />
                      </div>

                      <div className="form-actions">
                        <a href="#login" className="forgot-link">Back to Sign In</a>
                        <button type="submit" className="auth-submit-btn" disabled={submitting}>
                          {submitting ? 'Resetting...' : 'Reset Password'}
                        </button>
                      </div>
                    </form>
                  </>
                )}
              </div>
            </div>
          </div>
        </section>
      </main>
      <Footer />
    </div>
  )
}
