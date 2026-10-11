# Theme builder

Status: in progress, steps 1 to 3a done (2026-10-11). Website branch `theme-builder`. Split out of phase 4 of the [theme token migration](theme-token-migration.md); the rest of that phase (consumer skill, development skills) waits for this. Product branch `token-spec-concept`; the website is a separate repository (`../website`, its own `AGENTS.md`).

## Goal

A theme builder on the website: start from a preset, adjust the knobs against a live preview of real components, and download a knob file to link in an app. Once it exists, the library stops shipping presets: the package is `stellar-admin.css` (the default look) plus the Tailwind adapter, and every other look is a file the developer owns.

Later, not in this plan: letting a reader make their builder theme the one the docs demos use.

## Starting point

- Prototype: `sandbox/html/token-spec/builder.html`. Its `CONTROLS` list (37 knobs with labels, types, ranges, defaults and auto/optional flags), `FONTS`/`HEADINGS` lists and the iframe preview that sets custom properties are the reference for behaviour.
- Preview pages: `docs/DocsSamplesGenerator` already exports every DocsSamples page into the website's `public/demo/tag-helpers/`, including the theme showcase (`showcase-theme-showcase.html`, from `Showcase/_ThemeShowcase`). The website shows exports in an iframe with `DemoPreview` (`src/components/demo-preview.tsx`), same origin, resized to content.
- Not reused: the inline-example POC (`src/components/inline-example`, `scripts/export-inline-example.mjs`). It scopes a compiled theme into the React page for homepage embeds; the builder needs a whole page isolated from the site's CSS, which the iframe gives for free.
- Out of date: the generator still downloads the fifteen old bundles and injects a script that swaps `stellar-admin.<theme>.css` from `localStorage["demo-theme"]` (`Generator.cs` `ThemeNames`, `ThemeSyncScript`, `DownloadThemeStylesheetsAsync`, `ThemeStylesheetLinkRegex`). The website's picker (`src/lib/demo-theme.ts`, `demo-theme-select.tsx`) lists those fifteen names.

## Steps

### 1. Docs export for the token stylesheet (product, then website regeneration)

