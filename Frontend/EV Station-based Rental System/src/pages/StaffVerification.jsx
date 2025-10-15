import React, { useState } from 'react'
import Navbar from '../components/Navbar'
import Footer from '../components/Footer'
import CTA from '../components/CTA'
import api from '../api/client'

function joinUrl(base, path) {
  if (!path) return ''
  const b = (base || '').replace(/\/$/, '')
  const p = String(path).replace(/^\//, '')
  return `${b}/${p}`
}

export default function StaffVerification() {
  const [userIdInput, setUserIdInput] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [citizen, setCitizen] = useState(null)
  const [license, setLicense] = useState(null)
  const token = typeof window !== 'undefined' ? localStorage.getItem('auth.token') : null
  const apiBase = import.meta.env.VITE_API_URL || ''

  // List state
  const [tab, setTab] = useState('submitted') // none | submitted | approved
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [total, setTotal] = useState(0)
  const [items, setItems] = useState([])

  async function loadList(nextPage = page) {
    setLoading(true)
    setError('')
    try {
      const { data } = await api.listVerificationUsers({ status: tab, query: search, page: nextPage, pageSize }, token)
      setItems(Array.isArray(data?.items) ? data.items : [])
      setTotal(Number(data?.total || 0))
      setPage(Number(data?.page || nextPage))
      setPageSize(Number(data?.pageSize || pageSize))
    } catch (e) {
      setError(e?.message || 'Lỗi tải danh sách')
    } finally {
      setLoading(false)
    }
  }

  async function loadUserData(userId) {
    setLoading(true)
    setError('')
    setCitizen(null)
    setLicense(null)
    try {
      const [cRes, lRes] = await Promise.allSettled([
        api.getCitizenInfo(userId, token),
        api.getDriverLicense(userId, token),
      ])
      if (cRes.status === 'fulfilled') setCitizen(cRes.value.data)
      if (lRes.status === 'fulfilled') setLicense(lRes.value.data)
      if (cRes.status === 'rejected' && lRes.status === 'rejected') {
        const msg = (cRes.reason && (cRes.reason.message || cRes.reason.Message)) || 'Không tìm thấy giấy tờ cho người dùng này'
        setError(msg)
      }
    } catch (e) {
      setError(e?.message || 'Lỗi tải dữ liệu')
    } finally {
      setLoading(false)
    }
  }

  function handleSearch(e) {
    e.preventDefault()
    const id = Number(userIdInput)
    if (!id || id <= 0) {
      setError('Vui lòng nhập User ID hợp lệ')
      return
    }
    loadUserData(id)
  }

  React.useEffect(() => {
    loadList(1)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [tab])

  async function handleApprove(type, approve) {
    if (!citizen && !license) return
    try {
      setLoading(true)
      setError('')
      const uid = Number((citizen?.userId) || (license?.userId))
      if (!uid) throw new Error('Thiếu UserId')
      if (type === 'citizen') {
        await api.setCitizenInfoStatus(uid, approve, token)
      } else {
        await api.setDriverLicenseStatus(uid, approve, token)
      }
      await loadUserData(uid)
      window.alert(approve ? 'Đã xác nhận' : 'Đã từ chối')
    } catch (e) {
      setError(e?.message || 'Lỗi cập nhật trạng thái')
    } finally {
      setLoading(false)
    }
  }

  function Images({ list }) {
    if (!list || list.length === 0) return null
    return (
      <div className="docs-grid">
        {list.map((img, idx) => (
          <img key={idx} className="media-img" src={joinUrl(apiBase, img.imagePath || img.path)} alt={img.imageType || 'document'} />
        ))}
      </div>
    )
  }

  function InfoRow({ label, value }) {
    if (value === undefined || value === null || value === '') return null
    return (
      <div className="row-between">
        <span className="muted">{label}</span>
        <span className="strong">{String(value)}</span>
      </div>
    )
  }

  return (
    <div data-figma-layer="Staff Verification Page">
      <Navbar />
      <main>
        <section id="staff-verify" className="section" aria-labelledby="staff-title">
          <div className="container">
            <div className="section-header">
              <h1 id="staff-title" className="section-title">Xác thực giấy tờ người dùng</h1>
              <p className="section-subtitle">Nhập User ID để xem Căn cước công dân và Giấy phép lái xe, đối chiếu và xác thực.</p>
            </div>

            <div className="card">
              <form className="card-body" onSubmit={handleSearch} noValidate>
                {error ? <div role="alert" className="badge gray" aria-live="assertive">{error}</div> : null}
                <div className="row-between">
                  <div className="field">
                    <label htmlFor="userId" className="label">User ID</label>
                    <input id="userId" name="userId" className="input" type="number" min={1} value={userIdInput}
                           onChange={e=>setUserIdInput(e.target.value)} placeholder="VD: 101" required />
                  </div>
                  <CTA as="button" type="submit" disabled={loading} aria-busy={loading}>Tìm</CTA>
                </div>
              </form>
            </div>

            <div className="docs-grid">
              <div className="card">
                <div className="card-body">
                  <div className="row-between">
                    <div className="row">
                      <button className="btn" onClick={() => setTab('none')} aria-pressed={tab==='none'}>Chưa nộp</button>
                      <button className="btn" onClick={() => setTab('submitted')} aria-pressed={tab==='submitted'}>Đã nộp</button>
                      <button className="btn" onClick={() => setTab('approved')} aria-pressed={tab==='approved'}>Đã xác thực</button>
                    </div>
                    <div className="field" style={{minWidth: '220px'}}>
                      <label htmlFor="search" className="label">Tìm kiếm</label>
                      <input id="search" className="input" value={search} onChange={e=>setSearch(e.target.value)} placeholder="Tên, email, SĐT..." />
                    </div>
                  </div>
                  <div className="card-subtext">Tổng: {total}</div>
                  <div className="docs-grid">
                    {items.map((it) => (
                      <div key={it.userId} className="card">
                        <div className="card-body">
                          <div className="row-between">
                            <div>
                              <div className="card-title">{it.fullName || it.userName}</div>
                              <div className="card-subtext">{it.email} • {it.phoneNumber || ''}</div>
                            </div>
                            <CTA as="button" onClick={() => { setUserIdInput(String(it.userId)); loadUserData(it.userId) }}>Xem</CTA>
                          </div>
                          <div className="row-between">
                            <span className="badge gray">CCCD: {it.citizen?.status || 'Chưa có'}</span>
                            <span className="badge gray">GPLX: {it.driver?.status || 'Chưa có'}</span>
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>
                  <div className="row-between">
                    <button className="btn" disabled={page<=1 || loading} onClick={()=>loadList(page-1)}>Trước</button>
                    <div className="card-subtext">Trang {page} / {Math.max(1, Math.ceil(total / pageSize))}</div>
                    <button className="btn" disabled={page>=Math.ceil(total / pageSize) || loading} onClick={()=>loadList(page+1)}>Sau</button>
                  </div>
                </div>
              </div>
              <div className="card">
                <div className="card-header">
                  <h2 className="card-title">Căn cước công dân</h2>
                </div>
                <div className="card-body">
                  {!citizen && <p className="muted">Chưa có dữ liệu</p>}
                  {citizen && (
                    <>
                      <InfoRow label="Trạng thái" value={citizen.status} />
                      <InfoRow label="Họ tên" value={citizen.fullName} />
                      <InfoRow label="Số CCCD" value={citizen.citizenId} />
                      <InfoRow label="Giới tính" value={citizen.sex} />
                      <InfoRow label="Ngày sinh" value={citizen.dayOfBirth} />
                      <InfoRow label="Ngày cấp" value={citizen.citiRegisDate} />
                      <InfoRow label="Nơi cấp" value={citizen.citiRegisOffice} />
                      <InfoRow label="Địa chỉ" value={citizen.address} />
                      <Images list={citizen.images} />
                      <div className="row">
                        <CTA as="button" onClick={() => handleApprove('citizen', true)} disabled={loading}>Xác nhận</CTA>
                        <button className="btn" onClick={() => handleApprove('citizen', false)} disabled={loading}>Từ chối</button>
                      </div>
                    </>
                  )}
                </div>
              </div>

              <div className="card">
                <div className="card-header">
                  <h2 className="card-title">Giấy phép lái xe</h2>
                </div>
                <div className="card-body">
                  {!license && <p className="muted">Chưa có dữ liệu</p>}
                  {license && (
                    <>
                      <InfoRow label="Trạng thái" value={license.status} />
                      <InfoRow label="Họ tên" value={license.fullName} />
                      <InfoRow label="Số GPLX" value={license.licenseId} />
                      <InfoRow label="Hạng" value={license.licenseType} />
                      <InfoRow label="Ngày cấp" value={license.registerDate} />
                      <InfoRow label="Nơi cấp" value={license.registerOffice} />
                      <Images list={license.images} />
                      <div className="row">
                        <CTA as="button" onClick={() => handleApprove('license', true)} disabled={loading}>Xác nhận</CTA>
                        <button className="btn" onClick={() => handleApprove('license', false)} disabled={loading}>Từ chối</button>
                      </div>
                    </>
                  )}
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
