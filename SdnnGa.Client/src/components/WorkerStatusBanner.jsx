import { Alert, Button } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import apiCall from '../utils/apiCall'

const POLL_INTERVAL_MS = 3000

// Показує стан локального Python-воркера (TensorFlow), поки він не готовий до навчання моделей.
const WorkerStatusBanner = () => {
  const [status, setStatus] = useState(null)
  const [retrying, setRetrying] = useState(false)

  const fetchStatus = useCallback(async () => {
    try {
      setStatus(await apiCall('System/Status'))
    } catch {
      setStatus(null)
    }
  }, [])

  useEffect(() => {
    fetchStatus()
    const timer = setInterval(fetchStatus, POLL_INTERVAL_MS)
    return () => clearInterval(timer)
  }, [fetchStatus])

  const retry = async () => {
    setRetrying(true)
    try {
      setStatus(await apiCall('System/Worker/Start', 'POST'))
    } finally {
      setRetrying(false)
    }
  }

  const worker = status?.worker
  if (!worker || worker.state === 'Ready') {
    return null
  }

  if (worker.state === 'Failed') {
    return (
      <Alert
        type="error"
        showIcon
        message="Training worker is not available"
        description={
          <div>
            <div style={{ whiteSpace: 'pre-wrap' }}>{worker.message}</div>
            {status.logFile && <div className="mt-2">Log: {status.logFile}</div>}
          </div>
        }
        action={
          <Button size="small" onClick={retry} loading={retrying}>
            Retry
          </Button>
        }
        style={{ margin: '16px 16px 0' }}
      />
    )
  }

  return (
    <Alert
      type="info"
      showIcon
      message="Starting the training worker (loading TensorFlow)…"
      style={{ margin: '16px 16px 0' }}
    />
  )
}

export default WorkerStatusBanner
