import React from 'react'
import CTA from './CTA'

export default function StationCard({ name, address, vehicles, distance }) {
  return (
    <div className="card" data-figma-layer="StationCard" data-tailwind='class: "border border-slate-200 rounded-lg bg-white"'>
      <div className="card-body">
        <div className="row-between">
          <div>
            <h3 className="card-title">{name}</h3>
            <p className="card-subtext">{address}</p>
          </div>
          <span className="badge gray" aria-label={`${distance} km away`}>{distance} km</span>
        </div>
        <p className="vehicle-meta" aria-label={`${vehicles} vehicles available`}>{vehicles} available</p>
        <CTA as="a" href="#" variant="secondary" aria-label={`View ${name}`}>View station</CTA>
      </div>
    </div>
  )}
