import React, { useEffect, useState } from 'react'
import Home from './pages/Home'
import Signup from './pages/Signup'

function resolveRoute() {
  const hash = typeof window !== 'undefined' ? window.location.hash.replace('#', '') : ''
  return hash === 'signup' ? 'signup' : 'home'
}

export default function App() {
  const [route, setRoute] = useState(resolveRoute())

  useEffect(() => {
    const onHashChange = () => setRoute(resolveRoute())
    window.addEventListener('hashchange', onHashChange)
    return () => window.removeEventListener('hashchange', onHashChange)
  }, [])

  return route === 'signup' ? <Signup /> : <Home />
}
