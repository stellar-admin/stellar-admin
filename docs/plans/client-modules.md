# Client modules

Status: step 2 (bundle with Rolldown) done, step 3 comes with the rich text editor (2026-10-21).

Remove Lit and TypeScript from the StellarAdmin.UI web components, rewriting them as plain JavaScript bundled by Rolldown into one ES module, and let large, rarely used components load their own bundle only on pages that render them. This comes before the [rich text editor](rich-text-editor.md), whose script loading depends on it.

## Current state (2026-10-20)

- `src/StellarAdmin.UI/Client/rolldown.config.js` builds `js/stellar-admin-ui.ts` into one minified IIFE, `wwwroot/stellar-admin.js` (92 KB, 28 KB compressed), with `rollup-plugin-minify-html-literals`.
- All 15 components in `Client/js/web-components/` extend `LitElement`, register with `@customElement` and return `this` from `createRenderRoot`. None use Lit templates, reactive properties or the update cycle, so Lit (about 15 KB minified) contributes only the base class.
- The admin panel's own components are already plain custom elements in one unbundled file, `src/StellarAdmin/Client/js/stellar-admin-panel.js`, which `scripts/copy-js.mjs` copies to `wwwroot` with the vendored `htmx.min.js`: `admin-remote-sheet`, `admin-lookup-picker`, `admin-create-sheet`, `admin-lookup-created`, `admin-lookup-sheet-editor` and `admin-multi-lookup-sheet-editor`. The admin panel layout loads it with a deferred script tag.
- Pages load the script with `<script defer src="/_content/StellarAdmin.UI/stellar-admin.js" asp-append-version="true">`: the admin panel layout, the ComponentPlayground and DocsSamples layouts, the readme, the `stellar-admin-ui` consumer skill (`SKILL.md`, `references/setup.md`, `javascript.md`, `conventions.md`) and the sandbox prototypes.

## Approach: bundle with Rolldown (revised 2026-10-21)

Decision (2026-10-21): author plain JavaScript and bundle it with Rolldown. This replaces the no-build decision of 2026-10-20, which step 1 implemented. No-build suits an app, where it removes a toolchain from the developer's own project. For a library it moves the cost onto every app: our developers never build our JavaScript, so a build in this repository costs them nothing, while shipping unbundled modules cost them 24 requests and about 53 KB (Brotli) on a first visit instead of one file, plus an import map, a `<base href>` tag with its side effects on relative and `#` links, and an MVC workaround to cache those modules.

