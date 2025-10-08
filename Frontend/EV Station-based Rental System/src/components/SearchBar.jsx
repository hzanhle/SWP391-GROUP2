import React from 'react'
import CTA from './CTA'

export default function SearchBar() {
  return (
    <section id="search" className="section" data-figma-layer="SearchBar" data-tailwind='class: "py-10"'>
      <div className="container search-grid">
        <div className="search-panel card card-body" aria-labelledby="search-title">
          <div className="section-header">
            <h2 id="search-title" className="section-title" data-figma-layer="H2" data-tailwind='class: "text-3xl font-bold"'>Search stations</h2>
            <p className="section-subtitle" data-tailwind='class: "text-slate-600"'>Choose location and filters to find the best match.</p>
          </div>

          <div className="field" data-figma-layer="LocationField" data-tailwind='class: "grid gap-2"'>
            <label className="label" htmlFor="location">Location</label>
            <input id="location" className="input" placeholder="Enter a city or address" list="locations" aria-autocomplete="list" aria-label="Location"
              data-tailwind='class: "w-full p-3 border border-slate-200 rounded-md bg-white"' />
            <datalist id="locations">
              <option value="Downtown" />
              <option value="Airport" />
              <option value="City Center" />
            </datalist>
          </div>

          <div className="field" data-figma-layer="DistanceField">
            <label className="label" htmlFor="distance">Distance (km)</label>
            <input id="distance" type="range" min="1" max="50" defaultValue="10" aria-label="Distance filter" className="input"
              data-tailwind='class: "w-full"' />
          </div>

          <div className="field" data-figma-layer="BatteryField">
            <label className="label" htmlFor="battery">Min battery %</label>
            <select id="battery" className="select" aria-label="Battery level filter" data-tailwind='class: "w-full p-3 border border-slate-200 rounded-md bg-white"'>
              <option>Any</option>
              <option>50%</option>
              <option>75%</option>
              <option>90%</option>
            </select>
          </div>

          <div className="field" data-figma-layer="ChargerField">
            <label className="label" htmlFor="charger">Charger type</label>
            <select id="charger" className="select" aria-label="Charger type filter" data-tailwind='class: "w-full p-3 border border-slate-200 rounded-md bg-white"'>
              <option>Any</option>
              <option>Type 2</option>
              <option>CCS</option>
              <option>CHAdeMO</option>
            </select>
          </div>

          <CTA as="button" aria-label="Apply filters">Apply filters</CTA>
        </div>

        <div className="map-card" role="img" aria-label="Map preview" data-figma-layer="Map" data-tailwind='class: "min-h-[260px] rounded-lg border border-slate-200 bg-[repeating-linear-gradient(45deg,#f8fafc,#f8fafc_10px,#eef2f7_10px,#eef2f7_20px)] shadow-sm"'></div>
      </div>
    </section>
  )
}
