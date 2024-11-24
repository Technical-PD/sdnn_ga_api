import { useParams } from 'react-router-dom'
import useGetSessionData from '../hooks/useGetSessionData'
import Loading from './Loading'
import { List, message } from 'antd'
import dayjs from 'dayjs'
import { Column } from '@ant-design/charts'
import { useEffect, useState } from 'react'
import apiCall from '../utils/apiCall'

const EpochList = () => {
  const { sessionId } = useParams()

  const [data, loading] = useGetSessionData(sessionId)
  const [chartEntity, setChartEntity] = useState([])

  useEffect(() => {
    const fetchData = async () => {
      try {
        if (sessionId) {
          const data = await apiCall(
            `Statistic/Models/BySession/${sessionId}/ByAccuracy`
          )

          const { entity } = data

          Array.isArray(entity) &&
            setChartEntity(
              entity
                .sort((a, b) => {
                  return new Date(a.recCreated) - new Date(b.recCreated)
                })
                .map((item, index) => {
                  return {
                    ...item,
                    index
                  }
                })
            )
        }
      } catch (error) {
        message.error('Failed to fetch data')
      }
    }

    fetchData()
  }, [sessionId])

  const configAcc = {
    data: chartEntity || [],
    yField: 'accuracyValue',
    colorField: 'index',
    group: true,
    style: {
      inset: 5
    }
  }

  const configLoss = {
    data: chartEntity || [],
    yField: 'lossValue',
    colorField: 'index',
    group: true,
    style: {
      inset: 5
    }
  }

  if (loading) {
    return <Loading />
  }

  if (Array.isArray(data)) {
    return (
      <div>
        <List
          header={<h1>Epochs</h1>}
          bordered
          dataSource={data || []}
          renderItem={item => (
            <List.Item>
              <a href={`/session/${sessionId}/epoch/${item?.id}`}>
                Epoch {item.name} |{' '}
                {dayjs(item.recCreated).format('DD/MM/YYYY HH:mm:ss')}
              </a>
            </List.Item>
          )}
        />
        <div>
          <h1>Best models by Accuracy in each epoch:</h1>
          <Column {...configAcc} />
        </div>

        <div>
          <h1>Best models by Loss in each epoch:</h1>
          <Column {...configLoss} />
        </div>
      </div>
    )
  } else {
    return <h1>No data</h1>
  }
}

export default EpochList
