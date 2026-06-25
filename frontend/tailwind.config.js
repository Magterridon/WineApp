/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ['./index.html', './src/**/*.{vue,js,ts}'],
  theme: {
    extend: {
      fontFamily: {
        heading: ['Fraunces', 'Georgia', 'serif'],
        sans: ['Inter', 'system-ui', 'sans-serif'],
      },
      colors: {
        wine: {
          50:  '#fdf2f4',
          100: '#fce7eb',
          200: '#f8c9d2',
          300: '#f49db0',
          400: '#ed6a87',
          500: '#e03e65',
          600: '#c92150',
          700: '#a91844',
          800: '#722f37',
          900: '#4a1020',
          950: '#2d0912',
        },
      },
    },
  },
  plugins: [require('daisyui')],
  daisyui: {
    themes: [
      {
        wine: {
          primary:           '#4a1020',
          'primary-content': '#ffffff',
          secondary:         '#722f37',
          'secondary-content': '#ffffff',
          accent:            '#c7956c',
          'accent-content':  '#ffffff',
          neutral:           '#2a2a2a',
          'neutral-content': '#ffffff',
          'base-100':        '#faf8f5',
          'base-200':        '#f0ece6',
          'base-300':        '#e0d8ce',
          'base-content':    '#1a1a1a',
          info:              '#3b82f6',
          success:           '#22c55e',
          warning:           '#f59e0b',
          error:             '#ef4444',
        },
      },
    ],
    darkTheme: false,
    base: true,
    styled: true,
    utils: true,
    logs: false,
  },
}
