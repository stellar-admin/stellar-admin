// Refreshes ../tailwind/vendor/ from node_modules. Run this after bumping a vendored dependency;
// it is deliberately NOT part of the build, since these files change only when the dependency
// does, and committing them keeps the build free of a per-build copy step.
//
//   npm run sync:vendor

import { createRequire } from "node:module";
import { readFileSync, writeFileSync } from "node:fs";
import { dirname, resolve } from "node:path";

const require = createRequire(import.meta.url);
const VENDOR = resolve(import.meta.dirname, "../../tailwind/vendor");

const pkg = require("tw-animate-css/package.json");
const src = dirname(require.resolve("tw-animate-css/package.json"));

const header = `/* tw-animate-css v${pkg.version} — ${pkg.homepage ?? "https://github.com/Wombosvideo/tw-animate-css"}
 * MIT License; see tw-animate-css-LICENSE alongside this file.
 * Vendored (rather than resolved from node_modules) so a consuming app's own Tailwind
 * build can import it in single-build mode. Refresh with: npm run sync:vendor */

`;

writeFileSync(
  `${VENDOR}/tw-animate.css`,
  header + readFileSync(`${src}/dist/tw-animate.css`, "utf8"),
);
// Keep the .txt extension: NuGet treats a PackagePath without one as a directory, which nests the
// file inside a folder named after itself.
writeFileSync(`${VENDOR}/tw-animate-css-LICENSE.txt`, readFileSync(`${src}/LICENSE`, "utf8"));

console.log(`sync-vendor: tw-animate-css v${pkg.version} -> tailwind/vendor/`);
