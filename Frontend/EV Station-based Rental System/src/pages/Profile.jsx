import React, { useEffect, useState } from 'react'
import Navbar from '../components/Navbar'
import Footer from '../components/Footer'
import CTA from '../components/CTA'
import DocumentUploader from '../components/DocumentUploader'
import api from '../api/client'

function loadUser() {
  try { return JSON.parse(localStorage.getItem('auth.user') || '{}') } catch { return {} }
}

const API_BASE = (import.meta.env.VITE_API_URL || '').replace(/\/$/, '')

export default function Profile() {
  const [submitting, setSubmitting] = useState(false)
  const [form, setForm] = useState({ userName: '', fullName: '', email: '', phoneNumber: '' })

  const [citizen, setCitizen] = useState({
    CitizenId: '',
    Sex: '',
    CitiRegisDate: '',
    CitiRegisOffice: '',
    FullName: '',
    Address: '',
    DayOfBirth: '',
  })
  const [citizenFiles, setCitizenFiles] = useState({ front: null, back: null })
  const [dl, setDl] = useState({
    LicenseId: '',
    LicenseType: '',
    RegisterDate: '',
    RegisterOffice: '',
  })
  const [dlFiles, setDlFiles] = useState({ front: null, back: null })
  const [hasCitizen, setHasCitizen] = useState(false)
  const [hasDL, setHasDL] = useState(false)
  const [apiError, setApiError] = useState('')

  useEffect(() => {
    const u = loadUser()
    setForm({
      userName: u.userName || u.username || '',
      fullName: u.fullName || '',
      email: u.email || '',
      phoneNumber: u.phoneNumber || u.phone || '',
    })

    // Prefill name into citizen form
    setCitizen((c) => ({ ...c, FullName: u.fullName || '' }))

    // Try fetching existing docs if API is configured
    const token = localStorage.getItem('auth.token') || ''
    const userId = u.id || u.Id
    if (API_BASE && token && userId) {
      Promise.allSettled([
        api.getCitizenInfo(userId, token),
        api.getDriverLicense(userId, token),
      ]).then(([cit, dr]) => {
        if (cit.status === 'fulfilled' && cit.value?.data) {
          const d = cit.value.data
          setHasCitizen(true)
          setCitizen({
            CitizenId: d.citizenId || d.CitizenId || '',
            Sex: d.sex || d.Sex || '',
            CitiRegisDate: (d.citiRegisDate || d.CitiRegisDate || '').toString().slice(0,10),
            CitiRegisOffice: d.citiRegisOffice || d.CitiRegisOffice || '',
            FullName: d.fullName || d.FullName || '',
            Address: d.address || d.Address || '',
            DayOfBirth: (d.dayOfBirth || d.DayOfBirth || '').toString().slice(0,10),
          })
        }
        if (dr.status === 'fulfilled' && dr.value?.data) {
          const d = dr.value.data
          setHasDL(true)
          setDl({
            LicenseId: d.licenseId || d.LicenseId || '',
            LicenseType: d.licenseType || d.LicenseType || '',
            RegisterDate: (d.registerDate || d.RegisterDate || '').toString().slice(0,10),
            RegisterOffice: d.registerOffice || d.RegisterOffice || '',
          })
        }
      }).catch(()=>{})
    }
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

  async function submitCitizen(e) {
    e.preventDefault()
    setApiError('')
    const token = localStorage.getItem('auth.token') || ''
    const u = loadUser()
    const userId = u.id || u.Id
    if (!API_BASE) { setApiError('Thiếu cấu hình VITE_API_URL'); return }
    if (!token || !userId) { setApiError('Bạn cần đăng nhập để gửi thông tin'); return }

    const payload = {
      UserId: userId,
      CitizenId: citizen.CitizenId,
      Sex: citizen.Sex,
      CitiRegisDate: citizen.CitiRegisDate,
      CitiRegisOffice: citizen.CitiRegisOffice,
      FullName: citizen.FullName,
      Address: citizen.Address,
      DayOfBirth: citizen.DayOfBirth,
      Files: [citizenFiles.front, citizenFiles.back].filter(Boolean),
    }

    try {
      if (hasCitizen) await api.updateCitizenInfo(payload, token)
      else await api.createCitizenInfo(payload, token)
      alert('Đã lưu thông tin công dân')
      setHasCitizen(true)
    } catch (err) {
      setApiError(err?.message || 'Lỗi khi gửi thông tin công dân')
    }
  }

  async function submitDL(e) {
    e.preventDefault()
    setApiError('')
    const token = localStorage.getItem('auth.token') || ''
    const u = loadUser()
    const userId = u.id || u.Id
    if (!API_BASE) { setApiError('Thiếu cấu hình VITE_API_URL'); return }
    if (!token || !userId) { setApiError('Bạn cần đăng nhập để gửi thông tin'); return }

    const payload = {
      UserId: userId,
      LicenseId: dl.LicenseId,
      LicenseType: dl.LicenseType,
      RegisterDate: dl.RegisterDate,
      RegisterOffice: dl.RegisterOffice,
      Files: [dlFiles.front, dlFiles.back].filter(Boolean),
    }

    try {
      if (hasDL) await api.updateDriverLicense(payload, token)
      else await api.createDriverLicense(payload, token)
      alert('Đã lưu giấy phép lái xe')
      setHasDL(true)
    } catch (err) {
      setApiError(err?.message || 'Lỗi khi gửi giấy phép lái xe')
    }
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

            <div id="profile-docs" className="section" aria-labelledby="docs-title">
              <div className="section-header">
                <h2 id="docs-title" className="section-title">Giấy tờ xác thực</h2>
                {!API_BASE && (<p className="section-subtitle">Vui lòng cấu hình VITE_API_URL để gửi dữ liệu tới backend.</p>)}
                {apiError ? <div role="alert" className="badge gray" aria-live="assertive">{apiError}</div> : null}
              </div>

              <div className="docs-grid">
                <div className="card">
                  <div className="card-body doc-card">
                    <h3 className="card-title">Thông tin công dân</h3>
                    <form onSubmit={submitCitizen} className="profile-form" noValidate>
                      <div className="field">
                        <label className="label" htmlFor="CitizenId">Số CCCD</label>
                        <input id="CitizenId" name="CitizenId" className="input" value={citizen.CitizenId} onChange={(e)=>setCitizen({...citizen, CitizenId:e.target.value})} required />
                      </div>
                      <div className="field">
                        <label className="label" htmlFor="FullName">Họ và tên</label>
                        <input id="FullName" name="FullName" className="input" value={citizen.FullName} onChange={(e)=>setCitizen({...citizen, FullName:e.target.value})} />
                      </div>
                      <div className="field">
                        <label className="label" htmlFor="Sex">Giới tính</label>
                        <select id="Sex" name="Sex" className="input" value={citizen.Sex} onChange={(e)=>setCitizen({...citizen, Sex:e.target.value})}>
                          <option value="">Chọn</option>
                          <option value="Male">Nam</option>
                          <option value="Female">Nữ</option>
                          <option value="Other">Khác</option>
                        </select>
                      </div>
                      <div className="field">
                        <label className="label" htmlFor="DayOfBirth">Ngày sinh</label>
                        <input id="DayOfBirth" name="DayOfBirth" className="input" type="date" value={citizen.DayOfBirth} onChange={(e)=>setCitizen({...citizen, DayOfBirth:e.target.value})} />
                      </div>
                      <div className="field">
                        <label className="label" htmlFor="Address">Địa chỉ</label>
                        <input id="Address" name="Address" className="input" value={citizen.Address} onChange={(e)=>setCitizen({...citizen, Address:e.target.value})} />
                      </div>
                      <div className="field">
                        <label className="label" htmlFor="CitiRegisOffice">Nơi cấp</label>
                        <input id="CitiRegisOffice" name="CitiRegisOffice" className="input" value={citizen.CitiRegisOffice} onChange={(e)=>setCitizen({...citizen, CitiRegisOffice:e.target.value})} />
                      </div>
                      <div className="field">
                        <label className="label" htmlFor="CitiRegisDate">Ngày cấp</label>
                        <input id="CitiRegisDate" name="CitiRegisDate" className="input" type="date" value={citizen.CitiRegisDate} onChange={(e)=>setCitizen({...citizen, CitiRegisDate:e.target.value})} />
                      </div>

                      <div className="doc-uploaders">
                        <DocumentUploader label="Ảnh CCCD mặt trước" hint="PNG/JPG/PDF" value={citizenFiles.front} onChange={(f)=>setCitizenFiles(s=>({...s, front:f}))} />
                        <DocumentUploader label="Ảnh CCCD mặt sau" hint="PNG/JPG/PDF" value={citizenFiles.back} onChange={(f)=>setCitizenFiles(s=>({...s, back:f}))} />
                      </div>

                      <div className="row-between">
                        <span className="section-subtitle">{hasCitizen ? 'Đã có thông tin, bấm để cập nhật' : 'Chưa có thông tin, bấm để tạo mới'}</span>
                        <CTA as="button" type="submit">{hasCitizen ? 'Cập nhật' : 'Tạo mới'}</CTA>
                      </div>
                    </form>
                  </div>
                </div>

                <div className="card">
                  <div className="card-body doc-card">
                    <h3 className="card-title">Giấy phép lái xe</h3>
                    <form onSubmit={submitDL} className="profile-form" noValidate>
                      <div className="field">
                        <label className="label" htmlFor="LicenseId">Số GPLX</label>
                        <input id="LicenseId" name="LicenseId" className="input" value={dl.LicenseId} onChange={(e)=>setDl({...dl, LicenseId:e.target.value})} required />
                      </div>
                      <div className="field">
                        <label className="label" htmlFor="LicenseType">Hạng</label>
                        <input id="LicenseType" name="LicenseType" className="input" value={dl.LicenseType} onChange={(e)=>setDl({...dl, LicenseType:e.target.value})} placeholder="A1/A2/B1..." />
                      </div>
                      <div className="field">
                        <label className="label" htmlFor="RegisterOffice">Nơi cấp</label>
                        <input id="RegisterOffice" name="RegisterOffice" className="input" value={dl.RegisterOffice} onChange={(e)=>setDl({...dl, RegisterOffice:e.target.value})} />
                      </div>
                      <div className="field">
                        <label className="label" htmlFor="RegisterDate">Ngày cấp</label>
                        <input id="RegisterDate" name="RegisterDate" className="input" type="date" value={dl.RegisterDate} onChange={(e)=>setDl({...dl, RegisterDate:e.target.value})} />
                      </div>

                      <div className="doc-uploaders">
                        <DocumentUploader label="Ảnh GPLX mặt trước" hint="PNG/JPG/PDF" value={dlFiles.front} onChange={(f)=>setDlFiles(s=>({...s, front:f}))} />
                        <DocumentUploader label="Ảnh GPLX mặt sau" hint="PNG/JPG/PDF" value={dlFiles.back} onChange={(f)=>setDlFiles(s=>({...s, back:f}))} />
                      </div>

                      <div className="row-between">
                        <span className="section-subtitle">{hasDL ? 'Đã có giấy phép, bấm để cập nhật' : 'Chưa có giấy phép, bấm để tạo mới'}</span>
                        <CTA as="button" type="submit">{hasDL ? 'Cập nhật' : 'Tạo mới'}</CTA>
                      </div>
                    </form>
                  </div>
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
