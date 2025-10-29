import { useState } from "react";

function CarBox({ model }) {
  const [imageLoading, setImageLoading] = useState(true);
  const apiBaseUrl = (import.meta.env.VITE_VEHICLE_API_URL || import.meta.env.VITE_API_URL || '').replace(/\/$/, '');

  if (!model) {
    return null;
  }

  // Get image URL from API or use placeholder
  const placeholderImg = "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='400' height='300'%3E%3Crect fill='%23ff4d30' width='400' height='300'/%3E%3C/svg%3E";
  let imageUrl = placeholderImg;

  if (model.imageUrls && model.imageUrls.length > 0) {
    const imagePath = model.imageUrls[0];
    if (imagePath.startsWith('http')) {
      imageUrl = imagePath;
    } else {
      const filename = imagePath.split('/').pop();
      imageUrl = `${apiBaseUrl}/api/Model/image/${filename}`;
    }
  }

  return (
    <div className="box-cars">
      <div className="pick-car">
        {imageLoading && <span className="loader"></span>}
        <img
          style={{ display: imageLoading ? "none" : "block" }}
          src={imageUrl}
          alt={`${model.manufacturer} ${model.modelName}`}
          onLoad={() => setImageLoading(false)}
          onError={() => {
            setImageLoading(false);
          }}
        />
      </div>
      <div className="pick-description">
        <div className="pick-description__price">
          <span>${model.rentFeeForHour}</span>/ rent per hour
        </div>
        <div className="pick-description__table">
          <div className="pick-description__table__col">
            <span>Model</span>
            <span>{model.modelName}</span>
          </div>

          <div className="pick-description__table__col">
            <span>Brand</span>
            <span>{model.manufacturer}</span>
          </div>

          <div className="pick-description__table__col">
            <span>Year</span>
            <span>{model.year}</span>
          </div>

          <div className="pick-description__table__col">
            <span>Capacity</span>
            <span>{model.vehicleCapacity} seats</span>
          </div>

          <div className="pick-description__table__col">
            <span>Range</span>
            <span>{model.batteryRange} km</span>
          </div>

          <div className="pick-description__table__col">
            <span>Max Speed</span>
            <span>{model.maxSpeed} km/h</span>
          </div>

          <div className="pick-description__table__col">
            <span>Battery</span>
            <span>{model.batteryCapacity} mAh</span>
          </div>
        </div>
        <a className="cta-btn" href="#booking-section">
          Reserve Now
        </a>
      </div>
    </div>
  );
}

export default CarBox;
