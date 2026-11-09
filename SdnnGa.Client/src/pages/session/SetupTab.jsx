import { Alert, Card, Skeleton, Steps } from 'antd'
import { useCallback, useEffect, useMemo, useState } from 'react'
import PropTypes from 'prop-types'
import apiCall from '../../utils/apiCall'
import { inspectCsv, suggestLossFunction } from '../../utils/csv'
import { fetchStorageText } from '../../utils/storageFiles'
import DatasetStep from './DatasetStep'
import TrainingStep from './TrainingStep'
import GeneticStep from './GeneticStep'
import LaunchStep from './LaunchStep'

const loadDatasetSide = async path => (path ? inspectCsv(await fetchStorageText(path)) : null)

// Покрокове налаштування сесії: дані → навчання → генетичний алгоритм → запуск.
const SetupTab = ({ overview, onChanged, onStarted }) => {
  const sessionId = overview.id
  const [state, setState] = useState({ loading: true, dataset: { x: null, y: null }, fitConfig: null, geneticConfig: null })
  const [current, setCurrent] = useState(null)

  const load = useCallback(async () => {
    try {
      // Кожен запит окремо: відсутній конфіг не повинен ламати завантаження решти.
      const optional = url => apiCall(url).catch(() => null)
      const [session, fit, genetic] = await Promise.all([
        optional(`Sessions/${sessionId}`),
        optional(`FitConfig/BySession/${sessionId}`),
        optional(`GeneticConfig/BySession/${sessionId}`)
      ])

      const [x, y] = await Promise.all([
        loadDatasetSide(session?.entity?.xTrainFileName).catch(() => null),
        loadDatasetSide(session?.entity?.yTrainFileName).catch(() => null)
      ])

      setState({
        loading: false,
        dataset: { x, y },
        fitConfig: fit?.entity ?? null,
        geneticConfig: genetic?.entity ?? null
      })
    } catch {
      setState(previous => ({ ...previous, loading: false }))
    }
  }, [sessionId])

  useEffect(() => {
    load()
  }, [load])

  const done = useMemo(
    () => [overview.hasData, overview.hasFitConfig, overview.hasGeneticConfig],
    [overview.hasData, overview.hasFitConfig, overview.hasGeneticConfig]
  )

  // Стартуємо з першого незавершеного кроку.
  useEffect(() => {
    if (current === null && !state.loading) {
      const firstIncomplete = done.findIndex(isDone => !isDone)
      setCurrent(firstIncomplete === -1 ? 3 : firstIncomplete)
    }
  }, [current, state.loading, done])

  const afterSave = async nextStep => {
    await Promise.all([load(), onChanged()])
    setCurrent(nextStep)
  }

  if (state.loading || current === null) {
    return (
      <Card>
        <Skeleton active paragraph={{ rows: 8 }} />
      </Card>
    )
  }

  const locked = overview.isRunning
  const { dataset, fitConfig, geneticConfig } = state

  const steps = [
    {
      title: 'Dataset',
      description: dataset.x ? `${dataset.x.rows} rows · ${dataset.x.columns} → ${dataset.y?.columns ?? '?'}` : 'Upload CSV files'
    },
    { title: 'Training', description: fitConfig ? 'Configured' : 'Loss, optimizer, epochs' },
    { title: 'Genetic algorithm', description: geneticConfig ? 'Configured' : 'Generations & mutations' },
    { title: overview.epochCount > 0 ? 'Continue' : 'Run', description: 'Search space & start' }
  ]

  return (
    <Card>
      <Steps
        current={current}
        onChange={setCurrent}
        className="mb-6"
        items={steps.map((step, index) => ({
          ...step,
          status: index === current ? 'process' : index < 3 && done[index] ? 'finish' : 'wait'
        }))}
      />

      {locked && (
        <Alert
          className="mb-5"
          type="info"
          showIcon
          message="The search is running, so settings are locked. You can review them here and follow progress on the Results tab."
        />
      )}

      {current === 0 && (
        <DatasetStep
          sessionId={sessionId}
          current={dataset}
          disabled={locked}
          onUploaded={() => afterSave(1)}
        />
      )}
      {current === 1 && (
        <TrainingStep
          sessionId={sessionId}
          config={fitConfig}
          suggestedLoss={suggestLossFunction(dataset.y)}
          disabled={locked}
          onSaved={() => afterSave(2)}
          onBack={() => setCurrent(0)}
        />
      )}
      {current === 2 && (
        <GeneticStep
          sessionId={sessionId}
          config={geneticConfig}
          disabled={locked}
          onSaved={() => afterSave(3)}
          onBack={() => setCurrent(1)}
        />
      )}
      {current === 3 && (
        <LaunchStep
          overview={overview}
          dataset={dataset}
          fitConfig={fitConfig}
          geneticConfig={geneticConfig}
          disabled={locked}
          onStarted={onStarted}
          onGoToStep={setCurrent}
        />
      )}
    </Card>
  )
}

SetupTab.propTypes = {
  overview: PropTypes.object.isRequired,
  onChanged: PropTypes.func.isRequired,
  onStarted: PropTypes.func.isRequired
}

export default SetupTab
