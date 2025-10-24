import React, { useState, useEffect } from 'react'
import Navbar from '../components/Navbar'
import Footer from '../components/Footer'
import CTA from '../components/CTA'
import Stepper from '../components/Stepper'
import * as vehicleApi from '../api/vehicle'
import * as stationApi from '../api/station'
import * as bookingApi from '../api/booking'
import { validateUserDocuments } from '/utils/documentValidation';

export default function BookingNew() {
  const [step, setStep] = useState(0)
  const steps = ['Chọn điểm thuê', 'Chọn xe', 'Lịch & xác nhận']
  
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
  const [selectedVehicle, setSelectedVehicle] = useState(null)
  const [pickupDate, setPickupDate] = useState('')
  const [dropoffDate, setDropoffDate] = useState('')
  
  const [preview, setPreview] = useState(null)
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
          setError('Vui lòng đăng nhập để đặt xe')
          window.location.hash = 'login'
          return
        }

        const userData = JSON.parse(authUser)
        console.log('[BookingNew] Parsed userData:', userData)

        setUser(userData)
        setToken(authToken)

        // Validate documents - use stored userId as primary source
        let effectiveUserId = storedId ? Number(storedId) : null
        console.log('[BookingNew] storedId:', storedId, '→ effectiveUserId:', effectiveUserId, 'isNaN:', isNaN(effectiveUserId))

        // Fallback to extracting from user object if stored ID not available
        if (!effectiveUserId || isNaN(effectiveUserId)) {
          const fallbackId = userData?.userId ?? userData?.UserId ??
            userData?.id     ?? userData?.Id ?? null
          effectiveUserId = Number(fallbackId)
          console.log('[BookingNew] Fallback extraction: fallbackId =', fallbackId, '→ effectiveUserId =', effectiveUserId)
        }

        // If still invalid, prevent proceeding
        if (!effectiveUserId || isNaN(effectiveUserId)) {
          console.error('[BookingNew] Invalid userId. Extracted values:', {
            storedId,
            userIdField: userData?.userId,
            UserIdField: userData?.UserId,
            idField: userData?.id,
            IdField: userData?.Id
          })
          setError('Không thể xác định ID người dùng. Vui lòng đăng nhập lại.')
          window.location.hash = 'login'
          return
        }

   console.log('[BookingNew] About to validate documents with userId:', effectiveUserId)
   const docValidation = await validateUserDocuments(effectiveUserId, authToken)

        console.log('[BookingNew] Document validation result:', docValidation)

        if (!docValidation.hasAllDocuments) {
          setDocumentError({
            message: `Vui lòng cập nhật đầy đủ tài liệu trước khi đặt xe: ${docValidation.missingDocs.join(', ')}`,
            missingDocs: docValidation.missingDocs,
          })
        }
        
        // Fetch stations and models
        const [stationsRes, modelsRes, vehiclesRes] = await Promise.all([
          stationApi.getActiveStations(authToken),
          vehicleApi.getActiveModels(authToken),
          vehicleApi.getActiveVehicles(authToken),
        ])

        const stationsData = Array.isArray(stationsRes.data) ? stationsRes.data : []
        const modelsData = Array.isArray(modelsRes.data) ? modelsRes.data : []
        const vehiclesData = Array.isArray(vehiclesRes.data) ? vehiclesRes.data : []

        console.log('[BookingNew] Loaded data:', { stationsData, modelsData, vehiclesData })

        setStations(stationsData)
        setModels(modelsData)
        setVehicles(vehiclesData)

        // Pre-fill from quick_search_data if available
        const quickSearchStr = localStorage.getItem('quick_search_data')
        if (quickSearchStr) {
          try {
            const data = JSON.parse(quickSearchStr)
            if (data.stationId) {
              const station = stationsData.find(s => s.stationId === data.stationId)
              if (station) setSelectedStation(station)
            }
            if (data.modelId) {
              const model = modelsData.find(m => m.modelId === data.modelId)
              if (model) setSelectedModel(model)
              if (model) {
                const vehicle = vehiclesData.find(v => v.modelId === data.modelId)
                if (vehicle) setSelectedVehicle(vehicle)
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
        setError(err.message || 'Lỗi khi tải trang đặt xe')
      } finally {
        setLoading(false)
      }
    }
    
    init()
  }, [])

  // Filter vehicles based on selected model
  const filteredVehicles = selectedModel 
    ? vehicles.filter(v => v.modelId === selectedModel.modelId)
    : []

  async function handlePreview() {
    // allow using first available vehicle if user did not pick a specific one
    const vehicleToUse = selectedVehicle || (selectedModel && filteredVehicles.length > 0 ? filteredVehicles[0] : null)

    if (!selectedStation || !vehicleToUse || !pickupDate || !dropoffDate) {
      setPreviewError('Vui lòng điền đầy đủ thông tin')
      return
    }

    if (new Date(pickupDate) >= new Date(dropoffDate)) {
      setPreviewError('Thời gian trả xe phải sau thời gian nhận xe')
      return
    }

    try {
      setPreviewLoading(true)
      setPreviewError(null)

      const previewUserId = Number(localStorage.getItem('auth.userId')) || Number(user?.userId || user?.UserId || user?.id || user?.Id)
      if (!previewUserId || isNaN(previewUserId)) {
        setPreviewError('Không thể xác định ID người dùng')
        return
      }

      // if we auto-picked a vehicle, reflect it in state so UI shows selection
      if (!selectedVehicle && vehicleToUse) {
        setSelectedVehicle(vehicleToUse)
      }

      const previewRes = await bookingApi.getOrderPreview({
        userId: previewUserId,
        vehicleId: vehicleToUse.vehicleId,
        fromDate: new Date(pickupDate).toISOString(),
        toDate: new Date(dropoffDate).toISOString(),
        rentFeeForHour: selectedModel.rentFeeForHour,
        modelPrice: selectedModel.modelCost,
        paymentMethod: 'VNPay',
      }, token)

      setPreview(previewRes.data)
      setStep(2)
    } catch (err) {
      console.error('Error getting preview:', err)
      setPreviewError(err.message || 'Lỗi khi xem trước đơn hàng')
    } finally {
      setPreviewLoading(false)
    }
  }

  async function handleConfirmBooking() {
    if (!preview) {
      setBookingError('Vui lòng xem trước đơn hàng trước')
      return
    }
    
    try {
      setBookingLoading(true)
      setBookingError(null)
      
      const bookingUserId = Number(user?.userId || user?.UserId || user?.id || user?.Id)
      if (!bookingUserId || isNaN(bookingUserId)) {
        setBookingError('Không thể xác định ID người dùng')
        return
      }

      const orderRes = await bookingApi.createOrder({
        userId: bookingUserId,
        vehicleId: selectedVehicle.vehicleId,
        fromDate: new Date(pickupDate).toISOString(),
        toDate: new Date(dropoffDate).toISOString(),
        rentFeeForHour: selectedModel.rentFeeForHour,
        modelPrice: selectedModel.modelCost,
        paymentMethod: 'VNPay',
      }, token)
      
      const orderId = orderRes.data.orderId
      
      // Store booking info for payment page
      localStorage.setItem('pending_booking', JSON.stringify({
        orderId,
        totalAmount: orderRes.data.totalAmount,
        expiresAt: orderRes.data.expiresAt,
        vehicleInfo: {
          station: selectedStation.name,
          model: `${selectedModel.manufacturer} ${selectedModel.modelName}`,
          color: selectedVehicle.color,
        },
        dates: {
          from: pickupDate,
          to: dropoffDate,
        },
      }))
      
      // Navigate to payment page
      window.location.hash = 'payment'
    } catch (err) {
      console.error('Error creating booking:', err)
      setBookingError(err.message || 'Lỗi khi tạo đơn hàng. Vui lòng thử lại.')
    } finally {
      setBookingLoading(false)
    }
  }

  function next() {
    if (step === 0 && !selectedStation) {
      setPreviewError('Vui lòng chọn điểm thuê xe')
      return
    }
    if (step === 1 && !selectedModel) {
      setPreviewError('Vui lòng chọn mẫu xe')
      return
    }
    setStep((s) => Math.min(s + 1, steps.length - 1))
  }

  function back() {
    setStep((s) => Math.max(s - 1, 0))
    setPreviewError(null)
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
                <p>Đang tải...</p>
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
              <h1 className="section-title">Đặt xe mới</h1>
              <p className="section-subtitle">Hoàn thành 3 bước đ��n giản để đặt xe.</p>
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
                    Cập nhật tài liệu →
                  </a>
                </span>
              </div>
            )}

            <div className="card">
              <div className="card-body">
                <Stepper steps={steps} current={step} />

                {/* Step 0: Select Station */}
                {step === 0 && (
                  <div className="field">
                    <label htmlFor="station" className="label">Chọn điểm thuê xe</label>
                    <select
                      id="station"
                      className="select"
                      value={String(selectedStation?.stationId ?? '')}
                      onChange={(e) => {
                        const val = e.target.value
                        const station = stations.find(s => String(s.stationId) === val || s.stationId === Number(val) || s.name === val)
                        setSelectedStation(station || null)
                      }}
                    >
                      <option value="">-- Chọn điểm thuê --</option>
                      {stations.map(station => (
                        <option key={station.stationId} value={String(station.stationId)}>
                          {station.name} - {station.location}
                        </option>
                      ))}
                    </select>

                    {stations.length === 0 && !loading && (
                      <div className="error-message error-visible warning" role="alert">
                        <span>
                          Không tìm thấy điểm thuê. Nếu bạn đang dùng bản preview, frontend không thể truy cập API localhost.
                          Chạy frontend cục bộ hoặc cấu hình VITE_STATION_API_URL tới URL công khai để lấy dữ liệu.
                        </span>
                      </div>
                    )}

                  </div>
                )}

                {/* Step 1: Select Vehicle Model */}
                {step === 1 && (
                  <div className="field">
                    <label htmlFor="model" className="label">Chọn mẫu xe</label>
                    <select
                      id="model"
                      className="select"
                      value={String(selectedModel?.modelId ?? '')}
                      onChange={(e) => {
                        const val = e.target.value
                        const model = models.find(m => String(m.modelId) === val || m.modelId === Number(val) || (`${m.manufacturer} ${m.modelName}`) === val)
                        setSelectedModel(model || null)
                        setSelectedVehicle(null)
                      }}
                    >
                      <option value="">-- Chọn mẫu xe --</option>
                      {models.map(model => (
                        <option key={model.modelId} value={String(model.modelId)}>
                          {model.manufacturer} {model.modelName} - ${model.rentFeeForHour}/giờ
                        </option>
                      ))}
                    </select>

                    {selectedModel && (
                      <div style={{ marginTop: '2rem' }}>
                        <label htmlFor="vehicle" className="label">Chọn chiếc xe cụ thể</label>
                        <select
                          id="vehicle"
                          className="select"
                          value={String(selectedVehicle?.vehicleId ?? '')}
                          onChange={(e) => {
                            const val = e.target.value
                            const vehicle = filteredVehicles.find(v => String(v.vehicleId) === val || v.vehicleId === Number(val))
                            setSelectedVehicle(vehicle || null)
                          }}
                        >
                          <option value="">-- Chọn xe --</option>
                          {filteredVehicles.map(vehicle => (
                            <option key={vehicle.vehicleId} value={String(vehicle.vehicleId)}>
                              {vehicle.color} - {vehicle.status}
                            </option>
                          ))}
                        </select>
                      </div>
                    )}
                  </div>
                )}

                {/* Step 2: Schedule & Confirmation */}
                {step === 2 && (
                  <div className="booking-grid">
                    <div className="field">
                      <label htmlFor="pickup-date" className="label">Thời gian nhận xe</label>
                      <input 
                        id="pickup-date"
                        type="datetime-local" 
                        className="input"
                        value={pickupDate}
                        onChange={(e) => setPickupDate(e.target.value)}
                      />
                    </div>
                    <div className="field">
                      <label htmlFor="dropoff-date" className="label">Thời gian trả xe</label>
                      <input 
                        id="dropoff-date"
                        type="datetime-local" 
                        className="input"
                        value={dropoffDate}
                        onChange={(e) => setDropoffDate(e.target.value)}
                      />
                    </div>

                    {previewError && (
                      <div className="error-message error-visible grid-span-full">
                        <span>{previewError}</span>
                      </div>
                    )}

                    {!preview && !previewLoading && (
                      <div className="field grid-span-full text-center">
                        <CTA as="button" onClick={handlePreview} disabled={!((selectedVehicle || (selectedModel && filteredVehicles.length>0)) && pickupDate && dropoffDate)}>
                          Xem trước chi phí
                        </CTA>
                      </div>
                    )}

                    {previewLoading && (
                      <div className="field grid-span-full text-center">
                        <p>Đang tính toán...</p>
                      </div>
                    )}

                    {preview && (
                      <div className="summary grid-span-full">
                        <h3 className="card-title">Tóm tắt đơn hàng</h3>
                        <div style={{ display: 'grid', gap: '1rem', marginTop: '1rem' }}>
                          <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                            <span className="card-subtext">Địa điểm:</span>
                            <span className="card-subtext" style={{ fontWeight: 'bold' }}>{selectedStation?.name}</span>
                          </div>
                          <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                            <span className="card-subtext">Xe:</span>
                            <span className="card-subtext" style={{ fontWeight: 'bold' }}>{selectedModel?.manufacturer} {selectedModel?.modelName}</span>
                          </div>
                          <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                            <span className="card-subtext">Giá thuê:</span>
                            <span className="card-subtext" style={{ fontWeight: 'bold' }}>${preview.totalRentalCost?.toFixed(2)}</span>
                          </div>
                          <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                            <span className="card-subtext">Tiền cọc:</span>
                            <span className="card-subtext" style={{ fontWeight: 'bold' }}>${preview.depositAmount?.toFixed(2)}</span>
                          </div>
                          <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                            <span className="card-subtext">Phí dịch vụ:</span>
                            <span className="card-subtext" style={{ fontWeight: 'bold' }}>${preview.serviceFee?.toFixed(2)}</span>
                          </div>
                          <hr style={{ margin: '1rem 0' }} />
                          <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                            <h4 style={{ color: '#ff4d30', fontSize: '1.8rem' }}>Tổng cộng:</h4>
                            <h4 style={{ color: '#ff4d30', fontSize: '1.8rem' }}>${preview.totalPaymentAmount?.toFixed(2)}</h4>
                          </div>
                        </div>
                      </div>
                    )}

                    {bookingError && (
                      <div className="error-message error-visible grid-span-full">
                        <span>{bookingError}</span>
                      </div>
                    )}
                  </div>
                )}

                <div className="row-between" style={{ marginTop: '2rem' }}>
                  <CTA as="button" variant="ghost" onClick={back}>Quay lại</CTA>
                  {step < steps.length - 1 ? (
                    <CTA as="button" onClick={next}>Tiếp tục</CTA>
                  ) : (
                    <CTA as="button" onClick={handleConfirmBooking} disabled={!preview || bookingLoading}>
                      {bookingLoading ? 'Đang xử lý...' : 'Xác nhận & Thanh toán'}
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
