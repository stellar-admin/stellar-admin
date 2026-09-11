// Builds the per-theme prebuilt bundles: one self-contained stylesheet per theme, compiled into
// ../wwwroot/stellar-admin.<theme>.css. The theme list is derived from css/themes/, and the
// per-theme entry (css/base.css + the theme file) is synthesized here and fed to the Tailwind CLI
// over stdin — there are no checked-in entry files. Generate upstream themes with
// util/ThemeGenerator; author custom themes directly. Register each theme in
// ClientOutput and util/theme-coverage/coverage.json.
//
//   node ./scripts/build-theme-bundles.mjs

import { check } from "../../../../util/theme-coverage/check.mjs";
import { spawn } from "node:child_process";
import { readdirSync, statSync } from "node:fs";
import { basename, resolve } from "node:path";

const clientRoot = resolve(import.meta.dirname, "..");

const coverage = check(resolve(clientRoot, "../../.."));
if (coverage.errors.length) throw new Error(coverage.errors.join("\n"));

const themes = readdirSync(resolve(clientRoot, "css/themes"))
  .filter((file) => file.endsWith(".css"))
  .map((file) => basename(file, ".css"))
  .sort();

function buildTheme(theme) {
  // Relative imports in a stdin entry resolve against cwd (clientRoot).
  const entry = `@import "./css/base.css";\n@import "./css/themes/${theme}.css";\n`;

  return new Promise((resolvePromise, rejectPromise) => {
    const child = spawn(
      process.platform === "win32" ? "npx.cmd" : "npx",
      ["@tailwindcss/cli", "-i", "-", "-o", `../wwwroot/stellar-admin.${theme}.css`],
      { cwd: clientRoot, stdio: ["pipe", "inherit", "inherit"] },
    );
    child.on("error", rejectPromise);
    child.on("close", (code) => {
      if (code === 0) {
        const output = resolve(clientRoot, `../wwwroot/stellar-admin.${theme}.css`);
        if (!statSync(output, { throwIfNoEntry: false })?.size) {
          rejectPromise(
            new Error(`theme-bundles: ${theme} produced an empty or missing stylesheet`),
          );
          return;
        }
        resolvePromise();
      } else {
        rejectPromise(new Error(`theme-bundles: ${theme} failed with exit code ${code}`));
      }
    });
    child.stdin.write(entry);
    child.stdin.end();
  });
}

await Promise.all(
  themes.map(async (theme) => {
    await buildTheme(theme);
    console.log(`theme-bundles: ${theme} -> wwwroot/stellar-admin.${theme}.css`);
  }),
);
