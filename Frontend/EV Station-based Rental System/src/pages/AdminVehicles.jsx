import { useEffect, useState } from 'react'
import { getAllVehicles, createVehicle, updateVehicle, deleteVehicle, getAllModels } from '../api/vehicle'
import { getAllStations } from '../api/station'
import { 
  Box, Container, Card, CardContent, CardHeader, Typography, Button, TextField, Dialog, 
  DialogTitle, DialogContent, DialogActions, Alert, CircularProgress, Stack, Grid, Paper,
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow, IconButton, Select, MenuItem, FormControl, InputLabel
} from '@mui/material'
import { Edit as EditIcon, Delete as DeleteIcon, Add as AddIcon } from '@mui/icons-material'
import AdminLayout from '../components/admin/AdminLayout'


function VehicleForm({ initial, onSubmit, onCancel, models, stations }) {
  const [form, setForm] = useState(() => ({
    modelId: initial?.modelId || '',
    stationId: initial?.stationId || '',
    color: initial?.color || '',
    status: initial?.status || 'Available',
    isActive: initial?.isActive !== undefined ? initial.isActive : true,
    licensePlate: initial?.licensePlate || '',
  }))

  // Đồng bộ form khi initial thay đổi (tránh controlled -> uncontrolled)
  useEffect(() => {
    setForm({
      modelId: initial?.modelId ?? '',
      stationId: initial?.stationId ?? '',
      color: initial?.color ?? '',
      status: initial?.status ?? 'Available',
      isActive: initial?.isActive !== undefined ? !!initial.isActive : true,
      licensePlate: initial?.licensePlate ?? '',
    })
  }, [initial])

  function updateField(k, v) { setForm(prev => ({ ...prev, [k]: v })) }

  async function submit(e) {
    e.preventDefault()
    if (!form.modelId) {
      alert('Please select a model')
      return
    }
    if (!form.stationId) {
      alert('Please select a station')
      return
    }  

    // Ép kiểu ID về số cho backend (nếu backend dùng số)
    const payload = {
      ...form,
      modelId: Number(form.modelId),
      stationId: Number(form.stationId),
    }
    await onSubmit(payload)
  }
  
  return (
    <Box component="form" onSubmit={submit} sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
      <Grid container spacing={2} sx={{ width: '100%' }}>
        <Grid size={{ xs: 12 }}>
          <FormControl fullWidth required>
            <InputLabel>Vehicle Model</InputLabel>
            <Select
              value={String(form.modelId ?? '')}    // luôn dùng chuỗi để khớp với MenuItem
              label="Vehicle Model"
              onChange={e => updateField('modelId', String(e.target.value))}
            >
              {models.map(m => {
                const mid = m.modelId ?? m.id ?? m.Id
                return (
                  <MenuItem key={mid} value={String(mid)}>
                    {m.manufacturer} {m.modelName}
                  </MenuItem>
                )
              })}
            </Select>
          </FormControl>
        </Grid>
        <Grid size={{ xs: 12 }}>
          <FormControl fullWidth required>
            <InputLabel>Station</InputLabel>
            <Select
              value={String(form.stationId ?? '')}  // luôn dùng chuỗi để khớp với MenuItem
              label="Station"
              onChange={e => updateField('stationId', String(e.target.value))}
            >
              {stations.map(s => {
                const sid = s.stationId ?? s.id ?? s.Id
                return (
                  <MenuItem key={sid} value={String(sid)}>
                    {s.name}
                  </MenuItem>
                )
              })}
            </Select>
          </FormControl>
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField 
            fullWidth 
            label="License Plate" 
            value={form.licensePlate ?? ''} 
            onChange={e => updateField('licensePlate', e.target.value)} 
            required 
            placeholder="59C2-811.69"
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField 
            fullWidth 
            label="Color" 
            value={form.color ?? ''} 
            onChange={e => updateField('color', e.target.value)} 
            required 
            placeholder="e.g., Red, White, Black"
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <FormControl fullWidth>
            <InputLabel>Status</InputLabel>
            <Select
              value={form.status ?? 'Available'}
              label="Status"
              onChange={e => updateField('status', e.target.value)}
            >
              <MenuItem value="Available">Available</MenuItem>
              <MenuItem value="InMaintenance">In Maintenance</MenuItem>
              <MenuItem value="Rented">Rented</MenuItem>
              <MenuItem value="Damaged">Damaged</MenuItem>
            </Select>
          </FormControl>
        </Grid>
        <Grid size={{ xs: 12 }}>
          <FormControl fullWidth>
            <InputLabel>Active</InputLabel>
            <Select
              value={form.isActive ? 'true' : 'false'}
              label="Active"
              onChange={e => updateField('isActive', e.target.value === 'true')}
            >
              <MenuItem value="true">Active</MenuItem>
              <MenuItem value="false">Inactive</MenuItem>
            </Select>
          </FormControl>
        </Grid>
      </Grid>
      <Box sx={{ display: 'flex', gap: 1, justifyContent: 'flex-end' }}>
        <Button variant="outlined" onClick={onCancel}>Cancel</Button>
        <Button variant="contained" type="submit">Save</Button>
      </Box>
    </Box>
  );
}

