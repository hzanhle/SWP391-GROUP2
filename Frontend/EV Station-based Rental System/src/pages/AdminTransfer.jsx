import { useEffect, useMemo, useState } from 'react'
import { Box, Container, Card, CardContent, CardHeader, Typography, Alert, Stack, TextField, MenuItem, Button, Collapse, Checkbox, FormControlLabel, Divider, Chip } from '@mui/material'
import AdminLayout from '../components/admin/AdminLayout'
import { getAllStations } from '../api/station'
import { getAllVehicles, getAllModels } from '../api/vehicle'
import { createTransfers } from '../api/transferVehicle'

function GroupRow({ model, vehicles, selectedIds, onToggleAll, onToggleOne, open, onToggleOpen }) {
  const selectedCount = vehicles.filter(v => selectedIds.has(v.vehicleId || v.VehicleId)).length
  const total = vehicles.length
  const title = `${model.name} (${total} vehicles)`
  return (
    <Card className="admin-card" sx={{ mb: 2 }}>
      <CardHeader
        title={<Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <Button size="small" variant="outlined" onClick={onToggleOpen}>{open ? 'Hide' : 'Show'}</Button>
          <Typography variant="h6">{title}</Typography>
          <Chip size="small" label={`${selectedCount}/${total} selected`} />
          <Button size="small" onClick={onToggleAll}>{selectedCount === total ? 'Unselect all' : 'Select all'}</Button>
        </Box>}
      />
      <Collapse in={open}>
        <CardContent>
          <Stack spacing={1}>
            {vehicles.map(v => {
              const id = v.vehicleId || v.VehicleId
              const plate = v.licensePlate || v.LicensePlate || `ID ${id}`
              return (
                <FormControlLabel key={id}
                  control={<Checkbox checked={selectedIds.has(id)} onChange={(e)=>onToggleOne(id, e.target.checked)} />}
                  label={`${plate}`}
                />
              )
            })}
          </Stack>
        </CardContent>
      </Collapse>
    </Card>
  )
}

