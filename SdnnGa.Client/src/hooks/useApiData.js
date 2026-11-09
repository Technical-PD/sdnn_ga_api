import { App } from 'antd'
import { useCallback, useEffect, useRef, useState } from 'react'
import apiCall from '../utils/apiCall'

/**
 * Завантажує `entity` з API (ServiceResult) і, за потреби, періодично оновлює дані.
 * Спінер (`loading`) показується лише при першому завантаженні, щоб сторінка не "блимала" під час опитування.
 * Повертає [data, loading, refresh].
 */
const useApiData = (
  url,
  { pollIntervalMs = 0, initialData = null, errorMessage = 'Failed to load data' } = {}
) => {
  const { message } = App.useApp()
  const [data, setData] = useState(initialData)
  const [loading, setLoading] = useState(Boolean(url))
  const loadedOnce = useRef(false)
  const initialDataRef = useRef(initialData)

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
        isFirstLoad && message.error(result?.message || errorMessage)
      } else {
        setData(result?.entity ?? initialDataRef.current)
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
  }, [url, errorMessage, message])

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
