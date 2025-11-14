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
} from '@mui/material'
import { Visibility as VisibilityIcon } from '@mui/icons-material'
import AdminLayout from '../components/admin/AdminLayout'
import RiskCustomerDetailModal from '../components/admin/RiskCustomerDetailModal'
import * as riskCustomerApi from '../api/riskCustomer'
import '../styles/admin.css'

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

function getTrustScoreBadge(score) {
  if (score >= 80) return <Chip label={score} color="success" size="small" />
  if (score >= 60) return <Chip label={score} color="info" size="small" />
  if (score >= 40) return <Chip label={score} color="warning" size="small" />
  return <Chip label={score} color="error" size="small" />
}

function formatDate(dateStr) {
  if (!dateStr) return 'N/A'
  try {
    return new Date(dateStr).toLocaleString('vi-VN', { dateStyle: 'short', timeStyle: 'short' })
  } catch {
    return String(dateStr)
  }
}

export default function AdminRiskCustomers() {
  const [riskCustomers, setRiskCustomers] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [search, setSearch] = useState('')
  const [riskLevelFilter, setRiskLevelFilter] = useState('')
  const [selectedUserId, setSelectedUserId] = useState(null)
  const [detailModalOpen, setDetailModalOpen] = useState(false)

  const token = typeof window !== 'undefined' ? localStorage.getItem('auth.token') : ''

  useEffect(() => {
    load()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [riskLevelFilter])

  async function load() {
    try {
      setLoading(true)
      setError('')
      const filters = {}
      if (riskLevelFilter) {
        filters.riskLevel = riskLevelFilter
      }
      const { data } = await riskCustomerApi.getRiskCustomers(token, filters)
      const customersList = Array.isArray(data) ? data : []
      setRiskCustomers(customersList)
    } catch (err) {
      setError(err.message || 'Không thể tải danh sách khách hàng rủi ro')
      console.error('[AdminRiskCustomers] Load error:', err)
    } finally {
      setLoading(false)
    }
  }

  const filtered = useMemo(() => {
    let result = riskCustomers
    if (search) {
      const q = search.toLowerCase()
      result = result.filter((c) => {
        const userId = String(c.userId || c.UserId || '')
        const userName = String(c.userName || c.UserName || '').toLowerCase()
        const email = String(c.email || c.Email || '').toLowerCase()
        return userId.includes(q) || userName.includes(q) || email.includes(q)
      })
    }
    return result
  }, [riskCustomers, search])

  function handleViewDetail(userId) {
    setSelectedUserId(userId)
    setDetailModalOpen(true)
  }

  function handleCloseDetail() {
    setDetailModalOpen(false)
    setSelectedUserId(null)
  }

  return (
    <AdminLayout active="risk-customers">
      <Box className="admin-page">
        <Container maxWidth="xl">
          <Stack className="admin-stack" spacing={3}>
            {/* Header */}
            <Box className="admin-header">
              <Box>
                <Typography variant="h4" component="h1" gutterBottom className="font-600">
                  Risk Customer Management
                </Typography>
                <Typography variant="body1" color="text.secondary">
                  View and manage customers with high risk scores based on trust score, violations, and rental history.
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
                <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
                  <TextField
                    fullWidth
                    label="Search Customers"
                    placeholder="Search by User ID, Name, or Email..."
                    value={search}
                    onChange={(e) => setSearch(e.target.value)}
                    variant="outlined"
                  />
                  <FormControl sx={{ minWidth: 200 }}>
                    <InputLabel>Risk Level</InputLabel>
                    <Select
                      value={riskLevelFilter}
                      label="Risk Level"
                      onChange={(e) => setRiskLevelFilter(e.target.value)}
                    >
                      <MenuItem value="">All Levels</MenuItem>
                      <MenuItem value="Critical">Critical</MenuItem>
                      <MenuItem value="High">High</MenuItem>
                      <MenuItem value="Medium">Medium</MenuItem>
                      <MenuItem value="Low">Low</MenuItem>
                    </Select>
                  </FormControl>
                </Stack>
              </CardContent>
            </Card>

            {/* Risk Customers Table */}
            <Card className="admin-card">
              <CardHeader title={`Risk Customers (${filtered.length})`} />
              <CardContent>
                {loading ? (
                  <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
                    <CircularProgress />
                  </Box>
                ) : filtered.length === 0 ? (
                  <Box sx={{ textAlign: 'center', p: 3 }}>
                    <Typography variant="body2" color="text.secondary">
                      {search || riskLevelFilter
                        ? 'No risk customers found matching your filters.'
                        : 'No risk customers found.'}
                    </Typography>
                  </Box>
                ) : (
                  <TableContainer sx={{ overflowX: 'auto' }}>
                    <Table stickyHeader>
                      <TableHead>
                        <TableRow>
                          <TableCell>User ID</TableCell>
                          <TableCell>Name</TableCell>
                          <TableCell>Email</TableCell>
                          <TableCell>Trust Score</TableCell>
                          <TableCell>Risk Score</TableCell>
                          <TableCell>Risk Level</TableCell>
                          <TableCell>Total Orders</TableCell>
                          <TableCell>Violations</TableCell>
                          <TableCell>Last Violation</TableCell>
                          <TableCell align="center">Actions</TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {filtered.map((customer) => {
                          const userId = customer.userId || customer.UserId
                          const riskLevel = customer.riskLevel || customer.RiskLevel
                          const riskScore = customer.riskScore || customer.RiskScore
                          const trustScore = customer.trustScore || customer.TrustScore
                          const violations = [
                            customer.lateReturnsCount || customer.LateReturnsCount || 0,
                            customer.damageCount || customer.DamageCount || 0,
                            customer.noShowCount || customer.NoShowCount || 0,
                          ].filter((v) => v > 0)

                          return (
                            <TableRow key={userId} hover>
                              <TableCell>
                                <Typography variant="body2" fontWeight="bold">
                                  #{userId}
                                </Typography>
                              </TableCell>
                              <TableCell>{customer.userName || customer.UserName || 'N/A'}</TableCell>
                              <TableCell>{customer.email || customer.Email || 'N/A'}</TableCell>
                              <TableCell>{getTrustScoreBadge(trustScore)}</TableCell>
                              <TableCell>
                                <Typography
                                  variant="body2"
                                  fontWeight="bold"
                                  color={riskScore >= 70 ? 'error' : riskScore >= 50 ? 'warning' : 'text.primary'}
                                >
                                  {riskScore}
                                </Typography>
                              </TableCell>
                              <TableCell>{getRiskLevelBadge(riskLevel)}</TableCell>
                              <TableCell>
                                <Typography variant="body2">
                                  {customer.totalOrders || customer.TotalOrders || 0}
                                </Typography>
                              </TableCell>
                              <TableCell>
                                <Stack direction="row" spacing={0.5} flexWrap="wrap">
                                  {customer.lateReturnsCount > 0 && (
                                    <Chip label={`${customer.lateReturnsCount} Late`} size="small" color="warning" />
                                  )}
                                  {customer.damageCount > 0 && (
                                    <Chip label={`${customer.damageCount} Damage`} size="small" color="error" />
                                  )}
                                  {customer.noShowCount > 0 && (
                                    <Chip label={`${customer.noShowCount} NoShow`} size="small" color="error" />
                                  )}
                                  {violations.length === 0 && (
                                    <Typography variant="caption" color="text.secondary">
                                      None
                                    </Typography>
                                  )}
                                </Stack>
                              </TableCell>
                              <TableCell>
                                {formatDate(customer.lastViolationDate || customer.LastViolationDate)}
                              </TableCell>
                              <TableCell align="center">
                                <Tooltip title="View Details">
                                  <IconButton size="small" onClick={() => handleViewDetail(userId)}>
                                    <VisibilityIcon fontSize="small" />
                                  </IconButton>
                                </Tooltip>
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

      {/* Risk Customer Detail Modal */}
      {selectedUserId && (
        <RiskCustomerDetailModal
          open={detailModalOpen}
          onClose={handleCloseDetail}
          userId={selectedUserId}
        />
      )}
    </AdminLayout>
  )
}

