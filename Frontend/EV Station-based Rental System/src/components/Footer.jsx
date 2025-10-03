import React from 'react'

export default function Footer() {
  return (
    <footer className="footer footer-gradient" role="contentinfo" data-figma-layer="Footer" data-tailwind='class: "bg-white border-t border-slate-200"'>
      <div className="container footer-inner">
        <div>
          <div className="brand" aria-label="EVStation">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" aria-hidden="true" data-export="svg"><path d="M6 3h7a3 3 0 0 1 3 3v5h1a2 2 0 0 1 2 2v5h-2v-5h-1v5H4v-8a7 7 0 0 1 2-5V3z" stroke="#0ea5e9" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/></svg>
            <span>EVStation</span>
          </div>
          <p className="section-subtitle">Charge, drive, repeat.</p>
        </div>
        <div>
          <h4 className="footer-title">Links</h4>
          <a href="#stations" className="footer-link">Stations</a><br />
          <a href="#how" className="footer-link">How it works</a><br />
          <a href="#pricing" className="footer-link">Pricing</a>
        </div>
        <div>
          <h4 className="footer-title">Support</h4>
          <a href="#support" className="footer-link">Help Center</a><br />
          <a href="#contact" className="footer-link">Contact</a>
        </div>
        <div>
          <h4 className="footer-title">Social</h4>
          <div className="row" aria-label="Social links">
            <a className="footer-link" href="#" aria-label="Twitter">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M22 5.8c-.7.3-1.4.5-2.1.6.8-.5 1.3-1.2 1.6-2.1-.8.5-1.7.9-2.6 1.1C18 4.6 17 4 15.8 4c-2.2 0-3.9 1.9-3.5 4-.1 0-4.6-.2-7.6-3.9-1 1.8-.5 4 1.2 5.1-.6 0-1.2-.2-1.7-.5 0 1.9 1.3 3.5 3.1 3.9-.6.2-1.2.2-1.7.1.5 1.6 2 2.7 3.7 2.7-1.6 1.3-3.6 2-5.6 1.7 1.8 1.1 3.9 1.7 6 1.7 7.3 0 11.3-6.1 11-11.5.8-.5 1.4-1.2 1.9-2.1z"/></svg>
              <span className="sr-only">Twitter</span>
            </a>
            <a className="footer-link" href="#" aria-label="Instagram">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M7 2h10a5 5 0 0 1 5 5v10a5 5 0 0 1-5 5H7a5 5 0 0 1-5-5V7a5 5 0 0 1 5-5zm5 5a5 5 0 1 0 .001 10.001A5 5 0 0 0 12 7zm6.5-.8a1.2 1.2 0 1 0 0 2.4 1.2 1.2 0 0 0 0-2.4z"/></svg>
              <span className="sr-only">Instagram</span>
            </a>
            <a className="footer-link" href="#" aria-label="LinkedIn">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M4.98 3.5C4.98 4.88 3.86 6 2.5 6S0 4.88 0 3.5 1.12 1 2.5 1s2.48 1.12 2.48 2.5zM.5 8h4V24h-4V8zm7 0h3.8v2.2h.1c.5-.9 1.8-2.2 3.8-2.2C20.6 8 22 10.4 22 14.2V24h-4v-8.4c0-2-.7-3.3-2.5-3.3-1.4 0-2.2 1-2.6 2-.1.3-.1.8-.1 1.2V24h-4V8z"/></svg>
              <span className="sr-only">LinkedIn</span>
            </a>
          </div>
        </div>
      </div>
      <div className="footer-bottom">
        <small>© {new Date().getFullYear()} EVStation</small>
        <small>All rights reserved</small>
      </div>
    </footer>
  )
}
