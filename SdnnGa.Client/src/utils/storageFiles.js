import apiCall from './apiCall'

// Файли (дані, конфіги, ваги) лежать у локальному сховищі застосунку; API віддає їх за ключем.
const fetchStorageBlob = path =>
  apiCall('Data/GetDataFile', 'POST', { dataStoragePath: path }, false, true)

export const fetchStorageText = async path => (await fetchStorageBlob(path)).text()

export const downloadStorageFile = async (path, fileName) => {
  const blob = await fetchStorageBlob(path)
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = fileName || path.split('/').pop()
  document.body.appendChild(link)
  link.click()
  link.remove()
  URL.revokeObjectURL(url)
}
