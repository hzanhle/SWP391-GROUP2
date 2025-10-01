const BASE_URL = (import.meta.env.VITE_VEHICLE_API_URL || '').replace(/\/$/, '')

async function request(path, { method = 'GET', body, token, headers = {} } = {}) {
  if (!BASE_URL) {
    throw new Error('VITE_VEHICLE_API_URL is not set')
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


// Vehicle API Service Methods

/**
 * Get all vehicles
 */
export function getAllVehicles(token) {
  return request('/api/Vehicle', { token })
}

/**
 * Get active vehicles only
 */
export function getActiveVehicles(token) {
  return request('/api/Vehicle/active', { token })
}

/**
 * Get vehicle by ID
 */
export function getVehicleById(id, token) {
  return request(`/api/Vehicle/${id}`, { token })
}

/**
 * Create a new vehicle
 */
export function createVehicle(vehicle, token) {
  return request('/api/Vehicle', {
    method: 'POST',
    body: {
      modelId: vehicle.modelId,
      color: vehicle.color,
      status: vehicle.status ?? 'Available',
      isActive: vehicle.isActive ?? true,
    },
    token
  })
}

/**
 * Update an existing vehicle
 */
export function updateVehicle(id, vehicle, token) {
  return request(`/api/Vehicle/${id}`, {
    method: 'PUT',
    body: {
      modelId: vehicle.modelId,
      color: vehicle.color,
      status: vehicle.status,
      isActive: vehicle.isActive,
    },
    token
  })
}

/**
 * Delete a vehicle
 */
export function deleteVehicle(id, token) {
  return request(`/api/Vehicle/${id}`, {
    method: 'DELETE',
    token
  })
}

/**
 * Update vehicle status
 */
export function updateVehicleStatus(id, status, token) {
  return request(`/api/Vehicle/${id}/status`, {
    method: 'PATCH',
    body: status,
    token
  })
}


// Model API Service Methods

/**
 * Get all models
 */
export function getAllModels(token) {
  return request('/api/Model', { token })
}

/**
 * Get active models only
 */
export function getActiveModels(token) {
  return request('/api/Model/active', { token })
}

/**
 * Get model by ID
 */
export function getModelById(id, token) {
  return request(`/api/Model/${id}`, { token })
}

/**
 * Create a new model
 */
export function createModel(model, token) {
  const formData = new FormData()
  
  // Add files if provided
  if (model.files && model.files.length > 0) {
    model.files.forEach(file => {
      formData.append('Files', file)
    })
  }
  
  // Add other fields
  formData.append('ModelName', model.modelName)
  formData.append('Manufacturer', model.manufacturer)
  formData.append('Year', model.year)
  formData.append('MaxSpeed', model.maxSpeed)
  formData.append('BatteryCapacity', model.batteryCapacity)
  formData.append('ChargingTime', model.chargingTime)
  formData.append('BatteryRange', model.batteryRange)
  formData.append('VehicleCapacity', model.vehicleCapacity)
  formData.append('Price', model.price)
  
  return request('/api/Model', {
    method: 'POST',
    body: formData,
    token
  })
}

/**
 * Update an existing model
 */
export function updateModel(id, model, token) {
  const formData = new FormData()
  
  // Add files if provided
  if (model.files && model.files.length > 0) {
    model.files.forEach(file => {
      formData.append('Files', file)
    })
  }
  
  // Add other fields
  formData.append('ModelName', model.modelName)
  formData.append('Manufacturer', model.manufacturer)
  formData.append('Year', model.year)
  formData.append('MaxSpeed', model.maxSpeed)
  formData.append('BatteryCapacity', model.batteryCapacity)
  formData.append('ChargingTime', model.chargingTime)
  formData.append('BatteryRange', model.batteryRange)
  formData.append('VehicleCapacity', model.vehicleCapacity)
  formData.append('Price', model.price)
  
  return request(`/api/Model/${id}`, {
    method: 'PUT',
    body: formData,
    token
  })
}

/**
 * Delete a model
 */
export function deleteModel(id, token) {
  return request(`/api/Model/${id}`, {
    method: 'DELETE',
    token
  })
}