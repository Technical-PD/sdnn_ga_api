import { Tag } from 'antd'
import {
  CheckCircleOutlined,
  ClockCircleOutlined,
  CloseCircleOutlined,
  PlayCircleOutlined,
  SyncOutlined,
  ToolOutlined
} from '@ant-design/icons'
import PropTypes from 'prop-types'
import { modelState } from '../utils/modelState'

const SESSION_STATUS = {
  NeedsSetup: { color: 'default', icon: <ToolOutlined />, text: 'Needs setup' },
  Ready: { color: 'blue', icon: <PlayCircleOutlined />, text: 'Ready to run' },
  Running: { color: 'processing', icon: <SyncOutlined spin />, text: 'Running' },
  Finished: { color: 'success', icon: <CheckCircleOutlined />, text: 'Finished' }
}

export const SessionStatusTag = ({ status }) => {
  const view = SESSION_STATUS[status] ?? SESSION_STATUS.NeedsSetup
  return (
    <Tag color={view.color} icon={view.icon} className="m-0">
      {view.text}
    </Tag>
  )
}

SessionStatusTag.propTypes = {
  status: PropTypes.string
}

const MODEL_STATUS = {
  Trained: { color: 'success', icon: <CheckCircleOutlined />, text: 'Trained' },
  Failed: { color: 'error', icon: <CloseCircleOutlined />, text: 'Failed' },
  Training: { color: 'processing', icon: <SyncOutlined spin />, text: 'In progress' },
  Queued: { color: 'default', icon: <ClockCircleOutlined />, text: 'Queued' }
}

export const ModelStatusTag = ({ model }) => {
  const view = MODEL_STATUS[modelState(model)]
  return (
    <Tag color={view.color} icon={view.icon} className="m-0">
      {view.text}
    </Tag>
  )
}

ModelStatusTag.propTypes = {
  model: PropTypes.object
}
