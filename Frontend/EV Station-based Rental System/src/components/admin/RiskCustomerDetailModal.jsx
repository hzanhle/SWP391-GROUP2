import React, { useState, useEffect } from 'react'
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Alert,
  CircularProgress,
  Stack,
  Box,
  Typography,
  Divider,
  Chip,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
} from '@mui/material'
import * as riskCustomerApi from '../../api/riskCustomer'

function getRiskLevelBadge(riskLevel) {
  const levelMap = {
    Critical: { label: 'Critical', color: 'error' },
    High: { label: 'High', color: 'warning' },
    Medium: { label: 'Medium', color: 'info' },
    Low: { label: 'Low', color: 'success' },
  }
  const l = levelMap[riskLevel] || { label: riskLevel, color: 'default' }
  return <Chip label={l.label} color={l.color} size="small" />
}

function getViolationTypeBadge(type) {
  const typeMap = {
    LateReturn: { label: 'Late Return', color: 'warning' },
    Damage: { label: 'Damage', color: 'error' },
    NoShow: { label: 'No-Show', color: 'error' },
    Other: { label: 'Other', color: 'default' },
  }
  const t = typeMap[type] || { label: type, color: 'default' }
  return <Chip label={t.label} color={t.color} size="small" />
}

function formatDate(dateStr) {
  if (!dateStr) return 'N/A'
  try {
    return new Date(dateStr).toLocaleString('vi-VN', { dateStyle: 'short', timeStyle: 'short' })
  } catch {
    return String(dateStr)
  }
}

