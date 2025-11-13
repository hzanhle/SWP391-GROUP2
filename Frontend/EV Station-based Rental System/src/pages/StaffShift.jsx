import { useEffect, useState } from 'react'
import { Card, CardContent, CardHeader, Typography } from '@mui/material'
import StaffLayout from '../components/staff/StaffLayout'
import { getMyShifts, checkIn, checkOut } from '../api/staffShiff'
import '../styles/staff.css'

function ConfirmDialog({ isOpen, title, message, onConfirm, onCancel, loading }) {
  if (!isOpen) return null
  return (
    <div style={{
      position: 'fixed',
      top: 0,
      left: 0,
      right: 0,
      bottom: 0,
      backgroundColor: 'rgba(0, 0, 0, 0.5)',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      zIndex: 1000
    }}>
      <div style={{
        backgroundColor: 'var(--staff-bg-primary)',
        borderRadius: '0.5rem',
        boxShadow: '0 10px 40px rgba(0, 0, 0, 0.3)',
        maxWidth: '400px',
        width: '90%',
        padding: '2rem'
      }}>
        <h2 style={{
          fontSize: '1.25rem',
          fontWeight: '700',
          color: 'var(--staff-text-primary)',
          marginBottom: '1rem',
          marginTop: 0
        }}>{title}</h2>
        <p style={{
          fontSize: '1rem',
          color: 'var(--staff-text-secondary)',
          marginBottom: '2rem',
          lineHeight: '1.5'
        }}>{message}</p>
        <div style={{
          display: 'flex',
          gap: '1rem',
          justifyContent: 'flex-end'
        }}>
          <button
            onClick={onCancel}
            disabled={loading}
            style={{
              padding: '0.75rem 1.5rem',
              borderRadius: '0.375rem',
              border: '1px solid var(--staff-border)',
              backgroundColor: 'var(--staff-bg-primary)',
              color: 'var(--staff-text-secondary)',
              cursor: loading ? 'not-allowed' : 'pointer',
              fontSize: '0.875rem',
              fontWeight: '600',
              opacity: loading ? 0.6 : 1,
              transition: 'all 0.3s ease'
            }}
          >
            Cancel
          </button>
          <button
            onClick={onConfirm}
            disabled={loading}
            style={{
              padding: '0.75rem 1.5rem',
              borderRadius: '0.375rem',
              border: 'none',
              backgroundColor: 'var(--staff-accent)',
              color: '#fff',
              cursor: loading ? 'not-allowed' : 'pointer',
              fontSize: '0.875rem',
              fontWeight: '600',
              opacity: loading ? 0.6 : 1,
              transition: 'all 0.3s ease'
            }}
          >
            {loading ? 'Processing...' : 'Confirm'}
          </button>
        </div>
      </div>
    </div>
  )
}

function StatCard({ title, value, caption }) {
  return (
    <Card className="admin-card stat-card">
      <CardHeader title={<span className="stat-title">{title}</span>} />
      <CardContent>
        <div className="stat-value">{value}</div>
        {caption ? <div className="stat-caption">{caption}</div> : null}
      </CardContent>
    </Card>
  )
}

