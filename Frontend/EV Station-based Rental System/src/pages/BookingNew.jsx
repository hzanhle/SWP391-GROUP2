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
  
  const [bookingLoading, setBookingLoading] = useState(false)
  const [bookingError, setBookingError] = useState(null)

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

  // Get first available vehicle for selected model
  const getFirstAvailableVehicle = () => {
    if (!selectedModel) return null

    if (vehicles.length === 0) {
      console.warn('[BookingNew] No vehicles loaded from API')
      return null
    }

    const modelId = selectedModel.ModelId || selectedModel.modelId
    const available = vehicles.filter(v => {
      const vModelId = v.ModelId || v.modelId
      return vModelId === modelId
    })

    if (available.length > 0) {
      const vehicleId = available[0].VehicleId || available[0].vehicleId || available[0].id
      console.log('[BookingNew] Found vehicle for model:', { modelId, vehicleId })
      return available[0]
    }

    // Fallback: return first vehicle if available
    console.warn('[BookingNew] No vehicle matched model, using first available')
    return vehicles.length > 0 ? vehicles[0] : null
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

    try {
      setPreviewLoading(true)

      // 1) Lấy xe khả dụng đầu tiên theo model đã chọn
      const chosenVehicle = getFirstAvailableVehicle()
      if (!chosenVehicle) {
        setPreviewError('No vehicles available for this model. Please choose another model.')
        return
      }

      const vehicleId = Number(chosenVehicle.VehicleId || chosenVehicle.vehicleId || chosenVehicle.id)
      if (!vehicleId || Number.isNaN(vehicleId)) {
        setPreviewError('Vehicle information is invalid. Please try again.')
        return
      }

      // 2) Check availability (nếu API lỗi vẫn cho xem preview)
      const fromISO = new Date(pickupDate).toISOString()
      const toISO = new Date(dropoffDate).toISOString()

      try {
        const availabilityRes = await vehicleApi.checkVehicleAvailability({
          vehicleId,
          fromDate: fromISO,
          toDate: toISO,
        }, token)

        if (availabilityRes?.data && availabilityRes.data.isAvailable === false) {
          setPreviewError(`Vehicle is not available in this time range. ${availabilityRes.data?.reason || 'Please choose another time.'}`)
          return
        }
      } catch (availErr) {
        console.warn('[BookingNew] Availability check failed, continue preview:', availErr)
      }

      // 3) Lấy userId an toàn
      const previewUserId =
        Number(localStorage.getItem('auth.userId')) ||
        Number(user?.userId || user?.UserId || user?.id || user?.Id)

      if (!previewUserId || Number.isNaN(previewUserId)) {
        setPreviewError('Cannot determine user ID. Please log in again.')
        return
      }

      // 4) Giá tiền hợp lệ
      const rentFee = Number(selectedModel.RentFeeForHour || selectedModel.rentFeeForHour || 0)
      const modelCost = Number(selectedModel.ModelCost || selectedModel.modelCost || 0)
      if (!(rentFee > 0)) {
        setPreviewError('Rent price invalid. Please try again or choose another model.')
        return
      }
      if (!(modelCost > 0)) {
        setPreviewError('Model price invalid. Please try again or choose another model.')
        return
      }

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

      setPreview(previewRes.data)
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
    
    try {
      setBookingLoading(true)
      
      const bookingUserId = Number(localStorage.getItem('auth.userId')) || Number(user?.userId || user?.UserId || user?.id || user?.Id)
      if (!bookingUserId || isNaN(bookingUserId)) {
        setBookingError('Cannot determine user ID. Please log in again.')
        return
      }

      const vehicleToUse = getFirstAvailableVehicle()
      if (!vehicleToUse) {
        setBookingError('Cannot find a vehicle. Please go back and try again.')
        return
      }

      const vehicleId = vehicleToUse.VehicleId || vehicleToUse.vehicleId || vehicleToUse.id
      if (!vehicleId || isNaN(Number(vehicleId))) {
        setBookingError('Vehicle information is invalid. Please try again.')
        return
      }

      const rentFee = Number(selectedModel.RentFeeForHour || selectedModel.rentFeeForHour || 0)
      const modelCost = Number(selectedModel.ModelCost || selectedModel.modelCost || 0)

      if (rentFee <= 0 || modelCost <= 0) {
        setBookingError('Vehicle price information is invalid. Please try again.')
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
        totalCost: orderRes.data.TotalCost,
        depositCost: orderRes.data.DepositCost || 0,
        serviceFee: orderRes.data.ServiceFee || 0,
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
            <div className="container">
              <div className="text-center py-12">
                <p>Loading...</p>
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
                <span style={{ color: '#856404' }}>
                  ⚠️ {documentError.message}
                  <br />
                  <a href="#profile-docs" style={{ color: '#0066cc', textDecoration: 'underline', marginTop: '0.5rem', display: 'inline-block' }}>
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
                )}

                <div className="row-between" style={{ marginTop: '2rem' }}>
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
