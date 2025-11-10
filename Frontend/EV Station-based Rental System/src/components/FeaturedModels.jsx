import { useState, useEffect } from "react";
import CarBox from "./Carbox";
import { getActiveModels } from "../api/vehicle";

function FeaturedModels() {
  const [models, setModels] = useState([]);
  const [selectedModel, setSelectedModel] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [colorBtn, setColorBtn] = useState("btn1");

  useEffect(() => {
    fetchModels();
  }, []);

  const fetchModels = async () => {
    try {
      setLoading(true);
      const response = await getActiveModels();
      const data = Array.isArray(response.data) ? response.data : [];
      setModels(data);

      if (data.length > 0) {
        setSelectedModel(data[0]);
      }
      setError(null);
    } catch (err) {
      setError('Failed to load vehicles. Please try again later.');
    } finally {
      setLoading(false);
    }
  };

  const btnID = (id) => {
    setColorBtn(colorBtn === id ? "" : id);
  };

  const coloringButton = (id) => {
    return colorBtn === id ? "colored-button" : "";
  };

  const handleModelSelect = (model, btnId) => {
    setSelectedModel(model);
    btnID(btnId);
  };

  if (loading) {
    return (
      <section className="pick-section" aria-busy="true">
        <div className="container">
          <div className="pick-container">
            <div className="pick-container__title">
              <h3>Electric Vehicles</h3>
              <h2>Our EV fleet</h2>
              <p>
                Choose from our selection of high-performance electric vehicles for your next adventure or business trip
              </p>
            </div>
            <div className="pick-container__car-content">
              <div className="pick-box">
                {[1,2,3,4,5].map((i) => (
                  <div key={i} className="skeleton skeleton-pill" aria-hidden="true"></div>
                ))}
              </div>
              <div className="pick-car">
                <div className="skeleton skeleton-card" aria-hidden="true"></div>
              </div>
            </div>
          </div>
        </div>
      </section>
    );
  }

  if (error) {
    return (
      <section className="pick-section">
        <div className="container">
          <div className="pick-container">
            <div className="pick-container__title">
              <h3>Electric Vehicles</h3>
              <h2>Our EV fleet</h2>
              <p>
                Choose from our selection of high-performance electric vehicles for your next adventure or business trip
              </p>
            </div>
            <div role="alert" className="error-card">{error}</div>
          </div>
        </div>
      </section>
    );
  }

  return (
    <>
      <section className="pick-section">
        <div className="container">
          <div className="pick-container">
            <div className="pick-container__title">
              <h3>Electric Vehicles</h3>
              <h2>Our EV fleet</h2>
              <p>
                Choose from our selection of high-performance electric vehicles for your next adventure or business trip
              </p>
            </div>
            <div className="pick-container__car-content">
              <div className="pick-box" role="tablist" aria-label="Select vehicle model">
                {models.map((model, index) => (
                  <button
                    key={model.modelId}
                    className={`${coloringButton(`btn${index + 1}`)}`}
                    onClick={() => handleModelSelect(model, `btn${index + 1}`)}
                    role="tab"
                    aria-selected={selectedModel?.modelId === model.modelId}
                  >
                    {model.manufacturer} {model.modelName}
                  </button>
                ))}
              </div>

              {selectedModel && <CarBox model={selectedModel} />}
            </div>
          </div>
        </div>
      </section>
    </>
  );
}

export default FeaturedModels;
