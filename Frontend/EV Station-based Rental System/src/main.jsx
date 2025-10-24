import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import './styles/tokens.css'
import './styles/main.css'
import './styles/featured.css'
import './styles/profile.css'
import './styles/notification.css'
import './styles/admin.css'
import App from './App.jsx'

// Initialize theme early to avoid flash
try {
  const saved = localStorage.getItem('theme')
  if (saved === 'dark' || saved === 'light') {
    document.documentElement.dataset.theme = saved
  }
} catch {}

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <App />
  </StrictMode>,
)
