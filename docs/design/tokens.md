# Theme tokens

The public theming vocabulary of `stellar-admin.css`. Approved in phase 1 of the [theme token migration](../plans/theme-token-migration.md); `src/StellarAdmin.TagHelpers/Client/css/tokens.css` implements it. Names here are public API once released. A name not listed here, and anything starting `--_`, is internal. The knobs are also listed for tools in `Client/css/knobs.json` (shipped as `stellar-admin.knobs.json`), which the CSS build checks against `tokens.css`.

## Tiers

| Tier | What | Who sets it |
| --- | --- | --- |
| 0 | Knobs: design decisions (`--sa-accent`, `--sa-radius`, `--sa-density`, …) | Themes and presets, usually only these |
| 1 | Foundation tokens, derived from the knobs (`--sa-color-*`, `--sa-radius-*`, `--sa-space-*`, …) | Themes, rarely, to override one derivation |
| 2 | Raw rules in `@layer sa.overrides` | Escape hatch; each rule points to a missing knob |

Layer order: `theme, base, sa.reset, sa.tokens, sa.components, components, sa.theme, sa.overrides, utilities`. Tailwind's layers are named so that an author's utilities stay above the components on a page that also loads Tailwind CSS; the first stylesheet that declares layers fixes the order, so a Tailwind build loaded before `stellar-admin.css` starts with the same statement. Themes and presets go in `sa.theme`.

## Presets and the Tailwind adapter

The default theme needs no file. The shipped presets are knob files linked after `stellar-admin.css`: `presets/ledger.css` (warm paper, indigo, roomier, tactile buttons; Lexend), `presets/ops.css` (dense, sharp, strong lines, orange; IBM Plex Sans) and `presets/soft.css` (pills, roomy, violet; Figtree). An app's own theme file is the same shape and goes after the preset.

`stellar-admin.tailwind.css` is an `@theme inline` file for an app's own Tailwind build: spacing, small text sizes, fonts, radius steps and the shadcn colour names (`--color-primary`, `--color-muted-foreground`, …) map onto the tokens. Import it after `@import "tailwindcss"`, with the layer order declared first.

## Scopes

Formulas resolve on `:root`, `.dark`, `[data-density]` and `[data-sa-scope]`, so a knob set on any of these re-derives everything inside it.

- `.dark` on any element switches that subtree to dark mode.
- `data-density="compact"` (0.8) or `"comfortable"` (1.2) sets the density of a section.
- `data-sa-scope` marks an element that sets its own knobs, such as a branded panel.

## Knobs (tier 0)

### Colour

| Knob | Default | Meaning |
| --- | --- | --- |
| `--sa-accent` | `oklch(0.5 0.16 258)` | Brand colour: primary actions, focus, selection, bold highlights |
| `--sa-danger` | `oklch(0.58 0.22 27)` | Destructive actions and errors |
| `--sa-success`, `--sa-warning`, `--sa-info` | green, amber, blue | Status colours |
| `--sa-neutral-hue` | `255` | Hue the greys lean toward |
| `--sa-neutral-chroma` | `0.006` | How much the greys lean (0 is pure grey; about 0.04 at most) |
| `--sa-accent-tint` | `0.15` | 0–1: how much the accent tints hover, selection and highlight washes |
| `--sa-surface-depth` | `0.8` | 0–1: separation between the page and cards (0 is one flat surface) |
| `--sa-nav-tint` | `0.5` | 0–1: the sidebar, from page colour to sunken |
| `--sa-line-strength` | `1` | 0.75–2: dividers and control borders (lower values act as 0.75) |

Optional (unset by default; when unset they follow the knob they refine): `--sa-accent-dark`, `--sa-danger-dark`, `--sa-neutral-hue-dark`, `--sa-neutral-chroma-dark` (dark mode values), `--sa-on-accent` (text on the accent, overriding the automatic choice), `--sa-focus-color` (focus ring colour).

### Shape and depth

