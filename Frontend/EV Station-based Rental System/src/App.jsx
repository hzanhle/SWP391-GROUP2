import React, { useEffect, useState } from 'react'
import Home from './pages/Home'
import Signup from './pages/Signup'
import Login from './pages/Login'
import ForgotPassword from './pages/ForgotPassword'
import VerifyEmail from './pages/VerifyEmail'

function resolveRoute() {
  const hash = typeof window !== 'undefined' ? window.location.hash.replace('#', '') : ''
  switch (hash) {
    case 'signup': return 'signup'
    case 'login': return 'login'
    case 'forgot-password': return 'forgot-password'
    case 'verify-email': return 'verify-email'
    default: return 'home'
  }
}

export default function App() {
  const [route, setRoute] = useState(resolveRoute())

  useEffect(() => {
    const onHashChange = () => setRoute(resolveRoute())
    window.addEventListener('hashchange', onHashChange)
    return () => window.removeEventListener('hashchange', onHashChange)
  }, [])

  if (route === 'signup') return <Signup />
  if (route === 'login') return <Login />
  if (route === 'forgot-password') return <ForgotPassword />
  if (route === 'verify-email') return <VerifyEmail />
  return <Home />
}
