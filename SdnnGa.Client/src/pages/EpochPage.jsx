import { Alert, App, Button, Card, Col, Dropdown, Progress, Row, Skeleton, Statistic, Table, Tag, Tooltip, Typography } from 'antd'
import { DownloadOutlined, DownOutlined, TrophyFilled } from '@ant-design/icons'
import { useMemo } from 'react'
import { useParams, useSearchParams } from 'react-router-dom'
import PropTypes from 'prop-types'
import PageHeader from '../components/PageHeader'
import { ModelStatusTag } from '../components/StatusTag'
import { modelState } from '../utils/modelState'
import MetricsChart from '../components/MetricsChart'
import ModelArchitecture from '../components/ModelArchitecture'
import useApiData from '../hooks/useApiData'
import useTrainingProgress from '../hooks/useTrainingProgress'
import { GenerationProgress, ModelTrainingProgress } from '../components/TrainingProgress'
import { fractionOf } from '../utils/progress'
import StopSearchButton from '../components/StopSearchButton'
import { labelOf, MODEL_ORIGINS, SELECTION_CRITERIA } from '../constants/options'
import { downloadStorageFile } from '../utils/storageFiles'
import { formatLoss, formatPercent } from '../utils/format'
import NotFoundPage from './NotFoundPage'

const parseHistory = fitHistory => {
  try {
    return JSON.parse(fitHistory) || {}
  } catch {
    return {}
  }
}

const STATE_ORDER = { Trained: 0, Training: 1, Queued: 2, Stopped: 3, Failed: 4 }

// Моделі, створені до появи схрещування, не мають origin: 0-ве покоління — випадкове, решта — мутації.
const originOf = (model, epochNo) => model.origin ?? (epochNo === 0 ? 'Random' : 'Mutation')

const parentsOf = model =>
  (model.parentModelNames || '')
    .split(',')
    .map(name => name.trim())
    .filter(Boolean)

const OriginCell = ({ model, epochNo }) => {
  const origin = originOf(model, epochNo)
  const view = MODEL_ORIGINS[origin] ?? MODEL_ORIGINS.Mutation
  const parents = parentsOf(model)

  return (
    <div>
      <Tag color={view.color} className="m-0">
        {view.label}
      </Tag>
      {parents.length > 0 && (
        <div className="text-xs text-slate-500 mt-1">
          {origin === 'Mutation' ? 'of ' : ''}
          {parents.join(' × ')}
        </div>
      )}
    </div>
  )
}

OriginCell.propTypes = { model: PropTypes.object.isRequired, epochNo: PropTypes.number }

// Підзаголовок: як виведено покоління (усі моделі покоління мають однакове походження).
const describeBreeding = (models, epochNo) => {
  const sample = models[0]
  if (!sample) {
    return ''
  }
  const origin = originOf(sample, epochNo)
  const parents = parentsOf(sample)
  if (origin === 'Random') {
    return 'Randomly generated within the search space.'
  }
  const how = (MODEL_ORIGINS[origin]?.label ?? origin).toLowerCase()
  return parents.length > 0 ? `Bred by ${how} of ${parents.join(' × ')}.` : `Bred by ${how}.`
}

const DownloadMenu = ({ model }) => {
  const { message } = App.useApp()

  const download = async (path, fileName) => {
    try {
      await downloadStorageFile(path, fileName)
    } catch (error) {
      message.error(`Download failed: ${error.message}`)
    }
  }

  const items = [
    {
      key: 'keras',
      label: 'Keras model (.json)',
      disabled: !model.modelConfigFileName,
      onClick: () => download(model.modelConfigFileName, `${model.name}.keras-model.json`)
    },
    {
      key: 'weights',
      label: 'Trained weights (.weights.h5)',
      disabled: !model.weightsFileName,
      onClick: () => download(model.weightsFileName, `${model.name}.weights.h5`)
    },
    {
      key: 'ga',
      label: 'Genetic config (.json)',
      disabled: !model.modelConfigDotNetFileName,
      onClick: () => download(model.modelConfigDotNetFileName, `${model.name}.ga-config.json`)
    }
  ]

  return (
    <span onClick={event => event.stopPropagation()}>
      <Dropdown menu={{ items }} trigger={['click']}>
        <Button size="small" icon={<DownloadOutlined />}>
          Download <DownOutlined />
        </Button>
      </Dropdown>
    </span>
  )
}

