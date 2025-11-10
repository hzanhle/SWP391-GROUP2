import { useEffect, useState } from 'react'
import { 
  Box, Container, Card, CardContent, CardHeader, Typography, Button, TextField, Dialog,
  DialogTitle, DialogContent, DialogActions, Alert, CircularProgress, Stack, Grid, Paper,
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow, IconButton
} from '@mui/material'
import { Delete as DeleteIcon, Add as AddIcon } from '@mui/icons-material'
import AdminLayout from '../components/admin/AdminLayout'
import * as staffApi from '../api/staffShiff'

export default function AdminStaffShift() {
  const token = typeof window !== 'undefined' ? localStorage.getItem('auth.token') : ''
  const [shifts, setShifts] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const [openDialog, setOpenDialog] = useState(false)
  const [form, setForm] = useState({ userId: '', stationId: '', shiftDate: '', startTime: '', endTime: '' })

  useEffect(() => {
    let mounted = true
    ;(async () => {
      try {
        setLoading(true)
        const res = await staffApi.getAllShifts(token)
        if (!mounted) return
        setShifts(Array.isArray(res.data?.data) ? res.data.data : (Array.isArray(res.data) ? res.data : []))
        setError('')
      } catch (e) {
        setError(e.message || 'Failed to load shifts')
      } finally { 
        if (mounted) setLoading(false) 
      }
    })()
    return () => { mounted = false }
  }, [token])

  async function handleCreate(e) {
    e.preventDefault()
    setSubmitting(true)
    try {
      const payload = {
        userId: Number(form.userId) || undefined,
        stationId: Number(form.stationId) || undefined,
        shiftDate: form.shiftDate,
        startTime: form.startTime,
        endTime: form.endTime,
      }
      const res = await staffApi.createShift(payload, token)
      const created = res.data?.data || res.data
      setShifts(prev => [created, ...prev])
      setForm({ userId: '', stationId: '', shiftDate: '', startTime: '', endTime: '' })
      setOpenDialog(false)
      setError('')
    } catch (e) {
      setError(e.message || 'Failed to create shift')
    } finally { 
      setSubmitting(false) 
    }
  }

  async function handleDelete(id) {
    if (!confirm('Delete this shift?')) return
    try {
      await staffApi.deleteShift(id, token)
      setShifts(prev => prev.filter(s => Number(s.id ?? s.Id) !== Number(id)))
    } catch (e) {
      alert(e.message || 'Delete failed')
    }
  }

  return (
    <AdminLayout active="staffshift">
      <Box sx={{ py: 3, backgroundColor: '#f5f5f5', minHeight: '100vh' }}>
        <Container maxWidth="lg">
          <Stack spacing={3}>
            {/* Header */}
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Box>
                <Typography variant="h4" component="h1" gutterBottom sx={{ fontWeight: 600 }}>
                  Staff Shifts
                </Typography>
                <Typography variant="body1" color="text.secondary">
  Manage shifts - Create / Delete / View
</Typography>
              </Box>
              <Button 
                variant="contained" 
                startIcon={<AddIcon />}
                onClick={() => setOpenDialog(true)}
              >
                Add Shift
              </Button>
            </Box>

            {/* Error Alert */}
            {error && (
              <Alert severity="error" onClose={() => setError('')}>
                {error}
              </Alert>
            )}

            {/* Loading State */}
            {loading ? (
              <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
                <CircularProgress />
              </Box>
            ) : (
              <TableContainer component={Paper}>
                <Table>
                  <TableHead sx={{ backgroundColor: '#f5f5f5' }}>
                    <TableRow>
                      <TableCell sx={{ fontWeight: 600 }}>ID</TableCell>
                      <TableCell sx={{ fontWeight: 600 }}>Staff</TableCell>
                      <TableCell sx={{ fontWeight: 600 }}>Station</TableCell>
                      <TableCell sx={{ fontWeight: 600 }}>Date</TableCell>
                      <TableCell sx={{ fontWeight: 600 }}>Start Time</TableCell>
                      <TableCell sx={{ fontWeight: 600 }}>End Time</TableCell>
                      <TableCell sx={{ fontWeight: 600 }} align="center">Actions</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {(shifts || []).map(s => (
                      <TableRow key={s.id ?? s.Id} hover>
                        <TableCell>{s.id ?? s.Id}</TableCell>
                        <TableCell>{s.userFullName ?? s.UserFullName ?? s.userId ?? s.UserId}</TableCell>
                        <TableCell>{s.station?.name ?? s.Station?.Name ?? s.stationName ?? s.StationName ?? (s.stationId ?? s.StationId)}</TableCell>
                        <TableCell>{(s.shiftDate || s.ShiftDate || '').split('T')[0]}</TableCell>
                        <TableCell>{(s.startTime || s.StartTime) || '-'}</TableCell>
                        <TableCell>{(s.endTime || s.EndTime) || '-'}</TableCell>
                        <TableCell align="center">
                          <IconButton 
                            size="small" 
                            onClick={() => handleDelete(s.id ?? s.Id)}
                            color="error"
                          >
                            <DeleteIcon />
                          </IconButton>
                        </TableCell>
                      </TableRow>
                    ))}
                    {!(shifts || []).length && (
                      <TableRow>
                        <TableCell colSpan={7} align="center" sx={{ py: 3, color: 'text.secondary' }}>
  No shifts
</TableCell>
                      </TableRow>
                    )}
                  </TableBody>
                </Table>
              </TableContainer>
            )}

            {/* Create Shift Dialog */}
            <Dialog open={openDialog} onClose={() => setOpenDialog(false)} maxWidth="sm" fullWidth>
              <DialogTitle>Create New Shift</DialogTitle>
              <DialogContent sx={{ pt: 2 }}>
                <Stack spacing={2}>
                  <TextField
                    fullWidth
                    label="User ID"
                    type="number"
                    value={form.userId}
                    onChange={e => setForm({ ...form, userId: e.target.value })}
                  />
                  <TextField
                    fullWidth
                    label="Station ID"
                    type="number"
                    value={form.stationId}
                    onChange={e => setForm({ ...form, stationId: e.target.value })}
                  />
                  <TextField
                    fullWidth
                    label="Date"
                    type="date"
                    value={form.shiftDate}
                    onChange={e => setForm({ ...form, shiftDate: e.target.value })}
                    slotProps={{
                      inputLabel: { shrink: true }
                    }}
                  />
                  <TextField
                    fullWidth
                    label="Start Time"
                    type="time"
                    value={form.startTime}
                    onChange={e => setForm({ ...form, startTime: e.target.value })}
                    slotProps={{
                      inputLabel: { shrink: true }
                    }}
                  />
                  <TextField
                    fullWidth
                    label="End Time"
                    type="time"
                    value={form.endTime}
                    onChange={e => setForm({ ...form, endTime: e.target.value })}
                    slotProps={{
                      inputLabel: { shrink: true }
                    }}
                  />
                </Stack>
              </DialogContent>
              <DialogActions>
                <Button onClick={() => { setOpenDialog(false); setForm({ userId: '', stationId: '', shiftDate: '', startTime: '', endTime: '' }) }}>
                  Cancel
                </Button>
                <Button onClick={handleCreate} variant="contained" disabled={submitting}>
                  {submitting ? 'Creating...' : 'Create Shift'}
                </Button>
              </DialogActions>
            </Dialog>
          </Stack>
        </Container>
      </Box>
    </AdminLayout>
  );
}
