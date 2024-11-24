import { useParams } from 'react-router-dom'
import useGetEpochData from '../hooks/useGetEpochData'
import Loading from './Loading'
import { Button, List, message, Select } from 'antd'
import { Line } from '@ant-design/charts'
//import { Line } from '@ant-design/plots';
import { useState } from 'react'
import apiCall from '../utils/apiCall'

const BASE_CONFIG = {
  point: {
    shapeField: 'square',
    sizeField: 4
  },
  interaction: {
    tooltip: {
      marker: false
    }
  },
  style: {
    lineWidth: 2
  }
}

const SORT_FIELDS = [
  {
    value: 'lossValue',
    label: 'Loss'
  },
  {
    value: 'accuracyValue',
    label: 'Accuracy'
  }
]

const DIRECTION_BY_SORT_FIELD = {
  lossValue: 'asc',
  accuracyValue: 'desc'
}

const ModelList = () => {
  const { epochId } = useParams()
  const [sortField, setSortField] = useState('lossValue')

  const [data, loading] = useGetEpochData(epochId)

  if (loading) {
    return <Loading />
  }

  return Array.isArray(data) ? (
    <List
      header={
        <div className="flex w-full justify-between items-center">
          <h1>Models</h1>
          <Select
            className="w-[200px]"
            value={sortField}
            onChange={setSortField}
            options={SORT_FIELDS}
          />
        </div>
      }
      bordered
      dataSource={
        data
          .sort((a, b) => {
            const direction = DIRECTION_BY_SORT_FIELD[sortField] || 'asc'

            if (direction === 'asc') {
              return a[sortField] - b[sortField]
            } else {
              return b[sortField] - a[sortField]
            }
          })
          .filter(item => item.isTrained) || []
      }
      renderItem={item => {
        const { fitHistory } = item
        const fitHistoryData = JSON.parse(fitHistory)

        const { accuracy = [], loss = [], val_accuracy = [], val_loss = [] } = fitHistoryData || {}

        const accuracyConfig = {
          ...BASE_CONFIG,
          data: 
          {
            value: accuracy.flatMap((accuracy_item, index) => [
              {
                number: index + 1,
                value: accuracy_item,
                category: 'accuracy',
              },
              {
                number: index + 1,
                value: val_accuracy[index],
                category: 'val_accuracy',
              }
            ])
          },
          xField: (d) => d.number,
          yField: 'value', 
          colorField: 'category'
        }

        const lossConfig = {
          ...BASE_CONFIG,
          data: 
          {
            value: loss.flatMap((loss_item, index) => [
              {
                number: index + 1,
                value: loss_item,
                category: 'loss',
              },
              {
                number: index + 1,
                value: val_loss[index],
                category: 'val_loss',
              }
            ])
          },
          xField: (d) => d.number,
          yField: 'value', 
          colorField: 'category'
        }

        console.log(lossConfig)

        const downloadConfig = async () => {
          try {
            const response = await apiCall('Data/GetDataFile', 'POST', {
              dataStoragePath: item?.modelConfigFileName
            })
            if (response) {
              const jsonFile = new Blob([JSON.stringify(response)], {
                type: 'application/json'
              })
              const url = URL.createObjectURL(jsonFile)
              const fileName = item?.modelConfigFileName.split('/').pop()
              const link = document.createElement('a')
              link.href = url
              link.download = fileName
              link.click()
              URL.revokeObjectURL(url)
            }
          } catch (error) {
            message.error('Failed to download model config')
          }
        }
        const downloadConfigDotNet = async () => {
          try {
            const response = await apiCall('Data/GetDataFile', 'POST', {
              dataStoragePath: item?.modelConfigDotNetFileName
            })
            if (response) {
              const jsonFile = new Blob([JSON.stringify(response)], {
                type: 'application/json'
              })
              const url = URL.createObjectURL(jsonFile)
              const fileName = item?.modelConfigDotNetFileName.split('/').pop()
              const link = document.createElement('a')
              link.href = url
              link.download = fileName
              link.click()
              URL.revokeObjectURL(url)
            }
          } catch (error) {
            message.error('Failed to download model config')
          }
        }
        const downloadWeights = async () => {
          try {
            const response = await apiCall(
              'Data/GetDataFile',
              'POST',
              {
                dataStoragePath: item?.weightsFileName
              },
              false,
              true
            )

            if (response) {
              const url = URL.createObjectURL(response)
              const fileName = item?.weightsFileName.split('/').pop()
              const link = document.createElement('a')
              link.href = url
              link.download = fileName
              link.click()
              URL.revokeObjectURL(url)
            }
          } catch (error) {
            message.error('Failed to download model weights')
          }
        }

        return (
          <List.Item>
            <div
              style={{
                display: 'flex',
                justifyContent: 'space-between',
                gap: '240px',
                width: '100%'
              }}>
              <div>
                <h3>{item?.name}</h3>
                <div>Is trained: {item?.isTrained?.toString()}</div>
                <div>Loss {item?.lossValue}</div>
                <div>Accuracy {item?.accuracyValue}</div>
                <div>
                  {item?.modelConfigFileName && (
                    <Button onClick={downloadConfig} className="mt-4">
                      Load config
                    </Button>
                  )}
                  {item?.modelConfigDotNetFileName && (
                    <Button onClick={downloadConfigDotNet} className="mt-4">
                      Load config .net
                    </Button>
                  )}
                  {item?.weightsFileName && (
                    <Button onClick={downloadWeights} className="mt-4">
                      Load weights
                    </Button>
                  )}
                </div>
              </div>
              <div className="flex justify-between w-full">
                <div className="w-[50%]">
                  <h3>Accuracy</h3>
                  <Line {...accuracyConfig} />
                </div>
                <div className="w-[50%]">
                  <h3>Loss</h3>
                  <Line {...lossConfig} />
                </div>
              </div>
            </div>
          </List.Item>
        )
      }}
    />
  ) : (
    <div>
      <h1>No data</h1>
    </div>
  )
}

export default ModelList
