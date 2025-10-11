const BASE_URL = (import.meta.env.VITE_API_URL || '').replace(/\/$/, '')

async function request(path, { method = 'GET', body, token, headers = {} } = {}) {
  if (!BASE_URL) {
    throw new Error('VITE_API_URL is not set')
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

export function registerUser(user) {
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
  return request('/api/User/login', { method: 'POST', body: {
    userName: credentials.userName,
    password: credentials.password,
  } })
}

export function getUserById(userId, token) {
  return request(`/api/User/${userId}`, { method: 'GET', token })
}

export function getAllUsers(token) {
  return request('/api/User', { method: 'GET', token })
}

export function deleteUser(userId, token) {
  const q = new URLSearchParams({ userId: String(userId) }).toString()
  return request(`/api/User?${q}`, { method: 'DELETE', token })
}

export function toggleUserActive(userId, token) {
  return request(`/api/User/${userId}`, { method: 'PATCH', token })
}

export function toggleStaffAdmin(userId, token) {
  return request(`/api/User/SetRole${userId}`, { method: 'PATCH', token })
}

// CitizenInfo APIs (multipart/form-data)
export function createCitizenInfo(payload, token) {
  const fd = new FormData()
  if (payload.UserId != null) fd.append('UserId', String(payload.UserId))
  if (payload.CitizenId) fd.append('CitizenId', payload.CitizenId)
  if (payload.Sex) fd.append('Sex', payload.Sex)
  if (payload.CitiRegisDate) fd.append('CitiRegisDate', payload.CitiRegisDate)
  if (payload.CitiRegisOffice) fd.append('CitiRegisOffice', payload.CitiRegisOffice)
  if (payload.FullName) fd.append('FullName', payload.FullName)
  if (payload.Address) fd.append('Address', payload.Address)
  if (payload.DayOfBirth) fd.append('DayOfBirth', payload.DayOfBirth)
  const files = payload.Files || []
  for (const f of files) if (f) fd.append('Files', f)
  return request('/api/CitizenInfo', { method: 'POST', body: fd, token })
}

export function updateCitizenInfo(payload, token) {
  const fd = new FormData()
  if (payload.UserId != null) fd.append('UserId', String(payload.UserId))
  if (payload.CitizenId) fd.append('CitizenId', payload.CitizenId)
  if (payload.Sex) fd.append('Sex', payload.Sex)
  if (payload.CitiRegisDate) fd.append('CitiRegisDate', payload.CitiRegisDate)
  if (payload.CitiRegisOffice) fd.append('CitiRegisOffice', payload.CitiRegisOffice)
  if (payload.FullName) fd.append('FullName', payload.FullName)
  if (payload.Address) fd.append('Address', payload.Address)
  if (payload.DayOfBirth) fd.append('DayOfBirth', payload.DayOfBirth)
  const files = payload.Files || []
  for (const f of files) if (f) fd.append('Files', f)
  return request('/api/CitizenInfo', { method: 'PUT', body: fd, token })
}

export function getCitizenInfo(userId, token) {
  return request(`/api/CitizenInfo/${userId}`, { method: 'GET', token })
}

// DriverLicense APIs (multipart/form-data)
export function createDriverLicense(payload, token) {
  const fd = new FormData()
  if (payload.UserId != null) fd.append('UserId', String(payload.UserId))
  if (payload.LicenseId) fd.append('LicenseId', payload.LicenseId)
  if (payload.LicenseType) fd.append('LicenseType', payload.LicenseType)
  if (payload.RegisterDate) fd.append('RegisterDate', payload.RegisterDate)
  if (payload.RegisterOffice) fd.append('RegisterOffice', payload.RegisterOffice)
  const files = payload.Files || []
  for (const f of files) if (f) fd.append('Files', f)
  return request('/api/DriverLicense', { method: 'POST', body: fd, token })
}

export function updateDriverLicense(payload, token) {
  const fd = new FormData()
  if (payload.UserId != null) fd.append('UserId', String(payload.UserId))
  if (payload.LicenseId) fd.append('LicenseId', payload.LicenseId)
  if (payload.LicenseType) fd.append('LicenseType', payload.LicenseType)
  if (payload.RegisterDate) fd.append('RegisterDate', payload.RegisterDate)
  if (payload.RegisterOffice) fd.append('RegisterOffice', payload.RegisterOffice)
  const files = payload.Files || []
  for (const f of files) if (f) fd.append('Files', f)
  return request('/api/DriverLicense', { method: 'PUT', body: fd, token })
}

export function getDriverLicense(userId, token) {
  return request(`/api/DriverLicense/${userId}`, { method: 'GET', token })
}

export function getNotifications(userId, token) {
  return request(`/api/Notification/${userId}`, { method: 'GET', token })
}

export function clearNotifications(userId, token) {
  return request(`/api/Notification/${userId}`, { method: 'DELETE', token })
}

export default { request, registerUser, login, getUserById, getAllUsers, deleteUser, toggleUserActive, toggleStaffAdmin, createCitizenInfo, updateCitizenInfo, getCitizenInfo, createDriverLicense, updateDriverLicense, getDriverLicense, getNotifications, clearNotifications }
