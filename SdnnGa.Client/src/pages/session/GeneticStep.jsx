import { App, Button, Col, Divider, Form, InputNumber, Radio, Row, Slider, Typography } from 'antd'
import { useState } from 'react'
import PropTypes from 'prop-types'
import HelpLabel from '../../components/HelpLabel'
import apiCall from '../../utils/apiCall'
import { GENETIC_DEFAULTS, SELECTION_CRITERIA } from '../../constants/options'

const PERCENT_MARKS = { 0: '0%', 50: '50%', 100: '100%' }

const MUTATIONS = [
  {
    name: 'actFuncMutationProb',
    label: 'Change activation function',
    help: 'Chance that a layer switches to a different activation function (ReLU, sigmoid, tanh, …).'
  },
  {
    name: 'countOfNeuronMutationProb',
    label: 'Change neuron count',
    help: 'Chance that a hidden layer gains or loses up to 10 neurons.'
  },
  {
    name: 'countOfInternalLayerMutationProb',
    label: 'Add or remove a layer',
    help: 'Chance that the network gets one hidden layer more or less.'
  },
  {
    name: 'biasMutationProb',
    label: 'Toggle bias',
    help: 'Chance that a layer switches its bias term on or off.'
  }
]

// Крок 3: параметри еволюції архітектур.
const GeneticStep = ({ sessionId, config, disabled, onSaved, onBack }) => {
  const { message } = App.useApp()
  const [form] = Form.useForm()
  const [saving, setSaving] = useState(false)
  const generations = Form.useWatch('maxEpoches', form)
  const population = Form.useWatch('countOfModelsInEpoch', form)

  const initialValues = config
    ? {
        maxEpoches: config.maxEpoches,
        countOfModelsInEpoch: config.countOfModelsInEpoch,
        selectionCriterion: config.selectionCriterion,
        actFuncMutationProb: Math.round(config.actFuncMutationProb),
        countOfNeuronMutationProb: Math.round(config.countOfNeuronMutationProb),
        countOfInternalLayerMutationProb: Math.round(config.countOfInternalLayerMutationProb),
        biasMutationProb: Math.round(config.biasMutationProb),
        stopAccValue: Math.round(config.stopAccValue),
        stopLossValue: config.stopLossValue
      }
    : GENETIC_DEFAULTS

  const save = async values => {
    setSaving(true)
    try {
      // Імовірності та поріг точності — у відсотках (API ділить на 100).
      const result = await apiCall(`GeneticConfig/${sessionId}`, 'POST', { name: 'Genetic settings', ...values })
      if (result?.isError) {
        message.error(result.message)
        return
      }
      message.success('Genetic algorithm settings saved')
      await onSaved()
    } catch (error) {
      message.error(`Could not save: ${error.message}`)
    } finally {
      setSaving(false)
    }
  }

  return (
    <Form
      key={config?.id ?? 'new'}
      form={form}
      layout="vertical"
      initialValues={initialValues}
      onFinish={save}
      disabled={disabled}
      requiredMark={false}>
      <Row gutter={24}>
        <Col xs={24} md={8}>
          <Form.Item
            name="maxEpoches"
            label={<HelpLabel label="Generations" help="How many rounds of evolution to run (at most)." />}
            rules={[{ required: true, message: 'Required' }]}>
            <InputNumber min={1} max={500} className="w-full" />
          </Form.Item>
        </Col>
        <Col xs={24} md={8}>
          <Form.Item
            name="countOfModelsInEpoch"
            label={<HelpLabel label="Models per generation" help="How many candidate networks are trained in each generation." />}
            rules={[{ required: true, message: 'Required' }]}>
            <InputNumber min={1} max={100} className="w-full" />
          </Form.Item>
        </Col>
        <Col xs={24} md={8}>
          <Form.Item
            name="selectionCriterion"
            label={<HelpLabel label="Pick the parent by" help="Which model of a generation becomes the parent of the next one." />}>
            <Radio.Group optionType="button" buttonStyle="solid" options={SELECTION_CRITERIA} />
          </Form.Item>
        </Col>
      </Row>

      <Typography.Text type="secondary" className="text-sm">
        Up to <b>{(generations || 0) * (population || 0)}</b> models will be trained. Each generation mutates the best model of the previous one.
      </Typography.Text>

      <Divider orientation="left" orientationMargin={0} className="!mt-6">
        Mutation probabilities
      </Divider>
      <Row gutter={[32, 0]}>
        {MUTATIONS.map(mutation => (
          <Col key={mutation.name} xs={24} md={12}>
            <Form.Item name={mutation.name} label={<HelpLabel label={mutation.label} help={mutation.help} />}>
              <Slider min={0} max={100} marks={PERCENT_MARKS} tooltip={{ formatter: value => `${value}%` }} />
            </Form.Item>
          </Col>
        ))}
      </Row>

      <Divider orientation="left" orientationMargin={0}>
        Stop early when
      </Divider>
      <Row gutter={[32, 0]}>
        <Col xs={24} md={12}>
          <Form.Item
            name="stopAccValue"
            label={<HelpLabel label="Accuracy reaches" help="Stop evolving once the best model of a generation is at least this accurate." />}>
            <Slider min={0} max={100} marks={PERCENT_MARKS} tooltip={{ formatter: value => `${value}%` }} />
          </Form.Item>
        </Col>
        <Col xs={24} md={12}>
          <Form.Item
            name="stopLossValue"
            label={<HelpLabel label="Loss drops to" help="Stop evolving once the best model's loss is at or below this value." />}
            rules={[{ required: true, message: 'Required' }]}>
            <InputNumber min={0} step={0.001} style={{ width: 200 }} />
          </Form.Item>
        </Col>
      </Row>

      <div className="flex justify-between mt-2">
        <Button onClick={onBack} disabled={false}>
          Back
        </Button>
        <Button type="primary" htmlType="submit" loading={saving}>
          Save and continue
        </Button>
      </div>
    </Form>
  )
}

GeneticStep.propTypes = {
  sessionId: PropTypes.string.isRequired,
  config: PropTypes.object,
  disabled: PropTypes.bool,
  onSaved: PropTypes.func.isRequired,
  onBack: PropTypes.func.isRequired
}

export default GeneticStep
