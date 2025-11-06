import { useEffect, useState } from 'react'
import StationCard from './StationCard'
import StationMap from './StationMap'
import { getActiveStations } from '../api/station'

export default function FeaturedStations() {
  const [stations, setStations] = useState([])
  const [error, setError] = useState(null)
  const [loading, setLoading] = useState(true)
  const [selectedStation, setSelectedStation] = useState(null)

  useEffect(() => {
    let mounted = true
    ;(async () => {
      try {
        setLoading(true)
        const { data } = await getActiveStations()
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
          setError('Failed to load stations. Please try again later.')
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
    <section className="featured-stations-section" aria-labelledby="featured-title" id="stations">
      <div className="container">
        <div className="section-header">
          <h3>Charging Stations</h3>
          <h2 id="featured-title" className="section-title">Our Stations</h2>
          <p className="section-subtitle">Find convenient charging stations across the city.</p>
        </div>
      </div>

      {loading && (
        <div className="container">
          <div className="carousel-track" aria-hidden="true">
            {[1,2,3,4].map(i => (
              <div key={i} className="skeleton skeleton-card" aria-hidden="true"></div>
            ))}
          </div>
        </div>
      )}

      {error && (
        <div className="container">
          <div role="alert" className="error-card">
            <p className="card-subtext">{error}</p>
          </div>
        </div>
      )}

      {!loading && !error && stations.length > 0 && (
        <>
          <div className="container carousel">
            <div className="carousel-track" role="list">
              {stations.map((s) => (
                <div
                  role="listitem"
                  key={s.id || `${s.name}-${s.address}`}
                  className="clickable-card"
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

      {!loading && !error && stations.length === 0 && (
        <div className="container">
          <div className="state-wrapper"><p className="state-message">No stations available.</p></div>
        </div>
      )}
    </section>
  )
}
