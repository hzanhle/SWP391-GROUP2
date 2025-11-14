import { useEffect, useState, useMemo } from 'react'
import {
  Box,
  Container,
  Card,
  CardContent,
  CardHeader,
  Typography,
  Alert,
  CircularProgress,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  IconButton,
  Chip,
  TextField,
  Tooltip,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Grid,
} from '@mui/material'
import { Visibility as VisibilityIcon } from '@mui/icons-material'
import StaffLayout from '../components/staff/StaffLayout'
import StaffOrderDetailModal from '../components/staff/StaffOrderDetailModal'
import * as bookingApi from '../api/booking'
import * as staffShiftApi from '../api/staffShiff'
import '../styles/staff.css'

function getStatusBadge(status) {
  const statusMap = {
    Pending: { label: 'Pending', color: 'warning' },
    Confirmed: { label: 'Confirmed', color: 'info' },
    InProgress: { label: 'In Progress', color: 'primary' },
    Completed: { label: 'Completed', color: 'success' },
    Cancelled: { label: 'Cancelled', color: 'error' },
  }
  const s = statusMap[status] || { label: status, color: 'default' }
  return <Chip label={s.label} color={s.color} size="small" />
}

function formatDate(dateStr) {
  if (!dateStr) return 'N/A'
  try {
    return new Date(dateStr).toLocaleString('vi-VN')
  } catch {
    return String(dateStr)
  }
}

