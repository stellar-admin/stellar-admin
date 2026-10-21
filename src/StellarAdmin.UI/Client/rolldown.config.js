import { defineConfig } from "rolldown";

// One minified ES module holds every component, so pages load a single file.
export default defineConfig({
  input: "js/stellar-admin.js",
  output: {
    file: "../wwwroot/stellar-admin.js",
    format: "esm",
    minify: true,
    sourcemap: true,
  },
});
