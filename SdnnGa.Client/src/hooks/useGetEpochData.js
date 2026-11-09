import useApiData from './useApiData'

// Моделі епохи. Опитуємо сервер, щоб результати навчання з'являлися, щойно модель навчена.
const useGetEpochData = epochId =>
  useApiData(epochId ? `NeuralNetworkModel/ByEpoche/${epochId}` : null, {
    pollIntervalMs: 5000,
    errorMessage: 'Failed to fetch epoch data'
  })

export default useGetEpochData