export default function StaffOrders() {
  const [orders, setOrders] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [search, setSearch] = useState('')
  const [selectedStationId, setSelectedStationId] = useState(null)
  const [stations, setStations] = useState([])
  const [statusFilter, setStatusFilter] = useState('')
  const [detailModalOpen, setDetailModalOpen] = useState(false)
  const [selectedOrderId, setSelectedOrderId] = useState(null)

  const token = typeof window !== 'undefined' ? localStorage.getItem('auth.token') : ''

  useEffect(() => {
    loadStations()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  useEffect(() => {
    if (selectedStationId) {
      loadOrders()
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedStationId, statusFilter])

  async function loadStations() {
    try {
      setLoading(true)
      setError('')
      // Get staff's shifts to extract stations
      const today = new Date()
      const startOfMonth = new Date(today.getFullYear(), today.getMonth(), 1)
      const endOfMonth = new Date(today.getFullYear(), today.getMonth() + 1, 0)
      
      const { data } = await staffShiftApi.getMyShifts(
        startOfMonth.toISOString().split('T')[0],
        endOfMonth.toISOString().split('T')[0],
        token
      )
      
      const shifts = Array.isArray(data) ? data : []
      // Extract unique stations from shifts
      const stationMap = new Map()
      shifts.forEach(shift => {
        const stationId = shift.stationId || shift.StationId
        const stationName = shift.station?.name || shift.Station?.Name || `Station ${stationId}`
        if (stationId && !stationMap.has(stationId)) {
          stationMap.set(stationId, {
            id: stationId,
            name: stationName
          })
        }
      })
      
      const stationsList = Array.from(stationMap.values())
      setStations(stationsList)
      
      // Auto-select first station or today's active shift station
      if (stationsList.length > 0) {
        const todayShift = shifts.find(s => {
          const shiftDate = new Date(s.shiftDate || s.ShiftDate)
          return shiftDate.toDateString() === today.toDateString() && 
                 (s.status === 'CheckedIn' || s.status === 'Scheduled')
        })
        const defaultStationId = todayShift 
          ? (todayShift.stationId || todayShift.StationId)
          : stationsList[0].id
        setSelectedStationId(defaultStationId)
      }
    } catch (err) {
      setError(err.message || 'Không thể tải danh sách trạm')
      console.error('[StaffOrders] Load stations error:', err)
    } finally {
      setLoading(false)
    }
  }

  async function loadOrders() {
    if (!selectedStationId) return
    
    try {
      setLoading(true)
      setError('')
      const filters = {}
      if (statusFilter) filters.status = statusFilter
      
      const { data } = await bookingApi.getOrdersByStation(selectedStationId, filters, token)
      const ordersList = Array.isArray(data) ? data : []
      setOrders(ordersList)
    } catch (err) {
      setError(err.message || 'Không thể tải danh sách đơn hàng')
      console.error('[StaffOrders] Load orders error:', err)
    } finally {
      setLoading(false)
    }
  }

  const filtered = useMemo(() => {
    if (!search) return orders
    const q = search.toLowerCase()
    return orders.filter((o) => {
      const orderId = String(o.orderId || o.OrderId || '')
      const userId = String(o.userId || o.UserId || '')
      const vehicleId = String(o.vehicleId || o.VehicleId || '')
      return orderId.includes(q) || userId.includes(q) || vehicleId.includes(q)
    })
  }, [orders, search])

  function handleViewDetails(orderId) {
    setSelectedOrderId(orderId)
    setDetailModalOpen(true)
  }

  return (
    <StaffLayout active="orders">
      <Box className="admin-page">
        <Container maxWidth="xl">
          <Stack className="admin-stack" spacing={3}>
            {/* Header */}
            <Box className="admin-header">
              <Box>
                <Typography variant="h4" component="h1" gutterBottom className="font-600">
                  Station Orders
                </Typography>
                <Typography variant="body1" color="text.secondary">
                  View and manage orders at your station.
                </Typography>
              </Box>
            </Box>

            {/* Error Alert */}
            {error && (
              <Alert severity="error" onClose={() => setError('')}>
                {error}
              </Alert>
            )}

            {/* Filters */}
            <Card className="admin-card">
              <CardContent>
                <Grid container spacing={2}>
                  <Grid item xs={12} md={4}>
                    <FormControl fullWidth variant="outlined">
                      <InputLabel>Select Station</InputLabel>
                      <Select
                        value={selectedStationId || ''}
                        onChange={(e) => setSelectedStationId(e.target.value)}
                        label="Select Station"
                      >
                        {stations.map(station => (
                          <MenuItem key={station.id} value={station.id}>
                            {station.name}
                          </MenuItem>
                        ))}
                      </Select>
                    </FormControl>
                  </Grid>
                  <Grid item xs={12} md={4}>
                    <TextField
                      fullWidth
                      label="Search Orders"
                      placeholder="Search by Order ID, User ID, Vehicle ID..."
                      value={search}
                      onChange={(e) => setSearch(e.target.value)}
                      variant="outlined"
                    />
                  </Grid>
                  <Grid item xs={12} md={4}>
                    <FormControl fullWidth variant="outlined">
                      <InputLabel>Filter by Status</InputLabel>
                      <Select
                        value={statusFilter}
                        onChange={(e) => setStatusFilter(e.target.value)}
                        label="Filter by Status"
                      >
                        <MenuItem value="">All Status</MenuItem>
                        <MenuItem value="Pending">Pending</MenuItem>
                        <MenuItem value="Confirmed">Confirmed</MenuItem>
                        <MenuItem value="InProgress">In Progress</MenuItem>
                        <MenuItem value="Completed">Completed</MenuItem>
                        <MenuItem value="Cancelled">Cancelled</MenuItem>
                      </Select>
                    </FormControl>
                  </Grid>
                </Grid>
              </CardContent>
            </Card>

            {/* Orders Table */}
            <Card className="admin-card">
              <CardHeader title={`Orders (${filtered.length})`} />
              <CardContent>
                {loading ? (
                  <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
                    <CircularProgress />
                  </Box>
                ) : !selectedStationId ? (
                  <Box sx={{ textAlign: 'center', p: 3 }}>
                    <Typography variant="body2" color="text.secondary">
                      Please select a station to view orders.
                    </Typography>
                  </Box>
                ) : filtered.length === 0 ? (
                  <Box sx={{ textAlign: 'center', p: 3 }}>
                    <Typography variant="body2" color="text.secondary">
                      {search || statusFilter ? 'No orders found matching your criteria.' : 'No orders found for this station.'}
                    </Typography>
                  </Box>
                ) : (
                  <TableContainer sx={{ overflowX: 'auto' }}>
                    <Table stickyHeader>
                      <TableHead>
                        <TableRow>
                          <TableCell>Order ID</TableCell>
                          <TableCell>Customer ID</TableCell>
                          <TableCell>Vehicle ID</TableCell>
                          <TableCell>Status</TableCell>
                          <TableCell>From Date</TableCell>
                          <TableCell>To Date</TableCell>
                          <TableCell>Total Cost</TableCell>
                          <TableCell align="center">Actions</TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {filtered.map((order) => (
                          <TableRow key={order.orderId || order.OrderId} hover>
                            <TableCell>
                              <Typography variant="body2" fontWeight="bold">
                                #{order.orderId || order.OrderId}
                              </Typography>
                            </TableCell>
                            <TableCell>#{order.userId || order.UserId}</TableCell>
                            <TableCell>#{order.vehicleId || order.VehicleId}</TableCell>
                            <TableCell>{getStatusBadge(order.status || order.Status)}</TableCell>
                            <TableCell>{formatDate(order.fromDate || order.FromDate)}</TableCell>
                            <TableCell>{formatDate(order.toDate || order.ToDate)}</TableCell>
                            <TableCell>
                              {Number(order.totalCost || order.TotalCost || 0).toLocaleString('vi-VN')} ₫
                            </TableCell>
                            <TableCell align="center">
                              <Tooltip title="View Details">
                                <IconButton
                                  size="small"
                                  onClick={() => handleViewDetails(order.orderId || order.OrderId)}
                                >
                                  <VisibilityIcon fontSize="small" />
                                </IconButton>
                              </Tooltip>
                            </TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  </TableContainer>
                )}
              </CardContent>
            </Card>
          </Stack>
        </Container>
      </Box>

      {/* Order Detail Modal */}
      <StaffOrderDetailModal
        open={detailModalOpen}
        onClose={() => {
          setDetailModalOpen(false)
          setSelectedOrderId(null)
        }}
        orderId={selectedOrderId}
      />
    </StaffLayout>
  )
}

