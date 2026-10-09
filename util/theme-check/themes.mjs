// Shared by the theme checks: argument parsing, the themes to run, and applying one to a page.

import { readdirSync, readFileSync } from "node:fs";
import { basename, resolve } from "node:path";
import { randomThemes } from "./random-theme.mjs";

const repoRoot = resolve(import.meta.dirname, "../..");

export function parseArguments(argv, defaults) {
  const options = { ...defaults, urls: [] };
  for (let i = 0; i < argv.length; i++) {
    const argument = argv[i];
    if (!argument.startsWith("--")) {
      options.urls.push(argument);
      continue;
    }
    const key = argument.slice(2).replace(/-(\w)/g, (_, c) => c.toUpperCase());
    if (!(key in defaults)) throw new Error(`Unknown option ${argument}`);
    options[key] = argv[++i];
  }
  if (!options.urls.length) throw new Error("Pass at least one page URL");
  return options;
}

// The base theme, each preset file, then seeded random themes.
export function themesFor({ presets, random, seed }) {
  const themes = [{ name: "base" }];
  if (presets !== "none") {
    const folder = resolve(repoRoot, presets);
    for (const file of readdirSync(folder).filter((f) => f.endsWith(".css")).sort())
      themes.push({ name: basename(file, ".css"), css: readFileSync(resolve(folder, file), "utf8") });
  }
  return themes.concat(randomThemes(+random, +seed));
}

export function withMode(url, mode) {
  const result = new URL(url);
  result.searchParams.set("mode", mode);
  return result.href;
}

// Applied after load: no transitions (so colours are final), the preset's CSS, the random knobs
// on the root element.
export function applyTheme(theme) {
  return `(() => {
    const style = document.createElement("style");
    style.textContent = "*, ::before, ::after { transition: none !important; }\\n" + ${JSON.stringify(theme.css ?? "")};
    document.head.append(style);
    for (const [name, value] of Object.entries(${JSON.stringify(theme.knobs ?? {})}))
      document.documentElement.style.setProperty(name, value);
  })()`;
}
