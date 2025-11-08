import { createTheme } from '@mui/material/styles'

export function makeMuiTheme(mode = 'light') {
  return createTheme({
    palette: {
      mode,
      primary: { main: '#0ea5e9' },
      secondary: { main: '#334155' },
    },
    shape: { borderRadius: 8 },
    typography: { fontFamily: 'Poppins, Rubik, Inter, system-ui, -apple-system, Segoe UI, Roboto, Helvetica, Arial, sans-serif' },
    components: {
      MuiPaper: { defaultProps: { elevation: 0 } },
      MuiButton: { defaultProps: { variant: 'contained' } },
    },
  })
}
