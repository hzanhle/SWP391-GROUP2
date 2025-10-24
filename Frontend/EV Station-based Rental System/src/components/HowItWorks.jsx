// Icon components as placeholders
const IconBox = ({ icon }) => (
  <div style={{
    width: "100px",
    height: "100px",
    background: "#ff4d30",
    borderRadius: "10px",
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    color: "white",
    fontSize: "2.5rem"
  }}>
    {icon}
  </div>
);

function HowItWorks() {
  return (
    <>
      <section className="plan-section">
        <div className="container">
          <div className="plan-container">
            <div className="plan-container__title">
              <h3>How it works</h3>
              <h2>Simple EV rental process</h2>
            </div>

            <div className="plan-container__boxes">
              <div className="plan-container__boxes__box">
                <IconBox icon="🚗" />
                <h3>Select Vehicle</h3>
                <p>
                  Browse our extensive selection of electric vehicles tailored to meet all your driving needs and preferences.
                </p>
              </div>

              <div className="plan-container__boxes__box">
                <IconBox icon="✓" />
                <h3>Complete Booking</h3>
                <p>
                  Fill in your details and choose your preferred pickup and dropoff stations for seamless rental experience.
                </p>
              </div>

              <div className="plan-container__boxes__box">
                <IconBox icon="⚡" />
                <h3>Enjoy Your Ride</h3>
                <p>
                  Pick up your vehicle and enjoy unlimited access to our charging network across the city.
                </p>
              </div>
            </div>
          </div>
        </div>
      </section>
    </>
  );
}

export default HowItWorks;
