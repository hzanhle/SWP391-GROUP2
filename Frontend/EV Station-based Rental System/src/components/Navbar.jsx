import { useState, useEffect } from "react";
import NotificationBell from "./NotificationBell";

function Navbar() {
  const [nav, setNav] = useState(false);
  const [user, setUser] = useState(null);

  useEffect(() => {
    const storedUser = localStorage.getItem('auth.user');
    if (storedUser) {
      try {
        setUser(JSON.parse(storedUser));
      } catch {
        setUser(null);
      }
    }

    const handleStorageChange = () => {
      const updated = localStorage.getItem('auth.user');
      if (updated) {
        try {
          setUser(JSON.parse(updated));
        } catch {
          setUser(null);
        }
      } else {
        setUser(null);
      }
    };

    window.addEventListener('storage', handleStorageChange);
    return () => window.removeEventListener('storage', handleStorageChange);
  }, []);

  const openNav = () => {
    setNav(!nav);
  };

  const handleLogout = () => {
    localStorage.removeItem('auth.token');
    localStorage.removeItem('auth.user');
    setUser(null);
    window.location.hash = '';
  };

  const userName = user?.userName || user?.username || 'User';
  const roleId = Number((user && (user.roleId ?? user.RoleId)) ?? 0);
  const isStaff = roleId === 2;
  const isAdmin = roleId === 3;
  const isMember = roleId === 1 || (roleId === 0 && user);

  return (
    <>
      <nav>
        {/* mobile */}
        <div className={`mobile-navbar ${nav ? "open-nav" : ""}`}>
          <div onClick={openNav} className="mobile-navbar__close">
            <i className="fa-solid fa-xmark"></i>
          </div>
          <ul className="mobile-navbar__links">
            {!isStaff && (
              <>
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
              </>
            )}
            {isStaff && (
              <>
                <li>
                  <a onClick={openNav} href="#staff-shifts" style={{color: "inherit"}}>
                    My Shifts
                  </a>
                </li>
                <li>
                  <a onClick={openNav} href="#staff-vehicles" style={{color: "inherit"}}>
                    Vehicle Management
                  </a>
                </li>
                <li>
                  <a onClick={openNav} href="#staff-verify" style={{color: "inherit"}}>
                    Verification
                  </a>
                </li>
              </>
            )}
            {user && (
              <>
                <li>
                  <a onClick={openNav} href={isStaff ? "#profile" : "#profile"} style={{color: "inherit"}}>
                    My Account
                  </a>
                </li>
                <li>
                  <a onClick={() => { handleLogout(); openNav(); }} href="#" style={{color: "inherit"}}>
                    Logout
                  </a>
                </li>
              </>
            )}
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
            {!isStaff && (
              <>
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
              </>
            )}
            {isStaff && (
              <>
                <li>
                  <a className="shifts-link" href="#staff-shifts">
                    My Shifts
                  </a>
                </li>
                <li>
                  <a className="vehicles-link" href="#staff-vehicles">
                    Vehicles
                  </a>
                </li>
                <li>
                  <a className="verification-link" href="#staff-verify">
                    Verification
                  </a>
                </li>
              </>
            )}
          </ul>

          <div className="navbar__right">
            {user ? (
              <div className="navbar__user">
                <span className="navbar__greeting">Hello, {userName}</span>
                <div className="navbar__notifications">
                  <NotificationBell />
                </div>
                {isMember && (
                  <div style={{ display: 'flex', gap: '1rem' }}>
                    <a href="#history" className="navbar-link" title="History">
                      <i className="fa-solid fa-history"></i>
                      <span>History</span>
                    </a>
                    <a href="#feedback" className="navbar-link" title="Feedback">
                      <i className="fa-solid fa-comment"></i>
                      <span>Feedback</span>
                    </a>
                  </div>
                )}
                <a href="#profile" className="navbar__profile-btn">
                  <i className="fa-solid fa-user"></i>
                  <span>Profile</span>
                </a>
                <button className="navbar__logout-btn" onClick={handleLogout}>
                  <i className="fa-solid fa-sign-out-alt"></i>
                  <span>Logout</span>
                </button>
              </div>
            ) : (
              <div className="navbar__buttons">
                <a className="navbar__buttons__sign-in" href="#login">
                  Sign In
                </a>
                <a className="navbar__buttons__register" href="#signup">
                  Register
                </a>
              </div>
            )}
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
