import React, { useEffect, useState } from 'react'
import Navbar from './components/Navbar'
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
import AdminUsers from './pages/AdminUsers'
import StaffVerification from './pages/StaffVerification'
import AdminModels from './pages/AdminModels'
import AdminDashboard from './pages/AdminDashboard'

function getRoleId() {
  try {
    const raw = localStorage.getItem('auth.user') || '{}'
    const u = JSON.parse(raw)
    return Number(u.roleId ?? u.RoleId ?? 0)
  } catch { return 0 }
}

function resolveRoute() {
  const hash = typeof window !== 'undefined' ? window.location.hash.replace('#', '') : ''
  const path = typeof window !== 'undefined' ? window.location.pathname.replace(/^\//, '') : ''
  if (!hash && path.toLowerCase() === 'admin') return 'admin'
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
    case 'admin-users': return 'admin-users'
    case 'staff-verify': return 'staff-verify'
    case 'admin-models': return 'admin-models'
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

  return (
    <>
      <Navbar />
      {routeData === 'signup' && <Signup />}
      {routeData === 'login' && <Login />}
      {routeData === 'forgot-password' && <ForgotPassword />}
      {routeData === 'verify-email' && <VerifyEmail />}
      {routeData === 'profile' && <Profile />}
      {routeData === 'profile-docs' && <ProfileDocs />}
      {routeData === 'stations' && <Stations />}
      {routeData === 'vehicles' && <Vehicles />}
      {routeData === 'booking-new' && <BookingNew />}
      {routeData === 'booking' && <BookingDetail />}
      {routeData === 'check-in' && <CheckIn />}
      {routeData === 'return' && <Return />}
      {routeData === 'history' && <History />}
      {routeData === 'admin' && <AdminDashboard />}
      {routeData === 'admin-users' && <AdminUsers />}
      {routeData === 'admin-models' && <AdminModels />}
      {routeData === 'staff-verify' && <StaffVerification />}
      {routeData === 'home' && <Home />}
    </>
  )
}
