import { useEffect, useMemo, useState } from 'react'
import { getDashboardSummary, getRevenueByMonth, getTopUsedVehicles, getUserGrowth, getPeakRentalHours, getSystemRentalAnalytics } from '../api/adminDashboard'
import { getAllStations as fetchAllStations } from '../api/station'
import { getAllModels, getVehicleById } from '../api/vehicle'
import { Card, CardContent, CardHeader, Typography, Box, Divider, Tooltip } from '@mui/material'
import { LineChart, BarChart, PieChart } from '@mui/x-charts'
import { Info as InfoIcon } from '@mui/icons-material'
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
  const [userGrowth, setUserGrowth] = useState(null)
  const [peakHours, setPeakHours] = useState(null)
  const [systemAnalytics, setSystemAnalytics] = useState(null)
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)

  const rawUser = (typeof window !== 'undefined' && localStorage.getItem('auth.user')) || '{}'
  let currentRoleId = 0
  try { currentRoleId = Number(JSON.parse(rawUser)?.roleId || JSON.parse(rawUser)?.RoleId || 0) } catch {
    // Ignore parse errors, use default 0
  }
  const forbidden = currentRoleId !== 3

  useEffect(() => {
    let mounted = true
    ;(async () => {
      try {
        setLoading(true)
        const [s, r, v, st, ug, sl, ml, ph, sa] = await Promise.all([
          getDashboardSummary(token).catch(e => { throw e }), // summary is required
          getRevenueByMonth(new Date().getFullYear(), token).catch(() => ({ data: [] })),
          getTopUsedVehicles(10, token).catch(() => ({ data: [] })),
          // Avoid calling stations stats endpoint (currently failing on server); rely on station list instead
          Promise.resolve({ data: null }),
          getUserGrowth(token).catch(() => ({ data: null })),
          fetchAllStations(token).catch(() => ({ data: [] })),
          getAllModels(token).catch(() => ({ data: [] })),
          getPeakRentalHours(token).catch(() => ({ data: null })),
          getSystemRentalAnalytics(token).catch(() => ({ data: null }))
        ])
        if (!mounted) return
        setSummary(s.data || null)
        setRevenue(Array.isArray(r.data) ? r.data : [])
        const topRaw = Array.isArray(v.data) ? v.data : []
        const modelList = Array.isArray(ml.data) ? ml.data : (Array.isArray(ml.data?.data) ? ml.data.data : [])
        const modelNameById = new Map(modelList.map(m => [Number(m.modelId || m.ModelId), `${m.manufacturer || m.Manufacturer || ''} ${m.modelName || m.ModelName || ''}`.trim()]))
        const vehicleIds = Array.from(new Set(topRaw.map(x => Number(x.vehicleId || x.VehicleId)).filter(id => Number.isFinite(id))))
        const vehicleMap = {}
        await Promise.all(vehicleIds.map(async (vid) => {
          try {
            const res = await getVehicleById(vid, token)
            const vdata = res?.data || res?.data?.data
            if (vdata) vehicleMap[vid] = vdata
          } catch {
            // Ignore individual vehicle fetch errors
          }
        }))
        const enriched = topRaw.map(item => {
          const vid = Number(item.vehicleId || item.VehicleId)
          const veh = vehicleMap[vid]
          const mid = Number(veh?.modelId || veh?.ModelId)
          const name = modelNameById.get(mid) || (veh?.name || veh?.Name) || `Vehicle ${vid}`
          const count = Number(item.totalBookings ?? item.TotalBookings ?? item.count ?? item.Count ?? 0)
          return { ...item, vehicleId: vid, name, count }
        })
        setTopVehicles(enriched)
        setStationStatsState(st.data ?? null)
        setStationList(Array.isArray(sl.data) ? sl.data : [])
        setUserGrowth(ug.data || null)
        setPeakHours(ph.data ?? null)
        setSystemAnalytics(sa.data ?? null)
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

  // Peak Hours Chart Data - tính phần trăm và tách thành 3 series
  const peakHoursChartData = useMemo(() => {
    if (!peakHours || !peakHours.hourlyStats || !Array.isArray(peakHours.hourlyStats)) {
      return null
    }
    
    const hourlyStats = peakHours.hourlyStats || []
    const top3Peak = (peakHours.top3PeakHours || peakHours.Top3PeakHours || []).map(h => Number(h))
    const top3Low = (peakHours.top3LowHours || peakHours.Top3LowHours || []).map(h => Number(h))
    
    // Tính tổng số rentals
    const totalRentals = hourlyStats.reduce((sum, stat) => {
      const count = Number(stat.rentalCount ?? stat.RentalCount ?? 0)
      return sum + count
    }, 0)
    
    if (totalRentals === 0) return null
    
    // Tạo data cho 24 giờ (0-23)
    const hours = Array.from({ length: 24 }, (_, i) => i)
    const hourLabels = hours.map(h => `${h}:00`)
    
    // Tạo 3 series riêng: peak (xanh), low (đỏ), normal (xám)
    const peakData = hours.map(hour => {
      if (!top3Peak.includes(hour)) return null
      const stat = hourlyStats.find(s => Number(s.hour ?? s.Hour) === hour)
      const count = stat ? Number(stat.rentalCount ?? stat.RentalCount ?? 0) : 0
      const percentage = totalRentals > 0 ? (count / totalRentals) * 100 : 0
      return Number(percentage.toFixed(2))
    })
    
    const lowData = hours.map(hour => {
      if (!top3Low.includes(hour)) return null
      const stat = hourlyStats.find(s => Number(s.hour ?? s.Hour) === hour)
      const count = stat ? Number(stat.rentalCount ?? stat.RentalCount ?? 0) : 0
      const percentage = totalRentals > 0 ? (count / totalRentals) * 100 : 0
      return Number(percentage.toFixed(2))
    })
    
    const normalData = hours.map(hour => {
      if (top3Peak.includes(hour) || top3Low.includes(hour)) return null
      const stat = hourlyStats.find(s => Number(s.hour ?? s.Hour) === hour)
      const count = stat ? Number(stat.rentalCount ?? stat.RentalCount ?? 0) : 0
      const percentage = totalRentals > 0 ? (count / totalRentals) * 100 : 0
      return Number(percentage.toFixed(2))
    })
    
    return {
      hours: hourLabels,
      series: [
        {
          data: peakData,
          label: 'Peak Hours',
          color: '#4caf50' // xanh lá
        },
        {
          data: lowData,
          label: 'Low Hours',
          color: '#f44336' // đỏ
        },
        {
          data: normalData,
          label: 'Normal Hours',
          color: '#9e9e9e' // xám
        }
      ],
      top3Peak,
      top3Low
    }
  }, [peakHours])

  // System Analytics - Rentals by Day of Week
  const rentalsByDayChart = useMemo(() => {
    if (!systemAnalytics || !systemAnalytics.rentalsByDayOfWeek) return null
    
    const dayMap = systemAnalytics.rentalsByDayOfWeek || {}
    const dayOrder = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday']
    const dayLabels = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun']
    
    const data = dayOrder.map(day => {
      const count = Number(dayMap[day] ?? dayMap[day.toLowerCase()] ?? 0)
      return count
    })
    
    return {
      labels: dayLabels,
      data
    }
  }, [systemAnalytics])

  // System Analytics - Most Rented Vehicles
  const mostRentedVehiclesChart = useMemo(() => {
    if (!systemAnalytics || !systemAnalytics.mostRentedVehicles || !Array.isArray(systemAnalytics.mostRentedVehicles)) {
      return null
    }
    
    const vehicles = systemAnalytics.mostRentedVehicles || []
    const sorted = [...vehicles].sort((a, b) => {
      const countA = Number(a.rentalCount ?? a.RentalCount ?? 0)
      const countB = Number(b.rentalCount ?? b.RentalCount ?? 0)
      return countB - countA
    }).slice(0, 10) // Top 10
    
    return {
      labels: sorted.map(v => v.vehicleName ?? v.VehicleName ?? `Vehicle ${v.vehicleId ?? v.VehicleId}`),
      data: sorted.map(v => Number(v.rentalCount ?? v.RentalCount ?? 0)),
      revenue: sorted.map(v => Number(v.totalRevenue ?? v.TotalRevenue ?? 0))
    }
  }, [systemAnalytics])

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
              <div className="col-12 lg-3">
                <StatCard 
                  title={
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                      Total Users
                      <Tooltip title="Total number of registered users in the system">
                        <InfoIcon sx={{ fontSize: 16, color: 'text.secondary', cursor: 'help' }} />
                      </Tooltip>
                    </Box>
                  }
                  value={summary?.totalUsers ?? summary?.TotalUsers ?? 0} 
                  caption="Active/Total" 
                />
              </div>
              <div className="col-12 lg-3">
                <StatCard 
                  title={
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                      Bookings
                      <Tooltip title="Total number of bookings/rentals">
                        <InfoIcon sx={{ fontSize: 16, color: 'text.secondary', cursor: 'help' }} />
                      </Tooltip>
                    </Box>
                  }
                  value={summary?.totalBookings ?? summary?.TotalBookings ?? 0} 
                />
              </div>
              <div className="col-12 lg-3">
                <StatCard 
                  title={
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                      Revenue (Year)
                      <Tooltip title="Total revenue for the current year in VND">
                        <InfoIcon sx={{ fontSize: 16, color: 'text.secondary', cursor: 'help' }} />
                      </Tooltip>
                    </Box>
                  }
                  value={(summary?.yearlyRevenue ?? summary?.YearlyRevenue ?? 0).toLocaleString()} 
                />
              </div>
              <div className="col-12 lg-3">
                <StatCard 
                  title={
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                      Total Stations
                      <Tooltip title="Number of charging stations in the system">
                        <InfoIcon sx={{ fontSize: 16, color: 'text.secondary', cursor: 'help' }} />
                      </Tooltip>
                    </Box>
                  }
                  value={summary?.totalStations ?? summary?.TotalStations ?? 0} 
                />
              </div>

              <div className="col-12 lg-8">
                <Card className="admin-card">
                  <CardHeader title="Revenue by Month" />
                  <CardContent>
                    {revenueSeries && revenueSeries.values.length > 0 ? (
                      <LineChart
                        height={280}
                        series={[{ data: revenueSeries.values.filter(v => v !== null && v !== undefined), label: 'Revenue' }]}
                        xAxis={[{ scaleType: 'point', data: revenueSeries.months.map((m) => `T${m}`).filter(label => label !== null && label !== undefined) }]}
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
                        yAxis={[{ scaleType: 'band', data: (topVehicles || []).map((v, idx) => (v.name || v.Name || v.modelName || v.ModelName || `Vehicle ${idx + 1}`)).filter(name => name !== null && name !== undefined) }]}
                        series={[{ data: (topVehicles || []).map(v => Number(v.count ?? v.usageCount ?? v.TotalBookings ?? 0)), label: 'Rentals' }]}
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
                  <CardHeader title="User Growth Statistics" />
                  <CardContent>
                    {userGrowth ? (
                      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                        <Box>
                          <Typography variant="body2" color="text.secondary">Total Users</Typography>
                          <Typography variant="h5" sx={{ fontWeight: 700 }}>
                            {userGrowth.totalUsers ?? userGrowth.TotalUsers ?? 0}
                          </Typography>
                        </Box>
                        <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
                          <Box sx={{ flex: 1, minWidth: '120px' }}>
                            <Typography variant="body2" color="text.secondary">This Month</Typography>
                            <Typography variant="h6" sx={{ fontWeight: 600 }}>
                              {userGrowth.newUsersThisMonth ?? userGrowth.NewUsersThisMonth ?? 0}
                            </Typography>
                          </Box>
                          <Box sx={{ flex: 1, minWidth: '120px' }}>
                            <Typography variant="body2" color="text.secondary">Last Month</Typography>
                            <Typography variant="h6" sx={{ fontWeight: 600 }}>
                              {userGrowth.newUsersLastMonth ?? userGrowth.NewUsersLastMonth ?? 0}
                            </Typography>
                          </Box>
                        </Box>
                        <Box>
                          <Typography variant="body2" color="text.secondary">Growth Rate</Typography>
                          <Typography 
                            variant="h6" 
                            sx={{ 
                              fontWeight: 700,
                              color: (userGrowth.growthRate ?? userGrowth.GrowthRate ?? 0) >= 0 ? '#4caf50' : '#f44336'
                            }}
                          >
                            {((userGrowth.growthRate ?? userGrowth.GrowthRate ?? 0) >= 0 ? '+' : '')}
                            {(userGrowth.growthRate ?? userGrowth.GrowthRate ?? 0).toFixed(1)}%
                          </Typography>
                        </Box>
                      </Box>
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

              {/* Peak Hours Chart */}
              <div className="col-12">
                <Card className="admin-card">
                  <CardHeader title="Peak Rental Hours (24-hour breakdown)" />
                  <CardContent>
                    {peakHoursChartData ? (
                      <Box>
                        <BarChart
                          height={300}
                          series={peakHoursChartData.series}
                          xAxis={[{
                            scaleType: 'band',
                            data: peakHoursChartData.hours,
                            label: 'Hour'
                          }]}
                          yAxis={[{
                            label: 'Percentage (%)'
                          }]}
                        />
                        <Box sx={{ mt: 2, display: 'flex', gap: 2, flexWrap: 'wrap', justifyContent: 'center' }}>
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                            <Box sx={{ width: 20, height: 20, bgcolor: '#4caf50', borderRadius: 1 }} />
                            <Typography variant="body2">Top 3 Peak Hours: {peakHoursChartData.top3Peak.map(h => `${h}:00`).join(', ')}</Typography>
                          </Box>
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                            <Box sx={{ width: 20, height: 20, bgcolor: '#f44336', borderRadius: 1 }} />
                            <Typography variant="body2">Top 3 Low Hours: {peakHoursChartData.top3Low.map(h => `${h}:00`).join(', ')}</Typography>
                          </Box>
                        </Box>
                      </Box>
                    ) : (
                      <div className="empty-placeholder">No peak hours data available</div>
                    )}
                  </CardContent>
                </Card>
              </div>

              {/* System-Wide Analytics Section */}
              {systemAnalytics && (
                <>
                  <div className="col-12">
                    <Divider sx={{ my: 3 }} />
                    <Typography variant="h5" sx={{ mb: 2, fontWeight: 'bold' }}>System-Wide Rental Analytics</Typography>
                  </div>

                  {/* System-Wide Stat Cards */}
                  <div className="col-12 lg-3">
                    <StatCard 
                      title="Total Rentals" 
                      value={systemAnalytics.totalRentals ?? systemAnalytics.TotalRentals ?? 0} 
                    />
                  </div>
                  <div className="col-12 lg-3">
                    <StatCard 
                      title="Completed Rentals" 
                      value={systemAnalytics.completedRentals ?? systemAnalytics.CompletedRentals ?? 0} 
                    />
                  </div>
                  <div className="col-12 lg-3">
                    <StatCard 
                      title="Active Rentals" 
                      value={systemAnalytics.activeRentals ?? systemAnalytics.ActiveRentals ?? 0} 
                    />
                  </div>
                  <div className="col-12 lg-3">
                    <StatCard 
                      title="Total Revenue" 
                      value={(systemAnalytics.totalRevenue ?? systemAnalytics.TotalRevenue ?? 0).toLocaleString('vi-VN')} 
                      caption="VND"
                    />
                  </div>

                  <div className="col-12 lg-3">
                    <StatCard 
                      title="Avg Rental Value" 
                      value={(systemAnalytics.averageRentalValue ?? systemAnalytics.AverageRentalValue ?? 0).toLocaleString('vi-VN')} 
                      caption="VND"
                    />
                  </div>
                  <div className="col-12 lg-3">
                    <StatCard 
                      title="Avg Rental Duration" 
                      value={`${(systemAnalytics.averageRentalDurationHours ?? systemAnalytics.AverageRentalDurationHours ?? 0).toFixed(1)}h`} 
                    />
                  </div>
                  <div className="col-12 lg-3">
                    <StatCard 
                      title="On-Time Return Rate" 
                      value={`${(systemAnalytics.onTimeReturnRate ?? systemAnalytics.OnTimeReturnRate ?? 0).toFixed(1)}%`} 
                    />
                  </div>
                  <div className="col-12 lg-3">
                    <StatCard 
                      title="Avg Trust Score" 
                      value={(systemAnalytics.averageTrustScore ?? systemAnalytics.AverageTrustScore ?? 0).toFixed(1)} 
                    />
                  </div>

                  {/* Additional Metrics */}
                  <div className="col-12 lg-6">
                    <Card className="admin-card">
                      <CardHeader title="Performance Metrics" />
                      <CardContent>
                        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                          <Box>
                            <Typography variant="body2" color="text.secondary">Total Late Returns</Typography>
                            <Typography variant="h6">{systemAnalytics.totalLateReturns ?? systemAnalytics.TotalLateReturns ?? 0}</Typography>
                          </Box>
                          <Box>
                            <Typography variant="body2" color="text.secondary">Total Damage Reports</Typography>
                            <Typography variant="h6">{systemAnalytics.totalDamageReports ?? systemAnalytics.TotalDamageReports ?? 0}</Typography>
                          </Box>
                          <Box>
                            <Typography variant="body2" color="text.secondary">Peak Rental Hours</Typography>
                            <Typography variant="h6">
                              {(systemAnalytics.peakRentalHours ?? systemAnalytics.PeakRentalHours ?? []).map((h, idx) => (
                                <span key={idx}>{idx > 0 && ', '}{h}:00</span>
                              ))}
                            </Typography>
                          </Box>
                        </Box>
                      </CardContent>
                    </Card>
                  </div>

                  {/* Rentals by Day of Week */}
                  <div className="col-12 lg-6">
                    <Card className="admin-card">
                      <CardHeader title="Rentals by Day of Week" />
                      <CardContent>
                        {rentalsByDayChart ? (
                          <BarChart
                            height={280}
                            series={[{ data: rentalsByDayChart.data, label: 'Rentals' }]}
                            xAxis={[{ scaleType: 'band', data: rentalsByDayChart.labels }]}
                          />
                        ) : (
                          <div className="empty-placeholder">No data</div>
                        )}
                      </CardContent>
                    </Card>
                  </div>

                  {/* Most Rented Vehicles from System Analytics */}
                  <div className="col-12">
                    <Card className="admin-card">
                      <CardHeader title="Most Rented Vehicles (System-Wide)" />
                      <CardContent>
                        {mostRentedVehiclesChart && mostRentedVehiclesChart.data.length > 0 ? (
                          <BarChart
                            height={300}
                            yAxis={[{ scaleType: 'band', data: mostRentedVehiclesChart.labels }]}
                            series={[{ data: mostRentedVehiclesChart.data, label: 'Rental Count' }]}
                            layout="horizontal"
                          />
                        ) : (
                          <div className="empty-placeholder">No data</div>
                        )}
                      </CardContent>
                    </Card>
                  </div>
                </>
              )}
            </div>
          )}
        </div>
      </section>
    </AdminLayout>
  )
}
