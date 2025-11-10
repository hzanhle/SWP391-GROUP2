import React from 'react'
import StationMap from '../components/StationMap'

export default function BookingStep1_SelectStation({ stations, selectedStation, onSelectStation }) {
  return (
    <div className="station-selection-container">
      <div className="station-selection-map-section">
        <label className="label booking-section-label">Chọn điểm thuê xe trên bản đồ</label>
        <StationMap stations={stations} selectedStation={selectedStation} />
      </div>

      <div className="station-selection-grid-section">
        <label className="label booking-section-label">Hoặc chọn từ danh sách</label>

        {stations.length === 0 ? (
          <div className="error-message error-visible warning" role="alert">
            <span>
              Không tìm thấy điểm thuê. Nếu bạn đang dùng bản preview, frontend không thể truy cập API localhost.
              Chạy frontend cục bộ hoặc cấu hình VITE_STATION_API_URL tới URL công khai để lấy dữ liệu.
            </span>
          </div>
        ) : (
          <div className="stations-grid">
            {stations.map((station, index) => {
              const stationId = station.Id ?? station.stationId ?? station.id
              const selectedId = selectedStation?.Id ?? selectedStation?.stationId ?? selectedStation?.id
              const isThisSelected = stationId && selectedId && (stationId === selectedId)

              return (
                <div
                  key={`station-${stationId || index}`}
                  className={`station-card-item ${isThisSelected ? 'selected' : ''}`}
                  onClick={() => onSelectStation(station)}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter' || e.key === ' ') {
                      e.preventDefault()
                      onSelectStation(station)
                    }
                  }}
                  role="button"
                  tabIndex={0}
                  aria-pressed={isThisSelected}
                  aria-label={`Select ${station.Name || station.name}`}
                >
                  <h4 className="station-card-name">{station.Name || station.name}</h4>
                  <p className="station-card-location">{station.Location || station.location}</p>
                  {isThisSelected && (
                    <div className="station-card-checkmark">✓</div>
                  )}
                </div>
              )
            })}
          </div>
        )}
      </div>
    </div>
  )
}
