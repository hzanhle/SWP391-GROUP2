import React, { useEffect, useMemo, useRef, useState } from 'react'
import api from '../api/client'

function useAuth() {
  const token = typeof window !== 'undefined' ? localStorage.getItem('auth.token') : ''
  const raw = (typeof window !== 'undefined' && localStorage.getItem('auth.user')) || '{}'
  let userId = null
  try { const u = JSON.parse(raw); userId = u.id || u.Id || u.userId || null } catch {}
  return { token, userId, isAuthed: !!token && !!userId }
}

function formatTime(ts) {
  const d = new Date(ts)
  if (isNaN(d.getTime())) return ''
  const diff = (Date.now() - d.getTime()) / 1000
  if (diff < 60) return 'vừa xong'
  if (diff < 3600) return `${Math.floor(diff/60)} phút trước`
  if (diff < 86400) return `${Math.floor(diff/3600)} giờ trước`
  return d.toLocaleString()
}

export default function NotificationBell() {
  const { token, userId, isAuthed } = useAuth()
  const [open, setOpen] = useState(false)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [items, setItems] = useState([])
  const menuRef = useRef(null)

  useEffect(() => {
    function onDocClick(e) {
      if (!menuRef.current) return
      if (!menuRef.current.contains(e.target)) setOpen(false)
    }
    if (open) document.addEventListener('click', onDocClick)
    return () => document.removeEventListener('click', onDocClick)
  }, [open])

  useEffect(() => {
    if (!open || !isAuthed) return
    let cancelled = false
    async function load() {
      try {
        setLoading(true)
        setError('')
        const { data } = await api.getNotifications(userId, token)
        if (!cancelled) setItems(Array.isArray(data) ? data : [])
      } catch (e) {
        if (!cancelled) setError(e?.message || 'Không thể tải thông báo')
      } finally {
        if (!cancelled) setLoading(false)
      }
    }
    load()
    // mark as viewed
    try { localStorage.setItem('notif.lastViewedAt', String(Date.now())) } catch {}
    return () => { cancelled = true }
  }, [open, isAuthed, token, userId])

  const unreadCount = useMemo(() => {
    let last = 0
    try { last = Number(localStorage.getItem('notif.lastViewedAt') || 0) } catch {}
    return items.reduce((acc, it) => acc + (new Date(it.created || it.Created).getTime() > last ? 1 : 0), 0)
  }, [items, open])

  if (!isAuthed) return null

  return (
    <div className="dropdown dropdown-right" ref={menuRef}>
      <button className="btn btn-ghost" aria-label="Notifications" onClick={() => setOpen(v=>!v)}>
        <span className="sr-only">Notifications</span>
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M15 17h5l-1.4-1.4A2 2 0 0 1 18 14.2V11a6 6 0 1 0-12 0v3.2c0 .5-.2 1-.6 1.4L4 17h5m6 0v1a3 3 0 1 1-6 0v-1m6 0H9" stroke="#000000" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/></svg>
        {unreadCount > 0 && <span className="notif-dot" aria-hidden="true"></span>}
      </button>

      {open && (
        <div className="dropdown-menu card" role="menu">
          <div className="card-body">
            <div className="row-between">
              <h4 className="card-title">Thông báo</h4>
              <div className="row">
                {items.length > 0 && (<small className="card-subtext">{unreadCount} chưa đọc</small>)}
                {items.length > 0 && (
                  <button className="btn-ghost" onClick={async () => {
                    try {
                      await api.clearNotifications(userId, token)
                      setItems([])
                      setError('')
                    } catch (e) {
                      setError(e?.message || 'Không thể xóa thông báo')
                    }
                  }}>Xóa tất cả</button>
                )}
              </div>
            </div>

            {loading && (
              <div className="text-center py-4"><div className="spinner"/></div>
            )}
            {error && (
              <div className="text-center py-4"><p className="text-red-600">{error}</p></div>
            )}

            {!loading && !error && items.length === 0 && (
              <div className="py-6 text-center card-subtext">Không có thông báo mới</div>
            )}

            {!loading && !error && items.length > 0 && (
              <ul className="notif-list">
                {items.slice(0, 8).map(n => (
                  <li key={n.id || n.Id} className="notif-item">
                    <div className="notif-item-title">{n.title || n.Title}</div>
                    <div className="notif-item-desc">{n.message || n.Message}</div>
                    <div className="notif-item-time">{formatTime(n.created || n.Created)}</div>
                  </li>
                ))}
              </ul>
            )}

            {!loading && !error && items.length > 0 && (
              <div className="mt-3">
                <a href="#profile" className="btn-gradient">Xem tất cả</a>
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  )
}
