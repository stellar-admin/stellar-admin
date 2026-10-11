# Theme knobs

The knobs a theme file sets, how to turn a description into knob values, three complete themes as worked examples, and how to link the result to the theme builder. How to write, link and load a theme file is in the [skill's workflow](../SKILL.md#workflow).

The package also ships this list as `stellar-admin.knobs.json` (served at `/_content/StellarAdmin.TagHelpers/stellar-admin.knobs.json`), for the installed version. If a knob below is missing from the app's copy, the app's package predates it: don't use it.

## The knobs

Starred knobs (★) are the essential ones: most themes are made from these alone. Optional knobs are unset by default and follow the knob they refine until set. Ranges are the useful ones; the theme builder's controls use the same.

### Colour

Colours accept any CSS colour; `oklch(lightness chroma hue)` is the easiest to reason about. The accent's fills and text are adjusted for contrast automatically, so any lightness works.

| Knob | Default | Range | Meaning |
| --- | --- | --- | --- |
| `--sa-accent` ★ | `oklch(0.5 0.16 258)` | any colour | Brand colour: primary actions, focus, selection, bold highlights |
| `--sa-accent-dark` | follows the accent | any colour | The accent in dark mode |
| `--sa-on-accent` | automatic | any colour | Text on accent fills, instead of the automatic black or white (contrast is then the theme's job) |
| `--sa-danger` | `oklch(0.58 0.22 27)` | any colour | Destructive actions and errors |
| `--sa-danger-dark` | follows danger | any colour | The danger colour in dark mode |
| `--sa-success`, `--sa-warning`, `--sa-info` | green, amber, blue | any colour | Status colours |
| `--sa-neutral-hue` ★ | `255` | 0–360 | Hue the greys lean toward |
| `--sa-neutral-chroma` ★ | `0.006` | 0–0.04 | How much the greys lean (0 is pure grey) |
| `--sa-neutral-hue-dark`, `--sa-neutral-chroma-dark` | follow the light values | as above | The greys in dark mode |
| `--sa-accent-tint` | `0.15` | 0–1 | How much the accent tints hover, selection and highlight washes |
| `--sa-surface-depth` | `0.8` | 0–1 | Separation between the page and cards (0 is one flat surface) |
| `--sa-nav-tint` | `0.5` | 0–1 | The sidebar, from page colour to sunken |
| `--sa-line-strength` | `1` | 0.75–2 | Dividers and control borders |

### Shape and depth

| Knob | Default | Range | Meaning |
| --- | --- | --- | --- |
| `--sa-radius` ★ | `6px` | 0–24px, or `9999px` | Control corners; `9999px` gives pill controls |
| `--sa-radius-outer` | derived from the radius | 0–32px | Card and dialog corners |
| `--sa-pills` ★ | `0` | 0 or 1 | Tags (badges, chips, the switch) are pills instead of following the radius |
| `--sa-outlines` | `1` | 0 or 1 | Filled parts (buttons, badges, chips, the switch track) also draw an outline |
| `--sa-relief` ★ | `0` | 0, 1, 2 | 0 flat; 1 raised (a shadow under buttons); 2 tactile (also a bottom edge, a hover lift, a pressed state and recessed fields) |
| `--sa-sheen` | `1` | 0 or 1 | Solid buttons get a lit surface: a gradient, a top highlight, a darker edge and a pressed shadow |
| `--sa-stroke` | `1px` | 0–3px | Border width |
| `--sa-elevation` ★ | `0.7` | 0–3 | Shadow strength: 0 flat, 1 soft, 2 or more floating |

### Space and type

| Knob | Default | Range | Meaning |
| --- | --- | --- | --- |
| `--sa-density` ★ | `1` | 0.7–1.4 | Spacing and control sizes, not text: 0.75 dense to 1.25 roomy |
| `--sa-type-scale` | `1` | 0.8–1.2 | Text sizes |
| `--sa-font-sans` ★ | `"Inter", ui-sans-serif, system-ui, sans-serif` | a font stack | Body and control font |
| `--sa-font-heading` ★ | `var(--sa-font-sans)` | a font stack | Titles |
| `--sa-font-mono` | `ui-monospace, "SFMono-Regular", Menlo, Consolas, monospace` | a font stack | Code |
| `--sa-kbd-font` | the body font | `var(--sa-font-mono)` | Keyboard keys |
| `--sa-weight-control` | `500` | 400, 500, 600 | Buttons, tabs, menu items |
| `--sa-weight-heading` | `600` | 400–700 | Titles |
| `--sa-leading-body` | `1.5` | 1.3–1.8 | Line height of running text |

End every font stack with native fallbacks. The theme builder offers Inter, Geist, IBM Plex Sans, Figtree, Source Sans 3, Space Grotesk, Lexend and Archivo for the body; Fraunces, Space Grotesk and Lexend for headings; JetBrains Mono, IBM Plex Mono and Geist Mono for code. Any family the app loads works.

### Section labels

Menu labels, sidebar group labels, and table column heads when `--sa-column-head-label` is on.

| Knob | Default | Range | Meaning |
| --- | --- | --- | --- |
| `--sa-section-label-font` | `inherit` | `inherit`, `var(--sa-font-heading)`, `var(--sa-font-mono)` | Font |
| `--sa-section-label-text` | 11.5px | `var(--sa-text-2xs)` (11px), `var(--sa-text-xs)` (12px), `var(--sa-text-sm)` (14px) | Size |
| `--sa-section-label-weight` | `500` | 400–700 | Weight |
| `--sa-section-label-transform` | `none` | `none`, `uppercase` | Case |
| `--sa-section-label-tracking` | `0em` | 0–0.15em | Letter spacing; pair uppercase with about 0.06em |

### Structure and buttons

| Knob | Default | Range | Meaning |
| --- | --- | --- | --- |
| `--sa-footer-fill` | `0` | 0 or 1 | Card footers sit on a tinted band |
| `--sa-footer-rule` | `1` | 0 or 1 | Card footers have a divider above them |
| `--sa-dialog-footer-fill`, `--sa-dialog-footer-rule` | follow the card footer | 0 or 1 | Dialog and sheet footers |
| `--sa-column-head-label` | `0` | 0 or 1 | Table column heads take the section label style |
| `--sa-column-head-fill` | `1` | 0 or 1 | Table column heads sit on a sunken band |
| `--sa-column-head-case` | `none` | `none`, `uppercase`, `var(--sa-section-label-transform)` | Column head case |
| `--sa-column-head-font` | `inherit` | `inherit`, `var(--sa-font-mono)` | Column head font |
| `--sa-current-page-fill` | `0` | 0 outline, 0.15 soft, 1 solid | The current page in pagination |
| `--sa-solid-destructive` | `1` | 0 or 1 | Destructive buttons are solid (1) or a soft tint (0) |
| `--sa-filled-secondary` | `1` | 0 or 1 | Secondary buttons have a sunken fill (1) or a bordered surface (0) |
| `--sa-link-decoration` | `none` | `none`, `underline` | Link buttons at rest |
| `--sa-press-offset` | `1px` | 0–2px | How far a button moves when pressed |

### Focus

| Knob | Default | Range | Meaning |
| --- | --- | --- | --- |
| `--sa-focus-width` | `3px` | 1–4px | Focus ring width |
| `--sa-focus-offset` | `0px` | 0–4px | Gap between the control and the ring |
| `--sa-focus-color` | a wash of the accent | any colour | Focus ring colour |

## From a description to knobs

Start from the default (or the closest worked example below) and move only the knobs the description asks for. Words in a description usually map like this:

| Description | Knobs |
| --- | --- |
| Dense, compact, data-heavy, power users | `--sa-density` 0.78–0.9; `--sa-radius` 2–4px; `--sa-elevation` 0.3–0.5; `--sa-focus-width` 2px; `--sa-section-label-text: var(--sa-text-xs)` |
| Roomy, airy, relaxed, consumer | `--sa-density` 1.1–1.2; `--sa-radius` 8–10px; `--sa-leading-body` 1.6 |
| Soft, friendly, playful, rounded | `--sa-radius: 9999px`; `--sa-pills: 1`; `--sa-outlines: 0`; `--sa-elevation` 1.5–2; `--sa-accent-tint` 0.5–0.7; `--sa-line-strength` 0.7 |
| Sharp, technical, engineered, serious | `--sa-radius` 0–3px; `--sa-outlines: 1`; `--sa-line-strength` 1.2–1.5; `--sa-sheen: 0`; `--sa-column-head-label: 1`; optionally `--sa-column-head-font: var(--sa-font-mono)` |
| Flat, minimal, quiet | `--sa-elevation` 0–0.3; `--sa-relief: 0`; `--sa-sheen: 0`; `--sa-surface-depth` 0.3–0.5; `--sa-accent-tint` 0.05–0.1 |
| Tactile, physical, raised | `--sa-relief: 2` (or 1 for a lighter touch); `--sa-sheen: 1`; `--sa-elevation` 1 |
| Floating, layered | `--sa-elevation` 1.5–2.5; `--sa-surface-depth` 0.9–1 |
| Editorial, literary, classic | `--sa-font-heading: "Fraunces", Georgia, serif`; `--sa-section-label-transform: uppercase` with `--sa-section-label-tracking: 0.07em`; `--sa-link-decoration: underline`; `--sa-leading-body` 1.6 |
| Warm | `--sa-neutral-hue` 60–90 with `--sa-neutral-chroma` 0.01–0.02 |
| Cool | `--sa-neutral-hue` 220–260 with `--sa-neutral-chroma` 0.01–0.02 |
| Pure grey, neutral | `--sa-neutral-chroma: 0` |
| Muted, calm accent | accent chroma 0.08–0.12; `--sa-accent-tint` 0.05–0.1; `--sa-solid-destructive: 0` |
| Bold, vibrant | accent chroma 0.18–0.22; `--sa-accent-tint` 0.3–0.5; `--sa-current-page-fill: 1` |
| Structured, banded, enterprise | `--sa-footer-fill: 1`; `--sa-column-head-fill: 1`; `--sa-column-head-label: 1` |

Colour names as oklch hues: red 27, orange 48, amber 70, yellow 95, green 150, teal 185, cyan 210, blue 258, indigo 270, violet 295, pink 350. Lightness 0.45–0.65 and chroma 0.12–0.2 give a typical accent; a brand colour can be given as hex instead.

A dark mode that should feel different from the light one (warm paper turning to a cool night) uses the `-dark` knobs; otherwise leave them unset.

## Worked examples

Three complete themes, also offered as starting points in the theme builder.

**Ledger** — warm paper that turns indigo in dark mode, an indigo accent, roomier controls, uppercase labels on banded table heads, a soft focus halo, outlined parts and tactile buttons. Font: Lexend.

```css
@layer sa.theme {
  :root {
    --sa-accent: oklch(0.48 0.18 262);
    --sa-danger: oklch(0.55 0.2 27);
    --sa-neutral-hue: 80;
    --sa-neutral-chroma: 0.012;
    --sa-neutral-hue-dark: 262;
    --sa-neutral-chroma-dark: 0.022;
    --sa-accent-tint: 0;
    --sa-radius: 6px;
    --sa-outlines: 1;
    --sa-pills: 0;
    --sa-elevation: 1;
    --sa-density: 1.125;
    --sa-font-sans: "Lexend", system-ui, sans-serif;
    --sa-weight-heading: 600;
    --sa-section-label-transform: uppercase;
    --sa-section-label-tracking: 0.07em;
    --sa-footer-fill: 0;
    --sa-column-head-label: 1;
    --sa-column-head-fill: 1;
    --sa-column-head-case: var(--sa-section-label-transform);
    --sa-relief: 2;
    --sa-solid-destructive: 1;
    --sa-link-decoration: underline;
    --sa-focus-width: 3px;
    --sa-focus-color: color-mix(in oklab, var(--sa-color-accent) 35%, transparent);
    --sa-surface-depth: 0.6;
    --sa-section-label-text: var(--sa-text-xs);
  }
}
```

**Ops** — dense and sharp, with strong lines, a safety-orange accent, banded table heads and an offset focus ring. Font: IBM Plex Sans.

```css
@layer sa.theme {
  :root {
    --sa-accent: oklch(0.66 0.18 48);
    --sa-neutral-hue: 250;
    --sa-neutral-chroma: 0.015;
    --sa-accent-tint: 0.2;
    --sa-surface-depth: 0.8;
    --sa-line-strength: 1.4;
    --sa-radius: 3px;
    --sa-outlines: 1;
    --sa-pills: 0;
    --sa-radius-outer: 4px;
    --sa-elevation: 0.5;
    --sa-density: 0.78;
    --sa-focus-width: 2px;
    --sa-focus-offset: 2px;
    --sa-font-sans: "IBM Plex Sans", system-ui, sans-serif;
    --sa-column-head-label: 1;
    --sa-column-head-fill: 1;
    --sa-current-page-fill: 1;
    --sa-relief: 0;
    --sa-sheen: 0;
    --sa-solid-destructive: 0;
    --sa-section-label-text: var(--sa-text-xs);
    --sa-footer-fill: 1;
  }
}
```

**Soft** — pill controls, roomy spacing, floating surfaces and a violet accent. Font: Figtree.

```css
@layer sa.theme {
  :root {
    --sa-accent: oklch(0.56 0.2 295);
    --sa-neutral-hue: 295;
    --sa-neutral-chroma: 0.012;
    --sa-accent-tint: 0.7;
    --sa-surface-depth: 0.9;
    --sa-nav-tint: 0.15;
    --sa-line-strength: 0.7;
    --sa-radius: 9999px;
    --sa-radius-outer: 1.375rem;
    --sa-elevation: 1.8;
    --sa-density: 1.15;
    --sa-font-sans: "Figtree", system-ui, sans-serif;
    --sa-footer-fill: 0;
    --sa-footer-rule: 0;
    --sa-current-page-fill: 0.15;
    --sa-pills: 1;
    --sa-outlines: 0;
    --sa-relief: 0;
    --sa-sheen: 0;
    --sa-solid-destructive: 0;
    --sa-section-label-text: var(--sa-text-xs);
    --sa-column-head-fill: 0;
  }
}
```

These set more knobs than a theme needs; some repeat a default to make the choice explicit. A theme of four or five knobs is normal.

## Link the result to the theme builder

The theme builder reads a theme from its URL, so a link shows the theme on real components and lets the user tune it. Build the link from the knobs the theme sets:

`https://www.stellaradmin.com/theme-builder#v1:` followed by `name=value` pairs joined with `&`, where each name drops its `--sa-` prefix and each value is URL-encoded (`encodeURIComponent`: spaces become `%20`, commas `%2C`, quotes `%22`).

For a warm, editorial, dense theme with a green accent:

```css
/* StellarAdmin theme: warm, editorial, dense, green accent. Link it after stellar-admin.css.
   Builder: https://www.stellaradmin.com/theme-builder#v1:accent=oklch(0.52%200.13%20150)&neutral-hue=80&neutral-chroma=0.014&radius=4px&density=0.85&font-heading=%22Fraunces%22%2C%20Georgia%2C%20serif&section-label-transform=uppercase&section-label-tracking=0.07em&link-decoration=underline */
@layer sa.theme {
  :root {
    --sa-accent: oklch(0.52 0.13 150);
    --sa-neutral-hue: 80;
    --sa-neutral-chroma: 0.014;
    --sa-radius: 4px;
    --sa-density: 0.85;
    --sa-font-heading: "Fraunces", Georgia, serif;
    --sa-section-label-transform: uppercase;
    --sa-section-label-tracking: 0.07em;
    --sa-link-decoration: underline;
  }
}
```

That theme needs Fraunces loaded in the layout:

```html
<link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Fraunces:wght@400..700&amp;display=swap"/>
```
