import React, { useEffect, useState } from 'react'
import Home from './pages/Home'
import Signup from './pages/Signup'
import Login from './pages/Login'
import ForgotPassword from './pages/ForgotPassword'
import VerifyEmail from './pages/VerifyEmail'
import Profile from './pages/Profile'
import ProfileDocs from './pages/ProfileDocs'
import Stations from './pages/Stations'
import Vehicles from './pages/Vehicles'
import BookingNew from './pages/BookingNew'
import BookingDetail from './pages/BookingDetail'
import CheckIn from './pages/CheckIn'
import Return from './pages/Return'
import History from './pages/History'
import VehicleDetail from './pages/ModelDetail'
// ✨ THÊM MỚI ✨
import ModelDetail from './pages/ModelDetail'

function resolveRoute() {
  const hash = typeof window !== 'undefined' ? window.location.hash.replace('#', '') : ''
  
  // ✨ THÊM MỚI - Handle dynamic route với ID ✨
  if (hash.startsWith('models/')) {
    const id = hash.split('/')[1]
    return { route: 'model-detail', id }
  }
  
  switch (hash) {
    case 'signup': return 'signup'
    case 'login': return 'login'
    case 'forgot-password': return 'forgot-password'
    case 'verify-email': return 'verify-email'
    case 'profile': return 'profile'
    case 'profile-docs': return 'profile-docs'
    case 'stations': return 'stations'
    case 'vehicles': return 'vehicles'
    case 'booking-new': return 'booking-new'
    case 'booking': return 'booking'
    case 'check-in': return 'check-in'
    case 'return': return 'return'
    case 'history': return 'history'
    // ✨ THÊM MỚI ✨
    case 'models': return 'models'
    default: return 'home'
  }
}

export default function App() {
  const [routeData, setRouteData] = useState(resolveRoute())

  useEffect(() => {
    const onHashChange = () => setRouteData(resolveRoute())
    window.addEventListener('hashchange', onHashChange)
    return () => window.removeEventListener('hashchange', onHashChange)
  }, [])

  // ✨ THÊM MỚI - Handle object route data ✨
  const route = typeof routeData === 'string' ? routeData : routeData.route

  if (route === 'signup') return <Signup />
  if (route === 'login') return <Login />
  if (route === 'forgot-password') return <ForgotPassword />
  if (route === 'verify-email') return <VerifyEmail />
  if (route === 'profile') return <Profile />
  if (route === 'profile-docs') return <ProfileDocs />
  if (route === 'stations') return <Stations />
  if (route === 'vehicles') return <Vehicles />
  if (route === 'booking-new') return <BookingNew />
  if (route === 'booking') return <BookingDetail />
  if (route === 'check-in') return <CheckIn />
  if (route === 'return') return <Return />
  if (route === 'history') return <History />
  // ✨ THÊM MỚI ✨
  if (route === 'model-detail') return <ModelDetail id={routeData.id} />
  return <Home />
}