import { Box, Button, Typography, Select, MenuItem, FormControl, InputLabel } from '@mui/material'
import { ChevronLeft as ChevronLeftIcon, ChevronRight as ChevronRightIcon } from '@mui/icons-material'

export function TablePagination({ 
  page = 0, 
  rowsPerPage = 10, 
  totalRows = 0, 
  onPageChange, 
  onRowsPerPageChange,
  rowsPerPageOptions = [5, 10, 25, 50]
}) {
  const totalPages = Math.ceil(totalRows / rowsPerPage)
  const startRow = page * rowsPerPage + 1
  const endRow = Math.min((page + 1) * rowsPerPage, totalRows)

  return (
    <Box sx={{ 
      display: 'flex', 
      alignItems: 'center', 
      justifyContent: 'space-between',
      flexWrap: 'wrap',
      gap: 2,
      p: 2,
      borderTop: 1,
      borderColor: 'divider'
    }}>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
        <Typography variant="body2" color="text.secondary">
          Showing {startRow} to {endRow} of {totalRows} entries
        </Typography>
        <FormControl size="small" sx={{ minWidth: 120 }}>
          <InputLabel>Rows per page</InputLabel>
          <Select
            value={rowsPerPage}
            label="Rows per page"
            onChange={(e) => onRowsPerPageChange(e.target.value)}
          >
            {rowsPerPageOptions.map(option => (
              <MenuItem key={option} value={option}>{option}</MenuItem>
            ))}
          </Select>
        </FormControl>
      </Box>
      
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
        <Button
          onClick={() => onPageChange(page - 1)}
          disabled={page === 0}
          startIcon={<ChevronLeftIcon />}
          variant="outlined"
          size="small"
        >
          Previous
        </Button>
        <Typography variant="body2" sx={{ px: 2 }}>
          Page {page + 1} of {totalPages || 1}
        </Typography>
        <Button
          onClick={() => onPageChange(page + 1)}
          disabled={page >= totalPages - 1}
          endIcon={<ChevronRightIcon />}
          variant="outlined"
          size="small"
        >
          Next
        </Button>
      </Box>
    </Box>
  )
}

