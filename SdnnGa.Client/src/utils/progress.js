// Частка завершених епох моделі (0..1) з даних живого прогресу навчання.
export const fractionOf = progress => (progress?.epochs ? Math.min(1, progress.epoch / progress.epochs) : 0)
