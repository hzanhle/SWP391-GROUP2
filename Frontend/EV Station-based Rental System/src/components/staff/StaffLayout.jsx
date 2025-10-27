import { useEffect, useMemo, useState } from 'react';

function Icon({ name }) {
  const common = { width: 20, height: 20, viewBox: '0 0 24 24', fill: 'none', 'aria-hidden': true };
  switch (name) {
    case 'shifts': return <svg {...common}><circle cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="2"/><path d="M12 6v6l4 2" stroke="currentColor" strokeWidth="2" strokeLinecap="round"/></svg>;
    case 'vehicles': return <svg {...common}><path d="M3 9h18M5 9v8a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V9M9 5h6a2 2 0 0 1 2 2v2H7V7a2 2 0 0 1 2-2z" stroke="currentColor" strokeWidth="2" strokeLinecap="round"/></svg>;
    case 'verify': return <svg {...common}><path d="M9 16.17L4.83 12m0 0L3 13.83m1.83-1.83l5.34-5.34a2 2 0 0 1 2.83 0l5.34 5.34M3 21h18" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/></svg>;
    case 'logout': return <svg {...common}><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4M16 17l5-5-5-5M21 12H9" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/></svg>;
    case 'moon': return <svg {...common}><path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/></svg>;
    case 'sun': return <svg {...common}><circle cx="12" cy="12" r="5" stroke="currentColor" strokeWidth="2"/><path d="M12 1v6m0 6v6M23 12h-6m-6 0H1" stroke="currentColor" strokeWidth="2" strokeLinecap="round"/></svg>;
    default: return null;
  }
}

export default function StaffLayout({ children, active = 'shifts' }) {
  const [collapsed, setCollapsed] = useState(false);
  const [isDarkMode, setIsDarkMode] = useState(() => {
    try {
      const saved = localStorage.getItem('staff-theme');
      return saved ? JSON.parse(saved) : false;
    } catch {
      return false;
    }
  });

  useEffect(() => {
    const el = document.documentElement;
    el.dataset.theme = isDarkMode ? 'dark' : 'light';
    try {
      localStorage.setItem('staff-theme', JSON.stringify(isDarkMode));
    } catch {}
    return () => {};
  }, [isDarkMode]);

  const links = useMemo(() => ([
    { key: 'shifts', label: 'My Shifts', href: '#staff-shifts', icon: 'shifts' },
    { key: 'vehicles', label: 'Vehicle Management', href: '#staff-vehicles', icon: 'vehicles' },
    { key: 'verify', label: 'Staff Verification', href: '#staff-verify', icon: 'verify' },
  ]), []);

  function logout(e) {
    e?.preventDefault?.();
    try {
      localStorage.removeItem('auth.token');
      localStorage.removeItem('auth.user');
      window.location.hash = '';
    } catch {}
  }

  let displayName = 'Staff';
  try {
    const u = JSON.parse(localStorage.getItem('auth.user') || '{}');
    displayName = u.fullName || u.userName || u.FullName || u.UserName || 'Staff';
  } catch {}

  return (
    <div className="staff-shell">
      <aside className={`staff-sidebar ${collapsed ? 'collapsed' : ''}`} aria-label="Staff sidebar">
        <div className="staff-brand">
          <div className="staff-brand-icon">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
              <path d="M13 2L3 14h9l-1 8 10-12h-9l1-8z" fill="#0ea5e9"/>
            </svg>
          </div>
          {!collapsed && <span className="staff-brand-text">EV Staff</span>}
        </div>
        
        <nav className="staff-nav" role="menu">
          {links.map(l => (
            <a 
              key={l.key} 
              role="menuitem" 
              href={l.href} 
              className={`staff-nav-link ${active === l.key ? 'active' : ''}`}
              title={l.label}
            >
              <span className="staff-icon">
                <Icon name={l.icon} />
              </span>
              {!collapsed && <span className="staff-nav-text">{l.label}</span>}
            </a>
          ))}
        </nav>

        <div className="staff-sidebar-bottom">
          {!collapsed && (
            <div className="staff-user">
              <div className="staff-user-avatar">
                {displayName.charAt(0).toUpperCase()}
              </div>
              <div className="staff-user-info">
                <div className="staff-user-name">{displayName}</div>
                <div className="staff-user-role">Staff Member</div>
              </div>
            </div>
          )}
          <div className="staff-sidebar-actions">
            <button className="staff-theme-toggle" onClick={() => setIsDarkMode(!isDarkMode)} title={isDarkMode ? 'Light mode' : 'Dark mode'}>
              <Icon name={isDarkMode ? 'sun' : 'moon'} />
              {!collapsed && <span>{isDarkMode ? 'Light' : 'Dark'}</span>}
            </button>
            <button className="staff-logout" onClick={logout} title="Logout">
              <Icon name="logout" />
              {!collapsed && <span>Logout</span>}
            </button>
          </div>
        </div>
      </aside>
      
      <div className="staff-main">
        {children}
      </div>
    </div>
  );
}
