import React, { useState } from 'react'
import Navbar from '../components/Navbar'
import Footer from '../components/Footer'
import CTA from '../components/CTA'
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
      window.alert((data && (data.message || data.Message)) || 'Đã gửi OTP. Vui lòng kiểm tra email để lấy mã xác minh.')
    } catch (err) {
      const msg = (err?.data && (err.data.message || err.data.Message)) || err?.message || 'Gửi OTP thất bại'
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
      window.alert('Đổi mật khẩu thành công! Vui lòng đăng nhập lại.')
      window.location.hash = 'login'
    } catch (err) {
      const msg = (err?.data && (err.data.message || err.data.Message)) || err?.message || 'Xác minh hoặc đổi mật khẩu thất bại'
      setError(msg)
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div data-figma-layer="Forgot Password Page">
      <Navbar />
      <main>
        <section id="forgot-password" className="section" aria-labelledby="forgot-title">
          <div className="container">
            <div className="section-header">
              <h1 id="forgot-title" className="section-title">Quên mật khẩu</h1>
              <p className="section-subtitle">Nhập email để nhận mã OTP đặt lại mật khẩu.</p>
            </div>

            <div className="card">
              {!sent ? (
                <form className="card-body" onSubmit={handleRequest} noValidate>
                  {error ? <div role="alert" className="badge gray" aria-live="assertive">{error}</div> : null}
                  <div className="field">
                    <label htmlFor="email" className="label">Email</label>
                    <input id="email" name="email" className="input" type="email" placeholder="ban@example.com" autoComplete="email" required />
                  </div>

                  <div className="row-between">
                    <a className="nav-link" href="#login">Trở về đăng nhập</a>
                    <CTA as="button" type="submit" disabled={submitting} aria-busy={submitting}>
                      {submitting ? 'Đang gửi…' : 'Gửi OTP'}
                    </CTA>
                  </div>
                </form>
              ) : (
                <form className="card-body" onSubmit={handleVerifyAndReset} noValidate>
                  {error ? <div role="alert" className="badge gray" aria-live="assertive">{error}</div> : null}
                  <div className="field">
                    <label htmlFor="otp" className="label">Mã OTP</label>
                    <input id="otp" name="otp" className="input" type="text" placeholder="VD: 123456" inputMode="numeric" pattern="^[0-9]{6}$" required />
                  </div>
                  <div className="field">
                    <label htmlFor="newPassword" className="label">Mật khẩu mới</label>
                    <input id="newPassword" name="newPassword" className="input" type="password" placeholder="••••••••" autoComplete="new-password" minLength={6} required />
                  </div>
                  <div className="field">
                    <label htmlFor="confirmPassword" className="label">Xác nhận mật khẩu</label>
                    <input id="confirmPassword" name="confirmPassword" className="input" type="password" placeholder="••••••••" autoComplete="new-password" minLength={6} required />
                  </div>

                  <div className="row-between">
                    <a className="nav-link" href="#login">Trở về đăng nhập</a>
                    <CTA as="button" type="submit" disabled={submitting} aria-busy={submitting}>
                      {submitting ? 'Đang xác minh…' : 'Đổi mật khẩu'}
                    </CTA>
                  </div>
                </form>
              )}
            </div>
          </div>
        </section>
      </main>
      <Footer />
    </div>
  )
}
