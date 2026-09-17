# OSS docs: new topic pages

**Index status:** completed. **Indexed:** 2026-09-05. Historical record; completion is recorded in the phase/progress notes below. Deferred items require a separately scoped task.

Status: **COMPLETE 2026-08-18**.

Chunk #2 of the follow-up from the 2026-08-17 review of the free `StellarAdmin.TagHelpers`
docs (review notes: session artifact "oss-review-notes", §2.1 "Missing topics"; the bug sweep
in `oss-bug-sweep.md` is done). Scope: the topics a new consumer needs that no page covers
today. Copy fixes to *existing* pages, re-theming and per-component example gaps are the
later content/coverage chunks and stay out.

Paths: `docs/` = `website/content/docs/tag-helpers/`, `Pages/` =
`stellar-admin-pro/docs/DocsSamples/Pages/`, `TH/` =
`stellar-admin/src/StellarAdmin.TagHelpers/`.

## Sources of truth used

- `stellar-admin/readme.md` — already has "Customizing the theme" and "Using the design
  tokens in your own markup" (`theme-tokens.css` copy step); the website has zero hits for
  `theme-tokens`.
- `stellar-admin/skills/stellar-admin-theming/SKILL.md` — pick a theme, override tokens,
  `.dark` class, `ConfigureMenu`, semantic-token table. Content is right; product/package
  names are pre-rename (`StellarAdmin.UI`, `stellar-admin-ui.<theme>.css`) and must not be
  copied verbatim.
- `TH/StellarAdminUIBuilder.cs` (`AddIcon`, `AddIconPack<T>`, `ConfigureMenu`),
  `StellarAdminUIMenuOptions`, `MenuColor`/`MenuAppearance`/`MenuAccent`, `Icons/IIconPack`,
  `IconDefinition`, `SvgShape`, `LucideIconPack`.
- `TH/Client/js/stellar-admin-ui.ts` — bundle = seven `sel-*` light-DOM components
  (collapsible, dialog, dropdown-menu, input-otp, sidebar, slider, table-selection) + the
  `interestfor` polyfill + `window.stellarAdmin = { dialog, alertDialog }`.
- `TH/Client/css/theme.css` (`:root` / `.dark` token blocks), `theme-tokens.css`
  (`@custom-variant dark (&:is(.dark *))` + `@theme inline` map).

## Pages to add

Nav (`docs/meta.json`): keep "Getting Started" as is; the "Customization" group becomes
`theming`, `icons`, `javascript`, `templated-views` (order: what everyone needs first). `label`
goes into `components/meta.json` under Forms, alphabetical (between `input-otp` and `radio`).

### 1. Theming & dark mode — `docs/theming.mdx`

Sections:
- **A theme is a stylesheet** — the eight names, the `<link>` from Installation, switching
  = swapping the link. Cross-link Installation step 4 to this page (one sentence, no rewrite).
- **Customising a theme** — every colour/radius is a CSS custom property; redeclare in your
  own stylesheet (`:root { --primary: ...; --radius: ... }` + `.dark { ... }`), no build
  tooling. List the token families briefly (base palette, `sidebar-*`, `chart-*`, `--radius`).
