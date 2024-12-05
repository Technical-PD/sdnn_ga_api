import { message, Modal, Upload } from 'antd'
import apiCall from '../utils/apiCall'
import { useState } from 'react'
import { InboxOutlined } from '@ant-design/icons'
import PropTypes from 'prop-types'

const { Dragger } = Upload

const SessionItemUploadModal = ({ item, isModalOpen, setIsModalOpen }) => {
  const [firstFile, setFirstFile] = useState(null)
  const [secondFile, setSecondFile] = useState(null)

  const handleCancel = () => {
    setIsModalOpen(false)
  }

  const handleChangeFirst = ({ fileList }) => {
    setFirstFile(fileList[0])
  }

  const handleChangeSecond = ({ fileList }) => {
    setSecondFile(fileList[0])
  }

  const handleUpload = async () => {
    const firstFormData = new FormData()
    const secondFormData = new FormData()

    firstFormData.append('dataForm', firstFile.originFileObj)
    secondFormData.append('dataForm', secondFile.originFileObj)

    try {
      const res = await Promise.all([
        apiCall(`Data/AddXData/${item.id}`, 'POST', firstFormData, true),
        apiCall(`Data/AddYData/${item.id}`, 'POST', secondFormData, true)
      ])

      message.success('Upload successful!')
      setIsModalOpen(false)
    } catch (error) {
      message.error(`Upload failed: ${error.message}`)
    }
  }

  return (
    <Modal
      title="Upload dataset"
      open={isModalOpen}
      onOk={handleUpload}
      onCancel={handleCancel}>
      <div className="flex space-x-4 mb-16">
        <Dragger
          onChange={handleChangeFirst}
          fileList={firstFile ? [firstFile] : []}
          accept=".csv"
          beforeUpload={() => false}>
          <p className="ant-upload-drag-icon">
            <InboxOutlined />
          </p>
          <p className="ant-upload-text">X Train dataset (CSV file)</p>
        </Dragger>
        <Dragger
          onChange={handleChangeSecond}
          fileList={secondFile ? [secondFile] : []}
          accept=".csv"
          beforeUpload={() => false}>
          <p className="ant-upload-drag-icon">
            <InboxOutlined />
          </p>
          <p className="ant-upload-text">Y Train dataset (CSV file)</p>
        </Dragger>
      </div>
    </Modal>
  )
}

SessionItemUploadModal.propTypes = {
  item: PropTypes.object,
  isModalOpen: PropTypes.bool,
  setIsModalOpen: PropTypes.func
}

export default SessionItemUploadModal
