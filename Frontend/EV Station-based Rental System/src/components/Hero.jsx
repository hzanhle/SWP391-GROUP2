import { useEffect, useState } from "react";
import vehicle from '../assets/ev-illustration.png';
function Hero() {
  const [goUp, setGoUp] = useState(false);

  const scrollToTop = () => {
    window.scrollTo({ top: (0, 0), behavior: "smooth" });
  };

  const bookBtn = () => {
    document
      .querySelector("#booking-section")
      .scrollIntoView({ behavior: "smooth" });
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
                  href="#booking-section"
                  style={{color: "white", textDecoration: "none"}}
                >
                  Book Now &nbsp; <i className="fa-solid fa-circle-check"></i>
                </a>
                <a className="hero-content__text__btns__learn-more" href="#" style={{color: "white", textDecoration: "none"}}>
                  Learn More &nbsp; <i className="fa-solid fa-angle-right"></i>
                </a>
              </div>
            </div>

            {/* img placeholder */}
            <div
              style={{
                width: "65%",
                height: "400px",
                background: "linear-gradient(135deg, #ff4d30 0%, #ffe5db 100%)",
                borderRadius: "10px",
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                color: "white",
                fontSize: "2rem"
              }}
            >
              <img src={vehicle} alt="vehicle-img" class="hero-content__car-img"></img>
            </div>
          </div>
        </div>

        {/* page up */}
        <div
          onClick={scrollToTop}
          className={`scroll-up ${goUp ? "show-scroll" : ""}`}
        >
          <i className="fa-solid fa-angle-up"></i>
        </div>
      </section>
    </>
  );
}

export default Hero;
