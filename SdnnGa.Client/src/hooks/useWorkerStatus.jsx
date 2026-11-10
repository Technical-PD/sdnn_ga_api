import { createContext, useCallback, useContext, useEffect, useState } from 'react'
import PropTypes from 'prop-types'
import apiCall from '../utils/apiCall'

// Під час запуску опитуємо частіше (для плавного індикатора), потім — рідше.
const STARTING_POLL_MS = 700
const READY_POLL_MS = 3000

const WorkerStatusContext = createContext({ status: null, retry: async () => {}, retrying: false })

// Один спільний опитувач стану Python-воркера для екрану запуску, банера, бокової панелі та кнопки запуску.
export const WorkerStatusProvider = ({ children }) => {
  const [status, setStatus] = useState(null)
  const [retrying, setRetrying] = useState(false)

  const refresh = useCallback(async () => {
    try {
      const next = await apiCall('System/Status')
      setStatus(next)
      return next
    } catch {
      setStatus(null)
      return null
    }
  }, [])

  useEffect(() => {
    let cancelled = false
    let timer

    const loop = async () => {
      const current = await refresh()
      if (cancelled) {
        return
      }
      const state = current?.worker?.state
      timer = setTimeout(loop, state === 'Ready' || state === 'Failed' ? READY_POLL_MS : STARTING_POLL_MS)
    }

    loop()
    return () => {
      cancelled = true
      clearTimeout(timer)
    }
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
