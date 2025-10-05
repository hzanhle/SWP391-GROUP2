import React from 'react'

export default function Stepper({ steps = [], current = 0 }) {
  return (
    <ol className="stepper" role="list">
      {steps.map((s, i) => (
        <li key={s} className={i === current ? 'stepper-item active' : i < current ? 'stepper-item done' : 'stepper-item'}>
          <span className="stepper-index" aria-hidden="true">{i + 1}</span>
          <span className="stepper-label">{s}</span>
        </li>
      ))}
    </ol>
  )
}
