import React from 'react'

function StepIcon({ label }) {
  return (
    <div className="step-icon" aria-hidden="true" data-export="svg">
      <svg width="24" height="24" viewBox="0 0 24 24" fill="none"><path d="M5 12h14M12 5v14" stroke="#0ea5e9" strokeWidth="2" strokeLinecap="round"/></svg>
      <span className="sr-only">{label}</span>
    </div>
  )
}

export default function HowItWorks() {
  return (
    <section id="how" className="section" data-figma-layer="How it works" data-tailwind='class: "py-10"'>
      <div className="container">
        <div className="section-header">
          <h2 className="section-title">How it works</h2>
          <p className="section-subtitle">Three simple steps to get you moving.</p>
        </div>
        <div className="steps">
          <div className="step" data-figma-layer="Step">
            <StepIcon label="Search" />
            <h3>Search</h3>
            <p className="section-subtitle">Find stations and vehicles nearby.</p>
          </div>
          <div className="step" data-figma-layer="Step">
            <StepIcon label="Reserve" />
            <h3>Reserve</h3>
            <p className="section-subtitle">Pick your time and confirm.</p>
          </div>
          <div className="step" data-figma-layer="Step">
            <StepIcon label="Pick up" />
            <h3>Pick up</h3>
            <p className="section-subtitle">Unlock and drive away.</p>
          </div>
        </div>
      </div>
    </section>
  )
}
