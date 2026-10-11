// Builds the stylesheets into ../wwwroot, without a CSS compiler: the sources are plain CSS, and
// native nesting and the colour functions stay as written (see the browser baseline in the readme).
//
//   stellar-admin.css           the layer order, reset.css, tokens.css, structure.css and every
//                               components/*.css file in name order, so a component never relies on
//                               another's file order and out-ranks another component's part by
//                               selector instead
//   presets/<name>.css          each css/presets/*.css, as it is
//   stellar-admin.tailwind.css  css/tailwind-adapter.css, as it is
//   stellar-admin.knobs.json    css/knobs.json, the knob manifest for tools such as the website's theme
//                               builder, after checking it against tokens.css and the presets
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

write("stellar-admin.knobs.json", JSON.stringify(checkKnobs(JSON.parse(read("knobs.json"))), null, 2) + "\n");

// Fails the build when the manifest and the stylesheet disagree: every knob declared on the knob :root
// block of tokens.css (above its fixed tokens) has an entry with the same default, every entry names a
// knob, optional knobs are read through a var() fallback, and the presets set only knobs, with values a
// choice offers.
function checkKnobs(manifest) {
  const errors = [];
  const normalize = (value) => value.replace(/\s+/g, " ").trim();
  const declarations = (text) =>
    new Map(
      [...text.replace(/\/\*[\s\S]*?\*\//g, "").matchAll(/(--sa-[\w-]+)\s*:\s*([^;]+);/g)].map((m) => [m[1], normalize(m[2])]),
    );

  const tokens = read("tokens.css");
  const start = tokens.indexOf(":root {", tokens.indexOf("@layer sa.tokens {"));
  const block = tokens.slice(start, tokens.indexOf("\n  }", start));
  const fixedAt = block.indexOf("/* Fixed tokens */");
  if (start < 0 || fixedAt < 0) throw new Error("knobs: cannot find the knob block in tokens.css");
  const knobs = declarations(block.slice(0, fixedAt));
  const fixed = declarations(block.slice(fixedAt));
  const sources = [tokens, read("structure.css"), ...cssFiles("components").map((file) => read(`components/${file}`))].join("\n");

  const groups = new Set(manifest.groups.map((group) => group.id));
  const entries = new Map();
  for (const knob of manifest.knobs) {
    if (entries.has(knob.name)) errors.push(`${knob.name}: listed twice`);
    entries.set(knob.name, knob);
    if (!groups.has(knob.group)) errors.push(`${knob.name}: unknown group ${knob.group}`);
    if (!["color", "number", "length", "choice", "font", "switch"].includes(knob.type)) errors.push(`${knob.name}: unknown type ${knob.type}`);
    if ((knob.type === "choice" || knob.type === "font") && !knob.choices?.length) errors.push(`${knob.name}: no choices`);
    if ((knob.type === "number" || knob.type === "length") && [knob.min, knob.max, knob.step].some((n) => typeof n !== "number"))
      errors.push(`${knob.name}: needs min, max and step`);

    if (knob.optional) {
      if ("default" in knob) errors.push(`${knob.name}: an optional knob has no default`);
      if (knobs.has(knob.name) || fixed.has(knob.name)) errors.push(`${knob.name}: optional, but tokens.css gives it a default`);
      if (!sources.includes(`var(${knob.name},`)) errors.push(`${knob.name}: optional, but no stylesheet reads it through a var() fallback`);
      if (!manifest.knobs.some((other) => other.name === knob.follows)) errors.push(`${knob.name}: follows unknown knob ${knob.follows}`);
      if (knob.initial === undefined) errors.push(`${knob.name}: an optional knob needs an initial value`);
    } else {
      const declared = knobs.get(knob.name) ?? fixed.get(knob.name);
      if (declared === undefined) errors.push(`${knob.name}: not a knob in tokens.css`);
      else if (normalize(knob.default) !== declared) errors.push(`${knob.name}: default ${knob.default}, tokens.css has ${declared}`);
      if (knob.choices && !knob.choices.some((choice) => choice.value === knob.default)) errors.push(`${knob.name}: the default is not a choice`);
    }
  }
  for (const name of knobs.keys()) if (!entries.has(name)) errors.push(`${name}: a knob in tokens.css without a manifest entry`);

  for (const file of cssFiles("presets")) {
    for (const [name, value] of declarations(read(`presets/${file}`))) {
      const knob = entries.get(name);
      if (!knob) errors.push(`presets/${file}: ${name} is not in the manifest`);
      else if (knob.choices && !knob.choices.some((choice) => choice.value === value)) errors.push(`presets/${file}: ${name}: ${value} is not a choice`);
      else if (knob.type === "switch" && value !== "0" && value !== "1") errors.push(`presets/${file}: ${name}: ${value} is not 0 or 1`);
    }
  }

  if (errors.length) throw new Error(`knobs.json disagrees with the stylesheets:\n  ${errors.join("\n  ")}`);
  return manifest;
}
