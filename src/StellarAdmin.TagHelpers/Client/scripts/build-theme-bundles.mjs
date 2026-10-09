// Builds the per-theme prebuilt bundles: one self-contained stylesheet per theme, compiled into
// ../wwwroot/stellar-admin.<theme>.css. The theme list is derived from css/themes/, and the
// per-theme entry (css/base.css + the theme file) is synthesized in a temporary directory;
// there are no checked-in entry files. Generate upstream themes with
// util/ThemeGenerator; author custom themes directly. Register each theme in
// ClientOutput and util/theme-coverage/coverage.json. It also builds the token-driven
// ../wwwroot/stellar-admin.css from its checked-in entry, css/stellar-admin.css: converted
// components (css/components/*.css, imported in name order at the entry's marker) name the
// selectors they replace in a "Replaces:" header, and that bundle gets copies of components.css and
// the theme file without those rules.
//
//   node ./scripts/build-theme-bundles.mjs

import { check } from "../../../../util/theme-coverage/check.mjs";
import { spawn } from "node:child_process";
import { mkdtempSync, readdirSync, readFileSync, rmSync, statSync, writeFileSync } from "node:fs";
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

// Removes replaced selectors (`.sa-switch:checked ~ …` starts with .sa-switch) from every selector
// list, wherever the list is written and however it wraps, and drops a rule once none of its
// selectors is left. It walks the statements of the file, of grouping at-rules (@layer, @media,
// @supports, @container) and of style rules (nested rules); other at-rules are copied as they are.
function withoutRules(css, replaced) {
  const skipTo = (text, from) => {
    const at = css.indexOf(text, from);
    return at === -1 ? css.length : at;
  };
  // The index of the next `stop` character outside comments, strings, parentheses and brackets.
  function scan(from, stops) {
    let depth = 0;
    for (let i = from; i < css.length; i++) {
      const c = css[i];
      if (c === "/" && css[i + 1] === "*") i = skipTo("*/", i + 2) + 1;
      else if (c === '"' || c === "'") i = skipTo(c, i + 1);
      else if (c === "(" || c === "[") depth++;
      else if (c === ")" || c === "]") depth--;
      else if (depth === 0 && stops.includes(c)) return i;
    }
    return css.length;
  }
  function closingBrace(open) {
    for (let i = open + 1, depth = 1; i < css.length; i++) {
      i = scan(i, "{}");
      if (css[i] === "{") depth++;
      else if (--depth === 0) return i;
    }
    return css.length;
  }
  function selectors(prelude) {
    const list = [];
    for (let i = 0, depth = 0, from = 0; i <= prelude.length; i++) {
      const c = prelude[i];
      if (c === "(" || c === "[") depth++;
      else if (c === ")" || c === "]") depth--;
      else if (i === prelude.length || (c === "," && depth === 0)) {
        list.push(prelude.slice(from, i));
        from = i + 1;
      }
    }
    return list;
  }
  function statements(from, to) {
    let out = "";
    for (let i = from; i < to; ) {
      const stop = Math.min(scan(i, "{;}"), to);
      if (stop >= to || css[stop] !== "{") {
        out += css.slice(i, stop + (stop < to ? 1 : 0));
        i = stop + 1;
        continue;
      }
      const close = closingBrace(stop);
      const prelude = css.slice(i, stop);
      // Comments and whitespace before the prelude stay with it.
      const lead = /^(\s|\/\*[\s\S]*?\*\/)*/.exec(prelude)[0];
      const head = prelude.slice(lead.length);
      if (head.startsWith("@")) {
        out += /^@(layer|media|supports|container)\b/.test(head)
          ? `${prelude}{${statements(stop + 1, close)}}`
          : css.slice(i, close + 1);
      } else {
        const kept = selectors(head).filter(
          (selector) => !replaced.has(/^\s*(\.[\w-]+)/.exec(selector)?.[1]),
        );
        if (kept.length)
          out += `${lead}${kept.join(",").replace(/^\s*\n/, "")}{${statements(stop + 1, close)}}`;
      }
      i = close + 1;
    }
    return out;
  }
  return statements(0, css.length);
}

function buildTokensBundle() {
  const css = resolve(clientRoot, "css");
  const components = readdirSync(resolve(css, "components"))
    .filter((file) => file.endsWith(".css"))
    .sort();
  const replaced = new Set(
    components
      .flatMap((file) => {
        const header = /Replaces:([^*]*)\*\//.exec(readFileSync(resolve(css, "components", file), "utf8"));
        return header ? header[1].split(/[\s,]+/).filter(Boolean) : [];
      }),
  );
  const legacy = { "components.css": "components.css", "themes/shadcn.nova.css": "nova.css" };
  for (const [source, copy] of Object.entries(legacy))
    writeFileSync(
      resolve(entriesFolder, copy),
      withoutRules(readFileSync(resolve(css, source), "utf8"), replaced),
    );
  const entry = readFileSync(resolve(css, "stellar-admin.css"), "utf8")
    .replace(
      /@import "\.\/([^"]+)";/g,
      (_, path) => `@import "${legacy[path] ? `./${legacy[path]}` : `../css/${path}`}";`,
    )
    .replace(
      "/* components/*.css */",
      components.map((file) => `@import "../css/components/${file}";`).join("\n"),
    );
  writeFileSync(resolve(entriesFolder, "stellar-admin.css"), entry);
  return buildBundle("tokens", resolve(entriesFolder, "stellar-admin.css"), "../wwwroot/stellar-admin.css");
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
    buildTokensBundle().then(() =>
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
