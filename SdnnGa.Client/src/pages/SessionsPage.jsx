import { Button, Card, Col, Empty, Input, Progress, Row, Skeleton, Tooltip, Typography } from 'antd'
import {
  CheckCircleFilled,
  CloudUploadOutlined,
  ExperimentOutlined,
  LineChartOutlined,
  MinusCircleOutlined,
  PlusOutlined,
  SettingOutlined,
  TrophyOutlined
} from '@ant-design/icons'
import { useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import PropTypes from 'prop-types'
import PageHeader from '../components/PageHeader'
import { SessionStatusTag } from '../components/StatusTag'
import CreateSessionModal from '../components/CreateSessionModal'
import DeleteSessionButton from '../components/DeleteSessionButton'
import useApiData from '../hooks/useApiData'
import { formatDateTime, formatLoss, formatPercent, fromNow } from '../utils/format'

const HOW_IT_WORKS = [
  { icon: <CloudUploadOutlined />, title: 'Upload a dataset', text: 'Two CSV files: features (X) and targets (Y).' },
  { icon: <SettingOutlined />, title: 'Configure training', text: 'Loss function, optimizer and number of epochs.' },
  { icon: <ExperimentOutlined />, title: 'Tune the evolution', text: 'Generations, population size and mutation rates.' },
  { icon: <LineChartOutlined />, title: 'Run & compare', text: 'Watch generations improve and download the best model.' }
]

const SetupCheck = ({ done, label }) => (
  <span className={`inline-flex items-center gap-1 text-xs ${done ? 'text-emerald-600' : 'text-slate-400'}`}>
    {done ? <CheckCircleFilled /> : <MinusCircleOutlined />}
    {label}
  </span>
)

SetupCheck.propTypes = { done: PropTypes.bool, label: PropTypes.string }

const SessionCard = ({ session, onOpen, onDeleted }) => {
  const hasResults = session.epochCount > 0
  const progress = session.maxEpoches ? Math.min(100, Math.round((session.epochCount / session.maxEpoches) * 100)) : 0

  return (
    <Card hoverable className="session-card h-full" onClick={onOpen} styles={{ body: { height: '100%' } }}>
      <div className="flex flex-col h-full gap-3">
        <div className="flex items-start justify-between gap-2">
          <Typography.Title level={5} className="!m-0" ellipsis={{ tooltip: session.name }}>
            {session.name}
          </Typography.Title>
          <SessionStatusTag status={session.status} />
        </div>

        <Typography.Paragraph type="secondary" className="!mb-0 text-sm" ellipsis={{ rows: 2 }}>
          {session.description || <span className="italic">No description</span>}
        </Typography.Paragraph>

        {hasResults ? (
          <div>
            <div className="flex justify-between text-xs text-slate-500">
              <span>
                Generation {session.epochCount}
                {session.maxEpoches ? ` of ${session.maxEpoches}` : ''}
              </span>
              <span>
                {session.trainedModelCount} trained
                {session.failedModelCount > 0 ? ` · ${session.failedModelCount} failed` : ''}
              </span>
            </div>
            <Progress
              percent={progress}
              showInfo={false}
              size="small"
              status={session.isRunning ? 'active' : 'normal'}
            />
            {session.bestModelId && (
              <div className="flex items-center gap-4 mt-1 text-sm">
                <TrophyOutlined className="text-amber-500" />
                <span>
                  Accuracy <b>{formatPercent(session.bestModelAccuracy)}</b>
                </span>
                <span>
                  Loss <b>{formatLoss(session.bestModelLoss)}</b>
                </span>
              </div>
            )}
          </div>
        ) : (
          <div className="flex flex-wrap gap-x-4 gap-y-1">
            <SetupCheck done={session.hasData} label="Dataset" />
            <SetupCheck done={session.hasFitConfig} label="Training" />
            <SetupCheck done={session.hasGeneticConfig} label="Genetic algorithm" />
          </div>
        )}

        <div className="flex items-center justify-between mt-auto pt-2 border-t border-slate-100">
          <Tooltip title={formatDateTime(session.recCreated)}>
            <Typography.Text type="secondary" className="text-xs">
              Created {fromNow(session.recCreated)}
            </Typography.Text>
          </Tooltip>
          {/* Події з порталу Popconfirm "спливають" через React-дерево — не даємо їм відкрити картку. */}
          <span onClick={event => event.stopPropagation()}>
            <DeleteSessionButton session={session} onDeleted={onDeleted} iconOnly />
          </span>
        </div>
      </div>
    </Card>
  )
}

SessionCard.propTypes = {
  session: PropTypes.object.isRequired,
  onOpen: PropTypes.func,
  onDeleted: PropTypes.func
}

const SessionsPage = () => {
  const navigate = useNavigate()
  const [sessions, loading, refresh] = useApiData('Sessions/Overview', {
    pollIntervalMs: 5000,
    initialData: [],
    errorMessage: 'Could not load sessions'
  })
  const [isCreateOpen, setIsCreateOpen] = useState(false)
  const [search, setSearch] = useState('')

  const filtered = useMemo(() => {
    const query = search.trim().toLowerCase()
    return query
      ? sessions.filter(
          session =>
            session.name?.toLowerCase().includes(query) || session.description?.toLowerCase().includes(query)
        )
      : sessions
  }, [sessions, search])

  const createButton = (
    <Button type="primary" icon={<PlusOutlined />} onClick={() => setIsCreateOpen(true)}>
      New session
    </Button>
  )

  return (
    <>
      <PageHeader
        title="Sessions"
        subtitle="Each session evolves neural network architectures for one dataset and keeps the best models it finds."
        actions={
          sessions.length > 0 && (
            <>
              <Input.Search
                allowClear
                placeholder="Search sessions"
                value={search}
                onChange={event => setSearch(event.target.value)}
                style={{ width: 240 }}
              />
              {createButton}
            </>
          )
        }
      />

      {loading ? (
        <Row gutter={[16, 16]}>
          {[0, 1, 2].map(key => (
            <Col key={key} xs={24} md={12} xl={8}>
              <Card>
                <Skeleton active />
              </Card>
            </Col>
          ))}
        </Row>
      ) : sessions.length === 0 ? (
        <Card>
          <div className="py-8 text-center">
            <Empty
              image={Empty.PRESENTED_IMAGE_SIMPLE}
              description={<span className="text-base">No sessions yet</span>}>
              {createButton}
            </Empty>
          </div>
          <Typography.Title level={5} className="!mt-2">
            How it works
          </Typography.Title>
          <Row gutter={[16, 16]}>
            {HOW_IT_WORKS.map((step, index) => (
              <Col key={step.title} xs={24} sm={12} xl={6}>
                <div className="h-full rounded-lg border border-slate-200 bg-slate-50 p-4">
                  <div className="flex items-center gap-2 text-indigo-600 text-lg">
                    {step.icon}
                    <span className="text-xs font-semibold text-slate-400">STEP {index + 1}</span>
                  </div>
                  <div className="font-medium mt-2">{step.title}</div>
                  <div className="text-sm text-slate-500">{step.text}</div>
                </div>
              </Col>
            ))}
          </Row>
        </Card>
      ) : filtered.length === 0 ? (
        <Card>
          <Empty description={`No sessions match "${search}"`} />
        </Card>
      ) : (
        <Row gutter={[16, 16]}>
          {filtered.map(session => (
            <Col key={session.id} xs={24} md={12} xl={8}>
              <SessionCard
                session={session}
                onOpen={() => navigate(`/session/${session.id}`)}
                onDeleted={refresh}
              />
            </Col>
          ))}
        </Row>
      )}

      <CreateSessionModal open={isCreateOpen} onClose={() => setIsCreateOpen(false)} />
    </>
  )
}

export default SessionsPage
