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
  TextField,
  Chip,
  Divider,
  Stack,
} from '@mui/material'
import * as settlementApi from '../../api/settlement'
import * as bookingApi from '../../api/booking'

export default function SettlementRefundModal({ open, onClose, orderId, onSuccess }) {
  const [loading, setLoading] = useState(false)
  const [loadingData, setLoadingData] = useState(true)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState(false)
  const [settlement, setSettlement] = useState(null)
  const [payment, setPayment] = useState(null)

  // VNPay offline refund form
  const [proofDocument, setProofDocument] = useState(null)
  const [transactionId, setTransactionId] = useState('')
  const [notes, setNotes] = useState('')

  const token = typeof window !== 'undefined' ? localStorage.getItem('auth.token') : ''

  useEffect(() => {
    if (open && orderId) {
      loadData()
    } else {
      resetForm()
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, orderId])

  async function loadData() {
    try {
      setLoadingData(true)
      setError('')

      const [orderRes, settlementRes] = await Promise.allSettled([
        bookingApi.getOrderById(orderId, token),
        settlementApi.getSettlementByOrderId(orderId, token),
      ])

      if (orderRes.status === 'fulfilled') {
        const orderData = orderRes.value.data
        const payments = orderData?.Payments || []
        setPayment(payments.find(p => p.Type === 'Deposit' || p.type === 'Deposit') || payments[0])
      }

      if (settlementRes.status === 'fulfilled') {
        setSettlement(settlementRes.value.data)
      } else {
        // Settlement might not exist yet - that's okay, we'll show a message
        console.warn('Settlement not found for order:', orderId)
        setSettlement(null)
      }
    } catch (err) {
      setError(err.message || 'Không thể tải dữ liệu')
    } finally {
      setLoadingData(false)
    }
  }

  function resetForm() {
    setProofDocument(null)
    setTransactionId('')
    setNotes('')
    setError('')
    setSuccess(false)
  }

  function getRefundStatusBadge(status) {
    const statusMap = {
      Pending: { label: 'Chờ xử lý', color: 'warning' },
      Processing: { label: 'Đang xử lý', color: 'info' },
      Processed: { label: 'Đã hoàn tiền', color: 'success' },
      Failed: { label: 'Thất bại', color: 'error' },
      AwaitingManualProof: { label: 'Chờ minh chứng', color: 'warning' },
      NotRequired: { label: 'Không cần hoàn', color: 'default' },
    }
    const s = statusMap[status] || { label: status, color: 'default' }
    return <Chip label={s.label} color={s.color} size="small" />
  }

  function getPaymentMethodBadge(method) {
    const methodMap = {
      VNPay: { label: 'VNPay', color: 'primary' },
      PayOS: { label: 'PayOS', color: 'secondary' },
    }
    const m = methodMap[method] || { label: method, color: 'default' }
    return <Chip label={m.label} color={m.color} size="small" />
  }

  async function handleVNPayRefund() {
    if (!proofDocument) {
      setError('Vui lòng upload minh chứng hoàn tiền')
      return
    }

    try {
      setLoading(true)
      setError('')
      setSuccess(false)

      const result = await settlementApi.markRefundAsProcessed(
        orderId,
        proofDocument,
        notes || `Refund processed offline for VNPay payment. Transaction ID: ${transactionId || 'N/A'}`,
        token
      )

      if (result.data) {
        setSuccess(true)
        setTimeout(() => {
          onSuccess?.()
          handleClose()
        }, 1500)
      }
    } catch (err) {
      setError(err.message || 'Không thể ghi nhận hoàn tiền')
    } finally {
      setLoading(false)
    }
  }

  async function handlePayOSRefund() {
    if (!confirm('Bạn có chắc muốn hoàn tiền tự động qua PayOS?')) {
      return
    }

    try {
      setLoading(true)
      setError('')
      setSuccess(false)

      // Use PayOS refund endpoint (automatic via Payout API)
      const result = await settlementApi.refundPayOSDeposit(orderId, token)

      if (result.data?.success) {
        setSuccess(true)
        setTimeout(() => {
          onSuccess?.()
          handleClose()
        }, 2000)
      } else {
        setError(result.data?.message || 'Không thể hoàn tiền tự động')
      }
    } catch (err) {
      setError(err.message || 'Không thể hoàn tiền tự động')
    } finally {
      setLoading(false)
    }
  }

  function handleClose() {
    resetForm()
    onClose()
  }

  const paymentMethod = payment?.PaymentMethod || payment?.paymentMethod || ''
  const isVNPay = paymentMethod.toUpperCase() === 'VNPAY'
  const isPayOS = paymentMethod.toUpperCase() === 'PAYOS'
  const depositRefundAmount = settlement?.DepositRefundAmount || settlement?.depositRefundAmount || 0
  const refundStatus = settlement?.RefundStatus || settlement?.refundStatus || 'Pending'
  const canRefund = refundStatus === 'Pending' || refundStatus === 'AwaitingManualProof'

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="md" fullWidth>
      <DialogTitle>Settlement & Refund - Order #{orderId}</DialogTitle>
      <DialogContent>
        {loadingData ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
            <CircularProgress />
          </Box>
        ) : (
          <Stack spacing={3}>
            {error && (
              <Alert severity="error" onClose={() => setError('')}>
                {error}
              </Alert>
            )}
            {success && (
              <Alert severity="success">
                {isVNPay ? 'Đã ghi nhận hoàn tiền thành công!' : 'Đang xử lý hoàn tiền tự động...'}
              </Alert>
            )}

            {/* Settlement Info */}
            {!settlement ? (
              <Alert severity="info">
                Settlement chưa được tạo cho order này. Settlement sẽ được tạo tự động khi vehicle return được hoàn thành.
              </Alert>
            ) : (
              <>
                <Box>
                  <Typography variant="h6" gutterBottom>
                    Settlement Information
                  </Typography>
                  <Stack spacing={1}>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography variant="body2" color="text.secondary">
                        Deposit to Refund:
                      </Typography>
                      <Typography variant="body1" fontWeight="bold" color="primary">
                        {Number(depositRefundAmount).toLocaleString('vi-VN')} ₫
                      </Typography>
                    </Box>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography variant="body2" color="text.secondary">
                        Payment Method:
                      </Typography>
                      {getPaymentMethodBadge(paymentMethod)}
                    </Box>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography variant="body2" color="text.secondary">
                        Refund Status:
                      </Typography>
                      {getRefundStatusBadge(refundStatus)}
                    </Box>
                  </Stack>
                </Box>

                <Divider />
              </>
            )}

            {/* Refund Actions */}
            {settlement && canRefund && (
              <Box>
                <Typography variant="h6" gutterBottom>
                  Refund Action
                </Typography>

                {isVNPay && (
                  <Stack spacing={2}>
                    <Alert severity="info">
                      VNPay refunds are processed offline. Please upload proof document after refunding manually via VNPay portal.
                    </Alert>

                    <TextField
                      fullWidth
                      type="file"
                      label="Proof Document (Required)"
                      InputLabelProps={{ shrink: true }}
                      inputProps={{ accept: 'image/*,.pdf' }}
                      onChange={(e) => setProofDocument(e.target.files?.[0] || null)}
                      required
                      helperText="Upload screenshot or receipt from VNPay portal"
                    />

                    <TextField
                      fullWidth
                      label="Transaction ID (Optional)"
                      value={transactionId}
                      onChange={(e) => setTransactionId(e.target.value)}
                      placeholder="VNPay transaction ID"
                    />

                    <TextField
                      fullWidth
                      label="Notes"
                      value={notes}
                      onChange={(e) => setNotes(e.target.value)}
                      multiline
                      rows={3}
                      placeholder="Additional notes about the refund..."
                    />

                    <Button
                      variant="contained"
                      color="primary"
                      onClick={handleVNPayRefund}
                      disabled={loading || !proofDocument}
                      fullWidth
                    >
                      {loading ? <CircularProgress size={20} /> : 'Mark as Refunded Offline'}
                    </Button>
                  </Stack>
                )}

                {isPayOS && (
                  <Stack spacing={2}>
                    <Alert severity="info">
                      PayOS refunds are processed automatically via PayOS Payout API.
                    </Alert>

                    <Button
                      variant="contained"
                      color="secondary"
                      onClick={handlePayOSRefund}
                      disabled={loading}
                      fullWidth
                    >
                      {loading ? <CircularProgress size={20} /> : 'Refund Online via PayOS'}
                    </Button>
                  </Stack>
                )}

                {!isVNPay && !isPayOS && (
                  <Alert severity="warning">
                    Unknown payment method: {paymentMethod}. Cannot process refund.
                  </Alert>
                )}
              </Box>
            )}

            {settlement && !canRefund && (
              <Alert severity="info">
                Refund status: {refundStatus}. No action available.
              </Alert>
            )}
          </Stack>
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={handleClose} disabled={loading}>
          Close
        </Button>
      </DialogActions>
    </Dialog>
  )
}

