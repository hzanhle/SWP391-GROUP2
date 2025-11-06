import { useEffect, useState } from "react";
import vehicle from '../assets/ev-illustration.png';
function Hero() {
  const [goUp, setGoUp] = useState(false);

  const scrollToTop = () => {
    window.scrollTo({ top: (0, 0), behavior: "smooth" });
  };

  const bookBtn = () => {
    window.location.hash = 'booking-new';
  };

  useEffect(() => {
    const onPageScroll = () => {
      if (window.pageYOffset > 600) {
        setGoUp(true);
      } else {
        setGoUp(false);
      }
    };
    window.addEventListener("scroll", onPageScroll);

    return () => {
      window.removeEventListener("scroll", onPageScroll);
    };
  }, []);
  return (
    <>
      <section id="home" className="hero-section">
        <div className="container">
          <div className="hero-content">
            <div className="hero-content__text">
              <h4>Charge your journey</h4>
              <h1>
                Power up your <span>EV experience</span>
              </h1>
              <p>
                Find the perfect electric vehicle for your needs. Competitive rates, flexible booking options, and seamless charging station access everywhere.
              </p>
              <div className="hero-content__text__btns">
                <a
                  onClick={bookBtn}
                  className="hero-content__text__btns__book-ride"
                  href="#booking-new"
                >
                  Book Now &nbsp; <i className="fa-solid fa-circle-check" aria-hidden="true"></i>
                </a>
                <a className="hero-content__text__btns__learn-more" href="#">
                  Learn More &nbsp; <i className="fa-solid fa-angle-right" aria-hidden="true"></i>
                </a>
              </div>
            </div>

            {/* decorative media */}
            <div className="hero-media" aria-hidden="true">
              <img src={vehicle} alt="vehicle-img" className="hero-content__car-img" loading="lazy" decoding="async" />
            </div>
          </div>
        </div>

        {/* page up */}
        <div
          onClick={scrollToTop}
          className={`scroll-up ${goUp ? "show-scroll" : ""}`}
          role="button"
          aria-label="Scroll to top"
          tabIndex={0}
          onKeyDown={(e) => (e.key === 'Enter' ? scrollToTop() : null)}
        >
          <i className="fa-solid fa-angle-up" aria-hidden="true"></i>
        </div>
      </section>
    </>
  );
}

export default Hero;
