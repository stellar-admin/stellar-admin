// Copies vendored scripts from node_modules into wwwroot. Add an entry per package.
import { copyFileSync, mkdirSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const clientDir = join(dirname(fileURLToPath(import.meta.url)), "..");

const files = [["node_modules/htmx.org/dist/htmx.min.js", "../wwwroot/htmx.min.js"]];

for (const [source, target] of files) {
  const targetPath = join(clientDir, target);
  mkdirSync(dirname(targetPath), { recursive: true });
  copyFileSync(join(clientDir, source), targetPath);
  console.log(`${source} -> ${target}`);
}
