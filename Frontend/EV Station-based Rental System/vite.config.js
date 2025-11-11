import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'
import tailwindcss from '@tailwindcss/vite'

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  const allowed = (env.VITE_ALLOWED_HOSTS || '').split(',').map(s => s.trim()).filter(Boolean)

  return {
    plugins: [react(), tailwindcss()],
    resolve: { alias: { '@': path.resolve(__dirname, './src') } },
    server: {
      host: true,
      port: 5173,
      strictPort: true,
      cors: true,
      allowedHosts: allowed,   // đọc từ env
      hmr: { clientPort: 443 },
    },
  }
})
