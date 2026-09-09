import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// Dev server proxies API calls to the ASP.NET host so the browser makes same-origin
// requests (no CORS in dev, no base-URL configuration in the client).
// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/accounts': 'http://localhost:5078',
      '/transfers': 'http://localhost:5078',
    },
  },
})
