import { Alert, App, Button, Col, Divider, Form, InputNumber, Radio, Row, Slider, Typography } from 'antd'
import { useState } from 'react'
import PropTypes from 'prop-types'
import HelpLabel from '../../components/HelpLabel'
import apiCall from '../../utils/apiCall'
import {
  CROSSOVER_METHODS,
  EVOLUTION_STRATEGIES,
  GENETIC_DEFAULTS,
  labelOf,
  PARENT_CRITERIA,
  SELECTION_CRITERIA,
  usesCrossover
} from '../../constants/options'

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
    help: 'Chance that a hidden layer switches its bias term on or off.'
  }
]

// Пояснення, які саме моделі стануть батьками при схрещуванні.
const describeParents = (first, second) => {
  if (!first || !second) {
    return null
  }
  if (first === second) {
    return `The two models with the ${labelOf(PARENT_CRITERIA, first).toLowerCase()} in each generation are crossed.`
  }
  return `The model with the ${labelOf(PARENT_CRITERIA, first).toLowerCase()} is crossed with the model with the ${labelOf(
    PARENT_CRITERIA,
    second
  ).toLowerCase()}. If one model is best on both, the runner-up becomes parent B.`
}

// Крок 3: параметри еволюції архітектур.
const GeneticStep = ({ sessionId, config, disabled, onSaved, onBack }) => {
  const { message } = App.useApp()
  const [form] = Form.useForm()
  const [saving, setSaving] = useState(false)
  const generations = Form.useWatch('maxEpoches', form)
  const population = Form.useWatch('countOfModelsInEpoch', form)
  const strategy = Form.useWatch('evolutionStrategy', form)
  const firstParent = Form.useWatch('firstParentCriterion', form)
  const secondParent = Form.useWatch('secondParentCriterion', form)
  const crossoverMethod = Form.useWatch('crossoverMethod', form)

  const crossover = usesCrossover(strategy)
  const mutationOff = strategy === 'Crossover'

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
        stopLossValue: config.stopLossValue,
        evolutionStrategy: config.evolutionStrategy ?? GENETIC_DEFAULTS.evolutionStrategy,
        crossoverMethod: config.crossoverMethod ?? GENETIC_DEFAULTS.crossoverMethod,
        firstParentCriterion: config.firstParentCriterion ?? GENETIC_DEFAULTS.firstParentCriterion,
        secondParentCriterion: config.secondParentCriterion ?? GENETIC_DEFAULTS.secondParentCriterion
      }
    : GENETIC_DEFAULTS

  const save = async values => {
    setSaving(true)
    try {
      // getFieldsValue(true) — включно з прихованими полями схрещування, щоб не скинути їх у режимі "лише мутація".
      // Імовірності та поріг точності — у відсотках (API ділить на 100).
      const allValues = { ...form.getFieldsValue(true), ...values }
      const result = await apiCall(`GeneticConfig/${sessionId}`, 'POST', { name: 'Genetic settings', ...allValues })
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
            label={<HelpLabel label="Models per generation" help="How many candidate networks (children) are trained in each generation." />}
            rules={[{ required: true, message: 'Required' }]}>
            <InputNumber min={1} max={100} className="w-full" />
          </Form.Item>
        </Col>
        <Col xs={24} md={8}>
          <Form.Item
            name="selectionCriterion"
            label={
              <HelpLabel
                label="Best model has the"
                help="Defines which model counts as the best one: it is shown in the results, checked against the stop conditions, and used as the parent for mutation-only evolution."
              />
            }>
            <Radio.Group optionType="button" buttonStyle="solid" options={SELECTION_CRITERIA} />
          </Form.Item>
        </Col>
      </Row>

      <Typography.Text type="secondary" className="text-sm">
        Up to <b>{(generations || 0) * (population || 0)}</b> models will be trained. The first generation is random; every next generation is
        bred from the best models of the previous one.
      </Typography.Text>

      <Divider orientation="left" orientationMargin={0} className="!mt-6">
        How new generations are created
      </Divider>
      <Form.Item name="evolutionStrategy" className="!mb-4">
        <Radio.Group className="w-full">
          <Row gutter={[12, 12]}>
            {EVOLUTION_STRATEGIES.map(option => (
              <Col key={option.value} xs={24} md={8}>
                <Radio value={option.value} className="choice-card">
                  <div className="font-medium">{option.label}</div>
                  <div className="text-xs text-slate-500 mt-0.5">{option.description}</div>
                </Radio>
              </Col>
            ))}
          </Row>
        </Radio.Group>
      </Form.Item>

      {crossover && (
        <div className="rounded-lg border border-slate-200 bg-slate-50 p-4 mb-2">
          <Row gutter={[24, 0]}>
            <Col xs={24} md={12}>
              <Form.Item
                name="firstParentCriterion"
                label={<HelpLabel label="Parent A" help="How the first parent is picked from the previous generation." />}>
                <Radio.Group optionType="button" buttonStyle="solid" options={PARENT_CRITERIA} />
              </Form.Item>
            </Col>
            <Col xs={24} md={12}>
              <Form.Item
                name="secondParentCriterion"
                label={<HelpLabel label="Parent B" help="How the second parent is picked. It is always a different model than parent A." />}>
                <Radio.Group optionType="button" buttonStyle="solid" options={PARENT_CRITERIA} />
              </Form.Item>
            </Col>
          </Row>
          <Typography.Paragraph type="secondary" className="text-sm !-mt-2">
            {describeParents(firstParent, secondParent)}
          </Typography.Paragraph>

          <Form.Item
            name="crossoverMethod"
            label={<HelpLabel label="Crossover method" help="How the hidden layers of the two parents are combined into a child." />}
            extra={CROSSOVER_METHODS.find(option => option.value === crossoverMethod)?.description}
            className="!mb-3">
            <Radio.Group
              optionType="button"
              buttonStyle="solid"
              options={CROSSOVER_METHODS.map(({ value, label }) => ({ value, label }))}
            />
          </Form.Item>

          <Alert
            type="info"
            showIcon
            message="The input size and the number of output neurons never change: they always match your dataset. If a generation has fewer than two successfully trained models, it is bred by mutation instead."
          />
        </div>
      )}

      <Divider orientation="left" orientationMargin={0}>
        Mutation probabilities
      </Divider>
      {mutationOff && (
        <Typography.Paragraph type="secondary" className="text-sm">
          Mutation is not used with <b>Crossover only</b>. These values are kept for when you switch strategy.
        </Typography.Paragraph>
      )}
      <Row gutter={[32, 0]}>
        {MUTATIONS.map(mutation => (
          <Col key={mutation.name} xs={24} md={12}>
            <Form.Item name={mutation.name} label={<HelpLabel label={mutation.label} help={mutation.help} />}>
              <Slider min={0} max={100} marks={PERCENT_MARKS} disabled={disabled || mutationOff} tooltip={{ formatter: value => `${value}%` }} />
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
