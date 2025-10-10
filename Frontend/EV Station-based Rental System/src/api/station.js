const BASE_URL = (import.meta.env.VITE_STATION_API_URL || import.meta.env.VITE_API_URL || '').replace(/\/$/, '')

async function request(path, { method = 'GET', body, token, headers = {} } = {}) {
  if (!BASE_URL) {
    throw new Error('VITE_STATION_API_URL or VITE_API_URL is not set')
  }
  const url = `${BASE_URL}${path.startsWith('/') ? '' : '/'}${path}`

  const isFormData = typeof FormData !== 'undefined' && body instanceof FormData
  const init = {
    method,
    headers: {
      'Accept': 'application/json',
      ...(isFormData ? {} : (body ? { 'Content-Type': 'application/json' } : {})),
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...headers,
    },
    body: body ? (isFormData ? body : JSON.stringify(body)) : undefined,
  }

  let res
  try {
    res = await fetch(url, init)
  } catch (fetchErr) {
    const message = `Network error: failed to reach API at ${BASE_URL}. If you are running the API locally, the hosted preview cannot access localhost. Use a public URL or run the frontend locally.`
    const error = new Error(message)
    error.cause = fetchErr
    error.data = null
    error.status = 0
    throw error
  }

  const isJson = res.headers.get('content-type')?.includes('application/json')
  const data = isJson ? await res.json().catch(() => null) : null

  if (!res.ok) {
    const message = (data && (data.message || data.Message)) || `${res.status} ${res.statusText}`
    const error = new Error(message)
    error.status = res.status
    error.data = data
    throw error
  }
  return { status: res.status, data }
}

export function getAllStations(token) {
  return request('/api/station', { token })
}

export function getActiveStations(token) {
  return request('/api/station/active', { token })
}

export function getStationById(id, token) {
  return request(`/api/station/${id}`, { token })
}

export function createStation(payload, token) {
  return request('/api/station', { method: 'POST', body: {
    name: payload.name,
    location: payload.location,
    managerId: payload.managerId ?? null,
  }, token })
}

export function updateStation(station, token) {
  return request('/api/station', { method: 'PUT', body: station, token })
}

export function deleteStation(id, token) {
  return request(`/api/station/${id}`, { method: 'DELETE', token })
}

export function setStationStatus(id, token) {
  return request(`/api/station/${id}`, { method: 'PATCH', token })
}

export default { request, getAllStations, getActiveStations, getStationById, createStation, updateStation, deleteStation, setStationStatus }
