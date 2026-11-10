import { theme as antTheme } from 'antd'

// Тема Ant Design для світлого та темного режимів. Системні шрифти — застосунок працює без інтернету.
const FONT =
  "'Segoe UI Variable Text', 'Segoe UI', system-ui, -apple-system, Roboto, 'Helvetica Neue', Arial, sans-serif"

export const buildTheme = isDark => ({
  algorithm: isDark ? antTheme.darkAlgorithm : antTheme.defaultAlgorithm,
  token: {
    colorPrimary: isDark ? '#6366f1' : '#4f46e5',
    colorInfo: isDark ? '#6366f1' : '#4f46e5',
    colorSuccess: isDark ? '#22c55e' : '#16a34a',
    colorWarning: '#d97706',
    colorError: isDark ? '#ef4444' : '#dc2626',
    colorBgBase: isDark ? '#0b0f17' : '#ffffff',
    colorBgLayout: isDark ? '#0b0f17' : '#f4f5fa',
    borderRadius: 8,
    fontFamily: FONT
  },
  components: {
    Menu: {
      itemBorderRadius: 8,
      ...(isDark ? {} : { itemSelectedBg: '#eef2ff', itemSelectedColor: '#4338ca' })
    },
    Card: {
      paddingLG: 20
    },
    Table: isDark ? {} : { headerBg: '#f8fafc' }
  }
})
