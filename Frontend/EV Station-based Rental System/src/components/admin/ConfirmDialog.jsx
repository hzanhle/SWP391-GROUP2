import { Dialog, DialogTitle, DialogContent, DialogContentText, DialogActions, Button } from '@mui/material'
import { Warning as WarningIcon } from '@mui/icons-material'

export function ConfirmDialog({ 
  open, 
  onClose, 
  onConfirm, 
  title = 'Confirm Action', 
  message = 'Are you sure you want to proceed?',
  confirmText = 'Confirm',
  cancelText = 'Cancel',
  severity = 'warning'
}) {
  const handleConfirm = () => {
    onConfirm()
    onClose()
  }

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
        <WarningIcon color={severity} sx={{ fontSize: 24 }} />
        {title}
      </DialogTitle>
      <DialogContent>
        <DialogContentText>
          {message}
        </DialogContentText>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} variant="outlined">
          {cancelText}
        </Button>
        <Button onClick={handleConfirm} variant="contained" color={severity}>
          {confirmText}
        </Button>
      </DialogActions>
    </Dialog>
  )
}

