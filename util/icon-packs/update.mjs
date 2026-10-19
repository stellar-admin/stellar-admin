// Downloads the pinned Lucide and Tabler releases into src/StellarAdmin.UI/Icons/Definitions: the
// icon nodes the icon packs are generated from, and each icon's keywords. Change a version here and
// run the script to refresh a pack.
//
//   node util/icon-packs/update.mjs

import { mkdirSync, writeFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const LUCIDE = "lucide-static@1.59.0";
const TABLER = "@tabler/icons@3.49.0";

const root = join(dirname(fileURLToPath(import.meta.url)), "..", "..");
const definitions = join(root, "src", "StellarAdmin.UI", "Icons", "Definitions");
const bom = "﻿";

async function download(path) {
  const response = await fetch(`https://unpkg.com/${path}`);
  if (!response.ok) {
    throw new Error(`${path}: ${response.status} ${response.statusText}`);
  }

  return response.text();
}

// One line per icon, so a refresh diffs by icon
function writeKeywords(pack, names, keywordsByName) {
  const lines = names
    .filter((name) => keywordsByName[name]?.length)
    .map((name) => `  ${JSON.stringify(name)}: ${JSON.stringify(keywordsByName[name])}`);
  const file = join(definitions, "Keywords", `${pack}.json`);
  writeFileSync(file, `${bom}{\n${lines.join(",\n")}\n}\n`);
  console.log(`${file}: ${lines.length} icons with keywords`);
}

function writeNodes(pack, text) {
  const file = join(definitions, `${pack}.json`);
  writeFileSync(file, bom + text);
  const names = Object.keys(JSON.parse(text));
  console.log(`${file}: ${names.length} icons`);

  return names;
}

mkdirSync(join(definitions, "Keywords"), { recursive: true });

// Emoji are dropped: nobody types them into a search box
const hasEmoji = /\p{Extended_Pictographic}/u;
const lucideTags = JSON.parse(await download(`${LUCIDE}/tags.json`));
const lucideKeywords = Object.fromEntries(
  Object.entries(lucideTags).map(([name, tags]) => [
    name,
    tags.filter((tag) => !hasEmoji.test(tag)),
  ]),
);
const lucide = writeNodes("Lucide", await download(`${LUCIDE}/icon-nodes.json`));
writeKeywords("Lucide", lucide, lucideKeywords);

const tablerIcons = JSON.parse(await download(`${TABLER}/icons.json`));
const tablerKeywords = Object.fromEntries(
  // A few tags are numbers, such as 911
  Object.entries(tablerIcons).map(([name, icon]) => [name, (icon.tags ?? []).map(String)]),
);
for (const [pack, style] of [
  ["TablerOutline", "outline"],
  ["TablerFilled", "filled"],
]) {
  const names = writeNodes(pack, await download(`${TABLER}/tabler-nodes-${style}.json`));
  writeKeywords(pack, names, tablerKeywords);
}
