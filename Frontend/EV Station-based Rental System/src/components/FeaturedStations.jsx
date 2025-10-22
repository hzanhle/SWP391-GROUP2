import { useEffect, useState } from 'react'
import StationCard from './StationCard'
import { getActiveStations } from '../api/station'

export default function FeaturedStations() {
  const [stations, setStations] = useState([])
  const [error, setError] = useState(null)

  useEffect(() => {
    let mounted = true
    ;(async () => {
      try {
        const { data } = await getActiveStations()
        if (!mounted) return
        const mapped = Array.isArray(data) ? data.map(s => ({
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
    <section className="section" aria-labelledby="featured-title" data-figma-layer="Featured stations carousel" data-tailwind='class: "py-10"'>
      <div className="container carousel">
        <div className="section-header">
          <h2 id="featured-title" className="section-title">Featured stations</h2>
          <p className="section-subtitle">Trusted locations across the city.</p>
        </div>
        {error ? (
          <div role="alert" className="card card-body">
            <p className="card-subtext">{error}</p>
          </div>
        ) : (
          <div className="carousel-track" role="list">
            {stations.map((s) => (
              <div role="listitem" key={`${s.name}-${s.address}`}>
                <StationCard {...s} />
              </div>
            ))}
          </div>
        )}
      </div>
    </section>
  )
}
