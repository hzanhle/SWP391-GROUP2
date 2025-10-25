import React from 'react'

export default function BookingStep2_SelectModel({ models, vehicles, selectedModel, onSelectModel }) {
  // Debug logging
  console.log('[Step2] Props received:', {
    modelsCount: models.length,
    vehiclesCount: vehicles.length,
    vehiclesSample: vehicles.slice(0, 1),
    selectedModel,
  })

  return (
    <div className="model-selection-container">
      <label className="label booking-section-label">Chọn mẫu xe</label>
      
      {models.length === 0 ? (
        <div className="error-message error-visible warning" role="alert">
          <span>Không tìm thấy mẫu xe nào.</span>
        </div>
      ) : (
        <div className="models-grid">
          {models.map(model => {
            const modelId = model.ModelId ?? model.modelId ?? model.id
            const selectedId = selectedModel?.ModelId ?? selectedModel?.modelId ?? selectedModel?.id

            // Filter vehicles by this model
            const modelVehicles = vehicles.length > 0 ? vehicles.filter(v => {
              const vModelId = v.ModelId ?? v.modelId ?? v.id
              const match = vModelId === modelId
              if (match) {
                console.log('[Step2] Vehicle matched model:', { modelId, vModelId, vehicle: v })
              }
              return match
            }) : []

            // If no vehicles data, assume available (will validate on booking)
            const hasAvailableVehicles = vehicles.length === 0 || modelVehicles.length > 0
            const isThisSelected = modelId && selectedId && (modelId === selectedId)

            if (vehicles.length > 0 && modelVehicles.length === 0) {
              console.log('[Step2] No vehicles for model:', { modelId, modelName: model.ModelName || model.modelName })
            }

            const manufacturer = model.Manufacturer || model.manufacturer || ''
            const modelName = model.ModelName || model.modelName || ''
            const batteryCapacity = model.BatteryCapacity || model.batteryCapacity
            const batteryRange = model.BatteryRange || model.batteryRange
            const vehicleCapacity = model.VehicleCapacity || model.vehicleCapacity
            const rentFee = model.RentFeeForHour || model.rentFeeForHour

            return (
              <div
                key={`model-${modelId}`}
                className={`model-card-item ${isThisSelected ? 'selected' : ''} ${!hasAvailableVehicles ? 'unavailable' : ''}`}
                onClick={() => {
                  if (hasAvailableVehicles) {
                    onSelectModel(model)
                  }
                }}
                onKeyDown={(e) => {
                  if ((e.key === 'Enter' || e.key === ' ') && hasAvailableVehicles) {
                    e.preventDefault()
                    onSelectModel(model)
                  }
                }}
                role="button"
                tabIndex={hasAvailableVehicles ? 0 : -1}
                aria-pressed={isThisSelected}
                aria-disabled={!hasAvailableVehicles}
                aria-label={`Select ${manufacturer} ${modelName}`}
              >
                <div className="model-card-header">
                  <h4 className="model-card-name">{manufacturer} {modelName}</h4>
                  {!hasAvailableVehicles && vehicles.length > 0 && (
                    <span className="model-card-status unavailable">Hết xe</span>
                  )}
                </div>

                <div className="model-card-specs">
                  {batteryCapacity && (
                    <div className="model-spec">
                      <span className="spec-label">🔋 Pin:</span>
                      <span className="spec-value">{batteryCapacity} kWh</span>
                    </div>
                  )}
                  {batteryRange && (
                    <div className="model-spec">
                      <span className="spec-label">📏 Quãng đường:</span>
                      <span className="spec-value">{batteryRange} km</span>
                    </div>
                  )}
                  {vehicleCapacity && (
                    <div className="model-spec">
                      <span className="spec-label">👥 Chỗ ngồi:</span>
                      <span className="spec-value">{vehicleCapacity}</span>
                    </div>
                  )}
                </div>

                <div className="model-card-price">
                  <span className="price-label">Giá thuê:</span>
                  <span className="price-value">${rentFee}/giờ</span>
                </div>

                {isThisSelected && (
                  <div className="model-card-checkmark">✓</div>
                )}
              </div>
            )
          })}
        </div>
      )}
    </div>
  )
}
