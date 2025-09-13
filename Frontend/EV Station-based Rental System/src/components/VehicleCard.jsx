import React from 'react'
import CTA from './CTA'

export default function VehicleCard({ image, model, battery, status, rate }) {
  const available = status === 'Available'
  return (
    <article className="card" data-figma-layer="VehicleCard" data-tailwind='class: "border border-slate-200 rounded-lg overflow-hidden bg-white"'>
      {image ? (
        <img src={image} alt="Vehicle" className="vehicle-media" />
      ) : (
        <div className="vehicle-media" role="img" aria-label="Vehicle image placeholder" style={{background:'#e2e8f0'}} />
      )}
      <div className="card-body">
        <div style={{display:'flex', justifyContent:'space-between', alignItems:'center'}}>
          <h3 className="vehicle-title">{model}</h3>
          <span className={available ? 'badge green' : 'badge gray'}>{status}</span>
        </div>
        <p className="vehicle-meta">Battery: {battery}%</p>
        <p className="vehicle-meta">Rate: ${rate}/hr</p>
        <CTA as="button" variant={available ? 'primary' : 'secondary'} aria-disabled={!available} aria-label={`Book ${model}`}>
          Book
        </CTA>
      </div>
    </article>
  )
}
