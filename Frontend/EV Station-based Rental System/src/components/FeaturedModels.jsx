import { useState, useEffect } from "react";
import CarBox from "./CarBox";
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
    } catch (err) {
      setError(err.message);
      console.error("Failed to fetch models:", err);
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
            <div className="loading-message">Loading vehicles...</div>
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
            <div className="error-message">Failed to load vehicles. Please try again later.</div>
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
              <div className="pick-box">
                {models.map((model, index) => (
                  <button
                    key={model.modelId}
                    className={`${coloringButton(`btn${index + 1}`)}`}
                    onClick={() => handleModelSelect(model, `btn${index + 1}`)}
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
