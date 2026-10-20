# Client modules

Status: step 1 implemented, awaiting review (2026-10-21).

Remove Lit and the build from the StellarAdmin.UI web components, rewriting them as plain JavaScript, then ship them and the admin panel's script as ES modules so heavy or rarely used components load only on pages that render them. This comes before the [rich text editor](rich-text-editor.md), whose script loading depends on it.

## Current state (2026-10-20)

- `src/StellarAdmin.UI/Client/rolldown.config.js` builds `js/stellar-admin-ui.ts` into one minified IIFE, `wwwroot/stellar-admin.js` (92 KB, 28 KB compressed), with `rollup-plugin-minify-html-literals`.
- All 15 components in `Client/js/web-components/` extend `LitElement`, register with `@customElement` and return `this` from `createRenderRoot`. None use Lit templates, reactive properties or the update cycle, so Lit (about 15 KB minified) contributes only the base class.
- The admin panel's own components are already plain custom elements in one unbundled file, `src/StellarAdmin/Client/js/stellar-admin-panel.js`, which `scripts/copy-js.mjs` copies to `wwwroot` with the vendored `htmx.min.js`: `admin-remote-sheet`, `admin-lookup-picker`, `admin-create-sheet`, `admin-lookup-created`, `admin-lookup-sheet-editor` and `admin-multi-lookup-sheet-editor`. The admin panel layout loads it with a deferred script tag.
- Pages load the script with `<script defer src="/_content/StellarAdmin.UI/stellar-admin.js" asp-append-version="true">`: the admin panel layout, the ComponentPlayground and DocsSamples layouts, the readme, the `stellar-admin-ui` consumer skill (`SKILL.md`, `references/setup.md`, `javascript.md`, `conventions.md`) and the sandbox prototypes.

## Approach: no build

Decision (2026-10-20): follow the web platform and ship client code without a build, as Rails 8 does by default.

- Plain ES modules, one `.js` file per component, written directly in `wwwroot` and served as written: no bundler, transpiler, minifier or copy step.
- Types in JSDoc, checked with `tsc --noEmit --checkJs` in CI and the editor rather than compiled.
- Vendor `interestfor`, the only remaining npm dependency, into `wwwroot` with a copy script, as the admin panel already does for htmx.
- ASP.NET compresses static assets with Brotli and gzip at build time. Unminified files are larger before compression, which is accepted.
- The rich text editor is the one exception: it stays bundled because ProseKit comes from npm.

### Versioned module URLs: the import map

Decision (2026-10-20): use the import map ASP.NET generates. `asp-append-version` versions only the URL in the script tag, so the files the entry imports would otherwise be served with `Cache-Control: no-cache` and revalidated with an ETag on every page load. `MapStaticAssets` already publishes a fingerprinted, immutable copy of every RCL file, and `<script type="importmap"></script>` renders a map from each plain URL to its fingerprinted copy, which the browser applies to every module it loads.

- The framework's map only works with two things in place, both found on 2026-10-21. Its keys are relative (`./_content/...`) and resolve against the page, so on a page below the site root they match nothing unless the layout has `<base href="~/" />` before the map. And `.WithStaticAssets()` on `MapControllerRoute` silently does nothing ([dotnet/aspnetcore#59003](https://github.com/dotnet/aspnetcore/issues/59003), fix in [PR #65188](https://github.com/dotnet/aspnetcore/pull/65188), still open), so MVC views get no map unless the app calls `MapControllers().WithStaticAssets()`.
- Developers: the script tag becomes `<script type="module" src="~/_content/StellarAdmin.UI/stellar-admin.js"></script>`. That is all they need. For immutable caching of the modules on their own pages they add `<base href="~/" />` and `<script type="importmap"></script>` to the layout, plus `MapControllers().WithStaticAssets()` for MVC. The base tag makes relative and `#` links, including relative htmx URLs, resolve against the site root. Without the map the components still work and the browser revalidates each module. Admin panel users do nothing.
- Us (decision 2026-10-21): the admin panel layout has `<base href="~/" />` and the import map tag (its views use no relative or `#` links). `MapStellarAdmin` adds the static asset metadata to its own routes with a convention that builds the same collection `WithStaticAssets()` would, through the public `StaticAssetsEndpointDataSourceHelper`, and skips routes that already have it, so it steps aside once the framework is fixed. It doesn't touch the host's controllers. The ComponentPlayground and DocsSamples layouts render the import map tag without a base tag, so only their top-level pages use it. htmx partials need nothing: the map belongs to the document, and swapped-in elements upgrade from the registry.
- A developer with their own hand-written import map merges it with the generated one rather than adding a second.

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

Import map fix (2026-10-21): the MVC and relative-key problems above were found after step 1, and the admin panel now has the base tag and its own metadata convention. Verified: `dotnet build`, all 1688 .NET tests, and a headless Chromium pass over AdminPlayground's `/stellaradmin`, `/stellaradmin/airports`, `/stellaradmin/airports/create` and `/stellaradmin/categories/edit/1`, each loading all 25 modules from fingerprinted URLs with no errors and every `sel-*` element upgraded. The readme, the `stellar-admin-ui` setup and JavaScript references and the product guide now describe the base tag, the static asset calls (including `MapControllers().WithStaticAssets()` for MVC) and what the base tag does to relative and `#` links. Skill validation and the generator drift check pass. The website's UI installation and JavaScript pages say the same (uncommitted in `../website`; lint, type check and production build pass, both pages checked in the dev server).

## Step 2: load components only on pages that use them

- Measure each module. Candidates to split out of the entry: icon picker, command, questionnaire, carousel, input OTP and possibly slider.
- An internal helper builds the module URL, and each split-out component's tag helper renders `<script type="module" src="…">` inside the element. The browser loads a module URL once per page, so repeated components are fine.
- `stellar-admin.js` stays the single entry for the rest and keeps setting `window.stellarAdmin`. Module scripts run at the same point as deferred scripts, so its timing is unchanged.
- Load `stellar-admin-panel.js` as a module and guard each `customElements.define`. Split out the lookup sheet editors only if measuring shows it pays off.
- Verify as in step 1, plus a check that a page without a split-out component does not load it.

## Outside this repository

- The website's embed exporter loads the single `stellar-admin.js` IIFE and needs updating for modules.
- The release notes call out the script tag change as breaking.

## Afterwards

Resume the [rich text editor](rich-text-editor.md) on top of this.
