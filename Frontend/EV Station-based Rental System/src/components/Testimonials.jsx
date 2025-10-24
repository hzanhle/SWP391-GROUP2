// Avatar placeholder
const AvatarPlaceholder = ({ initials }) => (
  <div style={{
    width: "70px",
    height: "70px",
    borderRadius: "50%",
    background: "linear-gradient(135deg, #ff4d30 0%, #fa4226 100%)",
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    color: "white",
    fontSize: "1.5rem",
    fontWeight: "bold"
  }}>
    {initials}
  </div>
);

function Testimonials() {
  return (
    <>
      <section className="testimonials-section">
        <div className="container">
          <div className="testimonials-content">
            <div className="testimonials-content__title">
              <h4>Reviewed by People</h4>
              <h2>Customer Testimonials</h2>
              <p>
                Discover the positive impact we've made on our clients by reading through their testimonials. Our customers have experienced our service and quality, and they're eager to share their positive experiences with you.
              </p>
            </div>

            <div className="all-testimonials">
              <div className="all-testimonials__box">
                <span className="quotes-icon">
                  <i className="fa-solid fa-quote-right"></i>
                </span>
                <p>
                  "I rented an EV from this service and had an amazing experience! The booking was easy, the rates were affordable, and the vehicles were in perfect condition. Highly recommended!"
                </p>
                <div className="all-testimonials__box__name">
                  <div className="all-testimonials__box__name__profile">
                    <AvatarPlaceholder initials="SJ" />
                    <span>
                      <h4>Sarah Johnson</h4>
                      <p>San Francisco</p>
                    </span>
                  </div>
                </div>
              </div>

              <div className="all-testimonials__box box-2">
                <span className="quotes-icon">
                  <i className="fa-solid fa-quote-right"></i>
                </span>
                <p>
                  "The electric vehicle was in great condition and made my trip fantastic. The charging stations were convenient, and the entire experience was seamless. Will definitely rent again!"
                </p>
                <div className="all-testimonials__box__name">
                  <div className="all-testimonials__box__name__profile">
                    <AvatarPlaceholder initials="MC" />
                    <span>
                      <h4>Michael Chen</h4>
                      <p>Los Angeles</p>
                    </span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>
    </>
  );
}

export default Testimonials;
