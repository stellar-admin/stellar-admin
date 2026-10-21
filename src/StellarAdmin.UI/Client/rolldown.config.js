import { defineConfig } from "rolldown";

// One minified ES module holds every component, so pages load a single file. Large, rarely used
// components are separate builds, each one self-contained module with no chunks shared with the
// main bundle, that their tag helper loads only on pages that render them.
const output = (file) => ({
  file: `../wwwroot/${file}`,
  format: "esm",
  minify: true,
  sourcemap: true,
});

export default defineConfig([
  { input: "js/stellar-admin.js", output: output("stellar-admin.js") },
  { input: "js/rich-text-editor/rich-text-editor.js", output: output("rich-text-editor.js") },
]);
