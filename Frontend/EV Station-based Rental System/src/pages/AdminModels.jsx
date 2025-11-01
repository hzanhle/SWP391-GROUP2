import { useEffect, useState } from 'react'
import { getAllModels, createModel, updateModel, deleteModel } from '../api/vehicle'
import { 
  Box, Container, Card, CardContent, CardHeader, Typography, Button, TextField, Dialog, 
  DialogTitle, DialogContent, DialogActions, Alert, CircularProgress, Stack, Grid, Paper,
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow, IconButton, Chip
} from '@mui/material'
import { Edit as EditIcon, Delete as DeleteIcon, Add as AddIcon } from '@mui/icons-material'
import AdminLayout from '../components/admin/AdminLayout'

function ModelForm({ initial, onSubmit, onCancel }) {
  const [form, setForm] = useState(() => ({
    modelName: initial?.modelName || '',
    manufacturer: initial?.manufacturer || '',
    year: initial?.year || 2025,
    maxSpeed: initial?.maxSpeed || 60,
    batteryCapacity: initial?.batteryCapacity || 5000,
    chargingTime: initial?.chargingTime || 120,
    batteryRange: initial?.batteryRange || 100,
    vehicleCapacity: initial?.vehicleCapacity || 2,
    modelCost: initial?.modelCost || 10000000,
    rentFeeForHour: initial?.rentFeeForHour || 50000,
    files: [],
  }))

  function updateField(k, v) { setForm(prev => ({ ...prev, [k]: v })) }

  function handleFiles(e) {
    updateField('files', Array.from(e.target.files || []))
  }

  async function submit(e) {
    e.preventDefault()
    await onSubmit(form)
  }

  return (
    <Box component="form" onSubmit={submit} sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
      <Grid container spacing={2} sx={{ width: '100%' }}>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField fullWidth label="Tên mẫu" value={form.modelName} onChange={e=>updateField('modelName', e.target.value)} required />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField fullWidth label="Hãng" value={form.manufacturer} onChange={e=>updateField('manufacturer', e.target.value)} required />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField fullWidth type="number" label="Năm" value={form.year} onChange={e=>updateField('year', Number(e.target.value))} required />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField fullWidth type="number" label="Tốc độ tối đa (km/h)" value={form.maxSpeed} onChange={e=>updateField('maxSpeed', Number(e.target.value))} required />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField fullWidth type="number" label="Dung lượng pin (mAh)" value={form.batteryCapacity} onChange={e=>updateField('batteryCapacity', Number(e.target.value))} required />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField fullWidth type="number" label="Thời gian sạc (phút)" value={form.chargingTime} onChange={e=>updateField('chargingTime', Number(e.target.value))} required />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField fullWidth type="number" label="Quãng đường (km)" value={form.batteryRange} onChange={e=>updateField('batteryRange', Number(e.target.value))} required />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField fullWidth type="number" label="Số chỗ" value={form.vehicleCapacity} onChange={e=>updateField('vehicleCapacity', Number(e.target.value))} required />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField fullWidth type="number" label="Giá mẫu (VNĐ)" value={form.modelCost} onChange={e=>updateField('modelCost', Number(e.target.value))} required />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField fullWidth type="number" label="Giá thuê/giờ (VNĐ)" value={form.rentFeeForHour} onChange={e=>updateField('rentFeeForHour', Number(e.target.value))} required />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <TextField fullWidth type="file" onChange={handleFiles} slotProps={{
            htmlInput: { multiple: true }
          }} />
        </Grid>
      </Grid>
      <Box sx={{ display: 'flex', gap: 1, justifyContent: 'flex-end' }}>
        <Button variant="outlined" onClick={onCancel}>Hủy</Button>
        <Button variant="contained" type="submit">Lưu</Button>
      </Box>
    </Box>
  );
}

