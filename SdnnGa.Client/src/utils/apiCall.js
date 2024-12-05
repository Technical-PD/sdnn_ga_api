const BASE_URL = 'http://localhost:8080/api/'

const apiCall = async (url, method = 'GET', body, isFiles, returnBlob) => {
  const headers = new Headers()

  headers.append('Access-Control-Allow-Origin', '*')
  headers.append('ngrok-skip-browser-warning', true)
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
  let data
  if (returnBlob) {
    data = await response.blob()
  } else {
    data = await response.json()
  }
  return data
}

export default apiCall
