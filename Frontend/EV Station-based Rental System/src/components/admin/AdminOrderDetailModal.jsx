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

export default function AdminOrderDetailModal({ open, onClose, orderId }) {
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
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle>Order Details - #{orderId}</DialogTitle>
      <DialogContent>
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
              <Grid item xs={6}>
                <Typography variant="subtitle2" color="text.secondary">
                  Order ID
                </Typography>
                <Typography variant="body1" fontWeight="bold">
                  #{order.orderId || order.OrderId}
                </Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="subtitle2" color="text.secondary">
                  User ID
                </Typography>
                <Typography variant="body1">{order.userId || order.UserId}</Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="subtitle2" color="text.secondary">
                  Vehicle ID
                </Typography>
                <Typography variant="body1">{order.vehicleId || order.VehicleId}</Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="subtitle2" color="text.secondary">
                  Total Cost
                </Typography>
                <Typography variant="body1" fontWeight="bold" color="primary">
                  {Number(order.totalCost || order.TotalCost || 0).toLocaleString('vi-VN')} ₫
                </Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="subtitle2" color="text.secondary">
                  Deposit Amount
                </Typography>
                <Typography variant="body1">
                  {Number(order.depositAmount || order.DepositAmount || 0).toLocaleString('vi-VN')} ₫
                </Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="subtitle2" color="text.secondary">
                  Hourly Rate
                </Typography>
                <Typography variant="body1">
                  {Number(order.hourlyRate || order.HourlyRate || 0).toLocaleString('vi-VN')} ₫/h
                </Typography>
              </Grid>
            </Grid>

            <Divider />

            {/* Dates */}
            <Grid container spacing={2}>
              <Grid item xs={6}>
                <Typography variant="subtitle2" color="text.secondary">
                  From Date
                </Typography>
                <Typography variant="body1">{formatDate(order.fromDate || order.FromDate)}</Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="subtitle2" color="text.secondary">
                  To Date
                </Typography>
                <Typography variant="body1">{formatDate(order.toDate || order.ToDate)}</Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="subtitle2" color="text.secondary">
                  Created At
                </Typography>
                <Typography variant="body1">{formatDate(order.createdAt || order.CreatedAt)}</Typography>
              </Grid>
              {order.expiresAt || order.ExpiresAt ? (
                <Grid item xs={6}>
                  <Typography variant="subtitle2" color="text.secondary">
                    Expires At
                  </Typography>
                  <Typography variant="body1">{formatDate(order.expiresAt || order.ExpiresAt)}</Typography>
                </Grid>
              ) : null}
            </Grid>

            {/* Payments */}
            {(order.Payments || order.payments || []).length > 0 && (
              <>
                <Divider />
                <Box>
                  <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                    Payments
                  </Typography>
                  <Stack spacing={1}>
                    {(order.Payments || order.payments || []).map((payment, idx) => (
                      <Box
                        key={idx}
                        sx={{
                          p: 2,
                          border: '1px solid',
                          borderColor: 'divider',
                          borderRadius: 1,
                        }}
                      >
                        <Grid container spacing={2}>
                          <Grid item xs={6}>
                            <Typography variant="caption" color="text.secondary">
                              Payment Method
                            </Typography>
                            <Typography variant="body2" fontWeight="bold">
                              {payment.PaymentMethod || payment.paymentMethod || 'N/A'}
                            </Typography>
                          </Grid>
                          <Grid item xs={6}>
                            <Typography variant="caption" color="text.secondary">
                              Amount
                            </Typography>
                            <Typography variant="body2" fontWeight="bold">
                              {Number(payment.Amount || payment.amount || 0).toLocaleString('vi-VN')} ₫
                            </Typography>
                          </Grid>
                          <Grid item xs={6}>
                            <Typography variant="caption" color="text.secondary">
                              Status
                            </Typography>
                            <Typography variant="body2">
                              {payment.Status || payment.status || 'N/A'}
                            </Typography>
                          </Grid>
                          {payment.TransactionId || payment.transactionId ? (
                            <Grid item xs={6}>
                              <Typography variant="caption" color="text.secondary">
                                Transaction ID
                              </Typography>
                              <Typography variant="body2" sx={{ wordBreak: 'break-all' }}>
                                {payment.TransactionId || payment.transactionId}
                              </Typography>
                            </Grid>
                          ) : null}
                        </Grid>
                      </Box>
                    ))}
                  </Stack>
                </Box>
              </>
            )}
          </Stack>
        ) : null}
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Close</Button>
      </DialogActions>
    </Dialog>
  )
}

