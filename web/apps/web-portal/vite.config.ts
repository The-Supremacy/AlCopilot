import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';
import path from 'node:path';

export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  server: {
    host: process.env.WEB_PORTAL_HOST ?? 'localhost',
    port: Number.parseInt(process.env.PORT ?? '', 10) || 4174,
    proxy: {
      '/api': {
        target: process.env.WEB_PORTAL_API_PROXY_TARGET ?? 'http://localhost:5243',
        changeOrigin: false,
        secure: false,
      },
    },
  },
  test: {
    environment: 'jsdom',
    globals: true,
    setupFiles: './src/test/setup.ts',
  },
});
