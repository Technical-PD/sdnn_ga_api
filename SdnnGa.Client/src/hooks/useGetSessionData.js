import useApiData from './useApiData'

// Епохи сесії. Опитуємо сервер, щоб нові епохи з'являлися, поки генетичний алгоритм працює.
const useGetSessionData = sessionId =>
  useApiData(sessionId ? `Epoches/BySession/${sessionId}` : null, {
    pollIntervalMs: 5000,
    errorMessage: 'Failed to fetch session data'
  })

export default useGetSessionData
