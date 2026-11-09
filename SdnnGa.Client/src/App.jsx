import { createBrowserRouter, RouterProvider } from 'react-router-dom'
import AppLayout from './layout/AppLayout'
import SessionsPage from './pages/SessionsPage'
import SessionPage from './pages/SessionPage'
import EpochPage from './pages/EpochPage'
import NotFoundPage from './pages/NotFoundPage'
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
    <RouterProvider router={router} />
  </WorkerStatusProvider>
)

export default App