- The generator downloads `stellar-admin.css` and the presets instead of the bundles, and the injected script links `stellar-admin.css` plus the selected preset file (none for the default). Storage key and fonts handling (`theme-fonts.js`) as today.
- The script also accepts a knob set (a later step's hook): if `localStorage["demo-theme"]` is a custom theme, it writes the knobs onto the root instead of linking a preset. Shape decided in step 3; step 1 only keeps the script's theme lookup in one function so that is a small change.
- Website: `demo-theme.ts` lists Default, Ledger, Ops, Soft; regenerate the exports and the inline-example outputs (`pnpm examples:export`; its adapter reads a bundle name and needs pointing at `stellar-admin.css`).
- Verify: generator run, the website diff, website lint/typecheck/build, a few demo pages and the showcase in each preset, light and dark.

This is the export half of migration phase 5, pulled forward because the builder depends on it. The website's theming pages stay for phase 5.

### 2. Knob manifest (product)

- `wwwroot/stellar-admin.knobs.json`, built from a checked-in source next to `tokens.css`. Per knob: name, group, label, one-line meaning, type (`color`, `number`, `length`, `choice`, `font`, `switch`), range/step or choices, default, and `optional` (unset by default, derives from another knob; name what it follows). Font choices carry a family stack and a Google Fonts family string.
- Seeded from the prototype's `CONTROLS`, checked against `tokens.css`.
- A test (TagHelpers tests or a `build-css.mjs` check) fails when a required knob in `tokens.css` has no manifest entry, a manifest entry names no knob, or a default differs.
- The generator copies it into the website's demo assets with the stylesheet, so the builder reads the knobs of the exact library version its preview uses.
- Later readers: the consumer skill (migration phase 4) and the docs knob reference (phase 5).
- Done as: source `Client/css/knobs.json`, written to `wwwroot/stellar-admin.knobs.json` by `build-css.mjs`, whose check is the test (it fails the CSS build, and so `dotnet build`). 54 knobs, 10 optional, in seven groups (colour, shape, type, section labels, structure, buttons, focus). Every knob in the `tokens.css` knob block has an entry, the fixed tokens are left out except `--sa-press-offset` (a knob in `tokens.md`). The prototype's compound controls (card footer, column heads) are separate knobs here; the builder may group them again. `length` knobs carry a `unit` and `named` values (`Pill` = `9999px` for the radius); optional knobs carry `follows` and an `initial` value for when they are first set. The check also reads the presets: each name is a knob, and switch and choice values are ones the manifest offers.
- For step 3: preset values the controls cannot show as they are: Soft's `--sa-radius-outer: 1.375rem` (the control is px) and `--sa-line-strength: 0.7` (below the range; acts as 0.75), Ledger's `--sa-focus-color: color-mix(…)` (not a plain colour). The builder keeps such a value as written until the user moves the control.

### 3. Builder MVP (website)

- Route `/theme-builder` (name to confirm on the website side). Left: controls generated from the manifest, by group; optional knobs show "auto" until set. Right: `DemoPreview` of the showcase export.
- Seed: a preset picker (Default, Ledger, Ops, Soft). The website keeps the preset values as data (from step 4 on they live only there). Changing seed resets the knobs.
- Preview: knobs written onto the iframe's root element (`contentDocument.documentElement.style`); light/dark toggle (`.dark` on the iframe root); density preview through `--sa-density`. Fonts: load the chosen Google Fonts family into the iframe.
- State: knobs that differ from the default (not from the seed) in the URL hash, so a link reproduces the theme. Hash format: a short versioned encoding (`v1:` + name=value pairs), tolerant of unknown or removed knobs.
- Export: a `theme.css` with only the changed knobs:

  ```css
  /* StellarAdmin theme. Link after stellar-admin.css. Builder: https://…/theme-builder#v1:… */
  @import url("https://fonts.googleapis.com/css2?family=Figtree:wght@400..700&display=swap"); /* only when a web font is chosen; opt out with a checkbox */
  @layer sa.theme {
    :root {
      --sa-accent: oklch(0.56 0.2 295);
      --sa-radius: 9999px;
    }
  }
  ```

  Copy and download buttons, and the include snippet for each consumer: `<link rel="stylesheet" href="~/css/theme.css">` after `stellar-admin.css` (TagHelpers) or `dashboard.AddStylesheet("~/css/theme.css")` (Dashboard).
- Import: paste a `theme.css` (or open a builder URL) to keep editing.
- No contrast checker in the UI: the token formulas carry contrast. The product keeps `util/theme-check` for the presets.
- Verify: website lint/typecheck/build; in a browser, every control changes the preview, the URL round-trips, an exported file linked into DocsSamples reproduces the preview, mobile width usable.
- Done as (website `src/routes/theme-builder.tsx`, `src/components/theme-builder/knob-control.tsx`, `src/lib/theme-builder/{knobs,seeds}.ts`; a "Theme builder" link in the top navigation):
  - The page fetches the exported manifest at runtime and builds one control per knob by group: colour (swatch plus a text field for any CSS colour), number and length (slider plus text field; `named` values as buttons), switch (checkbox), choice and font (select; a value outside the choices shows as "Custom"). Optional knobs have an "Auto" checkbox; other knobs a "Reset" once changed. A value equal to the default is stored as no value.
  - Seeds: Default, Ledger, Ops, Soft, their values copied from the presets into `seeds.ts`.
  - Preview: its own fixed-height iframe of the showcase export, not `DemoPreview` (which sizes the frame to its content and exposes no load hook). Every knob is written onto the frame's root, unset optional knobs as `initial` (so their `var()` fallback applies), so a preset the reader picked for the docs demos, which the page links from storage, cannot show through. Light/dark is the builder's own and survives the page's own mode script. On narrow screens the preview sits above the controls, pinned to the top half of the screen.
  - The hash and the export carry the knobs that differ from the default. The export's font `@import` covers the chosen web fonts only when a font knob changed (the default Inter is the app's to load, as without a theme), with a checkbox to leave it out. Import takes a pasted theme file or a builder link.

### 3a. Simple and advanced controls (product manifest, website)

The full list of 54 knobs is daunting for a first visit.

- Manifest: `"essential": true` on the knobs a tool shows first: accent, neutral hue and tint, radius, pill tags, relief, elevation, density, font, heading font. In the manifest, not the website, so the consumer skill and the docs knob reference can start from the same set. The CSS build checks that only required knobs carry it, as `true`, and that at least one does.
- Builder: a Simple / Advanced switch, Simple by default. Simple lists the essential knobs in one list without their explanations (still in the label tooltip); Advanced is the grouped list with everything. The switch is a view only: values, link and export always carry every knob. When the theme changes knobs Simple does not show (a seed, an import, a link), Simple says how many, with a link to Advanced. The choice is remembered in local storage, not in the URL.
- Later, not now: compound controls in Simple (a "Style" choice setting relief, sheen and outlines together).

### 4. More preview pages (product samples, website)

- A page picker over a few exports: the showcase, a form-heavy page, overlays (dialog, menus, popovers open), a Dashboard-like shell (sidebar, header, table). Add DocsSamples pages where no existing one fits; they export like any other.

### 5. Remove the presets from the library (product, breaking)

- Delete `Client/css/presets/`, their `ClientOutput` lines and the `wwwroot/presets/` outputs. The package ships `stellar-admin.css`, `stellar-admin.tailwind.css` and the knob manifest.
- Keep the presets as test fixtures (for example `util/theme-check/presets/`), so DocsSamples' and ComponentPlayground's pickers and the contrast check still exercise non-default themes. DocsSamples serves them from its own static files; the generator exports them for the website docs picker until the website takes its preset values from its own data.
- Update the readme, product guide, `tokens.md` and the release notes: presets come from the builder.

