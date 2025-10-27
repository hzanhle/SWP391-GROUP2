import React, { useEffect, useState } from 'react'
import AdminLayout from '../components/admin/AdminLayout'
import * as staffApi from '../api/staffShiff'
import '../styles/admin.css'

export default function AdminStaffShift() {
  const token = typeof window !== 'undefined' ? localStorage.getItem('auth.token') : ''
  const [shifts, setShifts] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [form, setForm] = useState({ userId: '', stationId: '', shiftDate: '', startTime: '', endTime: '' })
  const [submitting, setSubmitting] = useState(false)

  useEffect(() => {
    let mounted = true
    ;(async () => {
      try {
        setLoading(true)
        const res = await staffApi.getAllShifts(token)
        if (!mounted) return
        setShifts(Array.isArray(res.data?.data) ? res.data.data : (Array.isArray(res.data) ? res.data : []))
        setError('')
      } catch (e) {
        setError(e.message || 'Không tải được ca làm việc')
      } finally { if (mounted) setLoading(false) }
    })()
    return () => { mounted = false }
  }, [token])

  async function handleCreate(e) {
    e.preventDefault()
    setSubmitting(true)
    try {
      const payload = {
        userId: Number(form.userId) || undefined,
        stationId: Number(form.stationId) || undefined,
        shiftDate: form.shiftDate,
        startTime: form.startTime,
        endTime: form.endTime,
      }
      const res = await staffApi.createShift(payload, token)
      const created = res.data?.data || res.data
      setShifts(prev => [created, ...prev])
      setForm({ userId: '', stationId: '', shiftDate: '', startTime: '', endTime: '' })
      setError('')
    } catch (e) {
      setError(e.message || 'Tạo ca thất bại')
    } finally { setSubmitting(false) }
  }

  async function handleDelete(id) {
    if (!confirm('Xóa ca làm việc này?')) return
    try {
      await staffApi.deleteShift(id, token)
      setShifts(prev => prev.filter(s => Number(s.id ?? s.Id) !== Number(id)))
    } catch (e) {
      alert(e.message || 'Xóa thất bại')
    }
  }

  return (
    <AdminLayout active="staffshift">
      <section className="section">
        <div className="container">
          <div className="section-header">
            <h1 className="section-title">Ca làm việc nhân viên</h1>
            <p className="section-subtitle">Quản lý ca làm việc của nhân viên - Tạo / Xóa / Xem</p>
          </div>

          {error ? <div role="alert" className="card card-body">{error}</div> : null}

          <div className="card card-body" style={{ marginBottom: '1rem' }}>
            <h3 className="sub-title">Tạo ca mới</h3>
            <form onSubmit={handleCreate} className="staffshift-form">
              <div className="form-grid">
                <label className="field">
                  <span className="label">UserId</span>
                  <input className="input" value={form.userId} onChange={e => setForm({ ...form, userId: e.target.value })} />
                </label>
                <label className="field">
                  <span className="label">StationId</span>
                  <input className="input" value={form.stationId} onChange={e => setForm({ ...form, stationId: e.target.value })} />
                </label>
                <label className="field">
                  <span className="label">Shift Date</span>
                  <input type="date" className="input" value={form.shiftDate} onChange={e => setForm({ ...form, shiftDate: e.target.value })} />
                </label>
                <label className="field">
                  <span className="label">Start Time</span>
                  <input type="time" className="input" value={form.startTime} onChange={e => setForm({ ...form, startTime: e.target.value })} />
                </label>
                <label className="field">
                  <span className="label">End Time</span>
                  <input type="time" className="input" value={form.endTime} onChange={e => setForm({ ...form, endTime: e.target.value })} />
                </label>
              </div>
              <div style={{ marginTop: '1rem' }}>
                <button className="btn btn-primary" type="submit" disabled={submitting}>{submitting ? 'Đang tạo...' : 'Tạo ca'}</button>
              </div>
            </form>
          </div>

          {loading ? (
            <div className="card card-body">Đang tải ca làm việc...</div>
          ) : (
            <div className="card card-body">
              <h3 className="sub-title">Danh sách ca</h3>
              <div className="table-responsive">
                <table className="table">
                  <thead>
                    <tr>
                      <th>ID</th>
                      <th>Nhân viên</th>
                      <th>Trạm</th>
                      <th>Ngày</th>
                      <th>Thời gian</th>
                      <th>Trạng thái</th>
                      <th>Hành động</th>
                    </tr>
                  </thead>
                  <tbody>
                    {(shifts || []).map(s => (
                      <tr key={s.id ?? s.Id}>
                        <td>{s.id ?? s.Id}</td>
                        <td>{s.userFullName ?? s.UserFullName ?? s.userId ?? s.UserId}</td>
                        <td>{s.station?.name ?? s.Station?.Name ?? s.stationName ?? s.StationName ?? (s.stationId ?? s.StationId)}</td>
                        <td>{(s.shiftDate || s.ShiftDate || '').split('T')[0]}</td>
                        <td>{(s.startTime || s.StartTime) || ''} - {(s.endTime || s.EndTime) || ''}</td>
                        <td>{s.status ?? s.Status ?? ''}</td>
                        <td>
                          <button className="btn btn-danger" onClick={() => handleDelete(s.id ?? s.Id)}>Xóa</button>
                        </td>
                      </tr>
                    ))}
                    {!(shifts || []).length ? (
                      <tr><td colSpan={7}>Không c�� ca</td></tr>
                    ) : null}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </div>
      </section>
    </AdminLayout>
  )
}
