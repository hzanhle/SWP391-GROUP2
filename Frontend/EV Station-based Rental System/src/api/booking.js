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
    const message = `Network error: failed to reach Booking API at ${BASE_URL}. If you are running the API locally, the hosted preview cannot access localhost. Use a public URL or run the frontend locally.`
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

// Preview order - calculate total cost and check availability
export function getOrderPreview(payload, token) {
  return request('/api/orders/preview', {
    method: 'POST',
    body: {
      userId: payload.userId,
      vehicleId: payload.vehicleId,
      fromDate: payload.fromDate,
      toDate: payload.toDate,
      rentFeeForHour: payload.rentFeeForHour,
      modelPrice: payload.modelPrice,
      paymentMethod: payload.paymentMethod || 'VNPay',
    },
    token,
  })
}

// Create order - creates a new booking with status Pending
export function createOrder(payload, token) {
  return request('/api/orders', {
    method: 'POST',
    body: {
      userId: payload.userId,
      vehicleId: payload.vehicleId,
      fromDate: payload.fromDate,
      toDate: payload.toDate,
      rentFeeForHour: payload.rentFeeForHour,
      modelPrice: payload.modelPrice,
      paymentMethod: payload.paymentMethod || 'VNPay',
    },
    token,
  })
}

// Get payment URL for VNPay
export function createVNPayURL(orderId, token) {
  const q = new URLSearchParams({ orderId: String(orderId) }).toString()
  return request(`/api/payment/vnpay-create?${q}`, {
    method: 'GET',
    token,
  })
}

// Create PayOS payment link
export function createPayOSLink(orderId, token) {
  return request('/api/payment/payos/create', {
    method: 'POST',
    body: orderId,
    token,
  })
}

// Get payment status
export function getPaymentStatus(orderId, token) {
  return request(`/api/payment/status/${orderId}`, {
    method: 'GET',
    token,
  })
}

// Get order details
export function getOrderById(orderId, token) {
  return request(`/api/orders/${orderId}`, {
    method: 'GET',
    token,
  })
}

// Get user's orders
export function getOrdersByUserId(userId, token) {
  return request(`/api/orders/user/${userId}`, {
    method: 'GET',
    token,
  })
}

// Confirm payment (webhook - called from backend after VNPay callback)
export function confirmPayment(payload) {
  return request('/api/orders/confirm-payment', {
    method: 'POST',
    body: {
      orderId: payload.orderId,
      transactionId: payload.transactionId,
      gatewayResponse: payload.gatewayResponse,
    },
  })
}

// Start rental (check-in)
export function startRental(orderId, images, token) {
  const formData = new FormData()
  
  // Append all images to formData with key "images"
  if (images && Array.isArray(images)) {
    images.forEach((image) => {
      if (image) {
        formData.append('images', image)
      }
    })
  }
  
  // Append optional fields if needed (empty object for VehicleCheckInRequest)
  // formData.append('OdometerReading', '')
  // formData.append('FuelLevel', '')
  // formData.append('Notes', '')
  
  return request(`/api/orders/${orderId}/start`, {
    method: 'POST',
    body: formData,
    token,
  })
}

// Complete rental (check-out/return)
export function completeRental(orderId, images, hasDamage = false, damageDescription = '', conditionNotes = '', token) {
  const formData = new FormData()
  
  // Append all images to formData with key "images"
  if (images && Array.isArray(images)) {
    images.forEach((image) => {
      if (image) {
        formData.append('images', image)
      }
    })
  }
  
  // Append VehicleReturnRequest fields
  formData.append('HasDamage', hasDamage.toString())
  if (damageDescription) {
    formData.append('DamageDescription', damageDescription)
  }
  // Optional fields
  // formData.append('OdometerReading', '')
  // formData.append('FuelLevel', '')
  if (conditionNotes) {
    formData.append('ConditionNotes', conditionNotes)
  }
  
  return request(`/api/orders/${orderId}/complete`, {
    method: 'POST',
    body: formData,
    token,
  })
}

// Create contract after payment
export function createContract(contractData, token) {
  return request('/api/contracts/create', {
    method: 'POST',
    body: contractData,
    token,
  })
}

// Submit feedback after return
export function submitFeedback(feedbackPayload, token) {
  return request('/api/feedback', {
    method: 'POST',
    body: feedbackPayload,
    token,
  })
}

// [Admin] Get all orders
export function getAllOrders(token) {
  return request('/api/orders/all', {
    method: 'GET',
    token,
  })
}

export default {
  request,
  getOrderPreview,
  createOrder,
  createVNPayURL,
  createPayOSLink,
  getPaymentStatus,
  getOrderById,
  getOrdersByUserId,
  getAllOrders,
  confirmPayment,
  startRental,
  completeRental,
  createContract,
  submitFeedback,
}
