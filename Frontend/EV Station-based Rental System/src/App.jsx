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
import Payment from './pages/Payment'
import CheckIn from './pages/CheckIn'
import Return from './pages/Return'
import History from './pages/History'
import AdminUsers from './pages/AdminUsers'
import StaffVerification from './pages/StaffVerification'
import AdminModels from './pages/AdminModels'
import AdminStations from './pages/AdminStations'
import AdminVehicles from './pages/AdminVehicles'
import AdminDashboard from './pages/AdminDashboard'
import AdminStaffShift from './pages/AdminStaffShift'
import StaffShift from './pages/StaffShift'
import StaffVehicle from './pages/StaffVehicle'
import Feedback from './pages/Feedback'

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
  const roleId = getRoleId()
  const token = typeof window !== 'undefined' ? localStorage.getItem('auth.token') : ''

  
  // Extract base route (without query parameters)
  const baseHash = hash.split('?')[0]

  // Auto-redirect admin to dashboard
  if (token && roleId === 3) {
    if (!hash && path.toLowerCase() === 'admin') return 'admin'
      if (baseHash.startsWith('admin')) return baseHash
    // If admin tries to access member pages, redirect to admin
    if (['home', 'stations', 'vehicles', 'booking', 'booking-new', 'payment', 'check-in', 'return', 'history', 'profile', 'profile-docs', 'staff-shifts', 'staff-vehicles'].includes(baseHash)) {
      return 'admin'
    }
    // Default for admin: show admin dashboard
    return 'admin'
  }

  // Auto-redirect staff to shifts
  if (token && roleId === 2) {
    if (hash.startsWith('staff')) return hash.replace('#', '')
    // If staff tries to access member pages, redirect to staff shifts
    if (['home', 'stations', 'vehicles', 'booking', 'booking-new', 'payment', 'check-in', 'return', 'history', 'profile-docs'].includes(hash)) {
      return 'staff-shifts'
    }
    // Allow staff to access profile and feedback
    if (baseHash === 'profile') return hash
    // Default for staff: show shifts
    return 'staff-shifts'
  }

  if (!hash && path.toLowerCase() === 'admin') return 'admin'
  switch (baseHash) {
    case 'signup': return 'signup'
    case 'login': return 'login'
    case 'forgot-password': return 'forgot-password'
    case 'verify-email': return 'verify-email'
    case 'profile': return 'profile'
    case 'profile-docs': return 'profile-docs'
    case 'stations': return 'stations'
    case 'vehicles': return 'vehicles'
    case 'booking-new': return 'booking-new'
    case 'payment': return 'payment'
    case 'booking': return 'booking'
    case 'check-in': return 'check-in'
    case 'return': return 'return'
    case 'history': return 'history'
    case 'feedback': return 'feedback'
    case 'admin-users': return 'admin-users'
    case 'staff-verify': return 'staff-verify'
    case 'admin-models': return 'admin-models'
    case 'admin-stations': return 'admin-stations'
    case 'admin-vehicles': return 'admin-vehicles'
    case 'admin-staffshift': return 'admin-staffshift'
    case 'staff-shifts': return 'staff-shifts'
    case 'staff-vehicles': return 'staff-vehicles'
    case 'admin': return 'admin'
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

  // Check if current route is admin route
  const isAdminRoute = ['admin', 'admin-users', 'admin-models', 'admin-stations', 'admin-vehicles', 'admin-staffshift', 'staff-verify'].includes(routeData)
  // Check if current route is staff route
  const isStaffRoute = ['staff-shifts', 'staff-vehicles'].includes(routeData)

  return (
    <>
      {!isAdminRoute && !isStaffRoute && <Navbar />}
      {routeData === 'signup' && <Signup />}
      {routeData === 'login' && <Login />}
      {routeData === 'forgot-password' && <ForgotPassword />}
      {routeData === 'verify-email' && <VerifyEmail />}
      {routeData === 'profile' && <Profile />}
      {routeData === 'profile-docs' && <ProfileDocs />}
      {routeData === 'stations' && <Stations />}
      {routeData === 'vehicles' && <Vehicles />}
      {routeData === 'booking-new' && <BookingNew />}
      {routeData === 'payment' && <Payment />}
      {routeData === 'booking' && <BookingDetail />}
      {routeData === 'check-in' && <CheckIn />}
      {routeData === 'return' && <Return />}
      {routeData === 'history' && <History />}
      {routeData === 'feedback' && <Feedback />}
      {routeData === 'admin' && <AdminDashboard />}
      {routeData === 'admin-users' && <AdminUsers />}
      {routeData === 'admin-models' && <AdminModels />}
      {routeData === 'admin-stations' && <AdminStations />}
      {routeData === 'admin-vehicles' && <AdminVehicles />}
      {routeData === 'admin-staffshift' && <AdminStaffShift />}
      {routeData === 'staff-verify' && <StaffVerification />}
      {routeData === 'staff-shifts' && <StaffShift />}
      {routeData === 'staff-vehicles' && <StaffVehicle />}
      {routeData === 'home' && <Home />}
    </>
  )
}
