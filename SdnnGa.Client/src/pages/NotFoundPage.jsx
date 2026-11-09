import { Button, Result } from 'antd'
import { useNavigate } from 'react-router-dom'
import PropTypes from 'prop-types'

const NotFoundPage = ({ title = 'Page not found', subtitle = 'This page does not exist or was deleted.' }) => {
  const navigate = useNavigate()
  return (
    <Result
      status="404"
      title={title}
      subTitle={subtitle}
      extra={
        <Button type="primary" onClick={() => navigate('/')}>
          Back to sessions
        </Button>
      }
    />
  )
}

NotFoundPage.propTypes = {
  title: PropTypes.string,
  subtitle: PropTypes.string
}

export default NotFoundPage
