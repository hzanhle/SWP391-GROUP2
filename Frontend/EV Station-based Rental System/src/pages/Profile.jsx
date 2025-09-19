import React, { useEffect, useState } from 'react'
import Navbar from '../components/Navbar'
import Footer from '../components/Footer'
import CTA from '../components/CTA'

function loadUser() {
  try { return JSON.parse(localStorage.getItem('auth.user') || '{}') } catch { return {} }
}

export default function Profile() {
  const [submitting, setSubmitting] = useState(false)
  const [form, setForm] = useState({ userName: '', fullName: '', email: '', phoneNumber: '' })

  useEffect(() => {
    const u = loadUser()
    setForm({
      userName: u.userName || u.username || '',
      fullName: u.fullName || '',
      email: u.email || '',
      phoneNumber: u.phoneNumber || u.phone || '',
    })
  }, [])

  function onChange(e) {
    const { name, value } = e.target
    setForm((prev) => ({ ...prev, [name]: value }))
  }

  async function handleSubmit(e) {
    e.preventDefault()
    if (submitting) return
    setSubmitting(true)
    try {
      const current = loadUser()
      const next = { ...current, ...form }
      localStorage.setItem('auth.user', JSON.stringify(next))
      alert('Đã lưu hồ sơ')
    } finally { setSubmitting(false) }
  }

  const initials = (form.fullName || form.userName || 'EV').trim().split(/\s+/).slice(0,2).map(s=>s[0]?.toUpperCase()).join('')

  return (
    <div data-figma-layer="Renter Profile Page">
      <Navbar />
      <main>
        <section id="profile" className="section" aria-labelledby="profile-title">
          <div className="container">
            <div className="section-header">
              <h1 id="profile-title" className="section-title">Hồ sơ cá nhân</h1>
              <p className="section-subtitle">Cập nhật thông tin liên hệ và tài khoản của bạn.</p>
            </div>

            <div className="card">
              <div className="card-body">
                <div className="profile-header">
                  <div className="avatar-circle" aria-hidden="true">{initials}</div>
                  <div className="profile-meta">
                    <h2 className="card-title">{form.fullName || form.userName || 'Người dùng'}</h2>
                    <p className="card-subtext">Quản lý thông tin và bảo mật tài khoản</p>
                    <div className="row">
                      <span className="badge green">Đã xác minh số điện thoại</span>
                      <span className="badge gray">Email chưa xác minh</span>
                    </div>
                  </div>
                </div>

                <form onSubmit={handleSubmit} className="profile-form" noValidate>
                  <div className="field">
                    <label htmlFor="userName" className="label">Tên đăng nhập</label>
                    <input id="userName" name="userName" className="input" type="text" value={form.userName} onChange={onChange} required />
                  </div>

                  <div className="field">
                    <label htmlFor="fullName" className="label">Họ và tên</label>
                    <input id="fullName" name="fullName" className="input" type="text" value={form.fullName} onChange={onChange} />
                  </div>

                  <div className="field">
                    <label htmlFor="email" className="label">Email</label>
                    <input id="email" name="email" className="input" type="email" value={form.email} onChange={onChange} />
                  </div>

                  <div className="field">
                    <label htmlFor="phoneNumber" className="label">Số điện thoại</label>
                    <input id="phoneNumber" name="phoneNumber" className="input" type="tel" value={form.phoneNumber} onChange={onChange} />
                  </div>

                  <div className="row-between">
                    <a className="nav-link" href="#profile-docs">Hồ sơ giấy tờ</a>
                    <CTA as="button" type="submit" disabled={submitting} aria-busy={submitting}>{submitting ? 'Đang lưu…' : 'Lưu thay đổi'}</CTA>
                  </div>
                </form>
              </div>
            </div>
          </div>
        </section>
      </main>
      <Footer />
    </div>
  )
}
