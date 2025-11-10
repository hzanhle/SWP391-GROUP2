import { useEffect, useState } from 'react'
import { 
  Box, Container, Card, CardContent, Typography, Button, Alert, CircularProgress, Stack, Grid, Paper,
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow, IconButton, Chip,
  Dialog, DialogTitle, DialogContent, DialogActions, FormControlLabel, Checkbox
} from '@mui/material'
import { Delete as DeleteIcon, Edit as EditIcon } from '@mui/icons-material'
import { getAllUsers, deleteUser, toggleUserActive, toggleStaffAdmin } from '../api/client'
import { isWarned, setWarned } from '../../utils/warnings'
import AdminLayout from '../components/admin/AdminLayout'

function roleLabel(roleId) {
  switch (Number(roleId)) {
    case 1: return 'Member'
    case 2: return 'Staff'
    case 3: return 'Admin'
    default: return String(roleId)
  }
}

function getRoleColor(roleId) {
  switch (Number(roleId)) {
    case 1: return 'default'
    case 2: return 'warning'
    case 3: return 'error'
    default: return 'default'
  }
}

export default function AdminUsers() {
  const [users, setUsers] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [busyId, setBusyId] = useState(null)
  const [selectedUser, setSelectedUser] = useState(null)
  const [openDialog, setOpenDialog] = useState(false)

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
      setOpenDialog(false)
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
    } finally { 
      setBusyId(null) 
    }
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
    } finally { 
      setBusyId(null) 
    }
  }

  function onWarnChange(id, checked) {
    setWarned(id, checked)
  }

  const forbidden = currentRoleId !== 3

  if (forbidden) {
    return (
      <AdminLayout active="users">
        <Container maxWidth="lg" sx={{ py: 4 }}>
          <Alert severity="error">
            <Typography variant="h6">Unauthorized</Typography>
            <Typography>You do not have permission to access the Admin page.</Typography>
          </Alert>
        </Container>
      </AdminLayout>
    )
  }

  return (
    <AdminLayout active="users">
      <Box sx={{ py: 3, backgroundColor: '#f5f5f5', minHeight: '100vh' }}>
        <Container maxWidth="lg">
          <Stack spacing={3}>
            {/* Header */}
            <Box>
              <Typography variant="h4" component="h1" gutterBottom sx={{ fontWeight: 600 }}>
                User Management
              </Typography>
              <Typography variant="body1" color="text.secondary">
  Delete, switch roles between Staff and Admin, mark warnings.
</Typography>

            </Box>

            {/* Error Alert */}
            {error && (
              <Alert severity="error" onClose={() => setError('')}>
                {error}
              </Alert>
            )}

            {/* Loading State */}
            {loading ? (
              <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
                <CircularProgress />
              </Box>
            ) : (
              <TableContainer component={Paper}>
                <Table>
                  <TableHead sx={{ backgroundColor: '#f5f5f5' }}>
                    <TableRow>
                      <TableCell sx={{ fontWeight: 600 }}>Username</TableCell>
                      <TableCell sx={{ fontWeight: 600 }}>Email</TableCell>
                      <TableCell sx={{ fontWeight: 600 }}>Phone</TableCell>
                      <TableCell sx={{ fontWeight: 600 }}>Role</TableCell>
                      <TableCell sx={{ fontWeight: 600 }}>Status</TableCell>
                      <TableCell sx={{ fontWeight: 600 }} align="center">Actions</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {users.map(u => {
                      const id = u.id ?? u.Id
                      const name = u.userName ?? u.UserName
                      const email = u.email ?? u.Email
                      const phone = u.phoneNumber ?? u.PhoneNumber
                      const roleId = Number(u.roleId ?? u.RoleId)
                      const isActive = (u.isActive ?? u.IsActive)
                      return (
                        <TableRow key={id} hover>
                          <TableCell sx={{ fontWeight: 500 }}>{name}</TableCell>
                          <TableCell>{email}</TableCell>
                          <TableCell>{phone}</TableCell>
                          <TableCell>
                            <Chip 
                              label={roleLabel(roleId)}
                              color={getRoleColor(roleId)}
                              size="small"
                              variant="outlined"
                            />
                          </TableCell>
                          <TableCell>
                            <Chip 
                              label={isActive ? 'Active' : 'Inactive'}
                              color={isActive ? 'success' : 'default'}
                              size="small"
                            />
                          </TableCell>
                          <TableCell align="center">
                            <IconButton
                              size="small"
                              onClick={() => { setSelectedUser(u); setOpenDialog(true) }}
                              color="primary"
                            >
                              <EditIcon />
                            </IconButton>
                            <IconButton
                              size="small"
                              onClick={() => onDelete(id)}
                              color="error"
                              disabled={busyId === id}
                            >
                              <DeleteIcon />
                            </IconButton>
                          </TableCell>
                        </TableRow>
                      )
                    })}
                    {users.length === 0 && (
                      <TableRow>
                        <TableCell colSpan={6} align="center" sx={{ py: 3, color: 'text.secondary' }}>
  No users found
</TableCell>
                      </TableRow>
                    )}
                  </TableBody>
                </Table>
              </TableContainer>
            )}

            {/* User Details Dialog */}
            <Dialog open={openDialog} onClose={() => setOpenDialog(false)} maxWidth="sm" fullWidth>
              <DialogTitle>User Details</DialogTitle>
              <DialogContent sx={{ pt: 2 }}>
                {selectedUser && (
                  <Stack spacing={2}>
                    <Box>
                      <Typography variant="body2" color="textSecondary">Username:</Typography>
                      <Typography variant="body1" sx={{ fontWeight: 500 }}>
                        {selectedUser.userName ?? selectedUser.UserName}
                      </Typography>
                    </Box>
                    <Box>
                      <Typography variant="body2" color="textSecondary">Email:</Typography>
                      <Typography variant="body1">{selectedUser.email ?? selectedUser.Email}</Typography>
                    </Box>
                    <Box>
                      <Typography variant="body2" color="textSecondary">Phone:</Typography>
                      <Typography variant="body1">{selectedUser.phoneNumber ?? selectedUser.PhoneNumber}</Typography>
                    </Box>
                    <Box>
                      <Typography variant="body2" color="textSecondary" sx={{ mb: 1 }}>Role:</Typography>
                      <Box sx={{ display: 'flex', gap: 1 }}>
                        <Button
                          variant={Number(selectedUser.roleId ?? selectedUser.RoleId) === 2 ? 'contained' : 'outlined'}
                          size="small"
                          onClick={() => onToggleRole(selectedUser.id ?? selectedUser.Id, selectedUser.roleId ?? selectedUser.RoleId)}
                          disabled={busyId === (selectedUser.id ?? selectedUser.Id) || ![2, 3].includes(Number(selectedUser.roleId ?? selectedUser.RoleId))}
                        >
                          Staff
                        </Button>
                        <Button
                          variant={Number(selectedUser.roleId ?? selectedUser.RoleId) === 3 ? 'contained' : 'outlined'}
                          size="small"
                          onClick={() => onToggleRole(selectedUser.id ?? selectedUser.Id, selectedUser.roleId ?? selectedUser.RoleId)}
                          disabled={busyId === (selectedUser.id ?? selectedUser.Id) || ![2, 3].includes(Number(selectedUser.roleId ?? selectedUser.RoleId))}
                        >
                          Admin
                        </Button>
                      </Box>
                    </Box>
                    <Box>
                      <Button
                        fullWidth
                        variant={selectedUser.isActive ?? selectedUser.IsActive ? 'contained' : 'outlined'}
                        onClick={() => onToggleActive(selectedUser.id ?? selectedUser.Id)}
                        disabled={busyId === (selectedUser.id ?? selectedUser.Id)}
                      >
                        {(selectedUser.isActive ?? selectedUser.IsActive) ? 'Deactivate' : 'Activate'}
                      </Button>
                    </Box>
                    <FormControlLabel
                      control={
                        <Checkbox
                          checked={isWarned(selectedUser.id ?? selectedUser.Id)}
                          onChange={(e) => onWarnChange(selectedUser.id ?? selectedUser.Id, e.target.checked)}
                        />
                      }
                      label="User Warning"
                    />
                  </Stack>
                )}
              </DialogContent>
              <DialogActions>
                <Button onClick={() => setOpenDialog(false)}>Close</Button>
              </DialogActions>
            </Dialog>
          </Stack>
        </Container>
      </Box>
    </AdminLayout>
  )
}
