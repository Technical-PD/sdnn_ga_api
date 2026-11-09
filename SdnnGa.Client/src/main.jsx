import { createRoot } from 'react-dom/client'
import { App as AntApp, ConfigProvider } from 'antd'
import './index.css'
import App from './App.jsx'
import theme from './theme'

createRoot(document.getElementById('root')).render(
  <ConfigProvider theme={theme}>
    <AntApp>
      <App />
    </AntApp>
  </ConfigProvider>
)
