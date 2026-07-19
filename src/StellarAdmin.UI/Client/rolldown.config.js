import { defineConfig } from "rolldown";
import minifyHTMLPlugin from "rollup-plugin-minify-html-literals";
const minifyHTML = minifyHTMLPlugin.default;

export default defineConfig({
  input: "js/stellar-admin-ui.ts",
  output: {
    file: "../wwwroot/stellar-admin-ui.js",
    format: "iife",
    minify: true,
    sourcemap: true,
  },
  plugins: [
    // Minifies the HTML/CSS inside the tagged templates
    minifyHTML(),
  ],
  // This tells Rolldown to treat the TS files correctly
  resolve: {
    extensions: [".ts", ".js"],
  },
});
