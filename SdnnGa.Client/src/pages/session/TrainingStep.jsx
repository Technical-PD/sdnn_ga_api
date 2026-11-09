import { Alert, App, Button, Col, Form, InputNumber, Radio, Row, Select, Typography } from 'antd'
import { BulbOutlined, InfoCircleOutlined } from '@ant-design/icons'
import { useState } from 'react'
import PropTypes from 'prop-types'
import HelpLabel from '../../components/HelpLabel'
import apiCall from '../../utils/apiCall'
import { FIXED_TRAINING_SETTINGS, labelOf, LOSS_FUNCTIONS, OPTIMIZERS, TRAINING_DEFAULTS } from '../../constants/options'

// Крок 2: як навчати кожну модель-кандидата.
const TrainingStep = ({ sessionId, config, suggestedLoss, disabled, onSaved, onBack }) => {
  const { message } = App.useApp()
  const [form] = Form.useForm()
  const [saving, setSaving] = useState(false)
  const lossFunc = Form.useWatch('lossFunc', form)
  const fitMethod = Form.useWatch('fitMethod', form)

  const initialValues = config
    ? { lossFunc: config.lossFunc, fitMethod: config.fitMethod, maxEpoches: config.maxEpoches }
    : { ...TRAINING_DEFAULTS, lossFunc: suggestedLoss ?? TRAINING_DEFAULTS.lossFunc }

  const save = async values => {
    setSaving(true)
    try {
      const result = await apiCall(`FitConfig/AddToSession/${sessionId}`, 'POST', { name: 'Training settings', ...values })
      if (result?.isError) {
        message.error(result.message)
        return
      }
      message.success('Training settings saved')
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
        <Col xs={24} lg={14}>
          <Form.Item
            name="lossFunc"
            label={<HelpLabel label="Loss function" help="What the network minimizes during training. It must match the kind of target you have." />}
            rules={[{ required: true }]}
            extra={
              suggestedLoss && lossFunc && lossFunc !== suggestedLoss ? (
                <span className="inline-flex items-center gap-1 text-amber-700">
                  <BulbOutlined />
                  Your targets look like a fit for <b>{labelOf(LOSS_FUNCTIONS, suggestedLoss)}</b>.
                  <Button
                    type="link"
                    size="small"
                    className="!px-1"
                    onClick={() => form.setFieldValue('lossFunc', suggestedLoss)}>
                    Use it
                  </Button>
                </span>
              ) : null
            }>
            <Select
              options={LOSS_FUNCTIONS}
              optionRender={option => (
                <div>
                  <div>{option.data.label}</div>
                  <div className="text-xs text-slate-500">{option.data.description}</div>
                </div>
              )}
            />
          </Form.Item>

          <Form.Item
            name="fitMethod"
            label={<HelpLabel label="Optimizer" help="The algorithm that updates the network weights." />}
            rules={[{ required: true }]}
            extra={OPTIMIZERS.find(option => option.value === fitMethod)?.description}>
            <Radio.Group optionType="button" buttonStyle="solid" options={OPTIMIZERS.map(({ value, label }) => ({ value, label }))} />
          </Form.Item>

          <Form.Item
            name="maxEpoches"
            label={
              <HelpLabel
                label="Training epochs per model"
                help="How many passes over the data each candidate model gets. Training stops earlier if the validation loss stops improving."
              />
            }
            rules={[{ required: true, message: 'Enter the number of epochs' }]}>
            <InputNumber min={1} max={5000} style={{ width: 200 }} addonAfter="epochs" />
          </Form.Item>
        </Col>

        <Col xs={24} lg={10}>
          <Alert
            type="info"
            icon={<InfoCircleOutlined />}
            showIcon
            message="Applied automatically"
            description={<Typography.Text className="text-sm">{FIXED_TRAINING_SETTINGS}</Typography.Text>}
          />
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

TrainingStep.propTypes = {
  sessionId: PropTypes.string.isRequired,
  config: PropTypes.object,
  suggestedLoss: PropTypes.string,
  disabled: PropTypes.bool,
  onSaved: PropTypes.func.isRequired,
  onBack: PropTypes.func.isRequired
}

export default TrainingStep
