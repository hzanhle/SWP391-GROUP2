import React from 'react'
import CTA from './CTA'

export default function StationCard({ name, address, vehicles, distance }) {
  return (
    <div className="card" data-figma-layer="StationCard" data-tailwind='class: "border border-slate-200 rounded-lg bg-white"'>
      <div className="card-body">
        <div style={{display:'flex', justifyContent:'space-between', alignItems:'center'}}>
          <div>
            <h3 style={{margin:0}}>{name}</h3>
            <p style={{margin:0, color:'#64748b', fontSize:'0.9rem'}}>{address}</p>
          </div>
          <span className="badge gray" aria-label={`${distance} km away`}>{distance} km</span>
        </div>
        <p className="vehicle-meta" aria-label={`${vehicles} vehicles available`}>{vehicles} available</p>
        <CTA as="a" href="#" variant="secondary" aria-label={`View ${name}`}>View station</CTA>
      </div>
    </div>
  )}
