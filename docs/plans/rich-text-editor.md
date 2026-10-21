# Rich text editor

Status: on hold (2026-10-20), waiting on [client modules](client-modules.md). Proposed 2026-10-18.

An editor for long text that staff can format, such as bold, lists and links. From the Voyager Travel session feedback, where a guide's `Bio` had only a plain text area.

Explore the options first, then propose the approach and public API to Jerrie before implementing.

## Prototype (2026-10-20)

`sandbox/html/rich-text-editor.html` (source in `sandbox/html/rich-text-editor/`): ProseKit with a toolbar of StellarAdmin components, a selection bubble, simulated image uploads with resizing, and the HTML posted through a hidden textarea.

## Loading the editor's assets

The editor's script (about 440 KB, or 135 KB compressed) must not load on every page, only on pages that render an editor, including editors swapped in by htmx. Decision (2026-10-20): follow web standards and render the script tag with the component.

On hold until [client modules](client-modules.md) is done: it removes Lit and moves the library's script to ES modules, and the editor follows that pattern.

- The tag helper renders `<script type="module" src="…">` inside `<sel-rich-text-editor>`, so it survives `hx-select` and out-of-band swaps. Full pages and htmx responses get the same markup; no request-scoped asset registry, response header or htmx extension.
- The browser loads and runs a module URL once per document, so several editors or repeated swaps load it once. htmx runs swapped-in scripts by default. Custom elements upgrade whenever the definition arrives, and the textarea fallback covers the wait.
- Ship the bundle as an ES module (the prototype builds an IIFE, which would run again on every swap), and guard `customElements.define` against an existing definition.
- The tag helper renders the script with the internal `ModuleScript` helper, which takes the fingerprinted URL from the page's static asset collection, or the plain URL with the path base when there is none. Every editor then uses the identical URL, since different URLs would load the script twice.
- The editor's CSS goes in the main stylesheet in `@layer sa.components`, not a lazily loaded `<link>`: a `<link>` is applied again on every swap, and there is no standard load-once for stylesheets. Copy the few ProseKit rules we need and drop `prosekit.css`, whose unlayered rules override ours.
- Later option: CSS module scripts (`import … with { type: "css" }`) once every browser supports them. An import map was ruled out by [client modules](client-modules.md), because ASP.NET's map needs a `<base href>` tag below the site root and `.WithStaticAssets()` doesn't reach MVC conventional routes.
- Rejected alternatives: a server-side registry written out by the layout plus a header, out-of-band swap or htmx `head-support` for partial responses (Django `Media`, Orchard `ResourceManager`); a client autoloader keyed by tag name (Shoelace, Stimulus lazy controllers); or one keyed by a URL on the element (Filament `x-load-src`). Livewire's `@assets` and Astro's component scripts take the same approach as this decision.

## Follow-ups

- **Tooltips on toggles don't open natively.** `sa-toggle` renders a `<label>` around a hidden checkbox, and browsers with native `interestfor` (Chromium 151 here) honour it only on buttons and links, so the polyfill stands down and a toggle's tooltip never opens. The prototype opens them from the editor element (`#setUpToggleTooltips` in `editor.ts`). This needs a library fix: either library script that does the same for every toggle with `interestfor`, or toolbar toggles rendered as `<button aria-pressed>`. Also check the Tooltip docs' "Elements" example: an `<input>` has no native `interestfor` either, so its input, select and checkbox tooltips probably don't open.
