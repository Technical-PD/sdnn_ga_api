// Стан окремої моделі: навчена / помилка / навчається / в черзі.
export const modelState = model =>
  model?.fitError ? 'Failed' : model?.isTrained ? 'Trained' : model?.modelConfigFileName ? 'Training' : 'Queued'
