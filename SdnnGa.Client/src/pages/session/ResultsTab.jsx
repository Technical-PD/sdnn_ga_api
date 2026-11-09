import { Alert, Button, Card, Col, Empty, Row, Space, Statistic, Table, Tag, Typography } from 'antd'
import { ArrowRightOutlined, SyncOutlined, TrophyFilled } from '@ant-design/icons'
import { useNavigate } from 'react-router-dom'
import PropTypes from 'prop-types'
import MetricsChart from '../../components/MetricsChart'
import { labelOf, SELECTION_CRITERIA } from '../../constants/options'
import { formatDateTime, formatLoss, formatPercent } from '../../utils/format'

const ModelCounts = ({ epoch }) => (
  <Space size={4} wrap>
    {epoch.trainedModelCount > 0 && <Tag color="success">{epoch.trainedModelCount} trained</Tag>}
    {epoch.failedModelCount > 0 && <Tag color="error">{epoch.failedModelCount} failed</Tag>}
    {epoch.pendingModelCount > 0 && (
      <Tag color="processing" icon={<SyncOutlined spin />}>
        {epoch.pendingModelCount} in progress
      </Tag>
    )}
  </Space>
)

ModelCounts.propTypes = { epoch: PropTypes.object.isRequired }

// Результати сесії: найкраща модель, динаміка по поколіннях, список поколінь.
const ResultsTab = ({ overview, onGoToSetup }) => {
  const navigate = useNavigate()
  const epochs = overview.epochs || []

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
        <Alert
          type="info"
          showIcon
          icon={<SyncOutlined spin />}
          message={`Search in progress: generation ${overview.epochCount}${overview.maxEpoches ? ` of ${overview.maxEpoches}` : ''}, ${overview.pendingModelCount} model(s) waiting or training. This page updates automatically.`}
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
            { title: 'Models', key: 'models', render: (_, epoch) => <ModelCounts epoch={epoch} /> },
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
  onGoToSetup: PropTypes.func.isRequired
}

export default ResultsTab
