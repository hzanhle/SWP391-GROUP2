import { useEffect, useState } from 'react'
import { getAllStations, createStation, updateStation, deleteStation, setStationStatus } from '../api/station'
import {
  Box, Container, Card, CardContent, CardHeader, Typography, Button, TextField, Dialog,
  DialogTitle, DialogContent, DialogActions, Alert, CircularProgress, Stack, Grid, Paper,
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow, IconButton, Chip, Switch, FormControlLabel,
  TableSortLabel, TablePagination, Snackbar
} from '@mui/material'
import { Edit as EditIcon, Delete as DeleteIcon, Add as AddIcon, PowerSettingsNew as PowerIcon } from '@mui/icons-material'
import AdminLayout from '../components/admin/AdminLayout'
import 'leaflet/dist/leaflet.css'
import L from 'leaflet'
import iconUrl from 'leaflet/dist/images/marker-icon.png'
import iconRetinaUrl from 'leaflet/dist/images/marker-icon-2x.png'
import shadowUrl from 'leaflet/dist/images/marker-shadow.png'

function StationForm({ initial, onSubmit, onCancel }) {
  // Configure default marker icon (bundlers need explicit assets)
  const DefaultIcon = useMemo(() => L.icon({ iconRetinaUrl, iconUrl, shadowUrl, iconSize: [25,41], iconAnchor: [12,41], popupAnchor: [1,-34], tooltipAnchor: [16,-28], shadowSize: [41,41] }), [])
  useEffect(() => { L.Marker.prototype.options.icon = DefaultIcon }, [DefaultIcon])

  function StationPickerMap({ lat, lng, onChange }) {
    const containerRef = useRef(null)
    const mapRef = useRef(null)
    const markerRef = useRef(null)

    useEffect(() => {
      let mounted = true
      if (!containerRef.current || mapRef.current) return
      const start = (Number.isFinite(lat) && Number.isFinite(lng)) ? [lat, lng] : [10.776, 106.700]
      const map = L.map(containerRef.current, { center: start, zoom: 14, scrollWheelZoom: false })
      mapRef.current = map
      L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', { maxZoom: 19, attribution: '&copy; OpenStreetMap contributors' }).addTo(map)

      const pos = (Number.isFinite(lat) && Number.isFinite(lng)) ? [lat, lng] : null
      if (pos) {
        markerRef.current = L.marker(pos, { draggable: true }).addTo(map)
      }

      function setPos(p) {
        if (!mounted) return
        if (!markerRef.current) markerRef.current = L.marker(p, { draggable: true }).addTo(map)
        else markerRef.current.setLatLng(p)
        onChange?.(p[0], p[1])
      }

      map.on('click', (e) => setPos([e.latlng.lat, e.latlng.lng]))
      markerRef.current?.on('dragend', () => {
        const p = markerRef.current.getLatLng()
        onChange?.(p.lat, p.lng)
      })

      // Ensure size
      setTimeout(() => map.invalidateSize(), 0)
      return () => { mounted = false; map.remove() }
    }, [])

    // Keep marker in sync when lat/lng change from inputs
    useEffect(() => {
      const map = mapRef.current
      if (!map) return
      if (Number.isFinite(lat) && Number.isFinite(lng)) {
        const p = [lat, lng]
        if (!markerRef.current) markerRef.current = L.marker(p, { draggable: true }).addTo(map)
        else markerRef.current.setLatLng(p)
      }
    }, [lat, lng])

    return (
      <div className="map-card">
        <div ref={containerRef} className="map-canvas" aria-label="Pick station location on map" />
      </div>
    )
  }
  const [form, setForm] = useState(() => ({
    name: initial?.name || '',
    location: initial?.location || '',
    isActive: initial?.isActive ?? true,
    lat: typeof initial?.lat === 'number' ? initial.lat : 0,
    lng: typeof initial?.lng === 'number' ? initial.lng : 0,
  }))
  const [geoLoading, setGeoLoading] = useState(false)
  const [errors, setErrors] = useState({ name: '', location: '', lat: '', lng: '' })

  function validate(next = form) {
    const e = { name: '', location: '', lat: '', lng: '' }
    if (!next.name || next.name.trim().length < 5) e.name = 'Name must be at least 5 characters'
    if (!next.location || next.location.trim().length < 10) e.location = 'Location must be at least 10 characters'
    if (!Number.isFinite(next.lat) || next.lat < -90 || next.lat > 90) e.lat = 'Lat must be between -90 and 90'
    if (!Number.isFinite(next.lng) || next.lng < -180 || next.lng > 180) e.lng = 'Lng must be between -180 and 180'
    return e
  }

  function updateField(k, v) {
    const next = { ...form, [k]: v }
    setForm(next)
    setErrors(validate(next))
  }

  async function geocode() {
    if (!form.location) return
    try {
      setGeoLoading(true)
      const loc = String(form.location).trim()

      // Parse direct coordinates: "lat, lng"
      let lat, lng
      const coordMatch = loc.match(/(-?\d+(?:\.\d+)?)\s*,\s*(-?\d+(?:\.\d+)?)/)
      if (coordMatch) {
        lat = parseFloat(coordMatch[1]);
        lng = parseFloat(coordMatch[2]);
      } else {
        // Parse Google Maps URLs with various patterns for coordinates
        const patterns = [
          /!3d(-?\d+(?:\.\d+)?)!4d(-?\d+(?:\.\d+)?)/,            // place lat/lng
          /[?&]ll=(-?\d+(?:\.\d+)?),\s*(-?\d+(?:\.\d+)?)/,      // ll=lat,lng
          /[?&]q=(-?\d+(?:\.\d+)?),\s*(-?\d+(?:\.\d+)?)/,       // q=lat,lng
          /@(-?\d+(?:\.\d+)?),\s*(-?\d+(?:\.\d+)?)/             // @ center lat,lng
        ]
        for (const re of patterns) {
          const m = loc.match(re)
          if (m) {
            lat = parseFloat(m[1])
            lng = parseFloat(m[2])
            break
          }
        }
      }

      if (Number.isFinite(lat) && Number.isFinite(lng)) {
        // If lat seems out of range but lng is in [-90,90], the input might be reversed (lng,lat)
        if ((lat < -90 || lat > 90) && (lng >= -90 && lng <= 90)) {
          const tmp = lat; lat = lng; lng = tmp;
        }
        setForm(prev => {
          const next = { ...prev, lat, lng };
          setErrors(validate(next))
          return next
        })
        return
      }

      // Fallback to Nominatim search
      const url = `https://nominatim.openstreetmap.org/search?format=json&limit=1&q=${encodeURIComponent(loc)}`
      const res = await fetch(url, { headers: { 'Accept': 'application/json' } })
      if (!res.ok) throw new Error('Geocode failed')
      const list = await res.json()
      if (Array.isArray(list) && list.length > 0) {
        const best = list[0]
        let nlat = parseFloat(best.lat), nlng = parseFloat(best.lon)
        if (Number.isFinite(nlat) && Number.isFinite(nlng)) {
          setForm(prev => {
            const next = { ...prev, lat: nlat, lng: nlng }
            setErrors(validate(next))
            return next
          })
        }
      }
    } finally {
      setGeoLoading(false)
    }
  }

  async function submit(e) {
    e.preventDefault()
    const eObj = validate(form)
    setErrors(eObj)
    const ok = Object.values(eObj).every(x => !x)
    if (!ok) return
    await onSubmit(form)
  }

  const canSave = Object.values(errors).every(x => !x)

  useEffect(() => { setErrors(validate(form)) }, [])

  return (
    <Box component="form" onSubmit={submit} sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
      <Grid container spacing={2} sx={{ width: '100%' }}>
        <Grid size={{ xs: 12 }}>
          <TextField fullWidth label="Station Name" value={form.name} onChange={e=>updateField('name', e.target.value)} required error={!!errors.name} helperText={errors.name} />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <TextField fullWidth label="Location/Address" value={form.location} onChange={e=>updateField('location', e.target.value)} required multiline rows={3} error={!!errors.location} helperText={errors.location} />
        </Grid>
        <Grid size={{ xs: 6 }}>
          <TextField fullWidth type="number" label="Latitude" value={form.lat} onChange={e=>updateField('lat', parseFloat(e.target.value))} inputProps={{ step: 'any', min: -90, max: 90 }} error={!!errors.lat} helperText={errors.lat} />
        </Grid>
        <Grid size={{ xs: 6 }}>
          <TextField fullWidth type="number" label="Longitude" value={form.lng} onChange={e=>updateField('lng', parseFloat(e.target.value))} inputProps={{ step: 'any', min: -180, max: 180 }} error={!!errors.lng} helperText={errors.lng} />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <FormControlLabel control={<Switch checked={!!form.isActive} onChange={e=>updateField('isActive', e.target.checked)} />} label="Active" />
        </Grid>
      </Grid>
      <Typography variant="subtitle2" sx={{ mt: 2 }}>Pick location on map</Typography>
      <StationPickerMap
        lat={Number.isFinite(form.lat) ? form.lat : undefined}
        lng={Number.isFinite(form.lng) ? form.lng : undefined}
        onChange={(plat, plng) => {
          setForm(prev => { const next = { ...prev, lat: plat, lng: plng }; setErrors(validate(next)); return next })
        }}
      />

      <Box sx={{ display: 'flex', gap: 1, justifyContent: 'space-between', alignItems: 'center' }}>
        <Button variant="outlined" onClick={geocode} disabled={geoLoading}>
          {geoLoading ? 'Locating…' : 'Locate from Address'}
        </Button>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Button variant="outlined" onClick={onCancel}>Cancel</Button>
          <Button variant="contained" type="submit" disabled={!canSave}>Save</Button>
        </Box>
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
  const [confirmOpen, setConfirmOpen] = useState(false)
  const [confirmTarget, setConfirmTarget] = useState(null)
  const [snack, setSnack] = useState({ open: false, message: '', severity: 'success' })
  const [filterStatus, setFilterStatus] = useState('all') // all | active | inactive
  const [orderBy, setOrderBy] = useState('stationId')
  const [order, setOrder] = useState('asc')
  const [page, setPage] = useState(0)
  const [rowsPerPage, setRowsPerPage] = useState(10)
  const token = typeof window !== 'undefined' ? localStorage.getItem('auth.token') : null

  const filtered = stations.filter(s => {
    const q = search.toLowerCase()
    const matchesQuery = (String(s.stationId||'')).includes(q) || (s.name||'').toLowerCase().includes(q) || (s.location||'').toLowerCase().includes(q)
    const matchesStatus = filterStatus === 'all' || (filterStatus === 'active' ? s.isActive : !s.isActive)
    return matchesQuery && matchesStatus
  })

  const sorted = [...filtered].sort((a,b) => {
    const dir = order === 'asc' ? 1 : -1
    const get = (obj, key) => {
      switch (key) {
        case 'stationId': return Number(obj.stationId)
        case 'name': return (obj.name||'').toLowerCase()
        case 'location': return (obj.location||'').toLowerCase()
        case 'isActive': return obj.isActive ? 1 : 0
        default: return obj[key]
      }
    }
    const va = get(a, orderBy), vb = get(b, orderBy)
    if (va < vb) return -1*dir
    if (va > vb) return 1*dir
    return 0
  })
  const paged = sorted.slice(page*rowsPerPage, page*rowsPerPage + rowsPerPage)

  async function load() {
    setLoading(true)
    setError('')
    try {
      const { data } = await getAllStations(token)
      const mapped = (Array.isArray(data) ? data : []).map(s => ({
        stationId: s.stationId ?? s.id ?? s.Id,
        name: s.name ?? s.Name ?? '',
        location: s.location ?? s.Location ?? '',
        isActive: s.isActive ?? s.IsActive ?? false,
        lat: s.lat ?? s.Lat ?? 0,
        lng: s.lng ?? s.Lng ?? 0,
      }))
      setStations(mapped)
    } catch (e) {
      setError(e?.message || 'Failed to load stations')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [])

  function askDelete(s) {
    setConfirmTarget(s)
    setConfirmOpen(true)
  }

  async function handleDeleteConfirmed() {
    if (!confirmTarget) return
    try {
      setLoading(true)
      await deleteStation(confirmTarget.stationId, token)
      setSnack({ open: true, message: 'Station deleted', severity: 'success' })
      await load()
    } catch (e) {
      setSnack({ open: true, message: e?.message || 'Delete failed', severity: 'error' })
    } finally {
      setLoading(false)
      setConfirmOpen(false)
      setConfirmTarget(null)
    }
  }

  async function handleToggleStatus(s) {
    try {
      setLoading(true)
      await setStationStatus(s.stationId, token)
      setSnack({ open: true, message: s.isActive ? 'Station deactivated' : 'Station activated', severity: 'success' })
      await load()
    } catch (e) {
      setSnack({ open: true, message: e?.message || 'Failed to update status', severity: 'error' })
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

            {/* Search & Filters */}
            <Card>
              <CardContent>
                <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} alignItems={{ xs: 'stretch', sm: 'center' }} justifyContent="space-between">
                  <TextField
                    fullWidth
                    placeholder="Search by ID, name, or location..."
                    value={search}
                    onChange={e => { setSearch(e.target.value); setPage(0) }}
                    variant="outlined"
                    size="small"
                  />
                  <Stack direction="row" spacing={1} alignItems="center">
                    <Chip label="All" clickable color={filterStatus==='all'?'primary':'default'} onClick={()=>{ setFilterStatus('all'); setPage(0) }} />
                    <Chip label="Active" clickable color={filterStatus==='active'?'primary':'default'} onClick={()=>{ setFilterStatus('active'); setPage(0) }} />
                    <Chip label="Inactive" clickable color={filterStatus==='inactive'?'primary':'default'} onClick={()=>{ setFilterStatus('inactive'); setPage(0) }} />
                  </Stack>
                </Stack>
              </CardContent>
            </Card>

            {/* Stations Table */}
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
                      <TableCell sortDirection={orderBy==='stationId'?order:false}>
                        <TableSortLabel active={orderBy==='stationId'} direction={orderBy==='stationId'?order:'asc'} onClick={() => setOrder(prev => (orderBy==='stationId' && prev==='asc') ? 'desc' : 'asc') || setOrderBy('stationId') }>
                          Station ID
                        </TableSortLabel>
                      </TableCell>
                      <TableCell sortDirection={orderBy==='name'?order:false}>
                        <TableSortLabel active={orderBy==='name'} direction={orderBy==='name'?order:'asc'} onClick={() => setOrder(prev => (orderBy==='name' && prev==='asc') ? 'desc' : 'asc') || setOrderBy('name') }>
                          Station Name
                        </TableSortLabel>
                      </TableCell>
                      <TableCell sortDirection={orderBy==='location'?order:false}>
                        <TableSortLabel active={orderBy==='location'} direction={orderBy==='location'?order:'asc'} onClick={() => setOrder(prev => (orderBy==='location' && prev==='asc') ? 'desc' : 'asc') || setOrderBy('location') }>
                          Location
                        </TableSortLabel>
                      </TableCell>
                      <TableCell sortDirection={orderBy==='isActive'?order:false}>
                        <TableSortLabel active={orderBy==='isActive'} direction={orderBy==='isActive'?order:'asc'} onClick={() => setOrder(prev => (orderBy==='isActive' && prev==='asc') ? 'desc' : 'asc') || setOrderBy('isActive') }>
                          Status
                        </TableSortLabel>
                      </TableCell>
                      <TableCell sx={{ fontWeight: 600 }} align="center">Actions</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {paged.map(s => (
                      <TableRow key={s.stationId} hover>
                        <TableCell>{s.stationId}</TableCell>
                        <TableCell>{s.name}</TableCell>
                        <TableCell>{s.location}</TableCell>
                        <TableCell>
                          <Chip size="small" label={s.isActive ? 'Active' : 'Inactive'} color={s.isActive ? 'success' : 'default'} />
                        </TableCell>
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
                            onClick={() => handleToggleStatus(s)}
                            color={s.isActive ? 'warning' : 'success'}
                            title={s.isActive ? 'Deactivate' : 'Activate'}
                          >
                            <PowerIcon />
                          </IconButton>
                          <IconButton
                            size="small"
                            onClick={() => askDelete(s)}
                            color="error"
                          >
                            <DeleteIcon />
                          </IconButton>
                        </TableCell>
                      </TableRow>
                    ))}
                    {filtered.length === 0 && (
                      <TableRow>
                        <TableCell colSpan={5} align="center" sx={{ py: 3, color: 'text.secondary' }}>
                          No stations found
                        </TableCell>
                      </TableRow>
                    )}
                  </TableBody>
                </Table>
                <TablePagination
                  component="div"
                  count={filtered.length}
                  page={page}
                  onPageChange={(e, p) => setPage(p)}
                  rowsPerPage={rowsPerPage}
                  onRowsPerPageChange={(e) => { setRowsPerPage(parseInt(e.target.value, 10)); setPage(0) }}
                  rowsPerPageOptions={[5,10,25,50]}
                />
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

            {/* Confirm Delete */}
            <Dialog open={confirmOpen} onClose={() => setConfirmOpen(false)}>
              <DialogTitle>Delete station?</DialogTitle>
              <DialogContent>
                <Typography>Are you sure you want to delete "{confirmTarget?.name}"?</Typography>
              </DialogContent>
              <DialogActions>
                <Button onClick={() => setConfirmOpen(false)}>Cancel</Button>
                <Button color="error" variant="contained" onClick={handleDeleteConfirmed}>Delete</Button>
              </DialogActions>
            </Dialog>

            <Snackbar
              open={snack.open}
              autoHideDuration={3000}
              onClose={() => setSnack(s => ({ ...s, open: false }))}
              anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
            >
              <Alert onClose={() => setSnack(s => ({ ...s, open: false }))} severity={snack.severity} sx={{ width: '100%' }}>
                {snack.message}
              </Alert>
            </Snackbar>
          </Stack>
        </Container>
      </Box>
    </AdminLayout>
  )
}
