import React, { useState } from 'react'
import Navbar from '../components/Navbar'
import Footer from '../components/Footer'
import CTA from '../components/CTA'

export default function ForgotPassword() {
  const [submitting, setSubmitting] = useState(false)
  const [sent, setSent] = useState(false)

  async function handleSubmit(e) {
    e.preventDefault()
    if (submitting) return
    setSubmitting(true)

    const form = e.currentTarget
    const email = form.email.value.trim()

    try {
      // UI-only: hiển thị trạng thái đã gửi yêu cầu đặt lại mật khẩu
      setSent(true)
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
              <p className="section-subtitle">Nhập email để nhận liên kết đặt lại mật khẩu.</p>
            </div>

            <div className="card">
              <form className="card-body" onSubmit={handleSubmit} noValidate>
                {sent ? <div role="alert" className="badge green" aria-live="polite">Đã gửi yêu cầu. Vui lòng kiểm tra email của bạn.</div> : null}
                <div className="field">
                  <label htmlFor="email" className="label">Email</label>
                  <input id="email" name="email" className="input" type="email" placeholder="ban@example.com" autoComplete="email" required />
                </div>

                <div className="row-between">
                  <a className="nav-link" href="#login">Trở về đăng nhập</a>
                  <CTA as="button" type="submit" disabled={submitting} aria-busy={submitting}>
                    {submitting ? 'Đang gửi…' : 'Gửi liên kết'}
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
