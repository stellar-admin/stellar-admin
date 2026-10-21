# Rich text editor

Status: in progress (2026-10-21) on the `rich-text-editor` branch. Approach and public API approved 2026-10-21. Proposed 2026-10-18.

An editor for long text that staff can format, such as bold, lists and links. From the Voyager Travel session feedback, where a guide's `Bio` had only a plain text area.

Explore the options first, then propose the approach and public API to Jerrie before implementing.

## Approach and public API (approved 2026-10-21)

- The editor's script is plain JavaScript with JSDoc in `Client/js/rich-text-editor/`, written fresh from the prototype rather than ported from `editor.ts`. ProseKit is a dependency of the UI client package, and its type definitions feed `npm run check`. A second, separate Rolldown build writes `wwwroot/rich-text-editor.js` (see Loading the editor's assets below).
- StellarAdmin.UI: `<sa-rich-text-editor asp-for label description placeholder rows upload-url accept max-file-size readonly>` on the same field base as `sa-textarea` (label, description, validation, `class-names`). `rows` sets the minimum height. The image button and slash item appear only with `upload-url`. The tag helper renders the toolbar, selection bubble, slash menu, link popover and table handles, and a textarea that holds the value and stands in until the script loads.
- Uploads use `sa-image-upload`'s protocol: POST `file` with the antiforgery token, JSON `{ url, error }` back. The editor inserts `url`.
- StellarAdmin: `RichTextEditor : FieldEditor` with `Placeholder`, `Rows`, `AcceptedTypes` and `MaxFileSize` as on `ImageEditor`, and `StoreImage`, whose delegate returns the image's URL and turns on images. The admin panel's upload action is generalized from image fields to both editors. `HtmlDisplay` renders the HTML on view pages and details and a plain-text excerpt in index columns. `string` properties with `[DataType(DataType.Html)]` get `RichTextEditor` and `HtmlDisplay` by default.
- Decision 1: the admin panel depends on `HtmlSanitizer` (Ganss.Xss, MIT) and sanitizes the HTML on save and on display by default, with a hook to replace it.
- Decision 2: every tool is on in the first version (images only with uploads). A tools flags enum waits until an app asks for it.
- Decision 3: fix toggle tooltips in the library first, as its own change (see Follow-ups).
- Not in the proposal, following the library's conventions: the editor's icons are `SemanticIconRole` members so they follow the icon pack, `RichTextEditorClassNames` has `Control`, `Editor` and `Toolbar`, and `sa-rich-text-editor` takes `readonly` (not `disabled`), as the textarea editor does.
- Phases, stopping at each boundary: the UI component (tag helper, script, CSS, DocsSamples, tests); the admin panel editor, display and sanitizing; the website docs and consumer skills.

## Prototype (2026-10-20)

`sandbox/html/rich-text-editor.html` (source in `sandbox/html/rich-text-editor/`): ProseKit with a toolbar of StellarAdmin components, a selection bubble, simulated image uploads with resizing, and the HTML posted through a hidden textarea.

## Phase 1: the UI component (2026-10-21)

Done on the `rich-text-editor` branch:

