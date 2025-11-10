import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react'

const ThemeModeContext = createContext({ mode: 'light', setMode: () => {}, toggleMode: () => {} })

function getInitialMode() {
  try {
    const saved = localStorage.getItem('theme')
    if (saved === 'dark' || saved === 'light') return saved
  } catch {}
  if (typeof window !== 'undefined' && window.matchMedia) {
    return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
  }
  return 'light'
}

export function ThemeModeProvider({ children }) {
  const [mode, setMode] = useState(getInitialMode)

  useEffect(() => {
    const el = document.documentElement
    el.dataset.theme = mode
    try {
      localStorage.setItem('theme', mode)
    } catch {}
  }, [mode])

  useEffect(() => {
    const onStorage = (e) => {
      if (e.key === 'theme' && (e.newValue === 'dark' || e.newValue === 'light')) {
        setMode(e.newValue)
      }
    }
    window.addEventListener('storage', onStorage)
    return () => window.removeEventListener('storage', onStorage)
  }, [])

  useEffect(() => {
    const mq = window.matchMedia('(prefers-color-scheme: dark)')
    const onChange = () => {
      try {
        const saved = localStorage.getItem('theme')
        if (saved !== 'dark' && saved !== 'light') {
          setMode(mq.matches ? 'dark' : 'light')
        }
      } catch {}
    }
    mq.addEventListener?.('change', onChange)
    return () => mq.removeEventListener?.('change', onChange)
  }, [])

  const toggleMode = useCallback(() => setMode((m) => (m === 'dark' ? 'light' : 'dark')), [])

  const value = useMemo(() => ({ mode, setMode, toggleMode }), [mode, toggleMode])

  return <ThemeModeContext.Provider value={value}>{children}</ThemeModeContext.Provider>
}

export function useThemeMode() {
  return useContext(ThemeModeContext)
}
