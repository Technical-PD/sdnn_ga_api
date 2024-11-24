import { useState } from 'react'
import {
  MenuFoldOutlined,
  MenuUnfoldOutlined,
  CopyOutlined
} from '@ant-design/icons'
import { Button, Layout, Menu, theme } from 'antd'
import SessionsList from './components/SessionsList'
import { createBrowserRouter, RouterProvider } from 'react-router-dom'
import EpochList from './components/EpochList'
import ModelList from './components/ModelList'
const { Header, Sider, Content } = Layout

const router = createBrowserRouter([
  {
    path: '/',
    element: <SessionsList />
  },
  {
    path: '/session/:sessionId',
    element: <EpochList />
  },
  {
    path: '/session/:sessionId/epoch/:epochId',
    element: <ModelList />
  }
])

const App = () => {
  const [collapsed, setCollapsed] = useState(false)
  const {
    token: { colorBgContainer, borderRadiusLG }
  } = theme.useToken()

  return (
    <Layout style={{ minHeight: 'calc(100vh + 10px)' }}>
      <Sider trigger={null} collapsible collapsed={collapsed}>
        <div className="demo-logo-vertical" />
        <Menu
          theme="dark"
          mode="inline"
          defaultSelectedKeys={['1']}
          items={[
            {
              key: '1',
              icon: <CopyOutlined />,
              label: 'Sessions',
              onClick: () => router.navigate('/')
            }
          ]}
        />
      </Sider>
      <Layout>
        <Header
          style={{
            padding: 0,
            background: colorBgContainer
          }}>
          <Button
            type="text"
            icon={collapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
            onClick={() => setCollapsed(!collapsed)}
            style={{
              fontSize: '16px',
              width: 64,
              height: 64
            }}
          />
        </Header>
        <Content
          style={{
            margin: '24px 16px',
            padding: 24,
            height: '60vh',
            background: colorBgContainer,
            borderRadius: borderRadiusLG,
            overflowY: 'auto'
          }}>
          <RouterProvider router={router} />
        </Content>
      </Layout>
    </Layout>
  )
}
export default App
