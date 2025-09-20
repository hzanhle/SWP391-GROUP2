import React from 'react'
import Navbar from '../components/Navbar'
import Footer from '../components/Footer'
import VehicleCard from '../components/VehicleCard'

const vehicles = [
  { model: 'Tesla Model 3', battery: 82, status: 'Available', rate: 12, image: '' },
  { model: 'Nissan Leaf', battery: 56, status: 'Booked', rate: 9, image: '' },
  { model: 'Hyundai Ioniq 5', battery: 73, status: 'Available', rate: 11, image: '' },
  { model: 'Kia EV6', battery: 65, status: 'Available', rate: 10, image: '' },
]

export default function Vehicles() {
  return (
    <div data-figma-layer="Vehicles Page">
      <Navbar />
      <main>
        <section id="vehicles" className="section" aria-labelledby="vehicles-title">
          <div className="container">
            <div className="section-header">
              <h1 id="vehicles-title" className="section-title">Xe có sẵn</h1>
              <p className="section-subtitle">Chọn xe phù hợp và tiến hành đặt.</p>
            </div>
            <div className="vehicle-grid">
              {vehicles.map((v) => (
                <VehicleCard key={v.model} {...v} />
              ))}
            </div>
          </div>
        </section>
      </main>
      <Footer />
    </div>
  )
}
