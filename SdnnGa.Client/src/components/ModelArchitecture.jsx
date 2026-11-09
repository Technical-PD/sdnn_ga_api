import { useEffect, useState } from 'react'
import { Skeleton, Typography } from 'antd'
import { ArrowRightOutlined } from '@ant-design/icons'
import PropTypes from 'prop-types'
import { fetchStorageText } from '../utils/storageFiles'
import { formatNumber } from '../utils/format'

// Кількість параметрів щільних шарів (ваги + bias).
const countParameters = config => {
  let previous = config.InputShape?.[0] ?? 0
  let total = 0
  for (const layer of [...(config.InternalLayers || []), config.OutputLayer].filter(Boolean)) {
    total += previous * layer.NeuronsCount + (layer.UseBias ? layer.NeuronsCount : 0)
    previous = layer.NeuronsCount
  }
  return total
}

const LayerBox = ({ title, units, activation, bias, tone }) => {
  const tones = {
    input: 'bg-slate-50 border-slate-200',
    hidden: 'bg-indigo-50 border-indigo-200',
    output: 'bg-emerald-50 border-emerald-200'
  }
  return (
    <div className={`rounded-lg border px-3 py-2 min-w-[96px] text-center ${tones[tone]}`}>
      <div className="text-[11px] uppercase tracking-wide text-slate-500">{title}</div>
      <div className="text-lg font-semibold text-slate-800 leading-tight">{units}</div>
      {activation && <div className="text-xs text-slate-600">{activation}</div>}
      {bias !== undefined && <div className="text-[11px] text-slate-400">{bias ? 'with bias' : 'no bias'}</div>}
    </div>
  )
}

LayerBox.propTypes = {
  title: PropTypes.string,
  units: PropTypes.node,
  activation: PropTypes.string,
  bias: PropTypes.bool,
  tone: PropTypes.oneOf(['input', 'hidden', 'output'])
}

// Схема архітектури моделі з .NET-конфігу (model_configs_net/model-N.json).
const ModelArchitecture = ({ configPath }) => {
  const [config, setConfig] = useState(null)
  const [error, setError] = useState(null)

  useEffect(() => {
    let cancelled = false
    setConfig(null)
    setError(null)

    if (!configPath) {
      return undefined
    }

    fetchStorageText(configPath)
      .then(text => !cancelled && setConfig(JSON.parse(text)))
      .catch(() => !cancelled && setError('Architecture is not available.'))

    return () => {
      cancelled = true
    }
  }, [configPath])

  if (!configPath || error) {
    return <Typography.Text type="secondary">{error || 'The model has not been built yet.'}</Typography.Text>
  }

  if (!config) {
    return <Skeleton active paragraph={{ rows: 2 }} />
  }

  const layers = [
    { key: 'input', title: 'Input', units: config.InputShape?.[0], tone: 'input' },
    ...(config.InternalLayers || []).map((layer, index) => ({
      key: `hidden-${index}`,
      title: `Hidden ${index + 1}`,
      units: layer.NeuronsCount,
      activation: layer.ActivationFunc,
      bias: layer.UseBias,
      tone: 'hidden'
    })),
    {
      key: 'output',
      title: 'Output',
      units: config.OutputLayer?.NeuronsCount,
      activation: config.OutputLayer?.ActivationFunc,
      bias: config.OutputLayer?.UseBias,
      tone: 'output'
    }
  ]

  return (
    <div>
      <div className="flex flex-wrap items-center gap-2">
        {layers.map((layer, index) => (
          <div key={layer.key} className="flex items-center gap-2">
            {index > 0 && <ArrowRightOutlined className="text-slate-300" />}
            <LayerBox {...layer} />
          </div>
        ))}
      </div>
      <Typography.Text type="secondary" className="block mt-2 text-xs">
        {config.InternalLayers?.length ?? 0} hidden layer(s) · ≈ {formatNumber(countParameters(config))} trainable parameters
      </Typography.Text>
    </div>
  )
}

ModelArchitecture.propTypes = {
  configPath: PropTypes.string
}

export default ModelArchitecture
