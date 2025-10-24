import { useState } from "react";

function Navbar() {
  const [nav, setNav] = useState(false);

  const openNav = () => {
    setNav(!nav);
  };

  return (
    <>
      <nav>
        {/* mobile */}
        <div className={`mobile-navbar ${nav ? "open-nav" : ""}`}>
          <div onClick={openNav} className="mobile-navbar__close">
            <i className="fa-solid fa-xmark"></i>
          </div>
          <ul className="mobile-navbar__links">
            <li>
              <a onClick={openNav} href="#" style={{color: "inherit"}}>
                Home
              </a>
            </li>
            <li>
              <a onClick={openNav} href="#about" style={{color: "inherit"}}>
                About
              </a>
            </li>
            <li>
              <a onClick={openNav} href="#vehicles" style={{color: "inherit"}}>
                Vehicles
              </a>
            </li>
            <li>
              <a onClick={openNav} href="#testimonials" style={{color: "inherit"}}>
                Testimonials
              </a>
            </li>
            <li>
              <a onClick={openNav} href="#stations" style={{color: "inherit"}}>
                Stations
              </a>
            </li>
            <li>
              <a onClick={openNav} href="#contact" style={{color: "inherit"}}>
                Contact
              </a>
            </li>
          </ul>
        </div>

        {/* desktop */}

        <div className="navbar">
          <div className="navbar__img">
            <a href="#" onClick={() => window.scrollTo(0, 0)} style={{fontSize: "1.8rem", fontWeight: "bold", color: "#ff4d30", textDecoration: "none"}}>
              EV
            </a>
          </div>
          <ul className="navbar__links">
            <li>
              <a className="home-link" href="#" onClick={() => window.location.hash = ""}>
                Home
              </a>
            </li>
            <li>
              {" "}
              <a className="about-link" href="#about">
                About
              </a>
            </li>
            <li>
              {" "}
              <a className="models-link" href="#vehicles">
                Vehicles
              </a>
            </li>
            <li>
              {" "}
              <a className="testi-link" href="#testimonials">
                Testimonials
              </a>
            </li>
            <li>
              {" "}
              <a className="stations-link" href="#stations">
                Stations
              </a>
            </li>
            <li>
              {" "}
              <a className="contact-link" href="#contact">
                Contact
              </a>
            </li>
          </ul>
          <div className="navbar__buttons">
            <a className="navbar__buttons__sign-in" href="#login">
              Sign In
            </a>
            <a className="navbar__buttons__register" href="#signup">
              Register
            </a>
          </div>

          {/* mobile */}
          <div className="mobile-hamb" onClick={openNav}>
            <i className="fa-solid fa-bars"></i>
          </div>
        </div>
      </nav>
    </>
  );
}

export default Navbar;