export default function RiskCustomerDetailModal({ open, onClose, userId }) {
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [profile, setProfile] = useState(null)

  const token = typeof window !== 'undefined' ? localStorage.getItem('auth.token') : ''

  useEffect(() => {
    if (open && userId) {
      loadProfile()
    } else {
      reset()
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, userId])

  async function loadProfile() {
    try {
      setLoading(true)
      setError('')
      const { data } = await riskCustomerApi.getUserRiskProfile(userId, token)
      setProfile(data)
    } catch (err) {
      setError(err.message || 'Không thể tải thông tin chi tiết')
      console.error('[RiskCustomerDetailModal] Load error:', err)
    } finally {
      setLoading(false)
    }
  }

  function reset() {
    setProfile(null)
    setError('')
    setLoading(true)
  }

  if (!open) return null

  return (
    <Dialog open={open} onClose={onClose} maxWidth="lg" fullWidth>
      <DialogTitle>Risk Customer Profile - User #{userId}</DialogTitle>
      <DialogContent>
        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
            <CircularProgress />
          </Box>
        ) : error ? (
          <Alert severity="error">{error}</Alert>
        ) : !profile ? (
          <Alert severity="info">No profile data available</Alert>
        ) : (
          <Stack spacing={3}>
            {/* Basic Info */}
            <Box>
              <Typography variant="h6" gutterBottom>
                Basic Information
              </Typography>
              <Stack spacing={1}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Typography variant="body2" color="text.secondary">
                    User ID:
                  </Typography>
                  <Typography variant="body2" fontWeight="bold">
                    #{profile.userId || profile.UserId}
                  </Typography>
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Typography variant="body2" color="text.secondary">
                    Name:
                  </Typography>
                  <Typography variant="body2">{profile.userName || profile.UserName || 'N/A'}</Typography>
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Typography variant="body2" color="text.secondary">
                    Email:
                  </Typography>
                  <Typography variant="body2">{profile.email || profile.Email || 'N/A'}</Typography>
                </Box>
              </Stack>
            </Box>

            <Divider />

            {/* Risk Assessment */}
            <Box>
              <Typography variant="h6" gutterBottom>
                Risk Assessment
              </Typography>
              <Stack spacing={1}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <Typography variant="body2" color="text.secondary">
                    Trust Score:
                  </Typography>
                  <Chip
                    label={profile.trustScore || profile.TrustScore}
                    color={profile.trustScore >= 80 ? 'success' : profile.trustScore >= 60 ? 'info' : profile.trustScore >= 40 ? 'warning' : 'error'}
                    size="small"
                  />
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <Typography variant="body2" color="text.secondary">
                    Risk Score:
                  </Typography>
                  <Typography
                    variant="body1"
                    fontWeight="bold"
                    color={profile.riskScore >= 70 ? 'error' : profile.riskScore >= 50 ? 'warning' : 'text.primary'}
                  >
                    {profile.riskScore || profile.RiskScore}
                  </Typography>
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <Typography variant="body2" color="text.secondary">
                    Risk Level:
                  </Typography>
                  {getRiskLevelBadge(profile.riskLevel || profile.RiskLevel)}
                </Box>
                <Box>
                  <Typography variant="body2" color="text.secondary" gutterBottom>
                    Risk Factors:
                  </Typography>
                  <Stack direction="row" spacing={0.5} flexWrap="wrap">
                    {(profile.riskFactors || profile.RiskFactors || []).map((factor, idx) => (
                      <Chip key={idx} label={factor} size="small" variant="outlined" />
                    ))}
                    {(!profile.riskFactors || profile.riskFactors.length === 0) && (
                      <Typography variant="caption" color="text.secondary">
                        None
                      </Typography>
                    )}
                  </Stack>
                </Box>
              </Stack>
            </Box>

            <Divider />

            {/* Statistics */}
            <Box>
              <Typography variant="h6" gutterBottom>
                Statistics
              </Typography>
              <Stack spacing={1}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Typography variant="body2" color="text.secondary">
                    Total Orders:
                  </Typography>
                  <Typography variant="body2">{profile.totalOrders || profile.TotalOrders || 0}</Typography>
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Typography variant="body2" color="text.secondary">
                    Completed Orders:
                  </Typography>
                  <Typography variant="body2">{profile.completedOrders || profile.CompletedOrders || 0}</Typography>
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Typography variant="body2" color="text.secondary">
                    Cancelled Orders:
                  </Typography>
                  <Typography variant="body2">{profile.cancelledOrders || profile.CancelledOrders || 0}</Typography>
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Typography variant="body2" color="text.secondary">
                    Late Returns:
                  </Typography>
                  <Typography variant="body2">{profile.lateReturnsCount || profile.LateReturnsCount || 0}</Typography>
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Typography variant="body2" color="text.secondary">
                    Damages:
                  </Typography>
                  <Typography variant="body2">{profile.damageCount || profile.DamageCount || 0}</Typography>
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Typography variant="body2" color="text.secondary">
                    Total Damage Amount:
                  </Typography>
                  <Typography variant="body2" fontWeight="bold" color="error">
                    {Number(profile.totalDamageAmount || profile.TotalDamageAmount || 0).toLocaleString('vi-VN')} ₫
                  </Typography>
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Typography variant="body2" color="text.secondary">
                    No-Shows:
                  </Typography>
                  <Typography variant="body2">{profile.noShowCount || profile.NoShowCount || 0}</Typography>
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Typography variant="body2" color="text.secondary">
                    Total Penalties:
                  </Typography>
                  <Typography variant="body2">{profile.penaltyCount || profile.PenaltyCount || 0}</Typography>
                </Box>
              </Stack>
            </Box>

            <Divider />

            {/* Violations */}
            {profile.violations && profile.violations.length > 0 && (
              <Box>
                <Typography variant="h6" gutterBottom>
                  Violation History
                </Typography>
                <TableContainer component={Paper} variant="outlined">
                  <Table size="small">
                    <TableHead>
                      <TableRow>
                        <TableCell>Order ID</TableCell>
                        <TableCell>Type</TableCell>
                        <TableCell>Description</TableCell>
                        <TableCell>Amount</TableCell>
                        <TableCell>Date</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {profile.violations.map((violation, idx) => (
                        <TableRow key={idx}>
                          <TableCell>#{violation.orderId || violation.OrderId}</TableCell>
                          <TableCell>{getViolationTypeBadge(violation.violationType || violation.ViolationType)}</TableCell>
                          <TableCell>{violation.description || violation.Description}</TableCell>
                          <TableCell>
                            {violation.amount
                              ? `${Number(violation.amount).toLocaleString('vi-VN')} ₫`
                              : 'N/A'}
                          </TableCell>
                          <TableCell>{formatDate(violation.violationDate || violation.ViolationDate)}</TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              </Box>
            )}

            {/* Recent Orders with Issues */}
            {profile.recentOrders && profile.recentOrders.length > 0 && (
              <Box>
                <Typography variant="h6" gutterBottom>
                  Recent Orders with Issues
                </Typography>
                <TableContainer component={Paper} variant="outlined">
                  <Table size="small">
                    <TableHead>
                      <TableRow>
                        <TableCell>Order ID</TableCell>
                        <TableCell>Status</TableCell>
                        <TableCell>From Date</TableCell>
                        <TableCell>To Date</TableCell>
                        <TableCell>Issues</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {profile.recentOrders.map((order) => (
                        <TableRow key={order.orderId || order.OrderId}>
                          <TableCell>#{order.orderId || order.OrderId}</TableCell>
                          <TableCell>
                            <Chip
                              label={order.status || order.Status}
                              size="small"
                              color={order.status === 'Completed' ? 'success' : 'default'}
                            />
                          </TableCell>
                          <TableCell>{formatDate(order.fromDate || order.FromDate)}</TableCell>
                          <TableCell>{formatDate(order.toDate || order.ToDate)}</TableCell>
                          <TableCell>
                            <Stack direction="row" spacing={0.5} flexWrap="wrap">
                              {order.hasLateReturn && <Chip label="Late" size="small" color="warning" />}
                              {order.hasDamage && (
                                <Chip
                                  label={`Damage: ${Number(order.damageCharge || order.DamageCharge || 0).toLocaleString('vi-VN')} ₫`}
                                  size="small"
                                  color="error"
                                />
                              )}
                              {!order.hasLateReturn && !order.hasDamage && (
                                <Typography variant="caption" color="text.secondary">
                                  None
                                </Typography>
                              )}
                            </Stack>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              </Box>
            )}

            {/* Recent Score Changes */}
            {profile.recentScoreChanges && profile.recentScoreChanges.length > 0 && (
              <Box>
                <Typography variant="h6" gutterBottom>
                  Recent Trust Score Changes
                </Typography>
                <TableContainer component={Paper} variant="outlined">
                  <Table size="small">
                    <TableHead>
                      <TableRow>
                        <TableCell>Change</TableCell>
                        <TableCell>Type</TableCell>
                        <TableCell>Reason</TableCell>
                        <TableCell>Order ID</TableCell>
                        <TableCell>Date</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {profile.recentScoreChanges.map((change, idx) => (
                        <TableRow key={idx}>
                          <TableCell>
                            <Typography
                              variant="body2"
                              fontWeight="bold"
                              color={change.changeAmount >= 0 ? 'success.main' : 'error.main'}
                            >
                              {change.changeAmount >= 0 ? '+' : ''}
                              {change.changeAmount}
                            </Typography>
                          </TableCell>
                          <TableCell>
                            <Chip
                              label={change.changeType || change.ChangeType}
                              size="small"
                              color={change.changeType === 'Penalty' ? 'error' : 'success'}
                            />
                          </TableCell>
                          <TableCell>{change.reason || change.Reason}</TableCell>
                          <TableCell>
                            {change.orderId ? `#${change.orderId || change.OrderId}` : 'N/A'}
                          </TableCell>
                          <TableCell>{formatDate(change.createdAt || change.CreatedAt)}</TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              </Box>
            )}
          </Stack>
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Close</Button>
      </DialogActions>
    </Dialog>
  )
}

