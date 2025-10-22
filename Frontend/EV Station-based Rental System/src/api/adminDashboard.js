const DASHBOARD_BASE_URL = (import.meta.env.VITE_AdminDashboard_API_URL || '').replace(/\/$/, '')

async function request(path, { method = 'GET', body, token, headers = {} } = {}) {
  if (!DASHBOARD_BASE_URL) throw new Error('VITE_AdminDashboard_API_URL is not set')
  const url = `${DASHBOARD_BASE_URL}${path.startsWith('/') ? '' : '/'}${path}`
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
    const error = new Error(`Network error: failed to reach API at ${DASHBOARD_BASE_URL}. If you are running the API locally, the hosted preview cannot access localhost. Use a public URL or run the frontend locally.`)
    error.cause = err; error.status = 0; error.data = null; throw error
  }

  const isJson = res.headers.get('content-type')?.includes('application/json')
  const data = isJson ? await res.json().catch(() => null) : null
  if (!res.ok) { const error = new Error((data && (data.message || data.Message)) || `${res.status} ${res.statusText}`); error.status = res.status; error.data = data; throw error }
  return { status: res.status, data }
}

export function getDashboardSummary(token) {
  return request('/api/Dashboard/summary', { token })
}

export function getStationStats(token) {
  return request('/api/Dashboard/stations', { token })
}

export function getRevenueByMonth(year, token) {
  const q = new URLSearchParams(); if (year) q.set('year', String(year))
  return request(`/api/Dashboard/revenue/monthly${q.toString() ? `?${q}` : ''}`, { token })
}

export function getTopUsedVehicles(top = 10, token) {
  const q = new URLSearchParams({ top: String(top) }).toString()
  return request(`/api/Dashboard/vehicles/top-used?${q}`, { token })
}

export function getUserGrowth(token) {
  return request('/api/Dashboard/users/growth', { token })
}

export default { getDashboardSummary, getStationStats, getRevenueByMonth, getTopUsedVehicles, getUserGrowth }
