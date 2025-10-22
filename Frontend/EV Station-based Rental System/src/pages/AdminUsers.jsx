import { useEffect, useState } from 'react'
import CTA from '../components/CTA'
import api, { getAllUsers, deleteUser, toggleUserActive, toggleStaffAdmin } from '../api/client'
import { isWarned, setWarned } from '../../utils/warnings'
import AdminLayout from '../components/admin/AdminLayout'
import '../styles/admin.css'

function roleLabel(roleId) {
  switch (Number(roleId)) {
    case 1: return 'Member'
    case 2: return 'Staff'
    case 3: return 'Admin'
    default: return String(roleId)
  }
}

export default function AdminUsers() {
  const [users, setUsers] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [busyId, setBusyId] = useState(null)

  const token = (typeof window !== 'undefined' && localStorage.getItem('auth.token')) || ''
  const rawUser = (typeof window !== 'undefined' && localStorage.getItem('auth.user')) || '{}'
  let currentRoleId = 0
  try { currentRoleId = Number(JSON.parse(rawUser)?.roleId || JSON.parse(rawUser)?.RoleId || 0) } catch {}

  useEffect(() => {
    let mounted = true
    ;(async () => {
      try {
        setLoading(true)
        const { data } = await getAllUsers(token)
        if (!mounted) return
        setUsers(Array.isArray(data) ? data : [])
        setError('')
      } catch (e) {
        setError(e.message || 'Failed to load users')
      } finally {
        setLoading(false)
      }
    })()
    return () => { mounted = false }
  }, [token])

  async function onDelete(id) {
    if (!confirm('Delete this user?')) return
    try {
      setBusyId(id)
      await deleteUser(id, token)
      setUsers(prev => prev.filter(u => (u.id ?? u.Id) !== id))
    } catch (e) {
      alert(e.message)
    } finally {
      setBusyId(null)
    }
  }

  async function onToggleActive(id) {
    try {
      setBusyId(id)
      await toggleUserActive(id, token)
      setUsers(prev => prev.map(u => {
        const uid = u.id ?? u.Id
        if (uid !== id) return u
        const isActive = (u.isActive ?? u.IsActive)
        return { ...u, isActive: !isActive, IsActive: !isActive }
      }))
    } catch (e) {
      alert(e.message)
    } finally { setBusyId(null) }
  }

  async function onToggleRole(id, roleId) {
    try {
      setBusyId(id)
      if (roleId === 2 || roleId === 3) {
        await toggleStaffAdmin(id, token)
        setUsers(prev => prev.map(u => {
          const uid = u.id ?? u.Id
          if (uid !== id) return u
          const r = Number(u.roleId ?? u.RoleId)
          const next = r === 2 ? 3 : 2
          return { ...u, roleId: next, RoleId: next }
        }))
      }
    } catch (e) {
      alert(e.message)
    } finally { setBusyId(null) }
  }

  function onWarnChange(id, checked) {
    setWarned(id, checked)
  }

  const forbidden = currentRoleId !== 3

  if (forbidden) {
    return (
      <AdminLayout active="users">
        <section className="section" aria-labelledby="admin-title">
          <div className="container">
            <div className="card card-body">
              <h1 id="admin-title" className="section-title">Unauthorized</h1>
              <p className="section-subtitle">Bạn không có quyền truy cập trang Admin.</p>
              <CTA as="a" href="#" variant="secondary">Về trang chủ</CTA>
            </div>
          </div>
        </section>
      </AdminLayout>
    )
  }

  return (
    <AdminLayout active="users">
      <section className="section" aria-labelledby="users-title">
        <div className="container">
            <div className="section-header">
              <h1 id="users-title" className="section-title">Quản lý người dùng</h1>
              <p className="section-subtitle">Xóa, chuyển role Staff ⇄ Admin, đánh dấu cảnh báo.</p>
            </div>

            {loading ? (
              <div className="card card-body">Đang tải...</div>
            ) : error ? (
              <div role="alert" className="card card-body">{error}</div>
            ) : (
              <div className="vehicle-grid">
                {users.map(u => {
                  const id = u.id ?? u.Id
                  const name = u.userName ?? u.UserName
                  const email = u.email ?? u.Email
                  const phone = u.phoneNumber ?? u.PhoneNumber
                  const roleId = Number(u.roleId ?? u.RoleId)
                  const isActive = (u.isActive ?? u.IsActive)
                  const warned = isWarned(id)
                  return (
                    <div key={id} className="card">
                      <div className="card-body">
                        <div className="row-between">
                          <div>
                            <h3 className="card-title">{name}</h3>
                            <p className="card-subtext">{email} · {phone}</p>
                          </div>
                          <span className={`badge ${isActive ? 'gray' : 'gray'}`}>{isActive ? 'Active' : 'Inactive'}</span>
                        </div>
                        <p className="card-subtext">Role: {roleLabel(roleId)}</p>
                        <div className="row">
                          <CTA as="button" onClick={() => onToggleActive(id)} disabled={busyId===id}>
                            {isActive ? 'Deactivate' : 'Activate'}
                          </CTA>
                          {(roleId === 2 || roleId === 3) && (
                            <CTA as="button" variant="secondary" onClick={() => onToggleRole(id, roleId)} disabled={busyId===id}>
                              {roleId === 2 ? 'Make Admin' : 'Make Staff'}
                            </CTA>
                          )}
                          <CTA as="button" variant="ghost" onClick={() => onDelete(id)} disabled={busyId===id}>Delete</CTA>
                        </div>
                        <div className="field mt-2">
                          <label className="label">
                            <input type="checkbox" className="checkbox" checked={warned} onChange={(e)=>onWarnChange(id, e.target.checked)} />
                            <span className="card-subtext"> Warning account</span>
                          </label>
                        </div>
                      </div>
                    </div>
                  )
                })}
              </div>
            )}
          </div>
        </section>
      </AdminLayout>
  )
}
