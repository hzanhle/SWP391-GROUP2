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

    function onKey(e) {
      if (e.key === 'Escape') setNav(false);
    }

    window.addEventListener('storage', handleStorageChange);
    window.addEventListener('keydown', onKey);
    return () => {
      window.removeEventListener('storage', handleStorageChange);
      window.removeEventListener('keydown', onKey);
    };
  }, []);

  const toggleNav = () => {
    setNav((v) => !v);
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
      <nav role="navigation" aria-label="Primary">
        {/* mobile drawer */}
        <div
          id="mobile-menu"
          className={`mobile-navbar ${nav ? "open-nav" : ""}`}
          aria-hidden={!nav}
        >
          <button
            type="button"
            onClick={toggleNav}
            className="mobile-navbar__close"
            aria-label="Close menu"
          >
            <i className="fa-solid fa-xmark" aria-hidden="true"></i>
          </button>
          <ul className="mobile-navbar__links" role="menu">
            {!isStaff && (
              <>
                <li role="none">
                  <a onClick={toggleNav} href="#" role="menuitem">
                    Home
                  </a>
                </li>
                <li role="none">
                  <a onClick={toggleNav} href="#about" role="menuitem">
                    About
                  </a>
                </li>
                <li role="none">
                  <a onClick={toggleNav} href="#vehicles" role="menuitem">
                    Vehicles
                  </a>
                </li>
                <li role="none">
                  <a onClick={toggleNav} href="#testimonials" role="menuitem">
                    Testimonials
                  </a>
                </li>
                <li role="none">
                  <a onClick={toggleNav} href="#stations" role="menuitem">
                    Stations
                  </a>
                </li>
                <li role="none">
                  <a onClick={toggleNav} href="#contact" role="menuitem">
                    Contact
                  </a>
                </li>
              </>
            )}
            {isStaff && (
              <>
                <li role="none">
                  <a onClick={toggleNav} href="#staff-shifts" role="menuitem">
                    My Shifts
                  </a>
                </li>
                <li role="none">
                  <a onClick={toggleNav} href="#staff-vehicles" role="menuitem">
                    Vehicle Management
                  </a>
                </li>
                <li role="none">
                  <a onClick={toggleNav} href="#staff-verify" role="menuitem">
                    Verification
                  </a>
                </li>
              </>
            )}
            {user && (
              <>
                <li role="none">
                  <a onClick={toggleNav} href="#profile" role="menuitem">
                    My Account
                  </a>
                </li>
                <li role="none">
                  <a
                    onClick={() => {
                      handleLogout();
                      toggleNav();
                    }}
                    href="#"
                    role="menuitem"
                  >
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
            <a href="#" onClick={() => window.scrollTo(0, 0)} className="navbar__brand">
              EV
            </a>
          </div>
          <ul className="navbar__links" role="menubar">
            {!isStaff && (
              <>
                <li role="none">
                  <a className="home-link" href="#" onClick={() => (window.location.hash = "")} role="menuitem">
                    Home
                  </a>
                </li>
                <li role="none">
                  <a className="about-link" href="#about" role="menuitem">
                    About
                  </a>
                </li>
                <li role="none">
                  <a className="models-link" href="#vehicles" role="menuitem">
                    Vehicles
                  </a>
                </li>
                <li role="none">
                  <a className="testi-link" href="#testimonials" role="menuitem">
                    Testimonials
                  </a>
                </li>
                <li role="none">
                  <a className="stations-link" href="#stations" role="menuitem">
                    Stations
                  </a>
                </li>
                <li role="none">
                  <a className="contact-link" href="#contact" role="menuitem">
                    Contact
                  </a>
                </li>
              </>
            )}
            {isStaff && (
              <>
                <li role="none">
                  <a className="shifts-link" href="#staff-shifts" role="menuitem">
                    My Shifts
                  </a>
                </li>
                <li role="none">
                  <a className="vehicles-link" href="#staff-vehicles" role="menuitem">
                    Vehicles
                  </a>
                </li>
                <li role="none">
                  <a className="verification-link" href="#staff-verify" role="menuitem">
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
                  <div className="navbar__quick-links">
                    <a href="#history" className="navbar-link" title="History">
                      <i className="fa-solid fa-history" aria-hidden="true"></i>
                      <span>History</span>
                    </a>
                    <a href="#feedback" className="navbar-link" title="Feedback">
                      <i className="fa-solid fa-comment" aria-hidden="true"></i>
                      <span>Feedback</span>
                    </a>
                  </div>
                )}
                <a href="#profile" className="navbar__profile-btn">
                  <i className="fa-solid fa-user" aria-hidden="true"></i>
                  <span>Profile</span>
                </a>
                <button className="navbar__logout-btn" onClick={handleLogout} type="button">
                  <i className="fa-solid fa-sign-out-alt" aria-hidden="true"></i>
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

          {/* mobile trigger */}
          <button
            type="button"
            className="mobile-hamb"
            onClick={toggleNav}
            aria-controls="mobile-menu"
            aria-expanded={nav}
            aria-label="Open menu"
          >
            <i className="fa-solid fa-bars" aria-hidden="true"></i>
          </button>
        </div>
      </nav>
    </>
  );
}

export default Navbar;
