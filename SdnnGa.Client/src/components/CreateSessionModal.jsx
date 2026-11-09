import { App, Form, Input, Modal } from 'antd'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import PropTypes from 'prop-types'
import apiCall from '../utils/apiCall'

// Створення сесії; після створення одразу відкриваємо її, щоб користувач продовжив налаштування.
const CreateSessionModal = ({ open, onClose }) => {
  const { message } = App.useApp()
  const navigate = useNavigate()
  const [form] = Form.useForm()
  const [saving, setSaving] = useState(false)

  const handleCreate = async () => {
    const values = await form.validateFields()
    setSaving(true)
    try {
      const result = await apiCall('Sessions', 'POST', {
        name: values.name.trim(),
        description: values.description?.trim() || null
      })

      if (result?.isError || !result?.entity?.id) {
        message.error(result?.message || 'Could not create the session.')
        return
      }

      message.success('Session created. Next, upload your dataset.')
      form.resetFields()
      onClose()
      navigate(`/session/${result.entity.id}`)
    } catch (error) {
      message.error(`Could not create the session: ${error.message}`)
    } finally {
      setSaving(false)
    }
  }

  return (
    <Modal
      title="New session"
      open={open}
      okText="Create session"
      onOk={handleCreate}
      confirmLoading={saving}
      onCancel={onClose}
      destroyOnClose>
      <p className="text-slate-500 mb-4">
        A session searches for the best neural network architecture for one dataset.
      </p>
      <Form form={form} layout="vertical" requiredMark="optional" onFinish={handleCreate}>
        <Form.Item
          name="name"
          label="Name"
          rules={[
            { required: true, whitespace: true, message: 'Give the session a name' },
            { max: 64, message: 'Use at most 64 characters' }
          ]}>
          <Input placeholder="e.g. Iris classification" autoFocus maxLength={64} />
        </Form.Item>
        <Form.Item name="description" label="Description" rules={[{ max: 256 }]}>
          <Input.TextArea
            placeholder="What are you trying to predict?"
            autoSize={{ minRows: 2, maxRows: 4 }}
            maxLength={256}
            showCount
          />
        </Form.Item>
      </Form>
    </Modal>
  )
}

CreateSessionModal.propTypes = {
  open: PropTypes.bool,
  onClose: PropTypes.func
}

export default CreateSessionModal
