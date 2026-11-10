import { Button, Progress, Typography, theme } from 'antd'
import { DeploymentUnitOutlined } from '@ant-design/icons'
import { useEffect, useRef, useState } from 'react'
import PropTypes from 'prop-types'
import { useWorkerStatus } from '../hooks/useWorkerStatus'

// Етапи запуску воркера (відсотки надсилає sdnnga_worker.py) — для плавного руху смуги між ними.
const WORKER_MILESTONES = [0, 2, 5, 15, 25, 90, 100]
const CONTINUE_AFTER_MS = 4000

const nextMilestone = percent => WORKER_MILESTONES.find(value => value > percent) ?? 100

/**
 * Екран завантаження застосунку: інтерфейс → локальний сервер → рушій навчання (Python + TensorFlow).
 * Показується лише під час запуску; після готовності (або "Continue") більше не з'являється.
 */
const StartupScreen = ({ children }) => {
  const { token } = theme.useToken()
  const { status } = useWorkerStatus()
  const [finished, setFinished] = useState(false)
  const [now, setNow] = useState(() => Date.now())
  const mountedAt = useRef(Date.now())
  const stage = useRef({ percent: -1, since: Date.now() })

  const worker = status?.worker
  const workerDone = worker?.state === 'Ready' || worker?.state === 'Failed'

  useEffect(() => {
    if (finished) {
      return undefined
    }
    const timer = setInterval(() => setNow(Date.now()), 200)
    return () => clearInterval(timer)
  }, [finished])

  useEffect(() => {
    if (workerDone) {
      const timer = setTimeout(() => setFinished(true), 500)
      return () => clearTimeout(timer)
    }
    return undefined
  }, [workerDone])

  if (finished) {
    return children
  }

  // Загальний прогрес: 0–10% інтерфейс, 10–20% сервер, 20–100% рушій навчання.
  const workerPercent = worker?.state === 'Ready' ? 100 : worker?.progress ?? 0
  if (workerPercent !== stage.current.percent) {
    stage.current = { percent: workerPercent, since: now }
  }

  let percent
  let label
  if (!status) {
    percent = 10
    label = 'Connecting to the local server…'
  } else if (worker?.state === 'Failed') {
    percent = 100
    label = 'The training engine could not start'
  } else {
    // Усередині етапу смуга повільно наближається до наступного (імпорт TensorFlow — один довгий крок).
    const elapsed = (now - stage.current.since) / 1000
    const creep = (nextMilestone(workerPercent) - workerPercent) * 0.9 * (1 - Math.exp(-elapsed / 10))
    percent = Math.round(20 + (workerPercent + creep) * 0.8)
    label = worker?.state === 'Ready' ? 'Ready' : worker?.message || 'Starting the training engine…'
  }

  const canContinue = now - mountedAt.current > CONTINUE_AFTER_MS && !workerDone

  return (
    <div
      className="fixed inset-0 flex items-center justify-center"
      style={{ background: token.colorBgLayout, zIndex: 1000 }}>
      <div className="w-[380px] text-center px-6">
        <div
          className="mx-auto mb-4 flex h-14 w-14 items-center justify-center rounded-2xl text-2xl text-white"
          style={{ background: token.colorPrimary }}>
          <DeploymentUnitOutlined />
        </div>
        <Typography.Title level={3} className="!mb-1">
          SDNN-GA
        </Typography.Title>
        <Typography.Text type="secondary">Neural architecture search</Typography.Text>

        <Progress
          className="startup-progress !mt-8 !mb-1"
          percent={percent}
          status={worker?.state === 'Failed' ? 'exception' : percent >= 100 ? 'success' : 'active'}
          showInfo={false}
        />
        <div className="flex justify-between text-xs">
          <Typography.Text type="secondary">{label}</Typography.Text>
          <Typography.Text type="secondary">{percent}%</Typography.Text>
        </div>

        <div className="h-16 mt-4">
          {canContinue && (
            <>
              <Button type="link" onClick={() => setFinished(true)}>
                Continue without waiting
              </Button>
              <Typography.Text type="secondary" className="block text-xs">
                You can set up sessions while TensorFlow is loading.
              </Typography.Text>
            </>
          )}
        </div>
      </div>
    </div>
  )
}

StartupScreen.propTypes = {
  children: PropTypes.node
}

export default StartupScreen
