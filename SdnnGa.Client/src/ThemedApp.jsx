import { App as AntApp, ConfigProvider } from 'antd'
import App from './App.jsx'
import { buildTheme } from './theme'
import { useThemeMode } from './hooks/useThemeMode'

// Тема Ant Design залежить від вибраного режиму (світлий/темний/системний).
const ThemedApp = () => {
  const { isDark } = useThemeMode()
  return (
    <ConfigProvider theme={buildTheme(isDark)}>
      <AntApp>
        <App />
      </AntApp>
    </ConfigProvider>
  )
}

export default ThemedApp
