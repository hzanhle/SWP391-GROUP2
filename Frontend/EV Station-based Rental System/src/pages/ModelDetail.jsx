import { useState, useEffect } from 'react'
import { getModelById, deleteModel } from '../api/vehicle'
import { resolveImageUrl } from '../utils/url'

export default function ModelDetail({ id }) {
  const [model, setModel] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
  const [currentImageIndex, setCurrentImageIndex] = useState(0)

  useEffect(() => {
    if (id) {
      loadModel()
    }
  }, [id])

  async function loadModel() {
    try {
      setLoading(true)
      // Get token from localStorage or your auth context
      const token = localStorage.getItem('token') // Adjust based on your auth implementation
      const { data } = await getModelById(id, token)
      setModel(data)
      setError(null)
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading(false)
    }
  }

  async function handleDelete() {
    if (!confirm('Are you sure you want to delete this model?')) return
    try {
      const token = localStorage.getItem('token') // Adjust based on your auth implementation
      await deleteModel(id, token)
      window.location.hash = '#models'
    } catch (err) {
      alert(err.message)
    }
  }

  function goBack() {
    window.location.hash = '#models'
  }

  function goToEdit() {
    window.location.hash = `#models/${id}/edit`
  }

  function nextImage() {
    if (model?.imageUrls?.length > 0) {
      setCurrentImageIndex((prev) => (prev + 1) % model.imageUrls.length)
    }
  }

  function prevImage() {
    if (model?.imageUrls?.length > 0) {
      setCurrentImageIndex((prev) => (prev - 1 + model.imageUrls.length) % model.imageUrls.length)
    }
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-xl">Loading...</div>
      </div>
    )
  }

  if (error) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-red-500 text-xl">Error: {error}</div>
      </div>
    )
  }

  if (!model) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-xl">Model not found</div>
      </div>
    )
  }

  return (
    <div className="container mx-auto px-4 py-8 max-w-6xl">
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <button
          onClick={goBack}
          className="px-4 py-2 text-gray-600 hover:text-gray-900"
        >
          ← Back to Models
        </button>
        <div className="flex gap-2">
          <button
            onClick={goToEdit}
            className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
          >
            Edit
          </button>
          <button
            onClick={handleDelete}
            className="px-4 py-2 bg-red-500 text-white rounded hover:bg-red-600"
          >
            Delete
          </button>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        {/* Image Gallery */}
        <div>
          <div className="bg-gray-100 rounded-lg overflow-hidden">
            {model.imageUrls && model.imageUrls.length > 0 ? (
              <div className="relative">
                <img
                  src={resolveImageUrl(model.imageUrls[currentImageIndex])}
                  alt={`${model.modelName} - Image ${currentImageIndex + 1}`}
                  className="w-full h-96 object-cover"
                  onError={(e) => { e.currentTarget.style.display = 'none' }}
                />
                {model.imageUrls.length > 1 && (
                  <>
                    <button
                      onClick={prevImage}
                      className="absolute left-2 top-1/2 -translate-y-1/2 bg-black/50 text-white p-2 rounded-full hover:bg-black/70"
                    >
                      ←
                    </button>
                    <button
                      onClick={nextImage}
                      className="absolute right-2 top-1/2 -translate-y-1/2 bg-black/50 text-white p-2 rounded-full hover:bg-black/70"
                    >
                      →
                    </button>
                    <div className="absolute bottom-4 left-1/2 -translate-x-1/2 flex gap-2">
                      {model.imageUrls.map((_, idx) => (
                        <button
                          key={idx}
                          onClick={() => setCurrentImageIndex(idx)}
                          className={`w-2 h-2 rounded-full ${
                            idx === currentImageIndex ? 'bg-white' : 'bg-white/50'
                          }`}
                        />
                      ))}
                    </div>
                  </>
                )}
              </div>
            ) : (
              <div className="w-full h-96 flex items-center justify-center text-gray-400">
                No images available
              </div>
            )}
          </div>

          {/* Thumbnail Gallery */}
          {model.imageUrls && model.imageUrls.length > 1 && (
            <div className="flex gap-2 mt-4 overflow-x-auto">
              {model.imageUrls.map((url, idx) => (
                <button
                  key={idx}
                  onClick={() => setCurrentImageIndex(idx)}
                  className={`flex-shrink-0 w-20 h-20 rounded overflow-hidden border-2 ${
                    idx === currentImageIndex ? 'border-blue-500' : 'border-gray-300'
                  }`}
                >
                  <img src={resolveImageUrl(url)} alt={`Thumbnail ${idx + 1}`} className="w-full h-full object-cover" />
                </button>
              ))}
            </div>
          )}
        </div>

        {/* Model Information */}
        <div>
          <div className="flex items-start justify-between mb-4">
            <div>
              <h1 className="text-3xl font-bold text-gray-900">{model.modelName}</h1>
              <p className="text-xl text-gray-600 mt-1">{model.manufacturer}</p>
            </div>
            <span
              className={`px-3 py-1 rounded-full text-sm font-medium ${
                model.isActive
                  ? 'bg-green-100 text-green-800'
                  : 'bg-gray-100 text-gray-800'
              }`}
            >
              {model.isActive ? 'Active' : 'Inactive'}
            </span>
          </div>

          <div className="text-3xl font-bold text-blue-600 mb-6">
            ${model.price.toLocaleString()}
          </div>

          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <InfoItem label="Year" value={model.year} />
              <InfoItem label="Max Speed" value={`${model.maxSpeed} km/h`} />
              <InfoItem label="Battery Capacity" value={`${model.batteryCapacity} kWh`} />
              <InfoItem label="Charging Time" value={`${model.chargingTime} hours`} />
              <InfoItem label="Battery Range" value={`${model.batteryRange} km`} />
              <InfoItem label="Vehicle Capacity" value={`${model.vehicleCapacity} person(s)`} />
            </div>
          </div>

          <div className="mt-8 p-4 bg-blue-50 rounded-lg">
            <h3 className="font-semibold text-gray-900 mb-2">Specifications Summary</h3>
            <ul className="space-y-1 text-sm text-gray-700">
              <li>• Model ID: {model.modelId}</li>
              <li>• Manufacturing Year: {model.year}</li>
              <li>• Range per charge: {model.batteryRange} km</li>
              <li>• Full charge time: {model.chargingTime} hours</li>
            </ul>
          </div>
        </div>
      </div>
    </div>
  )
}

function InfoItem({ label, value }) {
  return (
    <div className="bg-gray-50 p-4 rounded-lg">
      <div className="text-sm text-gray-600 mb-1">{label}</div>
      <div className="text-lg font-semibold text-gray-900">{value}</div>
    </div>
  )
}
