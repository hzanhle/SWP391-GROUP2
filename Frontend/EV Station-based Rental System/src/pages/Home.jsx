import React, { useEffect, useState } from 'react'
import Navbar from '../components/Navbar'
import Hero from '../components/Hero'
import SearchBar from '../components/SearchBar'
import FeaturedStations from '../components/FeaturedStations'
import HowItWorks from '../components/HowItWorks'
import Testimonials from '../components/Testimonials'
import VehicleCard from '../components/VehicleCard'
import Footer from '../components/Footer'
import { getActiveModels } from '../api/vehicle'

export default function Home() {
  const [models, setModels] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)

  useEffect(() => {
    let mounted = true
    async function load() {
      try {
        setLoading(true)
        setError(null)
        const token = typeof window !== 'undefined' ? localStorage.getItem('auth.token') : null
        const { data } = await getActiveModels(token)
        if (mounted) setModels(Array.isArray(data) ? data : [])
      } catch (e) {
        if (mounted) setError(e.message || 'Failed to load vehicles')
      } finally {
        if (mounted) setLoading(false)
      }
    }
    load()
    return () => { mounted = false }
  }, [])

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

            {loading && (
              <div className="text-center py-10 fade-in">
                <div className="spinner" aria-hidden="true"></div>
                <p className="text-gray-600 mt-3">Loading vehicles...</p>
              </div>
            )}

            {error && (
              <div className="text-center py-10 fade-in" role="alert">
                <p className="text-red-600">{error}</p>
              </div>
            )}

            {!loading && !error && models.length === 0 && (
              <div className="text-center py-10 fade-in">
                <p className="text-gray-600">No vehicles available.</p>
              </div>
            )}

            {!loading && !error && models.length > 0 && (
              <div className="vehicle-grid fade-in">
                {models.map((m) => (
                  <div key={m.modelId} className="slide-up"><VehicleCard model={m} /></div>
                ))}
              </div>
            )}
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
