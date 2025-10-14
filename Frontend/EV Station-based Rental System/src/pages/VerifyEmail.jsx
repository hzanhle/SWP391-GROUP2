import React, { useState, useEffect } from 'react'
import Navbar from '../components/Navbar'
import Footer from '../components/Footer'
import CTA from '../components/CTA'
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
      <Navbar />
      <main>
        <section id="verify-email" className="section" aria-labelledby="verify-title">
          <div className="container">
            <div className="section-header">
              <h1 id="verify-title" className="section-title">Xác minh email</h1>
              <p className="section-subtitle">Nhập mã xác minh đã được gửi tới email của bạn{email ? ` (${email})` : ''}.</p>
            </div>

            <div className="card">
              <form className="card-body" onSubmit={handleSubmit} noValidate>
                {error ? <div role="alert" className="badge gray" aria-live="assertive">{error}</div> : null}
                {verified ? <div role="status" className="badge green">Xác minh thành công</div> : null}
                <div className="field">
                  <label htmlFor="code" className="label">Mã xác minh</label>
                  <input id="code" name="code" className="input" type="text" placeholder="VD: 123456" inputMode="numeric" pattern="^[0-9]{4,8}$" required />
                </div>

                <div className="row-between">
                  <a className="nav-link" href="#signup">Đăng ký lại</a>
                  <CTA as="button" type="submit" disabled={submitting} aria-busy={submitting}>
                    {submitting ? 'Đang xác minh…' : 'Xác minh'}
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
