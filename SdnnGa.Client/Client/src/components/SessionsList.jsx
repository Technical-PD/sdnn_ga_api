import { Button, Input, List, message, Modal } from 'antd'
import useGetSessionList from '../hooks/useGetSessionList'
import Loading from './Loading'
import SessionItem from './SessionItem'
import { useState } from 'react'
import apiCall from '../utils/apiCall'

const SessionsList = () => {
  const [data, loading] = useGetSessionList()

  const [isModalOpen, setIsModalOpen] = useState(false)
  const [name, setName] = useState('')
  const [description, setDescription] = useState('')

  const handleCancel = () => {
    setIsModalOpen(false)
  }
  const handleOpen = () => {
    setIsModalOpen(true)
  }

  const handleChangeName = e => {
    setName(e.target.value)
  }
  const handleChangeDescription = e => {
    setDescription(e.target.value)
  }

  const handleOk = async () => {
    try {
      const body = {
        name,
        description
      }

      await apiCall('Sessions', 'POST', body)

      message.success('Session created successfully!')

      setIsModalOpen(false)
    } catch (error) {
      message.error(`Failed to create session: ${error.message}`)
    }
  }

  return loading ? (
    <Loading />
  ) : (
    <>
      <List
        header={
          <div className="flex items-center w-full justify-between">
            <h1>Sessions</h1>
            <div>
              <Button onClick={handleOpen} type="primary" variant="dashed">
                Create session
              </Button>
            </div>
          </div>
        }
        bordered
        dataSource={data || []}
        renderItem={item => <SessionItem item={item} />}
      />
      <Modal
        okText="Create"
        onOk={handleOk}
        open={isModalOpen}
        onClose={handleCancel}
        onCancel={handleCancel}>
        <div className="py-16">
          <p>Name:</p>
          <Input value={name} onChange={handleChangeName} />
          <p>Description:</p>
          <Input.TextArea
            value={description}
            onChange={handleChangeDescription}
          />
        </div>
      </Modal>
    </>
  )
}

export default SessionsList
