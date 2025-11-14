import { Box, Skeleton } from '@mui/material'

export function TableSkeleton({ rows = 5, cols = 5 }) {
  return (
    <Box>
      <Skeleton variant="rectangular" height={56} sx={{ mb: 1, borderRadius: 1 }} />
      {Array.from({ length: rows }).map((_, i) => (
        <Box key={i} sx={{ display: 'flex', gap: 1, mb: 1 }}>
          {Array.from({ length: cols }).map((_, j) => (
            <Skeleton key={j} variant="rectangular" height={52} sx={{ flex: 1, borderRadius: 1 }} />
          ))}
        </Box>
      ))}
    </Box>
  )
}

export function CardSkeleton({ lines = 3 }) {
  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
      <Skeleton variant="rectangular" height={40} sx={{ borderRadius: 1 }} />
      {Array.from({ length: lines }).map((_, i) => (
        <Skeleton key={i} variant="text" height={24} />
      ))}
    </Box>
  )
}

