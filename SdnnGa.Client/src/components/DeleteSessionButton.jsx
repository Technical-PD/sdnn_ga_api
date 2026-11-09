import { App, Button, Popconfirm, Tooltip } from 'antd'
import { DeleteOutlined } from '@ant-design/icons'
import { useState } from 'react'
import PropTypes from 'prop-types'
import apiCall from '../utils/apiCall'

const DeleteSessionButton = ({ session, onDeleted, iconOnly = false }) => {
  const { message } = App.useApp()
  const [deleting, setDeleting] = useState(false)

  const handleDelete = async () => {
    setDeleting(true)
    try {
      const result = await apiCall(`Sessions/${session.id}`, 'DELETE')
      if (result?.isError) {
        message.error(result.message)
        return
      }
      message.success(`Session "${session.name}" deleted`)
      onDeleted?.()
    } catch (error) {
      message.error(`Could not delete the session: ${error.message}`)
    } finally {
      setDeleting(false)
    }
  }

  const button = (
    <Button
      danger
      type={iconOnly ? 'text' : 'default'}
      icon={<DeleteOutlined />}
      loading={deleting}
      disabled={session.isRunning}
      onClick={event => event.stopPropagation()}>
      {iconOnly ? null : 'Delete'}
    </Button>
  )

  if (session.isRunning) {
    return <Tooltip title="Wait until the search finishes to delete this session">{button}</Tooltip>
  }

  return (
    <Popconfirm
      title="Delete this session?"
      description="Its dataset, generations, models and weights will be removed permanently."
      okText="Delete"
      okButtonProps={{ danger: true }}
      onConfirm={event => {
        event?.stopPropagation()
        return handleDelete()
      }}
      onCancel={event => event?.stopPropagation()}>
      {button}
    </Popconfirm>
  )
}

DeleteSessionButton.propTypes = {
  session: PropTypes.object.isRequired,
  onDeleted: PropTypes.func,
  iconOnly: PropTypes.bool
}

export default DeleteSessionButton
