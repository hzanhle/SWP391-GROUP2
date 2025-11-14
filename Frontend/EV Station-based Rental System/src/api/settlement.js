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
    const message = `Network error: failed to reach Booking API at ${BASE_URL}`
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

// Get settlement by order ID
export function getSettlementByOrderId(orderId, token) {
  return request(`/api/settlement/order/${orderId}`, {
    method: 'GET',
    token,
  })
}

// Process automatic refund (for PayOS)
export function processAutomaticRefund(orderId, token) {
  return request(`/api/settlement/${orderId}/refund/automatic`, {
    method: 'POST',
    token,
  })
}

// Mark refund as processed manually (for VNPay - offline refund)
export function markRefundAsProcessed(orderId, proofDocument, notes, token) {
  const formData = new FormData()
  if (proofDocument) {
    formData.append('ProofDocument', proofDocument)
  }
  if (notes) {
    formData.append('Notes', notes)
  }
  
  return request(`/api/settlement/${orderId}/refund/manual`, {
    method: 'POST',
    body: formData,
    token,
  })
}

// Mark refund as failed
export function markRefundAsFailed(orderId, notes, token) {
  return request(`/api/settlement/${orderId}/refund/failed`, {
    method: 'POST',
    body: { Notes: notes || '' },
    token,
  })
}

// PayOS refund (alternative endpoint)
export function refundPayOSDeposit(orderId, token) {
  return request('/api/payment/payos/refund', {
    method: 'POST',
    body: orderId,
    token,
  })
}

export default {
  getSettlementByOrderId,
  processAutomaticRefund,
  markRefundAsProcessed,
  markRefundAsFailed,
  refundPayOSDeposit,
}