export default function AdminVehicles() {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [vehicles, setVehicles] = useState([])
  const [models, setModels] = useState([])
  const [stations, setStations] = useState([])
  const [search, setSearch] = useState('')
  const [editing, setEditing] = useState(null)
  const [openDialog, setOpenDialog] = useState(false)
  const token = typeof window !== 'undefined' ? localStorage.getItem('auth.token') : null

  const getModelName = (modelId) => {
    const m = models.find(x => x.modelId === modelId)
    return m ? `${m.manufacturer} ${m.modelName}` : 'Unknown'
  }

  const filtered = vehicles.filter(v => {
    const q = search.toLowerCase()
    const modelName = getModelName(v.modelId).toLowerCase()
    return modelName.includes(q) || (v.color||'').toLowerCase().includes(q)
  })

  async function load() {
    setLoading(true)
    setError('')
    try {
      const [vehiclesRes, modelsRes, stationsRes ] = await Promise.all([
        getAllVehicles(token),
        getAllModels(token),
        getAllStations(token)
      ])
      setVehicles(Array.isArray(vehiclesRes.data) ? vehiclesRes.data : [])
      setModels(Array.isArray(modelsRes.data) ? modelsRes.data : [])
      setStations(Array.isArray(stationsRes.data) ? stationsRes.data : [])
    } catch (e) {
      setError(e?.message || 'Failed to load vehicles')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [])

  async function handleDelete(v) {
    if (!confirm('Delete this vehicle?')) return
    try {
      setLoading(true)
      await deleteVehicle(v.vehicleId, token)
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
      if (editing && editing.vehicleId) {
        await updateVehicle(editing.vehicleId, form, token)
      } else {
        await createVehicle(form, token)
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
    <AdminLayout active="vehicles">
      <Box className="admin-page">
        <Container maxWidth="lg">
          <Stack className="admin-stack" spacing={3}>
            {/* Header */}
            <Box className="admin-header">
              <Box>
                <Typography variant="h4" component="h1" gutterBottom className="font-600">
                  Vehicles
                </Typography>
                <Typography variant="body1" color="text.secondary">
                  Add, edit, delete vehicles in your fleet.
                </Typography>
              </Box>
              <Button 
                variant="contained" 
                startIcon={<AddIcon />}
                onClick={() => { setEditing({}); setOpenDialog(true) }}
              >
                Add Vehicle
              </Button>
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
                  placeholder="Search by model or color..."
                  value={search}
                  onChange={e => setSearch(e.target.value)}
                  variant="outlined"
                  size="small"
                />
              </CardContent>
            </Card>

            {/* Vehicles Table */}
            {loading ? (
              <Card className="admin-card">
                <CardContent>
                  <div className="skeleton skeleton-bar"></div>
                  {[...Array(6)].map((_, i) => (
                    <div key={i} className="skeleton skeleton-line"></div>
                  ))}
                </CardContent>
              </Card>
            ) : (
              <TableContainer component={Paper}>
                <Table>
                  <TableHead className="thead-muted">
                    <TableRow>
                      <TableCell className="font-600">Vehicle ID</TableCell>
                      <TableCell className="font-600">Model</TableCell>
                      <TableCell className="font-600">Color</TableCell>
                      <TableCell className="font-600">Status</TableCell>
                      <TableCell className="font-600">Active</TableCell>
                      <TableCell sx={{ fontWeight: 600 }} align="center">Actions</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {filtered.map(v => (
                      <TableRow key={v.vehicleId} hover>
                        <TableCell>{v.vehicleId}</TableCell>
                        <TableCell>{getModelName(v.modelId)}</TableCell>
                        <TableCell>{v.color}</TableCell>
                        <TableCell>{v.status}</TableCell>
                        <TableCell>{v.isActive ? '✓' : '✗'}</TableCell>
                        <TableCell align="center">
                          <IconButton 
                            size="small" 
                            onClick={() => { setEditing(v); setOpenDialog(true) }}
                            color="primary"
                          >
                            <EditIcon />
                          </IconButton>
                          <IconButton 
                            size="small" 
                            onClick={() => handleDelete(v)}
                            color="error"
                          >
                            <DeleteIcon />
                          </IconButton>
                        </TableCell>
                      </TableRow>
                    ))}
                    {filtered.length === 0 && (
                      <TableRow>
                        <TableCell colSpan={6} align="center" sx={{ py: 3, color: 'text.secondary' }}>
                          No vehicles found
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
                {editing?.vehicleId ? 'Edit Vehicle' : 'Add New Vehicle'}
              </DialogTitle>
              <DialogContent className="pt-2">
                <VehicleForm 
                  initial={editing} 
                  onSubmit={handleSubmit} 
                  onCancel={() => { setOpenDialog(false); setEditing(null) }}
                  models={models}
                  stations={stations}
                />
              </DialogContent>
            </Dialog>
          </Stack>
        </Container>
      </Box>
    </AdminLayout>
  )
}
