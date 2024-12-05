import { Modal, Input, Select, Switch, message } from 'antd'
import React, { useState } from 'react'
import PropTypes from 'prop-types'
import apiCall from '../utils/apiCall'

const ACTIVATION_FUNCTIONS = [
  {
    label: 'ReLU',
    value: 'relu'
  },
  {
    label: 'Sigmoid',
    value: 'sigmoid'
  },
  {
    label: 'Tanh',
    value: 'tanh'
  },
  {
    label: 'Softmax',
    value: 'softmax'
  },
  {
    label: 'Linear',
    value: 'linear'
  }
]

const SessionStartConfigModal = ({ isModalOpen, setIsModalOpen, item }) => {
  const [config, setConfig] = useState({
    modelRangeConfig: {
      inputCount: 0,
      outputCount: 0,
      countOfInternalLayers: 0,
      neuronsInLayerMin: 0,
      neuronsInLayerMax: 0,
      activationFunc: 'relu',
      useBias: true
    }
  })

  const handleOk = async () => {
    try {
      await apiCall(
        `GeneticFlow/StartFlow?sessionId=${item.id}`,
        'POST',
        config
      )

      message.success('Session started successfully!')
      setIsModalOpen(false)
    } catch (error) {
      message.error(`Failed to start session: ${error.message}`)
    }
  }

  const handleCancel = () => {
    setIsModalOpen(false)
  }

  return (
    <Modal
      okText="Start"
      onOk={handleOk}
      open={isModalOpen}
      onClose={handleCancel}
      onCancel={handleCancel}>
      <div className="py-16">
        <div className="w-full justify-between">
          <div>
            <p>Input count:</p>
            <Input
              value={config.modelRangeConfig.inputCount}
              onChange={e =>
                setConfig({
                  ...config,
                  modelRangeConfig: {
                    ...config.modelRangeConfig,
                    inputCount: e.target.value
                  }
                })
              }
            />
          </div>
          <div>
            <p>Output count:</p>
            <Input
              value={config.modelRangeConfig.outputCount}
              onChange={e =>
                setConfig({
                  ...config,
                  modelRangeConfig: {
                    ...config.modelRangeConfig,
                    outputCount: e.target.value
                  }
                })
              }
            />
          </div>
        </div>
        <div className="">
          <div>
            <p>Count of internal layers:</p>
            <Input
              value={config.modelRangeConfig.countOfInternalLayers}
              onChange={e =>
                setConfig({
                  ...config,
                  modelRangeConfig: {
                    ...config.modelRangeConfig,
                    countOfInternalLayers: e.target.value
                  }
                })
              }
            />
          </div>
          <div>
            <p>Neurons in layer min:</p>
            <Input
              value={config.modelRangeConfig.neuronsInLayerMin}
              onChange={e =>
                setConfig({
                  ...config,
                  modelRangeConfig: {
                    ...config.modelRangeConfig,
                    neuronsInLayerMin: e.target.value
                  }
                })
              }
            />
          </div>
          <div>
            <p>Neurons in layer max:</p>
            <Input
              value={config.modelRangeConfig.neuronsInLayerMax}
              onChange={e =>
                setConfig({
                  ...config,
                  modelRangeConfig: {
                    ...config.modelRangeConfig,
                    neuronsInLayerMax: e.target.value
                  }
                })
              }
            />
          </div>
          <div>
            <p>Activation function:</p>
            <Select
              value={config.modelRangeConfig.activationFunc}
              onChange={value =>
                setConfig({
                  ...config,
                  modelRangeConfig: {
                    ...config.modelRangeConfig,
                    activationFunc: value
                  }
                })
              }
              className="w-full"
              options={ACTIVATION_FUNCTIONS}
            />
          </div>
          <div>
            <p>Use bias:</p>
            <Switch
              checked={config.modelRangeConfig.useBias}
              onChange={value =>
                setConfig({
                  ...config,
                  modelRangeConfig: {
                    ...config.modelRangeConfig,
                    useBias: value
                  }
                })
              }
            />
          </div>
        </div>
      </div>
    </Modal>
  )
}

SessionStartConfigModal.propTypes = {
  isModalOpen: PropTypes.bool,
  setIsModalOpen: PropTypes.func,
  item: PropTypes.object
}

export default SessionStartConfigModal
