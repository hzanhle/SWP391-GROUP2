import React from 'react'
import StationCard from './StationCard'

const stations = [
  { name: 'Central Hub', address: '123 Main St', vehicles: 6, distance: 1.2 },
  { name: 'Riverside', address: '45 River Rd', vehicles: 3, distance: 3.8 },
  { name: 'Airport West', address: 'Terminal 2', vehicles: 9, distance: 7.4 },
  { name: 'Tech Park', address: '88 Innovation Ave', vehicles: 2, distance: 5.1 },
]

export default function FeaturedStations() {
  return (
    <section className="section" aria-labelledby="featured-title" data-figma-layer="Featured stations carousel" data-tailwind='class: "py-10"'>
      <div className="container carousel">
        <div className="section-header">
          <h2 id="featured-title" className="section-title">Featured stations</h2>
          <p className="section-subtitle">Trusted locations across the city.</p>
        </div>
        <div className="carousel-track" role="list">
          {stations.map((s) => (
            <div role="listitem" key={s.name}>
              <StationCard {...s} />
            </div>
          ))}
        </div>
      </div>
    </section>
  )
}
