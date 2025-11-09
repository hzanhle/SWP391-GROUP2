import { useEffect, useMemo, useState, useCallback } from 'react'
import { Box, Container, Card, CardContent, CardHeader, Typography, Alert, Stack, TextField, MenuItem, Button, IconButton, Tooltip } from '@mui/material'
import AdminLayout from '../components/admin/AdminLayout'
import { getTransfers, updateTransferStatus, deleteTransfer } from '../api/transferVehicle'
import { getAllStations } from '../api/station'
import { getAllModels } from '../api/vehicle'

function formatDate(d) {
  if (!d) return ''
  try { const dt = new Date(d); if (Number.isNaN(dt.getTime())) return String(d); return dt.toLocaleString() } catch { return String(d) }
}

function StatusBadge({ status }) {
  const s = String(status || '').trim()
  let cls = 'badge gray'
  if (s === 'Đang chuyển') cls = 'badge orange'
  else if (s === 'Hoàn thành') cls = 'badge green'
  else if (s === 'Hủy bỏ') cls = 'badge red'
  return <span className={cls}>{s || 'N/A'}</span>
}

export default function AdminOngoingTransfers() {
  const token = typeof window !== 'undefined' ? localStorage.getItem('auth.token') : ''
  const [transfers, setTransfers] = useState([])
  const [stations, setStations] = useState([])
  const [models, setModels] = useState([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [search, setSearch] = useState('')

  const stationName = useMemo(() => {
    const m = new Map();
    (stations || []).forEach(s => m.set(Number(s.stationId ?? s.id ?? s.Id), String(s.name || s.Name || 'Station')))
    return (id) => m.get(Number(id)) || `#${id}`
  }, [stations])

  const modelName = useMemo(() => {
    const m = new Map();
    (models || []).forEach(x => {
      const id = Number(x.modelId || x.ModelId)
      const name = `${x.manufacturer || x.Manufacturer || ''} ${x.modelName || x.ModelName || ''}`.trim() || `Model ${id}`
      m.set(id, name)
    })
    return (id) => m.get(Number(id)) || `Model ${id}`
  }, [models])

  const load = useCallback(async () => {
    try {
      setError(''); setLoading(true)
      const [t, st, md] = await Promise.all([
        getTransfers(token).catch(()=>({ data: [] })),
        getAllStations(token).catch(()=>({ data: [] })),
        getAllModels(token).catch(()=>({ data: [] })),
      ])
      const list = Array.isArray(t.data) ? t.data : []
      setTransfers(list)
      setStations(Array.isArray(st.data) ? st.data : [])
      setModels(Array.isArray(md.data) ? md.data : [])
    } catch (e) {
      setError(e.message || 'Failed to load transfers')
    } finally { setLoading(false) }
  }, [token])

  useEffect(() => { load() }, [load])

  const ongoingTransfers = useMemo(() => {
    return (transfers || []).filter(tv => String(tv.transferStatus || tv.TransferStatus) === 'Đang chuyển')
  }, [transfers])

  const filtered = useMemo(() => {
    const s = search.trim()
    if (!s) return ongoingTransfers
    return ongoingTransfers.filter(tv => String(tv.vehicleId || tv.VehicleId).includes(s))
  }, [ongoingTransfers, search])

  async function setStatus(vehicleId, status) {
    try {
      setError(''); setLoading(true)
      await updateTransferStatus(vehicleId, status, token)
      await load()
    } catch (e) {
      setError(e.message || 'Failed to update status')
    } finally { setLoading(false) }
  }

  async function remove(vehicleId) {
    try {
      setError(''); setLoading(true)
      await deleteTransfer(vehicleId, token)
      await load()
    } catch (e) {
      setError(e.message || 'Failed to delete transfer')
    } finally { setLoading(false) }
  }

  return (
    <AdminLayout active="transfer-ongoing">
      <Box className="admin-page">
        <Container maxWidth="lg">
          <Stack className="admin-stack" spacing={3}>
            <Box className="admin-header">
              <Box>
                <Typography variant="h4" component="h1" gutterBottom className="font-600">Ongoing Transfers</Typography>
                <Typography variant="body1" color="text.secondary">Monitor and manage vehicles currently being transferred between stations.</Typography>
              </Box>
              <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1}>
                <TextField placeholder="Search by Vehicle ID" size="small" value={search} onChange={(e)=>setSearch(e.target.value)} />
                <Button variant="outlined" onClick={load} disabled={loading}>{loading ? 'Refreshing…' : 'Refresh'}</Button>
              </Stack>
            </Box>

            {error && <Alert severity="error" onClose={() => setError('')}>{error}</Alert>}

            <Card className="admin-card">
              <CardHeader title="Transfers In Progress" />
              <CardContent>
                {filtered.length === 0 ? (
                  <Typography variant="body2" color="text.secondary">No ongoing transfers found.</Typography>
                ) : (
                  <div style={{ overflowX: 'auto' }}>
                    <table>
                      <thead>
                        <tr>
                          <th>Vehicle ID</th>
                          <th>Model</th>
                          <th>From Station</th>
                          <th>To Station</th>
                          <th>Status</th>
                          <th>Created</th>
                          <th>Updated</th>
                          <th>Actions</th>
                        </tr>
                      </thead>
                      <tbody>
                        {filtered.map(tv => {
                          const vehicleId = tv.vehicleId || tv.VehicleId
                          const modelId = tv.modelId || tv.ModelId
                          const fromId = tv.currentStationId || tv.CurrentStationId
                          const toId = tv.targetStationId || tv.TargetStationId
                          const status = tv.transferStatus || tv.TransferStatus
                          return (
                            <tr key={vehicleId}>
                              <td>#{vehicleId}</td>
                              <td>{modelName(modelId)}</td>
                              <td>{stationName(fromId)}</td>
                              <td>{stationName(toId)}</td>
                              <td><StatusBadge status={status} /></td>
                              <td>{formatDate(tv.createAt || tv.CreateAt)}</td>
                              <td>{formatDate(tv.updateAt || tv.UpdateAt)}</td>
                              <td>
                                <Stack direction="row" spacing={1}>
                                  <Tooltip title="Mark as Completed">
                                    <span>
                                      <Button size="small" variant="contained" color="success" onClick={()=>setStatus(vehicleId, 'Hoàn thành')} disabled={loading}>Hoàn thành</Button>
                                    </span>
                                  </Tooltip>
                                  <Tooltip title="Cancel Transfer">
                                    <span>
                                      <Button size="small" variant="outlined" color="error" onClick={()=>setStatus(vehicleId, 'Hủy bỏ')} disabled={loading}>Hủy bỏ</Button>
                                    </span>
                                  </Tooltip>
                                  <Tooltip title="Delete Request">
                                    <span>
                                      <IconButton color="error" onClick={()=>remove(vehicleId)} disabled={loading}>
                                        <svg width="20" height="20" viewBox="0 0 24 24" fill="none"><path d="M3 6h18M8 6V4a2 2 0 012-2h4a2 2 0 012 2v2m1 0v14a2 2 0 01-2 2H7a2 2 0 01-2-2V6m3 0v14m8-14v14" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/></svg>
                                      </IconButton>
                                    </span>
                                  </Tooltip>
                                </Stack>
                              </td>
                            </tr>
                          )
                        })}
                      </tbody>
                    </table>
                  </div>
                )}
              </CardContent>
            </Card>
          </Stack>
        </Container>
      </Box>
    </AdminLayout>
  )
}
