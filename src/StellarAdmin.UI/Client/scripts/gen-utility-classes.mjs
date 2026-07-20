// Dumps every Tailwind candidate used by the tag helpers and theme packs to a flat file, so a
// consuming app's own Tailwind build can generate the library's classes without us shipping the
// 1.1 MB of .cs sources they'd otherwise have to scan.
//
// This uses @tailwindcss/oxide's Scanner — the exact extractor Tailwind itself runs — over the
// exact directories Client/css/stellar-admin-ui.css names in its @source directives. Tailwind
// normalises a trailing-slash directory source to {base, pattern: "**/*"}, which is the
// SourceEntry shape constructed below, so the candidate set is identical by construction rather
// than by a heuristic that could drift.
//
//   node ./scripts/gen-utility-classes.mjs <projectRoot> <outFile>

import { Scanner } from "@tailwindcss/oxide";
import { mkdirSync, writeFileSync } from "node:fs";
import { dirname, resolve } from "node:path";

const root = resolve(process.argv[2] ?? "..");
const out = resolve(process.argv[3] ?? "../tailwind/utility-classes.txt");

const scanner = new Scanner({
  sources: [
    { base: `${root}/TagHelpers`, pattern: "**/*", negated: false },
    { base: `${root}/Theming/ThemePacks`, pattern: "**/*", negated: false },
  ],
});

// Sorted + de-duplicated so the file is stable across runs and reviewable in a diff.
const candidates = [...new Set(scanner.scan())].sort();

mkdirSync(dirname(out), { recursive: true });
writeFileSync(out, candidates.join("\n") + "\n");

console.log(
  `gen-classes: ${candidates.length} candidates from ${scanner.files.length} files -> ${out}`,
);