- Sources stay plain JavaScript with types in JSDoc, checked with `tsc --checkJs` (`npm run check`, run in CI). No Lit and no TypeScript compile. They move back out of `wwwroot` into `Client/js/` so the unbundled files aren't published as static assets.
- Rolldown bundles them into one minified ES module, `wwwroot/stellar-admin.js`, with a source map. It holds every current component, the icon picker included, plus the `window.stellarAdmin` helpers and the `interestfor` polyfill imported from its npm package.
- Developers add one tag: `<script type="module" src="~/_content/StellarAdmin.UI/stellar-admin.js" asp-append-version="true">`. No import map and no base tag.
- Only large, rarely used components get their own bundle and load on demand: the [rich text editor](rich-text-editor.md) first, and later candidates such as a map or a spreadsheet-like grid. Each is a self-contained ES module with no shared chunks, which its tag helper loads with the internal `ModuleScript` helper (see step 2's history below), so it needs no import map either.
- `MapStellarAdmin` keeps adding the static asset metadata to its routes. `asp-append-version` then gives the admin panel's bundle and stylesheet fingerprinted, immutable URLs. Without that metadata it falls back to `?v=` and the browser revalidates the file, which is how consumer pages behave unless their endpoints call `.WithStaticAssets()`.

### Superseded: no build and the import map

The 2026-10-20 approach shipped one ES module per component from `wwwroot`, unminified, and relied on the ASP.NET import map for fingerprinted module URLs. Found on 2026-10-21: the framework's map uses page-relative keys, so it needs `<base href="~/" />` on pages below the site root, and `.WithStaticAssets()` on `MapControllerRoute` does nothing ([dotnet/aspnetcore#59003](https://github.com/dotnet/aspnetcore/issues/59003), fix in [PR #65188](https://github.com/dotnet/aspnetcore/pull/65188), still open). Step 1 and its import map fix implemented this, and step 2 below reverses the parts the bundle makes unnecessary.

## Step 1: remove Lit, TypeScript and the build

- Move the sources from `Client/js/` to `wwwroot/`: the entry stays `wwwroot/stellar-admin.js`, and the components, wrappers and helpers keep their folders under `wwwroot/js/`. Un-ignore them in `.gitignore`. The CSS outputs and the vendored copy stay ignored.
- Rewrite the 24 `.ts` files as plain JavaScript: extend `HTMLElement`, register with `customElements.define` skipping an existing definition, drop `createRenderRoot`, the `super.connectedCallback()` calls and the Lit imports. Types move to JSDoc and relative imports get explicit `.js` extensions.
- Copy `interestfor` from its package into `wwwroot/vendor/` with a copy script.
- Remove Rolldown, `rolldown.config.js`, the `lit`, `rolldown` and HTML-minify packages, the source map and the decorator setting. Keep `tsconfig.json` for `tsc --checkJs`, add `npm run check` and run it in `ci.yml`. The csproj's client build then covers the CSS and the vendor copy.
- The script tag becomes `<script type="module">` in the admin panel, ComponentPlayground and DocsSamples layouts, the readme, the `stellar-admin-ui` skill files, the sandbox HTML pages and `build/smoke-test.sh`. Add the import map pieces described above.
- Verify: the type check, the existing tests, and each component in the browser, since Lit's connect timing and the class-field semantics (the old `useDefineForClassFields: false`) differ slightly from a plain custom element.

### Step 1 progress (2026-10-20)

Done on the `client-modules` branch: the 24 TypeScript files converted to JavaScript with JSDoc under `wwwroot/js/` (a TypeScript compiler API script turned annotations into JSDoc and Lit into `HTMLElement`, then every output was reviewed by hand), the entry rewritten as `wwwroot/stellar-admin.js`, `interestfor` vendored by `scripts/copy-vendor.mjs`, Lit, Rolldown and the HTML minifier removed, `npm run check` added and run in `ci.yml`, the module tag and import map in the layouts, readme, skills, sandbox pages and smoke test, the Lit notice removed, and the product guide updated.

Verified: `npm run check`, the oxfmt check, `dotnet build` of the solution, all 1688 .NET tests, the consumer skill validation, and a headless Chromium pass over 19 DocsSamples pages (no errors, every `sel-*` element upgraded, all 25 modules from fingerprinted URLs through the import map, one interaction per component) and the admin panel home page. The browser pass found a conversion bug, now fixed: a multi-line JSDoc after `return` in `toast.js` made the typed toast shortcuts return `undefined`. `allowUnreachableCode: false` now catches that. `build/smoke-test.sh` was updated but not run.

Import map fix (2026-10-21): the MVC and relative-key problems above were found after step 1, and the admin panel now has the base tag and its own metadata convention. Verified: `dotnet build`, all 1688 .NET tests, and a headless Chromium pass over AdminPlayground's `/stellaradmin`, `/stellaradmin/airports`, `/stellaradmin/airports/create` and `/stellaradmin/categories/edit/1`, each loading all 25 modules from fingerprinted URLs with no errors and every `sel-*` element upgraded. The readme, the `stellar-admin-ui` setup and JavaScript references and the product guide now describe the base tag, the static asset calls (including `MapControllers().WithStaticAssets()` for MVC) and what the base tag does to relative and `#` links. Skill validation and the generator drift check pass. The website's UI installation and JavaScript pages say the same (lint, type check and production build pass, and both pages were checked in the dev server).

## Step 2: bundle with Rolldown

- Move `wwwroot/stellar-admin.js` and `wwwroot/js/` back to `Client/js/`, with the entry at `Client/js/stellar-admin.js`, and restore the `wwwroot` ignore rules so only the built outputs live there.
- Add `rolldown` and `rolldown.config.js`: one entry, ES module output, minified, with a source map, to `wwwroot/stellar-admin.js`. Import `interestfor` from its package and drop `scripts/copy-vendor.mjs` and `wwwroot/vendor/`. Restore the csproj's client build inputs and outputs.
- Put the icon picker back in the bundle: import it from the entry, remove its `ModuleScript` call and the two script URL tests, and restore its XML remark that the picker layout requires `stellar-admin.js`. `ModuleScript` stays for the rich text editor, whose tag helper tests will cover it.
- Remove `<script type="importmap"></script>` from the admin panel, ComponentPlayground and DocsSamples layouts, and `<base href="~/"/>` from the admin panel layout. Update the comment on the `MapStellarAdmin` metadata convention to say it serves `asp-append-version`.
- Docs back to the one module tag, without the base tag, import map and `MapControllers().WithStaticAssets()` text: the readme, the `stellar-admin-ui` setup and JavaScript references, the product guide, and the website's UI installation and JavaScript pages. The sandbox pages and smoke test keep their module tags.
- Verify: `npm run check`, the oxfmt check, the build, all .NET tests, the skill validation and drift check, `build/smoke-test.sh`, and a headless Chromium pass over the DocsSamples component pages and the admin panel (one request for the bundle, every `sel-*` element upgraded, one interaction per component, no errors).

### Step 2 progress (2026-10-21)

Done: the sources moved to `Client/js/` with the entry at `Client/js/stellar-admin.js`, `rolldown.config.js` builds one minified ES module with a source map (76 KB, 20 KB Brotli, against 92 KB and 28 KB for the Lit IIFE), `interestfor` imported from its package, `copy-vendor.mjs` and `wwwroot/vendor/` removed, the csproj inputs and outputs restored, the icon picker back in the bundle with its `ModuleScript` call and two tests removed, the import map tags and the admin base tag removed, the `MapStellarAdmin` convention adding only the `ResourceAssetCollection` (the import map definition went), and the docs and website pages back to the one module tag. The skill's and website's script timing notes now say module script instead of `defer`.

Verified: `npm run check`, the oxfmt check, `dotnet build` of the solution, all 1688 .NET tests, the skill validation and drift check, `build/smoke-test.sh` against freshly packed packages (the package ships only `stellar-admin.js` and its map), the website's lint, type check and production build (2 existing CSS optimizer warnings), and a headless Chromium pass over 20 DocsSamples component pages and 4 admin panel pages. Every page made one request for the bundle, at its fingerprinted URL in both apps, every `sel-*` element upgraded, and the only error was DocsSamples' missing `favicon.ico`. Interactions checked: carousel, collapsible, command filtering, `dialog()`, dropdown menu, icon picker, slider keyboard, tabs, toast and input OTP.

### Step 2 history: on-demand components (2026-10-21)

Measured (Brotli, quality 11): the unbundled entry and everything it imports came to about 53 KB. The candidates for loading on demand were small: icon picker 4.3 KB, command 5.3 KB with `command-score`, slider 3.8 KB, questionnaire 3.4 KB, carousel 2.3 KB and input OTP 1.9 KB, 21 KB together.

The icon picker was split out as a trial: the internal `ModuleScript` helper renders `<script type="module">` inside the element at the module's fingerprinted URL from the page's static asset collection, or its plain URL with the path base. Verified in headless Chromium: DocsSamples `/IconPicker` loaded the module once for its five pickers, `/Tabs` didn't load it, and on the admin amenities grid it loaded only when an htmx inline row edit swapped in a picker (htmx 4 recreates each `<script>` in swapped content with its attributes, and the browser runs a module URL once). Decision (2026-10-21): components this small belong in the bundle, so the icon picker goes back. The mechanism stays for large, rarely used components, starting with the rich text editor.

## Step 3: on-demand bundles

With the [rich text editor](rich-text-editor.md): a second Rolldown entry builds its self-contained module, its tag helper loads it with `ModuleScript`, and the browser check covers a page without an editor (not loaded), one with several (loaded once) and an htmx partial.

## Outside this repository

- The website's embed exporter loads the single `stellar-admin.js` IIFE. With the bundle it stays a single file, so it needs only the module tag. On-demand bundles will need it to serve their files too.
- The release notes call out the script tag change as breaking.

## Afterwards

Resume the [rich text editor](rich-text-editor.md) on top of this.