| Knob | Default | Meaning |
| --- | --- | --- |
| `--sa-radius` | `6px` | Control corners; `9999px` for pill controls |
| `--sa-pills` | `0` | 0 or 1: tags (badges, chips, the switch) are pills, or follow `--sa-radius` |
| `--sa-outlines` | `1` | 0 or 1: filled parts (buttons, badges, chips, the switch track) also draw an outline |
| `--sa-relief` | `0` | 0 flat, 1 raised (drop shadow under buttons), 2 tactile (also bottom edge, hover lift, pressed state, recessed fields) |
| `--sa-sheen` | `1` | 0 flat, 1 a lit surface on solid buttons (default, secondary, solid destructive; in light mode the secondary is white shading to grey with a full edge, in dark its grey at 60%): a top-to-bottom gradient, a highlight along the top edge, a darker edge and a pressed shadow. Under a light label the fill darkens 10% so the lit top keeps its contrast |
| `--sa-stroke` | `1px` | Border width |
| `--sa-elevation` | `0.7` | Shadow strength: 0 flat, 1 soft, 2 or more floating |
| `--sa-press-offset` | `1px` | How far a button moves when pressed |

Optional: `--sa-radius-outer` (container corners, otherwise derived from `--sa-radius`).

### Space and type

| Knob | Default | Meaning |
| --- | --- | --- |
| `--sa-density` | `1` | Spacing and control sizes: 0.75 dense to 1.25 roomy; not type |
| `--sa-type-scale` | `1` | Text sizes |
| `--sa-font-sans` | Inter, system fallbacks | Body and control font |
| `--sa-font-heading` | `var(--sa-font-sans)` | Titles |
| `--sa-font-mono` | system monospace | Code (keyboard keys stay sans; a theme sets the optional `--sa-kbd-font: var(--sa-font-mono)` for mono keys) |
| `--sa-weight-control` | `500` | Buttons, tabs, menu items |
| `--sa-weight-heading` | `600` | Titles |
| `--sa-leading-body` | `1.5` | Line height of running text |
| `--sa-section-label-font`, `--sa-section-label-text`, `--sa-section-label-weight`, `--sa-section-label-transform`, `--sa-section-label-tracking` | inherit, 11.5px, 500, none, 0em | Section labels: menu labels, sidebar group labels, and column heads when `--sa-column-head-label` is on |

### Focus

| Knob | Default | Meaning |
| --- | --- | --- |
| `--sa-focus-width` | `3px` | Focus ring width |
| `--sa-focus-offset` | `0px` | Gap between the control and the ring |

### Structure

Switches are 0 or 1 unless stated; keyword switches take a CSS keyword.

| Knob | Default | Meaning |
| --- | --- | --- |
| `--sa-footer-fill` | `0` | Card footers sit on a tinted band |
| `--sa-footer-rule` | `1` | Card footers have a divider above them |
| `--sa-column-head-label` | `0` | Table column heads take the section label style |
| `--sa-column-head-fill` | `1` | Table column heads sit on a sunken band |
| `--sa-column-head-case` | `none` | Keyword: column head text transform |
| `--sa-column-head-font` | `inherit` | Keyword: column head font family |
| `--sa-current-page-fill` | `0` | Current page in pagination: 0 outline, 0.15 soft, 1 solid accent |
| `--sa-solid-destructive` | `1` | Destructive buttons: 0 soft tint, 1 solid |
| `--sa-filled-secondary` | `1` | Secondary buttons: 1 sunken fill, 0 bordered surface |
| `--sa-link-decoration` | `none` | Keyword: link buttons underlined at rest |

Optional: `--sa-dialog-footer-fill`, `--sa-dialog-footer-rule` (dialog footers, otherwise as card footers); `--sa-kbd-font` (keyboard keys, otherwise sans).

### Fixed

`--sa-radius-round` (9999px: avatars, dots), `--sa-control-icon` (1rem), `--sa-motion-fast` (150ms), `--sa-motion-ease`, `--sa-disabled-opacity` (0.5). Themes may set them; they are not design decisions.

