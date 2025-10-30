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
    const message = `Network error: failed to reach Feedback API at ${BASE_URL}. If you are running the API locally, the hosted preview cannot access localhost. Use a public URL or run the frontend locally.`
    const error = new Error(message)
    error.cause = fetchErr
    error.data = null
    error.status = 0
    throw error
  }

  const isJson = res.headers.get('content-type')?.includes('application/json')
  let data = isJson ? await res.json().catch(() => null) : null

  if (!res.ok) {
    const message = (data && (data.message || data.Message)) || `${res.status} ${res.statusText}`
    const error = new Error(message)
    error.status = res.status
    error.data = data
    throw error
  }

  // Unwrap nested data if present
  if (data && typeof data === 'object' && (data.data !== undefined || data.Data !== undefined)) {
    data = data.data !== undefined ? data.data : data.Data
  }

  return { status: res.status, data }
}

export function submitFeedback(feedbackPayload, token) {
  return request('/api/feedback', {
    method: 'POST',
    body: {
      userId: feedbackPayload.userId || feedbackPayload.UserId,
      orderId: feedbackPayload.orderId || feedbackPayload.OrderId,
      vehicleId: feedbackPayload.vehicleId || feedbackPayload.VehicleId,
      vehicleRating: feedbackPayload.vehicleRating || feedbackPayload.VehicleRating,
      comments: feedbackPayload.comments || feedbackPayload.Comments,
    },
    token,
  })
}

export function getFeedbackByOrder(orderId, token) {
  return request(`/api/feedback/order/${orderId}`, {
    method: 'GET',
    token,
  })
}

export function getFeedbackByUser(userId, token) {
  return request(`/api/feedback/user/${userId}`, {
    method: 'GET',
    token,
  })
}

export function getAllFeedback(token) {
  return request('/api/feedback', {
    method: 'GET',
    token,
  })
}

export default {
  request,
  submitFeedback,
  getFeedbackByOrder,
  getFeedbackByUser,
  getAllFeedback,
}
