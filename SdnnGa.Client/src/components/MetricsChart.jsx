import { Line } from '@ant-design/charts'
import { Empty } from 'antd'
import PropTypes from 'prop-types'
import { useThemeMode } from '../hooks/useThemeMode'

/**
 * Лінійний графік кількох серій.
 * series: [{ name, values: number[] }]; по осі X — номер точки (з `startAt`).
 */
const MetricsChart = ({ series, xLabel, yFormatter, height = 220, startAt = 1, colors }) => {
  const { isDark } = useThemeMode()
  const data = series.flatMap(({ name, values }) =>
    (values || [])
      .map((value, index) => ({ x: index + startAt, value, series: name }))
      .filter(point => point.value != null && !Number.isNaN(point.value))
  )

  if (data.length === 0) {
    return (
      <div style={{ height }} className="flex items-center justify-center">
        <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="No data yet" />
      </div>
    )
  }

  return (
    <Line
      data={data}
      xField="x"
      yField="value"
      colorField="series"
      height={height}
      autoFit
      // Темна тема G2 з прозорим тлом, щоб графік зливався з карткою.
      theme={{ type: isDark ? 'classicDark' : 'classic', view: { viewFill: 'transparent' } }}
      scale={{ color: colors ? { range: colors } : undefined }}
      axis={{
        // Номери епох/поколінь — цілі числа, дробові поділки ховаємо.
        x: { title: xLabel, tickCount: 8, tickFilter: value => Number.isInteger(Number(value)) },
        y: { title: false, labelFormatter: yFormatter }
      }}
      legend={{ color: { position: 'top', layout: { justifyContent: 'flex-end' } } }}
      tooltip={{ items: [{ channel: 'y', valueFormatter: yFormatter }] }}
      point={data.length <= 40 ? { shapeField: 'circle', sizeField: 2.5 } : undefined}
      style={{ lineWidth: 2 }}
    />
  )
}

MetricsChart.propTypes = {
  series: PropTypes.arrayOf(PropTypes.shape({ name: PropTypes.string, values: PropTypes.array })).isRequired,
  xLabel: PropTypes.string,
  yFormatter: PropTypes.func,
  height: PropTypes.number,
  startAt: PropTypes.number,
  colors: PropTypes.arrayOf(PropTypes.string)
}

export default MetricsChart
