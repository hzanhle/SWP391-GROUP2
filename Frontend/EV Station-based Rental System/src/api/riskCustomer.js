const BASE_URL = (import.meta.env.VITE_BOOKING_API_URL || '').replace(/\/$/, '')

async function request(path, { method = 'GET', body, token, headers = {} } = {}) {
  if (!BASE_URL) {
    throw new Error('VITE_BOOKING_API_URL is not set')
  }
  const url = `${BASE_URL}${path.startsWith('/') ? '' : '/'}${path}`

  const isFormData = typeof FormData !== 'undefined' && body instanceof FormData
  const init = {
    method,
    headers: {
      'Accept': 'application/json',
      ...(isFormData ? {} : (body !== undefined ? { 'Content-Type': 'application/json' } : {})),
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...headers,
    },
    body: body !== undefined ? (isFormData ? body : JSON.stringify(body)) : undefined,
  }

  let res
  try {
    res = await fetch(url, init)
  } catch (fetchErr) {
    const message = `Network error: failed to reach Booking API at ${BASE_URL}.`
    const error = new Error(message)
    error.cause = fetchErr
    error.data = null
    error.status = 0
    throw error
  }

  let data
  const contentType = res.headers.get('content-type')
  if (contentType && contentType.includes('application/json')) {
    data = await res.json()
  } else {
    data = { message: await res.text() }
  }

  if (!res.ok) {
    const error = new Error(data.message || `HTTP ${res.status}: ${res.statusText}`)
    error.data = data
    error.status = res.status
    throw error
  }

  return { data, status: res.status, headers: res.headers }
}

// Get all risk customers
export function getRiskCustomers(token, filters = {}) {
  const params = new URLSearchParams()
  if (filters.riskLevel) {
    params.append('riskLevel', filters.riskLevel)
  }
  if (filters.minRiskScore !== undefined) {
    params.append('minRiskScore', filters.minRiskScore)
  }
  
  const queryString = params.toString()
  const path = `/api/risk-customers${queryString ? `?${queryString}` : ''}`
  
  return request(path, {
    method: 'GET',
    token,
  })
}

// Get detailed risk profile for a user
export function getUserRiskProfile(userId, token) {
  return request(`/api/risk-customers/${userId}`, {
    method: 'GET',
    token,
  })
}

// Calculate risk score for a user
export function calculateUserRisk(userId, token) {
  return request(`/api/risk-customers/${userId}/calculate`, {
    method: 'POST',
    token,
  })
}

export default {
  getRiskCustomers,
  getUserRiskProfile,
  calculateUserRisk,
}

