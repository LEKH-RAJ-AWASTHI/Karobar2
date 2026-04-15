/** @type {import('tailwindcss').Config} */
module.exports = {
  darkMode: "class",
  content: [
    "./src/**/*.{html,ts}",
  ],
  theme: {
    extend: {
      "colors": {
              "primary-fixed-dim": "#c3c0ff",
              "surface-tint": "#4d44e3",
              "primary": "#3525cd",
              "on-tertiary-fixed-variant": "#7b2f00",
              "inverse-primary": "#c3c0ff",
              "surface-bright": "#f8f9ff",
              "on-surface": "#0b1c30",
              "error": "#ba1a1a",
              "secondary-fixed-dim": "#c3c0ff",
              "surface-container": "#e5eeff",
              "outline-variant": "#c7c4d8",
              "secondary-fixed": "#e2dfff",
              "background": "#f8f9ff",
              "on-error": "#ffffff",
              "surface": "#f8f9ff",
              "on-error-container": "#93000a",
              "on-tertiary-fixed": "#351000",
              "tertiary-fixed-dim": "#ffb695",
              "primary-fixed": "#e2dfff",
              "on-surface-variant": "#464555",
              "primary-container": "#4f46e5",
              "secondary": "#58579b",
              "tertiary-fixed": "#ffdbcc",
              "on-primary-container": "#dad7ff",
              "on-primary-fixed": "#0f0069",
              "on-primary-fixed-variant": "#3323cc",
              "tertiary": "#7e3000",
              "on-tertiary": "#ffffff",
              "surface-dim": "#cbdbf5",
              "on-secondary-container": "#454386",
              "secondary-container": "#b6b4ff",
              "on-primary": "#ffffff",
              "surface-variant": "#d3e4fe",
              "on-secondary-fixed-variant": "#413f82",
              "surface-container-lowest": "#ffffff",
              "on-secondary-fixed": "#140f54",
              "tertiary-container": "#a44100",
              "on-tertiary-container": "#ffd2be",
              "on-background": "#0b1c30",
              "on-secondary": "#ffffff",
              "inverse-surface": "#213145",
              "inverse-on-surface": "#eaf1ff",
              "surface-container-low": "#eff4ff",
              "surface-container-high": "#dce9ff",
              "outline": "#777587",
              "error-container": "#ffdad6"
      },
      "fontFamily": {
              "headline": ["Inter"],
              "body": ["Inter"],
              "label": ["Inter"],
              "sans": ["Inter", "sans-serif"]
      }
    },
  },
  plugins: [
    require('@tailwindcss/forms'),
    require('@tailwindcss/container-queries')
  ],
}
