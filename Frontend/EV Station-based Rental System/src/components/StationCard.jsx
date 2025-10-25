import React from 'react'
import CTA from './CTA'

export default function StationCard({ id, name, address, vehicles, distance }) {
  function handleView(e) {
    e.preventDefault()
    try {
      if (id != null) {
        const qs = { stationId: Number(id) }
        localStorage.setItem('quick_search_data', JSON.stringify(qs))
      }
    } catch {}
    window.location.hash = 'booking-new'
  }
  return (
    <div className="card" data-figma-layer="StationCard" data-tailwind='class: "border border-slate-200 rounded-lg bg-white"'>
      <div className="card-body">
        <div className="row-between">
          <div>
            <h3 className="card-title">{name}</h3>
            <p className="card-subtext">{address}</p>
          </div>
          {distance != null && (
            <span className="badge gray" aria-label={`${distance} km away`}>{distance} km</span>
          )}
        </div>
        {vehicles != null && (
          <p className="vehicle-meta" aria-label={`${vehicles} vehicles available`}>{vehicles} available</p>
        )}
        <CTA as="a" href="#booking-new" onClick={handleView} variant="secondary" aria-label={`View ${name}`}>View station</CTA>
      </div>
    </div>
  )}
