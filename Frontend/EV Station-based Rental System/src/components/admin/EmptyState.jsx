import { Box, Typography } from '@mui/material'
import { Inbox as InboxIcon, Search as SearchIcon, ErrorOutline as ErrorIcon } from '@mui/icons-material'

export function EmptyState({ 
  icon = 'inbox', 
  title = 'No data available', 
  message = 'There is no data to display at this time.',
  action 
}) {
  const IconComponent = {
    inbox: InboxIcon,
    search: SearchIcon,
    error: ErrorIcon
  }[icon] || InboxIcon

  return (
    <Box sx={{ 
      display: 'flex', 
      flexDirection: 'column', 
      alignItems: 'center', 
      justifyContent: 'center',
      py: 6,
      px: 2,
      textAlign: 'center'
    }}>
      <IconComponent sx={{ fontSize: 64, color: 'text.secondary', mb: 2, opacity: 0.5 }} />
      <Typography variant="h6" color="text.secondary" gutterBottom>
        {title}
      </Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 2, maxWidth: 400 }}>
        {message}
      </Typography>
      {action && <Box sx={{ mt: 2 }}>{action}</Box>}
    </Box>
  )
}

