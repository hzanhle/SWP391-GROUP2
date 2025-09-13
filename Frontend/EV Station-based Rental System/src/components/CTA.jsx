import React from 'react'

export default function CTA({ as = 'button', variant = 'primary', children, className = '', ...props }) {
  const Comp = as
  const classes = ['btn', variant === 'primary' ? 'btn-primary' : variant === 'secondary' ? 'btn-secondary' : 'btn-ghost', className].join(' ').trim()
  return (
    <Comp
      data-figma-layer="CTA"
      data-export="svg"
      data-tailwind={variant === 'primary' ? 'class: "bg-sky-500 text-white px-6 py-3 rounded-lg shadow-md hover:brightness-95"' : variant === 'secondary' ? 'class: "bg-slate-200 text-slate-900 px-6 py-3 rounded-lg"' : 'class: "border border-slate-200 text-slate-900 px-6 py-3 rounded-lg bg-transparent"'}
      className={classes}
      {...props}
    >
      {children}
    </Comp>
  )
}
