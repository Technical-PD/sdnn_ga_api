// Службові значення fitError, які записує бекенд (ModelStatusMessages).
export const STOPPED_BY_USER = 'Stopped by user.'

export const isStoppedError = fitError => fitError === STOPPED_BY_USER || Boolean(fitError?.startsWith('Interrupted'))

// Стан окремої моделі: навчена / помилка / зупинена / чекає на навчання / в черзі на створення.
export const modelState = model => {
  if (model?.fitError) {
    return isStoppedError(model.fitError) ? 'Stopped' : 'Failed'
  }
  if (model?.isTrained) {
    return 'Trained'
  }
  return model?.modelConfigFileName ? 'Training' : 'Queued'
}
