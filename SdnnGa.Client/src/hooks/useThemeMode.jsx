import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react'
import PropTypes from 'prop-types'

// Ключ дублюється в index.html (скрипт, що вмикає темну тему ще до завантаження React).
export const THEME_STORAGE_KEY = 'sdnnga.theme'

const MODES = ['light', 'dark', 'system']

const readStoredMode = () => {
  try {
    const value = localStorage.getItem(THEME_STORAGE_KEY)
    return MODES.includes(value) ? value : 'system'
  } catch {
    return 'system'
  }
}

const systemPrefersDark = () => window.matchMedia?.('(prefers-color-scheme: dark)').matches ?? false

const ThemeModeContext = createContext({ mode: 'system', isDark: false, setMode: () => {} })

// Світла / темна / системна тема: зберігається між запусками, застосовується до <html class="dark">.
export const ThemeModeProvider = ({ children }) => {
  const [mode, setModeState] = useState(readStoredMode)
  const [systemDark, setSystemDark] = useState(systemPrefersDark)

  useEffect(() => {
    const query = window.matchMedia?.('(prefers-color-scheme: dark)')
    if (!query) {
      return undefined
    }
    const onChange = event => setSystemDark(event.matches)
    query.addEventListener('change', onChange)
    return () => query.removeEventListener('change', onChange)
  }, [])

  const isDark = mode === 'dark' || (mode === 'system' && systemDark)

  useEffect(() => {
    const root = document.documentElement
    root.classList.toggle('dark', isDark)
    root.style.colorScheme = isDark ? 'dark' : 'light'
  }, [isDark])

  const setMode = useCallback(next => {
    setModeState(next)
    try {
      localStorage.setItem(THEME_STORAGE_KEY, next)
    } catch {
      // Лише зручність — без localStorage тема просто не запам'ятається.
    }
  }, [])

  const value = useMemo(() => ({ mode, isDark, setMode }), [mode, isDark, setMode])

  return <ThemeModeContext.Provider value={value}>{children}</ThemeModeContext.Provider>
}

ThemeModeProvider.propTypes = {
  children: PropTypes.node
}

// eslint-disable-next-line react-refresh/only-export-components
export const useThemeMode = () => useContext(ThemeModeContext)
