import { Progress, Tooltip, Typography } from 'antd'
import PropTypes from 'prop-types'
import { formatLoss, formatPercent } from '../utils/format'
import { fractionOf } from '../utils/progress'

/**
 * Смуга прогресу покоління: сегменти навчених (зелений), невдалих (червоний), зупинених (сірий)
 * моделей і частка поточного навчання (анімований).
 */
export const GenerationProgress = ({ total, trained = 0, failed = 0, stopped = 0, active = 0, compact = false }) => {
  if (!total) {
    return null
  }

  const finished = trained + failed + stopped
  const percent = Math.min(100, Math.round(((finished + active) / total) * 100))
  const width = value => `${(value / total) * 100}%`

  const bar = (
    <div className={`flex w-full overflow-hidden rounded-full bg-slate-200 ${compact ? 'h-1.5' : 'h-2.5'}`}>
      <div className="bg-emerald-500 transition-all duration-500" style={{ width: width(trained) }} />
      <div className="bg-rose-500 transition-all duration-500" style={{ width: width(failed) }} />
      <div className="bg-slate-400 transition-all duration-500" style={{ width: width(stopped) }} />
      <div className="bg-indigo-500 animate-pulse transition-all duration-500" style={{ width: width(active) }} />
    </div>
  )

  const details = [
    `${trained} trained`,
    failed > 0 && `${failed} failed`,
    stopped > 0 && `${stopped} stopped`,
    total - finished > 0 && `${total - finished} remaining`
  ]
    .filter(Boolean)
    .join(' · ')

  if (compact) {
    return (
      <Tooltip title={details}>
        <div className="flex items-center gap-2 min-w-[160px]">
          {bar}
          <span className="text-xs text-slate-500 whitespace-nowrap">
            {finished}/{total}
          </span>
        </div>
      </Tooltip>
    )
  }

  return (
    <div>
      {bar}
      <div className="flex justify-between mt-1.5 text-xs text-slate-500">
        <span>{details}</span>
        <span>{percent}%</span>
      </div>
    </div>
  )
}

GenerationProgress.propTypes = {
  total: PropTypes.number,
  trained: PropTypes.number,
  failed: PropTypes.number,
  stopped: PropTypes.number,
  active: PropTypes.number,
  compact: PropTypes.bool
}


/** Прогрес навчання однієї моделі: епоха N із M і поточні метрики. */
export const ModelTrainingProgress = ({ progress }) => {
  const percent = Math.round(fractionOf(progress) * 100)
  const metrics = [
    progress.loss != null && `loss ${formatLoss(progress.loss)}`,
    progress.accuracy != null && `acc ${formatPercent(progress.accuracy)}`
  ]
    .filter(Boolean)
    .join(' · ')

  return (
    <div className="min-w-[180px]">
      <Progress percent={percent} size="small" status="active" className="!m-0" />
      <Typography.Text type="secondary" className="block text-xs">
        {progress.epoch === 0 ? 'Starting training…' : `Epoch ${progress.epoch} of up to ${progress.epochs}`}
        {metrics && ` · ${metrics}`}
      </Typography.Text>
    </div>
  )
}

ModelTrainingProgress.propTypes = {
  progress: PropTypes.shape({
    epoch: PropTypes.number,
    epochs: PropTypes.number,
    loss: PropTypes.number,
    accuracy: PropTypes.number
  }).isRequired
}
