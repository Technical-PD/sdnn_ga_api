import { Alert, Button, Card, Col, Empty, Row, Space, Statistic, Table, Tag, Typography } from 'antd'
import { ArrowRightOutlined, SyncOutlined, TrophyFilled } from '@ant-design/icons'
import { useNavigate } from 'react-router-dom'
import PropTypes from 'prop-types'
import MetricsChart from '../../components/MetricsChart'
import StopSearchButton from '../../components/StopSearchButton'
import { GenerationProgress, ModelTrainingProgress } from '../../components/TrainingProgress'
import { fractionOf } from '../../utils/progress'
import useTrainingProgress from '../../hooks/useTrainingProgress'
import { labelOf, SELECTION_CRITERIA } from '../../constants/options'
import { formatDateTime, formatLoss, formatPercent } from '../../utils/format'

// Частка поточного навчання моделей покоління (для плавного руху смуги).
const activeFraction = (progressList, epochId) =>
  progressList.filter(item => item.epochId === epochId).reduce((sum, item) => sum + fractionOf(item), 0)

// Результати сесії: найкраща модель, динаміка по поколіннях, список поколінь.
const ResultsTab = ({ overview, onGoToSetup, onStopped }) => {
  const navigate = useNavigate()
  const epochs = overview.epochs || []
  const { list: progressList } = useTrainingProgress(overview.id, overview.isRunning)

  if (epochs.length === 0) {
    return (
      <Card>
        <Empty
          className="py-10"
          description={
            overview.isRunning ? (
              'The search is starting. The first generation will appear here shortly.'
            ) : (
              <span>No results yet. Finish the setup and start the search.</span>
            )
          }>
          {!overview.isRunning && (
            <Button type="primary" onClick={onGoToSetup}>
              Go to setup
            </Button>
          )}
        </Empty>
      </Card>
    )
  }

  const openEpoch = (epochId, modelId) =>
    navigate(`/session/${overview.id}/epoch/${epochId}${modelId ? `?model=${modelId}` : ''}`)
  const criterion = labelOf(SELECTION_CRITERIA, overview.selectionCriterion).toLowerCase()

  return (
    <div className="flex flex-col gap-4">
      {overview.isRunning && (
        <Card>
          <div className="flex flex-wrap items-start justify-between gap-4">
            <div className="flex-1 min-w-[280px]">
              <div className="flex items-center gap-2 font-medium">
                <SyncOutlined spin className="text-indigo-500" />
                Search in progress: generation {overview.epochCount - 1}
                {overview.maxEpoches ? ` (${overview.epochCount} of ${overview.maxEpoches})` : ''}
              </div>
              <Typography.Text type="secondary" className="text-sm">
                {overview.pendingModelCount} model(s) waiting or training. This page updates automatically.
              </Typography.Text>
              {overview.maxEpoches > 0 && (
                <div className="mt-3">
                  <Typography.Text type="secondary" className="text-xs">
                    Overall
                  </Typography.Text>
                  <GenerationProgress
                    total={overview.maxEpoches * Math.max(1, epochs[0]?.modelCount ?? 1)}
                    trained={overview.trainedModelCount}
                    failed={overview.failedModelCount}
                    stopped={overview.stoppedModelCount}
                    active={progressList.reduce((sum, item) => sum + fractionOf(item), 0)}
                  />
                </div>
              )}
            </div>
            <StopSearchButton sessionId={overview.id} onStopped={onStopped} />
          </div>

          {progressList.length > 0 && (
            <div className="mt-4 pt-4 border-t border-slate-200">
              <Typography.Text type="secondary" className="text-xs">
                Training now
              </Typography.Text>
              {progressList.map(item => (
                <div key={item.modelId} className="flex flex-wrap items-center gap-4 mt-2">
                  <span className="font-medium w-28">{item.modelName}</span>
                  <div className="flex-1 min-w-[240px]">
                    <ModelTrainingProgress progress={item} />
                  </div>
                </div>
              ))}
            </div>
          )}
        </Card>
      )}

      {!overview.isRunning && overview.status === 'Stopped' && (
        <Alert
          type="warning"
          showIcon
          message="The search was stopped. Trained models are kept; continue the search from the Setup tab."
          action={
            <Button size="small" onClick={onGoToSetup}>
              Go to setup
            </Button>
          }
        />
      )}

      <Row gutter={[16, 16]}>
        <Col xs={24} md={12} xl={6}>
          <Card className="h-full">
            <Statistic title="Best model accuracy" value={formatPercent(overview.bestModelAccuracy)} valueStyle={{ color: '#16a34a' }} />
          </Card>
        </Col>
        <Col xs={24} md={12} xl={6}>
          <Card className="h-full">
            <Statistic title="Best model loss" value={formatLoss(overview.bestModelLoss)} valueStyle={{ color: '#4f46e5' }} />
          </Card>
        </Col>
        <Col xs={24} md={12} xl={6}>
          <Card className="h-full">
            <Statistic
              title="Generations"
              value={overview.epochCount}
              suffix={overview.maxEpoches ? <span className="text-base text-slate-400">/ {overview.maxEpoches}</span> : null}
            />
          </Card>
        </Col>
        <Col xs={24} md={12} xl={6}>
          <Card className="h-full">
            <Statistic
              title="Models trained"
              value={overview.trainedModelCount}
              suffix={
                overview.failedModelCount > 0 ? (
                  <span className="text-base text-rose-500">· {overview.failedModelCount} failed</span>
                ) : null
              }
            />
          </Card>
        </Col>
      </Row>

      {overview.bestModelId && (
        <Card>
          <div className="flex flex-wrap items-center justify-between gap-3">
            <div className="flex items-center gap-3">
              <div className="flex items-center justify-center w-10 h-10 rounded-full bg-amber-50 text-amber-500 text-xl">
                <TrophyFilled />
              </div>
              <div>
                <div className="font-semibold">Best model: {overview.bestModelName}</div>
                <Typography.Text type="secondary" className="text-sm">
                  Selected by {criterion} · accuracy {formatPercent(overview.bestModelAccuracy)} · loss {formatLoss(overview.bestModelLoss)}
                </Typography.Text>
              </div>
            </div>
            <Button onClick={() => openEpoch(overview.bestModelEpochId, overview.bestModelId)} icon={<ArrowRightOutlined />} iconPosition="end">
              View model & download
            </Button>
          </div>
        </Card>
      )}

      <Row gutter={[16, 16]}>
        <Col xs={24} xl={12}>
          <Card title="Best accuracy per generation" size="small">
            <MetricsChart
              series={[{ name: 'Best accuracy', values: epochs.map(epoch => epoch.bestAccuracy) }]}
              xLabel="Generation"
              startAt={0}
              yFormatter={formatPercent}
              colors={['#16a34a']}
            />
          </Card>
        </Col>
        <Col xs={24} xl={12}>
          <Card title="Best loss per generation" size="small">
            <MetricsChart
              series={[{ name: 'Best loss', values: epochs.map(epoch => epoch.bestLoss) }]}
              xLabel="Generation"
              startAt={0}
              yFormatter={value => formatLoss(value)}
              colors={['#4f46e5']}
            />
          </Card>
        </Col>
      </Row>

      <Card title="Generations" size="small">
        <Table
          className="clickable-rows"
          rowKey="id"
          size="middle"
          dataSource={[...epochs].reverse()}
          pagination={epochs.length > 10 ? { pageSize: 10, showSizeChanger: false } : false}
          onRow={epoch => ({ onClick: () => openEpoch(epoch.id) })}
          columns={[
            {
              title: 'Generation',
              dataIndex: 'epochNo',
              width: 120,
              render: (value, epoch) => (
                <span className="font-medium">
                  {value}
                  {epoch.id === overview.bestModelEpochId && <TrophyFilled className="text-amber-500 ml-2" />}
                </span>
              )
            },
            { title: 'Started', dataIndex: 'recCreated', render: formatDateTime },
            {
              title: 'Progress',
              key: 'progress',
              width: 260,
              render: (_, epoch) => (
                <GenerationProgress
                  compact
                  total={epoch.modelCount}
                  trained={epoch.trainedModelCount}
                  failed={epoch.failedModelCount}
                  stopped={epoch.stoppedModelCount}
                  active={activeFraction(progressList, epoch.id)}
                />
              )
            },
            { title: 'Best accuracy', dataIndex: 'bestAccuracy', align: 'right', render: formatPercent },
            { title: 'Best loss', dataIndex: 'bestLoss', align: 'right', render: value => formatLoss(value) },
            {
              key: 'open',
              width: 48,
              render: () => <ArrowRightOutlined className="text-slate-400" />
            }
          ]}
        />
      </Card>
    </div>
  )
}

ResultsTab.propTypes = {
  overview: PropTypes.object.isRequired,
  onGoToSetup: PropTypes.func.isRequired,
  onStopped: PropTypes.func
}

export default ResultsTab
