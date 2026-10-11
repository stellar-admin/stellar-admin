// Builds the stylesheets into ../wwwroot, without a CSS compiler: the sources are plain CSS, and
// native nesting and the colour functions stay as written (see the browser baseline in the readme).
//
//   stellar-admin.css                   the layer order, reset.css, tokens.css, structure.css and
//                                       every components/*.css file in name order, so a component
//                                       never relies on another's file order and out-ranks another
//                                       component's part by selector instead
//   stellar-admin.tailwind.css          css/tailwind-adapter.css, as it is
//   stellar-admin.theme-variables.json  css/theme-variables.json, the theme variable manifest for
//                                       tools such as the website's theme builder, after checking it
//                                       against tokens.css
//
//   node ./scripts/build-css.mjs

import { readdirSync, readFileSync, rmSync, writeFileSync } from "node:fs";
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

// The presets were shipped from wwwroot/presets until the theme builder replaced them.
rmSync(resolve(wwwroot, "presets"), { recursive: true, force: true });

write("stellar-admin.tailwind.css", read("tailwind-adapter.css"));

write("stellar-admin.theme-variables.json", JSON.stringify(checkThemeVariables(JSON.parse(read("theme-variables.json"))), null, 2) + "\n");

// Fails the build when the manifest and the stylesheet disagree: every theme variable declared on the
// theme variable :root block of tokens.css (above its fixed tokens) has an entry with the same default,
// every entry names a theme variable, and optional ones are read through a var() fallback. `essential`
// marks the few a tool shows first (the builder's simple mode); only required ones can be essential.
function checkThemeVariables(manifest) {
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
  if (start < 0 || fixedAt < 0) throw new Error("variables: cannot find the variable block in tokens.css");
  const variables = declarations(block.slice(0, fixedAt));
  const fixed = declarations(block.slice(fixedAt));
  const sources = [tokens, read("structure.css"), ...cssFiles("components").map((file) => read(`components/${file}`))].join("\n");

  const groups = new Set(manifest.groups.map((group) => group.id));
  const entries = new Map();
  for (const variable of manifest.variables) {
    if (entries.has(variable.name)) errors.push(`${variable.name}: listed twice`);
    entries.set(variable.name, variable);
    if (!groups.has(variable.group)) errors.push(`${variable.name}: unknown group ${variable.group}`);
    if (!["color", "number", "length", "choice", "font", "switch"].includes(variable.type)) errors.push(`${variable.name}: unknown type ${variable.type}`);
    if ((variable.type === "choice" || variable.type === "font") && !variable.choices?.length) errors.push(`${variable.name}: no choices`);
    if ("essential" in variable && (variable.essential !== true || variable.optional)) errors.push(`${variable.name}: essential must be true, and only on a required variable`);
    if ((variable.type === "number" || variable.type === "length") && [variable.min, variable.max, variable.step].some((n) => typeof n !== "number"))
      errors.push(`${variable.name}: needs min, max and step`);

    if (variable.optional) {
      if ("default" in variable) errors.push(`${variable.name}: an optional variable has no default`);
      if (variables.has(variable.name) || fixed.has(variable.name)) errors.push(`${variable.name}: optional, but tokens.css gives it a default`);
      if (!sources.includes(`var(${variable.name},`)) errors.push(`${variable.name}: optional, but no stylesheet reads it through a var() fallback`);
      if (!manifest.variables.some((other) => other.name === variable.follows)) errors.push(`${variable.name}: follows unknown variable ${variable.follows}`);
      if (variable.initial === undefined) errors.push(`${variable.name}: an optional variable needs an initial value`);
    } else {
      const declared = variables.get(variable.name) ?? fixed.get(variable.name);
      if (declared === undefined) errors.push(`${variable.name}: not a variable in tokens.css`);
      else if (normalize(variable.default) !== declared) errors.push(`${variable.name}: default ${variable.default}, tokens.css has ${declared}`);
      if (variable.choices && !variable.choices.some((choice) => choice.value === variable.default)) errors.push(`${variable.name}: the default is not a choice`);
    }
  }
  if (!manifest.variables.some((variable) => variable.essential)) errors.push("no variable is essential");
  for (const name of variables.keys()) if (!entries.has(name)) errors.push(`${name}: a variable in tokens.css without a manifest entry`);

  if (errors.length) throw new Error(`theme-variables.json disagrees with the stylesheets:\n  ${errors.join("\n  ")}`);
  return manifest;
}
