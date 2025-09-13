import React from 'react'
import CTA from './CTA'
import illustration from '../assets/ev-illustration.svg'

export default function Hero() {
  return (
    <section id="hero" className="hero section" data-figma-layer="Hero" data-tailwind='class: "bg-gradient-to-b from-sky-500/10 to-transparent py-10"'>
      <div className="container hero-inner">
        <div>
          <h1 className="hero-title" data-figma-layer="H1" data-tailwind='class: "text-5xl font-bold"'>Charge, drive, repeat.</h1>
          <p className="hero-sub" data-figma-layer="Subtext" data-tailwind='class: "text-slate-600 text-lg mt-2"'>Find nearby EV charging stations and rent vehicles in minutes—simple, fast, and reliable.</p>
          <div className="hero-actions" data-figma-layer="Actions" data-tailwind='class: "flex gap-4 mt-6 flex-wrap"'>
            <CTA as="a" href="#search" aria-label="Find a Station" data-figma-layer="CTA" data-tailwind='class: "bg-sky-500 text-white px-6 py-3 rounded-lg shadow-md"'>Find a Station</CTA>
            <CTA as="a" href="#how" variant="ghost" aria-label="Learn how it works" data-figma-layer="CTA" data-tailwind='class: "border border-slate-200 text-slate-900 px-6 py-3 rounded-lg"'>How it works</CTA>
          </div>
        </div>
        <div className="hero-media" role="img" aria-label="App preview mockup" data-figma-layer="HeroMedia" data-tailwind='class: "rounded-lg border border-slate-200 p-4 shadow-md bg-white"'>
          <img src={illustration} alt="EV app mockup illustration" style={{width: '100%'}} />
        </div>
      </div>
    </section>
  )
}