export default function AdminModels() {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [models, setModels] = useState([])
  const [search, setSearch] = useState('')
  const [editing, setEditing] = useState(null)
  const [openDialog, setOpenDialog] = useState(false)
  const token = typeof window !== 'undefined' ? localStorage.getItem('auth.token') : null

  const filtered = models.filter(m => {
    const q = search.toLowerCase()
    return (m.modelName||'').toLowerCase().includes(q) || (m.manufacturer||'').toLowerCase().includes(q)
  })

  async function load() {
    setLoading(true)
    setError('')
    try {
      const { data } = await getAllModels(token)
      setModels(Array.isArray(data) ? data : [])
    } catch (e) {
      setError(e?.message || 'Không tải được danh sách mẫu')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [])

  async function handleDelete(m) {
    if (!confirm('Delete this model?')) return
    try {
      setLoading(true)
      await deleteModel(m.modelId, token)
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
      if (editing && editing.modelId) {
        await updateModel(editing.modelId, form, token)
      } else {
        await createModel(form, token)
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
    <AdminLayout active="models">
      <Box sx={{ py: 3, backgroundColor: '#f5f5f5', minHeight: '100vh' }}>
        <Container maxWidth="lg">
          <Stack spacing={3}>
            {/* Header */}
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Box>
                <Typography variant="h4" component="h1" gutterBottom sx={{ fontWeight: 600 }}>
                  Model Management
                </Typography>
                <Typography variant="body1" color="text.secondary">
                  Add, edit, delete two-wheel vehicle models.
                </Typography>
              </Box>
              <Button 
                variant="contained" 
                startIcon={<AddIcon />}
                onClick={() => { setEditing({}); setOpenDialog(true) }}
              >
                Add Model
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
                  placeholder="Search by model name or brand..."
                  value={search}
                  onChange={e => setSearch(e.target.value)}
                  variant="outlined"
                  size="small"
                />
              </CardContent>
            </Card>

            {/* Models Table */}
            {loading ? (
              <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
                <CircularProgress />
              </Box>
            ) : (
              <TableContainer component={Paper}>
                <Table>
                  <TableHead sx={{ backgroundColor: '#f5f5f5' }}>
                    <TableRow>
                      <TableCell sx={{ fontWeight: 600 }}>Model Name</TableCell>
                      <TableCell sx={{ fontWeight: 600 }}>Brand</TableCell>
                      <TableCell sx={{ fontWeight: 600 }} align="right">Year</TableCell>
                      <TableCell sx={{ fontWeight: 600 }} align="right">Speed (km/h)</TableCell>
                      <TableCell sx={{ fontWeight: 600 }} align="right">Battery (mAh)</TableCell>
                      <TableCell sx={{ fontWeight: 600 }} align="right">Price (VND)</TableCell>
                      <TableCell sx={{ fontWeight: 600 }} align="center">Actions</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {filtered.map(m => (
                      <TableRow key={m.modelId} hover>
                        <TableCell>{m.modelName}</TableCell>
                        <TableCell>{m.manufacturer}</TableCell>
                        <TableCell align="right">{m.year}</TableCell>
                        <TableCell align="right">{m.maxSpeed}</TableCell>
                        <TableCell align="right">{m.batteryCapacity}</TableCell>
                        <TableCell align="right">{m.modelCost?.toLocaleString?.()}</TableCell>
                        <TableCell align="center">
                          <IconButton 
                            size="small" 
                            onClick={() => { setEditing(m); setOpenDialog(true) }}
                            color="primary"
                          >
                            <EditIcon />
                          </IconButton>
                          <IconButton 
                            size="small" 
                            onClick={() => handleDelete(m)}
                            color="error"
                          >
                            <DeleteIcon />
                          </IconButton>
                        </TableCell>
                      </TableRow>
                    ))}
                    {filtered.length === 0 && (
                      <TableRow>
                        <TableCell colSpan={7} align="center" sx={{ py: 3, color: 'text.secondary' }}>
                          No models found
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
                {editing?.modelId ? 'Sửa mẫu xe' : 'Thêm mẫu xe mới'}
              </DialogTitle>
              <DialogContent sx={{ pt: 2 }}>
                <ModelForm 
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
