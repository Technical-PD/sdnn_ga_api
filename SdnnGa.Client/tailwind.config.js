// Кольори, що використовуються в компонентах, беруться з CSS-змінних (src/styles/palette.css),
// тому ті самі класи (bg-slate-50, text-indigo-600, …) автоматично адаптуються до темної теми.
const STEPS = [50, 100, 200, 300, 400, 500, 600, 700, 800, 900, 950]
const themed = name => Object.fromEntries(STEPS.map(step => [step, `rgb(var(--${name}-${step}) / <alpha-value>)`]))

/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        slate: themed('slate'),
        indigo: themed('indigo'),
        emerald: themed('emerald'),
        amber: themed('amber'),
        rose: themed('rose')
      }
    }
  },
  plugins: []
}
