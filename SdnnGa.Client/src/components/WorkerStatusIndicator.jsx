import { Badge, Tooltip, Typography } from 'antd'
import { useWorkerStatus } from '../hooks/useWorkerStatus'

const STATE_VIEW = {
  Ready: { badge: 'success', title: 'Training engine ready' },
  Starting: { badge: 'processing', title: 'Starting training engine…' },
  NotStarted: { badge: 'processing', title: 'Starting training engine…' },
  Failed: { badge: 'error', title: 'Training engine unavailable' }
}

// Компактний індикатор стану Python/TensorFlow-воркера в боковій панелі.
const WorkerStatusIndicator = () => {
  const { status } = useWorkerStatus()
  const worker = status?.worker
  const view = STATE_VIEW[worker?.state] ?? { badge: 'default', title: 'Connecting…' }

  const details = worker?.state === 'Ready'
    ? `TensorFlow ${worker.tensorFlowVersion} · Python ${worker.pythonVersion}`
    : worker?.message

  return (
    <Tooltip title={details} placement="right">
      <div className="rounded-lg bg-slate-50 border border-slate-200 px-3 py-2 overflow-hidden">
        <Badge status={view.badge} text={<span className="text-xs font-medium text-slate-700">{view.title}</span>} />
        {worker?.state === 'Ready' && (
          <Typography.Text type="secondary" className="block text-xs mt-0.5 whitespace-nowrap">
            TensorFlow {worker.tensorFlowVersion}
          </Typography.Text>
        )}
      </div>
    </Tooltip>
  )
}

export default WorkerStatusIndicator
