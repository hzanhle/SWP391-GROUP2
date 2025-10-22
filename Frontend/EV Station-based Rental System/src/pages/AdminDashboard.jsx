import { useEffect, useMemo, useState } from 'react'
import { getDashboardSummary, getRevenueByMonth, getTopUsedVehicles, getStationStats, getUserGrowth } from '../api/adminDashboard'
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
  const [stations, setStations] = useState([])
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
        const [s, r, v, st, ug] = await Promise.all([
          getDashboardSummary(token).catch(e=>{throw e}),
          getRevenueByMonth(new Date().getFullYear(), token),
          getTopUsedVehicles(10, token),
          getStationStats(token),
          getUserGrowth(token),
        ])
        if (!mounted) return
        setSummary(s.data?.data || s.data || null)
        setRevenue(Array.isArray(r.data?.data) ? r.data.data : (Array.isArray(r.data) ? r.data : []))
        setTopVehicles(Array.isArray(v.data?.data) ? v.data.data : (Array.isArray(v.data) ? v.data : []))
        setStations(Array.isArray(st.data?.data) ? st.data.data : (Array.isArray(st.data) ? st.data : []))
        setUserGrowth(Array.isArray(ug.data?.data) ? ug.data.data : (Array.isArray(ug.data) ? ug.data : []))
        setError('')
      } catch (e) {
        setError(e.message || 'Không tải được dữ liệu dashboard')
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
    return { months, values }
  }, [revenue])

  if (forbidden) {
    return (
      <AdminLayout active="overview">
        <section className="section">
          <div className="container">
            <div className="card card-body">
              <h1 className="section-title">Unauthorized</h1>
              <p className="section-subtitle">Bạn không có quyền truy cập Dashboard Admin.</p>
              <a className="btn" href="#">Về trang chủ</a>
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
            <h1 className="section-title">Bảng điều khiển</h1>
            <p className="section-subtitle">Tổng quan hệ thống và các chỉ số chính</p>
          </div>

          {error ? (
            <div role="alert" className="card card-body">{error}</div>
          ) : null}

          {loading ? (
            <div className="card card-body">Đang tải...</div>
          ) : (
            <div className="admin-grid">
              <div className="col-12 lg-3"><StatCard title="Tổng người dùng" value={summary?.totalUsers ?? summary?.TotalUsers ?? 0} caption="Active/Total" /></div>
              <div className="col-12 lg-3"><StatCard title="Đơn thuê" value={summary?.totalBookings ?? summary?.TotalBookings ?? 0} /></div>
              <div className="col-12 lg-3"><StatCard title="Doanh thu (năm)" value={(summary?.yearlyRevenue ?? summary?.YearlyRevenue ?? 0).toLocaleString()} /></div>
              <div className="col-12 lg-3"><StatCard title="Số trạm" value={summary?.totalStations ?? summary?.TotalStations ?? 0} /></div>

              <div className="col-12 lg-8">
                <Card className="admin-card">
                  <CardHeader title="Doanh thu theo tháng" />
                  <CardContent>
                    <LineChart height={280} series={[{ data: revenueSeries.values, label: 'Doanh thu' }]} xAxis={[{ scaleType: 'point', data: revenueSeries.months.map(m => `T${m}`) }]} />
                  </CardContent>
                </Card>
              </div>

              <div className="col-12 lg-4">
                <Card className="admin-card">
                  <CardHeader title="Top xe sử dụng" />
                  <CardContent>
                    <BarChart height={280} yAxis={[{ scaleType: 'band', data: (topVehicles || []).map(v => v.modelName || v.ModelName || v.name || v.Name) }]} series={[{ data: (topVehicles || []).map(v => Number(v.usageCount ?? v.UsageCount ?? v.count ?? 0)), label: 'Lượt thuê' }]} layout="horizontal" />
                  </CardContent>
                </Card>
              </div>

              <div className="col-12 lg-6">
                <Card className="admin-card">
                  <CardHeader title="Tăng trưởng người dùng" />
                  <CardContent>
                    <LineChart height={260} series={[{ data: (userGrowth || []).map(u => Number(u.count ?? u.Count ?? 0)), label: 'Users' }]} xAxis={[{ scaleType: 'point', data: (userGrowth || []).map(u => u.label || u.Label || u.month || u.Month) }]} />
                  </CardContent>
                </Card>
              </div>

              <div className="col-12 lg-6">
                <Card className="admin-card">
                  <CardHeader title="Trạng thái trạm" />
                  <CardContent>
                    <PieChart height={260} series={[{ data: ((() => { const items = Array.isArray(stations) ? stations : []; const on = items.filter(s => s.isOperational ?? s.IsOperational).length; const off = items.length - on; return [ { id: 0, value: on, label: 'Hoạt động' }, { id: 1, value: off, label: 'Bảo trì' }, ]; })()) }]} />
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
