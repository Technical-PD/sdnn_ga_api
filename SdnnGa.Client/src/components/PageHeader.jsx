import { Breadcrumb, Typography } from 'antd'
import { Link } from 'react-router-dom'
import PropTypes from 'prop-types'

// Заголовок сторінки: "хлібні крихти", назва, підзаголовок і дії праворуч.
const PageHeader = ({ breadcrumbs, title, tags, subtitle, actions }) => (
  <div className="mb-6">
    {breadcrumbs?.length > 0 && (
      <Breadcrumb
        className="mb-3"
        items={breadcrumbs.map(crumb => ({
          title: crumb.to ? <Link to={crumb.to}>{crumb.title}</Link> : crumb.title
        }))}
      />
    )}
    <div className="flex flex-wrap items-start justify-between gap-4">
      <div className="min-w-0">
        <div className="flex flex-wrap items-center gap-3">
          <Typography.Title level={3} className="!m-0">
            {title}
          </Typography.Title>
          {tags}
        </div>
        {subtitle && (
          <Typography.Paragraph type="secondary" className="!mt-1 !mb-0 max-w-3xl">
            {subtitle}
          </Typography.Paragraph>
        )}
      </div>
      {actions && <div className="flex flex-wrap items-center gap-2">{actions}</div>}
    </div>
  </div>
)

PageHeader.propTypes = {
  breadcrumbs: PropTypes.arrayOf(PropTypes.shape({ title: PropTypes.node, to: PropTypes.string })),
  title: PropTypes.node,
  tags: PropTypes.node,
  subtitle: PropTypes.node,
  actions: PropTypes.node
}

export default PageHeader
