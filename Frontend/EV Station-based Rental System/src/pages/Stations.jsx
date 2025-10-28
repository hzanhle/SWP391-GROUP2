import React from 'react'
import { useEffect, useState } from 'react'
import Navbar from '../components/Navbar'
import Footer from '../components/Footer'
import StationCard from '../components/StationCard'
import { getAllStations } from '../api/station'

export default function Stations() {
  const [stations, setStations] = useState([])
  const [error, setError] = useState(null)

  useEffect(() => {
    let mounted = true
    ;(async () => {
      try {
        const { data } = await getAllStations()
        if (!mounted) return
        const mapped = Array.isArray(data) ? data.map(s => ({
          id: s.stationId ?? s.id ?? s.Id,
          name: s.name ?? s.Name,
          address: s.location ?? s.Location,
        })) : []
        setStations(mapped)
      } catch (e) {
        setError(e.message)
      }
    })()
    return () => { mounted = false }
  }, [])

  return (
    <div data-figma-layer="Stations Page">
      <Navbar />
      <main>
      
      </main>
      <Footer />
    </div>
  )
}
