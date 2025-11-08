import React, { useEffect, useState } from 'react'
import {
  Box, Container, Card, CardContent, Typography, TextField, Button, Alert, Stack
} from '@mui/material'
import AdminLayout from '../components/admin/AdminLayout'
import { registerUser } from '../api/client'

export default function AdminCreateStaff() {
  const [form, setForm] = useState({ userName: '', email: '', phoneNumber: '', password: '', confirmPassword: '' })
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  const token = typeof window !== 'undefined' ? localStorage.getItem('auth.token') : ''
  const rawUser = typeof window !== 'undefined' ? localStorage.getItem('auth.user') : '{}'
  let currentRoleId = 0
  try { currentRoleId = Number(JSON.parse(rawUser)?.roleId || JSON.parse(rawUser)?.RoleId || 0) } catch {}
  const forbidden = currentRoleId !== 3

  function updateField(k, v) { setForm(prev => ({ ...prev, [k]: v })) }

  function validate() {
    if (!form.userName || form.userName.trim().length < 3) return 'Username must be at least 3 characters'
    if (!form.email || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) return 'Invalid email'
    if (!form.phoneNumber || form.phoneNumber.trim().length < 8) return 'Invalid phone number'
    if (!form.password || form.password.length < 6) return 'Password must be at least 6 characters'
    if (form.password !== form.confirmPassword) return 'Passwords do not match'
    return ''
  }

  async function handleSubmit(e) {
    e.preventDefault()
    setError(''); setSuccess('')
    const v = validate()
    if (v) { setError(v); return }
    try {
      setLoading(true)
      await registerUser({
        userName: form.userName,
        email: form.email,
        phoneNumber: form.phoneNumber,
        password: form.password,
        roleId: 2, // Staff
        isActive: true,
        trustScore: 0,
      })
      setSuccess('Staff account created successfully')
      setForm({ userName: '', email: '', phoneNumber: '', password: '', confirmPassword: '' })
      setTimeout(() => { window.location.hash = 'admin-users' }, 800)
    } catch (e) {
      setError(e?.message || 'Failed to create staff account')
    } finally {
      setLoading(false)
    }
  }

  if (forbidden) {
    return (
      <AdminLayout active="users">
        <Container maxWidth="lg" sx={{ py: 4 }}>
          <Alert severity="error">Unauthorized</Alert>
        </Container>
      </AdminLayout>
    )
  }

  return (
    <AdminLayout active="users">
      <Box className="admin-page">
        <Container maxWidth="sm">
          <form onSubmit={handleSubmit}>
            <Stack spacing={3}>
              <Box className="admin-header">
                <Box>
                  <Typography variant="h4" component="h1" gutterBottom className="font-600">Create Staff Account</Typography>
                  <Typography variant="body1" color="text.secondary">Enter details to create a staff account.</Typography>
                </Box>
              </Box>

              {error && <Alert severity="error" onClose={() => setError('')}>{error}</Alert>}
              {success && <Alert severity="success" onClose={() => setSuccess('')}>{success}</Alert>}

              <Card>
                <CardContent>
                  <Stack spacing={2}>
                    <TextField label="Username" value={form.userName} onChange={(e)=>updateField('userName', e.target.value)} fullWidth required />
                    <TextField label="Email" type="email" value={form.email} onChange={(e)=>updateField('email', e.target.value)} fullWidth required />
                    <TextField label="Phone" value={form.phoneNumber} onChange={(e)=>updateField('phoneNumber', e.target.value)} fullWidth required />
                    <TextField label="Password" type="password" value={form.password} onChange={(e)=>updateField('password', e.target.value)} fullWidth required />
                    <TextField label="Confirm Password" type="password" value={form.confirmPassword} onChange={(e)=>updateField('confirmPassword', e.target.value)} fullWidth required />
                  </Stack>
                </CardContent>
              </Card>

              <Box sx={{ display: 'flex', gap: 1, justifyContent: 'flex-end' }}>
                <Button variant="outlined" onClick={() => window.history.back()} disabled={loading}>Cancel</Button>
                <Button variant="contained" type="submit" disabled={loading}>{loading ? 'Creating…' : 'Create Staff'}</Button>
              </Box>
            </Stack>
          </form>
        </Container>
      </Box>
    </AdminLayout>
  )
}