## Foundation tokens (tier 1)

Colour, all `--sa-color-*`:

- Surfaces, back to front: `canvas` (page), `surface` (cards, popovers, table body), `surface-subtle` (footers, table head and foot), `surface-sunken` (tracks, chips, secondary fills), `surface-raised` (menus, dialogs, toasts), `field`, `field-disabled`.
- Ink: `ink`, `ink-muted`, `ink-placeholder`.
- Lines: `line` (dividers, container edges), `line-control`, `line-control-hover`.
- Accent: `accent` (as set; focus, washes), `accent-fill` (solid fills, contrast-adjusted), `on-accent` (text on the fill), `accent-hover`, `accent-text` (accent as text, 4.5:1).
- Danger: `danger`, `danger-fill`, `on-danger`, `danger-soft`, `danger-soft-hover`, `danger-text`.
- Status: `success`, `warning`, `info`.
- Washes: `hover`, `selected`, `highlight`, `on-highlight`; `tint`, a translucent ink fill that steps from whatever it sits on (tab and segmented tracks, keys, chips, skeletons, the progress track).
- Navigation region: `nav-surface`, `nav-ink`, `nav-highlight`, `on-nav-highlight`, `nav-accent`, `nav-line`.
- `scrim` (dialog backdrop).

Contrast guarantees: `on-accent` and `on-danger` are white or near-black by WCAG luminance, and `accent-fill` and `danger-fill` darken a mid-tone colour until white text passes 4.5:1. `accent-text` and `danger-text` pass 4.5:1 on surfaces in both modes.

Type: `--sa-text-2xs`, `-xs`, `-sm`, `-base` (scaled by `--sa-type-scale`), `--sa-text-body`, `--sa-text-control`, `--sa-leading-ui`.

Shape: `--sa-radius-control`, `-control-sm`, `-box` (boxes that can grow taller than a control: textareas, multi-line input groups, alerts, items, attachment media; the control radius capped at the container's, so pill controls don't turn them into ellipses), `-container`, `-overlay`, `-item` (rows in a menu, tabs, nav items; concentric with the overlay), `-inner` (things inside controls), `-tag`, `-round`. `--sa-border-width`.

Size and space (all multiples of `--sa-unit`, 0.25rem × density): `--sa-unit`, `--sa-control-h-xs`, `-h-sm`, `-h`, `-h-lg`, `--sa-control-px`, `--sa-control-gap`, `--sa-space-container`, `-container-sm`, `-cell-x`, `-cell-y`, `-overlay`, `-item-x`, `-item-y`, `-item-gap`.

Focus and validation: `--sa-focus-border`, `--sa-focus-ring-color`, `--sa-focus-ring-width`, `--sa-focus-shadow` (buttons, tabs), `--sa-focus-shadow-field` (inputs), `--sa-invalid-ring-color`, `--sa-invalid-shadow`, `--sa-invalid-focus-shadow`. The shadows are complete `box-shadow` values.

Elevation: `--sa-shadow-control`, `-container`, `-raised` (selected tab), `-overlay` (menus, toasts), `-dialog`. Never `none`: they are composed into shadow lists.

## Components

Components have no tokens of their own. Their rules read the knobs and foundation tokens directly (`var(--sa-color-surface)`, `var(--sa-radius-control)`, `calc(4 * var(--sa-unit))`), so a theme changes every component through the tiers above. The only per-component names are the optional knobs listed with the switches.

A theme that needs one component to differ writes a rule in `sa.overrides`. A need that recurs across themes becomes a new knob.

## Private variables

`--_*` variables are internal and may change in any release. Modifier classes (variants, sizes, `sa-menu-accent-bold`) and dark mode set them to pick a component's values. Scripts also publish measured values on components (`--_slider-thumb-measured`, `--sa-table-row-detail-width`, `-inset-start`, `-inset-end`); these are internal too.
