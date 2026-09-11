import { readFileSync, readdirSync, existsSync } from "node:fs";
import { resolve, dirname } from "node:path";
import { fileURLToPath } from "node:url";

export function inventory(root) {
  const folder = resolve(root, "src/StellarAdmin.TagHelpers/TagHelpers");
  const components = {};
  function visit(path, component) {
    for (const entry of readdirSync(path, { withFileTypes: true })) {
      const file = resolve(path, entry.name);
      if (entry.isDirectory()) {
        visit(file, component ?? entry.name);
        continue;
      }
      if (!entry.name.endsWith(".cs")) continue;
      const source = readFileSync(file, "utf8");
      const tags = [...source.matchAll(/\[HtmlTargetElement\(\s*"(sa-[\w-]+)"/g)].map((m) => m[1]);
      const hooks = [
        ...source.replace(/\[HtmlTargetElement\([^\]]*\)\]/g, "").matchAll(/"(sa-[a-z0-9-]+)"/g),
      ].map((m) => m[1]);
      if (!component && !tags.length) continue;
      const key = component ?? entry.name.replace("TagHelper.cs", "");
      const item = (components[key] ??= { tags: [], hooks: [] });
      item.tags.push(...tags);
      item.hooks.push(...hooks);
    }
  }
  visit(folder);
  return Object.fromEntries(
    Object.entries(components)
      .filter(([, item]) => item.tags.length)
      .sort()
      .map(([key, item]) => [
        key,
        { tags: [...new Set(item.tags)].sort(), hooks: [...new Set(item.hooks)].sort() },
      ]),
  );
}

export function check(root) {
  const errors = [];
  const manifest = JSON.parse(
    readFileSync(resolve(root, "util/theme-coverage/coverage.json"), "utf8"),
  );
  const sources = inventory(root);
  const themesFolder = resolve(root, "src/StellarAdmin.TagHelpers/Client/css/themes");
  const themes = readdirSync(themesFolder)
    .filter((name) => name.endsWith(".css"))
    .map((name) => name.slice(0, -4))
    .sort();
  const css = Object.fromEntries(
    themes.map((name) => [
      name,
      readFileSync(resolve(themesFolder, `${name}.css`), "utf8").replace(/\/\*[\s\S]*?\*\//g, ""),
    ]),
  );
  const equal = (a, b) => JSON.stringify(a) === JSON.stringify(b);
  if (manifest.version !== 1) errors.push("Unsupported coverage manifest version");
  if (!equal(Object.keys(manifest.themes).sort(), themes))
    errors.push("Theme inventory changed: declare every stylesheet in coverage.json");
  const project = readFileSync(
    resolve(root, "src/StellarAdmin.TagHelpers/StellarAdmin.TagHelpers.csproj"),
    "utf8",
  );
  for (const theme of themes) {
    if (!project.includes(`<ClientOutput Include="wwwroot/stellar-admin.${theme}.css"`))
      errors.push(`${theme}: missing ClientOutput registration`);
    if (!["custom", "upstream"].includes(manifest.themes[theme]?.source))
      errors.push(`${theme}: declare custom or upstream ownership`);
  }
  for (const component of new Set([...Object.keys(sources), ...Object.keys(manifest.components)])) {
    const source = sources[component];
    const entry = manifest.components[component];
    if (!source || !entry) {
      errors.push(`${component}: component inventory changed; review all themes`);
      continue;
    }
    for (const key of ["tags", "hooks"]) {
      if (!equal(source[key], entry[key]))
        errors.push(
          `${component}: ${key} changed; review subcomponents and variants in all themes`,
        );
    }
    if (!equal(Object.keys(entry.themes).sort(), themes))
      errors.push(`${component}: missing or stale theme coverage`);
    for (const theme of themes) {
      const coverage = entry.themes[theme];
      if (coverage?.status === "shared-only") {
        if (!coverage.reason?.trim())
          errors.push(`${component}/${theme}: shared-only requires a rationale`);
      } else if (coverage?.status === "reviewed") {
        if (!coverage.rules?.length)
          errors.push(`${component}/${theme}: reviewed requires rule evidence`);
        for (const rule of coverage.rules ?? []) {
          if (!/^sa-[a-z0-9-]+$/.test(rule) || !new RegExp(`\\.${rule}(?![\\w-])`).test(css[theme]))
            errors.push(`${component}/${theme}: missing theme rule .${rule}`);
        }
      } else errors.push(`${component}/${theme}: support is pending or undeclared`);
    }
    if (!entry.example || !existsSync(resolve(root, entry.example)))
      errors.push(`${component}: missing example or reference path`);
  }
  return { errors, components: Object.keys(sources).length, themes: themes.length };
}

if (process.argv[1] && resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
  const root = resolve(dirname(fileURLToPath(import.meta.url)), "../..");
  try {
    const result = check(root);
    if (result.errors.length) {
      console.error(result.errors.join("\n"));
      process.exitCode = 1;
    } else
      console.log(
        `Theme coverage: ${result.components} components × ${result.themes} themes reviewed.`,
      );
  } catch (error) {
    console.error(error.message);
    process.exitCode = 1;
  }
}
