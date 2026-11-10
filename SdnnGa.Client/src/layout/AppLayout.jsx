import { Layout, Menu, Segmented, Tooltip, Typography, theme } from 'antd'
import { AppstoreOutlined, DeploymentUnitOutlined, DesktopOutlined, MoonOutlined, SunOutlined } from '@ant-design/icons'
import { Outlet, useLocation, useNavigate } from 'react-router-dom'
import WorkerStatusBanner from '../components/WorkerStatusBanner'
import WorkerStatusIndicator from '../components/WorkerStatusIndicator'
import { useThemeMode } from '../hooks/useThemeMode'

const { Sider, Content } = Layout

const THEME_OPTIONS = [
  { value: 'light', label: <Tooltip title="Light theme"><SunOutlined /></Tooltip> },
  { value: 'dark', label: <Tooltip title="Dark theme"><MoonOutlined /></Tooltip> },
  { value: 'system', label: <Tooltip title="Follow Windows"><DesktopOutlined /></Tooltip> }
]

const AppLayout = () => {
  const navigate = useNavigate()
  const location = useLocation()
  const { token } = theme.useToken()
  const { mode, setMode } = useThemeMode()

  // Усі сторінки — це сесії та їх вкладені розділи.
  const selectedKey = location.pathname === '/' || location.pathname.startsWith('/session') ? 'sessions' : ''

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Sider
        width={232}
        style={{
          position: 'sticky',
          top: 0,
          height: '100vh',
          background: token.colorBgContainer,
          borderRight: `1px solid ${token.colorBorderSecondary}`
        }}>
        <div className="flex flex-col h-full">
          <div className="flex items-center gap-3 px-5 py-5">
            <div
              className="flex items-center justify-center w-9 h-9 rounded-lg text-white text-lg shrink-0"
              style={{ background: token.colorPrimary }}>
              <DeploymentUnitOutlined />
            </div>
            <div className="min-w-0 overflow-hidden">
              <div className="font-semibold text-base leading-tight" style={{ color: token.colorText }}>
                SDNN-GA
              </div>
              <Typography.Text type="secondary" className="text-xs whitespace-nowrap">
                Neural architecture search
              </Typography.Text>
            </div>
          </div>

          <Menu
            mode="inline"
            selectedKeys={[selectedKey]}
            style={{ borderInlineEnd: 'none', padding: '0 8px', background: 'transparent' }}
            items={[
              {
                key: 'sessions',
                icon: <AppstoreOutlined />,
                label: 'Sessions',
                onClick: () => navigate('/')
              }
            ]}
          />

          <div className="mt-auto p-3 flex flex-col gap-3">
            <div className="flex items-center justify-between px-1">
              <Typography.Text type="secondary" className="text-xs">
                Theme
              </Typography.Text>
              <Segmented size="small" value={mode} onChange={setMode} options={THEME_OPTIONS} />
            </div>
            <WorkerStatusIndicator />
          </div>
        </div>
      </Sider>

      <Layout>
        <WorkerStatusBanner />
        <Content style={{ padding: '24px 32px 40px' }}>
          <div style={{ maxWidth: 1360, margin: '0 auto' }}>
            <Outlet />
          </div>
        </Content>
      </Layout>
    </Layout>
  )
}

export default AppLayout
