# Theme builder

Status: in progress, step 1 done (2026-10-11). Website branch `theme-builder`. Split out of phase 4 of the [theme token migration](theme-token-migration.md); the rest of that phase (consumer skill, development skills) waits for this. Product branch `token-spec-concept`; the website is a separate repository (`../website`, its own `AGENTS.md`).

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

Step 1 (2026-10-11):

- Generator: `PresetNames` from `Client/css/presets/`, themes are `default` plus those; presets download to `assets/presets/<name>.css` (stable names); the page's `stellar-admin.css` exports fingerprinted like the other assets. The samples' `#docs-sample-theme` link becomes an empty `link[data-sa-theme]` plus `saThemeInit()`; the script links `presets/<theme>.css` or none, and an unknown stored value (an old `observatory`) falls back to `default`. `selectedTheme()` is the one lookup a custom theme will hook into.
- Website (branch `theme-builder`): 448 demos regenerated; the fifteen bundles removed; three include snippets changed (the Empty samples' `border-dashed`, from migration phase 3). `demo-theme.ts` is Default, Ledger, Ops, Soft, default `default`; the picker is one flat list. Inline-example exporter reads the fingerprinted `stellar-admin.*.css`, writes `generated/stellar-admin.css` (was `observatory.css`), and scopes `.dark <component>` rules under the website's ancestor `.dark` (the token stylesheet writes dark rules that way; the bundles used `:is(.dark *)`). Its README, `docs/design/inline-website-examples.md` and the embed skill's one line updated.
- Verified: generator run; website `lint`, `types:check` and `build` pass; `examples:export` reproducible. In Chromium, against the exported files served statically: card, button, theme showcase and textarea demos for default, ledger, ops, soft and a stale `observatory`, light and dark, link the expected preset (or none), take its accent, radius and font, and follow dark mode; a same-origin iframe swaps soft → ops → default → ledger live from a parent's storage change. Not run: the website's docs pages in a browser (the React picker itself), the inline examples rendered in a page (no route imports them).
