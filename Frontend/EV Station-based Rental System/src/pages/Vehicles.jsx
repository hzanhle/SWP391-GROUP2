import React, { useState, useEffect } from 'react'
import Navbar from '../components/Navbar'
import Footer from '../components/Footer'
import { getAllModels } from '../api/vehicle'

export default function Vehicles() {
  const [models, setModels] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
  const [selectedModel, setSelectedModel] = useState(null)
  const apiBaseUrl = (import.meta.env.VITE_VEHICLE_API_URL || import.meta.env.VITE_API_URL || '').replace(/\/$/, '')

  useEffect(() => {
    loadModels()
  }, [])

  async function loadModels() {
    try {
      setLoading(true)
      const { data } = await getAllModels()
      setModels(Array.isArray(data) ? data : [])
      setError(null)
    } catch (err) {
      console.error('Error loading models:', err)
      setError(err.message)
    } finally {
      setLoading(false)
    }
  }

  const formatVND = (amount) => {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(amount)
  }

  const getImageUrl = (model) => {
    const placeholderImg = "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='400' height='300'%3E%3Crect fill='%23ff4d30' width='400' height='300'/%3E%3C/svg%3E"

    if (!Array.isArray(model.imageUrls || model.ImageUrls) || (model.imageUrls || model.ImageUrls).length === 0) {
      return placeholderImg
    }

    const imagePath = model.imageUrls[0]
    if (imagePath.startsWith('http')) {
      return imagePath
    }

    return `${apiBaseUrl}/api/Model/image/${imagePath}`
  }

  const handleCloseModal = () => {
    setSelectedModel(null)
  }

  const handleModalBackdropClick = (e) => {
    if (e.target === e.currentTarget) {
      handleCloseModal()
    }
  }

  return (
    <div data-figma-layer="Vehicles Page">
      <Navbar />
      <main>
        <section className="models-section">
          <div className="models-page-hero">
            <div className="models-page-hero__overlay"></div>
            <div className="container">
              <div className="models-page-hero__content">
                <h1 className="models-page-hero__title">Electric Vehicle Models</h1>
                <p className="models-page-hero__subtitle">Choose the perfect EV for your needs</p>
              </div>
            </div>
          </div>

          <div className="container">
            {loading && (
              <div className="text-center py-12">
                <p className="text-lg text-gray-600">Loading vehicles...</p>
              </div>
            )}

            {error && (
              <div className="text-center py-12">
                <div className="error-card">
                  <p className="error-text">Error: {error}</p>
                  <button
                    onClick={loadModels}
                    className="retry-button"
                  >
                    Try Again
                  </button>
                </div>
              </div>
            )}

            {!loading && !error && models.length === 0 && (
              <div className="text-center py-12">
                <p className="text-lg text-gray-600">No vehicles available.</p>
              </div>
            )}

            {!loading && !error && models.length > 0 && (
              <div className="models-div">
                {models
                  .filter(model => model.isActive)
                  .map((model) => (
                    <div key={model.modelId} className="models-div__box">
                      <div className="models-div__box__img">
                        <img
                          src={getImageUrl(model)}
                          alt={`${model.manufacturer || model.Manufacturer || ''} ${model.modelName || model.ModelName || ''}`.trim()}
                        />
                        <div className="models-div__box__descr">
                          <div className="models-div__box__descr__name-price">
                            <div className="models-div__box__descr__name-price__name">
                              <p>{(model.manufacturer || model.Manufacturer)} {(model.modelName || model.ModelName)}</p>
                              <span>
                                <i className="fa-solid fa-star"></i>
                                <i className="fa-solid fa-star"></i>
                                <i className="fa-solid fa-star"></i>
                                <i className="fa-solid fa-star"></i>
                                <i className="fa-solid fa-star"></i>
                              </span>
                            </div>
                            <div className="models-div__box__descr__name-price__price">
                              <h4>{formatVND(model.rentFeeForHour ?? model.RentFeeForHour ?? 0)}</h4>
                              <p>per hour</p>
                            </div>
                          </div>
                          <div className="models-div__box__descr__name-price__details">
                            <span>
                              <i className="fa-solid fa-car-side"></i> &nbsp; {model.manufacturer}
                            </span>
                            <span style={{ textAlign: "right" }}>
                              {(model.vehicleCapacity ?? model.VehicleCapacity ?? '')} seats &nbsp; <i className="fa-solid fa-car-side"></i>
                            </span>
                            <span>
                              <i className="fa-solid fa-car-side"></i> &nbsp; Automatic
                            </span>
                            <span style={{ textAlign: "right" }}>
                              Electric &nbsp; <i className="fa-solid fa-car-side"></i>
                            </span>
                          </div>
                          <div className="models-div__box__descr__name-price__btn">
                            <button
                              onClick={() => setSelectedModel(model)}
                              style={{
                                background: 'none',
                                border: 'none',
                                cursor: 'pointer',
                                padding: 0,
                                color: 'inherit',
                              }}
                            >
                              <a href="#" onClick={(e) => {
                                e.preventDefault()
                                setSelectedModel(model)
                              }} style={{ textDecoration: 'none' }}>
                                View Details
                              </a>
                            </button>
                          </div>
                        </div>
                      </div>
                    </div>
                  ))}
              </div>
            )}
          </div>

          <div className="book-banner">
            <div className="book-banner__overlay"></div>
            <div className="container">
              <div className="text-content">
                <h2>Ready to start your EV journey?</h2>
                <span>
                  <i className="fa-solid fa-phone"></i>
                  <h3>(+84) 901-234-567</h3>
                </span>
              </div>
            </div>
          </div>
        </section>
      </main>

      {selectedModel && (
        <div
          className="vehicle-detail-modal"
          onClick={handleModalBackdropClick}
          style={{
            position: 'fixed',
            top: 0,
            left: 0,
            right: 0,
            bottom: 0,
            backgroundColor: 'rgba(0, 0, 0, 0.5)',
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center',
            zIndex: 1000,
            padding: '2rem',
            overflow: 'auto',
          }}
        >
          <div
            className="vehicle-detail-content"
            style={{
              backgroundColor: 'white',
              borderRadius: '12px',
              padding: '2rem',
              maxWidth: '900px',
              width: '100%',
              maxHeight: '90vh',
              overflow: 'auto',
              boxShadow: '0 10px 40px rgba(0, 0, 0, 0.2)',
            }}
          >
            <div
              style={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                marginBottom: '1.5rem',
              }}
            >
              <h2 style={{ margin: 0, fontSize: '2rem' }}>
                {selectedModel.manufacturer} {selectedModel.modelName}
              </h2>
              <button
                onClick={handleCloseModal}
                style={{
                  background: 'none',
                  border: 'none',
                  fontSize: '1.8rem',
                  cursor: 'pointer',
                  color: '#666',
                  padding: 0,
                }}
              >
                ✕
              </button>
            </div>

            <div
              style={{
                display: 'grid',
                gridTemplateColumns: '1fr 1fr',
                gap: '2rem',
                marginBottom: '2rem',
              }}
            >
              <div>
                <img
                  src={getImageUrl(selectedModel)}
                  alt={`${selectedModel.manufacturer} ${selectedModel.modelName}`}
                  style={{
                    width: '100%',
                    height: 'auto',
                    borderRadius: '8px',
                    objectFit: 'cover',
                  }}
                />
              </div>

              <div>
                <div style={{ marginBottom: '1.5rem' }}>
                  <h3 style={{ color: '#666', marginBottom: '0.5rem' }}>Rental Rate</h3>
                  <p style={{ fontSize: '1.8rem', fontWeight: '600', color: '#ff4d30' }}>
                    {formatVND(selectedModel.rentFeeForHour)} / hour
                  </p>
                </div>

                <div style={{ marginBottom: '1.5rem' }}>
                  <h3 style={{ color: '#666', marginBottom: '0.5rem' }}>Vehicle Type</h3>
                  <p style={{ fontSize: '1.1rem' }}>Electric Vehicle</p>
                </div>

                <div style={{ marginBottom: '1.5rem' }}>
                  <h3 style={{ color: '#666', marginBottom: '0.5rem' }}>Seating Capacity</h3>
                  <p style={{ fontSize: '1.1rem' }}>{selectedModel.vehicleCapacity} seats</p>
                </div>

                <div style={{ marginBottom: '1.5rem' }}>
                  <h3 style={{ color: '#666', marginBottom: '0.5rem' }}>Transmission</h3>
                  <p style={{ fontSize: '1.1rem' }}>Automatic</p>
                </div>

                <div style={{ marginBottom: '1.5rem' }}>
                  <h3 style={{ color: '#666', marginBottom: '0.5rem' }}>Manufacturer</h3>
                  <p style={{ fontSize: '1.1rem' }}>{selectedModel.manufacturer}</p>
                </div>

                {selectedModel.description && (
                  <div style={{ marginBottom: '1.5rem' }}>
                    <h3 style={{ color: '#666', marginBottom: '0.5rem' }}>Description</h3>
                    <p style={{ fontSize: '0.95rem', lineHeight: '1.6', color: '#555' }}>
                      {selectedModel.description}
                    </p>
                  </div>
                )}
              </div>
            </div>

            <div style={{ borderTop: '1px solid #eee', paddingTop: '1.5rem' }}>
              <button
                onClick={handleCloseModal}
                style={{
                  padding: '0.75rem 1.5rem',
                  marginRight: '1rem',
                  backgroundColor: '#f5f5f5',
                  border: '1px solid #ddd',
                  borderRadius: '6px',
                  cursor: 'pointer',
                  fontSize: '1rem',
                  fontWeight: '500',
                }}
              >
                Close
              </button>
              <a
                href="#booking-new"
                onClick={(e) => {
                  e.preventDefault()
                  handleCloseModal()
                  localStorage.setItem('selected_model', JSON.stringify({
                    modelId: selectedModel.modelId,
                    modelName: selectedModel.modelName,
                    manufacturer: selectedModel.manufacturer,
                    rentFeeForHour: selectedModel.rentFeeForHour,
                    vehicleCapacity: selectedModel.vehicleCapacity,
                  }))
                  window.location.hash = 'booking-new'
                }}
                style={{
                  padding: '0.75rem 1.5rem',
                  backgroundColor: '#ff4d30',
                  color: 'white',
                  border: 'none',
                  borderRadius: '6px',
                  cursor: 'pointer',
                  fontSize: '1rem',
                  fontWeight: '500',
                  textDecoration: 'none',
                  display: 'inline-block',
                }}
              >
                Book This Vehicle
              </a>
            </div>
          </div>
        </div>
      )}

      <Footer />
    </div>
  )
}
