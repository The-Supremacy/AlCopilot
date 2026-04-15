import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import path from 'node:path';

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  server: {
    host: process.env.MANAGEMENT_PORTAL_HOST ?? 'localhost',
    port: 4173,
    proxy: {
      '/api': {
        target: process.env.MANAGEMENT_API_PROXY_TARGET ?? 'http://localhost:5243',
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
