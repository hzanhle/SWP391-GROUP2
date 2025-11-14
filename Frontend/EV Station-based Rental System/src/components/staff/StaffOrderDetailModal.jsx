import { useState, useEffect } from 'react'
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Alert,
  CircularProgress,
  Box,
  Typography,
  Chip,
  Divider,
  Stack,
  Grid,
  Paper,
} from '@mui/material'
import * as bookingApi from '../../api/booking'

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

export default function StaffOrderDetailModal({ open, onClose, orderId }) {
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [order, setOrder] = useState(null)

  const token = typeof window !== 'undefined' ? localStorage.getItem('auth.token') : ''

  useEffect(() => {
    if (open && orderId) {
      loadOrder()
    } else {
      setOrder(null)
      setError('')
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, orderId])

  async function loadOrder() {
    try {
      setLoading(true)
      setError('')
      const { data } = await bookingApi.getOrderById(orderId, token)
      setOrder(data)
    } catch (err) {
      setError(err.message || 'Không thể tải thông tin đơn hàng')
    } finally {
      setLoading(false)
    }
  }

  if (!open) return null

  return (
    <Dialog open={open} onClose={onClose} maxWidth="lg" fullWidth>
      <DialogTitle>Order Details - #{orderId}</DialogTitle>
      <DialogContent dividers>
        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
            <CircularProgress />
          </Box>
        ) : error ? (
          <Alert severity="error">{error}</Alert>
        ) : order ? (
          <Stack spacing={3}>
            {/* Status */}
            <Box>
              <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                Status
              </Typography>
              {getStatusBadge(order.status || order.Status)}
            </Box>

            <Divider />

            {/* Order Info */}
            <Grid container spacing={2}>
              <Grid item xs={12} md={6}>
                <Paper elevation={1} sx={{ p: 2 }}>
                  <Typography variant="h6" gutterBottom>Order Information</Typography>
                  <Typography variant="body2"><strong>Order ID:</strong> #{order.orderId || order.OrderId}</Typography>
                  <Typography variant="body2"><strong>Customer ID:</strong> #{order.userId || order.UserId}</Typography>
                  <Typography variant="body2"><strong>Vehicle ID:</strong> #{order.vehicleId || order.VehicleId}</Typography>
                  <Typography variant="body2"><strong>From Date:</strong> {formatDate(order.fromDate || order.FromDate)}</Typography>
                  <Typography variant="body2"><strong>To Date:</strong> {formatDate(order.toDate || order.ToDate)}</Typography>
                  <Typography variant="body2"><strong>Total Cost:</strong> {Number(order.totalCost || order.TotalCost || 0).toLocaleString('vi-VN')} ₫</Typography>
                  <Typography variant="body2"><strong>Deposit Amount:</strong> {Number(order.depositAmount || order.DepositAmount || 0).toLocaleString('vi-VN')} ₫</Typography>
                </Paper>
              </Grid>
              <Grid item xs={12} md={6}>
                <Paper elevation={1} sx={{ p: 2 }}>
                  <Typography variant="h6" gutterBottom>Payment Information</Typography>
                  {order.Payments && Array.isArray(order.Payments) && order.Payments.length > 0 ? (
                    order.Payments.map((payment, idx) => (
                      <Box key={idx} sx={{ mb: 1 }}>
                        <Typography variant="body2"><strong>Payment #{idx + 1}:</strong></Typography>
                        <Typography variant="body2" sx={{ pl: 2 }}>
                          Amount: {Number(payment.amount || payment.Amount || 0).toLocaleString('vi-VN')} ₫
                        </Typography>
                        <Typography variant="body2" sx={{ pl: 2 }}>
                          Method: {payment.paymentMethod || payment.PaymentMethod || 'N/A'}
                        </Typography>
                        <Typography variant="body2" sx={{ pl: 2 }}>
                          Status: {payment.status || payment.Status || 'N/A'}
                        </Typography>
                      </Box>
                    ))
                  ) : (
                    <Typography variant="body2" color="text.secondary">No payment information available</Typography>
                  )}
                </Paper>
              </Grid>
            </Grid>

            <Divider />

            {/* Check-in Section */}
            <Box>
              <Typography variant="h6" gutterBottom>Check-in Information</Typography>
              <Alert severity="info">
                Check-in details will be displayed here when available. This feature requires additional API endpoints.
              </Alert>
            </Box>

            <Divider />

            {/* Return Section */}
            <Box>
              <Typography variant="h6" gutterBottom>Return Information</Typography>
              <Alert severity="info">
                Return details will be displayed here when available. This feature requires additional API endpoints.
              </Alert>
            </Box>
          </Stack>
        ) : (
          <Alert severity="info">No order data available</Alert>
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} disabled={loading}>Close</Button>
      </DialogActions>
    </Dialog>
  )
}

