// Швидкий аналіз CSV (з рядком заголовків), щоб показати користувачу розмір даних
// і автоматично заповнити кількість входів/виходів мережі.
export const inspectCsv = text => {
  const lines = (text || '').split(/\r?\n/).filter(line => line.trim() !== '')
  if (lines.length === 0) {
    return { rows: 0, columns: 0, headers: [], preview: [], isBinary: false, isOneHot: false, isNumeric: false }
  }

  const split = line => line.split(',').map(cell => cell.trim())
  const headers = split(lines[0])
  const rows = lines.slice(1).map(split)

  let isNumeric = true
  let isBinary = true
  let isOneHot = true

  for (const row of rows) {
    let sum = 0
    for (const cell of row) {
      const value = Number(cell)
      if (cell === '' || Number.isNaN(value)) {
        isNumeric = false
        isBinary = false
        isOneHot = false
        break
      }
      if (value !== 0 && value !== 1) {
        isBinary = false
      }
      sum += value
    }
    if (!isNumeric) {
      break
    }
    if (sum !== 1) {
      isOneHot = false
    }
  }

  return {
    rows: rows.length,
    columns: headers.length,
    headers,
    preview: rows.slice(0, 5),
    isNumeric,
    isBinary,
    isOneHot: isBinary && isOneHot && headers.length > 1
  }
}

// Підказка функції втрат за виглядом цільових даних (Y).
export const suggestLossFunction = yInfo => {
  if (!yInfo || !yInfo.isNumeric) {
    return null
  }
  if (yInfo.isOneHot) {
    return 'categorical_crossentropy'
  }
  if (yInfo.isBinary) {
    return 'binary_crossentropy'
  }
  return 'mean_squared_error'
}
