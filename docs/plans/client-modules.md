# Client modules

Status: proposed (2026-10-20).

Remove Lit and the build from the StellarAdmin.UI web components, rewriting them as plain JavaScript, then ship them and the admin panel's script as ES modules so heavy or rarely used components load only on pages that render them. This comes before the [rich text editor](rich-text-editor.md), whose script loading depends on it.

## Current state (2026-10-20)

- `src/StellarAdmin.UI/Client/rolldown.config.js` builds `js/stellar-admin-ui.ts` into one minified IIFE, `wwwroot/stellar-admin.js` (92 KB, 28 KB compressed), with `rollup-plugin-minify-html-literals`.
- All 15 components in `Client/js/web-components/` extend `LitElement`, register with `@customElement` and return `this` from `createRenderRoot`. None use Lit templates, reactive properties or the update cycle, so Lit (about 15 KB minified) contributes only the base class.
- The admin panel's own components are already plain custom elements in one unbundled file, `src/StellarAdmin/Client/js/stellar-admin-panel.js`, which `scripts/copy-js.mjs` copies to `wwwroot` with the vendored `htmx.min.js`: `admin-remote-sheet`, `admin-lookup-picker`, `admin-create-sheet`, `admin-lookup-created`, `admin-lookup-sheet-editor` and `admin-multi-lookup-sheet-editor`. The admin panel layout loads it with a deferred script tag.
- Pages load the script with `<script defer src="/_content/StellarAdmin.UI/stellar-admin.js" asp-append-version="true">`: the admin panel layout, the ComponentPlayground and DocsSamples layouts, the readme, the `stellar-admin-ui` consumer skill (`SKILL.md`, `references/setup.md`, `javascript.md`, `conventions.md`) and the sandbox prototypes.

## Approach: no build

Decision (2026-10-20): follow the web platform and ship client code without a build, as Rails 8 does by default.

- Plain ES modules, one `.js` file per component, served as written: no bundler, transpiler or minifier.
- Types in JSDoc, checked with `tsc --noEmit --checkJs` in CI and the editor rather than compiled.
- Vendor `interestfor`, the only remaining npm dependency, into `wwwroot` with a copy script, as the admin panel already does for htmx.
- ASP.NET compresses static assets with Brotli and gzip at build time, and `asp-append-version` gives cacheable versioned URLs. Unminified files are larger before compression; accepted.
- `<link rel="modulepreload">` for the entry's imports, so the browser fetches them in parallel instead of level by level.
- An import map later, if modules need stable names that map to versioned URLs; relative imports are enough until then.
- The rich text editor is the one exception: it stays bundled because ProseKit comes from npm.

## Step 1: remove Lit and TypeScript

- Rewrite each component as plain JavaScript with JSDoc types: extend `HTMLElement` and register with `customElements.define`, skipping an existing definition. Drop `createRenderRoot`.
- Remove Rolldown, `rolldown.config.js`, the `lit` and HTML-minify packages and the decorator setting; keep a `tsconfig.json` (or `jsconfig.json`) for the type check and add it to CI.
- Lit's connect timing differs slightly from a plain custom element, so check each component in the browser, along with the existing tests.
- Without a bundler the entry imports the component files, so it must load as a module: the script tag change described in step 2 lands with this step.

## Step 2: ES modules

- Serve the component modules directly from `wwwroot`, with relative imports between them.
- `stellar-admin.js` stays the single entry and imports the components used on most pages (dialog, toaster, dropdown menu, sidebar, collapsible, tabs, table and similar) and sets `window.stellarAdmin`.
- Heavy or rarely used components (candidates: icon picker, command, carousel, input OTP, questionnaire) load the way the rich text editor will: their tag helper renders `<script type="module" src="…">` inside the element with a versioned URL built by the tag helper. Pick the list by measuring each module's size.
- Breaking change: the script tag becomes `<script type="module" src="…">`. Update every place listed above and the website's setup docs, and call it out in the release notes. Module scripts run at the same point as deferred scripts, so `window.stellarAdmin` keeps its timing.
- Treat `stellar-admin-panel.js` the same way: load it as a module, guard each `customElements.define`, and split out components that only some pages use (the lookup sheet editors and pickers) if that pays off. The panel's layout is the only place that loads it.
- Consider `<link rel="modulepreload">` for the entry's chunks if the extra requests show up in page load.

## Afterwards

Resume the [rich text editor](rich-text-editor.md) on top of this.
