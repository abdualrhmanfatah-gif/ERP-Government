import path from 'path';
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

const target =
  process.env['services__webapi__https__0'] ||
  process.env['services__webapi__http__0'] ||
  'http://localhost:5226';

const proxyOptions = { target, secure: false, changeOrigin: true };

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  server: {
    port: parseInt(process.env.PORT!),
    proxy: {
      '/api': proxyOptions,
      '/openapi': proxyOptions,
      '/scalar': proxyOptions,
    },
  },
  build: {
    outDir: 'build',
    cssMinify: false,
  },
});
