import { message } from 'antd'
import { useEffect, useState } from 'react'
import apiCall from '../utils/apiCall'

const useGetSessionList = () => {
  const [data, setData] = useState([])
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true)

        const data = await apiCall('Sessions')

        if (data?.isError) {
          message.error(data?.message)
          return
        } else {
          setData(data?.entity || [])
        }
      } catch (error) {
        message.error('Failed to fetch data')
      } finally {
        setLoading(false)
      }
    }

    fetchData()
  }, [])

  return [data, loading]
}

export default useGetSessionList
