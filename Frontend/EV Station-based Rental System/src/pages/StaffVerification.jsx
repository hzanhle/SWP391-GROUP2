import React, { useState } from 'react'
import Navbar from '../components/Navbar'
import Footer from '../components/Footer'
import CTA from '../components/CTA'
import api from '../api/client'
import '../styles/staff.css'

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
        const msg = (cRes.reason && (cRes.reason.message || cRes.reason.Message)) || 'No documents found for this user'
        setError(msg)
      }
    } catch (e) {
      setError(e?.message || 'Error loading data')
    } finally {
      setLoading(false)
    }
  }

  function handleSearch(e) {
    e.preventDefault()
    const id = Number(userIdInput)
    if (!id || id <= 0) {
      setError('Please enter a valid User ID')
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
      if (!uid) throw new Error('Missing UserId')
      if (type === 'citizen') {
        await api.setCitizenInfoStatus(uid, approve, token)
      } else {
        await api.setDriverLicenseStatus(uid, approve, token)
      }
      await loadUserData(uid)
      window.alert(approve ? 'Approved' : 'Rejected')
    } catch (e) {
      setError(e?.message || 'Error updating status')
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
      <main style={{ paddingTop: '80px' }}>
        <section id="staff-verify" className="section" aria-labelledby="staff-title">
          <div className="container">
            <div className="section-header">
              <h1 id="staff-title" className="section-title">User Document Verification</h1>
              <p className="section-subtitle">Enter User ID to view ID card and driver's license, verify and authenticate.</p>
            </div>

            <div className="card">
              <form className="card-body" onSubmit={handleSearch} noValidate>
                {error ? <div role="alert" className="badge gray" aria-live="assertive">{error}</div> : null}
                <div className="row-between">
                  <div className="field">
                    <label htmlFor="userId" className="label">User ID</label>
                    <input id="userId" name="userId" className="input" type="number" min={1} value={userIdInput}
                           onChange={e=>setUserIdInput(e.target.value)} placeholder="E.g.: 101" required />
                  </div>
                  <CTA as="button" type="submit" disabled={loading} aria-busy={loading}>Search</CTA>
                </div>
              </form>
            </div>

            <div className="docs-grid">
              <div className="card">
                <div className="card-body">
                  <div className="row-between">
                    <div className="row">
                      <button className="btn" onClick={() => setTab('none')} aria-pressed={tab==='none'}>Not Submitted</button>
                      <button className="btn" onClick={() => setTab('submitted')} aria-pressed={tab==='submitted'}>Submitted</button>
                      <button className="btn" onClick={() => setTab('approved')} aria-pressed={tab==='approved'}>Verified</button>
                    </div>
                    <div className="field" style={{minWidth: '220px'}}>
                      <label htmlFor="search" className="label">Tìm kiếm</label>
                      <input id="search" className="input" value={search} onChange={e=>setSearch(e.target.value)} placeholder="Name, email, phone..." />
                    </div>
                  </div>
                  <div className="card-subtext">Total: {total}</div>
                  <div className="docs-grid">
                    {items.map((it) => (
                      <div key={it.userId} className="card">
                        <div className="card-body">
                          <div className="row-between">
                            <div>
                              <div className="card-title">{it.fullName || it.userName}</div>
                              <div className="card-subtext">{it.email} • {it.phoneNumber || ''}</div>
                            </div>
                            <CTA as="button" onClick={() => { setUserIdInput(String(it.userId)); loadUserData(it.userId) }}>View</CTA>
                          </div>
                          <div className="row-between">
                            <span className="badge gray">ID: {it.citizen?.status || 'Not submitted'}</span>
                            <span className="badge gray">License: {it.driver?.status || 'Not submitted'}</span>
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>
                  <div className="row-between">
                    <button className="btn" disabled={page<=1 || loading} onClick={()=>loadList(page-1)}>Previous</button>
                    <div className="card-subtext">Page {page} / {Math.max(1, Math.ceil(total / pageSize))}</div>
                    <button className="btn" disabled={page>=Math.ceil(total / pageSize) || loading} onClick={()=>loadList(page+1)}>Next</button>
                  </div>
                </div>
              </div>
              <div className="card">
                <div className="card-header">
                  <h2 className="card-title">ID Card</h2>
                </div>
                <div className="card-body">
                  {!citizen && <p className="muted">No data</p>}
                  {citizen && (
                    <>
                      <InfoRow label="Status" value={citizen.status} />
                      <InfoRow label="Full Name" value={citizen.fullName} />
                      <InfoRow label="ID Number" value={citizen.citizenId} />
                      <InfoRow label="Gender" value={citizen.sex} />
                      <InfoRow label="Date of Birth" value={citizen.dayOfBirth} />
                      <InfoRow label="Issue Date" value={citizen.citiRegisDate} />
                      <InfoRow label="Issued By" value={citizen.citiRegisOffice} />
                      <InfoRow label="Address" value={citizen.address} />
                      <Images list={citizen.images} />
                      <div className="row">
                        <CTA as="button" onClick={() => handleApprove('citizen', true)} disabled={loading}>Approve</CTA>
                        <button className="btn" onClick={() => handleApprove('citizen', false)} disabled={loading}>Reject</button>
                      </div>
                    </>
                  )}
                </div>
              </div>

              <div className="card">
                <div className="card-header">
                  <h2 className="card-title">Driver's License</h2>
                </div>
                <div className="card-body">
                  {!license && <p className="muted">No data</p>}
                  {license && (
                    <>
                      <InfoRow label="Status" value={license.status} />
                      <InfoRow label="Full Name" value={license.fullName} />
                      <InfoRow label="License Number" value={license.licenseId} />
                      <InfoRow label="Type" value={license.licenseType} />
                      <InfoRow label="Issue Date" value={license.registerDate} />
                      <InfoRow label="Issued By" value={license.registerOffice} />
                      <Images list={license.images} />
                      <div className="row">
                        <CTA as="button" onClick={() => handleApprove('license', true)} disabled={loading}>Approve</CTA>
                        <button className="btn" onClick={() => handleApprove('license', false)} disabled={loading}>Reject</button>
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
