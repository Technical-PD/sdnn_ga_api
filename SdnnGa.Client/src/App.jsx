import { createBrowserRouter, RouterProvider } from 'react-router-dom'
import AppLayout from './layout/AppLayout'
import SessionsPage from './pages/SessionsPage'
import SessionPage from './pages/SessionPage'
import EpochPage from './pages/EpochPage'
import NotFoundPage from './pages/NotFoundPage'
import StartupScreen from './components/StartupScreen'
import { WorkerStatusProvider } from './hooks/useWorkerStatus'

const router = createBrowserRouter([
  {
    element: <AppLayout />,
    children: [
      { path: '/', element: <SessionsPage /> },
      { path: '/session/:sessionId', element: <SessionPage /> },
      { path: '/session/:sessionId/epoch/:epochId', element: <EpochPage /> },
      { path: '*', element: <NotFoundPage /> }
    ]
  }
])

const App = () => (
  <WorkerStatusProvider>
    <StartupScreen>
      <RouterProvider router={router} />
    </StartupScreen>
  </WorkerStatusProvider>
)

export default App
