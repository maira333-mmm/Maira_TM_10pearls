/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      animation: {
        fadeIn: 'fadeIn 0.6s ease-in-out',
        fadeInOut: 'fadeInOut 4s ease-in-out forwards',
      },
      keyframes: {
        fadeIn: {
          '0%, 49.99%': {
            opacity: '0',
            zIndex: '1',
          },
          '50%, 100%': {
            opacity: '1',
            zIndex: '5',
          },
        },
        fadeInOut: {
          '0%': {
            opacity: '0',
            transform: 'translateX(-50%) translateY(10px)',
          },
          '10%, 90%': {
            opacity: '1',
            transform: 'translateX(-50%) translateY(0)',
          },
          '100%': {
            opacity: '0',
            transform: 'translateX(-50%) translateY(-10px)',
          },
        },
      },
    },
  },
  plugins: [],
}
