import React, { useState, useEffect } from 'react'
import Navbar from '../components/Navbar'
import Footer from '../components/Footer'
import CTA from '../components/CTA'
import Stepper from '../components/Stepper'
import BookingStep1_SelectStation from './BookingStep1_SelectStation'
import BookingStep2_SelectModel from './BookingStep2_SelectModel'
import BookingStep3_Schedule from './BookingStep3_Schedule'
import * as vehicleApi from '../api/vehicle'
import * as stationApi from '../api/station'
import * as bookingApi from '../api/booking'
import { validateUserDocuments } from '/utils/documentValidation'

export default function BookingNew() {
  const [step, setStep] = useState(0)
  const steps = ['Select Station', 'Select Vehicle', 'Schedule & Confirm']
  
  const [user, setUser] = useState(null)
  const [token, setToken] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
  const [documentError, setDocumentError] = useState(null)
  
  const [stations, setStations] = useState([])
  const [models, setModels] = useState([])
  const [vehicles, setVehicles] = useState([])
  
  const [selectedStation, setSelectedStation] = useState(null)
  const [selectedModel, setSelectedModel] = useState(null)
  const [pickupDate, setPickupDate] = useState('')
  const [dropoffDate, setDropoffDate] = useState('')
  
  const [preview, setPreview] = useState(null)
  const [termsAccepted, setTermsAccepted] = useState(false)
  const [previewLoading, setPreviewLoading] = useState(false)
  const [previewError, setPreviewError] = useState(null)
  const [selectedVehicle, setSelectedVehicle] = useState(null)
  const [suggestedStations, setSuggestedStations] = useState([])
  
  const [bookingLoading, setBookingLoading] = useState(false)
  const [bookingError, setBookingError] = useState(null)

  function parseTimeString(str) {
    if (!str || typeof str !== 'string') return null
    const m = str.match(/^(\d{1,2})(?::(\d{2}))?/)
    if (!m) return null
    const hh = Math.max(0, Math.min(23, parseInt(m[1], 10)))
    const mm = Math.max(0, Math.min(59, m[2] ? parseInt(m[2], 10) : 0))
    return { hh, mm }
  }
  function getStationHoursFor(dateStr) {
    const d = dateStr ? new Date(dateStr) : null
    if (!d) return null
    const openField = selectedStation?.OpenTime || selectedStation?.openTime || selectedStation?.OpeningTime || selectedStation?.openingTime || selectedStation?.OpenAt || selectedStation?.openAt
    const closeField = selectedStation?.CloseTime || selectedStation?.closeTime || selectedStation?.ClosingTime || selectedStation?.closingTime || selectedStation?.CloseAt || selectedStation?.closeAt
    const open = parseTimeString(openField) || { hh: 8, mm: 0 }
    const close = parseTimeString(closeField) || { hh: 22, mm: 0 }
    const start = new Date(d); start.setHours(open.hh, open.mm, 0, 0)
    const end = new Date(d); end.setHours(close.hh, close.mm, 0, 0)
    return { start, end }
  }
  function isWithinStationHours(dateStr) {
    if (!dateStr) return false
    const t = new Date(dateStr)
    const hrs = getStationHoursFor(dateStr)
    if (!hrs) return true
    return t >= hrs.start && t <= hrs.end
  }

  // Initialize user and fetch data
  useEffect(() => {
    async function init() {
      try {
        const authUser = localStorage.getItem('auth.user')
        const authToken = localStorage.getItem('auth.token')
        const storedId  = localStorage.getItem('auth.userId')

        console.log('[BookingNew] LocalStorage values:', { authUser, authToken, storedId })

        if (!authUser || !authToken) {
          setError('Please log in to book a vehicle')
          window.location.hash = 'login'
          return
        }

        const userData = JSON.parse(authUser)
        console.log('[BookingNew] Parsed userData:', userData)

        setUser(userData)
        setToken(authToken)

        // Validate documents
        let effectiveUserId = storedId ? Number(storedId) : null

        if (!effectiveUserId || isNaN(effectiveUserId)) {
          const fallbackId = userData?.userId ?? userData?.UserId ??
            userData?.id     ?? userData?.Id ?? null
          effectiveUserId = Number(fallbackId)
        }

        if (!effectiveUserId || isNaN(effectiveUserId)) {
          console.error('[BookingNew] Invalid userId. Extracted values:', {
            storedId,
            userIdField: userData?.userId,
            UserIdField: userData?.UserId,
            idField: userData?.id,
            IdField: userData?.Id
          })
          setError('Cannot determine user ID. Please log in again.')
          window.location.hash = 'login'
          return
        }

        const docValidation = await validateUserDocuments(effectiveUserId, authToken)
        console.log('[BookingNew] Document validation result:', docValidation)

        if (!docValidation.hasAllDocuments) {
          setDocumentError({
            message: `Please complete all documents before booking: ${docValidation.missingDocs.join(', ')}`,
            missingDocs: docValidation.missingDocs,
          })
        }
        
        // Fetch stations and models
        let stationsRes, modelsRes, vehiclesRes
        try {
          [stationsRes, modelsRes, vehiclesRes] = await Promise.all([
            stationApi.getActiveStations(authToken).catch(err => {
              console.error('[BookingNew] Error fetching stations:', err)
              return { data: [] }
            }),
            vehicleApi.getActiveModels(authToken).catch(err => {
              console.error('[BookingNew] Error fetching models:', err)
              return { data: [] }
            }),
            vehicleApi.getActiveVehicles(authToken).catch(err => {
              console.error('[BookingNew] Error fetching vehicles:', err)
              return { data: [] }
            }),
          ])
        } catch (err) {
          console.error('[BookingNew] Promise.all error:', err)
          stationsRes = { data: [] }
          modelsRes = { data: [] }
          vehiclesRes = { data: [] }
        }

        const stationsData = Array.isArray(stationsRes.data) ? stationsRes.data : []
        const modelsData = Array.isArray(modelsRes.data) ? modelsRes.data : []
        const vehiclesData = Array.isArray(vehiclesRes.data) ? vehiclesRes.data : []

        console.log('[BookingNew] Loaded data:', {
          stationsCount: stationsData.length,
          modelsCount: modelsData.length,
          vehiclesCount: vehiclesData.length
        })

        if (vehiclesData.length === 0) {
          console.warn('[BookingNew] No vehicles returned from API')
        }

        setStations(stationsData)
        setModels(modelsData)
        setVehicles(vehiclesData)

        // Debug: Check vehicles structure
        console.log('[BookingNew] Vehicles data sample:', vehiclesData.slice(0, 2))
        console.log('[BookingNew] Models data sample:', modelsData.slice(0, 2))
        if (vehiclesData.length > 0) {
          console.log('[BookingNew] First vehicle keys:', Object.keys(vehiclesData[0]))
        }
        if (modelsData.length > 0) {
          console.log('[BookingNew] First model keys:', Object.keys(modelsData[0]))
        }

        // Pre-fill from quick_search_data if available
        const quickSearchStr = localStorage.getItem('quick_search_data')
        if (quickSearchStr) {
          try {
            const data = JSON.parse(quickSearchStr)
            if (data.stationId || data.Id) {
              const searchId = data.stationId || data.Id
              const station = stationsData.find(s => (s.Id || s.stationId) === searchId)
              if (station) {
                setSelectedStation(station)
              }
            }
            if (data.modelId || data.ModelId) {
              const searchId = data.modelId || data.ModelId
              const model = modelsData.find(m => (m.ModelId || m.modelId) === searchId)
              if (model) {
                setSelectedModel(model)
              }
            }
            if (data.pickupDate) setPickupDate(data.pickupDate)
            if (data.dropoffDate) setDropoffDate(data.dropoffDate)
            localStorage.removeItem('quick_search_data')
          } catch (e) {
            console.error('Error parsing quick search data:', e)
          }
        }

        setError(null)
      } catch (err) {
        console.error('Error initializing booking:', err)
        setError(err.message || 'Error loading booking page')
      } finally {
        setLoading(false)
      }
    }
    
    init()
  }, [])

  // Pick a random available vehicle for the selected model (optionally at selected station)
  const getRandomAvailableVehicle = () => {
    if (!selectedModel) return { vehicle: null, suggestions: [] }

    if (vehicles.length === 0) {
      console.warn('[BookingNew] No vehicles loaded from API')
      return { vehicle: null, suggestions: [] }
    }

    const modelId = selectedModel.ModelId || selectedModel.modelId
    const stationId = selectedStation?.Id || selectedStation?.stationId || selectedStation?.id

    // Candidates: same model + status Available
    const allCandidates = vehicles.filter(v => {
      const vModelId = v.ModelId || v.modelId
      const vStatus = String(v.Status || v.status || '').toLowerCase()
      return vModelId === modelId && vStatus === 'available'
    })

    // Filter by current station if chosen
    const stationCandidates = stationId
      ? allCandidates.filter(v => (v.StationId || v.stationId || v.StationID) === stationId)
      : allCandidates

    if (stationCandidates.length > 0) {
      const idx = Math.floor(Math.random() * stationCandidates.length)
      const chosen = stationCandidates[idx]
      const vehicleId = chosen.VehicleId || chosen.vehicleId || chosen.id
      console.log('[BookingNew] Random vehicle chosen for model at station:', { modelId, stationId, vehicleId })
      return { vehicle: chosen, suggestions: [] }
    }

    // Build suggestions from other stations having this model available
    const suggestionStationIds = Array.from(new Set(allCandidates
      .map(v => v.StationId || v.stationId || v.StationID)
      .filter(Boolean)))
    const suggestions = stations.filter(s => suggestionStationIds.includes(s.Id || s.stationId || s.id))
    console.warn('[BookingNew] No vehicles available at selected station. Suggestions:', suggestions.map(s => s.Name || s.name))
    return { vehicle: null, suggestions }
  }

  const handlePreview = async () => {
    setPreviewError(null)

    if (!selectedStation) {
      setPreviewError('Please select a pickup station')
      return
    }
    if (!selectedModel) {
      setPreviewError('Please select a model')
      return
    }
    if (!pickupDate) {
      setPreviewError('Please enter pickup time')
      return
    }
    if (!dropoffDate) {
      setPreviewError('Please enter dropoff time')
      return
    }
    if (new Date(pickupDate) >= new Date(dropoffDate)) {
      setPreviewError('Dropoff time must be after pickup time')
      return
    }

    // Opening hours validation
    if (!isWithinStationHours(pickupDate) || !isWithinStationHours(dropoffDate)) {
      setPreviewError('Pickup and return times must be within station working hours')
      return
    }

    try {
      setPreviewLoading(true)

      // 1) Chọn ngẫu nhiên 1 xe khả dụng theo model (ưu tiên tại trạm đã chọn)
      const pick = getRandomAvailableVehicle()
      const chosenVehicle = pick.vehicle
      if (!chosenVehicle) {
        setSuggestedStations(pick.suggestions || [])
        setPreviewError('Hết xe ở trạm này cho mẫu đã chọn. Vui lòng chọn trạm khác bên dưới.')
        return
      }

      const vehicleId = Number(chosenVehicle.VehicleId || chosenVehicle.vehicleId || chosenVehicle.id)
      if (!vehicleId || Number.isNaN(vehicleId)) {
        setPreviewError('Vehicle information is invalid. Please try again.')
        return
      }

      // 2) Convert dates to ISO format
      const fromISO = new Date(pickupDate).toISOString()
      const toISO = new Date(dropoffDate).toISOString()

      // 3) Lấy userId an toàn
      const previewUserId =
        Number(localStorage.getItem('auth.userId')) ||
        Number(user?.userId || user?.UserId || user?.id || user?.Id)

      if (!previewUserId || Number.isNaN(previewUserId)) {
        setPreviewError('Cannot determine user ID. Please log in again.')
        return
      }

      // 4) Giá tiền hợp lệ (chỉ validate RentFeeForHour, không validate ModelCost)
      const rentFee = Number(selectedModel.RentFeeForHour || selectedModel.rentFeeForHour || 0)
      const modelCost = Number(selectedModel.ModelCost || selectedModel.modelCost || 0)
      if (!(rentFee > 0)) {
        setPreviewError('Rent price invalid. Please try again or choose another model.')
        return
      }
      // ModelCost validation removed - no price validation required

      // 5) Gọi preview
      const previewRes = await bookingApi.getOrderPreview({
        userId: previewUserId,
        vehicleId,
        fromDate: fromISO,
        toDate: toISO,
        rentFeeForHour: rentFee,
        modelPrice: modelCost,
        paymentMethod: 'VNPay',
      }, token)

      if (!previewRes?.data) {
        setPreviewError('Unable to calculate cost. Please try again.')
        return
      }

      // Map backend field names to frontend expected names (PascalCase → camelCase)
      const mappedPreview = {
        ...previewRes.data,
        totalRentalCost: previewRes.data.TotalRentalCost,
        depositCost: previewRes.data.DepositAmount,
        serviceFee: previewRes.data.ServiceFee,
        totalPaymentCost: previewRes.data.TotalPaymentAmount,
      }

      setPreview(mappedPreview)
      setSelectedVehicle(chosenVehicle)
      setSuggestedStations([])
    } catch (err) {
      console.error('Error getting preview:', err)
      setPreviewError(err?.message || 'Error calculating cost. Please try again.')
    } finally {
      setPreviewLoading(false)
    }
  }

  const handleConfirmBooking = async () => {
    setBookingError(null)

    if (!preview) {
      setBookingError('Please preview the order before confirming')
      return
    }

    if (!selectedStation) {
      setBookingError('Pickup station info missing. Please go back and choose again.')
      return
    }

    if (!selectedModel) {
      setBookingError('Model info missing. Please go back and choose again.')
      return
    }

    if (!pickupDate || !dropoffDate) {
      setBookingError('Time info missing. Please go back and re-enter.')
      return
    }

    // Opening hours validation
    if (!isWithinStationHours(pickupDate) || !isWithinStationHours(dropoffDate)) {
      setBookingError('Pickup and return times must be within station working hours')
      return
    }

    try {
      setBookingLoading(true)
      
      const bookingUserId = Number(localStorage.getItem('auth.userId')) || Number(user?.userId || user?.UserId || user?.id || user?.Id)
      if (!bookingUserId || isNaN(bookingUserId)) {
        setBookingError('Cannot determine user ID. Please log in again.')
        return
      }

      const vehicleToUse = selectedVehicle
      if (!vehicleToUse) {
        setBookingError('No selected vehicle. Please preview again.')
        return
      }

      const vehicleId = vehicleToUse.VehicleId || vehicleToUse.vehicleId || vehicleToUse.id
      if (!vehicleId || isNaN(Number(vehicleId))) {
        setBookingError('Vehicle information is invalid. Please try again.')
        return
      }

      const rentFee = Number(selectedModel.RentFeeForHour || selectedModel.rentFeeForHour || 0)
      const modelCost = Number(selectedModel.ModelCost || selectedModel.modelCost || 0)

      // Only validate RentFeeForHour, ModelCost validation removed
      if (rentFee <= 0) {
        setBookingError('Rent price information is invalid. Please try again.')
        return
      }

      const orderRes = await bookingApi.createOrder({
        userId: bookingUserId,
        vehicleId: Number(vehicleId),
        fromDate: new Date(pickupDate).toISOString(),
        toDate: new Date(dropoffDate).toISOString(),
        rentFeeForHour: rentFee,
        modelPrice: modelCost,
        paymentMethod: 'VNPay',
      }, token)
      
      if (!orderRes.data || !orderRes.data.OrderId) {
        setBookingError('Unable to create order. Please try again.')
        return
      }

      const orderId = orderRes.data.OrderId

      // Log full response for debugging
      console.log('[BookingNew] Full order response:', orderRes.data)

      // Store booking info for payment page
      const stationName = selectedStation.Name || selectedStation.name
      const manufacturer = selectedModel.Manufacturer || selectedModel.manufacturer
      const modelName = selectedModel.ModelName || selectedModel.modelName
      const vehicleColor = vehicleToUse.Color || vehicleToUse.color
      const licensePlate = vehicleToUse.LicensePlate || vehicleToUse.licensePlate || 'N/A'
      const vehicleType = selectedModel.VehicleType || selectedModel.vehicleType || 'Xe điện'

      const bookingDataToStore = {
        orderId,
        totalCost: preview?.totalPaymentCost || 0,
        totalRentalCost: preview?.totalRentalCost || 0,
        depositCost: preview?.depositCost || 0,
        serviceFee: preview?.serviceFee || 0,
        expiresAt: orderRes.data.ExpiresAt,
        vehicleInfo: {
          vehicleId: Number(vehicleId),
          station: stationName,
          model: `${manufacturer} ${modelName}`,
          color: vehicleColor,
          licensePlate: licensePlate,
          type: vehicleType,
        },
        dates: {
          from: pickupDate,
          to: dropoffDate,
        },
      }

      console.log('[BookingNew] Storing booking data:', bookingDataToStore)
      localStorage.setItem('pending_booking', JSON.stringify(bookingDataToStore))

      // Verify storage
      const stored = localStorage.getItem('pending_booking')
      console.log('[BookingNew] Verified stored data:', stored)

      console.log('[BookingNew] Booking created successfully, navigating to payment...')
      // Navigate to payment page
      window.location.hash = 'payment'
    } catch (err) {
      console.error('Error creating booking:', err)
      setBookingError(err.message || 'Error creating order. Please try again.')
    } finally {
      setBookingLoading(false)
    }
  }

  const handleNext = () => {
    if (step === 0 && !selectedStation) {
      setPreviewError('Please select a pickup station')
      return
    }
    if (step === 1 && !selectedModel) {
      setPreviewError('Please select a model')
      return
    }
    setStep((s) => Math.min(s + 1, steps.length - 1))
    setPreviewError(null)
    setBookingError(null)
  }

  const handleBack = () => {
    setStep((s) => Math.max(s - 1, 0))
    setPreviewError(null)
    setBookingError(null)
    setPreviewLoading(false)
    setPreview(null)
  }

  if (loading) {
    return (
      <div data-figma-layer="Booking New Page">
        <Navbar />
        <main>
          <section className="section page-offset">
            <div className="container" role="status" aria-busy="true">
              <div className="card">
                <div className="card-body">
                  <div className="skeleton skeleton-line"></div>
                  <div className="skeleton skeleton-pill"></div>
                  <div className="two-col-grid">
                    <div className="skeleton skeleton-card"></div>
                    <div className="skeleton skeleton-card"></div>
                  </div>
                </div>
              </div>
            </div>
          </section>
        </main>
        <Footer />
      </div>
    )
  }

  return (
    <div data-figma-layer="Booking New Page">
      <Navbar />
      <main>
        <section className="section page-offset">
          <div className="container">
            <div className="section-header">
              <h1 className="section-title">New Booking</h1>
              <p className="section-subtitle">Complete 3 simple steps to book a vehicle.</p>
            </div>

            {error && (
              <div className="error-message error-visible">
                <span>{error}</span>
              </div>
            )}

            {documentError && (
              <div className="error-message error-visible warning">
                <span className="warning-text">
                  ⚠️ {documentError.message}
                  <br />
                  <a href="#profile-docs" className="link-underline">
                    Update documents →
                  </a>
                </span>
              </div>
            )}

            <div className="card">
              <div className="card-body">
                <Stepper steps={steps} current={step} />

                {/* Step 0 */}
                {step === 0 && (
                  <BookingStep1_SelectStation 
                    stations={stations}
                    selectedStation={selectedStation}
                    onSelectStation={setSelectedStation}
                  />
                )}

                {/* Step 1 */}
                {step === 1 && (
                  <BookingStep2_SelectModel 
                    models={models}
                    vehicles={vehicles}
                    selectedModel={selectedModel}
                    onSelectModel={setSelectedModel}
                  />
                )}

                {/* Step 2 */}
                {step === 2 && (
                  <>
                    <BookingStep3_Schedule
                      selectedStation={selectedStation}
                      selectedModel={selectedModel}
                      pickupDate={pickupDate}
                      dropoffDate={dropoffDate}
                      onPickupDateChange={setPickupDate}
                      onDropoffDateChange={setDropoffDate}
                      preview={preview}
                      previewLoading={previewLoading}
                      previewError={previewError}
                      bookingLoading={bookingLoading}
                      bookingError={bookingError}
                      onPreview={handlePreview}
                      onConfirmBooking={handleConfirmBooking}
                      termsAccepted={termsAccepted}
                      setTermsAccepted={setTermsAccepted}
                    />

                    {/* Station suggestions when no vehicles at current station */}
                    {previewError && suggestedStations.length > 0 && (
                      <div className="mt-6">
                        <div className="error-message error-visible warning" role="alert">
                          <span>Gợi ý trạm khác có sẵn xe cho mẫu này:</span>
                        </div>
                        <div className="stations-grid">
                          {suggestedStations.map((station, index) => {
                            const stationId = station.Id ?? station.stationId ?? station.id
                            return (
                              <div
                                key={`suggestion-${stationId || index}`}
                                className={`station-card-item`}
                                onClick={() => {
                                  setSelectedStation(station)
                                  setStep(0)
                                  setPreview(null)
                                  setPreviewError(null)
                                  setSuggestedStations([])
                                }}
                                onKeyDown={(e) => {
                                  if (e.key === 'Enter' || e.key === ' ') {
                                    e.preventDefault()
                                    setSelectedStation(station)
                                    setStep(0)
                                    setPreview(null)
                                    setPreviewError(null)
                                    setSuggestedStations([])
                                  }
                                }}
                                role="button"
                                tabIndex={0}
                                aria-label={`Select ${station.Name || station.name}`}
                              >
                                <h4 className="station-card-name">{station.Name || station.name}</h4>
                                <p className="station-card-location">{station.Location || station.location}</p>
                              </div>
                            )
                          })}
                        </div>
                      </div>
                    )}
                  </>
                )}

                <div className="row-between mt-8">
                  <CTA as="button" variant="ghost" onClick={handleBack}>Back</CTA>
                  {step < steps.length - 1 ? (
                    <CTA as="button" onClick={handleNext}>Continue</CTA>
                  ) : (
                    <CTA as="button" onClick={handleConfirmBooking} disabled={!preview || bookingLoading || !termsAccepted}>
                      {bookingLoading ? 'Processing...' : 'Confirm & Pay'}
                    </CTA>
                  )}
                </div>
              </div>
            </div>
          </div>
        </section>
      </main>
      <Footer />
    </div>
  )
}
