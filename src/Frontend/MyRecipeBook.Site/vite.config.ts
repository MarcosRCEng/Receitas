import react from '@vitejs/plugin-react';
import { defineConfig } from 'vite';

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@app': '/src/app',
      '@assets': '/src/assets',
      '@features': '/src/features',
      '@shared': '/src/shared',
    },
  },
});
