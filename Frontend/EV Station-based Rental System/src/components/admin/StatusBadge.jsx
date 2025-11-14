import { Chip } from '@mui/material'

const statusConfig = {
  // User/Account Status
  active: { label: 'Active', color: 'success' },
  inactive: { label: 'Inactive', color: 'default' },
  banned: { label: 'Banned', color: 'error' },
  
  // Vehicle Status
  available: { label: 'Available', color: 'success' },
  rented: { label: 'Rented', color: 'warning' },
  maintenance: { label: 'Maintenance', color: 'error' },
  unavailable: { label: 'Unavailable', color: 'default' },
  
  // Order Status
  pending: { label: 'Pending', color: 'warning' },
  confirmed: { label: 'Confirmed', color: 'info' },
  completed: { label: 'Completed', color: 'success' },
  cancelled: { label: 'Cancelled', color: 'error' },
  
  // Transfer Status
  in_transit: { label: 'In Transit', color: 'warning' },
  completed: { label: 'Completed', color: 'success' },
  failed: { label: 'Failed', color: 'error' },
  
  // Role
  member: { label: 'Member', color: 'default' },
  staff: { label: 'Staff', color: 'info' },
  admin: { label: 'Admin', color: 'error' },
}

export function StatusBadge({ status, customLabel, customColor }) {
  const config = statusConfig[status?.toLowerCase()] || { 
    label: customLabel || status || 'Unknown', 
    color: customColor || 'default' 
  }
  
  return (
    <Chip 
      label={config.label} 
      color={config.color}
      size="small"
      sx={{ fontWeight: 500 }}
    />
  )
}

