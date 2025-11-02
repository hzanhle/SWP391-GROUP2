import React from 'react'
import { useEffect, useState } from 'react'
import Navbar from '../components/Navbar'
import Footer from '../components/Footer'
import StationCard from '../components/StationCard'
import StationMap from '../components/StationMap'
import { getAllStations } from '../api/station'

export default function Stations() {
  const [stations, setStations] = useState([])
  const [error, setError] = useState(null)
  const [loading, setLoading] = useState(true)
  const [selectedStation, setSelectedStation] = useState(null)

  useEffect(() => {
    let mounted = true
    ;(async () => {
      try {
        setLoading(true)
        const { data } = await getAllStations()
        if (!mounted) return
        const mapped = Array.isArray(data) ? data.map(s => ({
          id: s.stationId ?? s.id ?? s.Id,
          name: s.name ?? s.Name,
          address: s.location ?? s.Location,
        })) : []
        setStations(mapped)
        setError(null)
      } catch (e) {
        if (mounted) {
          setError(e.message)
        }
      } finally {
        if (mounted) {
          setLoading(false)
        }
      }
    })()
    return () => { mounted = false }
  }, [])

  return (
    <div data-figma-layer="Stations Page">
      <Navbar />
      <main>
        <section className="stations-page-section">
          <div className="stations-page-hero">
            <div className="stations-page-hero__overlay"></div>
            <div className="container">
              <div className="stations-page-hero__content">
                <h1 className="stations-page-hero__title">Charging Stations</h1>
                <p className="stations-page-hero__subtitle">Find convenient charging stations across the city</p>
              </div>
            </div>
          </div>

          <div className="container">
            {loading && (
              <div className="text-center py-12">
                <p className="text-lg text-gray-600">Loading stations...</p>
              </div>
            )}

            {error && (
              <div className="text-center py-12">
                <div className="error-card">
                  <p className="error-text">Error: {error}</p>
                </div>
              </div>
            )}

            {!loading && !error && stations.length === 0 && (
              <div className="text-center py-12">
                <p className="text-lg text-gray-600">No stations available.</p>
              </div>
            )}

            {!loading && !error && stations.length > 0 && (
              <>
                <div className="stations-list">
                  <div className="stations-grid">
                    {stations.map((s) => (
                      <div
                        key={s.id || `${s.name}-${s.address}`}
                        style={{ cursor: 'pointer' }}
                        onClick={() => setSelectedStation(s)}
                      >
                        <StationCard {...s} isSelected={selectedStation?.id === s.id} />
                      </div>
                    ))}
                  </div>
                </div>

                <div className="container map-section">
                  <StationMap stations={stations} selectedStation={selectedStation} />
                </div>
              </>
            )}
          </div>
        </section>
      </main>
      <Footer />
    </div>
  )
}
