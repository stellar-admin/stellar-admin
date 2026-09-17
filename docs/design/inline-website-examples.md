# Inline website examples

Use generated StellarAdmin HTML for product demonstrations that should participate in the homepage's layout. React owns the page and wrapper; the library's light-DOM elements own their interactive descendants. This website workflow does not change the public NuGet stylesheet consumption model or the existing iframe-based documentation demos.

## Retained implementation

The reusable implementation is in `../website/src/components/inline-example/`. Its [README](https://github.com/stellar-admin/website/blob/master/src/components/inline-example/README.md) shows the React API and route stylesheet setup. It supports the `collapsible` and `dialog` examples, unique instance prefixes and one shared browser runtime. The scoped Observatory stylesheet, HTML fragments and runtime asset manifest are checked in under `generated/`.

There is no example route or homepage import. The experimental `/examples/inline-components` route and standalone browser spike were removed after the experiment was accepted. Add a consumer only when implementing an authorized page. The browser runtime is loaded from an effect, so merely retaining these files does not execute it on the homepage.

## Export and build

`../website/scripts/export-inline-example.mjs` is a temporary adapter for the two existing docs exports. It reads `public/demo/tag-helpers/*-intro.html`, extracts the preview content with parse5, removes the outer sample wrapper, prefixes IDs and recognized references with an instance placeholder, adjusts the sample width and uses a local image. It records the exported JavaScript bundle's current filename in `assets.json`.

The adapter parses the compiled Observatory CSS with PostCSS. It scopes document and component selectors to `.sa-inline-example`, adapts dark tokens to the website's ancestor `.dark`, namespaces keyframes and Tailwind internal properties, and leaves the surrounding card responsible for its background. Nested selectors retain their parent relationship. The full theme is retained; this is not CSS dead-code elimination. The website's own Tailwind scan supplies utility classes from the generated HTML, so retained snippets may contribute utilities even when no route imports the component. The wrapper and scoped theme themselves must remain absent from page bundles until imported.

Run this from `../website/` after documentation exports change:

```bash
pnpm examples:export
```

Review and commit all changed generated files together with the exporter changes. The runtime filename can change when docs are regenerated, so refresh the manifest even if the example HTML appears unchanged. Do not edit `public/demo/` or the generated inline outputs by hand.

`pnpm build` consumes the checked-in artifacts; it does not run this exporter or .NET. The eventual integration should export curated homepage fragments from the Razor demo pipeline alongside the ordinary documentation outputs. That extension is not implemented yet. Keep the website independently buildable when making it.

## Adding or changing an example

Choose an existing export or author a curated Razor example at its source. Inspect the rendered HTML before extending the temporary adapter: its wrapper assumptions, supported ID references and two-name allowlist are deliberate limits. New `href` fragments, `interestfor`, form associations, CSS anchors or other references need explicit treatment. Preserve unique IDs within each page and identical generated markup across SSR and hydration.

Keep generated HTML trusted and static. Embedded scripts need explicit initializers; setting inner HTML does not execute them. Keep backend actions out of static demos unless the task includes a real backend or a clearly identified simulation. The current dialog's Save Changes button only closes it.

Load the browser runtime once after hydration. Avoid React updates that replace an active example's inner markup; use a stable placement or an intentional remount. The current wrapper closes its dialog on unmount, but the library's document scroll locking and observer lifetime are not a general solution for simultaneous Base UI and StellarAdmin modals.

Continue using light DOM. The experiment found that the collapsible inside Shadow DOM opened but failed to update its trigger's accessibility state because of document-wide lookups. Full theme bundles also changed the website font when linked globally. Extending the supported themes or components requires checking those boundaries again, not assuming the adapter handles every library feature.

## Verification

For code or exporter changes, run generation, inspect its diff, and run website lint, type checking and the production build. Check that generation is reproducible. Use the [development guide](../development.md) for environment and browser tooling.

For a page that consumes examples, exercise desktop/mobile and light/dark layouts, repeated instances, keyboard expansion, dialog Escape/focus restoration, client navigation away/back and navigation while a modal is open. Compare surrounding page and navigation styles with the embed stylesheet enabled/disabled. Check for hydration errors, duplicate IDs and duplicate runtime scripts. Exercise other overlay systems together when the page uses them.

For infrastructure retained without a consumer, verify the experimental route is absent from the generated route tree and production output. Inspect application assets for inline-example markers and ensure the homepage does not request the component runtime or scoped stylesheet. Existing `public/demo/` assets remain shipped for documentation iframes and are not evidence that the homepage loads them.

The experiment's observed results and remaining coverage are recorded in the [research record](../plans/homepage-component-embeds.md).
