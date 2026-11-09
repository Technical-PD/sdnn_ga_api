import { Alert, App, Button, Col, Descriptions, Form, InputNumber, Row, Select, Slider, Switch, Typography } from 'antd'
import { CheckCircleFilled, CloseCircleFilled, RocketOutlined } from '@ant-design/icons'
import { useState } from 'react'
import PropTypes from 'prop-types'
import HelpLabel from '../../components/HelpLabel'
import { useWorkerStatus } from '../../hooks/useWorkerStatus'
import apiCall from '../../utils/apiCall'
import { ACTIVATIONS, labelOf, LOSS_FUNCTIONS, OPTIMIZERS, SEARCH_DEFAULTS, SELECTION_CRITERIA } from '../../constants/options'

const storageKey = sessionId => `sdnnga.search.${sessionId}`

// Останні використані параметри простору пошуку (сервер їх не зберігає).
const loadLastSearch = sessionId => {
  try {
    return JSON.parse(localStorage.getItem(storageKey(sessionId))) || null
  } catch {
    return null
  }
}

const saveLastSearch = (sessionId, values) => {
  try {
    localStorage.setItem(storageKey(sessionId), JSON.stringify(values))
  } catch {
    // localStorage може бути недоступним — це лише зручність.
  }
}

const Check = ({ ok, children, action }) => (
  <div className="flex items-center gap-2 text-sm">
    {ok ? <CheckCircleFilled className="text-emerald-500" /> : <CloseCircleFilled className="text-rose-500" />}
    <span className={ok ? 'text-slate-700' : 'text-slate-900 font-medium'}>{children}</span>
    {!ok && action}
  </div>
)

Check.propTypes = { ok: PropTypes.bool, children: PropTypes.node, action: PropTypes.node }