DownloadMenu.propTypes = { model: PropTypes.object.isRequired }

const ModelDetails = ({ model }) => {
  const history = parseHistory(model.fitHistory)

  return (
    <div className="flex flex-col gap-4 py-2">
      {model.fitError && <Alert type="error" showIcon message="This model failed" description={model.fitError} />}

      {model.isTrained && (
        <Row gutter={[16, 16]}>
          <Col xs={24} xl={12}>
            <Typography.Text strong>Accuracy during training</Typography.Text>
            <MetricsChart
              series={[
                { name: 'Training', values: history.accuracy },
                { name: 'Validation', values: history.val_accuracy }
              ]}
              xLabel="Epoch"
              yFormatter={formatPercent}
              colors={['#16a34a', '#86efac']}
              height={200}
            />
          </Col>
          <Col xs={24} xl={12}>
            <Typography.Text strong>Loss during training</Typography.Text>
            <MetricsChart
              series={[
                { name: 'Training', values: history.loss },
                { name: 'Validation', values: history.val_loss }
              ]}
              xLabel="Epoch"
              yFormatter={value => formatLoss(value)}
              colors={['#4f46e5', '#a5b4fc']}
              height={200}
            />
          </Col>
        </Row>
      )}

      <div>
        <Typography.Text strong className="block mb-2">
          Architecture
        </Typography.Text>
        <ModelArchitecture configPath={model.modelConfigDotNetFileName} />
      </div>
    </div>
  )
}

ModelDetails.propTypes = { model: PropTypes.object.isRequired }

