import React, { useState } from 'react'
import CTA from './CTA'

export default function Navbar() {
  const [open, setOpen] = useState(false)
  return (
    <header className="navbar" data-figma-layer="Navbar" data-tailwind='class: "sticky top-0 z-50 bg-white border-b border-slate-200"'>
      <div className="container navbar-inner" role="navigation" aria-label="Primary" data-tailwind='class: "flex items-center justify-between px-6 py-4"'>
        <a href="#" className="brand" aria-label="EVStation Home" data-figma-layer="Logo" data-tailwind='class: "flex items-center gap-3 font-bold text-xl text-slate-900"'>
          <svg width="28" height="28" viewBox="0 0 24 24" fill="none" aria-hidden="true" data-export="svg"><path d="M6 3h7a3 3 0 0 1 3 3v5h1a2 2 0 0 1 2 2v5h-2v-5h-1v5H4v-8a7 7 0 0 1 2-5V3z" stroke="#0ea5e9" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/></svg>
          <span>EVStation</span>
        </a>

        <nav className="nav-links" aria-label="Primary navigation" data-figma-layer="NavLinks" data-tailwind='class: "hidden md:flex gap-6"'>
          <a className="nav-link" href="#stations" data-tailwind='class: "text-slate-500 hover:text-slate-900 px-3 py-2 rounded-md"'>Stations</a>
          <a className="nav-link" href="#how" data-tailwind='class: "text-slate-500 hover:text-slate-900 px-3 py-2 rounded-md"'>How it works</a>
          <a className="nav-link" href="#pricing" data-tailwind='class: "text-slate-500 hover:text-slate-900 px-3 py-2 rounded-md"'>Pricing</a>
          <a className="nav-link" href="#support" data-tailwind='class: "text-slate-500 hover:text-slate-900 px-3 py-2 rounded-md"'>Support</a>
        </nav>

        <div className="nav-cta" data-figma-layer="NavCTA" data-tailwind='class: "hidden md:inline-flex"'>
          <CTA as="a" href="#signup" aria-label="Sign up" data-figma-layer="CTA" data-tailwind='class: "bg-sky-500 text-white px-5 py-3 rounded-lg shadow-md"'>Sign up</CTA>
        </div>

        <button aria-label="Toggle menu" className="menu-toggle btn btn-ghost" onClick={() => setOpen(v=>!v)} data-tailwind='class: "inline-flex md:hidden items-center gap-2 border border-slate-200 px-3 py-2 rounded-md"'>
          <span>Menu</span>
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" aria-hidden="true" data-export="svg"><path d="M4 6h16M4 12h16M4 18h16" stroke="#0f172a" strokeWidth="2" strokeLinecap="round"/></svg>
        </button>
      </div>

      {open && (
        <div className="container" role="menu" data-figma-layer="MobileMenu" data-tailwind='class: "md:hidden px-6 pb-4"'>
          <div className="card card-body">
            <a className="nav-link" role="menuitem" href="#stations">Stations</a>
            <a className="nav-link" role="menuitem" href="#how">How it works</a>
            <a className="nav-link" role="menuitem" href="#pricing">Pricing</a>
            <a className="nav-link" role="menuitem" href="#support">Support</a>
            <CTA as="a" href="#signup" className="mt-2" data-tailwind='class: "mt-2"'>Sign up</CTA>
          </div>
        </div>
      )}
    </header>
  )
}
