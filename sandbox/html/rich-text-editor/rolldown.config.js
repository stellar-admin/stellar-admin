import { copyFileSync, mkdirSync } from "node:fs";
import { defineConfig } from "rolldown";

mkdirSync("dist", { recursive: true });
copyFileSync("node_modules/prosekit/dist/basic/style.css", "dist/prosekit.css");

export default defineConfig({
  input: "editor.ts",
  output: { file: "dist/rich-text-editor.js", format: "iife", minify: true, sourcemap: true },
});
