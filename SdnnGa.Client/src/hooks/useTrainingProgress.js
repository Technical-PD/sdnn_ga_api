import { useMemo } from 'react'
import useApiData from './useApiData'

const POLL_MS = 1500

/**
 * Живий прогрес навчання моделей сесії (епоха за епохою). Опитує сервер лише коли `active`.
 * Повертає { list, byModelId }.
 */
const useTrainingProgress = (sessionId, active) => {
  const [list] = useApiData(active && sessionId ? `NeuralNetworkModel/Progress/BySession/${sessionId}` : null, {
    pollIntervalMs: POLL_MS,
    initialData: []
  })

  const current = useMemo(() => (active ? list || [] : []), [active, list])
  const byModelId = useMemo(() => Object.fromEntries(current.map(item => [item.modelId, item])), [current])

  return { list: current, byModelId }
}

export default useTrainingProgress
