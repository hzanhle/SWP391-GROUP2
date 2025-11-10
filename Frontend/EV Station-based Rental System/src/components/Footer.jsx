import { useState } from 'react'

function Footer() {
  const [email, setEmail] = useState('')
  const [msg, setMsg] = useState('')
  const [status, setStatus] = useState('idle') // idle | error | success

  function onSubmit(e) {
    e.preventDefault()
    const ok = /.+@.+\..+/.test(email)
    if (!ok) {
      setStatus('error')
      setMsg('Please enter a valid email address.')
      return
    }
    setStatus('success')
    setMsg('Thanks for subscribing!')
  }

  return (
    <>
      <footer>
        <div className="container">
          <div className="footer-content">
            <ul className="footer-content__1">
              <li>
                <span>EV</span> Station
              </li>
              <li>
                We offer a wide selection of premium electric vehicles for all your driving needs. We have the perfect EV to meet your requirements.
              </li>
              <li>
                <a href="tel:123456789">
                  <i className="fa-solid fa-phone" aria-hidden="true"></i> &nbsp; (123) -456-789
                </a>
              </li>

              <li>
                <a
                  href="mailto:evstation@example.com"
                >
                  <i className="fa-solid fa-envelope" aria-hidden="true"></i>
                  &nbsp; evstation@example.com
                </a>
              </li>

              <li>
                <a
                  className="footer-credits"
                  target="_blank"
                  rel="noreferrer"
                  href="https://evstation.com"
                >
                  Design with ❤️ for EV Enthusiasts
                </a>
              </li>
            </ul>

            <ul className="footer-content__2">
              <li>Company</li>
              <li>
                <a href="#home">Gallery</a>
              </li>
              <li>
                <a href="#home">Careers</a>
              </li>
              <li>
                <a href="#home">Mobile App</a>
              </li>
              <li>
                <a href="#home">Blog</a>
              </li>
              <li>
                <a href="#home">How we work</a>
              </li>
            </ul>

            <ul className="footer-content__2">
              <li>Working Hours</li>
              <li>Mon - Fri: 8:00AM - 10:00PM</li>
              <li>Sat: 9:00AM - 8:00PM</li>
              <li>Sun: 10:00AM - 6:00PM</li>
            </ul>

            <ul className="footer-content__2">
              <li>Subscription</li>
              <li>
                <p>Subscribe to get special offers and the latest news.</p>
              </li>
              <li>
                <form onSubmit={onSubmit} noValidate>
                  <label htmlFor="subscribe-email" className="visually-hidden">Email address</label>
                  <input
                    id="subscribe-email"
                    type="email"
                    placeholder="Enter Email Address"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    aria-invalid={status === 'error'}
                    required
                  />
                  <button className="submit-email" type="submit">Submit</button>
                  <div role="status" aria-live="polite" className={`form-hint ${status}`}>
                    {msg}
                  </div>
                </form>
              </li>
            </ul>
          </div>
        </div>
      </footer>
    </>
  );
}

export default Footer;