export default function StaffShift() {
  const token = typeof window !== 'undefined' ? localStorage.getItem('auth.token') : ''
  const [shifts, setShifts] = useState([])
  const [todayShift, setTodayShift] = useState(null)
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)
  const [checkingInOut, setCheckingInOut] = useState(false)
  const [successMessage, setSuccessMessage] = useState('')
  const [confirmDialog, setConfirmDialog] = useState({ isOpen: false, type: null, loading: false })

  const rawUser = (typeof window !== 'undefined' && localStorage.getItem('auth.user')) || '{}'
  let currentRoleId = 0
  try { currentRoleId = Number(JSON.parse(rawUser)?.roleId || JSON.parse(rawUser)?.RoleId || 0) } catch {}
  const forbidden = currentRoleId !== 2

  const today = new Date()
  const todayStart = new Date(today.getFullYear(), today.getMonth(), today.getDate()).toISOString().split('T')[0]

  function getShiftId(shift) {
    return shift?.id ?? shift?.Id ?? shift?.shiftId ?? shift?.ShiftId ?? null
  }

  function normalizeShift(shift) {
    const shiftDateRaw = shift.shiftDate || shift.ShiftDate || null
    let shiftDateObj = null
    if (shiftDateRaw) {
      shiftDateObj = new Date(shiftDateRaw)
      if (Number.isNaN(shiftDateObj.getTime())) {
        // try normalizing by appending T00:00:00
        const fallback = new Date(`${shiftDateRaw}T00:00:00`)
        shiftDateObj = Number.isNaN(fallback.getTime()) ? null : fallback
      }
    }

    const shiftDateISO = shiftDateObj ? shiftDateObj.toISOString().split('T')[0] : null

    return {
      ...shift,
      id: getShiftId(shift),
      shiftDate: shiftDateRaw,
      shiftDateObj,
      shiftDateISO,
      startTime: shift.startTime ?? shift.StartTime ?? null,
      endTime: shift.endTime ?? shift.EndTime ?? null,
      checkInTime: shift.checkInTime ?? shift.CheckInTime ?? shift.actualCheckInTime ?? shift.ActualCheckInTime ?? null,
      checkOutTime: shift.checkOutTime ?? shift.CheckOutTime ?? shift.actualCheckOutTime ?? shift.ActualCheckOutTime ?? null,
      stationName: shift.stationName || shift.StationName || 'N/A',
      status: (shift.status ?? shift.Status ?? '').trim()
    }
  }

  useEffect(() => {
    let mounted = true
    ;(async () => {
      try {
        setLoading(true)
        const response = await getMyShifts(null, null, token)
        if (!mounted) return
        const rawList = Array.isArray(response.data?.data) ? response.data.data : (Array.isArray(response.data) ? response.data : [])
        const normalizedList = rawList.map(normalizeShift)
        setShifts(normalizedList)

        const today_shift = normalizedList.find(s => s.shiftDateISO === todayStart)
        setTodayShift(today_shift ?? null)
        setError('')
      } catch (e) {
        setError(e.message || 'Failed to load shifts')
      } finally {
        setLoading(false)
      }
    })()
    return () => { mounted = false }
  }, [token, todayStart])

  function openConfirm(type) {
    setConfirmDialog({ isOpen: true, type, loading: false })
    setError('')
  }

  async function confirmCheckIn() {
    if (!todayShift) return
    setConfirmDialog(prev => ({ ...prev, loading: true }))
    try {
      const shiftId = getShiftId(todayShift)
      await checkIn({ shiftId }, token)
      setSuccessMessage('Check-in successful!')
      setTimeout(() => setSuccessMessage(''), 3000)
      const response = await getMyShifts(null, null, token)
      const rawList = Array.isArray(response.data?.data) ? response.data.data : (Array.isArray(response.data) ? response.data : [])
      const normalizedList = rawList.map(normalizeShift)
      setShifts(normalizedList)
      const updated_shift = normalizedList.find(s => s.id === shiftId)
      setTodayShift(updated_shift || null)
      setConfirmDialog({ isOpen: false, type: null, loading: false })
    } catch (e) {
      setError(e.message || 'Failed to check in')
      setConfirmDialog(prev => ({ ...prev, loading: false }))
    }
  }

  async function confirmCheckOut() {
    if (!todayShift) return
    setConfirmDialog(prev => ({ ...prev, loading: true }))
    try {
      const shiftId = getShiftId(todayShift)
      await checkOut({ shiftId }, token)
      setSuccessMessage('Check-out successful!')
      setTimeout(() => setSuccessMessage(''), 3000)
      const response = await getMyShifts(null, null, token)
      const rawList = Array.isArray(response.data?.data) ? response.data.data : (Array.isArray(response.data) ? response.data : [])
      const normalizedList = rawList.map(normalizeShift)
      setShifts(normalizedList)
      const updated_shift = normalizedList.find(s => s.id === shiftId)
      setTodayShift(updated_shift || null)
      setConfirmDialog({ isOpen: false, type: null, loading: false })
    } catch (e) {
      setError(e.message || 'Failed to check out')
      setConfirmDialog(prev => ({ ...prev, loading: false }))
    }
  }

  function buildDateFromParts(dateStr, timeStr) {
    if (!timeStr) return null

    // Direct parse (covers ISO strings)
    const direct = new Date(timeStr)
    if (!Number.isNaN(direct.getTime())) return direct

    const segments = timeStr.split(':')
    if (segments.length < 2) return null

    const [hours, minutes, seconds] = segments.map(Number)
    if ([hours, minutes, seconds].some(v => Number.isNaN(v))) return null

    const base = dateStr ? new Date(dateStr) : new Date()
    if (Number.isNaN(base.getTime())) {
      const fallback = dateStr ? new Date(`${dateStr}T00:00:00`) : new Date()
      if (Number.isNaN(fallback.getTime())) return null
      fallback.setHours(hours || 0, minutes || 0, seconds || 0, 0)
      return fallback
    }

    const result = new Date(base)
    result.setHours(hours || 0, minutes || 0, seconds || 0, 0)
    return result
  }

  function formatTime(timeStr, dateStr) {
    if (!timeStr) return '-'
    const dt = buildDateFromParts(dateStr, timeStr)
    if (!dt || Number.isNaN(dt.getTime())) return '-'
    return dt.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
  }

  function formatDate(dateStr) {
    if (!dateStr) return '-'
    try {
      return new Date(dateStr).toLocaleDateString('en-US', { weekday: 'short', year: 'numeric', month: 'short', day: 'numeric' })
    } catch {
      return dateStr
    }
  }

  function hasCheckIn(shift) {
    return Boolean(shift?.checkInTime)
  }

  function hasCheckOut(shift) {
    return Boolean(shift?.checkOutTime)
  }

  if (forbidden) {
    return (
      <StaffLayout active="shifts">
        <section className="section">
          <div className="container">
            <div className="card card-body">
              <h1 className="section-title">Unauthorized</h1>
              <p className="section-subtitle">You do not have permission to access this page.</p>
              <a className="btn btn-primary mt-4" href="#" style={{ display: 'inline-block' }}>Back to Home</a>
            </div>
          </div>
        </section>
      </StaffLayout>
    )
  }

  return (
    <StaffLayout active="shifts">
      <section className="section">
        <div className="container">
          <div className="section-header">
            <h1 className="section-title">My Shifts</h1>
            <p className="section-subtitle">View your schedule and check in/out</p>
          </div>

          {error ? (
            <div role="alert" className="card card-body badge red" style={{ marginBottom: '1.5rem' }}>{error}</div>
          ) : null}

          {successMessage ? (
            <div role="status" className="card card-body badge green" style={{ marginBottom: '1.5rem' }}>{successMessage}</div>
          ) : null}

          {loading ? (
            <div className="card card-body">Loading shifts...</div>
          ) : (
            <>
              {todayShift ? (
                <div className="card card-body mb-4">
                  <div className="row-between mb-4">
                    <div>
                      <h2 className="card-title">Today's Shift</h2>
                      <p className="card-subtext">{formatDate(todayShift.shiftDate || todayShift.ShiftDate)}</p>
                    </div>
                    <div className="row">
                      {!hasCheckIn(todayShift) && (
                        <button
                          onClick={() => openConfirm('checkIn')}
                          disabled={confirmDialog.loading}
                          className="btn btn-primary"
                        >
                          {confirmDialog.loading ? 'Processing...' : 'Check In'}
                        </button>
                      )}
                      {hasCheckIn(todayShift) && !hasCheckOut(todayShift) && (
                        <button
                          onClick={() => openConfirm('checkOut')}
                          disabled={confirmDialog.loading}
                          style={{ backgroundColor: '#dc2626', borderColor: '#dc2626', color: '#fff' }}
                          className="btn"
                        >
                          {confirmDialog.loading ? 'Processing...' : 'Check Out'}
                        </button>
                      )}
                      {hasCheckIn(todayShift) && hasCheckOut(todayShift) && (
                        <span className="badge green">Completed</span>
                      )}
                    </div>
                  </div>

                  <div className="docs-grid">
                    <div>
                      <p className="muted mb-2">Station</p>
                      <p className="strong">
                        {todayShift.stationName || todayShift.StationName || 'N/A'}
                      </p>
                    </div>
                    <div>
                      <p className="muted mb-2">Start Time</p>
                      <p className="strong">
                        {formatTime(todayShift.startTime || todayShift.StartTime, todayShift.shiftDate || todayShift.ShiftDate)}
                      </p>
                    </div>
                    <div>
                      <p className="muted mb-2">End Time</p>
                      <p className="strong">
                        {formatTime(todayShift.endTime || todayShift.EndTime, todayShift.shiftDate || todayShift.ShiftDate)}
                      </p>
                    </div>
                    <div>
                      <p className="muted mb-2">Check In</p>
                      <p className="strong">
                        {formatTime(todayShift.checkInTime, todayShift.shiftDateISO)}
                      </p>
                    </div>
                    <div>
                      <p className="muted mb-2">Check Out</p>
                      <p className="strong">
                        {formatTime(todayShift.checkOutTime, todayShift.shiftDateISO)}
                      </p>
                    </div>
                  </div>
                </div>
              ) : (
                <div className="card card-body" style={{ textAlign: 'center' }}>
                  <p className="muted">No shift scheduled for today</p>
                </div>
              )}

              <div>
                <h2 className="section-title" style={{ marginTop: '2rem', marginBottom: '1rem' }}>All Shifts</h2>
                {shifts.length > 0 ? (
                  <div style={{ overflowX: 'auto' }}>
                    <table>
                      <thead>
                        <tr>
                          <th>Date</th>
                          <th>Station</th>
                          <th>Time</th>
                          <th>Check In</th>
                          <th>Check Out</th>
                          <th>Status</th>
                        </tr>
                      </thead>
                      <tbody>
                        {shifts.map((shift, idx) => (
                          <tr key={idx}>
                            <td>{formatDate(shift.shiftDate)}</td>
                            <td>{shift.stationName}</td>
                            <td>
                              {formatTime(shift.startTime, shift.shiftDateISO)} - {formatTime(shift.endTime, shift.shiftDateISO)}
                            </td>
                            <td>{formatTime(shift.checkInTime, shift.shiftDateISO)}</td>
                            <td>{formatTime(shift.checkOutTime, shift.shiftDateISO)}</td>
                            <td>
                              {hasCheckIn(shift) && hasCheckOut(shift) ? (
                                <span className="badge green">Completed</span>
                              ) : hasCheckIn(shift) ? (
                                <span className="badge orange">Checked In</span>
                              ) : (
                                <span className="badge gray">Pending</span>
                              )}
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                ) : (
                  <div className="card card-body">
                    <p className="muted">No shifts available</p>
                  </div>
                )}
              </div>
            </>
          )}
        </div>
      </section>

      <ConfirmDialog
        isOpen={confirmDialog.isOpen}
        title={confirmDialog.type === 'checkIn' ? 'Confirm Check-In' : 'Confirm Check-Out'}
        message={confirmDialog.type === 'checkIn'
          ? 'Are you sure you want to check in for this shift?'
          : 'Are you sure you want to check out?'}
        onConfirm={confirmDialog.type === 'checkIn' ? confirmCheckIn : confirmCheckOut}
        onCancel={() => setConfirmDialog({ isOpen: false, type: null, loading: false })}
        loading={confirmDialog.loading}
      />
    </StaffLayout>
  )
}
