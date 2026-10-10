// Builds the stylesheets into ../wwwroot, without a CSS compiler: the sources are plain CSS, and
// native nesting and the colour functions stay as written (see the browser baseline in the readme).
//
//   stellar-admin.css           the layer order, reset.css, tokens.css, structure.css and every
//                               components/*.css file in name order, so a component never relies on
//                               another's file order and out-ranks another component's part by
//                               selector instead
//   presets/<name>.css          each css/presets/*.css, as it is
//   stellar-admin.tailwind.css  css/tailwind-adapter.css, as it is
//
//   node ./scripts/build-css.mjs

import { mkdirSync, readdirSync, readFileSync, rmSync, writeFileSync } from "node:fs";
import { resolve } from "node:path";

const css = resolve(import.meta.dirname, "../css");
const wwwroot = resolve(import.meta.dirname, "../../wwwroot");

const cssFiles = (folder) =>
  readdirSync(resolve(css, folder))
    .filter((file) => file.endsWith(".css"))
    .sort();
const read = (path) => readFileSync(resolve(css, path), "utf8").replace(/^﻿/, "");

// Writes only on change, so the build's up-to-date check and static asset fingerprints stay stable.
function write(path, content) {
  const output = resolve(wwwroot, path);
  let current;
  try {
    current = readFileSync(output, "utf8");
  } catch {}
  if (current !== content) writeFileSync(output, content);
  console.log(`css: ${path}`);
}

const header =
  "/* StellarAdmin.TagHelpers. Built from Client/css by scripts/build-css.mjs. */\n" +
  "@layer theme, base, sa.reset, sa.tokens, sa.components, components, sa.theme, sa.overrides, utilities;\n";
const sources = ["reset.css", "tokens.css", "structure.css", ...cssFiles("components").map((file) => `components/${file}`)];
write("stellar-admin.css", header + sources.map((path) => `\n/* ${path} */\n${read(path).trim()}\n`).join(""));

const presets = cssFiles("presets");
mkdirSync(resolve(wwwroot, "presets"), { recursive: true });
for (const stale of readdirSync(resolve(wwwroot, "presets")).filter((file) => !presets.includes(file)))
  rmSync(resolve(wwwroot, "presets", stale));
for (const file of presets) write(`presets/${file}`, read(`presets/${file}`));

write("stellar-admin.tailwind.css", read("tailwind-adapter.css"));
