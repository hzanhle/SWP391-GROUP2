import React from 'react'

export default function Footer() {
  return (
    <footer className="footer" role="contentinfo" data-figma-layer="Footer" data-tailwind='class: "bg-white border-t border-slate-200"'>
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
          <div style={{display:'flex', gap:'12px'}} aria-label="Social links">
            <a className="footer-link" href="#" aria-label="Twitter">Twitter</a>
            <a className="footer-link" href="#" aria-label="Instagram">Instagram</a>
            <a className="footer-link" href="#" aria-label="LinkedIn">LinkedIn</a>
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
