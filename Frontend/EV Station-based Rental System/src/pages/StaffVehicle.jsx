import { useEffect, useState } from 'react'
import { Card, CardContent, CardHeader } from '@mui/material'
import StaffLayout from '../components/staff/StaffLayout'
import { getStaffVehicles, updateVehicleStatus, getVehicleHistory } from '../api/staffVehicle'
import '../styles/admin.css'

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

function VehicleCard({ vehicle, onStatusChange, onViewHistory }) {
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
    <Card className="admin-card" style={{ marginBottom: '1.5rem', padding: '1.5rem' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start', marginBottom: '1rem' }}>
        <div>
          <h3 style={{ fontSize: '1.25rem', fontWeight: '700', color: '#010103', margin: '0 0 0.25rem', fontFamily: 'Poppins, sans-serif' }}>
            {vehicle.licensePlate || vehicle.LicensePlate || 'N/A'}
          </h3>
          <p style={{ fontSize: '0.875rem', color: '#706f7b', margin: 0, fontFamily: 'Rubik, sans-serif' }}>
            {vehicle.modelName || vehicle.ModelName || 'Unknown Model'}
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
          <span style={{ fontSize: '0.875rem', fontWeight: '600', color: '#010103', fontFamily: 'Rubik, sans-serif' }}>
            Battery Level
          </span>
          <span style={{ fontSize: '0.875rem', fontWeight: '600', color: batteryColor, fontFamily: 'Rubik, sans-serif' }}>
            {batteryPercentage}%
          </span>
        </div>
        <div style={{ width: '100%', height: '8px', backgroundColor: '#e2e8f0', borderRadius: '4px', overflow: 'hidden' }}>
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

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2, 1fr)', gap: '1rem', marginBottom: '1.5rem' }}>
        <div>
          <p style={{ fontSize: '0.75rem', color: '#706f7b', margin: '0 0 0.25rem', fontFamily: 'Rubik, sans-serif' }}>
            Station
          </p>
          <p style={{ fontSize: '0.875rem', fontWeight: '600', color: '#010103', margin: 0, fontFamily: 'Rubik, sans-serif' }}>
            {vehicle.stationName || vehicle.StationName || 'N/A'}
          </p>
        </div>
        <div>
          <p style={{ fontSize: '0.75rem', color: '#706f7b', margin: '0 0 0.25rem', fontFamily: 'Rubik, sans-serif' }}>
            Last Updated
          </p>
          <p style={{ fontSize: '0.875rem', fontWeight: '600', color: '#010103', margin: 0, fontFamily: 'Rubik, sans-serif' }}>
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
              border: currentStatus === status.value ? `2px solid ${status.color}` : '1px solid #e2e8f0',
              backgroundColor: currentStatus === status.value ? status.color : '#ffffff',
              color: currentStatus === status.value ? status.textColor : '#706f7b',
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
        style={{
          width: '100%',
          padding: '0.5rem',
          borderRadius: '0.375rem',
          border: '1px solid #e2e8f0',
          backgroundColor: '#ffffff',
          color: '#ff4d30',
          cursor: 'pointer',
          fontSize: '0.875rem',
          fontWeight: '600',
          fontFamily: 'Rubik, sans-serif',
          transition: 'all 0.3s ease',
        }}
        onMouseEnter={e => {
          e.target.style.backgroundColor = '#fff7f3'
        }}
        onMouseLeave={e => {
          e.target.style.backgroundColor = '#ffffff'
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
  const [filteredVehicles, setFilteredVehicles] = useState([])
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)
  const [successMessage, setSuccessMessage] = useState('')
  const [statusFilter, setStatusFilter] = useState('all')
  const [batteryFilter, setBatteryFilter] = useState('all')
  const [searchTerm, setSearchTerm] = useState('')
  const [selectedHistory, setSelectedHistory] = useState(null)
  const [historyLoading, setHistoryLoading] = useState(false)

  const rawUser = (typeof window !== 'undefined' && localStorage.getItem('auth.user')) || '{}'
  let currentRoleId = 0
  try { currentRoleId = Number(JSON.parse(rawUser)?.roleId || JSON.parse(rawUser)?.RoleId || 0) } catch {}
  const forbidden = currentRoleId !== 2

  useEffect(() => {
    let mounted = true
    ;(async () => {
      try {
        setLoading(true)
        const response = await getStaffVehicles(null, null, null, token)
        if (!mounted) return
        const vehicleList = Array.isArray(response.data?.data) ? response.data.data : (Array.isArray(response.data) ? response.data : [])
        setVehicles(vehicleList)
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
      const batteryLevel = Number(v.batteryLevel || v.BatteryLevel || 0)
      if (batteryFilter === 'low') {
        filtered = filtered.filter(v => batteryLevel < 50)
      } else if (batteryFilter === 'medium') {
        filtered = filtered.filter(v => batteryLevel >= 50 && batteryLevel < 80)
      } else if (batteryFilter === 'high') {
        filtered = filtered.filter(v => batteryLevel >= 80)
      }
    }

    if (searchTerm) {
      filtered = filtered.filter(v => {
        const licensePlate = (v.licensePlate || v.LicensePlate || '').toLowerCase()
        const modelName = (v.modelName || v.ModelName || '').toLowerCase()
        const search = searchTerm.toLowerCase()
        return licensePlate.includes(search) || modelName.includes(search)
      })
    }

    setFilteredVehicles(filtered)
  }, [vehicles, statusFilter, batteryFilter, searchTerm])

  async function handleStatusChange(vehicleId, payload) {
    try {
      await updateVehicleStatus(vehicleId, payload, token)
      setSuccessMessage('Vehicle status updated successfully!')
      setTimeout(() => setSuccessMessage(''), 3000)

      const response = await getStaffVehicles(null, null, null, token)
      const vehicleList = Array.isArray(response.data?.data) ? response.data.data : (Array.isArray(response.data) ? response.data : [])
      setVehicles(vehicleList)
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
              <a className="btn btn-primary" href="#" style={{ display: 'inline-block', marginTop: '1rem', padding: '0.75rem 1.5rem', borderRadius: '0.5rem', backgroundColor: '#ff4d30', color: '#fff', textDecoration: 'none' }}>Back to Home</a>
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
            <div role="alert" className="card card-body" style={{ backgroundColor: '#fee2e2', borderColor: '#fecaca', color: '#991b1b', marginBottom: '1.5rem' }}>{error}</div>
          ) : null}

          {successMessage ? (
            <div role="status" className="card card-body" style={{ backgroundColor: '#dcfce7', borderColor: '#bbf7d0', color: '#166534', marginBottom: '1.5rem' }}>{successMessage}</div>
          ) : null}

          {selectedHistory ? (
            <div className="card card-body" style={{ marginBottom: '1.5rem' }}>
              <button
                onClick={() => setSelectedHistory(null)}
                style={{
                  padding: '0.5rem 1rem',
                  marginBottom: '1rem',
                  backgroundColor: '#ffffff',
                  border: '1px solid #e2e8f0',
                  borderRadius: '0.375rem',
                  color: '#ff4d30',
                  cursor: 'pointer',
                  fontWeight: '600',
                }}
              >
                ← Back to Vehicles
              </button>
              <h2 style={{ fontSize: '1.5rem', fontWeight: '700', color: '#010103', marginBottom: '1rem', fontFamily: 'Poppins, sans-serif' }}>Vehicle History</h2>
              {historyLoading ? (
                <p>Loading history...</p>
              ) : selectedHistory && selectedHistory.statusHistory && Array.isArray(selectedHistory.statusHistory) && selectedHistory.statusHistory.length > 0 ? (
                <div style={{ overflowX: 'auto' }}>
                  <table style={{ borderCollapse: 'collapse', width: '100%' }}>
                    <thead style={{ backgroundColor: '#f0f9ff', borderBottom: '2px solid #e2e8f0' }}>
                      <tr>
                        <th style={{ padding: '1rem', textAlign: 'left', fontWeight: '600', color: '#010103', fontFamily: 'Poppins, sans-serif', fontSize: '0.875rem' }}>Date</th>
                        <th style={{ padding: '1rem', textAlign: 'left', fontWeight: '600', color: '#010103', fontFamily: 'Poppins, sans-serif', fontSize: '0.875rem' }}>Status</th>
                        <th style={{ padding: '1rem', textAlign: 'left', fontWeight: '600', color: '#010103', fontFamily: 'Poppins, sans-serif', fontSize: '0.875rem' }}>Notes</th>
                      </tr>
                    </thead>
                    <tbody>
                      {selectedHistory.statusHistory.map((record, idx) => (
                        <tr key={idx} style={{ borderBottom: '1px solid #e2e8f0' }}>
                          <td style={{ padding: '1rem', color: '#706f7b', fontFamily: 'Rubik, sans-serif' }}>
                            {new Date(record.timestamp || record.Timestamp).toLocaleDateString()} {new Date(record.timestamp || record.Timestamp).toLocaleTimeString()}
                          </td>
                          <td style={{ padding: '1rem', color: '#706f7b', fontFamily: 'Rubik, sans-serif' }}>
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
                          <td style={{ padding: '1rem', color: '#706f7b', fontFamily: 'Rubik, sans-serif' }}>
                            {record.notes || record.Notes || '-'}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              ) : (
                <p style={{ color: '#706f7b' }}>No history records found.</p>
              )}
            </div>
          ) : (
            <>
              <div className="card card-body" style={{ marginBottom: '1.5rem' }}>
                <div className="admin-grid" style={{ gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))' }}>
                  <div>
                    <label style={{ display: 'block', fontSize: '0.875rem', fontWeight: '600', color: '#010103', marginBottom: '0.5rem', fontFamily: 'Rubik, sans-serif' }}>Search</label>
                    <input
                      type="text"
                      placeholder="Search by plate or model..."
                      value={searchTerm}
                      onChange={e => setSearchTerm(e.target.value)}
                      style={{
                        width: '100%',
                        padding: '0.5rem 0.75rem',
                        borderRadius: '0.375rem',
                        border: '1px solid #e2e8f0',
                        fontSize: '0.875rem',
                        fontFamily: 'Rubik, sans-serif',
                        boxSizing: 'border-box',
                      }}
                    />
                  </div>
                  <div>
                    <label style={{ display: 'block', fontSize: '0.875rem', fontWeight: '600', color: '#010103', marginBottom: '0.5rem', fontFamily: 'Rubik, sans-serif' }}>Status</label>
                    <select
                      value={statusFilter}
                      onChange={e => setStatusFilter(e.target.value)}
                      style={{
                        width: '100%',
                        padding: '0.5rem 0.75rem',
                        borderRadius: '0.375rem',
                        border: '1px solid #e2e8f0',
                        fontSize: '0.875rem',
                        fontFamily: 'Rubik, sans-serif',
                        boxSizing: 'border-box',
                      }}
                    >
                      <option value="all">All Status</option>
                      <option value="available">Available</option>
                      <option value="charging">Charging</option>
                      <option value="in_use">In Use</option>
                      <option value="maintenance">Maintenance</option>
                    </select>
                  </div>
                  <div>
                    <label style={{ display: 'block', fontSize: '0.875rem', fontWeight: '600', color: '#010103', marginBottom: '0.5rem', fontFamily: 'Rubik, sans-serif' }}>Battery</label>
                    <select
                      value={batteryFilter}
                      onChange={e => setBatteryFilter(e.target.value)}
                      style={{
                        width: '100%',
                        padding: '0.5rem 0.75rem',
                        borderRadius: '0.375rem',
                        border: '1px solid #e2e8f0',
                        fontSize: '0.875rem',
                        fontFamily: 'Rubik, sans-serif',
                        boxSizing: 'border-box',
                      }}
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
                  <p style={{ fontSize: '0.875rem', color: '#706f7b', marginBottom: '1rem', fontFamily: 'Rubik, sans-serif' }}>
                    Showing {filteredVehicles.length} of {vehicles.length} vehicles
                  </p>
                  {filteredVehicles.map((vehicle) => (
                    <VehicleCard
                      key={vehicle.id || vehicle.Id}
                      vehicle={vehicle}
                      onStatusChange={handleStatusChange}
                      onViewHistory={handleViewHistory}
                    />
                  ))}
                </div>
              ) : (
                <div className="card card-body" style={{ textAlign: 'center', padding: '2rem' }}>
                  <p style={{ color: '#706f7b', fontSize: '1rem' }}>No vehicles found matching your filters</p>
                </div>
              )}
            </>
          )}
        </div>
      </section>
    </StaffLayout>
  )
}
