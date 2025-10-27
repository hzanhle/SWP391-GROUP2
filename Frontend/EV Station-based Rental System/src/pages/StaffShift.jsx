import { useEffect, useState } from 'react'
import { Card, CardContent, CardHeader, Typography } from '@mui/material'
import StaffLayout from '../components/staff/StaffLayout'
import { getMyShifts, checkIn, checkOut } from '../api/staffShiff'
import '../styles/admin.css'

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

  const rawUser = (typeof window !== 'undefined' && localStorage.getItem('auth.user')) || '{}'
  let currentRoleId = 0
  try { currentRoleId = Number(JSON.parse(rawUser)?.roleId || JSON.parse(rawUser)?.RoleId || 0) } catch {}
  const forbidden = currentRoleId !== 2

  const today = new Date()
  const todayStart = new Date(today.getFullYear(), today.getMonth(), today.getDate()).toISOString().split('T')[0]

  useEffect(() => {
    let mounted = true
    ;(async () => {
      try {
        setLoading(true)
        const response = await getMyShifts(null, null, token)
        if (!mounted) return
        const shiftList = Array.isArray(response.data?.data) ? response.data.data : (Array.isArray(response.data) ? response.data : [])
        setShifts(shiftList)

        const today_shift = shiftList.find(s => {
          const shiftDate = new Date(s.shiftDate || s.ShiftDate).toISOString().split('T')[0]
          return shiftDate === todayStart
        })
        setTodayShift(today_shift || null)
        setError('')
      } catch (e) {
        setError(e.message || 'Failed to load shifts')
      } finally {
        setLoading(false)
      }
    })()
    return () => { mounted = false }
  }, [token, todayStart])

  async function handleCheckIn() {
    if (!todayShift) return
    try {
      setCheckingInOut(true)
      await checkIn({ shiftId: todayShift.id || todayShift.Id }, token)
      setSuccessMessage('Check-in successful!')
      setTimeout(() => setSuccessMessage(''), 3000)
      const response = await getMyShifts(null, null, token)
      const shiftList = Array.isArray(response.data?.data) ? response.data.data : (Array.isArray(response.data) ? response.data : [])
      setShifts(shiftList)
      const updated_shift = shiftList.find(s => (s.id || s.Id) === (todayShift.id || todayShift.Id))
      setTodayShift(updated_shift || null)
    } catch (e) {
      setError(e.message || 'Failed to check in')
    } finally {
      setCheckingInOut(false)
    }
  }

  async function handleCheckOut() {
    if (!todayShift) return
    try {
      setCheckingInOut(true)
      await checkOut({ shiftId: todayShift.id || todayShift.Id }, token)
      setSuccessMessage('Check-out successful!')
      setTimeout(() => setSuccessMessage(''), 3000)
      const response = await getMyShifts(null, null, token)
      const shiftList = Array.isArray(response.data?.data) ? response.data.data : (Array.isArray(response.data) ? response.data : [])
      setShifts(shiftList)
      const updated_shift = shiftList.find(s => (s.id || s.Id) === (todayShift.id || todayShift.Id))
      setTodayShift(updated_shift || null)
    } catch (e) {
      setError(e.message || 'Failed to check out')
    } finally {
      setCheckingInOut(false)
    }
  }

  function formatTime(timeStr) {
    if (!timeStr) return '-'
    try {
      return new Date(timeStr).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' })
    } catch {
      return timeStr
    }
  }

  function formatDate(dateStr) {
    if (!dateStr) return '-'
    try {
      return new Date(dateStr).toLocaleDateString('en-US', { weekday: 'short', year: 'numeric', month: 'short', day: 'numeric' })
    } catch {
      return dateStr
    }
  }

  if (forbidden) {
    return (
      <StaffLayout active="shifts">
        <section className="section">
          <div className="container">
            <div className="card card-body">
              <h1 className="section-title">Unauthorized</h1>
              <p className="section-subtitle">You do not have permission to access this page.</p>
              <a className="btn btn-primary" href="#" style={{ display: 'inline-block', marginTop: '1rem', padding: '0.75rem 1.5rem', borderRadius: '0.5rem', backgroundColor: '#ff4d30', color: '#fff', textDecoration: 'none' }}>Back to Home</a>
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
            <div role="alert" className="card card-body" style={{ backgroundColor: '#fee2e2', borderColor: '#fecaca', color: '#991b1b', marginBottom: '1.5rem' }}>{error}</div>
          ) : null}

          {successMessage ? (
            <div role="status" className="card card-body" style={{ backgroundColor: '#dcfce7', borderColor: '#bbf7d0', color: '#166534', marginBottom: '1.5rem' }}>{successMessage}</div>
          ) : null}

          {loading ? (
            <div className="card card-body">Loading shifts...</div>
          ) : (
            <>
              {todayShift ? (
                <div className="admin-grid" style={{ marginBottom: '2rem' }}>
                  <div className="col-12">
                    <Card className="admin-card" style={{ padding: '1.5rem' }}>
                      <div className="row-between">
                        <div>
                          <h2 className="card-title">Today's Shift</h2>
                          <p className="card-subtext">{formatDate(todayShift.shiftDate || todayShift.ShiftDate)}</p>
                        </div>
                        <div style={{ display: 'flex', gap: '1rem', alignItems: 'center' }}>
                          {!todayShift.checkInTime && (
                            <button
                              onClick={handleCheckIn}
                              disabled={checkingInOut}
                              style={{
                                padding: '0.75rem 1.5rem',
                                backgroundColor: '#16a34a',
                                color: '#fff',
                                border: 'none',
                                borderRadius: '0.5rem',
                                cursor: checkingInOut ? 'not-allowed' : 'pointer',
                                opacity: checkingInOut ? 0.6 : 1,
                              }}
                            >
                              {checkingInOut ? 'Processing...' : 'Check In'}
                            </button>
                          )}
                          {todayShift.checkInTime && !todayShift.checkOutTime && (
                            <button
                              onClick={handleCheckOut}
                              disabled={checkingInOut}
                              style={{
                                padding: '0.75rem 1.5rem',
                                backgroundColor: '#dc2626',
                                color: '#fff',
                                border: 'none',
                                borderRadius: '0.5rem',
                                cursor: checkingInOut ? 'not-allowed' : 'pointer',
                                opacity: checkingInOut ? 0.6 : 1,
                              }}
                            >
                              {checkingInOut ? 'Processing...' : 'Check Out'}
                            </button>
                          )}
                          {todayShift.checkInTime && todayShift.checkOutTime && (
                            <span className="badge green">Completed</span>
                          )}
                        </div>
                      </div>

                      <div className="admin-grid" style={{ marginTop: '1.5rem', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))' }}>
                        <div>
                          <p style={{ fontSize: '0.875rem', color: '#706f7b', margin: '0 0 0.5rem' }}>Station</p>
                          <p style={{ fontSize: '1.125rem', fontWeight: '600', color: '#010103', margin: 0 }}>
                            {todayShift.stationName || todayShift.StationName || 'N/A'}
                          </p>
                        </div>
                        <div>
                          <p style={{ fontSize: '0.875rem', color: '#706f7b', margin: '0 0 0.5rem' }}>Start Time</p>
                          <p style={{ fontSize: '1.125rem', fontWeight: '600', color: '#010103', margin: 0 }}>
                            {formatTime(todayShift.startTime || todayShift.StartTime)}
                          </p>
                        </div>
                        <div>
                          <p style={{ fontSize: '0.875rem', color: '#706f7b', margin: '0 0 0.5rem' }}>End Time</p>
                          <p style={{ fontSize: '1.125rem', fontWeight: '600', color: '#010103', margin: 0 }}>
                            {formatTime(todayShift.endTime || todayShift.EndTime)}
                          </p>
                        </div>
                        <div>
                          <p style={{ fontSize: '0.875rem', color: '#706f7b', margin: '0 0 0.5rem' }}>Check In</p>
                          <p style={{ fontSize: '1.125rem', fontWeight: '600', color: '#010103', margin: 0 }}>
                            {formatTime(todayShift.checkInTime || todayShift.CheckInTime)}
                          </p>
                        </div>
                        <div>
                          <p style={{ fontSize: '0.875rem', color: '#706f7b', margin: '0 0 0.5rem' }}>Check Out</p>
                          <p style={{ fontSize: '1.125rem', fontWeight: '600', color: '#010103', margin: 0 }}>
                            {formatTime(todayShift.checkOutTime || todayShift.CheckOutTime)}
                          </p>
                        </div>
                      </div>
                    </Card>
                  </div>
                </div>
              ) : (
                <div className="card card-body" style={{ textAlign: 'center', padding: '2rem' }}>
                  <p style={{ color: '#706f7b', fontSize: '1.125rem' }}>No shift scheduled for today</p>
                </div>
              )}

              <div>
                <h2 style={{ fontSize: '1.5rem', fontWeight: '700', color: '#010103', marginBottom: '1rem', fontFamily: 'Poppins, sans-serif' }}>All Shifts</h2>
                {shifts.length > 0 ? (
                  <div style={{ overflowX: 'auto' }}>
                    <table style={{ borderCollapse: 'collapse', width: '100%' }}>
                      <thead style={{ backgroundColor: '#f0f9ff', borderBottom: '2px solid #e2e8f0' }}>
                        <tr>
                          <th style={{ padding: '1rem', textAlign: 'left', fontWeight: '600', color: '#010103', fontFamily: 'Poppins, sans-serif', fontSize: '0.875rem' }}>Date</th>
                          <th style={{ padding: '1rem', textAlign: 'left', fontWeight: '600', color: '#010103', fontFamily: 'Poppins, sans-serif', fontSize: '0.875rem' }}>Station</th>
                          <th style={{ padding: '1rem', textAlign: 'left', fontWeight: '600', color: '#010103', fontFamily: 'Poppins, sans-serif', fontSize: '0.875rem' }}>Time</th>
                          <th style={{ padding: '1rem', textAlign: 'left', fontWeight: '600', color: '#010103', fontFamily: 'Poppins, sans-serif', fontSize: '0.875rem' }}>Check In</th>
                          <th style={{ padding: '1rem', textAlign: 'left', fontWeight: '600', color: '#010103', fontFamily: 'Poppins, sans-serif', fontSize: '0.875rem' }}>Check Out</th>
                          <th style={{ padding: '1rem', textAlign: 'left', fontWeight: '600', color: '#010103', fontFamily: 'Poppins, sans-serif', fontSize: '0.875rem' }}>Status</th>
                        </tr>
                      </thead>
                      <tbody>
                        {shifts.map((shift, idx) => (
                          <tr key={idx} style={{ borderBottom: '1px solid #e2e8f0' }}>
                            <td style={{ padding: '1rem', color: '#706f7b', fontFamily: 'Rubik, sans-serif' }}>{formatDate(shift.shiftDate || shift.ShiftDate)}</td>
                            <td style={{ padding: '1rem', color: '#706f7b', fontFamily: 'Rubik, sans-serif' }}>{shift.stationName || shift.StationName || 'N/A'}</td>
                            <td style={{ padding: '1rem', color: '#706f7b', fontFamily: 'Rubik, sans-serif' }}>
                              {formatTime(shift.startTime || shift.StartTime)} - {formatTime(shift.endTime || shift.EndTime)}
                            </td>
                            <td style={{ padding: '1rem', color: '#706f7b', fontFamily: 'Rubik, sans-serif' }}>{formatTime(shift.checkInTime || shift.CheckInTime)}</td>
                            <td style={{ padding: '1rem', color: '#706f7b', fontFamily: 'Rubik, sans-serif' }}>{formatTime(shift.checkOutTime || shift.CheckOutTime)}</td>
                            <td style={{ padding: '1rem' }}>
                              {shift.checkInTime && shift.checkOutTime ? (
                                <span className="badge green" style={{ backgroundColor: '#dcfce7', color: '#166534' }}>Completed</span>
                              ) : shift.checkInTime ? (
                                <span className="badge orange" style={{ backgroundColor: '#fef3c7', color: '#92400e' }}>Checked In</span>
                              ) : (
                                <span className="badge gray" style={{ backgroundColor: '#f3f4f6', color: '#6b7280' }}>Pending</span>
                              )}
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                ) : (
                  <div className="card card-body" style={{ textAlign: 'center', padding: '2rem' }}>
                    <p style={{ color: '#706f7b', fontSize: '1rem' }}>No shifts available</p>
                  </div>
                )}
              </div>
            </>
          )}
        </div>
      </section>
    </StaffLayout>
  )
}
