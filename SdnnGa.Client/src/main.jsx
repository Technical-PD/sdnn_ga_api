import { createRoot } from 'react-dom/client'
import './styles/palette.css'
import './index.css'
import ThemedApp from './ThemedApp.jsx'
import { ThemeModeProvider } from './hooks/useThemeMode'

createRoot(document.getElementById('root')).render(
  <ThemeModeProvider>
    <ThemedApp />
  </ThemeModeProvider>
)

// Заставка з index.html більше не потрібна — далі показується StartupScreen.
document.getElementById('boot-splash')?.remove()
