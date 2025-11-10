import { useEffect, useMemo, useRef, useState } from 'react'
import 'leaflet/dist/leaflet.css'
import L from 'leaflet'
import { getActiveStations } from '../api/station'
import iconUrl from 'leaflet/dist/images/marker-icon.png'
import iconRetinaUrl from 'leaflet/dist/images/marker-icon-2x.png'
import shadowUrl from 'leaflet/dist/images/marker-shadow.png'

// Configure default marker icon so it works with bundlers
const DefaultIcon = L.icon({
  iconRetinaUrl,
  iconUrl,
  shadowUrl,
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
  tooltipAnchor: [16, -28],
  shadowSize: [41, 41],
})
L.Marker.prototype.options.icon = DefaultIcon

function useGeocoder() {
  const cache = useMemo(() => new Map(), [])

  async function geocode(address) {
    if (!address) return null
    const key = address.trim().toLowerCase()
    if (cache.has(key)) return cache.get(key)

    // Try localStorage cache
    const lsKey = `geo:${key}`
    const cached = localStorage.getItem(lsKey)
    if (cached) {
      try {
        const parsed = JSON.parse(cached)
        cache.set(key, parsed)
        return parsed
      } catch {}
    }

    // Nominatim (OpenStreetMap) geocoding
    const url = `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(address)}`
    try {
      const res = await fetch(url, { headers: { 'Accept': 'application/json' } })
      if (!res.ok) return null
      const results = await res.json()
      if (!Array.isArray(results) || results.length === 0) return null
      const best = results[0]
      const point = [parseFloat(best.lat), parseFloat(best.lon)]
      cache.set(key, point)
      localStorage.setItem(lsKey, JSON.stringify(point))
      return point
    } catch {
      return null
    }
  }

  return { geocode }
}

export default function StationMap({ stations = [], selectedStation = null }) {
  const containerRef = useRef(null)
  const mapRef = useRef(null)
  const [error, setError] = useState(null)
  const { geocode } = useGeocoder()

  useEffect(() => {
    let isMounted = true

    async function init() {
      try {
        if (!isMounted) return

        // Show only selected station if provided, otherwise show all stations
        const data = selectedStation
          ? [selectedStation]
          : (Array.isArray(stations) ? stations : [])

        // Initialize map once
        if (!mapRef.current && containerRef.current) {
          mapRef.current = L.map(containerRef.current, {
            center: [10.776, 106.700],
            zoom: 12,
            scrollWheelZoom: false,
          })

          L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '&copy; OpenStreetMap contributors',
          }).addTo(mapRef.current)
        }

        const map = mapRef.current
        if (!map) return

        const bounds = L.latLngBounds([])

        // Clear existing markers
        map.eachLayer((layer) => {
          if (layer instanceof L.Marker) {
            map.removeLayer(layer)
          }
        })

        // Place markers using provided coordinates, fallback to geocoded address
        for (const s of data) {
          const name = s.name ?? s.Name
          const address = s.address ?? s.location ?? s.Location

          let lat = s.lat ?? s.Lat ?? s.latitude ?? s.Latitude
          let lng = s.lng ?? s.Lng ?? s.longitude ?? s.Longitude

          // Normalize to numbers (support comma decimal from some backends/locales)
          const toNum = (v) => {
            if (typeof v === 'number') return v
            if (typeof v === 'string') {
              const trimmed = v.trim()
              // Try standard parse
              let n = Number.parseFloat(trimmed)
              if (!Number.isFinite(n)) {
                // Replace comma decimal
                n = Number.parseFloat(trimmed.replace(',', '.'))
              }
              return n
            }
            return NaN
          }

          lat = toNum(lat)
          lng = toNum(lng)

          let point = null
          if (Number.isFinite(lat) && Number.isFinite(lng)) {
            // Swap if reversed (lng,lat)
            if ((lat < -90 || lat > 90) && (lng >= -90 && lng <= 90)) {
              const tmp = lat; lat = lng; lng = tmp
            }
            // Validate ranges
            if (lat >= -90 && lat <= 90 && lng >= -180 && lng <= 180) {
              point = [lat, lng]
            }
          }

          if (!point && typeof address === 'string') {
            const text = address.trim()
            const patterns = [
              /!3d(-?\d+(?:\.\d+)?)!4d(-?\d+(?:\.\d+)?)/,            // place lat/lng
              /[?&]ll=(-?\d+(?:\.\d+)?),\s*(-?\d+(?:\.\d+)?)/,      // ll=lat,lng
              /[?&]q=(-?\d+(?:\.\d+)?),\s*(-?\d+(?:\.\d+)?)/,       // q=lat,lng
              /@(-?\d+(?:\.\d+)?),\s*(-?\d+(?:\.\d+)?)/             // @ center lat,lng
            ]
            for (const re of patterns) {
              const m = text.match(re)
              if (m) {
                const plat = parseFloat(m[1])
                const plng = parseFloat(m[2])
                if (Number.isFinite(plat) && Number.isFinite(plng)) {
                  point = [plat, plng]
                }
                break
              }
            }
          }

          if (!point) {
            point = await geocode(address)
          }

          if (point && isFinite(point[0]) && isFinite(point[1])) {
            const marker = L.marker(point)
            marker.addTo(map).bindPopup(`<strong>${name ?? 'Station'}</strong><br/>${address ?? ''}`)
            bounds.extend(point)
          }
        }

        if (bounds.isValid()) {
          map.fitBounds(bounds.pad(0.2))
        } else if (selectedStation) {
          // If selected but geocoding failed, at least show the default location
          const address = selectedStation.address ?? selectedStation.location ?? selectedStation.Location
          map.setView([10.776, 106.700], 14)
        }

        // Ensure Leaflet recalculates size after layout
        setTimeout(() => {
          map.invalidateSize()
        }, 0)
      } catch (e) {
        if (!isMounted) return
        setError(e?.message || 'Failed to load stations for map')
      }
    }

    if (stations.length > 0 || selectedStation) {
      init()
    }
    return () => { isMounted = false }
  }, [stations, selectedStation, geocode])

  // Recompute map size when container resizes
  useEffect(() => {
    if (!containerRef.current) return
    const ro = new ResizeObserver(() => {
      if (mapRef.current) mapRef.current.invalidateSize()
    })
    ro.observe(containerRef.current)
    return () => ro.disconnect()
  }, [])

  return (
    <div className="map-card" aria-live="polite">
      {error && (
        <div className="card card-body" role="alert">
          <p className="card-subtext">{error}</p>
        </div>
      )}
      <div ref={containerRef} className="map-canvas" aria-label="Interactive map of stations" />
    </div>
  )
}
