import { useEffect, useState, useMemo } from 'react'
import {
  Box,
  Container,
  Card,
  CardContent,
  CardHeader,
  Typography,
  Button,
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
} from '@mui/material'
import { Visibility as VisibilityIcon, AccountBalanceWallet as RefundIcon } from '@mui/icons-material'
import AdminLayout from '../components/admin/AdminLayout'
import SettlementRefundModal from '../components/admin/SettlementRefundModal'
import AdminOrderDetailModal from '../components/admin/AdminOrderDetailModal'
import * as bookingApi from '../api/booking'
import '../styles/admin.css'

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

export default function AdminOrders() {
  const [orders, setOrders] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [search, setSearch] = useState('')
  const [selectedOrder, setSelectedOrder] = useState(null)
  const [refundModalOpen, setRefundModalOpen] = useState(false)
  const [detailModalOpen, setDetailModalOpen] = useState(false)
  const [selectedOrderId, setSelectedOrderId] = useState(null)

  const token = typeof window !== 'undefined' ? localStorage.getItem('auth.token') : ''

  useEffect(() => {
    load()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  async function load() {
    try {
      setLoading(true)
      setError('')
      const { data } = await bookingApi.getAllOrders(token)
      const ordersList = Array.isArray(data) ? data : []
      setOrders(ordersList)
      
      // Debug: Log first order to check Payments data
      if (ordersList.length > 0 && import.meta.env.DEV) {
        const firstOrder = ordersList[0]
        console.log('[AdminOrders] First order sample:', {
          orderId: firstOrder.orderId || firstOrder.OrderId,
          status: firstOrder.status || firstOrder.Status,
          hasPayments: !!(firstOrder.Payments || firstOrder.payments),
          payments: firstOrder.Payments || firstOrder.payments,
          paymentsCount: (firstOrder.Payments || firstOrder.payments || []).length
        })
      }
    } catch (err) {
      setError(err.message || 'Không thể tải danh sách đơn hàng')
      console.error('[AdminOrders] Load error:', err)
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
      const status = String(o.status || o.Status || '').toLowerCase()
      return orderId.includes(q) || userId.includes(q) || status.includes(q)
    })
  }, [orders, search])

  function handleViewSettlement(order) {
    setSelectedOrder(order)
    setRefundModalOpen(true)
  }

  function handleViewDetail(order) {
    const orderId = order.orderId || order.OrderId
    setSelectedOrderId(orderId)
    setDetailModalOpen(true)
  }

  function handleRefundSuccess() {
    load() // Reload orders to update refund status
  }

  return (
    <AdminLayout active="orders">
      <Box className="admin-page">
        <Container maxWidth="xl">
          <Stack className="admin-stack" spacing={3}>
            {/* Header */}
            <Box className="admin-header">
              <Box>
                <Typography variant="h4" component="h1" gutterBottom className="font-600">
                  Orders Management
                </Typography>
                <Typography variant="body1" color="text.secondary">
                  View and manage all rental orders. Process refunds for completed orders.
                </Typography>
              </Box>
            </Box>

            {/* Error Alert */}
            {error && (
              <Alert severity="error" onClose={() => setError('')}>
                {error}
              </Alert>
            )}

            {/* Search Box */}
            <Card className="admin-card">
              <CardContent>
                <TextField
                  fullWidth
                  label="Search Orders"
                  placeholder="Search by Order ID, User ID, or Status..."
                  value={search}
                  onChange={(e) => setSearch(e.target.value)}
                  variant="outlined"
                />
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
                ) : filtered.length === 0 ? (
                  <Box sx={{ textAlign: 'center', p: 3 }}>
                    <Typography variant="body2" color="text.secondary">
                      {search ? 'No orders found matching your search.' : 'No orders found.'}
                    </Typography>
                  </Box>
                ) : (
                  <TableContainer sx={{ overflowX: 'auto' }}>
                    <Table stickyHeader>
                      <TableHead>
                        <TableRow>
                          <TableCell>Order ID</TableCell>
                          <TableCell>User ID</TableCell>
                          <TableCell>Status</TableCell>
                          <TableCell>Total Cost</TableCell>
                          <TableCell>From Date</TableCell>
                          <TableCell>To Date</TableCell>
                          <TableCell>Created At</TableCell>
                          <TableCell align="center">Actions</TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {filtered.map((order) => {
                          const orderId = order.orderId || order.OrderId
                          const status = order.status || order.Status
                          const isCompleted = status === 'Completed'
                          const payments = order.Payments || order.payments || []
                          
                          // Find deposit payment: Type can be 0 (enum), "Deposit" (string), or "0"
                          // Also check if Type is undefined/null (might be first payment)
                          const payment = payments.find((p) => {
                            const type = p.Type ?? p.type
                            return type === 'Deposit' || type === 'deposit' || type === 0 || type === '0' || type === null || type === undefined
                          }) || payments[0] // Fallback to first payment if no deposit found
                          
                          // Get payment method and normalize it
                          const rawPaymentMethod = payment?.PaymentMethod || payment?.paymentMethod || ''
                          const paymentMethod = String(rawPaymentMethod).trim()
                          const normalizedMethod = paymentMethod.toLowerCase()
                          
                          // Check if it's VNPay or PayOS (case-insensitive, handle variations)
                          // Also check if paymentMethod contains these strings
                          const isVNPayOrPayOS = paymentMethod && (
                            normalizedMethod === 'vnpay' || 
                            normalizedMethod === 'payos' ||
                            normalizedMethod.startsWith('vnpay') ||
                            normalizedMethod.startsWith('payos')
                          )
                          
                          // Debug: Always log for completed orders to help debug
                          if (isCompleted) {
                            console.log(`[AdminOrders] Order ${orderId}:`, {
                              status,
                              paymentsCount: payments.length,
                              payments: payments,
                              selectedPayment: payment,
                              rawPaymentMethod,
                              paymentMethod,
                              normalizedMethod,
                              isVNPayOrPayOS,
                              shouldShowRefund: isCompleted && isVNPayOrPayOS
                            })
                          }

                          return (
                            <TableRow key={orderId || order.OrderId} hover>
                              <TableCell>
                                <Typography variant="body2" fontWeight="bold">
                                  #{orderId}
                                </Typography>
                              </TableCell>
                              <TableCell>{order.userId || order.UserId}</TableCell>
                              <TableCell>{getStatusBadge(status)}</TableCell>
                              <TableCell>
                                {Number(order.totalCost || order.TotalCost || 0).toLocaleString('vi-VN')} ₫
                              </TableCell>
                              <TableCell>{formatDate(order.fromDate || order.FromDate)}</TableCell>
                              <TableCell>{formatDate(order.toDate || order.ToDate)}</TableCell>
                              <TableCell>{formatDate(order.createdAt || order.CreatedAt)}</TableCell>
                              <TableCell align="center">
                                <Stack direction="row" spacing={1} justifyContent="center">
                                  <Tooltip title="View Details">
                                    <IconButton
                                      size="small"
                                      onClick={() => handleViewDetail(order)}
                                    >
                                      <VisibilityIcon fontSize="small" />
                                    </IconButton>
                                  </Tooltip>
                                  {/* Show refund icon for completed orders with VNPay/PayOS */}
                                  {isCompleted && isVNPayOrPayOS && (
                                    <Tooltip title="Settlement & Refund">
                                      <IconButton
                                        size="small"
                                        color="primary"
                                        onClick={() => handleViewSettlement(order)}
                                      >
                                        <RefundIcon fontSize="small" />
                                      </IconButton>
                                    </Tooltip>
                                  )}
                                  {/* Debug: Show disabled icon for completed orders without VNPay/PayOS */}
                                  {isCompleted && !isVNPayOrPayOS && (
                                    <Tooltip 
                                      title={
                                        payments.length === 0 
                                          ? 'No payments found' 
                                          : !payment 
                                            ? 'Payment not found' 
                                            : paymentMethod 
                                              ? `Payment: "${paymentMethod}" (No refund available for this method)` 
                                              : 'Payment method is empty'
                                      }
                                    >
                                      <span>
                                        <IconButton size="small" disabled>
                                          <RefundIcon fontSize="small" />
                                        </IconButton>
                                      </span>
                                    </Tooltip>
                                  )}
                                </Stack>
                              </TableCell>
                            </TableRow>
                          )
                        })}
                      </TableBody>
                    </Table>
                  </TableContainer>
                )}
              </CardContent>
            </Card>
          </Stack>
        </Container>
      </Box>

      {/* Settlement & Refund Modal */}
      {selectedOrder && (
        <SettlementRefundModal
          open={refundModalOpen}
          onClose={() => {
            setRefundModalOpen(false)
            setSelectedOrder(null)
          }}
          orderId={selectedOrder.orderId || selectedOrder.OrderId}
          onSuccess={handleRefundSuccess}
        />
      )}

      {/* Order Detail Modal */}
      <AdminOrderDetailModal
        open={detailModalOpen}
        onClose={() => {
          setDetailModalOpen(false)
          setSelectedOrderId(null)
        }}
        orderId={selectedOrderId}
      />
    </AdminLayout>
  )
}

