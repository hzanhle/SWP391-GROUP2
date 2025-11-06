import { useEffect, useState } from 'react'
import { getAllStations, createStation, updateStation, deleteStation } from '../api/station'
import { 
  Box, Container, Card, CardContent, CardHeader, Typography, Button, TextField, Dialog, 
  DialogTitle, DialogContent, DialogActions, Alert, CircularProgress, Stack, Grid, Paper,
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow, IconButton, Chip
} from '@mui/material'
import { Edit as EditIcon, Delete as DeleteIcon, Add as AddIcon } from '@mui/icons-material'
import AdminLayout from '../components/admin/AdminLayout'

function StationForm({ initial, onSubmit, onCancel }) {
  const [form, setForm] = useState(() => ({
    name: initial?.name || '',
    location: initial?.location || '',
    managerId: initial?.managerId || null,
  }))

  function updateField(k, v) { setForm(prev => ({ ...prev, [k]: v })) }

  async function submit(e) {
    e.preventDefault()
    await onSubmit(form)
  }

  return (
    <Box component="form" onSubmit={submit} sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
      <Grid container spacing={2} sx={{ width: '100%' }}>
        <Grid size={{ xs: 12 }}>
          <TextField fullWidth label="Station Name" value={form.name} onChange={e=>updateField('name', e.target.value)} required />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <TextField fullWidth label="Location/Address" value={form.location} onChange={e=>updateField('location', e.target.value)} required multiline rows={3} />
        </Grid>
      </Grid>
      <Box sx={{ display: 'flex', gap: 1, justifyContent: 'flex-end' }}>
        <Button variant="outlined" onClick={onCancel}>Cancel</Button>
        <Button variant="contained" type="submit">Save</Button>
      </Box>
    </Box>
  );
}

export default function AdminStations() {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [stations, setStations] = useState([])
  const [search, setSearch] = useState('')
  const [editing, setEditing] = useState(null)
  const [openDialog, setOpenDialog] = useState(false)
  const token = typeof window !== 'undefined' ? localStorage.getItem('auth.token') : null

  const filtered = stations.filter(s => {
    const q = search.toLowerCase()
    return (s.name||'').toLowerCase().includes(q) || (s.location||'').toLowerCase().includes(q)
  })

  async function load() {
    setLoading(true)
    setError('')
    try {
      const { data } = await getAllStations(token)
      setStations(Array.isArray(data) ? data : [])
    } catch (e) {
      setError(e?.message || 'Failed to load stations')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [])

  async function handleDelete(s) {
    if (!confirm('Delete this station?')) return
    try {
      setLoading(true)
      await deleteStation(s.stationId, token)
      await load()
    } catch (e) {
      alert(e?.message || 'Delete failed')
    } finally {
      setLoading(false)
    }
  }

  async function handleSubmit(form) {
    try {
      setLoading(true)
      if (editing && editing.stationId) {
        await updateStation({ ...form, stationId: editing.stationId }, token)
      } else {
        await createStation(form, token)
      }
      setEditing(null)
      setOpenDialog(false)
      await load()
    } catch (e) {
      alert(e?.message || 'Save failed')
    } finally {
      setLoading(false)
    }
  }

  return (
    <AdminLayout active="stations">
      <Box className="admin-page">
        <Container maxWidth="lg">
          <Stack className="admin-stack" spacing={3}>
            {/* Header */}
            <Box className="admin-header">
              <Box>
                <Typography variant="h4" component="h1" gutterBottom className="font-600">
                  Charging Stations
                </Typography>
                <Typography variant="body1" color="text.secondary">
                  Add, edit, delete charging stations.
                </Typography>
              </Box>
              <Button 
                variant="contained" 
                startIcon={<AddIcon />}
                onClick={() => { setEditing({}); setOpenDialog(true) }}
              >
                Add Station
              </Button>
            </Box>

            {/* Error Alert */}
            {error && (
              <Alert severity="error" onClose={() => setError('')}>
                {error}
              </Alert>
            )}

            {/* Search Box */}
            <Card>
              <CardContent>
                <TextField 
                  fullWidth
                  placeholder="Search by station name or location..."
                  value={search}
                  onChange={e => setSearch(e.target.value)}
                  variant="outlined"
                  size="small"
                />
              </CardContent>
            </Card>

            {/* Stations Table */}
            {loading ? (
              <Box className="center-justify pad-y-4">
                <CircularProgress />
              </Box>
            ) : (
              <TableContainer component={Paper}>
                <Table>
                  <TableHead className="thead-muted">
                    <TableRow>
                      <TableCell className="font-600">Station ID</TableCell>
                      <TableCell className="font-600">Station Name</TableCell>
                      <TableCell className="font-600">Location</TableCell>
                      <TableCell sx={{ fontWeight: 600 }} align="center">Actions</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {filtered.map(s => (
                      <TableRow key={s.stationId} hover>
                        <TableCell>{s.stationId}</TableCell>
                        <TableCell>{s.name}</TableCell>
                        <TableCell>{s.location}</TableCell>
                        <TableCell align="center">
                          <IconButton 
                            size="small" 
                            onClick={() => { setEditing(s); setOpenDialog(true) }}
                            color="primary"
                          >
                            <EditIcon />
                          </IconButton>
                          <IconButton 
                            size="small" 
                            onClick={() => handleDelete(s)}
                            color="error"
                          >
                            <DeleteIcon />
                          </IconButton>
                        </TableCell>
                      </TableRow>
                    ))}
                    {filtered.length === 0 && (
                      <TableRow>
                        <TableCell colSpan={4} align="center" sx={{ py: 3, color: 'text.secondary' }}>
                          No stations found
                        </TableCell>
                      </TableRow>
                    )}
                  </TableBody>
                </Table>
              </TableContainer>
            )}

            {/* Form Dialog */}
            <Dialog open={openDialog} onClose={() => setOpenDialog(false)} maxWidth="sm" fullWidth>
              <DialogTitle>
                {editing?.stationId ? 'Edit Station' : 'Add New Station'}
              </DialogTitle>
              <DialogContent className="pt-2">
                <StationForm 
                  initial={editing} 
                  onSubmit={handleSubmit} 
                  onCancel={() => { setOpenDialog(false); setEditing(null) }}
                />
              </DialogContent>
            </Dialog>
          </Stack>
        </Container>
      </Box>
    </AdminLayout>
  )
}
