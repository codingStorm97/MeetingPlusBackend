import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  envDir: '..',
  plugins: [react()],
  server: {
    proxy: {
      '/api': {
        target: 'https://localhost:9011',
        changeOrigin: true,
        secure: false,
      },
    },
  },
})
