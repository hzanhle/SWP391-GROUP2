import { useEffect, useMemo, useState } from 'react'
import { getDashboardSummary, getRevenueByMonth, getTopUsedVehicles, getUserGrowth } from '../api/adminDashboard'
import { getAllStations as fetchAllStations } from '../api/station'
import { Card, CardContent, CardHeader, Typography } from '@mui/material'
import { LineChart, BarChart, PieChart } from '@mui/x-charts'
import AdminLayout from '../components/admin/AdminLayout'
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

export default function AdminDashboard() {
  const token = typeof window !== 'undefined' ? localStorage.getItem('auth.token') : ''
  const [summary, setSummary] = useState(null)
  const [revenue, setRevenue] = useState([])
  const [topVehicles, setTopVehicles] = useState([])
  const [stationStats, setStationStatsState] = useState(null)
  const [stationList, setStationList] = useState([])
  const [userGrowth, setUserGrowth] = useState([])
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)

  const rawUser = (typeof window !== 'undefined' && localStorage.getItem('auth.user')) || '{}'
  let currentRoleId = 0
  try { currentRoleId = Number(JSON.parse(rawUser)?.roleId || JSON.parse(rawUser)?.RoleId || 0) } catch {}
  const forbidden = currentRoleId !== 3

  useEffect(() => {
    let mounted = true
    ;(async () => {
      try {
        setLoading(true)
        const [s, r, v, st, ug, sl] = await Promise.all([
          getDashboardSummary(token).catch(e => { throw e }), // summary is required
          getRevenueByMonth(new Date().getFullYear(), token).catch(() => ({ data: [] })),
          getTopUsedVehicles(10, token).catch(() => ({ data: [] })),
          // Avoid calling stations stats endpoint (currently failing on server); rely on station list instead
          Promise.resolve({ data: null }),
          getUserGrowth(token).catch(() => ({ data: [] })),
          fetchAllStations(token).catch(() => ({ data: [] }))
        ])
        if (!mounted) return
        setSummary(s.data || null)
        setRevenue(Array.isArray(r.data) ? r.data : [])
        setTopVehicles(Array.isArray(v.data) ? v.data : [])
        setStationStatsState(st.data ?? null)
        setStationList(Array.isArray(sl.data) ? sl.data : [])
        setUserGrowth(Array.isArray(ug.data) ? ug.data : [])
        setError('')
      } catch (e) {
        setError(e.message || 'Unable to load dashboard data')
      } finally {
        setLoading(false)
      }
    })()
    return () => { mounted = false }
  }, [token])

  const revenueSeries = useMemo(() => {
    const months = Array.from({ length: 12 }, (_, i) => i + 1)
    const map = new Map((revenue || []).map(r => [Number(r.month ?? r.Month), Number(r.totalRevenue ?? r.TotalRevenue ?? 0)]))
    const values = months.map(m => map.get(m) ?? 0)
    return {
      months: months.map(m => String(m)).filter(m => m !== null && m !== undefined),
      values: values.filter(v => v !== null && v !== undefined)
    }
  }, [revenue])

  if (forbidden) {
    return (
      <AdminLayout active="overview">
        <section className="section">
          <div className="container">
            <div className="card card-body">
              <h1 className="section-title">Unauthorized</h1>
              <p className="section-subtitle">You do not have permission to access the Admin Dashboard.</p>
              <a className="btn" href="#">Go to Home</a>
            </div>
          </div>
        </section>
      </AdminLayout>
    )
  }

  return (
    <AdminLayout active="overview">
      <section className="section">
        <div className="container">
          <div className="section-header">
            <h1 className="section-title">Dashboard</h1>
            <p className="section-subtitle">System overview and key metrics</p>
          </div>

          {error ? (
            <div role="alert" className="card card-body">{error}</div>
          ) : null}

          {loading ? (
            <div className="card card-body">Loading...</div>
          ) : (
            <div className="admin-grid">
              <div className="col-12 lg-3"><StatCard title="Total Users" value={summary?.totalUsers ?? summary?.TotalUsers ?? 0} caption="Active/Total" /></div>
              <div className="col-12 lg-3"><StatCard title="Bookings" value={summary?.totalBookings ?? summary?.TotalBookings ?? 0} /></div>
              <div className="col-12 lg-3"><StatCard title="Revenue (Year)" value={(summary?.yearlyRevenue ?? summary?.YearlyRevenue ?? 0).toLocaleString()} /></div>
              <div className="col-12 lg-3"><StatCard title="Total Stations" value={summary?.totalStations ?? summary?.TotalStations ?? 0} /></div>

              <div className="col-12 lg-8">
                <Card className="admin-card">
                  <CardHeader title="Revenue by Month" />
                  <CardContent>
                    {revenueSeries && revenueSeries.values.length > 0 ? (
                      <LineChart
                        height={280}
                        series={[{ data: revenueSeries.values.filter(v => v !== null && v !== undefined), label: 'Revenue' }]}
                        xAxis={[{ scaleType: 'point', data: revenueSeries.months.map((m, idx) => `T${m}`).filter(label => label !== null && label !== undefined) }]}
                      />
                    ) : (
                      <div className="empty-placeholder">No data</div>
                    )}
                  </CardContent>
                </Card>
              </div>

              <div className="col-12 lg-4">
                <Card className="admin-card">
                  <CardHeader title="Top Vehicles" />
                  <CardContent>
                    {topVehicles && topVehicles.length > 0 ? (
                      <BarChart
                        height={280}
                        yAxis={[{ scaleType: 'band', data: (topVehicles || []).map((v, idx) => (v.modelName || v.ModelName || v.name || v.Name || `Vehicle ${idx + 1}`)).filter(name => name !== null && name !== undefined) }]}
                        series={[{ data: (topVehicles || []).map(v => Number(v.usageCount ?? v.UsageCount ?? v.count ?? 0)), label: 'Rentals' }]}
                        layout="horizontal"
                      />
                    ) : (
                      <div className="empty-placeholder">No data</div>
                    )}
                  </CardContent>
                </Card>
              </div>

              <div className="col-12 lg-6">
                <Card className="admin-card">
                  <CardHeader title="User Growth" />
                  <CardContent>
                    {userGrowth && userGrowth.length > 0 ? (
                      <LineChart
                        height={260}
                        series={[{ data: (userGrowth || []).map(u => Number(u.count ?? u.Count ?? 0)), label: 'Users' }]}
                        xAxis={[{ scaleType: 'point', data: (userGrowth || []).map((u, idx) => String(u.label || u.Label || u.month || u.Month || `Month ${idx + 1}`)).filter(label => label !== null && label !== undefined) }]}
                      />
                    ) : (
                      <div className="empty-placeholder">No data</div>
                    )}
                  </CardContent>
                </Card>
              </div>

              <div className="col-12 lg-6">
                <Card className="admin-card">
                  <CardHeader title="Station Status" />
                  <CardContent>
                    {(() => {
                      // Prefer live station list for accurate status; fallback to admin aggregate
                      let on = 0, off = 0
                      const toBool = (v) => {
                        if (typeof v === 'boolean') return v
                        if (typeof v === 'number') return v === 1
                        if (typeof v === 'string') { const t = v.toLowerCase(); return t === 'true' || t === 'active' || t === 'operational' || t === 'online' || v === '1' }
                        return false
                      }
                      if (Array.isArray(stationList) && stationList.length > 0) {
                        const items = stationList
                        on = items.filter(s => toBool(
                          s.isOperational ?? s.IsOperational ?? s.isActive ?? s.IsActive ?? s.active ?? s.Active ?? s.status ?? s.Status
                        )).length
                        off = items.length - on
                      } else if (stationStats && typeof stationStats === 'object') {
                        const obj = stationStats
                        const getNum = (v) => Number(v ?? 0)
                        const onKeys = ['operational','Operational','active','Active','on','On','online','Online','operationalStations','OperationalStations','activeStations','ActiveStations','totalActive','TotalActive']
                        const offKeys = ['maintenance','Maintenance','inactive','Inactive','off','Off','offline','Offline','maintenanceStations','MaintenanceStations','inactiveStations','InactiveStations','notOperational','NotOperational','totalInactive','TotalInactive']
                        on = onKeys.reduce((acc, k) => acc + getNum(obj[k]), 0)
                        off = offKeys.reduce((acc, k) => acc + getNum(obj[k]), 0)
                        if (on === 0 && off === 0 && typeof obj.total === 'number') {
                          on = getNum(obj.total)
                        }
                      }
                      const hasData = (on + off) > 0
                      return hasData ? (
                        <PieChart
                          height={260}
                          series={[{ data: [
                            { id: 0, value: on, label: 'Operating' },
                            { id: 1, value: off, label: 'Maintenance' },
                          ] }]} />
                      ) : (
                        <div className="empty-placeholder">No data</div>
                      )
                    })()}
                  </CardContent>
                </Card>
              </div>
            </div>
          )}
        </div>
      </section>
    </AdminLayout>
  )
}
