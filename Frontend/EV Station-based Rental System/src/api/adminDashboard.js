function ensureAdminDashboardPath(baseUrl) {
  if (!baseUrl) return ''
  let trimmed = baseUrl.trim()
  if (!trimmed) return ''
  trimmed = trimmed.replace(/\/+$/, '')

  // If already ends with /admin-dashboard (case-insensitive), keep as-is
  if (/\/admin-dashboard$/i.test(trimmed)) {
    return trimmed
  }

  // If ends with /admin (from older config), replace with /admin-dashboard
  if (/\/admin$/i.test(trimmed)) {
    return trimmed.replace(/\/admin$/i, '/admin-dashboard')
  }

  // If URL only contains domain (no specific path), append /admin-dashboard
  try {
    const url = new URL(trimmed)
    return `${url.origin}/admin-dashboard`
  } catch {
    // Fallback: append path directly
    return `${trimmed}/admin-dashboard`
  }
}

const rawDashboardBase =
  import.meta.env.VITE_AdminDashboard_API_URL ||
  (import.meta.env.VITE_API_URL ? `${import.meta.env.VITE_API_URL}` : '')

const DASHBOARD_BASE_URL = ensureAdminDashboardPath(rawDashboardBase)

async function request(path, { method = 'GET', body, token, headers = {} } = {}) {
  if (!DASHBOARD_BASE_URL) throw new Error('Admin dashboard API base URL is not configured (set VITE_AdminDashboard_API_URL or VITE_API_URL)')
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
  let data = isJson ? await res.json().catch(() => null) : null
  if (!res.ok) { const error = new Error((data && (data.message || data.Message)) || `${res.status} ${res.statusText}`); error.status = res.status; error.data = data; throw error }

  // Unwrap nested data if present
  if (data && typeof data === 'object' && (data.data !== undefined || data.Data !== undefined)) {
    data = data.data !== undefined ? data.data : data.Data
  }

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
