import React from 'react'
import CTA from '../components/CTA'

export default function BookingStep3_Schedule({
  selectedStation,
  selectedModel,
  pickupDate,
  dropoffDate,
  onPickupDateChange,
  onDropoffDateChange,
  preview,
  previewLoading,
  previewError,
  bookingLoading,
  bookingError,
  onPreview,
  onConfirmBooking,
}) {
  return (
    <div className="booking-grid">
      <div className="field">
        <label htmlFor="pickup-date" className="label">Thời gian nhận xe</label>
        <input 
          id="pickup-date"
          type="datetime-local" 
          className="input"
          value={pickupDate}
          onChange={(e) => onPickupDateChange(e.target.value)}
        />
      </div>
      <div className="field">
        <label htmlFor="dropoff-date" className="label">Thời gian trả xe</label>
        <input 
          id="dropoff-date"
          type="datetime-local" 
          className="input"
          value={dropoffDate}
          onChange={(e) => onDropoffDateChange(e.target.value)}
        />
      </div>

      {previewError && (
        <div className="error-message error-visible grid-span-full">
          <span>{previewError}</span>
        </div>
      )}

      {!preview && !previewLoading && (
        <div className="field grid-span-full text-center">
          <CTA 
            as="button" 
            onClick={onPreview} 
            disabled={!selectedModel || !pickupDate || !dropoffDate}
          >
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
              <h4 style={{ color: '#ff4d30', fontSize: '1.8rem', margin: 0 }}>Tổng cộng:</h4>
              <h4 style={{ color: '#ff4d30', fontSize: '1.8rem', margin: 0 }}>${preview.totalPaymentAmount?.toFixed(2)}</h4>
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
  )
}
