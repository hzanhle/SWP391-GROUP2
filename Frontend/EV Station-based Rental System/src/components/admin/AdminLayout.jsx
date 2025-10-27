import { useEffect, useMemo } from 'react'

function Icon({ name }) {
  const common = { width: 18, height: 18, viewBox: '0 0 24 24', fill: 'none', 'aria-hidden': true }
  switch (name) {
    case 'home': return <svg {...common}><path d="M3 10.5 12 3l9 7.5V21a1 1 0 0 1-1 1h-5v-6H9v6H4a1 1 0 0 1-1-1v-10.5z" stroke="#010103" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/></svg>
    case 'users': return <svg {...common}><path d="M17 21v-2a4 4 0 0 0-4-4H7a4 4 0 0 0-4 4v2" stroke="#010103" strokeWidth="2"/><circle cx="9" cy="7" r="4" stroke="#010103" strokeWidth="2"/></svg>
    case 'models': return <svg {...common}><path d="M3 7h18M3 12h18M3 17h18" stroke="#010103" strokeWidth="2" strokeLinecap="round"/></svg>
    case 'settings': return <svg {...common}><path d="M12 15.5a3.5 3.5 0 1 0 0-7 3.5 3.5 0 0 0 0 7z" stroke="#010103" strokeWidth="2"/><path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 1 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 1 1-4 0v-.09a1.65 1.65 0 0 0-1-1.51 1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 1 1-2.83-2.83l.06-.06A1.65 1.65 0 0 0 5 15.4a1.65 1.65 0 0 0-1.51-1H3a2 2 0 1 1 0-4h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 1 1 2.83-2.83l.06.06A1.65 1.65 0 0 0 8 3.6a1.65 1.65 0 0 0 1-1.51V2a2 2 0 1 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 1 1 2.83 2.83l-.06.06A1.65 1.65 0 0 0 20.4 9c.35.52.55 1.14.55 1.77 0 .63-.2 1.25-.55 1.77z" stroke="#010103" strokeWidth="2"/></svg>
    case 'about': return <svg {...common}><path d="M12 20a8 8 0 1 0 0-16 8 8 0 0 0 0 16z" stroke="#010103" strokeWidth="2"/><path d="M12 8h.01M11 12h1v4h1" stroke="#010103" strokeWidth="2" strokeLinecap="round"/></svg>
    case 'help': return <svg {...common}><path d="M12 2a10 10 0 1 0 10 10A10 10 0 0 0 12 2zm0 14h.01M12 12a3 3 0 1 0-3-3" stroke="#010103" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/></svg>
    default: return null
  }
}

export default function AdminLayout({ children, active = 'overview' }) {
  useEffect(() => {
    const el = document.documentElement
    const prev = el.dataset.theme
    el.dataset.theme = 'light'
    return () => { if (prev) el.dataset.theme = prev; else delete el.dataset.theme }
  }, [])

  const links = useMemo(() => ([
    { key: 'overview', label: 'Home', href: '#admin', icon: 'home' },
    { key: 'analytics', label: 'Analytics', href: '#admin', icon: 'models' },
    { key: 'users', label: 'Clients', href: '#admin-users', icon: 'users' },
    { key: 'models', label: 'Tasks', href: '#admin-models', icon: 'models' },
    { key: 'staffshift', label: 'Staff Shifts', href: '#admin-staffshift', icon: 'users' },
  ]), [])

  const secondary = useMemo(() => ([
    { key: 'settings', label: 'Settings', href: '#admin', icon: 'settings' },
    { key: 'about', label: 'About', href: '#admin', icon: 'about' },
    { key: 'feedback', label: 'Feedback', href: '#admin', icon: 'help' },
  ]), [])

  function logout(e) {
    e?.preventDefault?.()
    try {
      localStorage.removeItem('auth.token')
      localStorage.removeItem('auth.user')
      window.location.hash = ''
    } catch {}
  }

  let displayName = 'Admin'
  try { const u = JSON.parse(localStorage.getItem('auth.user') || '{}'); displayName = u.fullName || u.userName || 'Admin' } catch {}

  return (
    <div className="admin-shell">
      <aside className="admin-sidebar" aria-label="Admin sidebar">
        <div className="admin-brand">
          <svg width="22" height="22" viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M6 3h7a3 3 0 0 1 3 3v5h1a2 2 0 0 1 2 2v5h-2v-5h-1v5H4v-8a7 7 0 0 1 2-5V3z" stroke="#0ea5e9" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/></svg>
          <span>EV Admin</span>
        </div>
        <nav className="admin-nav" role="menu">
          {links.map(l => (
            <a key={l.key} role="menuitem" href={l.href} className={`admin-nav-link ${active===l.key ? 'active' : ''}`}>
              <span className="admin-icon"><Icon name={l.icon} /></span>
              <span>{l.label}</span>
            </a>
          ))}
        </nav>
        <div className="admin-divider" />
        <nav className="admin-nav" role="menu" aria-label="Secondary">
          {secondary.map(l => (
            <a key={l.key} role="menuitem" href={l.href} className="admin-nav-link">
              <span className="admin-icon"><Icon name={l.icon} /></span>
              <span>{l.label}</span>
            </a>
          ))}
        </nav>
        <div className="admin-sidebar-bottom">
          <div className="admin-user">{displayName}</div>
          <a href="#" className="admin-logout" onClick={logout}>Logout</a>
        </div>
      </aside>
      <div className="admin-main">
        {children}
      </div>
    </div>
  )
}
