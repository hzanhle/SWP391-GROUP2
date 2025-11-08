import { useEffect, useMemo, useState } from 'react';

import { useThemeMode } from '../../theme/ThemeContext'
function Icon({ name }) {
  const common = { width: 20, height: 20, viewBox: '0 0 24 24', fill: 'none', 'aria-hidden': true };
  switch (name) {
    case 'home': return <svg {...common}><path d="M3 10.5 12 3l9 7.5V21a1 1 0 0 1-1 1h-5v-6H9v6H4a1 1 0 0 1-1-1v-10.5z" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/></svg>;
    case 'users': return <svg {...common}><path d="M17 21v-2a4 4 0 0 0-4-4H7a4 4 0 0 0-4 4v2" stroke="currentColor" strokeWidth="2"/><circle cx="9" cy="7" r="4" stroke="currentColor" strokeWidth="2"/></svg>;
    case 'models': return <svg {...common}><path d="M3 7h18M3 12h18M3 17h18" stroke="currentColor" strokeWidth="2" strokeLinecap="round"/></svg>;
    case 'schedule': return <svg {...common}><circle cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="2"/><path d="M12 6v6l4 2" stroke="currentColor" strokeWidth="2" strokeLinecap="round"/></svg>;
    case 'settings': return <svg {...common}><circle cx="12" cy="12" r="3" stroke="currentColor" strokeWidth="2"/><path d="M12 1v6m0 6v6M23 12h-6m-6 0H5" stroke="currentColor" strokeWidth="2"/></svg>;
    case 'logout': return <svg {...common}><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4M16 17l5-5-5-5M21 12H9" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/></svg>;
    default: return null;
  }
}

export default function AdminLayout({ children, active = 'overview' }) {
  const [collapsed, setCollapsed] = useState(false);


  const links = useMemo(() => ([
    { key: 'overview', label: 'Overview', href: '#admin', icon: 'home' },
    { key: 'users', label: 'Users', href: '#admin-users', icon: 'users' },
    { key: 'models', label: 'Vehicle Models', href: '#admin-models', icon: 'models' },
    { key: 'vehicles', label: 'Vehicles', href: '#admin-vehicles', icon: 'models' },
    { key: 'stations', label: 'Charging Stations', href: '#admin-stations', icon: 'home' },
    { key: 'staffshift', label: 'Staff Shifts', href: '#admin-staffshift', icon: 'schedule' },
  ]), []);

  function logout(e) {
    e?.preventDefault?.();
    try {
      localStorage.removeItem('auth.token');
      localStorage.removeItem('auth.user');
      window.location.hash = '';
    } catch {}
  }

  let displayName = 'Admin';
  try {
    const u = JSON.parse(localStorage.getItem('auth.user') || '{}');
    displayName = u.fullName || u.userName || u.FullName || u.UserName || 'Admin';
  } catch {}

  const { mode, toggleMode } = useThemeMode();
  const isDark = mode === 'dark';
  return (
    <div className="admin-shell">
      <aside className={`admin-sidebar ${collapsed ? 'collapsed' : ''}`} aria-label="Admin sidebar">
        <div className="admin-brand">
          <div className="admin-brand-icon">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
              <path d="M13 2L3 14h9l-1 8 10-12h-9l1-8z" fill="#0ea5e9"/>
            </svg>
          </div>
          {!collapsed && <span className="admin-brand-text">EV Admin</span>}
        </div>
        
        <nav className="admin-nav" role="menu">
          {links.map(l => (
            <a 
              key={l.key} 
              role="menuitem" 
              href={l.href} 
              className={`admin-nav-link ${active === l.key ? 'active' : ''}`}
              title={l.label}
            >
              <span className="admin-icon">
                <Icon name={l.icon} />
              </span>
              {!collapsed && <span className="admin-nav-text">{l.label}</span>}
            </a>
          ))}
        </nav>

        <div className="admin-sidebar-bottom">
          {!collapsed && (
            <div className="admin-user">
              <div className="admin-user-avatar">
                {displayName.charAt(0).toUpperCase()}
              </div>
              <div className="admin-user-info">
                <div className="admin-user-name">{displayName}</div>
                <div className="admin-user-role">Administrator</div>
              </div>
            </div>
          )}
          <div style={{ display: 'grid', gap: '0.5rem' }}>
            <button className="admin-theme-toggle" onClick={toggleMode} title={isDark ? 'Switch to light mode' : 'Switch to dark mode'}>
              <span className="admin-icon" aria-hidden="true">{isDark ? '☀️' : '🌙'}</span>
              {!collapsed && <span>{isDark ? 'Light' : 'Dark'}</span>}
            </button>
            <button className="admin-logout" onClick={logout} title="Logout">
            <Icon name="logout" />
            {!collapsed && <span>Logout</span>}
          </button>
          </div>
        </div>
      </aside>
      
      <div className="admin-main">
        {children}
      </div>
    </div>
  );
}
