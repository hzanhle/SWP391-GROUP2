import { useEffect, useState, useMemo, useCallback } from 'react'
import { getAllModels, createModel, updateModel, deleteModel } from '../api/vehicle'
import { 
  Box, Container, Card, CardContent, CardHeader, Typography, Button, TextField, Dialog, 
  DialogTitle, DialogContent, DialogActions, Alert, CircularProgress, Stack, Grid, Paper,
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow, IconButton, Chip, Tooltip
} from '@mui/material'
import { Edit as EditIcon, Delete as DeleteIcon, Add as AddIcon } from '@mui/icons-material'
import AdminLayout from '../components/admin/AdminLayout'
import { TableSkeleton } from '../components/admin/SkeletonLoader'
import { EmptyState } from '../components/admin/EmptyState'
import { ConfirmDialog } from '../components/admin/ConfirmDialog'
import { TablePagination } from '../components/admin/TablePagination'
import { highlightText } from '../utils/searchHighlight.jsx'

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
          <TextField fullWidth label="Model Name" value={form.modelName} onChange={e=>updateField('modelName', e.target.value)} required />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField fullWidth label="Brand" value={form.manufacturer} onChange={e=>updateField('manufacturer', e.target.value)} required />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField fullWidth type="number" label="Year" value={form.year} onChange={e=>updateField('year', Number(e.target.value))} required />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField fullWidth type="number" label="Max speed (km/h)" value={form.maxSpeed} onChange={e=>updateField('maxSpeed', Number(e.target.value))} required />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField fullWidth type="number" label="Battery capacity (mAh)" value={form.batteryCapacity} onChange={e=>updateField('batteryCapacity', Number(e.target.value))} required />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField fullWidth type="number" label="Charging time (min)" value={form.chargingTime} onChange={e=>updateField('chargingTime', Number(e.target.value))} required />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField fullWidth type="number" label="Range (km)" value={form.batteryRange} onChange={e=>updateField('batteryRange', Number(e.target.value))} required />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField fullWidth type="number" label="Seats" value={form.vehicleCapacity} onChange={e=>updateField('vehicleCapacity', Number(e.target.value))} required />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField fullWidth type="number" label="Model price (VND)" value={form.modelCost} onChange={e=>updateField('modelCost', Number(e.target.value))} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField fullWidth type="number" label="Rent per hour (VND)" value={form.rentFeeForHour} onChange={e=>updateField('rentFeeForHour', Number(e.target.value))} required />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <TextField fullWidth type="file" onChange={handleFiles} slotProps={{
            htmlInput: { multiple: true }
          }} />
        </Grid>
      </Grid>
      <Box sx={{ display: 'flex', gap: 1, justifyContent: 'flex-end' }}>
        <Button variant="outlined" onClick={onCancel}>Cancel</Button>
        <Button variant="contained" type="submit">Save</Button>
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
  const [deleteConfirm, setDeleteConfirm] = useState(null)
  const [page, setPage] = useState(0)
  const [rowsPerPage, setRowsPerPage] = useState(10)
  const token = typeof window !== 'undefined' ? localStorage.getItem('auth.token') : null

  const filtered = useMemo(() => {
    if (!search) return models
    const q = search.toLowerCase()
    return models.filter(m => 
      (m.modelName||'').toLowerCase().includes(q) || 
      (m.manufacturer||'').toLowerCase().includes(q)
    )
  }, [models, search])

  const paginatedModels = useMemo(() => {
    const start = page * rowsPerPage
    return filtered.slice(start, start + rowsPerPage)
  }, [filtered, page, rowsPerPage])

  const load = useCallback(async () => {
    setLoading(true)
    setError('')
    try {
      const { data } = await getAllModels(token)
      setModels(Array.isArray(data) ? data : [])
    } catch (e) {
      setError(e?.message || 'Failed to load models')
    } finally {
      setLoading(false)
    }
  }, [token])

  useEffect(() => { load() }, [load])

  async function handleDelete(m) {
    setDeleteConfirm(m)
  }

  async function confirmDelete() {
    if (!deleteConfirm) return
    try {
      setLoading(true)
      await deleteModel(deleteConfirm.modelId, token)
      await load()
      setDeleteConfirm(null)
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
      <Box className="admin-page">
        <Container maxWidth="lg">
          <Stack className="admin-stack" spacing={3}>
            {/* Header */}
            <Box className="admin-header">
              <Box>
                <Typography variant="h4" component="h1" gutterBottom className="font-600">
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
            <Card className="admin-card">
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
            {loading && models.length === 0 ? (
              <Card className="admin-card">
                <CardContent>
                  <TableSkeleton rows={6} cols={7} />
                </CardContent>
              </Card>
            ) : (
              <TableContainer 
                component={Paper} 
                className="admin-card" 
                sx={{ 
                  overflowX: 'auto',
                  position: 'relative'
                }}
              >
                <Table stickyHeader>
                  <TableHead>
                    <TableRow>
                      <TableCell className="font-600">Model Name</TableCell>
                      <TableCell className="font-600">Brand</TableCell>
                      <TableCell sx={{ fontWeight: 600 }} align="right">Year</TableCell>
                      <TableCell sx={{ fontWeight: 600 }} align="right">Speed (km/h)</TableCell>
                      <TableCell sx={{ fontWeight: 600 }} align="right">Battery (mAh)</TableCell>
                      <TableCell sx={{ fontWeight: 600 }} align="right">Price (VND)</TableCell>
                      <TableCell sx={{ fontWeight: 600 }} align="center">Actions</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {paginatedModels.map(m => (
                      <TableRow key={m.modelId} hover>
                        <TableCell>{highlightText(m.modelName, search)}</TableCell>
                        <TableCell>{highlightText(m.manufacturer, search)}</TableCell>
                        <TableCell align="right">{m.year}</TableCell>
                        <TableCell align="right">{m.maxSpeed}</TableCell>
                        <TableCell align="right">{m.batteryCapacity}</TableCell>
                        <TableCell align="right">{m.modelCost?.toLocaleString?.()}</TableCell>
                        <TableCell align="center">
                          <Tooltip title="Edit model">
                            <IconButton 
                              size="small" 
                              onClick={() => { setEditing(m); setOpenDialog(true) }}
                              color="primary"
                            >
                              <EditIcon />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Delete model">
                            <IconButton 
                              size="small" 
                              onClick={() => handleDelete(m)}
                              color="error"
                            >
                              <DeleteIcon />
                            </IconButton>
                          </Tooltip>
                        </TableCell>
                      </TableRow>
                    ))}
                    {paginatedModels.length === 0 && !loading && (
                      <TableRow>
                        <TableCell colSpan={7}>
                          <EmptyState 
                            icon={search ? "search" : "inbox"}
                            title={search ? "No models found" : "No models available"}
                            message={search 
                              ? `No models match your search "${search}". Try a different search term.`
                              : "Get started by adding your first vehicle model."
                            }
                            action={
                              !search && (
                                <Button 
                                  variant="contained" 
                                  startIcon={<AddIcon />}
                                  onClick={() => { setEditing({}); setOpenDialog(true) }}
                                >
                                  Add Model
                                </Button>
                              )
                            }
                          />
                        </TableCell>
                      </TableRow>
                    )}
                  </TableBody>
                </Table>
                {filtered.length > 0 && (
                  <TablePagination
                    page={page}
                    rowsPerPage={rowsPerPage}
                    totalRows={filtered.length}
                    onPageChange={setPage}
                    onRowsPerPageChange={(newRowsPerPage) => {
                      setRowsPerPage(newRowsPerPage)
                      setPage(0)
                    }}
                  />
                )}
              </TableContainer>
            )}

            {/* Form Dialog */}
            <Dialog open={openDialog} onClose={() => setOpenDialog(false)} maxWidth="sm" fullWidth>
              <DialogTitle>
                {editing?.modelId ? 'Edit model' : 'Add new model'}
              </DialogTitle>
              <DialogContent className="pt-2">
                <ModelForm 
                  initial={editing} 
                  onSubmit={handleSubmit} 
                  onCancel={() => { setOpenDialog(false); setEditing(null) }}
                />
              </DialogContent>
            </Dialog>

            {/* Delete Confirmation Dialog */}
            <ConfirmDialog
              open={!!deleteConfirm}
              onClose={() => setDeleteConfirm(null)}
              onConfirm={confirmDelete}
              title="Delete Model"
              message={`Are you sure you want to delete "${deleteConfirm?.modelName}"? This action cannot be undone.`}
              confirmText="Delete"
              cancelText="Cancel"
              severity="error"
            />
          </Stack>
        </Container>
      </Box>
    </AdminLayout>
  )
}
