import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  build: {
    // Зібраний клієнт віддається десктопним застосунком (SdnnGa.Api) з wwwroot.
    outDir: '../SdnnGa.Api/SdnnGa.Api/wwwroot',
    emptyOutDir: true
  },
  server: {
    // Для `npm run dev`: бекенд запускається командою `dotnet run --project SdnnGa.Api -- --headless`.
    proxy: {
      '/api': 'http://127.0.0.1:5080'
    }
  }
})