// Крок 4: простір пошуку початкових моделей і запуск генетичного алгоритму.
const LaunchStep = ({ overview, dataset, fitConfig, geneticConfig, disabled, onStarted, onGoToStep }) => {
  const { message } = App.useApp()
  const { status: workerStatus } = useWorkerStatus()
  const [form] = Form.useForm()
  const [starting, setStarting] = useState(false)
  const activation = Form.useWatch('activationFunc', form)
  const neurons = Form.useWatch('neurons', form)

  const last = loadLastSearch(overview.id)
  const initialValues = {
    ...SEARCH_DEFAULTS,
    ...last,
    // Розміри входу/виходу завжди беремо з поточних даних, якщо вони є.
    inputCount: dataset.x?.columns ?? last?.inputCount ?? 1,
    outputCount: dataset.y?.columns ?? last?.outputCount ?? 1
  }

  const workerReady = workerStatus?.worker?.state === 'Ready'
  const ready = overview.hasData && overview.hasFitConfig && overview.hasGeneticConfig
  const isContinue = overview.epochCount > 0
  const reachedLimit = isContinue && overview.maxEpoches != null && overview.epochCount >= overview.maxEpoches
  const crossEntropy = fitConfig?.lossFunc?.includes('crossentropy')
  const riskyOutput = crossEntropy && ['relu', 'linear', 'tanh'].includes(activation)

  const start = async values => {
    setStarting(true)
    try {
      const result = await apiCall(`GeneticFlow/StartFlow?sessionId=${overview.id}`, 'POST', {
        modelRangeConfig: {
          inputCount: values.inputCount,
          outputCount: values.outputCount,
          countOfInternalLayers: values.countOfInternalLayers,
          neuronsInLayerMin: values.neurons[0],
          neuronsInLayerMax: values.neurons[1],
          activationFunc: values.activationFunc,
          useBias: values.useBias
        }
      })

      if (result?.isError) {
        message.error(result.message)
        return
      }

      saveLastSearch(overview.id, values)
      message.success(isContinue ? 'Search continued' : 'Search started')
      await onStarted()
    } catch (error) {
      message.error(`Could not start: ${error.message}`)
    } finally {
      setStarting(false)
    }
  }

  return (
    <Row gutter={[32, 24]}>
      <Col xs={24} xl={14}>
        <Typography.Title level={5} className="!mt-0">
          Starting architecture
        </Typography.Title>
        <Typography.Paragraph type="secondary" className="text-sm">
          {isContinue
            ? 'The search continues from the best model found so far. These settings are only used for brand-new sessions.'
            : 'The first generation is generated randomly within these limits. Later generations evolve from the best model.'}
        </Typography.Paragraph>

        <Form form={form} layout="vertical" initialValues={initialValues} onFinish={start} disabled={disabled || starting} requiredMark={false}>
          <Row gutter={16}>
            <Col xs={12} md={8}>
              <Form.Item
                name="inputCount"
                label={<HelpLabel label="Inputs" help="Number of feature columns in X. Detected from your dataset." />}
                rules={[{ required: true }]}>
                <InputNumber min={1} className="w-full" />
              </Form.Item>
            </Col>
            <Col xs={12} md={8}>
              <Form.Item
                name="outputCount"
                label={<HelpLabel label="Outputs" help="Number of target columns in Y. Detected from your dataset." />}
                rules={[{ required: true }]}>
                <InputNumber min={1} className="w-full" />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item
                name="countOfInternalLayers"
                label={<HelpLabel label="Hidden layers" help="Number of hidden layers in the first generation. Mutations can add or remove layers later." />}
                rules={[{ required: true }]}>
                <InputNumber min={1} max={10} className="w-full" />
              </Form.Item>
            </Col>
          </Row>

          <Form.Item
            name="neurons"
            label={
              <span>
                <HelpLabel label="Neurons per hidden layer" help="Each hidden layer gets a random size within this range." />
                {neurons && <span className="ml-2 text-slate-500">{neurons[0]}–{neurons[1]}</span>}
              </span>
            }>
            <Slider range min={1} max={256} marks={{ 1: '1', 64: '64', 128: '128', 256: '256' }} />
          </Form.Item>

          <Row gutter={16}>
            <Col xs={24} md={12}>
              <Form.Item
                name="activationFunc"
                label={<HelpLabel label="Activation function" help="Used by the hidden layers and the output layer of the first generation." />}>
                <Select options={ACTIVATIONS} />
              </Form.Item>
            </Col>
            <Col xs={24} md={12}>
              <Form.Item name="useBias" label="Use bias in hidden layers" valuePropName="checked">
                <Switch />
              </Form.Item>
            </Col>
          </Row>

          {riskyOutput && (
            <Alert
              type="warning"
              showIcon
              className="mb-4"
              message={`The output layer also uses ${labelOf(ACTIVATIONS, activation)}. With cross-entropy loss this often makes training diverge. Sigmoid or Softmax is safer.`}
            />
          )}
        </Form>
      </Col>

      <Col xs={24} xl={10}>
        <div className="rounded-lg border border-slate-200 bg-slate-50 p-4">
          <Typography.Title level={5} className="!mt-0">
            Summary
          </Typography.Title>
          <Descriptions
            size="small"
            column={1}
            items={[
              { key: 'data', label: 'Dataset', children: dataset.x ? `${dataset.x.rows} rows, ${dataset.x.columns} → ${dataset.y?.columns ?? '?'} columns` : '—' },
              { key: 'loss', label: 'Loss', children: labelOf(LOSS_FUNCTIONS, fitConfig?.lossFunc) },
              { key: 'optimizer', label: 'Optimizer', children: `${labelOf(OPTIMIZERS, fitConfig?.fitMethod)}, ${fitConfig?.maxEpoches ?? '—'} epochs` },
              {
                key: 'evolution',
                label: 'Evolution',
                children: geneticConfig ? `${geneticConfig.maxEpoches} generations × ${geneticConfig.countOfModelsInEpoch} models` : '—'
              },
              { key: 'selection', label: 'Parent', children: labelOf(SELECTION_CRITERIA, geneticConfig?.selectionCriterion) },
              {
                key: 'stop',
                label: 'Stop at',
                children: geneticConfig ? `accuracy ≥ ${Math.round(geneticConfig.stopAccValue)}% or loss ≤ ${geneticConfig.stopLossValue}` : '—'
              }
            ]}
          />

          <div className="flex flex-col gap-1.5 mt-4">
            <Check ok={overview.hasData} action={<Button size="small" type="link" onClick={() => onGoToStep(0)}>Upload</Button>}>
              Dataset uploaded
            </Check>
            <Check ok={overview.hasFitConfig} action={<Button size="small" type="link" onClick={() => onGoToStep(1)}>Configure</Button>}>
              Training configured
            </Check>
            <Check ok={overview.hasGeneticConfig} action={<Button size="small" type="link" onClick={() => onGoToStep(2)}>Configure</Button>}>
              Genetic algorithm configured
            </Check>
            <Check ok={workerReady}>Training engine ready</Check>
          </div>

          {reachedLimit && !disabled && (
            <Alert
              className="mt-4"
              type="info"
              showIcon
              message={`All ${overview.maxEpoches} generations are done. Continuing adds one more generation; raise the generation limit in step 3 to evolve further.`}
            />
          )}

          <Button
            type="primary"
            size="large"
            block
            className="mt-4"
            icon={<RocketOutlined />}
            loading={starting}
            disabled={disabled || !ready || !workerReady}
            onClick={() => form.submit()}>
            {disabled ? 'Search is running…' : isContinue ? 'Continue search' : 'Start search'}
          </Button>
        </div>
      </Col>
    </Row>
  )
}

LaunchStep.propTypes = {
  overview: PropTypes.object.isRequired,
  dataset: PropTypes.object.isRequired,
  fitConfig: PropTypes.object,
  geneticConfig: PropTypes.object,
  disabled: PropTypes.bool,
  onStarted: PropTypes.func.isRequired,
  onGoToStep: PropTypes.func.isRequired
}

export default LaunchStep
