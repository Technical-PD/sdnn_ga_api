import useApiData from './useApiData'

// Повертає [sessions, loading, refresh]; refresh викликається після створення сесії.
const useGetSessionList = () => useApiData('Sessions')

export default useGetSessionList
