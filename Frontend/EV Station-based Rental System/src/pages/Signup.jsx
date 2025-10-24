import React, { useState } from 'react'
import Footer from '../components/Footer'
import api from '../api/client'

export default function Signup() {
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')

  async function handleSubmit(e) {
    e.preventDefault()
    if (submitting) return
    setSubmitting(true)
    setError('')

    const form = e.currentTarget
    const payload = {
      userName: form.username.value.trim(),
      email: form.email.value.trim(),
      phoneNumber: form.phone.value.trim(),
      password: form.password.value,
    }

    try {
      const { data } = await api.sendRegistrationOtp(payload)
      try { localStorage.setItem('pendingVerificationEmail', payload.email) } catch {}
      window.alert((data && (data.message || data.Message)) || 'OTP sent. Please check your email for verification code.')
      window.location.hash = '#verify-email'
    } catch (err) {
      const msg = (err?.data && (err.data.message || err.data.Message)) || err?.message || 'Registration failed'
      setError(msg)
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div data-figma-layer="Signup Page">
      <main>
        <section className="auth-section">
          <div className="auth-page-hero">
            <div className="auth-page-hero__overlay"></div>
            <div className="container">
              <div className="auth-page-hero__content">
                <h1 className="auth-page-hero__title">Create Account</h1>
                <p className="auth-page-hero__subtitle">Join to book electric vehicles and access charging stations</p>
              </div>
            </div>
          </div>

          <div className="container">
            <div className="auth-form-wrapper">
              <div className="auth-form-card">
                <h2>Get Started</h2>
                <p className="auth-form-subtitle">Create your account to begin booking EVs</p>

                <form onSubmit={handleSubmit} noValidate>
                  {error && (
                    <div role="alert" className="auth-error-message">
                      <i className="fa-solid fa-circle-exclamation"></i>
                      {error}
                    </div>
                  )}

                  <div className="form-group">
                    <label htmlFor="username">Username</label>
                    <input
                      id="username"
                      name="username"
                      type="text"
                      placeholder="Choose your username"
                      autoComplete="username"
                      required
                    />
                  </div>

                  <div className="form-group">
                    <label htmlFor="email">Email</label>
                    <input
                      id="email"
                      name="email"
                      type="email"
                      placeholder="you@example.com"
                      autoComplete="email"
                      required
                    />
                  </div>

                  <div className="form-group">
                    <label htmlFor="phone">Phone Number</label>
                    <input
                      id="phone"
                      name="phone"
                      type="tel"
                      placeholder="(+84) 901-234-567"
                      autoComplete="tel"
                      pattern="^[0-9+()\-\s]{7,}$"
                      required
                    />
                  </div>

                  <div className="form-group">
                    <label htmlFor="password">Password</label>
                    <input
                      id="password"
                      name="password"
                      type="password"
                      placeholder="••••••••"
                      autoComplete="new-password"
                      minLength={6}
                      required
                    />
                  </div>

                  <div className="form-actions">
                    <a href="#" className="forgot-link">Back to Home</a>
                    <button type="submit" className="auth-submit-btn" disabled={submitting}>
                      {submitting ? 'Creating account...' : 'Sign Up'}
                    </button>
                  </div>
                </form>

                <div className="auth-divider">
                  <span>or</span>
                </div>

                <div className="auth-footer-text">
                  <span>Already have an account?</span>
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
