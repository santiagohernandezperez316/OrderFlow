/** @type {import('tailwindcss').Config} */
export default {
  content: ["./index.html", "./src/**/*.{ts,tsx}"],
  theme: {
    extend: {
      colors: {
        canvas: "#F6F5F1",
        ink: "#20242B",
        line: "#D8D5CC",
        accent: {
          pending: "#1F4E5F",
          confirmed: "#2F6F4E",
          rejected: "#A23B2E",
        },
      },
      fontFamily: {
        sans: ['"IBM Plex Sans"', "sans-serif"],
        mono: ['"IBM Plex Mono"', "monospace"],
      },
    },
  },
  plugins: [],
};
