import { Alert, Button, Progress } from 'antd'
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
      message={
        <div className="flex flex-wrap items-center gap-x-4 gap-y-1">
          <span>
            Starting the training engine: {worker.message || 'loading TensorFlow'}. You can set up sessions in the meantime.
          </span>
          {worker.progress != null && (
            <Progress percent={worker.progress} size="small" status="active" className="!m-0 w-48" />
          )}
        </div>
      }
    />
  )
}

export default WorkerStatusBanner
