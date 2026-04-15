import type { Config } from 'tailwindcss';
import themePreset from '@alcopilot/frontend-theme/tailwind-preset';

const config: Config = {
  darkMode: ['class'],
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  presets: [themePreset],
  theme: {
    extend: {},
  },
  plugins: [],
};

export default config;