export default function AdminTransfer() {
  const token = typeof window !== 'undefined' ? localStorage.getItem('auth.token') : ''
  const [stations, setStations] = useState([])
  const [models, setModels] = useState([])
  const [vehicles, setVehicles] = useState([])
  const [sourceId, setSourceId] = useState('')
  const [targetId, setTargetId] = useState('')
  const [expanded, setExpanded] = useState({})
  const [selected, setSelected] = useState(new Set())
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  useEffect(() => {
    let mounted = true
    ;(async () => {
      try {
        const [st, md, vs] = await Promise.all([
          getAllStations(token).catch(()=>({ data: [] })),
          getAllModels(token).catch(()=>({ data: [] })),
          getAllVehicles(token).catch(()=>({ data: [] })),
        ])
        if (!mounted) return
        const stationList = Array.isArray(st.data) ? st.data : []
        const modelList = Array.isArray(md.data) ? md.data : []
        const vehicleList = Array.isArray(vs.data) ? vs.data : []
        setStations(stationList)
        setModels(modelList)
        setVehicles(vehicleList)
        setError('')
      } catch (e) {
        setError(e.message || 'Failed to load data')
      }
    })()
    return () => { mounted = false }
  }, [token])

  const modelMap = useMemo(() => {
    const m = new Map()
    models.forEach(x => {
      const id = Number(x.modelId || x.ModelId)
      const name = `${x.manufacturer || x.Manufacturer || ''} ${x.modelName || x.ModelName || ''}`.trim() || `Model ${id}`
      m.set(id, name)
    })
    return m
  }, [models])

  const vehiclesAtSource = useMemo(() => {
    const sid = Number(sourceId)
    if (!sid) return []
    return (vehicles || []).filter(v => Number(v.stationId || v.StationId) === sid && (v.isActive ?? v.IsActive ?? true))
  }, [vehicles, sourceId])

  const grouped = useMemo(() => {
    const g = new Map()
    vehiclesAtSource.forEach(v => {
      const mid = Number(v.modelId || v.ModelId)
      if (!g.has(mid)) g.set(mid, [])
      g.get(mid).push(v)
    })
    return Array.from(g.entries()).map(([modelId, list]) => ({
      modelId, 
      name: modelMap.get(modelId) || `Model ${modelId}`,
      vehicles: list
    }))
  }, [vehiclesAtSource, modelMap])

  function toggleOne(id, checked) {
    setSelected(prev => {
      const next = new Set(prev)
      if (checked) next.add(id); else next.delete(id)
      return next
    })
  }

  function toggleAll(modelId) {
    const list = grouped.find(g => g.modelId === modelId)?.vehicles || []
    setSelected(prev => {
      const next = new Set(prev)
      const ids = list.map(v => v.vehicleId || v.VehicleId)
      const allSelected = ids.every(id => next.has(id))
      ids.forEach(id => { if (allSelected) next.delete(id); else next.add(id) })
      return next
    })
  }

  function clearSelection() { setSelected(new Set()) }

  async function submitTransfers() {
    try {
      setError(''); setSuccess(''); setLoading(true)
      const sid = Number(sourceId), tid = Number(targetId)
      if (!sid || !tid || sid === tid) throw new Error('Please choose two different stations')
      // Build per-model batches
      const batches = []
      grouped.forEach(g => {
        const ids = g.vehicles.map(v => v.vehicleId || v.VehicleId).filter(id => selected.has(id))
        if (ids.length > 0) batches.push({ modelId: g.modelId, ids })
      })
      if (batches.length === 0) throw new Error('Please select at least one vehicle')

      // Call API per model
      for (const b of batches) {
        await createTransfers({ vehicleIds: b.ids, modelId: b.modelId, currentStationId: sid, targetStationId: tid }, token)
      }
      setSuccess(`Created ${batches.reduce((a,b)=>a+b.ids.length,0)} transfer request(s) across ${batches.length} model(s).`)
      clearSelection()
    } catch (e) {
      setError(e.message || 'Failed to create transfer requests')
    } finally { setLoading(false) }
  }

  return (
    <AdminLayout active="transfer">
      <Box className="admin-page">
        <Container maxWidth="lg">
          <Stack className="admin-stack" spacing={3}>
            <Box className="admin-header">
              <Box>
                <Typography variant="h4" component="h1" gutterBottom className="font-600">Transfer Vehicles</Typography>
                <Typography variant="body1" color="text.secondary">Select source and target stations, then choose vehicles grouped by model.</Typography>
              </Box>
            </Box>

            {error && <Alert severity="error" onClose={() => setError('')}>{error}</Alert>}
            {success && <Alert severity="success" onClose={() => setSuccess('')}>{success}</Alert>}

            <Card className="admin-card">
              <CardContent>
                <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
                  <TextField select fullWidth label="Source Station" value={sourceId} onChange={(e)=>{ setSourceId(e.target.value); clearSelection() }}>
                    <MenuItem value="">Select source</MenuItem>
                    {stations.map(s => (
                      <MenuItem key={s.stationId ?? s.id} value={String(s.stationId ?? s.id)}>{s.name}</MenuItem>
                    ))}
                  </TextField>
                  <TextField select fullWidth label="Target Station" value={targetId} onChange={(e)=>setTargetId(e.target.value)}>
                    <MenuItem value="">Select target</MenuItem>
                    {stations.map(s => (
                      <MenuItem key={s.stationId ?? s.id} value={String(s.stationId ?? s.id)} disabled={String(s.stationId ?? s.id)===String(sourceId)}>{s.name}</MenuItem>
                    ))}
                  </TextField>
                </Stack>
              </CardContent>
            </Card>

            {sourceId && (
              <Card className="admin-card">
                <CardHeader title="Vehicles at source station (grouped by model)" />
                <CardContent>
                  {grouped.length === 0 ? (
                    <Typography variant="body2" color="text.secondary">No vehicles at this station.</Typography>
                  ) : (
                    <>
                      {grouped.map(g => (
                        <GroupRow key={g.modelId}
                          model={g}
                          vehicles={g.vehicles}
                          selectedIds={selected}
                          open={!!expanded[g.modelId]}
                          onToggleOpen={() => setExpanded(prev => ({ ...prev, [g.modelId]: !prev[g.modelId] }))}
                          onToggleAll={() => toggleAll(g.modelId)}
                          onToggleOne={toggleOne}
                        />
                      ))}
                      <Divider sx={{ my: 1 }} />
                      <Box sx={{ display: 'flex', gap: 1, justifyContent: 'flex-end' }}>
                        <Button variant="outlined" onClick={clearSelection} disabled={selected.size === 0}>Clear</Button>
                        <Button variant="contained" onClick={submitTransfers} disabled={loading || selected.size === 0 || !targetId}>{loading ? 'Submitting…' : 'Create Transfer Requests'}</Button>
                      </Box>
                    </>
                  )}
                </CardContent>
              </Card>
            )}

          </Stack>
        </Container>
      </Box>
    </AdminLayout>
  )
}
