import { Badge, Card, Skeleton, Tabs } from 'antd'
import { LineChartOutlined, SettingOutlined } from '@ant-design/icons'
import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import PageHeader from '../components/PageHeader'
import { SessionStatusTag } from '../components/StatusTag'
import DeleteSessionButton from '../components/DeleteSessionButton'
import StopSearchButton from '../components/StopSearchButton'
import useApiData from '../hooks/useApiData'
import NotFoundPage from './NotFoundPage'
import SetupTab from './session/SetupTab'
import ResultsTab from './session/ResultsTab'

const SessionPage = () => {
  const { sessionId } = useParams()
  const navigate = useNavigate()
  const [overview, loading, refresh] = useApiData(`Sessions/${sessionId}/Overview`, {
    pollIntervalMs: 4000,
    errorMessage: 'Could not load the session'
  })
  const [tab, setTab] = useState(null)

  // Відкриваємо результати, якщо пошук уже запускався, інакше — налаштування.
  useEffect(() => {
    if (overview && tab === null) {
      setTab(overview.epochCount > 0 || overview.isRunning ? 'results' : 'setup')
    }
  }, [overview, tab])

  if (loading) {
    return (
      <Card>
        <Skeleton active paragraph={{ rows: 6 }} />
      </Card>
    )
  }

  if (!overview) {
    return <NotFoundPage title="Session not found" subtitle="This session does not exist or was deleted." />
  }

  return (
    <>
      <PageHeader
        breadcrumbs={[{ title: 'Sessions', to: '/' }, { title: overview.name }]}
        title={overview.name}
        tags={<SessionStatusTag status={overview.status} />}
        subtitle={overview.description}
        actions={
          <>
            {overview.isRunning && <StopSearchButton sessionId={overview.id} onStopped={refresh} />}
            <DeleteSessionButton session={overview} onDeleted={() => navigate('/')} />
          </>
        }
      />

      <Tabs
        activeKey={tab ?? 'setup'}
        onChange={setTab}
        destroyInactiveTabPane
        items={[
          {
            key: 'setup',
            label: (
              <span>
                <SettingOutlined /> Setup
              </span>
            ),
            children: (
              <SetupTab
                overview={overview}
                onChanged={refresh}
                onStarted={async () => {
                  await refresh()
                  setTab('results')
                }}
              />
            )
          },
          {
            key: 'results',
            label: (
              <span>
                <LineChartOutlined /> Results{' '}
                {overview.epochCount > 0 && (
                  <Badge count={overview.epochCount} color={overview.isRunning ? '#4f46e5' : '#94a3b8'} size="small" />
                )}
              </span>
            ),
            children: <ResultsTab overview={overview} onGoToSetup={() => setTab('setup')} onStopped={refresh} />
          }
        ]}
      />
    </>
  )
}

export default SessionPage
