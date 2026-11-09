// UI обслуговується тим самим локальним сервером, що й API (десктопний застосунок),
// а в режимі розробки Vite проксіює /api на http://127.0.0.1:5080 (див. vite.config.js).
const BASE_URL = '/api/'

const apiCall = async (url, method = 'GET', body, isFiles, returnBlob) => {
  const headers = new Headers()

  !isFiles && headers.append('Content-Type', 'application/json')

  const reqParams = {
    method,
    headers
  }

  if (body && !isFiles) {
    reqParams.body = JSON.stringify(body)
  }
  if (body && isFiles) {
    reqParams.body = body
  }

  const response = await fetch(`${BASE_URL}${url}`, reqParams)

  if (!response.ok) {
    const text = await response.text()
    throw new Error(text || `Request failed with status ${response.status}`)
  }

  let data
  if (returnBlob) {
    data = await response.blob()
  } else {
    data = await response.json()
  }
  return data
}

export default apiCall
