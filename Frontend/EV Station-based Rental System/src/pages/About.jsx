import Navbar from '../components/Navbar'
import Footer from '../components/Footer'
import vehicle from '../assets/ev-illustration.png'

export default function About() {
  return (
    <div data-figma-layer="About Page">
      <Navbar />
      <main>
        {/* Hero */}
        <section className="section about-hero" aria-labelledby="about-title">
          <div className="container">
            <div className="about-hero__grid">
              <div className="about-hero__text">
                <h4 className="section-subtitle">About Us</h4>
                <h1 id="about-title" className="about-hero__title">
                  Driving the future of <span>electric mobility</span>
                </h1>
                <p className="card-subtext">
                  We’re an EV-first rental platform committed to sustainable urban mobility. Our mission is to make electric
                  vehicles accessible, reliable, and enjoyable for everyone—whether you’re commuting, exploring, or traveling.
                </p>
                <div className="hero-content__text__btns">
                  <a className="hero-content__text__btns__book-ride" href="#booking-new">Book Now&nbsp; <i className="fa-solid fa-circle-check" aria-hidden="true"></i></a>
                  <a className="hero-content__text__btns__learn-more" href="#vehicles">Explore Fleet&nbsp; <i className="fa-solid fa-angle-right" aria-hidden="true"></i></a>
                </div>
              </div>
              <div className="about-hero__media" aria-hidden="true">
                <img src={vehicle} alt="Modern electric vehicle" className="about-hero__image" loading="lazy" decoding="async" />
              </div>
            </div>
          </div>
        </section>

        {/* Mission & Values */}
        <section className="section">
          <div className="container">
            <div className="section-header">
              <h2 className="section-title">Our Mission & Values</h2>
              <p className="section-subtitle">We focus on safety, sustainability, and a delightful rental experience.</p>
            </div>
            <div className="two-col-grid">
              <div className="card">
                <div className="card-body">
                  <h3 className="card-title">Sustainable by Design</h3>
                  <p className="card-subtext">
                    From zero-emission vehicles to energy-efficient operations, sustainability is embedded in everything we do.
                    We partner with trusted charging networks for seamless trips and reliable range.
                  </p>
                </div>
              </div>
              <div className="card">
                <div className="card-body">
                  <h3 className="card-title">Customer-First Experience</h3>
                  <p className="card-subtext">
                    Transparent pricing, flexible scheduling, and responsive support. We’re here to ensure your rental is smooth
                    from booking to return.
                  </p>
                </div>
              </div>
            </div>
          </div>
        </section>

        {/* Fleet & Stations */}
        <section className="section">
          <div className="container">
            <div className="section-header">
              <h2 className="section-title">Premium Fleet & Charging Access</h2>
              <p className="section-subtitle">Curated electric vehicles with easy access to our station network.</p>
            </div>
            <div className="two-col-grid">
              <div className="card map-card">
                <div className="card-body">
                  <h3 className="card-title">Quality You Can Trust</h3>
                  <p className="card-subtext">
                    Each vehicle is maintained and inspected regularly. Pick from efficient city riders to long-range models
                    for weekend getaways.
                  </p>
                </div>
              </div>
              <div className="card">
                <div className="card-body">
                  <h3 className="card-title">Charge with Confidence</h3>
                  <p className="card-subtext">
                    We integrate with station data to help you plan, locate, and charge efficiently. Your journey stays simple
                    and predictable.
                  </p>
                </div>
              </div>
            </div>
          </div>
        </section>

        {/* Call to action */}
        <section className="section">
          <div className="container">
            <div className="card">
              <div className="card-body row-between about-cta-row">
                <div>
                  <h3 className="card-title">Ready to experience electric?</h3>
                  <p className="card-subtext">Choose a model and book in minutes. It’s fast, clean, and convenient.</p>
                </div>
                <a className="hero-content__text__btns__book-ride" href="#booking-new">Start Booking</a>
              </div>
            </div>
          </div>
        </section>
      </main>
      <Footer />
    </div>
  )
}
