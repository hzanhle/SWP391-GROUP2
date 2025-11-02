import React, { useState, useEffect } from 'react'
import Navbar from '../components/Navbar'
import Footer from '../components/Footer'
import { getAllModels } from '../api/vehicle'

export default function Vehicles() {
  const [models, setModels] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
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

  const getImageUrl = (model) => {
    const placeholderImg = "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='400' height='300'%3E%3Crect fill='%23ff4d30' width='400' height='300'/%3E%3C/svg%3E"

    if (!model.imageUrls || model.imageUrls.length === 0) {
      return placeholderImg
    }

    const imagePath = model.imageUrls[0]
    if (imagePath.startsWith('http')) {
      return imagePath
    }

    return `${apiBaseUrl}/api/Model/image/${imagePath}`
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
                          alt={`${model.manufacturer} ${model.modelName}`}
                        />
                        <div className="models-div__box__descr">
                          <div className="models-div__box__descr__name-price">
                            <div className="models-div__box__descr__name-price__name">
                              <p>{model.manufacturer} {model.modelName}</p>
                              <span>
                                <i className="fa-solid fa-star"></i>
                                <i className="fa-solid fa-star"></i>
                                <i className="fa-solid fa-star"></i>
                                <i className="fa-solid fa-star"></i>
                                <i className="fa-solid fa-star"></i>
                              </span>
                            </div>
                            <div className="models-div__box__descr__name-price__price">
                              <h4>${model.rentFeeForHour}</h4>
                              <p>per hour</p>
                            </div>
                          </div>
                          <div className="models-div__box__descr__name-price__details">
                            <span>
                              <i className="fa-solid fa-car-side"></i> &nbsp; {model.manufacturer}
                            </span>
                            <span style={{ textAlign: "right" }}>
                              {model.vehicleCapacity} seats &nbsp; <i className="fa-solid fa-car-side"></i>
                            </span>
                            <span>
                              <i className="fa-solid fa-car-side"></i> &nbsp; Automatic
                            </span>
                            <span style={{ textAlign: "right" }}>
                              Electric &nbsp; <i className="fa-solid fa-car-side"></i>
                            </span>
                          </div>
                          <div className="models-div__box__descr__name-price__btn">
                            <a href="#booking-section" onClick={() => window.scrollTo(0, 0)}>
                              Book Ride
                            </a>
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
      <Footer />
    </div>
  )
}
