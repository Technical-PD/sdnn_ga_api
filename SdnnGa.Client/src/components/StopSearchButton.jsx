import { App, Button, Popconfirm } from 'antd'
import { StopOutlined } from '@ant-design/icons'
import { useState } from 'react'
import PropTypes from 'prop-types'
import apiCall from '../utils/apiCall'

// Зупинка генетичного пошуку сесії. Навчені моделі зберігаються, пошук можна продовжити пізніше.
const StopSearchButton = ({ sessionId, onStopped, size }) => {
  const { message } = App.useApp()
  const [stopping, setStopping] = useState(false)

  const stop = async () => {
    setStopping(true)
    try {
      const result = await apiCall(`GeneticFlow/Stop?sessionId=${sessionId}`, 'POST')
      if (result?.isError) {
        message.error(result.message)
        return
      }
      message.success('Search stopped. Trained models are kept. You can continue later from the Setup tab.')
      await onStopped?.()
    } catch (error) {
      message.error(`Could not stop the search: ${error.message}`)
    } finally {
      setStopping(false)
    }
  }

  return (
    <Popconfirm
      title="Stop the search?"
      description="Models that are training or waiting will be cancelled. Already trained models are kept."
      okText="Stop search"
      okButtonProps={{ danger: true }}
      onConfirm={stop}>
      <Button danger type="primary" icon={<StopOutlined />} loading={stopping} size={size}>
        Stop search
      </Button>
    </Popconfirm>
  )
}

StopSearchButton.propTypes = {
  sessionId: PropTypes.string.isRequired,
  onStopped: PropTypes.func,
  size: PropTypes.string
}

export default StopSearchButton
