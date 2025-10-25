import React from 'react'
import { useEffect, useState } from 'react'
import Navbar from '../components/Navbar'
import Footer from '../components/Footer'
import SearchBar from '../components/SearchBar'
import StationCard from '../components/StationCard'
import { getAllStations } from '../api/station'

export default function Stations() {
  const [stations, setStations] = useState([])
  const [error, setError] = useState(null)

  useEffect(() => {
    let mounted = true
    ;(async () => {
      try {
        const { data } = await getAllStations()
        if (!mounted) return
        const mapped = Array.isArray(data) ? data.map(s => ({
          id: s.stationId ?? s.id ?? s.Id,
          name: s.name ?? s.Name,
          address: s.location ?? s.Location,
        })) : []
        setStations(mapped)
      } catch (e) {
        setError(e.message)
      }
    })()
    return () => { mounted = false }
  }, [])

  return (
    <div data-figma-layer="Stations Page">
      <Navbar />
      <main>
        <SearchBar />
        <section id="stations-list" className="section" aria-labelledby="stations-title">
          <div className="container">
            <div className="section-header">
              <h2 id="stations-title" className="section-title">Điểm thuê gần bạn</h2>
              <p className="section-subtitle">Chọn điểm để xem xe có sẵn và đặt xe.</p>
            </div>

            {error ? (
              <div role="alert" className="card card-body">
                <p className="card-subtext">{error}</p>
              </div>
            ) : (
              <div className="vehicle-grid">
                {stations.map((s) => (
                  <StationCard key={`${s.name}-${s.address}`} {...s} />
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
