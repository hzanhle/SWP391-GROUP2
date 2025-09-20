import React from 'react'
import Navbar from '../components/Navbar'
import Footer from '../components/Footer'
import SearchBar from '../components/SearchBar'
import StationCard from '../components/StationCard'

const stations = [
  { name: 'Central Hub', address: '123 Main St', vehicles: 8, distance: 1.2 },
  { name: 'Riverside', address: '45 River Rd', vehicles: 5, distance: 3.4 },
  { name: 'Airport West', address: 'Terminal 2', vehicles: 12, distance: 7.8 },
]

export default function Stations() {
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

            <div className="vehicle-grid">
              {stations.map((s) => (
                <StationCard key={s.name} {...s} />
              ))}
            </div>
          </div>
        </section>
      </main>
      <Footer />
    </div>
  )
}
