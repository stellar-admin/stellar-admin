# Client modules

Status: proposed (2026-10-20).

Remove Lit from the StellarAdmin.UI web components, then ship `stellar-admin.js` as ES modules so heavy or rarely used components load only on pages that render them. This comes before the [rich text editor](rich-text-editor.md), whose script loading depends on it.

## Current state (2026-10-20)

- `src/StellarAdmin.UI/Client/rolldown.config.js` builds `js/stellar-admin-ui.ts` into one minified IIFE, `wwwroot/stellar-admin.js` (92 KB, 28 KB compressed), with `rollup-plugin-minify-html-literals`.
- All 15 components in `Client/js/web-components/` extend `LitElement`, register with `@customElement` and return `this` from `createRenderRoot`. None use Lit templates, reactive properties or the update cycle, so Lit (about 15 KB minified) contributes only the base class.
- Pages load the script with `<script defer src="/_content/StellarAdmin.UI/stellar-admin.js" asp-append-version="true">`: the admin panel layout, the ComponentPlayground and DocsSamples layouts, the readme, the `stellar-admin-ui` consumer skill (`SKILL.md`, `references/setup.md`, `javascript.md`, `conventions.md`) and the sandbox prototypes.

## Step 1: remove Lit

- Each component extends `HTMLElement` and registers with `customElements.define`, skipping an existing definition. Drop `createRenderRoot`, the decorator setting in `tsconfig.json`, and the `lit` and HTML-minify packages.
- Lit's connect timing differs slightly from a plain custom element, so check each component in the browser, along with the existing tests.
- No markup or consumer change.

## Step 2: ES modules

- Build with `format: "es"` and code splitting: one module per component plus shared chunks.
- `stellar-admin.js` stays the single entry and imports the components used on most pages (dialog, toaster, dropdown menu, sidebar, collapsible, tabs, table and similar) and sets `window.stellarAdmin`.
- Heavy or rarely used components (candidates: icon picker, command, carousel, input OTP, questionnaire) load the way the rich text editor will: their tag helper renders `<script type="module" src="…">` inside the element with a versioned URL built by the tag helper. Pick the list by measuring each module's size.
- Breaking change: the script tag becomes `<script type="module" src="…">`. Update every place listed above and the website's setup docs, and call it out in the release notes. Module scripts run at the same point as deferred scripts, so `window.stellarAdmin` keeps its timing.
- Consider `<link rel="modulepreload">` for the entry's chunks if the extra requests show up in page load.

## Afterwards

Resume the [rich text editor](rich-text-editor.md) on top of this.
