// Copies the vendored scripts from node_modules into ../wwwroot/vendor, where the components import
// them from, marked so the type check skips them. Add an entry per file.
//
//   node ./scripts/copy-vendor.mjs

import { mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { resolve } from "node:path";

const files = [["interestfor/src/interestfor.min.js", "interestfor.js"]];

const nodeModules = resolve(import.meta.dirname, "../node_modules");
const vendor = resolve(import.meta.dirname, "../../wwwroot/vendor");

mkdirSync(vendor, { recursive: true });
for (const [source, target] of files) {
  const script = readFileSync(resolve(nodeModules, source), "utf8");
  writeFileSync(resolve(vendor, target), `// @ts-nocheck\n${script}`);
  console.log(`${source} -> wwwroot/vendor/${target}`);
}
