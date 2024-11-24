import { List } from 'antd'
import PropTypes from 'prop-types'
import React, { useState } from 'react'
import { Button, Modal, message, Upload } from 'antd'
import apiCall from '../utils/apiCall'
import SessionItemUploadModal from './SessionItemUploadModal'
import SessionItemConfigModal from './SessionItemConfigModal'
import SessionStartConfigModal from './SessionStartConfigModal'

const SessionItem = ({ item }) => {
  const [isUploadModalOpen, setIsUploadModalOpen] = useState(false)
  const [isSessionConfigModalOpen, setIsSessionConfigModalOpen] =
    useState(false)
  const [isSessionStartConfigModalOpen, setIsSessionStartConfigModalOpen] =
    useState(false)

  const showModal = () => {
    setIsUploadModalOpen(true)
  }

  const showConfigModal = () => {
    setIsSessionConfigModalOpen(true)
  }

  const showStartConfigModal = () => {
    setIsSessionStartConfigModalOpen(true)
  }

  return (
    <List.Item>
      <div className="flex justify-between items-center w-full">
        <a href={`session/${item?.id}`}>{item.name}</a>
        <div className="flex space-x-2">
          <Button type="default" onClick={showModal}>
            Upload dataset
          </Button>
          <Button onClick={showConfigModal} type="default">
            Set configs
          </Button>
          <Button onClick={showStartConfigModal} type="primary">
            Start Genetic Flow
          </Button>
        </div>
        <SessionItemUploadModal
          isModalOpen={isUploadModalOpen}
          setIsModalOpen={setIsUploadModalOpen}
          item={item}
        />
        <SessionItemConfigModal
          item={item}
          isModalOpen={isSessionConfigModalOpen}
          setIsModalOpen={setIsSessionConfigModalOpen}
        />
        <SessionStartConfigModal
          isModalOpen={isSessionStartConfigModalOpen}
          setIsModalOpen={setIsSessionStartConfigModalOpen}
          item={item}
        />
      </div>
    </List.Item>
  )
}

SessionItem.propTypes = {
  item: PropTypes.object
}

export default SessionItem