- `sa-rich-text-editor` (`RichTextEditorTagHelper`, `RichTextEditorClassNames` with `Control`, `Editor` and `Toolbar`) with `accept`, `max-file-size`, `placeholder`, `readonly`, `rows`, `upload-url` and `value`, on the field base. The label names the editing area and the textarea with `aria-labelledby` (no `for`), and the description and error connect to both. Read-only renders no toolbar, menus or upload settings.
- 31 `SemanticIconRole` members for the editor's icons (`RichTextUndo` to `RichTextClearCells`, appended), mapped in the Lucide and Tabler outline packs. The filled Tabler pack has no filled formatting icons, so it leaves them on the default Lucide mapping.
- The script, plain JavaScript in `Client/js/rich-text-editor/` (`rich-text-editor.js`, `commands.js`, `images.js`), built by a second Rolldown build to `wwwroot/rich-text-editor.js` (454 KB, 118 KB Brotli), loaded with `ModuleScript`. Beyond the prototype: a value without tags becomes paragraphs (text from before a field became rich text), resized image sizes are restored from the saved HTML (ProseKit reads the rendered size, which a parsed value doesn't have), a divider inserted from an empty block replaces it, an empty document posts an empty value, the textarea fires `input` on changes, uploads are checked against `accept` and `max-file-size` with a note under the editor and a failed upload's image is removed, a click on the label focuses the editor, and Ctrl shows as ⌘ on Apple platforms.
- The CSS in `Client/css/components/rich-text-editor.css`, with ProseKit's structural rules (flat lists, tables, placeholder, gap cursor, popups) in the layer instead of `prosekit.css`. The invalid state matches the textarea's.
- DocsSamples `/RichTextEditor` (Intro, Model Binding, Images using the image upload demo endpoint, Read Only), tag helper tests, the product guide's client build paragraph, and the regenerated UI skill component reference and index.

Verified: `npm run check`, the oxfmt check, the Release build of the solution (no new warnings), all 1728 .NET tests including 9 new tag helper tests, and the skills generator drift check after regenerating. Headless Chromium 151 on DocsSamples `/RichTextEditor`: the four editors load `rich-text-editor.js` once and mount with no errors, and `/Textarea` doesn't load it; toolbar state follows the cursor, typing updates the textarea and fires `input`, undo, the block menu (label and focus), the bold tooltip with its shortcut, the selection bubble and its link popover, the slash menu filtering to a table, Tab between cells, the table handles and column menu, an image upload with progress ending in the endpoint's URL, an oversized image rejected with a note, read-only (no toolbar, not editable, readonly textarea), the label click, the divider replacing an empty heading, and two editors inserted later through a parsed fragment (as htmx swaps them) mounting without loading the script again, one turning a plain-text value into paragraphs and one keeping a saved image width. Screenshots in dark and light and at 390px wide (no horizontal scroll) were checked by eye. Not checked yet: an htmx swap in the admin panel (phase 2), other themes and density levels, and image resizing by drag.

## Loading the editor's assets

The editor's script (about 440 KB, or 135 KB compressed) must not load on every page, only on pages that render an editor, including editors swapped in by htmx. Decision (2026-10-20): follow web standards and render the script tag with the component.

[Client modules](archive/client-modules.md) removed Lit and TypeScript and bundles the library's plain JavaScript with Rolldown into one ES module. The editor follows that pattern as an on-demand bundle.

- The tag helper renders `<script type="module" src="…">` inside `<sel-rich-text-editor>`, so it survives `hx-select` and out-of-band swaps. Full pages and htmx responses get the same markup; no request-scoped asset registry, response header or htmx extension.
- The browser loads and runs a module URL once per document, so several editors or repeated swaps load it once. htmx runs swapped-in scripts by default. Custom elements upgrade whenever the definition arrives, and the textarea fallback covers the wait.
- Ship the bundle as an ES module (the prototype builds an IIFE, which would run again on every swap), and guard `customElements.define` against an existing definition. It's a second entry in `src/StellarAdmin.UI/Client/rolldown.config.js`, built as a self-contained module with no shared chunks.
- The tag helper renders the script with the internal `ModuleScript` helper, which takes the fingerprinted URL from the page's static asset collection, or the plain URL with the path base when there is none. Every editor then uses the identical URL, since different URLs would load the script twice.
- The editor's CSS goes in the main stylesheet in `@layer sa.components`, not a lazily loaded `<link>`: a `<link>` is applied again on every swap, and there is no standard load-once for stylesheets. Copy the few ProseKit rules we need and drop `prosekit.css`, whose unlayered rules override ours.
- Later option: CSS module scripts (`import … with { type: "css" }`) once every browser supports them. An import map was ruled out by [client modules](archive/client-modules.md), because ASP.NET's map needs a `<base href>` tag below the site root and `.WithStaticAssets()` doesn't reach MVC conventional routes.
- Verify in the browser: a page without an editor doesn't load the bundle, a page with several editors loads it once, and an editor swapped in by an htmx partial loads it.
- Rejected alternatives: a server-side registry written out by the layout plus a header, out-of-band swap or htmx `head-support` for partial responses (Django `Media`, Orchard `ResourceManager`); a client autoloader keyed by tag name (Shoelace, Stimulus lazy controllers); or one keyed by a URL on the element (Filament `x-load-src`). Livewire's `@assets` and Astro's component scripts take the same approach as this decision.

## Follow-ups

- **Done (2026-10-21): tooltips on toggles don't open natively.** `sa-toggle` renders a `<label>` around a hidden checkbox, and browsers with native `interestfor` (Chromium 151 here) honour it only on buttons and links, so the polyfill stands down and a toggle's tooltip never opens. The prototype opens them from the editor element (`#setUpToggleTooltips` in `editor.ts`). This needs a library fix: either library script that does the same for every toggle with `interestfor`, or toolbar toggles rendered as `<button aria-pressed>`. Also check the Tooltip docs' "Elements" example: an `<input>` has no native `interestfor` either, so its input, select and checkbox tooltips probably don't open.
  Fixed in the library: `Client/js/interest-fallback.js`, in the main bundle, handles `interestfor` on any element the browser doesn't (anything without `interestForElement`) when native support exists, with the `interest-delay-start` and `interest-delay-end` delays and the `interest` and `loseinterest` events. A control inside a label, such as a toggle's hidden checkbox, takes its hover from the label and anchors the tooltip to it. Verified in headless Chromium 151 on DocsSamples `/Tooltip`: the input, checkbox and select tooltips open on hover and close on leave (the button one still opens natively), and an injected `sa-toggle` opens its tooltip after the delay, above and centred on the label, closes it on leave and on click (the click still toggles), and opens it on keyboard focus and closes it on blur. `npm run check` passes. The skill's JavaScript reference and the product guide mention it. Not done: the website's JavaScript and Tooltip pages, which say only that the polyfill is bundled.
