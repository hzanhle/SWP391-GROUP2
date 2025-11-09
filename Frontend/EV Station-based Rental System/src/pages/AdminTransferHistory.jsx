import { useEffect, useMemo, useState, useCallback } from 'react'
import { Box, Container, Card, CardContent, CardHeader, Typography, Alert, Stack, TextField, MenuItem, Button } from '@mui/material'
import AdminLayout from '../components/admin/AdminLayout'
import { getTransfers } from '../api/transferVehicle'
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

export default function AdminTransferHistory() {
  const token = typeof window !== 'undefined' ? localStorage.getItem('auth.token') : ''
  const [transfers, setTransfers] = useState([])
  const [stations, setStations] = useState([])
  const [models, setModels] = useState([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [search, setSearch] = useState('')
  const [statusFilter, setStatusFilter] = useState('all')

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

  const historyTransfers = useMemo(() => {
    return (transfers || []).filter(tv => String(tv.transferStatus || tv.TransferStatus) !== 'Đang chuyển')
  }, [transfers])

  const uniqueStatuses = useMemo(() => {
    const set = new Set(historyTransfers.map(tv => String(tv.transferStatus || tv.TransferStatus)))
    return ['all', ...Array.from(set)]
  }, [historyTransfers])

  const filtered = useMemo(() => {
    const s = search.trim()
    let list = historyTransfers
    if (statusFilter !== 'all') list = list.filter(tv => String(tv.transferStatus || tv.TransferStatus) === statusFilter)
    if (!s) return list
    return list.filter(tv => String(tv.vehicleId || tv.VehicleId).includes(s))
  }, [historyTransfers, statusFilter, search])

  return (
    <AdminLayout active="transfer-history">
      <Box className="admin-page">
        <Container maxWidth="lg">
          <Stack className="admin-stack" spacing={3}>
            <Box className="admin-header">
              <Box>
                <Typography variant="h4" component="h1" gutterBottom className="font-600">Transfer History</Typography>
                <Typography variant="body1" color="text.secondary">View completed and cancelled transfer requests.</Typography>
              </Box>
              <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1}>
                <TextField placeholder="Search by Vehicle ID" size="small" value={search} onChange={(e)=>setSearch(e.target.value)} />
                <TextField select size="small" value={statusFilter} onChange={(e)=>setStatusFilter(e.target.value)}>
                  {uniqueStatuses.map(s => (
                    <MenuItem key={s} value={s}>{s === 'all' ? 'All statuses' : s}</MenuItem>
                  ))}
                </TextField>
                <Button variant="outlined" onClick={load} disabled={loading}>{loading ? 'Refreshing…' : 'Refresh'}</Button>
              </Stack>
            </Box>

            {error && <Alert severity="error" onClose={() => setError('')}>{error}</Alert>}

            <Card className="admin-card">
              <CardHeader title="Transfer Records" />
              <CardContent>
                {filtered.length === 0 ? (
                  <Typography variant="body2" color="text.secondary">No transfer history records found.</Typography>
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
