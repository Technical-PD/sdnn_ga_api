import { Spin } from 'antd'

const Loading = () => {
  return (
    <div
      style={{
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        height: '100%',
        widows: '100%'
      }}>
      <Spin size="large" />
    </div>
  )
}

export default Loading
