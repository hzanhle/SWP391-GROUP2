import React, { useState, useEffect } from 'react'
import Navbar from '../components/Navbar'
import Footer from '../components/Footer'
import VehicleCard from '../components/VehicleCard'
import { getAllModels } from '../api/vehicle'

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
      const token = localStorage.getItem('token') // Adjust based on your auth
      const { data } = await getAllModels(token)
      console.log('Models loaded:', data) // Debug log
      setModels(data)
      setError(null)
    } catch (err) {
      console.error('Error loading models:', err) // Debug log
      setError(err.message)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div data-figma-layer="Vehicles Page">
      <Navbar />
      <main>
        <section id="vehicles" className="section" aria-labelledby="vehicles-title">
          <div className="container">
            <div className="section-header">
              <h1 id="vehicles-title" className="section-title">Xe có sẵn</h1>
              <p className="section-subtitle">Chọn xe phù hợp và tiến hành đặt.</p>
            </div>

            {loading && (
              <div className="text-center py-8">
                <p className="text-lg text-gray-600">Đang tải...</p>
              </div>
            )}

            {error && (
              <div className="text-center py-8">
                <p className="text-lg text-red-600">Lỗi: {error}</p>
                <button 
                  onClick={loadModels}
                  className="mt-4 px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
                >
                  Thử lại
                </button>
              </div>
            )}

            {!loading && !error && models.length === 0 && (
              <div className="text-center py-8">
                <p className="text-lg text-gray-600">Không có xe nào.</p>
              </div>
            )}

            {!loading && !error && models.length > 0 && (
              <div className="vehicle-grid">
                {models
                  .filter(model => model.isActive)
                  .map((model) => (
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