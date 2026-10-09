// Builds the per-theme prebuilt bundles: one self-contained stylesheet per theme, compiled into
// ../wwwroot/stellar-admin.<theme>.css. The theme list is derived from css/themes/, and the
// per-theme entry (css/base.css + the theme file) is synthesized in a temporary directory;
// there are no checked-in entry files. Generate upstream themes with
// util/ThemeGenerator; author custom themes directly. Register each theme in
// ClientOutput and util/theme-coverage/coverage.json. It also builds the token-driven
// ../wwwroot/stellar-admin.css from its checked-in entry, css/stellar-admin.css: converted
// components (css/components/*.css) name the selectors they replace in a "Replaces:" header, and
// that bundle gets copies of components.css and the theme file without those rules.
//
//   node ./scripts/build-theme-bundles.mjs

import { check } from "../../../../util/theme-coverage/check.mjs";
import { spawn } from "node:child_process";
import { mkdtempSync, readdirSync, readFileSync, rmSync, statSync, writeFileSync } from "node:fs";
import { basename, resolve } from "node:path";

const clientRoot = resolve(import.meta.dirname, "..");

const coverage = check(resolve(clientRoot, "../../.."));
if (coverage.errors.length) throw new Error(coverage.errors.join("\n"));

const themes = readdirSync(resolve(clientRoot, "css/themes"))
  .filter((file) => file.endsWith(".css"))
  .map((file) => basename(file, ".css"))
  .sort();

// Remove pre-namespace bundles on incremental builds, unless a custom theme now owns the name.
for (const legacy of ["luma", "lyra", "maia", "mira", "nova", "rhea", "sera", "vega"]) {
  if (!themes.includes(legacy))
    rmSync(resolve(clientRoot, `../wwwroot/stellar-admin.${legacy}.css`), { force: true });
}

const entriesFolder = mkdtempSync(resolve(clientRoot, ".theme-build-"));

function buildTheme(theme) {
  // File inputs avoid stdin forwarding failures in process launchers. Imports resolve
  // relative to this temporary directory, which is removed after all builds finish.
  const entry = resolve(entriesFolder, `${theme}.css`);
  writeFileSync(entry, `@import "../css/base.css";\n@import "../css/themes/${theme}.css";\n`);
  return buildBundle(theme, entry, `../wwwroot/stellar-admin.${theme}.css`);
}

// Removes each rule whose selectors all start with a replaced class (`.sa-switch:checked ~ …` starts
// with .sa-switch). Rules are matched on a line of their own (`  .sa-x, .sa-y {`), as the generated
// theme files and components.css write them; anything else is kept.
function withoutRules(css, replaced) {
  const lines = css.split("\n");
  const kept = [];
  for (let i = 0; i < lines.length; i++) {
    const match = /^\s*([^{}@/]+?)\s*\{\s*$/.exec(lines[i]);
    const selectors = match?.[1].split(",").map((selector) => selector.trim());
    if (selectors?.every((selector) => replaced.has(/^\.[\w-]+/.exec(selector)?.[0]))) {
      for (let depth = 0; i < lines.length; i++) {
        depth += (lines[i].match(/\{/g) ?? []).length - (lines[i].match(/\}/g) ?? []).length;
        if (depth === 0) break;
      }
      continue;
    }
    kept.push(lines[i]);
  }
  return kept.join("\n");
}

function buildTokensBundle() {
  const css = resolve(clientRoot, "css");
  const replaced = new Set(
    readdirSync(resolve(css, "components"))
      .filter((file) => file.endsWith(".css"))
      .flatMap((file) => {
        const header = /Replaces:([^*]*)\*\//.exec(readFileSync(resolve(css, "components", file), "utf8"));
        return header ? header[1].split(/[\s,]+/).filter(Boolean) : [];
      }),
  );
  const legacy = { "components.css": "components.css", "themes/shadcn.nova.css": "nova.css" };
  for (const [source, copy] of Object.entries(legacy))
    writeFileSync(
      resolve(entriesFolder, copy),
      withoutRules(readFileSync(resolve(css, source), "utf8"), replaced),
    );
  const entry = readFileSync(resolve(css, "stellar-admin.css"), "utf8").replace(
    /@import "\.\/([^"]+)";/g,
    (_, path) => `@import "${legacy[path] ? `./${legacy[path]}` : `../css/${path}`}";`,
  );
  writeFileSync(resolve(entriesFolder, "stellar-admin.css"), entry);
  return buildBundle("tokens", resolve(entriesFolder, "stellar-admin.css"), "../wwwroot/stellar-admin.css");
}

function buildBundle(name, entry, outputPath) {
  return new Promise((resolvePromise, rejectPromise) => {
    const child = spawn(
      process.platform === "win32" ? "npx.cmd" : "npx",
      ["@tailwindcss/cli", "-i", entry, "-o", outputPath],
      { cwd: clientRoot, stdio: "inherit" },
    );
    child.on("error", rejectPromise);
    child.on("close", (code) => {
      if (code === 0) {
        const output = resolve(clientRoot, outputPath);
        if (!statSync(output, { throwIfNoEntry: false })?.size) {
          rejectPromise(
            new Error(`theme-bundles: ${name} produced an empty or missing stylesheet`),
          );
          return;
        }
        resolvePromise();
      } else {
        rejectPromise(new Error(`theme-bundles: ${name} failed with exit code ${code}`));
      }
    });
  });
}

try {
  const results = await Promise.allSettled([
    ...themes.map(async (theme) => {
      await buildTheme(theme);
      console.log(`theme-bundles: ${theme} -> wwwroot/stellar-admin.${theme}.css`);
    }),
    buildTokensBundle().then(() =>
      console.log("theme-bundles: tokens -> wwwroot/stellar-admin.css"),
    ),
  ]);
  const failures = results.filter((result) => result.status === "rejected");
  if (failures.length)
    throw new AggregateError(
      failures.map((result) => result.reason),
      "Theme bundle build failed",
    );
} finally {
  rmSync(entriesFolder, { recursive: true, force: true });
}
