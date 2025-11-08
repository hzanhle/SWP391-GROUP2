import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import './styles/tokens.css'
import './styles/main.css'
import './styles/featured.css'
import './styles/profile.css'
import './styles/notification.css'
import './styles/admin.css'
import './styles/dark.css'
import App from './App.jsx'
import { ThemeProvider as MuiThemeProvider, CssBaseline } from '@mui/material'
import { ThemeModeProvider, useThemeMode } from './theme/ThemeContext.jsx'
import { makeMuiTheme } from './theme/muiTheme.js'

// Initialize theme early to avoid flash
try {
  const saved = localStorage.getItem('theme')
  if (saved === 'dark' || saved === 'light') {
    document.documentElement.dataset.theme = saved
  }
} catch {}

function Root() {
  const { mode } = useThemeMode()
  const theme = makeMuiTheme(mode)
  return (
    <MuiThemeProvider theme={theme}>
      <CssBaseline />
      <App />
    </MuiThemeProvider>
  )
}

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <ThemeModeProvider>
      <Root />
    </ThemeModeProvider>
  </StrictMode>,
)
