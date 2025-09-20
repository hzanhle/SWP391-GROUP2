import React, { useState } from 'react'
import Navbar from '../components/Navbar'
import Footer from '../components/Footer'
import CTA from '../components/CTA'
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
      if (data?.token) {
        localStorage.setItem('auth.token', data.token)
        localStorage.setItem('auth.user', JSON.stringify(data.user || {}))
      }
      alert('Đăng nhập thành công')
      window.location.hash = ''
    } catch (err) {
      const msg = err?.data?.message || err?.message || 'Đăng nhập thất bại'
      setError(msg)
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div data-figma-layer="Login Page">
      <Navbar />
      <main>
        <section id="login" className="section" aria-labelledby="login-title">
          <div className="container">
            <div className="section-header">
              <h1 id="login-title" className="section-title">Đăng nhập</h1>
              <p className="section-subtitle">Truy cập để đặt xe và quản lý chuyến đi của bạn.</p>
            </div>

            <div className="card">
              <form className="card-body" onSubmit={handleSubmit} noValidate>
                {error ? <div role="alert" className="badge gray" aria-live="assertive">{error}</div> : null}
                <div className="field">
                  <label htmlFor="username" className="label">Tên đăng nhập</label>
                  <input id="username" name="username" className="input" type="text" placeholder="nhap_ten" autoComplete="username" required />
                </div>

                <div className="field">
                  <label htmlFor="password" className="label">Mật khẩu</label>
                  <input id="password" name="password" className="input" type="password" placeholder="••••••••" autoComplete="current-password" minLength={6} required />
                </div>

                <div className="row-between">
                  <a className="nav-link" href="#forgot-password">Quên mật khẩu?</a>
                  <CTA as="button" type="submit" disabled={submitting} aria-busy={submitting}>
                    {submitting ? 'Đang đăng nhập…' : 'Đăng nhập'}
                  </CTA>
                </div>

                <div className="row-center">
                  <span className="section-subtitle">Chưa có tài khoản?</span>
                  <a className="nav-link" href="#signup">Đăng ký</a>
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
