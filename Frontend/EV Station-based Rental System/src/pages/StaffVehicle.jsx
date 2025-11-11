import { useEffect, useState } from 'react'
import { Card, CardContent, CardHeader } from '@mui/material'
import StaffLayout from '../components/staff/StaffLayout'
import { getStaffVehicles, updateVehicleStatus, getVehicleHistory } from '../api/staffVehicle'
import { getAllModels } from '../api/vehicle'
import '../styles/staff.css'

const VEHICLE_STATUSES = [
  { value: 'available', label: 'Available', color: '#dcfce7', textColor: '#166534' },
  { value: 'charging', label: 'Charging', color: '#fef3c7', textColor: '#92400e' },
  { value: 'in_use', label: 'In Use', color: '#dbeafe', textColor: '#1e40af' },
  { value: 'maintenance', label: 'Maintenance', color: '#fee2e2', textColor: '#991b1b' },
]

function getStatusBadge(status) {
  const statusObj = VEHICLE_STATUSES.find(s => s.value === status)
  return statusObj || { label: status, color: '#f3f4f6', textColor: '#6b7280' }
}

function VehicleCard({ vehicle, onStatusChange, onViewHistory, getModelName }) {
  const [isUpdating, setIsUpdating] = useState(false)
  const currentStatus = vehicle.status || vehicle.Status || 'unknown'

  async function handleStatusChange(newStatus) {
    if (newStatus === currentStatus) return
    try {
      setIsUpdating(true)
      await onStatusChange(vehicle.id || vehicle.Id, { status: newStatus })
    } finally {
      setIsUpdating(false)
    }
  }

  const batteryLevel = vehicle.batteryLevel || vehicle.BatteryLevel || 0
  const batteryPercentage = Math.min(100, Math.max(0, Number(batteryLevel) || 0))
  const batteryColor = batteryPercentage >= 80 ? '#16a34a' : batteryPercentage >= 50 ? '#f59e0b' : '#dc2626'

  return (
    <Card className="card card-body" style={{ marginBottom: '1.5rem' }}>
      <div className="row-between mb-4">
        <div>
          <h3 style={{ fontSize: '1.25rem', fontWeight: '700', margin: '0 0 0.25rem' }}>
            {vehicle.licensePlate || vehicle.LicensePlate || 'N/A'}
          </h3>
          <p className="muted mb-0">
            {vehicle.licensePlate && vehicle.modelId ? getModelName(vehicle.modelId || vehicle.ModelId) : (vehicle.modelName || vehicle.ModelName || 'Unknown Model')}
          </p>
        </div>
        <span
          className="badge"
          style={{
            backgroundColor: getStatusBadge(currentStatus).color,
            color: getStatusBadge(currentStatus).textColor,
          }}
        >
          {getStatusBadge(currentStatus).label}
        </span>
      </div>

      <div style={{ marginBottom: '1.5rem' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.5rem' }}>
          <span className="strong" style={{ fontSize: '0.875rem' }}>
            Battery Level
          </span>
          <span style={{ fontSize: '0.875rem', fontWeight: '600', color: batteryColor }}>
            {batteryPercentage}%
          </span>
        </div>
        <div style={{ width: '100%', height: '8px', backgroundColor: 'var(--staff-border)', borderRadius: '4px', overflow: 'hidden' }}>
          <div
            style={{
              height: '100%',
              width: `${batteryPercentage}%`,
              backgroundColor: batteryColor,
              transition: 'width 0.3s ease',
            }}
          />
        </div>
      </div>

      <div className="docs-grid mb-4">
        <div>
          <p className="muted mb-2" style={{ fontSize: '0.75rem' }}>
            Station
          </p>
          <p className="strong">
            {vehicle.stationName || vehicle.StationName || 'N/A'}
          </p>
        </div>
        <div>
          <p className="muted mb-2" style={{ fontSize: '0.75rem' }}>
            Last Updated
          </p>
          <p className="strong">
            {new Date(vehicle.lastStatusUpdate || vehicle.LastStatusUpdate || new Date()).toLocaleDateString()}
          </p>
        </div>
      </div>

      <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap', marginBottom: '1rem' }}>
        {VEHICLE_STATUSES.map(status => (
          <button
            key={status.value}
            onClick={() => handleStatusChange(status.value)}
            disabled={isUpdating || currentStatus === status.value}
            style={{
              padding: '0.5rem 1rem',
              borderRadius: '0.375rem',
              border: currentStatus === status.value ? `2px solid ${status.color}` : '1px solid var(--staff-border)',
              backgroundColor: currentStatus === status.value ? status.color : 'var(--staff-bg-primary)',
              color: currentStatus === status.value ? status.textColor : 'var(--staff-text-secondary)',
              cursor: isUpdating || currentStatus === status.value ? 'not-allowed' : 'pointer',
              opacity: isUpdating ? 0.6 : 1,
              fontSize: '0.75rem',
              fontWeight: '600',
              fontFamily: 'Rubik, sans-serif',
              transition: 'all 0.3s ease',
            }}
          >
            {status.label}
          </button>
        ))}
      </div>

      <button
        onClick={() => onViewHistory(vehicle.id || vehicle.Id)}
        className="btn"
        style={{
          width: '100%',
          color: 'var(--staff-accent)',
        }}
      >
        View History
      </button>
    </Card>
  )
}

export default function StaffVehicle() {
  const token = typeof window !== 'undefined' ? localStorage.getItem('auth.token') : ''
  const [vehicles, setVehicles] = useState([])
  const [models, setModels] = useState([])
  const [filteredVehicles, setFilteredVehicles] = useState([])
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)
  const [successMessage, setSuccessMessage] = useState('')
  const [statusFilter, setStatusFilter] = useState('all')
  const [batteryFilter, setBatteryFilter] = useState('all')
  const [searchTerm, setSearchTerm] = useState('')
  const [selectedHistory, setSelectedHistory] = useState(null)
  const [historyLoading, setHistoryLoading] = useState(false)

  const getModelName = (modelId) => {
    const m = models.find(x => x.modelId === modelId || x.id === modelId)
    return m ? `${m.manufacturer} ${m.modelName}` : 'Unknown Model'
  }

  const rawUser = (typeof window !== 'undefined' && localStorage.getItem('auth.user')) || '{}'
  let currentRoleId = 0
  try { currentRoleId = Number(JSON.parse(rawUser)?.roleId || JSON.parse(rawUser)?.RoleId || 0) } catch {}
  const forbidden = currentRoleId !== 2

  useEffect(() => {
    let mounted = true
    ;(async () => {
      try {
        setLoading(true)
        const [vehiclesRes, modelsRes] = await Promise.all([
          getStaffVehicles(null, null, null, token),
          getAllModels(token)
        ])
        if (!mounted) return
        const vehicleList = Array.isArray(vehiclesRes.data?.data) ? vehiclesRes.data.data : (Array.isArray(vehiclesRes.data) ? vehiclesRes.data : [])
        const modelsList = Array.isArray(modelsRes.data?.data) ? modelsRes.data.data : (Array.isArray(modelsRes.data) ? modelsRes.data : [])
        setVehicles(vehicleList)
        setModels(modelsList)
        setError('')
      } catch (e) {
        setError(e.message || 'Failed to load vehicles')
      } finally {
        setLoading(false)
      }
    })()
    return () => { mounted = false }
  }, [token])

  useEffect(() => {
    let filtered = vehicles

    if (statusFilter !== 'all') {
      filtered = filtered.filter(v => (v.status || v.Status) === statusFilter)
    }

    if (batteryFilter !== 'all') {
      filtered = filtered.filter(v => {
        const batteryLevel = Number(v.batteryLevel || v.BatteryLevel || 0)
        if (batteryFilter === 'low') {
          return batteryLevel < 50
        } else if (batteryFilter === 'medium') {
          return batteryLevel >= 50 && batteryLevel < 80
        } else if (batteryFilter === 'high') {
          return batteryLevel >= 80
        }
        return true
      })
    }

    if (searchTerm) {
      filtered = filtered.filter(v => {
        const licensePlate = (v.licensePlate || v.LicensePlate || '').toLowerCase()
        const modelName = getModelName(v.modelId || v.ModelId).toLowerCase()
        const search = searchTerm.toLowerCase()
        return licensePlate.includes(search) || modelName.includes(search)
      })
    }

    setFilteredVehicles(filtered)
  }, [vehicles, models, statusFilter, batteryFilter, searchTerm])

  async function handleStatusChange(vehicleId, payload) {
    try {
      await updateVehicleStatus(vehicleId, payload, token)
      setSuccessMessage('Vehicle status updated successfully!')
      setTimeout(() => setSuccessMessage(''), 3000)

      const [vehiclesRes, modelsRes] = await Promise.all([
        getStaffVehicles(null, null, null, token),
        getAllModels(token)
      ])
      const vehicleList = Array.isArray(vehiclesRes.data?.data) ? vehiclesRes.data.data : (Array.isArray(vehiclesRes.data) ? vehiclesRes.data : [])
      const modelsList = Array.isArray(modelsRes.data?.data) ? modelsRes.data.data : (Array.isArray(modelsRes.data) ? modelsRes.data : [])
      setVehicles(vehicleList)
      setModels(modelsList)
    } catch (e) {
      setError(e.message || 'Failed to update vehicle status')
    }
  }

  async function handleViewHistory(vehicleId) {
    try {
      setHistoryLoading(true)
      const response = await getVehicleHistory(vehicleId, token)
      setSelectedHistory(response.data?.data || response.data || null)
    } catch (e) {
      setError(e.message || 'Failed to load vehicle history')
    } finally {
      setHistoryLoading(false)
    }
  }

  if (forbidden) {
    return (
      <StaffLayout active="vehicles">
        <section className="section">
          <div className="container">
            <div className="card card-body">
              <h1 className="section-title">Unauthorized</h1>
              <p className="section-subtitle">You do not have permission to access this page.</p>
              <a className="btn btn-primary mt-4" href="#">Back to Home</a>
            </div>
          </div>
        </section>
      </StaffLayout>
    )
  }

  return (
    <StaffLayout active="vehicles">
      <section className="section">
        <div className="container">
          <div className="section-header">
            <h1 className="section-title">Vehicle Management</h1>
            <p className="section-subtitle">Manage vehicle charging, status, and returns</p>
          </div>

          {error && !selectedHistory ? (
            <div role="alert" className="card card-body badge red mb-4">{error}</div>
          ) : null}

          {successMessage ? (
            <div role="status" className="card card-body badge green mb-4">{successMessage}</div>
          ) : null}

          {selectedHistory ? (
            <div className="card card-body mb-4">
              <button
                onClick={() => setSelectedHistory(null)}
                className="btn mb-4"
              >
                ← Back to Vehicles
              </button>
              <h2 className="section-title" style={{ marginBottom: '1rem' }}>Vehicle History</h2>
              {historyLoading ? (
                <p className="muted">Loading history...</p>
              ) : selectedHistory && selectedHistory.statusHistory && Array.isArray(selectedHistory.statusHistory) && selectedHistory.statusHistory.length > 0 ? (
                <div style={{ overflowX: 'auto' }}>
                  <table>
                    <thead>
                      <tr>
                        <th>Date</th>
                        <th>Status</th>
                        <th>Notes</th>
                      </tr>
                    </thead>
                    <tbody>
                      {selectedHistory.statusHistory.map((record, idx) => (
                        <tr key={idx}>
                          <td>
                            {new Date(record.timestamp || record.Timestamp).toLocaleDateString()} {new Date(record.timestamp || record.Timestamp).toLocaleTimeString()}
                          </td>
                          <td>
                            <span
                              className="badge"
                              style={{
                                backgroundColor: getStatusBadge(record.status || record.Status).color,
                                color: getStatusBadge(record.status || record.Status).textColor,
                              }}
                            >
                              {getStatusBadge(record.status || record.Status).label}
                            </span>
                          </td>
                          <td>
                            {record.notes || record.Notes || '-'}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              ) : (
                <p className="muted">No history records found.</p>
              )}
            </div>
          ) : (
            <>
              <div className="card card-body mb-4">
                <div className="docs-grid">
                  <div className="field">
                    <label className="label">Search</label>
                    <input
                      type="text"
                      placeholder="Search by plate or model..."
                      value={searchTerm}
                      onChange={e => setSearchTerm(e.target.value)}
                      className="input"
                    />
                  </div>
                  <div className="field">
                    <label className="label">Status</label>
                    <select
                      value={statusFilter}
                      onChange={e => setStatusFilter(e.target.value)}
                      className="input"
                    >
                      <option value="all">All Status</option>
                      <option value="available">Available</option>
                      <option value="charging">Charging</option>
                      <option value="in_use">In Use</option>
                      <option value="maintenance">Maintenance</option>
                    </select>
                  </div>
                  <div className="field">
                    <label className="label">Battery</label>
                    <select
                      value={batteryFilter}
                      onChange={e => setBatteryFilter(e.target.value)}
                      className="input"
                    >
                      <option value="all">All Levels</option>
                      <option value="low">Low (0-50%)</option>
                      <option value="medium">Medium (50-80%)</option>
                      <option value="high">High (80-100%)</option>
                    </select>
                  </div>
                </div>
              </div>

              {loading ? (
                <div className="card card-body">Loading vehicles...</div>
              ) : filteredVehicles.length > 0 ? (
                <div>
                  <p className="muted mb-4">
                    Showing {filteredVehicles.length} of {vehicles.length} vehicles
                  </p>
                  {filteredVehicles.map((vehicle) => (
                    <VehicleCard
                      key={vehicle.id || vehicle.Id}
                      vehicle={vehicle}
                      onStatusChange={handleStatusChange}
                      onViewHistory={handleViewHistory}
                      getModelName={getModelName}
                    />
                  ))}
                </div>
              ) : (
                <div className="card card-body">
                  <p className="muted">No vehicles found matching your filters</p>
                </div>
              )}
            </>
          )}
        </div>
      </section>
    </StaffLayout>
  )
}
