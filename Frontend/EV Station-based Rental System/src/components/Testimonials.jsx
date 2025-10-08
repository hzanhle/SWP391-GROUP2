import React from 'react'

const items = [
  { quote: 'Booking an EV was faster than ordering coffee.', author: 'Alex M.' },
  { quote: 'Reliable stations and clean vehicles—love it.', author: 'Priya R.' },
  { quote: 'The app made my commute effortless.', author: 'Diego S.' },
]

export default function Testimonials() {
  return (
    <section className="section" aria-labelledby="testimonials-title" data-figma-layer="Testimonials" data-tailwind='class: "py-10"'>
      <div className="container">
        <div className="section-header">
          <h2 id="testimonials-title" className="section-title">What drivers say</h2>
          <p className="section-subtitle">Real feedback from our community.</p>
        </div>
        <div className="testimonials">
          {items.map((t) => (
            <blockquote key={t.author} className="card card-body" data-export="png">
              <p className="quote">“{t.quote}”</p>
              <cite className="quote-author">{t.author}</cite>
            </blockquote>
          ))}
        </div>
      </div>
    </section>
  )
}
