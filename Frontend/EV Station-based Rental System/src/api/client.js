const BASE_URL = (import.meta.env.VITE_API_URL || '').replace(/\/$/, '')

async function request(path, { method = 'GET', body, token, headers = {} } = {}) {
  if (!BASE_URL) {
    throw new Error('VITE_API_URL is not set')
  }
  const url = `${BASE_URL}${path.startsWith('/') ? '' : '/'}${path}`
  const init = {
    method,
    headers: {
      'Accept': 'application/json',
      ...(body ? { 'Content-Type': 'application/json' } : {}),
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...headers,
    },
    body: body ? JSON.stringify(body) : undefined,
  }

  const res = await fetch(url, init)
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

export function registerUser(user) {
  // POST /api/User with body matching backend model
  return request('/api/User', { method: 'POST', body: {
    userName: user.userName,
    email: user.email,
    phoneNumber: user.phoneNumber,
    password: user.password,
    roleId: user.roleId ?? 0,
    isActive: user.isActive ?? true,
    trustScore: user.trustScore ?? 0,
  } })
}

export function login(credentials) {
  // POST /api/User/login expects { UserName, Password } (case-insensitive)
  return request('/api/User/login', { method: 'POST', body: {
    userName: credentials.userName,
    password: credentials.password,
  } })
}

export function getUserById(userId, token) {
  return request(`/api/User/${userId}`, { method: 'GET', token })
}

export default { request, registerUser, login, getUserById }
