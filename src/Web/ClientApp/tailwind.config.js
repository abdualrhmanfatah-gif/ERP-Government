import tokens from './src/design-system/tokens.tailwind.json' with { type: 'json' };

/** @type {import('tailwindcss').Config} */
export default {
  darkMode: ['class'],

  content: [
    './index.html',
    './src/**/*.{js,ts,jsx,tsx}',
  ],

  theme: {
    container: {
      center: true,
      padding: '2rem',
      screens: {
        '2xl': '1400px',
      },
    },

    extend: {
      // ── All values consumed from tokens.tailwind.json (generated from tokens.ts)
      colors: tokens.colors,
      fontFamily: tokens.fontFamily,
      fontSize: tokens.fontSize,
      spacing: tokens.spacing,
      borderRadius: tokens.borderRadius,
      width: tokens.width,
      height: tokens.height,
      maxWidth: tokens.maxWidth,
      minHeight: tokens.minHeight,
      boxShadow: tokens.boxShadow,
      transitionDuration: tokens.transitionDuration,
      transitionTimingFunction: tokens.transitionTimingFunction,
      zIndex: tokens.zIndex,
    },
  },

  plugins: [],
};
