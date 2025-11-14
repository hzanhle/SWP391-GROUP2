import React, { useState, useEffect, useMemo } from 'react'
import Navbar from '../components/Navbar'
import Footer from '../components/Footer'
import CTA from '../components/CTA'
import * as bookingApi from '../api/booking'
import { getVehicleById, getAllModels } from '../api/vehicle'
import { getMyRentalStats } from '../api/adminDashboard'
import { Card, CardContent, CardHeader, Typography, Box, Grid } from '@mui/material'
import { BarChart } from '@mui/x-charts'

export default function History() {
  const [orders, setOrders] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
  const [vehicleModelMap, setVehicleModelMap] = useState({})
  const [stats, setStats] = useState(null)
  const [statsLoading, setStatsLoading] = useState(true)

  useEffect(() => {
    const fetchHistory = async () => {
      try {
        setLoading(true)
        const authUser = localStorage.getItem('auth.user')
        const authToken = localStorage.getItem('auth.token')

        if (!authUser || !authToken) {
          setError('Please log in to view history')
          return
        }

        const user = JSON.parse(authUser)
        const userId = Number(user?.userId || user?.UserId || user?.id || user?.Id)

        if (!userId || isNaN(userId)) {
          setError('Unable to determine user ID')
          return
        }

        // Fetch orders and stats in parallel
        const [ordersRes, statsRes] = await Promise.allSettled([
          bookingApi.getOrdersByUserId(userId, authToken),
          getMyRentalStats(authToken)
        ])

        if (ordersRes.status === 'fulfilled') {
          const ordersList = Array.isArray(ordersRes.value.data) ? ordersRes.value.data : []
          setOrders(ordersList)

          // Build vehicle -> model name mapping
          const vehicleIds = Array.from(new Set(ordersList.map(o => Number(o.vehicleId || o.VehicleId)).filter(id => Number.isFinite(id))))
          let modelList = []
          try {
            const modelsRes = await getAllModels(authToken)
            modelList = Array.isArray(modelsRes.data) ? modelsRes.data : (Array.isArray(modelsRes.data?.data) ? modelsRes.data.data : [])
          } catch {}
          const modelNameById = new Map(modelList.map(m => [Number(m.modelId || m.ModelId), `${m.manufacturer || m.Manufacturer || ''} ${m.modelName || m.ModelName || ''}`.trim()]))
          const vehicleToModel = {}
          await Promise.all(vehicleIds.map(async (vid) => {
            try {
              const res = await getVehicleById(vid, authToken)
              const v = res?.data || res?.data?.data
              const mid = Number(v?.modelId || v?.ModelId)
              if (Number.isFinite(mid)) {
                vehicleToModel[vid] = modelNameById.get(mid) || String(mid)
              }
            } catch {}
          }))
          setVehicleModelMap(vehicleToModel)
        }

        if (statsRes.status === 'fulfilled') {
          setStats(statsRes.value.data)
        } else {
          console.warn('Failed to load rental stats:', statsRes.reason)
        }

        setError(null)
      } catch (err) {
        console.error('Error fetching booking history:', err)
        setError(err.message || 'Unable to load rental history')
      } finally {
        setLoading(false)
        setStatsLoading(false)
      }
    }

    fetchHistory()
  }, [])

  // Process peak hours data for chart
  const peakHoursChartData = useMemo(() => {
    if (!stats?.peakRentalHours || !Array.isArray(stats.peakRentalHours)) {
      return null
    }

    const hours = Array.from({ length: 24 }, (_, i) => i)
    const hourLabels = hours.map(h => `${h}:00`)
    const hourCounts = stats.peakRentalHours || []

    // Find top 3 peak hours
    const hourData = hours.map((hour, idx) => ({
      hour,
      count: hourCounts[idx] || 0
    }))
    const sorted = [...hourData].sort((a, b) => b.count - a.count)
    const top3Peak = sorted.slice(0, 3).map(h => h.hour)
    const top3Low = sorted.slice(-3).map(h => h.hour)

    return {
      hours: hourLabels,
      data: hours.map(h => hourCounts[h] || 0),
      top3Peak,
      top3Low
    }
  }, [stats])

  const getStatusBadgeClass = (status) => {
    const statusMap = {
      'Pending': 'yellow',
      'Confirmed': 'blue',
      'InProgress': 'green',
      'Completed': 'green',
      'Cancelled': 'red',
    }
    return statusMap[status] || 'gray'
  }

  const getActionButton = (order) => {
    const status = order.status || order.Status
    const orderId = order.orderId || order.OrderId

    if (status === 'Pending') {
      return (
        <CTA
          as="button"
          onClick={() => {
            localStorage.removeItem('activeOrder')
            localStorage.setItem('pending_booking', JSON.stringify(order))
            window.location.hash = 'payment'
          }}
          variant="primary"
        >
          Pay Now
        </CTA>
      )
    }

    if (status === 'Confirmed') {
      return (
        <CTA
          as="a"
          href={`#check-in?orderId=${orderId}`}
          variant="primary"
        >
          Check-in
        </CTA>
      )
    }

    if (status === 'InProgress') {
      return (
        <CTA
          as="a"
          href={`#return?orderId=${orderId}`}
          variant="primary"
        >
          Return Vehicle
        </CTA>
      )
    }

    if (status === 'Completed') {
      return (
        <CTA
          as="a"
          href="#feedback"
          variant="primary"
        >
          Rate
        </CTA>
      )
    }

    return (
      <CTA
        as="button"
        onClick={() => {
          const orderId = order.orderId || order.OrderId
          console.log('[History] Viewing order:', orderId)
          localStorage.removeItem('activeOrder')
          localStorage.setItem('pending_booking', JSON.stringify(order))
          window.location.hash = `booking?orderId=${orderId}`
        }}
        variant="secondary"
      >
        View
      </CTA>
    )
  }

  function StatCard({ title, value, caption, color = '#1976d2' }) {
    return (
      <Card className="card" style={{ height: '100%' }}>
        <CardContent>
          <Typography variant="body2" color="text.secondary" gutterBottom>
            {title}
          </Typography>
          <Typography variant="h4" style={{ color, fontWeight: 'bold' }}>
            {value}
          </Typography>
          {caption && (
            <Typography variant="caption" color="text.secondary">
              {caption}
            </Typography>
          )}
        </CardContent>
      </Card>
    )
  }

  return (
    <div data-figma-layer="History Page">
      <Navbar />
      <main>
        <section id="history" className="section page-offset" aria-labelledby="history-title">
          <div className="container">
            <div className="section-header">
              <h1 id="history-title" className="section-title">Rental History</h1>
              <p className="section-subtitle">Review your rentals and expenses.</p>
            </div>

            {/* Personal Analytics Section */}
            {!statsLoading && stats && (
              <Box sx={{ mb: 4 }}>
                <Typography variant="h5" gutterBottom sx={{ mb: 2, fontWeight: 600 }}>
                  Your Rental Statistics
                </Typography>
                <Grid container spacing={2} sx={{ mb: 3 }}>
                  <Grid item xs={12} sm={6} md={3}>
                    <StatCard
                      title="Total Rentals"
                      value={stats.totalRentals || stats.TotalRentals || 0}
                      caption={`${stats.completedRentals || stats.CompletedRentals || 0} completed`}
                      color="#1976d2"
                    />
                  </Grid>
                  <Grid item xs={12} sm={6} md={3}>
                    <StatCard
                      title="Total Spent"
                      value={`${Number(stats.totalSpent || stats.TotalSpent || 0).toLocaleString('vi-VN')} ₫`}
                      caption={`Avg: ${Number(stats.averageRentalCost || stats.AverageRentalCost || 0).toLocaleString('vi-VN')} ₫`}
                      color="#2e7d32"
                    />
                  </Grid>
                  <Grid item xs={12} sm={6} md={3}>
                    <StatCard
                      title="Trust Score"
                      value={stats.currentTrustScore || stats.CurrentTrustScore || 0}
                      caption="Your reliability rating"
                      color="#ed6c02"
                    />
                  </Grid>
                  <Grid item xs={12} sm={6} md={3}>
                    <StatCard
                      title="On-Time Return"
                      value={`${Number(stats.onTimeReturnRate || stats.OnTimeReturnRate || 0).toFixed(1)}%`}
                      caption={`${stats.lateReturns || stats.LateReturns || 0} late returns`}
                      color={Number(stats.onTimeReturnRate || stats.OnTimeReturnRate || 0) >= 90 ? "#2e7d32" : "#d32f2f"}
                    />
                  </Grid>
                </Grid>

                {/* Peak Hours Chart */}
                {peakHoursChartData && (
                  <Card className="card" sx={{ mb: 3 }}>
                    <CardHeader title="Your Peak Rental Hours" />
                    <CardContent>
                      <Box sx={{ width: '100%', height: 300 }}>
                        <BarChart
                          xAxis={[{ scaleType: 'band', data: peakHoursChartData.hours }]}
                          series={[{
                            data: peakHoursChartData.data,
                            label: 'Number of Rentals',
                            color: '#1976d2'
                          }]}
                          width={undefined}
                          height={300}
                        />
                      </Box>
                      <Box sx={{ mt: 2 }}>
                        <Typography variant="body2" color="text.secondary">
                          <strong>Top 3 Peak Hours:</strong> {peakHoursChartData.top3Peak.map(h => `${h}:00`).join(', ')}
                        </Typography>
                        <Typography variant="body2" color="text.secondary">
                          <strong>Top 3 Low Hours:</strong> {peakHoursChartData.top3Low.map(h => `${h}:00`).join(', ')}
                        </Typography>
                      </Box>
                    </CardContent>
                  </Card>
                )}

                {/* Additional Stats */}
                <Grid container spacing={2}>
                  <Grid item xs={12} md={6}>
                    <Card className="card">
                      <CardHeader title="Rental Behavior" />
                      <CardContent>
                        <Typography variant="body2" gutterBottom>
                          <strong>Average Duration:</strong> {Number(stats.averageRentalDurationHours || stats.AverageRentalDurationHours || 0).toFixed(1)} hours
                        </Typography>
                        <Typography variant="body2" gutterBottom>
                          <strong>Rentals with Damage:</strong> {stats.rentalsWithDamage || stats.RentalsWithDamage || 0}
                        </Typography>
                        {stats.mostRentedVehicleType && (
                          <Typography variant="body2">
                            <strong>Most Rented Vehicle:</strong> {stats.mostRentedVehicleType || stats.MostRentedVehicleType} ({stats.mostRentedVehicleCount || stats.MostRentedVehicleCount || 0} times)
                          </Typography>
                        )}
                      </CardContent>
                    </Card>
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <Card className="card">
                      <CardHeader title="Summary" />
                      <CardContent>
                        <Typography variant="body2" gutterBottom>
                          <strong>Completed:</strong> {stats.completedRentals || stats.CompletedRentals || 0} rentals
                        </Typography>
                        <Typography variant="body2" gutterBottom>
                          <strong>Cancelled:</strong> {stats.cancelledRentals || stats.CancelledRentals || 0} rentals
                        </Typography>
                        <Typography variant="body2">
                          <strong>Average Cost per Rental:</strong> {Number(stats.averageRentalCost || stats.AverageRentalCost || 0).toLocaleString('vi-VN')} ₫
                        </Typography>
                      </CardContent>
                    </Card>
                  </Grid>
                </Grid>
              </Box>
            )}

            {/* Orders List */}
            {loading && (
              <div className="card">
                <div className="card-body text-center">
                  <p>Loading history...</p>
                </div>
              </div>
            )}

            {error && (
              <div className="card">
                <div className="card-body">
                  <div className="error-message no-margin">
                    <span>{error}</span>
                  </div>
                </div>
              </div>
            )}

            {!loading && !error && orders.length === 0 && (
              <div className="card">
                <div className="card-body text-center">
                  <p className="card-subtext">You have no rental history yet.</p>
                  <CTA as="a" href="#booking-new" className="mt-4">Book Now</CTA>
                </div>
              </div>
            )}

            {!loading && !error && orders.length > 0 && (
              <div className="card">
                <CardHeader title="Rental Orders" />
                <div className="card-body">
                  <ul className="history-list" role="list">
                    {orders.map((order) => (
                      <li key={order.orderId || order.OrderId} className="history-item">
                        <div className="row-between">
                          <div>
                            <h3 className="card-title">#{order.orderId || order.OrderId}</h3>
                            <p className="card-subtext">
                              {new Date(order.fromDate || order.FromDate).toLocaleDateString('vi-VN')} •
                              {vehicleModelMap[Number(order.vehicleId || order.VehicleId)] || order.vehicle?.model || order.Vehicle?.Model || 'Unknown Model'} •
                              {Number(order.totalCost || order.TotalCost || 0).toLocaleString('vi-VN')} ₫
                            </p>
                          </div>
                          <div className="row">
                            <span className={`badge ${getStatusBadgeClass(order.status || order.Status)}`}>
                              {order.status || order.Status || 'Unknown'}
                            </span>
                            {getActionButton(order)}
                          </div>
                        </div>
                      </li>
                    ))}
                  </ul>
                </div>
              </div>
            )}

          </div>
        </section>
      </main>
      <Footer />
    </div>
  )
}
