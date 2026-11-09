import dayjs from 'dayjs'
import relativeTime from 'dayjs/plugin/relativeTime'

dayjs.extend(relativeTime)

// Сервер зберігає час у UTC, але SQLite повертає його без позначки зони — додаємо 'Z'.
const parseUtc = value => {
  if (!value) {
    return null
  }
  const text = String(value)
  return dayjs(/Z$|[+-]\d\d:\d\d$/.test(text) ? text : `${text}Z`)
}

export const formatPercent = value =>
  value == null || Number.isNaN(Number(value)) ? '—' : `${(Number(value) * 100).toFixed(1)}%`

export const formatLoss = value =>
  value == null || Number.isNaN(Number(value)) ? '—' : Number(value).toFixed(4)

export const formatDateTime = value => parseUtc(value)?.format('DD MMM YYYY, HH:mm') ?? '—'

export const fromNow = value => parseUtc(value)?.fromNow() ?? ''

export const formatNumber = value => (value == null ? '—' : Number(value).toLocaleString())