## Open decisions

- Route name and where the builder sits in the website navigation.
- Whether the website keeps a docs theme picker over its seeds after step 5, or only the default plus a reader's builder theme (the later feature).
- Font list: the prototype's ten families, or fewer.

## Verification record

Step 3a (2026-10-11):

- `build-css.mjs` passes with the ten flags; `essential` on an optional knob or with a non-`true` value fails, naming the knob (restored after).
- Generator run; the website gets the new manifest. The run also rewrote 286 demo pages with per-run noise (antiforgery tokens, carousel ids, SVG attribute order), kept as generated per the website's rule against discarding generated changes.
- Website `lint`, `types:check` and `build` pass. In Chromium against `pnpm dev` (port 3106, stopped after): a first visit shows Simple with 10 controls and no explanations; seeding Ledger shows "also changes 12 knobs that only the advanced controls show"; "Show all" switches to Advanced (54 controls, explanations) with the hash unchanged; Advanced survives a reload; back to Simple the hash is still unchanged. Found and fixed: a seed's values equal to the default (Ledger's radius and pill tags) showed as changed; seeds now drop them, checked again after.

Step 3 (2026-10-11):

- Website `lint`, `types:check` and `build` pass; the build prerenders `/theme-builder`.
- In Chromium against `pnpm dev` (port 3106, stopped after): for each of the 54 knobs, changing its control (dark-only knobs in dark mode) set the value on the preview root and in the hash, changed the showcase's computed styles, and Reset or Auto returned it (the hash was empty after all of them). With `ops` stored as the docs theme, the builder at default showed `--sa-radius: 6px` and no `--sa-radius-outer`. Seeding Soft set its values and loaded Figtree; opening its builder link reproduced the same export; the exported file added to the bare showcase page at the preview's size gave the same computed styles for every element (bar a running spinner's rotation). Import of a theme file (unknown knob and plain property ignored) and of a builder link applied. Phone width (390px): no horizontal scroll, preview pinned above the controls. Screenshots looked right in light and dark.
- Not run: the export linked into DocsSamples itself (checked on the exported showcase page instead), browsers other than Chromium, the production build in a browser.

Step 2 (2026-10-11):

- `node scripts/build-css.mjs` passes and writes the manifest. With the manifest broken on purpose (a changed default, a removed knob, an unknown knob, an optional knob nothing reads) and Soft given `--sa-pills: 2` and an unknown knob, it fails naming each of the seven problems; restored after.
- Generator run (which built TagHelpers through the csproj): it downloads the manifest from `_content/StellarAdmin.TagHelpers/stellar-admin.knobs.json` to the website's `public/demo/tag-helpers/assets/stellar-admin.knobs.json`. The run also rewrote 286 demo pages with per-run noise only (antiforgery tokens, carousel ids, SVG attribute order); those were discarded, so the website change is the one new file.
- Not run: tests (no .NET code under test changed), website lint/build (the website only gains a static JSON file).

Step 1 (2026-10-11):

- Generator: `PresetNames` from `Client/css/presets/`, themes are `default` plus those; presets download to `assets/presets/<name>.css` (stable names); the page's `stellar-admin.css` exports fingerprinted like the other assets. The samples' `#docs-sample-theme` link becomes an empty `link[data-sa-theme]` plus `saThemeInit()`; the script links `presets/<theme>.css` or none, and an unknown stored value (an old `observatory`) falls back to `default`. `selectedTheme()` is the one lookup a custom theme will hook into.
- Website (branch `theme-builder`): 448 demos regenerated; the fifteen bundles removed; three include snippets changed (the Empty samples' `border-dashed`, from migration phase 3). `demo-theme.ts` is Default, Ledger, Ops, Soft, default `default`; the picker is one flat list. Inline-example exporter reads the fingerprinted `stellar-admin.*.css`, writes `generated/stellar-admin.css` (was `observatory.css`), and scopes `.dark <component>` rules under the website's ancestor `.dark` (the token stylesheet writes dark rules that way; the bundles used `:is(.dark *)`). Its README, `docs/design/inline-website-examples.md` and the embed skill's one line updated.
- Verified: generator run; website `lint`, `types:check` and `build` pass; `examples:export` reproducible. In Chromium, against the exported files served statically: card, button, theme showcase and textarea demos for default, ledger, ops, soft and a stale `observatory`, light and dark, link the expected preset (or none), take its accent, radius and font, and follow dark mode; a same-origin iframe swaps soft → ops → default → ledger live from a parent's storage change. Not run: the website's docs pages in a browser (the React picker itself), the inline examples rendered in a page (no route imports them).