- **Dark mode** — `.dark` on an ancestor (`<html class="dark">`); every bundle carries both
  token sets; the toggle logic (cookie/`prefers-color-scheme` script) is the app's, with a
  minimal example script that honours a saved preference and `prefers-color-scheme` (the
  same shape the website's own demo pages use).
- **Using the tokens in your own markup** — only if the app runs its own Tailwind v4 build:
  copy `theme-tokens.css`, `@import` it after `tailwindcss`; utilities compile to `var(--...)`
  and the values still come from the linked theme stylesheet. Semantic-token table from the
  skill (primary/surfaces/muted/accent/destructive/border/ring) with the good-vs-avoid
  snippet.
- **Menu surfaces** — `ConfigureMenu` is theming (colour/appearance/accent of floating
  menus), so it lives here: `AddUI().ConfigureMenu(menu => { Color / Appearance / Accent })`,
  the three enums and defaults, and a note that it currently applies to Dropdown Menu
  (content + sub-content). No new demo partial; how the variants are shown depends on the
  "Decision needed" below.

### 2. Icons — `docs/icons.mdx`

Sections:
- **Built-in icons** — Lucide ships by default with `AddUI()`; `<sa-icon name="...">`;
  finding a name (move the Lucide screenshot paragraph here from `icon.mdx` and cross-link
  from `icon.mdx`, which stays the component reference for `<sa-icon>` itself).
- **Adding a single icon** — `AddIcon("voyager-logo", new IconDefinition(attributes,
  shapes))`, with a complete `IconDefinition`/`SvgShape` example (viewBox + a `path`).
- **Adding an icon pack** — implement `IIconPack.GetIcons()` returning
  `IDictionary<string, IconDefinition>`, register with `AddIconPack<MyPack>()`; note that a
  later pack wins on duplicate names.
- **API reference** — `IIconPack`, `IconDefinition`, `SvgShape`, `AddIcon`, `AddIconPack<T>`
  (TypeTable-style, consumer vocabulary only).
- Demo: one new DocsSamples partial `Pages/Icon/_Custom.cshtml` rendering a custom
  registered icon (register a small `VoyagerIconPack` in DocsSamples `Program.cs` — e.g. a
  "voyager-compass" path) so the page has a live example, exported through the generator.
  Prose caveats to state honestly: `AddIcon` throws if the name already exists (so you can't
  override a Lucide icon that way — use a pack), name matching is case-insensitive... —
  **verify** both against `DefaultIconManager` before writing; the review noted the
  behaviour flips after the first pack, so if it's genuinely inconsistent I'll document the
  reliable subset (exact-name registration) and list the inconsistency in the code-consistency
  chunk instead of papering over it.

### 3. JavaScript & interactivity — `docs/javascript.mdx`

Sections:
- **What `stellar-admin.js` is** — one bundle, `defer`, no dependencies, no jQuery/htmx;
  the `<script>` line from Installation. What is *not* included (htmx, Tailwind runtime).
- **Web components** — the `sel-*` elements the tag helpers render into (table: tag helper
  → element → what the script adds: state sync, keyboard, aria), all light DOM (so `class`
  and Tailwind utilities apply as usual, and the server-rendered markup is complete without
  JS where possible).
- **Invoker Commands** — `command`/`commandfor` is how buttons drive dialogs, sheets,
  collapsibles, sidebar and dropdowns; native (`show-modal`, `close`, `toggle-popover`) vs
  custom (`--toggle`, `--show`, `--hide`) commands; the trigger must be a `<button>`;
  browser-support note with the MDN link (no polyfill bundled — this is a statement of fact
  the reader needs before shipping; I will not quote version numbers).
- **Interest invokers** — `interestfor` for hover tooltips/popovers; the polyfill *is*
  bundled; link Tooltip/Popover pages.
- **`window.stellarAdmin`** — `dialog()` and `alertDialog()`; one-paragraph intro and links
  to the two existing JS helper pages (no duplication).
- **Dialogs and scroll lock** — `sel-dialog` locks body scroll for modal dialogs and mirrors
  `data-open`; mention once here and add a one-line cross-link from dialog/sheet/alert-dialog
  "API Reference" sections (three one-liners, no rewrites).
- No new demo partials; snippets only.

### 4. Label — `docs/components/label.mdx` (+ `Pages/Label/`)

Follows the component page template (intro demo, Usage, Examples, API Reference):
- Intro demo: `<sa-label for="...">` + `<sa-input>` pair.
- Examples: **With input** (explicit `for`), **Model binding** (`asp-for` — text comes from
  `[Display]`; child content overrides it), **Inside a field** — states plainly that inside
  `<sa-field>` you should use `<sa-field-label>` (which carries `data-slot="field-label"` and
  participates in horizontal layouts) and `<sa-label>` is for standalone use.
- API: `asp-for`, `for` (pass-through), child content overrides generated text; renders
  `<label data-slot="label" class="sa-label">`.
- New DocsSamples: `Pages/Label/Index.cshtml(.cs)`, `_Intro`, `_ModelBinding`; generator
  entries `Label/_Intro`, `Label/_ModelBinding`; nav entry.

## Small edits to existing pages (cross-links only)

- `docs/installation.mdx` step 4: link "switching themes" to Theming; add a sentence after
  step 4 pointing to JavaScript & interactivity for what the script does.
- `docs/components/icon.mdx`: keep the `name` lookup, link "Icons" for custom icons/packs.
- `docs/components/dropdown-menu.mdx`: link `ConfigureMenu` on Theming.
- `docs/components/dialog.mdx`, `sheet.mdx`, `alert-dialog.mdx`: one line each linking the
  scroll-lock note.
- `docs/templated-views.mdx`: nothing (its own review items — discarded child output,
  `ViewData` copy — belong to the content sweep, and the sibling-slot bug is fixed).

## Decision needed (Jerrie)

**DocsSamples sets `MenuColor.Inverted`** (`Program.cs:22`), so every published dropdown
demo renders an inverted (dark) menu that a reader's default app won't produce, and nothing
on the Dropdown Menu page says so. Options:
1. **Drop it from DocsSamples** so demos show the default, and add a "Menu options" example
   on the Dropdown Menu page whose prose says "configured app-wide via `ConfigureMenu`" and
   shows the inverted/translucent/bold variants via a screenshot-free description. Honest,
   but no live inverted demo. *(My recommendation — demos should show defaults.)*
2. Keep it and add a callout on the Dropdown Menu page + Theming page explaining the demos
   are configured `Inverted`.
3. Keep it and add per-instance overrides (`color`/`appearance`/`accent` attributes on
   `<sa-dropdown-menu-content>`) so demos can show each — that's a feature, belongs in the
   feature-decisions chunk, so not for this plan unless you want it now.

## Phases

1. [x] **Theming & dark mode** page + Installation/Dropdown cross-links; nav. (2026-08-18)
2. [x] **Icons** page + `Pages/Icon/_Custom` demo + DocsSamples `VoyagerIconPack`
   (suitcase, compass). Verified `DefaultIconManager`: names case-insensitive once Lucide is
   loaded; `AddIcon` throws on an existing name; a later pack's icons replace existing ones.
   (2026-08-18)
3. [x] **JavaScript & interactivity** page + dialog/sheet/alert-dialog one-liners; claims
   checked against `Client/js` (`dialog().showAsync()`, `result.data`, sidebar
   `--open-mobile`/`--close-mobile`). (2026-08-18)
4. [x] **Label** page (`components/label.mdx`) + `Pages/Label/{_Intro,_ModelBinding,
   _WithField}` + DocsStatic model + generator entries + DocsSamples nav; demos regenerated
   (340 partials). (2026-08-18)
5. [x] `MenuColor.Inverted` dropped from DocsSamples (Jerrie: option 1, no inverted examples for
   now); dropdown demos regenerated with the default menu. (2026-08-18)

Each phase: stop-and-review, commit on request. Copy follows the Voyager Travel theme and
"..." (no Unicode ellipsis).
