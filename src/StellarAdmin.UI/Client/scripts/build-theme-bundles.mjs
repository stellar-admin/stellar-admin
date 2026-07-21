// Builds the per-theme prebuilt bundles: one self-contained stylesheet per theme, compiled from
// css/themes/<theme>.css into ../wwwroot/stellar-admin-ui.<theme>.css. The theme list is derived
// from the entry files, so adding a theme is: generate tailwind/themes/<theme>.css with
// util/ThemePackGenerator, add the css/themes/<theme>.css entry, and add it to ClientOutput in
// StellarAdmin.UI.csproj.
//
//   node ./scripts/build-theme-bundles.mjs

import { execFile } from "node:child_process";
import { readdirSync } from "node:fs";
import { basename, resolve } from "node:path";
import { promisify } from "node:util";

const execFileAsync = promisify(execFile);
const clientRoot = resolve(import.meta.dirname, "..");

const themes = readdirSync(resolve(clientRoot, "css/themes"))
  .filter((file) => file.endsWith(".css"))
  .map((file) => basename(file, ".css"))
  .sort();

await Promise.all(
  themes.map(async (theme) => {
    await execFileAsync(
      process.platform === "win32" ? "npx.cmd" : "npx",
      [
        "@tailwindcss/cli",
        "-i",
        `./css/themes/${theme}.css`,
        "-o",
        `../wwwroot/stellar-admin-ui.${theme}.css`,
      ],
      { cwd: clientRoot },
    );
    console.log(`theme-bundles: ${theme} -> wwwroot/stellar-admin-ui.${theme}.css`);
  }),
);
