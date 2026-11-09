import { Alert, Button } from 'antd'
import { useWorkerStatus } from '../hooks/useWorkerStatus'

// Показується над сторінкою, лише поки воркер не готовий (запуск або помилка).
const WorkerStatusBanner = () => {
  const { status, retry, retrying } = useWorkerStatus()
  const worker = status?.worker

  if (!worker || worker.state === 'Ready') {
    return null
  }

  if (worker.state === 'Failed') {
    return (
      <Alert
        type="error"
        showIcon
        banner
        message="The training engine (Python + TensorFlow) could not start. Models cannot be trained until this is fixed."
        description={
          <div className="text-xs">
            <div style={{ whiteSpace: 'pre-wrap' }}>{worker.message}</div>
            {status.logFile && <div className="mt-1 text-slate-500">Log file: {status.logFile}</div>}
          </div>
        }
        action={
          <Button size="small" onClick={retry} loading={retrying}>
            Try again
          </Button>
        }
      />
    )
  }

  return (
    <Alert
      type="info"
      showIcon
      banner
      message="Starting the training engine (loading TensorFlow). This usually takes 10–30 seconds. You can set up sessions in the meantime."
    />
  )
}

export default WorkerStatusBanner
