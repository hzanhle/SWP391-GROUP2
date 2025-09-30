import React, { useState, useEffect } from 'react'

// Mock components - replace with your actual imports
const Navbar = () => <nav className="bg-blue-600 text-white p-4">Navbar</nav>
const Footer = () => <footer className="bg-gray-800 text-white p-4 mt-8">Footer</footer>

const VehicleCard = ({ model }) => {
  const {
    modelId,
    modelName,
    manufacturer,
    batteryCapacity,
    batteryRange,
    vehicleCapacity,
    isActive,
    price,
    imageUrls
  } = model

  const BASE_URL = (import.meta.env.VITE_VEHICLE_API_URL || '').replace(/\/$/, '')
  
  const getImageUrl = (path) => {
    if (!path) return null
    if (path.startsWith('http')) return path
    return `${BASE_URL}/${path}`
  }
  
  const image = imageUrls && imageUrls.length > 0 ? getImageUrl(imageUrls[0]) : null
  const available = isActive

  function handleBook() {
    window.location.hash = `#models/${modelId}`
  }

  return (
    <article className="border border-gray-200 rounded-lg overflow-hidden bg-white shadow-sm hover:shadow-md transition-shadow">
      {image ? (
        <img 
          src={image} 
          alt={`${manufacturer} ${modelName}`} 
          className="w-full h-56 object-contain bg-gray-50"
          onError={(e) => {
            e.target.style.display = 'none'
            e.target.nextSibling.style.display = 'flex'
          }}
        />
      ) : null}
      <div 
        className="w-full h-56 bg-gray-200 flex items-center justify-center"
        style={{ display: image ? 'none' : 'flex' }}
      >
        <span className="text-gray-400">No Image</span>
      </div>
      
      <div className="p-4">
        <div className="flex justify-between items-start mb-3">
          <h3 className="text-lg font-bold text-gray-900">
            {manufacturer} {modelName}
          </h3>
          <span className={`px-2 py-1 rounded text-xs font-semibold ${
            available ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-800'
          }`}>
            {available ? 'Available' : 'Unavailable'}
          </span>
        </div>
        
        <div className="space-y-1 mb-4 text-sm text-gray-600">
          <p>🔋 Battery: {batteryCapacity} kWh</p>
          <p>📏 Range: {batteryRange} km</p>
          <p>👥 Capacity: {vehicleCapacity} person(s)</p>
          <p className="font-semibold text-blue-600">💰 ${price}/day</p>
        </div>
        
        <button 
          className={`w-full py-2 px-4 rounded font-medium transition-colors ${
            available 
              ? 'bg-blue-600 text-white hover:bg-blue-700' 
              : 'bg-gray-300 text-gray-500 cursor-not-allowed'
          }`}
          onClick={handleBook}
          disabled={!available}
        >
          {available ? 'Book Now' : 'Unavailable'}
        </button>
      </div>
    </article>
  )
}

async function getAllModels(token) {
  const BASE_URL = (import.meta.env.VITE_VEHICLE_API_URL || '').replace(/\/$/, '')
  
  if (!BASE_URL) {
    throw new Error('VITE_VEHICLE_API_URL chưa được cấu hình trong file .env')
  }

  const url = `${BASE_URL}/api/Model`

  const headers = {
    'Accept': 'application/json',
    'Content-Type': 'application/json',
  }

  if (token) {
    headers['Authorization'] = `Bearer ${token}`
  }

  const res = await fetch(url, {
    method: 'GET',
    headers,
  })

  if (!res.ok) {
    throw new Error(`API Error: ${res.status} ${res.statusText}`)
  }

  const data = await res.json()
  return { status: res.status, data }
}

export default function Vehicles() {
  const [models, setModels] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)

  useEffect(() => {
    loadModels()
  }, [])

  async function loadModels() {
    try {
      setLoading(true)
      setError(null)
      
      const token = localStorage.getItem('token')
      const { data } = await getAllModels(token)
      
      setModels(Array.isArray(data) ? data : [])
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading(false)
    }
  }

  const activeModels = models.filter(model => model.isActive)

  return (
    <div className="min-h-screen flex flex-col bg-gray-50">
      <Navbar />
      
      <main className="flex-1">
        <section className="py-12 px-4">
          <div className="max-w-7xl mx-auto">
            <div className="text-center mb-12">
              <h1 className="text-4xl font-bold text-gray-900 mb-4">Xe có sẵn</h1>
              <p className="text-lg text-gray-600">Chọn xe phù hợp và tiến hành đặt.</p>
            </div>

            {loading && (
              <div className="text-center py-16">
                <div className="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
                <p className="mt-4 text-lg text-gray-600">Đang tải dữ liệu...</p>
              </div>
            )}

            {error && (
              <div className="text-center py-16">
                <div className="max-w-md mx-auto bg-red-50 border border-red-200 rounded-lg p-6">
                  <p className="text-2xl mb-2">❌</p>
                  <p className="text-lg font-semibold text-red-900 mb-2">Lỗi kết nối</p>
                  <p className="text-sm text-red-700 mb-4">{error}</p>
                  <button 
                    onClick={loadModels}
                    className="px-6 py-2 bg-red-600 text-white rounded hover:bg-red-700 font-medium"
                  >
                    Thử lại
                  </button>
                </div>
              </div>
            )}

            {!loading && !error && models.length === 0 && (
              <div className="text-center py-16">
                <p className="text-6xl mb-4">🚗</p>
                <p className="text-xl text-gray-600">Không có xe nào trong hệ thống.</p>
              </div>
            )}

            {!loading && !error && models.length > 0 && activeModels.length === 0 && (
              <div className="text-center py-16">
                <p className="text-6xl mb-4">⚠️</p>
                <p className="text-xl text-gray-600">Hiện tại không có xe nào available.</p>
                <p className="text-sm text-gray-500 mt-2">Tổng số xe: {models.length}</p>
              </div>
            )}

            {!loading && !error && activeModels.length > 0 && (
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                {activeModels.map((model) => (
                  <VehicleCard key={model.modelId} model={model} />
                ))}
              </div>
            )}
          </div>
        </section>
      </main>
      
      <Footer />
    </div>
  )
}