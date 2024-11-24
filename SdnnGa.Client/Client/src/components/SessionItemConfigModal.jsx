import { Button, Input, message, Modal, Select } from 'antd'
import React, { useEffect, useState } from 'react'
import PropTypes from 'prop-types'
import apiCall from '../utils/apiCall'

const OPTIMIZE_OPTIONS = [
  {
    label: 'SGD',
    value: 'sgd'
  },
  {
    label: 'Adam',
    value: 'adam'
  }
]

const SELECTION_CRITERION_OPTIONS = [
  {
    label: 'By Accuracy',
    value: 'ByAccuracy'
  },
  {
    label: 'By Loss',
    value: 'ByLoss'
  }
]

const LOSS_OPTIONS = [
  {
    label: 'Categorical Crossentropy',
    value: 'categorical_crossentropy'
  },
  {
    label: 'Mean Squared Error',
    value: 'mean_squared_error'
  },
  {
    label: 'Mean Absolute Error',
    value: 'mean_absolute_error'
  },
  {
    label: 'Mean Squared Logarithmic Error',
    value: 'mean_squared_logarithmic_error'
  },
  {
    label: 'Mean Absolute Percentage Error',
    value: 'mean_absolute_percentage_error'
  },
  {
    label: 'Binary Crossentropy',
    value: 'binary_crossentropy'
  }
]

const SessionItemConfigModal = ({ isModalOpen, setIsModalOpen, item }) => {
  const [isFirstDisabled, setIsFirstDisabled] = useState(false)
  const [isSecondDisabled, setIsSecondDisabled] = useState(false)

  useEffect(() => {
    const fetchData = async () => {
      try {
        if (isModalOpen) {
          const firstConfig = await apiCall(`FitConfig/BySession/${item?.id}`)

          if (firstConfig) {
            const { entity } = firstConfig

            if (entity) {
              setFirstFormData({
                maxEpoches: entity?.maxEpoches,
                fitMethod: entity?.fitMethod,
                lossFunc: entity?.lossFunc,
                name: entity?.name
              })
              setIsFirstDisabled(true)
            }
          }
        }
      } catch (error) {
        message.error(`Failed to fetch config data: ${error.message}`)
      }
    }

    const fetchSecondData = async () => {
      try {
        if (isModalOpen) {
          const secondConfig = await apiCall(
            `GeneticConfig/BySession/${item?.id}`
          )
          if (secondConfig) {
            const { entity } = secondConfig

            console.log(secondConfig)

            if (entity) {
              setSecondFormData({
                maxEpoches: entity?.maxEpoches,
                mutationCof: entity?.mutationCof,
                selectionCriterion: entity?.selectionCriterion,
                name: entity?.name
              })
              setIsSecondDisabled(true)
            }
          }
        }
      } catch (error) {
        message.error(`Failed to fetch config data: ${error.message}`)
      }
    }

    fetchData()
    fetchSecondData()
  }, [item?.id, isModalOpen])

  const handleCancel = () => {
    setIsModalOpen(false)
  }

  const handleOk = () => {
    setIsModalOpen(false)
  }

  const [firstFormData, setFirstFormData] = useState({
    maxEpoches: 0,
    fitMethod: '',
    lossFunc: '',
    name: `Model ${item.id}`
  })

  const [secondFormData, setSecondFormData] = useState({
    selectionCriterion: '',
    maxEpoches: 0,
    mutationCof: 0,
    name: `Model ${item.id}`
  })

  const handleSaveFirst = async () => {
    try {
      await apiCall(`FitConfig/AddToSession/${item?.id}`, 'POST', firstFormData)

      message.success('Model saved successfully!')
    } catch (error) {
      message.error(`Failed to save model: ${error.message}`)
    }
  }

  const handleSaveSecond = async () => {
    try {
      await apiCall(`GeneticConfig/${item?.id}`, 'POST', secondFormData)

      message.success('Model saved successfully!')
    } catch (error) {
      message.error(`Failed to save model: ${error.message}`)
    }
  }

  return (
    <Modal
      okText="Ok"
      onOk={handleOk}
      open={isModalOpen}
      onClose={handleCancel}
      onCancel={handleCancel}>
      <div className="py-16 flex space-x-4 w-full justify-between">
        <div>
          <p>Max epochs:</p>
          <Input
            value={firstFormData.maxEpoches}
            onChange={e => {
              setFirstFormData({
                ...firstFormData,
                maxEpoches: e.target.value
              })
            }}
            type="number"
            placeholder="Max epochs"
          />
          <p>Optimizer:</p>
          <Select
            value={firstFormData.fitMethod}
            onChange={value => {
              setFirstFormData({
                ...firstFormData,
                fitMethod: value
              })
            }}
            className="w-full"
            options={OPTIMIZE_OPTIONS}
          />
          <p>Loss function:</p>
          <Select
            value={firstFormData.lossFunc}
            onChange={value => {
              setFirstFormData({
                ...firstFormData,
                lossFunc: value
              })
            }}
            className="w-full"
            options={LOSS_OPTIONS}
          />
          <Button
            disabled={isFirstDisabled}
            onClick={handleSaveFirst}
            className="mt-4"
            type="primary">
            Save
          </Button>
        </div>
        <div className="flex border-r border-r-gray-500" />
        <div className="flex flex-col items-end">
          <p>Selection Criterion:</p>
          <Select
            value={secondFormData.selectionCriterion}
            onChange={value => {
              setSecondFormData({
                ...secondFormData,
                selectionCriterion: value
              })
            }}
            className="w-full"
            options={SELECTION_CRITERION_OPTIONS}
          />
          <p>Max epoch:</p>
          <Input
            value={secondFormData.maxEpoches}
            onChange={e => {
              setSecondFormData({
                ...secondFormData,
                maxEpoches: e.target.value
              })
            }}
            type="number"
            placeholder="Max epoch"
          />
          <p>Mutation coefficient:</p>
          <Input
            value={secondFormData.mutationCof}
            onChange={e => {
              setSecondFormData({
                ...secondFormData,
                mutationCof: e.target.value
              })
            }}
            type="number"
            placeholder="Mutation coefficient"
          />
          <Button
            disabled={isSecondDisabled}
            onClick={handleSaveSecond}
            className="mt-4"
            type="primary">
            Save
          </Button>
        </div>
      </div>
    </Modal>
  )
}

SessionItemConfigModal.propTypes = {
  isModalOpen: PropTypes.bool,
  setIsModalOpen: PropTypes.func,
  item: PropTypes.object
}

export default SessionItemConfigModal
