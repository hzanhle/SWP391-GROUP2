import React, { useState } from 'react'
import Footer from '../components/Footer'
import api from '../api/client'

export default function Login() {
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
      password: form.password.value,
    }

    try {
      const res = await api.login(payload)
      const data = res.data
      const token = data?.token ?? data?.Token
      const user = data?.user ?? data?.User
         // ---- derive userId from user object or JWT ----
   let userId =
     user?.userId ?? user?.UserId ??
     user?.id     ?? user?.Id     ??
     user?.uid    ?? null;

   if (!userId && token && token.split('.').length === 3) {
     try {
       const base64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')
       const payloadJwt = JSON.parse(atob(base64))
       userId =
         payloadJwt?.sub ||
         payloadJwt?.userId ||
         payloadJwt?.nameid ||
         payloadJwt?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ||
         null
     } catch { /* ignore */ }
   }
      if (token) {
        localStorage.setItem('auth.token', token)
        localStorage.setItem('auth.user', JSON.stringify(user || {}))
             if (userId != null) {
       localStorage.setItem('auth.userId', String(userId))
     }
        window.dispatchEvent(new StorageEvent('storage', {
          key: 'auth.user',
          newValue: JSON.stringify(user || {}),
          storageArea: localStorage,
        }))
        const roleId = Number((user && (user.roleId ?? user.RoleId)) ?? 0)
        const roleName = String((user && (user.roleName ?? user.RoleName)) || '').toLowerCase()
        const isAdmin = roleId === 3 || roleName === 'admin'
        const isStaff = roleId === 2 || roleName === 'staff'

        if (isAdmin) {
          window.location.hash = '#admin'
        } else if (isStaff) {
          window.location.hash = '#staff-verify'
        } else {
          window.location.hash = ''
        }
      } else {
        window.location.hash = ''
      }
    } catch (err) {
      const msg = err?.data?.message || err?.message || 'Failed to login'
      setError(msg)
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div data-figma-layer="Login Page">
      <main>
        <section className="auth-section">
          <div className="auth-page-hero">
            <div className="auth-page-hero__overlay"></div>
            <div className="container">
              <div className="auth-page-hero__content">
                <h1 className="auth-page-hero__title">Sign In</h1>
                <p className="auth-page-hero__subtitle">Access your account to book and manage your rides</p>
              </div>
            </div>
          </div>

          <div className="container">
            <div className="auth-form-wrapper">
              <div className="auth-form-card">
                <h2>Welcome Back</h2>
                <p className="auth-form-subtitle">Sign in to your account to continue</p>

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
                      placeholder="Enter your username"
                      autoComplete="username"
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
                      autoComplete="current-password"
                      minLength={6}
                      required
                    />
                  </div>

                  <div className="form-actions">
                    <a href="#forgot-password" className="forgot-link">Forgot password?</a>
                    <button type="submit" className="auth-submit-btn" disabled={submitting}>
                      {submitting ? 'Signing in...' : 'Sign In'}
                    </button>
                  </div>
                </form>

                <div className="auth-divider">
                  <span>or</span>
                </div>

                <div className="auth-footer-text">
                  <span>Don't have an account?</span>
                  <a href="#signup" className="auth-link">Create one</a>
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
