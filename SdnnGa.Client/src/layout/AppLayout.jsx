import { Layout, Menu, Typography } from 'antd'
import { AppstoreOutlined, DeploymentUnitOutlined } from '@ant-design/icons'
import { Outlet, useLocation, useNavigate } from 'react-router-dom'
import WorkerStatusBanner from '../components/WorkerStatusBanner'
import WorkerStatusIndicator from '../components/WorkerStatusIndicator'

const { Sider, Content } = Layout

const AppLayout = () => {
  const navigate = useNavigate()
  const location = useLocation()

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
          borderRight: '1px solid #e5e7eb'
        }}>
        <div className="flex flex-col h-full">
          <div className="flex items-center gap-3 px-5 py-5">
            <div className="flex items-center justify-center w-9 h-9 rounded-lg bg-indigo-600 text-white text-lg shrink-0">
              <DeploymentUnitOutlined />
            </div>
            <div className="min-w-0 overflow-hidden">
              <div className="font-semibold text-base leading-tight text-slate-900">SDNN-GA</div>
              <Typography.Text type="secondary" className="text-xs whitespace-nowrap">
                Neural architecture search
              </Typography.Text>
            </div>
          </div>

          <Menu
            mode="inline"
            selectedKeys={[selectedKey]}
            style={{ borderInlineEnd: 'none', padding: '0 8px' }}
            items={[
              {
                key: 'sessions',
                icon: <AppstoreOutlined />,
                label: 'Sessions',
                onClick: () => navigate('/')
              }
            ]}
          />

          <div className="mt-auto p-3">
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
