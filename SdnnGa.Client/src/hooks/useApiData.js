import { message } from 'antd'
import { useCallback, useEffect, useRef, useState } from 'react'
import apiCall from '../utils/apiCall'

/**
 * Завантажує `entity` з API (ServiceResult) і, за потреби, періодично оновлює дані.
 * Спінер (`loading`) показується лише при першому завантаженні, щоб список не "блимав" під час опитування.
 * Повертає [data, loading, refresh].
 */
const useApiData = (url, { pollIntervalMs = 0, errorMessage = 'Failed to fetch data' } = {}) => {
  const [data, setData] = useState([])
  const [loading, setLoading] = useState(false)
  const loadedOnce = useRef(false)

  const refresh = useCallback(async () => {
    if (!url) {
      return
    }

    const isFirstLoad = !loadedOnce.current
    try {
      if (isFirstLoad) {
        setLoading(true)
      }

      const result = await apiCall(url)

      if (result?.isError) {
        isFirstLoad && message.error(result?.message)
      } else {
        setData(result?.entity || [])
      }
    } catch {
      // Під час фонового опитування не засипаємо користувача однаковими помилками.
      isFirstLoad && message.error(errorMessage)
    } finally {
      loadedOnce.current = true
      if (isFirstLoad) {
        setLoading(false)
      }
    }
  }, [url, errorMessage])

  useEffect(() => {
    loadedOnce.current = false
    refresh()

    if (!pollIntervalMs) {
      return undefined
    }

    const timer = setInterval(refresh, pollIntervalMs)
    return () => clearInterval(timer)
  }, [refresh, pollIntervalMs])

  return [data, loading, refresh]
}

export default useApiData
