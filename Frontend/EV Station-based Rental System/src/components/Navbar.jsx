import React, { useState } from 'react'
import CTA from './CTA'
import NotificationBell from './NotificationBell'

export default function Navbar() {
  const [open, setOpen] = useState(false)
  const [ver, setVer] = useState(0)
  const isAuthed = typeof window !== 'undefined' && !!localStorage.getItem('auth.token')
  const rawUser = (typeof window !== 'undefined' && localStorage.getItem('auth.user')) || '{}'
  let displayName = 'Bạn'
  let roleId = 0
  let roleName = ''
  try {
    const u = JSON.parse(rawUser)
    displayName = (u.fullName || u.userName || u.username || 'Bạn')
    roleId = Number(u.roleId ?? u.RoleId ?? 0)
    roleName = String(u.roleName ?? u.RoleName ?? '')
  } catch {}
  const isAdmin = roleId === 3 || roleName.toLowerCase() === 'admin'
  const isStaff = roleId === 2 || roleName.toLowerCase() === 'staff'

  function handleLogout(e) {
    e.preventDefault()
    if (typeof window !== 'undefined') {
      localStorage.removeItem('auth.token')
      localStorage.removeItem('auth.user')
      setOpen(false)
      setVer(v => v + 1)
      window.location.hash = ''
    }
  }
  return (
    <header className="navbar" data-figma-layer="Navbar" data-tailwind='class: "sticky top-0 z-50 bg-white border-b border-slate-200"'>
      <div className="container navbar-inner" role="navigation" aria-label="Primary" data-tailwind='class: "flex items-center justify-between px-6 py-4"'>
        <a href="#" className="brand" aria-label="EVStation Home" data-figma-layer="Logo" data-tailwind='class: "flex items-center gap-3 font-bold text-xl text-slate-900"'>
          <svg width="28" height="28" viewBox="0 0 24 24" fill="none" aria-hidden="true" data-export="svg"><path d="M6 3h7a3 3 0 0 1 3 3v5h1a2 2 0 0 1 2 2v5h-2v-5h-1v5H4v-8a7 7 0 0 1 2-5V3z" stroke="#0ea5e9" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/></svg>
          <span>EVStation</span>
        </a>

        <nav className="nav-links" aria-label="Primary navigation" data-figma-layer="NavLinks" data-tailwind='class: "hidden md:flex gap-6"'>
          <div className="dropdown">
            <a className="nav-link dropdown-toggle" href="#stations" data-tailwind='class: "text-slate-500 hover:text-slate-900 px-3 py-2 rounded-md"'>Stations</a>
            <div className="dropdown-menu card card-body" role="menu">
              <a className="nav-link" role="menuitem" href="#stations">Nearby stations</a>
              <a className="nav-link" role="menuitem" href="#vehicles">Vehicles</a>
              <a className="nav-link" role="menuitem" href="#pricing">Pricing</a>
            </div>
          </div>
          <a className="nav-link" href="#how" data-tailwind='class: "text-slate-500 hover:text-slate-900 px-3 py-2 rounded-md"'>How it works</a>
          <a className="nav-link" href="#pricing" data-tailwind='class: "text-slate-500 hover:text-slate-900 px-3 py-2 rounded-md"'>Pricing</a>
          <a className="nav-link" href="#support" data-tailwind='class: "text-slate-500 hover:text-slate-900 px-3 py-2 rounded-md"'>Support</a>
          {isAdmin && (
            <>
              <a className="nav-link" href="admin" data-tailwind='class: "text-slate-500 hover:text-slate-900 px-3 py-2 rounded-md"'>Dashboard</a>
              <a className="nav-link" href="#admin-users" data-tailwind='class: "text-slate-500 hover:text-slate-900 px-3 py-2 rounded-md"'>Users</a>
              <a className="nav-link" href="#admin-models" data-tailwind='class: "text-slate-500 hover:text-slate-900 px-3 py-2 rounded-md"'>Models</a>
            </>
          )}
          {isStaff && (
            <a className="nav-link" href="#staff-verify" data-tailwind='class: "text-slate-500 hover:text-slate-900 px-3 py-2 rounded-md"'>Staff</a>
          )}
          {isAuthed ? (
            <>
              <a className="nav-link" href="#profile" data-tailwind='class: "text-slate-500 hover:text-slate-900 px-3 py-2 rounded-md"'>Profile</a>
              <span className="nav-link" aria-label="User greeting" data-tailwind='class: "text-slate-500 hover:text-slate-900 px-3 py-2 rounded-md"'>
                Hello, {displayName}
              </span>
              <a className="nav-link" href="#" onClick={handleLogout} data-tailwind='class: "text-slate-500 hover:text-slate-900 px-3 py-2 rounded-md"'>Logout</a>
            </>
          ) : (
            <a className="nav-link" href="#login" data-tailwind='class: "text-slate-500 hover:text-slate-900 px-3 py-2 rounded-md"'>Login</a>
          )}
        </nav>

        {!isAuthed && (
          <div className="nav-cta" data-figma-layer="NavCTA" data-tailwind='class: "hidden md:inline-flex"'>
            <CTA as="a" href="#signup" aria-label="Sign up or Book" data-figma-layer="CTA" data-tailwind='class: "bg-sky-500 text-white px-5 py-3 rounded-lg shadow-md"'>Sign up</CTA>
          </div>
        )}

        <div className="row" aria-label="Quick actions">
          {isAuthed && (<NotificationBell />)}

          <button
            aria-label="Toggle theme"
            className="btn btn-ghost"
            onClick={() => {
              try {
                const next = document.documentElement.dataset.theme === 'dark' ? 'light' : 'dark'
                document.documentElement.dataset.theme = next
                localStorage.setItem('theme', next)
                setVer(v => v + 1)
              } catch {}
            }}
            title="Toggle dark mode"
          >
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z" stroke="#0f172a" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/></svg>
            <span className="sr-only">Toggle dark mode</span>
          </button>

          <button aria-label="Toggle menu" className="menu-toggle btn btn-ghost" onClick={() => setOpen(v=>!v)} data-tailwind='class: "inline-flex md:hidden items-center gap-2 border border-slate-200 px-3 py-2 rounded-md"'>
            <span>Menu</span>
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" aria-hidden="true" data-export="svg"><path d="M4 6h16M4 12h16M4 18h16" stroke="#0f172a" strokeWidth="2" strokeLinecap="round"/></svg>
          </button>
        </div>
      </div>

      {open && (
        <>
          <div className="drawer-backdrop" onClick={() => setOpen(false)} />
          <aside className="mobile-drawer" role="menu">
            <div className="card card-body">
              <a className="nav-link" role="menuitem" href="#stations" onClick={() => setOpen(false)}>Stations</a>
              <a className="nav-link" role="menuitem" href="#how" onClick={() => setOpen(false)}>How it works</a>
              <a className="nav-link" role="menuitem" href="#pricing" onClick={() => setOpen(false)}>Pricing</a>
              <a className="nav-link" role="menuitem" href="#support" onClick={() => setOpen(false)}>Support</a>
              {isAdmin && (
                <>
                  <a className="nav-link" role="menuitem" href="admin" onClick={() => setOpen(false)}>Dashboard</a>
                  <a className="nav-link" role="menuitem" href="#admin-users" onClick={() => setOpen(false)}>Users</a>
                  <a className="nav-link" role="menuitem" href="#admin-models" onClick={() => setOpen(false)}>Models</a>
                </>
              )}
              {isStaff && (
                <a className="nav-link" role="menuitem" href="#staff-verify" onClick={() => setOpen(false)}>Staff</a>
              )}
              {isAuthed ? (
                <>
                  <a className="nav-link" role="menuitem" href="#profile" onClick={() => setOpen(false)}>Profile</a>
                  <span className="nav-link" role="menuitem">Xin chào, {displayName}</span>
                  <a className="nav-link" role="menuitem" href="#" onClick={handleLogout}>Đăng xuất</a>
                </>
              ) : (
                <a className="nav-link" role="menuitem" href="#login" onClick={() => setOpen(false)}>Login</a>
              )}
              {!isAuthed && (<CTA as="a" href="#signup" className="mt-2" data-tailwind='class: "mt-2"'>Sign up / Book</CTA>)}
            </div>
          </aside>
        </>
      )}
    </header>
  )
}
