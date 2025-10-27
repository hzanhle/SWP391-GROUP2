const STATION_BASE_URL = (import.meta.env.VITE_STATION_API_URL || import.meta.env.VITE_API_URL || '').replace(/\/$/, '')

async function request(path, { method = 'GET', body, token, headers = {} } = {}) {
  if (!STATION_BASE_URL) throw new Error('VITE_STATION_API_URL or VITE_API_URL is not set')
  const url = `${STATION_BASE_URL}${path.startsWith('/') ? '' : '/'}${path}`
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
    const error = new Error(`Network error: failed to reach API at ${STATION_BASE_URL}. If you are running the API locally, the hosted preview cannot access localhost. Use a public URL or run the frontend locally.`)
    error.cause = err; error.status = 0; error.data = null; throw error
  }

  const isJson = res.headers.get('content-type')?.includes('application/json')
  const data = isJson ? await res.json().catch(() => null) : null
  if (!res.ok) { const error = new Error((data && (data.message || data.Message)) || `${res.status} ${res.statusText}`); error.status = res.status; error.data = data; throw error }
  return { status: res.status, data }
}

export function getAllShifts(token) {
  return request('/api/StaffShift', { token })
}

export function getShiftById(id, token) {
  return request(`/api/StaffShift/${id}`, { token })
}

export function getShiftsByStation(stationId, token) {
  return request(`/api/StaffShift/station/${stationId}`, { token })
}

export function getShiftsByUser(userId, token) {
  return request(`/api/StaffShift/user/${userId}`, { token })
}

export function createShift(payload, token) {
  return request('/api/StaffShift', { method: 'POST', body: payload, token })
}

export function updateShift(id, payload, token) {
  return request(`/api/StaffShift/${id}`, { method: 'PUT', body: payload, token })
}

export function deleteShift(id, token) {
  return request(`/api/StaffShift/${id}`, { method: 'DELETE', token })
}

export default { getAllShifts, getShiftById, getShiftsByStation, getShiftsByUser, createShift, updateShift, deleteShift }
