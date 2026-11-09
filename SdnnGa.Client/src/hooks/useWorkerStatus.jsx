import { createContext, useCallback, useContext, useEffect, useState } from 'react'
import PropTypes from 'prop-types'
import apiCall from '../utils/apiCall'

const POLL_INTERVAL_MS = 3000

const WorkerStatusContext = createContext({ status: null, retry: async () => {}, retrying: false })

// Один спільний опитувач стану Python-воркера для банера, бокової панелі та кнопки запуску.
export const WorkerStatusProvider = ({ children }) => {
  const [status, setStatus] = useState(null)
  const [retrying, setRetrying] = useState(false)

  const refresh = useCallback(async () => {
    try {
      setStatus(await apiCall('System/Status'))
    } catch {
      setStatus(null)
    }
  }, [])

  useEffect(() => {
    refresh()
    const timer = setInterval(refresh, POLL_INTERVAL_MS)
    return () => clearInterval(timer)
  }, [refresh])

  const retry = useCallback(async () => {
    setRetrying(true)
    try {
      setStatus(await apiCall('System/Worker/Start', 'POST'))
    } finally {
      setRetrying(false)
    }
  }, [])

  return (
    <WorkerStatusContext.Provider value={{ status, retry, retrying }}>
      {children}
    </WorkerStatusContext.Provider>
  )
}

WorkerStatusProvider.propTypes = {
  children: PropTypes.node
}

// eslint-disable-next-line react-refresh/only-export-components
export const useWorkerStatus = () => useContext(WorkerStatusContext)
