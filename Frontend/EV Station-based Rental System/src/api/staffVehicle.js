const VEHICLE_BASE_URL = (import.meta.env.VITE_VEHICLE_API_URL || import.meta.env.VITE_API_URL || '').replace(/\/$/, '')

async function request(path, { method = 'GET', body, token, headers = {} } = {}) {
  if (!VEHICLE_BASE_URL) throw new Error('VITE_VEHICLE_API_URL or VITE_API_URL is not set')
  const url = `${VEHICLE_BASE_URL}${path.startsWith('/') ? '' : '/'}${path}`
  const isFormData = typeof FormData !== 'undefined' && body instanceof FormData
  const init = {
    method,
    headers: {
      Accept: 'application/json',
      ...(isFormData ? {} : (body !== undefined ? { 'Content-Type': 'application/json' } : {})),
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...headers,
    },
    body: body !== undefined ? (isFormData ? body : JSON.stringify(body)) : undefined,
  }

  let res
  try { res = await fetch(url, init) } catch (err) {
    const error = new Error(`Network error: failed to reach API at ${VEHICLE_BASE_URL}. If you are running the API locally, the hosted preview cannot access localhost. Use a public URL or run the frontend locally.`)
    error.cause = err; error.status = 0; error.data = null; throw error
  }

  const isJson = res.headers.get('content-type')?.includes('application/json')
  let data = isJson ? await res.json().catch(() => null) : null
  if (!res.ok) { const error = new Error((data && (data.message || data.Message)) || `${res.status} ${res.statusText}`); error.status = res.status; error.data = data; throw error }

  // Unwrap nested data if present
  if (data && typeof data === 'object' && (data.data !== undefined || data.Data !== undefined)) {
    data = data.data !== undefined ? data.data : data.Data
  }

  return { status: res.status, data }
}

export function getStaffVehicles(status, battery, search, token) {
  const params = new URLSearchParams()
  if (status) params.append('status', status)
  if (battery) params.append('battery', battery)
  if (search) params.append('search', search)
  const queryString = params.toString()
  const path = queryString ? `/api/staff/vehicles?${queryString}` : '/api/staff/vehicles'
  return request(path, { token })
}

export function getStaffVehicleById(vehicleId, token) {
  return request(`/api/staff/vehicles/${vehicleId}`, { token })
}

export function updateVehicleStatus(vehicleId, payload, token) {
  return request(`/api/staff/vehicles/${vehicleId}/status`, { method: 'PUT', body: payload, token })
}

export function getVehicleHistory(vehicleId, token) {
  return request(`/api/staff/vehicles/${vehicleId}/history`, { token })
}

export function getVehicleStats(token) {
  return request('/api/staff/vehicles/stats', { token })
}

export default { getStaffVehicles, getStaffVehicleById, updateVehicleStatus, getVehicleHistory, getVehicleStats }
