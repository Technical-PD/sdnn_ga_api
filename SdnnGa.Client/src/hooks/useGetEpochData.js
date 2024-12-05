import { message } from 'antd'
import { useEffect, useState } from 'react'
import apiCall from '../utils/apiCall'

const useGetEpochData = epochId => {
  const [data, setData] = useState([])
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true)

        const data = await apiCall(`NeuralNetworkModel/ByEpoche/${epochId}`)

        if (data?.isError) {
          message.error(data?.message)
          return
        } else {
          setData(data?.entity || [])
        }
      } catch (error) {
        message.error('Failed to fetch session data')
      } finally {
        setLoading(false)
      }
    }

    fetchData()
  }, [epochId])

  return [data, loading]
}

export default useGetEpochData
