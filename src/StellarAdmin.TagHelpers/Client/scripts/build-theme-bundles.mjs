// Builds the per-theme prebuilt bundles: one self-contained stylesheet per theme, compiled into
// ../wwwroot/stellar-admin.<theme>.css. The theme list is derived from css/themes/, and the
// per-theme entry (css/base.css + the theme file) is synthesized in a temporary directory;
// there are no checked-in entry files. Generate upstream themes with
// util/ThemeGenerator; author custom themes directly. Register each theme in
// ClientOutput and util/theme-coverage/coverage.json. It also builds the token-driven
// ../wwwroot/stellar-admin.css from its checked-in entry, css/stellar-admin.css.
//
//   node ./scripts/build-theme-bundles.mjs

import { check } from "../../../../util/theme-coverage/check.mjs";
import { spawn } from "node:child_process";
import { mkdtempSync, readdirSync, rmSync, statSync, writeFileSync } from "node:fs";
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
    buildBundle("tokens", "css/stellar-admin.css", "../wwwroot/stellar-admin.css").then(() =>
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