// Сторінка покоління: усі моделі-кандидати з метриками, графіками навчання та завантаженням.
const EpochPage = () => {
  const { sessionId, epochId } = useParams()
  // ?model=<id> — відкрити деталі конкретної моделі (посилання "View model" з результатів).
  const [searchParams] = useSearchParams()
  const focusModelId = searchParams.get('model')
  const [overview, overviewLoading, refreshOverview] = useApiData(`Sessions/${sessionId}/Overview`, { pollIntervalMs: 4000 })
  const [models, modelsLoading, refreshModels] = useApiData(`NeuralNetworkModel/ByEpoche/${epochId}`, {
    pollIntervalMs: 3000,
    initialData: []
  })
  const { byModelId: progressByModel } = useTrainingProgress(sessionId, Boolean(overview?.isRunning))

  const epoch = overview?.epochs?.find(item => item.id === epochId)
  const byLoss = (overview?.selectionCriterion ?? 'ByLoss') === 'ByLoss'

  const rows = useMemo(() => {
    const metric = byLoss
      ? (a, b) => a.lossValue - b.lossValue
      : (a, b) => b.accuracyValue - a.accuracyValue
    return [...models].sort(
      (a, b) => STATE_ORDER[modelState(a)] - STATE_ORDER[modelState(b)] || metric(a, b) || a.name.localeCompare(b.name)
    )
  }, [models, byLoss])

  const bestId = rows.find(model => modelState(model) === 'Trained')?.id

  if (overviewLoading || modelsLoading) {
    return (
      <Card>
        <Skeleton active paragraph={{ rows: 8 }} />
      </Card>
    )
  }

  if (!overview || (!epoch && models.length === 0)) {
    return <NotFoundPage title="Generation not found" subtitle="This generation does not exist or its session was deleted." />
  }

  const counts = rows.reduce((acc, model) => ({ ...acc, [modelState(model)]: (acc[modelState(model)] || 0) + 1 }), {})
  const inProgress = (counts.Training || 0) + (counts.Queued || 0)
  const activeFraction = rows.reduce((sum, model) => sum + fractionOf(progressByModel[model.id]), 0)
  const generationNo = epoch?.epochNo ?? '?'

  return (
    <>
      <PageHeader
        breadcrumbs={[
          { title: 'Sessions', to: '/' },
          { title: overview.name, to: `/session/${sessionId}` },
          { title: `Generation ${generationNo}` }
        ]}
        title={`Generation ${generationNo}`}
        tags={inProgress > 0 ? <Tag color="processing">Running</Tag> : <Tag color="success">Complete</Tag>}
        actions={
          overview.isRunning && (
            <StopSearchButton
              sessionId={sessionId}
              onStopped={async () => {
                await Promise.all([refreshOverview(), refreshModels()])
              }}
            />
          )
        }
        subtitle={`${describeBreeding(rows, epoch?.epochNo)} ${rows.length} candidate models, ranked by ${labelOf(SELECTION_CRITERIA, overview.selectionCriterion).toLowerCase()}. Click a model to see its training curves and architecture.`}
      />

      <Card size="small" className="mb-4">
        <Typography.Text type="secondary" className="text-xs">
          Generation progress
        </Typography.Text>
        <GenerationProgress
          total={rows.length}
          trained={counts.Trained || 0}
          failed={counts.Failed || 0}
          stopped={counts.Stopped || 0}
          active={activeFraction}
        />
      </Card>

      <Row gutter={[16, 16]} className="mb-4">
        <Col xs={12} md={6}>
          <Card size="small">
            <Statistic title="Trained" value={counts.Trained || 0} valueStyle={{ color: '#16a34a' }} />
          </Card>
        </Col>
        <Col xs={12} md={6}>
          <Card size="small">
            <Statistic title="In progress" value={inProgress} valueStyle={{ color: '#4f46e5' }} />
          </Card>
        </Col>
        <Col xs={12} md={6}>
          <Card size="small">
            <Statistic
              title="Failed"
              value={counts.Failed || 0}
              valueStyle={{ color: counts.Failed ? '#dc2626' : undefined }}
              suffix={counts.Stopped ? <span className="text-sm text-slate-500">· {counts.Stopped} stopped</span> : null}
            />
          </Card>
        </Col>
        <Col xs={12} md={6}>
          <Card size="small">
            <Statistic title="Best accuracy" value={formatPercent(epoch?.bestAccuracy)} />
          </Card>
        </Col>
      </Row>

      <Card size="small">
        <Table
          className="clickable-rows"
          rowKey="id"
          dataSource={rows}
          pagination={rows.length > 20 ? { pageSize: 20, showSizeChanger: false } : false}
          expandable={{
            expandRowByClick: true,
            defaultExpandedRowKeys: focusModelId ? [focusModelId] : [],
            expandedRowRender: model => <ModelDetails model={model} />
          }}
          columns={[
            {
              title: 'Model',
              dataIndex: 'name',
              render: (name, model) => (
                <span className="font-medium">
                  {name}
                  {model.id === bestId && (
                    <Tag color="gold" icon={<TrophyFilled />} className="ml-2">
                      Best
                    </Tag>
                  )}
                </span>
              )
            },
            {
              title: 'Created by',
              key: 'origin',
              width: 200,
              render: (_, model) => <OriginCell model={model} epochNo={epoch?.epochNo} />
            },
            {
              title: 'Status',
              key: 'status',
              width: 220,
              render: (_, model) =>
                progressByModel[model.id] && !model.isTrained && !model.fitError ? (
                  <ModelTrainingProgress progress={progressByModel[model.id]} />
                ) : model.fitError ? (
                  <Tooltip title={model.fitError}>
                    <span>
                      <ModelStatusTag model={model} />
                    </span>
                  </Tooltip>
                ) : (
                  <ModelStatusTag model={model} />
                )
            },
            {
              title: 'Accuracy',
              dataIndex: 'accuracyValue',
              width: 220,
              sorter: (a, b) => a.accuracyValue - b.accuracyValue,
              render: (value, model) =>
                model.isTrained ? (
                  <div className="flex items-center gap-2">
                    <Progress percent={Math.round(value * 1000) / 10} size="small" showInfo={false} className="!m-0 w-24" strokeColor="#16a34a" />
                    <span>{formatPercent(value)}</span>
                  </div>
                ) : (
                  '—'
                )
            },
            {
              title: 'Loss',
              dataIndex: 'lossValue',
              width: 120,
              align: 'right',
              sorter: (a, b) => a.lossValue - b.lossValue,
              render: (value, model) => (model.isTrained ? formatLoss(value) : '—')
            },
            {
              title: 'Epochs run',
              key: 'epochs',
              width: 110,
              align: 'right',
              render: (_, model) => parseHistory(model.fitHistory).loss?.length ?? '—'
            },
            {
              title: '',
              key: 'actions',
              width: 140,
              align: 'right',
              render: (_, model) => <DownloadMenu model={model} />
            }
          ]}
        />
      </Card>
    </>
  )
}

export default EpochPage
