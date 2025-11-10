const BASE_URL = (import.meta.env.VITE_VEHICLE_API_URL || import.meta.env.VITE_API_URL || '').replace(/\/$/, '')

const VEHICLE_BASE_URL = (import.meta.env.VITE_VEHICLE_API_URL || import.meta.env.VITE_API_URL || '').replace(/\/$/, '')

async function request(path, { method = 'GET', body, token, headers = {} } = {}) {
  if (!VEHICLE_BASE_URL) {
    throw new Error('VITE_VEHICLE_API_URL or VITE_API_URL is not set')
  }
  const url = `${VEHICLE_BASE_URL}${path.startsWith('/') ? '' : '/'}${path}`

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
    const message = `Network error: failed to reach API at ${VEHICLE_BASE_URL}. If you are running the API locally, the hosted preview cannot access localhost. Use a public URL or run the frontend locally.`
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

// API Service
export const findAvailableVehicle = async (modelId, color, stationId) => {
  try {
    const response = await fetch(`/api/vehicle/find-available?modelId=${modelId}&color=${color}&stationId=${stationId}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      }
    });
    
    const data = await response.json();
    
    if (!response.ok) {
      throw new Error(data.message || 'Có lỗi xảy ra');
    }
    
    return data;
  } catch (error) {
    console.error('Error finding available vehicle:', error);
    throw error;
  }
};

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
      stationId: vehicle.stationId,
      color: vehicle.color,
      status: vehicle.status ?? 'Available',
      isActive: vehicle.isActive ?? true,
      licensePlate: vehicle.licensePlate || '',
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
      stationId: vehicle.stationId,
      color: vehicle.color,
      status: vehicle.status,
      isActive: vehicle.isActive,
      licensePlate: vehicle.licensePlate || '',
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
  formData.append('ModelCost', model.modelCost)
  formData.append('RentFeeForHour', model.rentFeeForHour)

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
  formData.append('ModelCost', model.modelCost)
  formData.append('RentFeeForHour', model.rentFeeForHour)

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

/**
 * Check vehicle availability for a date range
 */
export function checkVehicleAvailability(payload, token) {
  return request('/api/Vehicle/check-availability', {
    method: 'POST',
    body: {
      vehicleId: payload.vehicleId,
      fromDate: payload.fromDate,
      toDate: payload.toDate,
    },
    token
  })
}

/**
 * Get available vehicles by model
 */
export function getAvailableVehiclesByModel(modelId, fromDate, toDate, token) {
  const params = new URLSearchParams()
  params.append('modelId', String(modelId))
  params.append('fromDate', fromDate)
  params.append('toDate', toDate)
  return request(`/api/Vehicle/available-by-model/${modelId}?${params.toString()}`, { token })
}

/**
 * Toggle vehicle active status
 */
export function toggleVehicleActive(id, token) {
  return request(`/api/Vehicle/${id}/toggle`, {
    method: 'PATCH',
    token
  })
}
