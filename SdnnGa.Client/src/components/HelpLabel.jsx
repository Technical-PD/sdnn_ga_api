import { Tooltip } from 'antd'
import { QuestionCircleOutlined } from '@ant-design/icons'
import PropTypes from 'prop-types'

// Підпис поля форми з іконкою-підказкою.
const HelpLabel = ({ label, help }) => (
  <span className="inline-flex items-center gap-1.5">
    {label}
    {help && (
      <Tooltip title={help}>
        <QuestionCircleOutlined className="text-slate-400 cursor-help" />
      </Tooltip>
    )}
  </span>
)

HelpLabel.propTypes = {
  label: PropTypes.node,
  help: PropTypes.node
}

export default HelpLabel
