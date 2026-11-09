import { Alert, App, Button, Col, Row, Table, Tag, Typography, Upload } from 'antd'
import { CheckCircleFilled, FileTextOutlined, InboxOutlined, UploadOutlined } from '@ant-design/icons'
import { useState } from 'react'
import PropTypes from 'prop-types'
import apiCall from '../../utils/apiCall'
import { inspectCsv } from '../../utils/csv'

const SIDES = {
  x: {
    title: 'Features (X)',
    hint: 'Inputs of the network: one column per feature, one row per sample.',
    endpoint: 'AddXData'
  },
  y: {
    title: 'Targets (Y)',
    hint: 'What the network should predict, in the same row order as X (for classes use one-hot columns).',
    endpoint: 'AddYData'
  }
}

const MAX_HEADER_TAGS = 10

const DatasetPreview = ({ info }) => {
  const columns = info.headers.map((header, index) => ({
    title: header,
    dataIndex: index,
    key: index,
    ellipsis: true,
    width: 110
  }))
  const rows = info.preview.map((row, rowIndex) => ({ key: rowIndex, ...row }))

  return (
    <div>
      <div className="flex flex-wrap gap-1 mb-2">
        {info.headers.slice(0, MAX_HEADER_TAGS).map(header => (
          <Tag key={header} className="m-0">
            {header}
          </Tag>
        ))}
        {info.headers.length > MAX_HEADER_TAGS && <Tag className="m-0">+{info.headers.length - MAX_HEADER_TAGS} more</Tag>}
      </div>
      <Table
        size="small"
        columns={columns}
        dataSource={rows}
        pagination={false}
        scroll={{ x: 'max-content' }}
        bordered
      />
    </div>
  )
}

DatasetPreview.propTypes = { info: PropTypes.object.isRequired }

const DatasetSide = ({ side, current, pending, onPick, disabled }) => {
  const config = SIDES[side]
  const info = pending?.info ?? current

  return (
    <div className="rounded-lg border border-slate-200 p-4">
      <div className="flex items-center justify-between gap-2">
        <Typography.Text strong>{config.title}</Typography.Text>
        {pending ? (
          <Tag color="gold" icon={<FileTextOutlined />} className="m-0">
            New file, not uploaded yet
          </Tag>
        ) : current ? (
          <Tag color="success" icon={<CheckCircleFilled />} className="m-0">
            Uploaded
          </Tag>
        ) : null}
      </div>
      <Typography.Paragraph type="secondary" className="!mt-1 text-sm">
        {config.hint}
      </Typography.Paragraph>

      {info && (
        <div className="mb-3">
          <div className="text-sm mb-2">
            {pending && <span className="font-medium">{pending.file.name}: </span>}
            <b>{info.rows.toLocaleString()}</b> rows × <b>{info.columns}</b> columns
          </div>
          <DatasetPreview info={info} />
        </div>
      )}

      <Upload.Dragger
        accept=".csv,text/csv"
        multiple={false}
        showUploadList={false}
        disabled={disabled}
        beforeUpload={file => {
          onPick(side, file)
          return false
        }}>
        <p className="ant-upload-drag-icon !mb-1">
          <InboxOutlined />
        </p>
        <p className="ant-upload-text !text-sm">
          {info ? 'Drop a CSV here to replace it' : 'Click or drop a CSV file here'}
        </p>
        <p className="ant-upload-hint !text-xs">First row must contain column names</p>
      </Upload.Dragger>
    </div>
  )
}

DatasetSide.propTypes = {
  side: PropTypes.oneOf(['x', 'y']).isRequired,
  current: PropTypes.object,
  pending: PropTypes.object,
  onPick: PropTypes.func.isRequired,
  disabled: PropTypes.bool
}

// Крок 1: завантаження X/Y CSV з попереднім переглядом і перевіркою розмірів.
const DatasetStep = ({ sessionId, current, disabled, onUploaded }) => {
  const { message } = App.useApp()
  const [pending, setPending] = useState({ x: null, y: null })
  const [uploading, setUploading] = useState(false)

  const pick = async (side, file) => {
    const info = inspectCsv(await file.text())
    setPending(previous => ({ ...previous, [side]: { file, info } }))
  }

  const effective = {
    x: pending.x?.info ?? current.x,
    y: pending.y?.info ?? current.y
  }

  const problems = []
  if (effective.x && effective.y && effective.x.rows !== effective.y.rows) {
    problems.push(`X has ${effective.x.rows} rows but Y has ${effective.y.rows}. Every sample needs a target.`)
  }
  for (const side of ['x', 'y']) {
    const info = effective[side]
    if (info && info.rows === 0) {
      problems.push(`${SIDES[side].title} has no data rows.`)
    } else if (info && !info.isNumeric) {
      problems.push(`${SIDES[side].title} contains empty or non-numeric values. The network can only learn from numbers.`)
    }
  }

  const hasPending = Boolean(pending.x || pending.y)
  const complete = Boolean(effective.x && effective.y)
  const canUpload = hasPending && complete && problems.length === 0 && !disabled

  const upload = async () => {
    setUploading(true)
    try {
      // Послідовно: кожен запит оновлює весь запис сесії.
      for (const side of ['x', 'y']) {
        if (pending[side]) {
          const form = new FormData()
          form.append('dataForm', pending[side].file)
          const result = await apiCall(`Data/${SIDES[side].endpoint}/${sessionId}`, 'POST', form, true)
          if (result?.isError) {
            throw new Error(result.message)
          }
        }
      }
      setPending({ x: null, y: null })
      message.success('Dataset uploaded')
      await onUploaded()
    } catch (error) {
      message.error(`Upload failed: ${error.message}`)
    } finally {
      setUploading(false)
    }
  }

  return (
    <div>
      <Row gutter={[16, 16]}>
        {['x', 'y'].map(side => (
          <Col key={side} xs={24} lg={12}>
            <DatasetSide side={side} current={current[side]} pending={pending[side]} onPick={pick} disabled={disabled} />
          </Col>
        ))}
      </Row>

      {problems.map(problem => (
        <Alert key={problem} type="error" showIcon className="mt-4" message={problem} />
      ))}

      <div className="flex flex-wrap items-center justify-between gap-3 mt-5">
        <Typography.Text type="secondary" className="text-sm">
          Sample datasets (Iris, Titanic, cars) are in the <b>samples</b> folder next to SdnnGa.exe.
        </Typography.Text>
        <div className="flex gap-2">
          {hasPending && (
            <Button onClick={() => setPending({ x: null, y: null })} disabled={uploading}>
              Discard changes
            </Button>
          )}
          {hasPending || !complete ? (
            <Button type="primary" icon={<UploadOutlined />} onClick={upload} loading={uploading} disabled={!canUpload}>
              Upload and continue
            </Button>
          ) : (
            <Button type="primary" onClick={() => onUploaded()}>
              Continue
            </Button>
          )}
        </div>
      </div>
    </div>
  )
}

DatasetStep.propTypes = {
  sessionId: PropTypes.string.isRequired,
  current: PropTypes.shape({ x: PropTypes.object, y: PropTypes.object }).isRequired,
  disabled: PropTypes.bool,
  onUploaded: PropTypes.func.isRequired
}

export default DatasetStep
