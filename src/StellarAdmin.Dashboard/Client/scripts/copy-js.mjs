// Copies vendored scripts from node_modules, and the Dashboard's own script, into wwwroot. Add an entry per file.
import { copyFileSync, mkdirSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const clientDir = join(dirname(fileURLToPath(import.meta.url)), "..");

const files = [
  ["node_modules/htmx.org/dist/htmx.min.js", "../wwwroot/htmx.min.js"],
  ["js/stellar-admin-dashboard.js", "../wwwroot/stellar-admin-dashboard.js"],
];

for (const [source, target] of files) {
  const targetPath = join(clientDir, target);
  mkdirSync(dirname(targetPath), { recursive: true });
  copyFileSync(join(clientDir, source), targetPath);
  console.log(`${source} -> ${target}`);
}
