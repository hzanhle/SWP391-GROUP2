import React from 'react'

// Resolve absolute/relative image URLs against API base
function resolveImageUrl(path) {
  const BASE_URL = (import.meta.env.VITE_VEHICLE_API_URL || import.meta.env.VITE_API_URL || '').replace(/\/$/, '')
  if (!path) return null
  if (/^https?:\/\//i.test(path)) return path
  return BASE_URL ? `${BASE_URL}/${path.replace(/^\//, '')}` : path
}

export default function VehicleCard(props) {
  // Supports two shapes:
  // 1) API model: { model: { modelId, modelName, manufacturer, batteryCapacity, batteryRange, vehicleCapacity, price, isActive, imageUrls } }
  // 2) Mock data used on Home: { model: string, battery: number, status: string, rate: number, image: string }

  const apiModel = typeof props.model === 'object' && props.model && 'modelName' in props.model
  const id = apiModel ? props.model.modelId : undefined
  const title = apiModel ? `${props.model.manufacturer} ${props.model.modelName}` : (props.model || '')
  const available = apiModel ? !!props.model.isActive : String(props.status || '').toLowerCase() === 'available'
  const batteryCapacity = apiModel ? props.model.batteryCapacity : props.battery
  const batteryRange = apiModel ? props.model.batteryRange : undefined
  const vehicleCapacity = apiModel ? props.model.vehicleCapacity : undefined
  const price = apiModel ? props.model.price : props.rate

  let imageUrl
  if (apiModel) {
    const imgs = Array.isArray(props.model.imageUrls) ? props.model.imageUrls : []
    imageUrl = imgs.length ? resolveImageUrl(imgs[0]) : null
  } else {
    imageUrl = resolveImageUrl(props.image)
  }

  const onBook = () => {
    if (apiModel && id != null) {
      window.location.hash = `#models/${id}`
    }
  }

  return (
    <article className="border border-gray-200 rounded-lg overflow-hidden bg-white shadow-sm hover:shadow-md transition-shadow" aria-label={title}>
      {imageUrl ? (
        <img
          src={imageUrl}
          alt={title}
          className="w-full h-56 object-contain bg-gray-50"
          loading="lazy"
          decoding="async"
          onError={(e) => {
            e.currentTarget.style.display = 'none'
            const fallback = e.currentTarget.nextElementSibling
            if (fallback) fallback.removeAttribute('hidden')
          }}
        />
      ) : null}
      <div className="w-full h-56 bg-gray-200 flex items-center justify-center" hidden={!!imageUrl}>
        <span className="text-gray-400">No Image</span>
      </div>

      <div className="p-4">
        <div className="flex justify-between items-start mb-3">
          <h3 className="text-lg font-bold text-gray-900">{title}</h3>
          <span className={`px-2 py-1 rounded text-xs font-semibold ${
            available ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-800'
          }`}>
            {available ? 'Available' : 'Unavailable'}
          </span>
        </div>

        <div className="space-y-1 mb-4 text-sm text-gray-600">
          {batteryCapacity != null && (<p>🔋 Battery: {batteryCapacity}{apiModel ? ' kWh' : '%'}</p>)}
          {batteryRange != null && (<p>📏 Range: {batteryRange} km</p>)}
          {vehicleCapacity != null && (<p>👥 Capacity: {vehicleCapacity} person(s)</p>)}
          {price != null && (<p className="font-semibold text-blue-600">💰 ${price}/day</p>)}
        </div>

        {apiModel && (
          <button
            className={`w-full py-2 px-4 rounded font-medium transition-colors ${
              available ? 'bg-blue-600 text-white hover:bg-blue-700' : 'bg-gray-300 text-gray-500 cursor-not-allowed'
            }`}
            onClick={onBook}
            disabled={!available}
          >
            {available ? 'Book Now' : 'Unavailable'}
          </button>
        )}
      </div>
    </article>
  )
}
