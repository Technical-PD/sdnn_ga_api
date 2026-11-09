// Тема Ant Design для всього застосунку. Системні шрифти — застосунок працює без інтернету.
const theme = {
  token: {
    colorPrimary: '#4f46e5',
    colorInfo: '#4f46e5',
    colorSuccess: '#16a34a',
    colorWarning: '#d97706',
    colorError: '#dc2626',
    colorBgLayout: '#f4f5fa',
    borderRadius: 8,
    fontFamily:
      "'Segoe UI Variable Text', 'Segoe UI', system-ui, -apple-system, Roboto, 'Helvetica Neue', Arial, sans-serif"
  },
  components: {
    Layout: {
      siderBg: '#ffffff',
      bodyBg: '#f4f5fa'
    },
    Menu: {
      itemBorderRadius: 8,
      itemSelectedBg: '#eef2ff',
      itemSelectedColor: '#4338ca'
    },
    Card: {
      paddingLG: 20
    },
    Table: {
      headerBg: '#f8fafc'
    }
  }
}

export default theme
