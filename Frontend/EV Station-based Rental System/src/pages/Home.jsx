import React from 'react'
import Navbar from '../components/Navbar'
import Hero from '../components/Hero'
import SearchBar from '../components/SearchBar'
import FeaturedStations from '../components/FeaturedStations'
import HowItWorks from '../components/HowItWorks'
import Testimonials from '../components/Testimonials'
import VehicleCard from '../components/VehicleCard'
import Footer from '../components/Footer'

const vehicles = [
  { model: 'Tesla Model 3', battery: 82, status: 'Available', rate: 12, image: '' },
  { model: 'Nissan Leaf', battery: 56, status: 'Booked', rate: 9, image: '' },
  { model: 'Hyundai Ioniq 5', battery: 73, status: 'Available', rate: 11, image: '' },
  { model: 'Kia EV6', battery: 65, status: 'Available', rate: 10, image: '' },
]

export default function Home() {
  return (
    <div data-figma-layer="Home" data-tailwind='class: "bg-slate-50"'>
      <Navbar />
      <main>
        <Hero />
        <section id="stations" className="section" aria-labelledby="vehicles-title" data-figma-layer="Vehicle availability list" data-tailwind='class: "py-10"'>
          <div className="container">
            <div className="section-header">
              <h2 id="vehicles-title" className="section-title">Available vehicles</h2>
              <p className="section-subtitle">Pick a ride that fits your trip.</p>
            </div>
            <div className="vehicle-grid">
              {vehicles.map((v) => (
                <VehicleCard key={v.model} {...v} />
              ))}
            </div>
          </div>
        </section>
        <SearchBar />
        <HowItWorks />
        <FeaturedStations />
        <Testimonials />
      </main>
      <Footer />
    </div>
  )
}
