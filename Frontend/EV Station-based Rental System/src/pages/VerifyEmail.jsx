import React, { useState } from 'react'
import Navbar from '../components/Navbar'
import Footer from '../components/Footer'
import CTA from '../components/CTA'

export default function VerifyEmail() {
  const [submitting, setSubmitting] = useState(false)
  const [verified, setVerified] = useState(false)

  async function handleSubmit(e) {
    e.preventDefault()
    if (submitting) return
    setSubmitting(true)

    const form = e.currentTarget
    const code = form.code.value.trim()

    try {
      // UI-only: mô phỏng xác minh mã
      setVerified(true)
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
              <p className="section-subtitle">Nhập mã xác minh đã được gửi tới email của bạn.</p>
            </div>

            <div className="card">
              <form className="card-body" onSubmit={handleSubmit} noValidate>
                {verified ? <div role="status" className="badge green">Xác minh thành công</div> : null}
                <div className="field">
                  <label htmlFor="code" className="label">Mã xác minh</label>
                  <input id="code" name="code" className="input" type="text" placeholder="VD: 123456" inputMode="numeric" pattern="^[0-9]{4,8}$" required />
                </div>

                <div className="row-between">
                  <a className="nav-link" href="#login">Trở về đăng nhập</a>
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
