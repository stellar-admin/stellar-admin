---
name: theming
description: >-
  Configures the look of a StellarAdmin app — picking a theme stylesheet, customizing theme
  values with CSS variables, enabling dark mode, using the design tokens in your own
  markup, and tuning the menu color / appearance / accent. Use when the user wants to
  change the StellarAdmin theme, add or toggle dark mode, adjust menu or dropdown
  appearance, or asks which colors or classes to use with StellarAdmin.
metadata:
  author: StellarAdmin
---

# Theming StellarAdmin

**A theme is a stylesheet.** The package ships one self-contained CSS bundle per theme; the layout links exactly one, and switching themes means switching that `<link>`. Nothing about the theme is configured in C#. Dark mode is a CSS class, and day-to-day styling means using the semantic design tokens instead of hard-coded colors.

## Pick a theme (the layout `<link>`)

StellarAdmin ships fifteen themes: `aurora`, `concourse`, `ice`, `ledger`, `shadcn.vega`, `shadcn.nova`, `observatory`, `parallax`, `shadcn.luma`, `shadcn.lyra`, `shadcn.maia`, `meridian`, `shadcn.mira`, `shadcn.rhea`, `shadcn.sera`. Aurora, Concourse, Ice, Ledger, Meridian, Observatory, and Parallax are independently designed; the other eight derive from shadcn/ui.

The eight shadcn-derived themes share the same base palette and radius; their component geometry, density, typography, and token usage differ. Ledger supplies its own warm light palette, charcoal dark palette, typography, and raised button treatment. Select a theme for its visual design; override semantic variables when the app needs different brand colours.

```razor
<link rel="stylesheet" href="/_content/StellarAdmin.TagHelpers/stellar-admin.observatory.css" asp-append-version="true"/>
```

The `shadcn.` prefix is reserved for upstream-derived themes; independent themes keep their unprefixed names. When upgrading, replace old links such as `stellar-admin.nova.css` with `stellar-admin.shadcn.nova.css`; old bundle names are no longer shipped.

Observatory is the default used in the documentation and the recommended starting point when the user has no theme preference. To change the theme, change `observatory` to another theme name — that's the whole operation. You **must** link exactly one; without it, components render unstyled.

Preview all fifteen themes with the StellarAdmin documentation demo picker. The [shadcn/ui Create page](https://ui.shadcn.com/create) covers the eight upstream-derived styles; Aurora, Concourse, Ice, Ledger, Meridian, Observatory, and Parallax are specific to StellarAdmin.

### Shadcn fonts

The shadcn themes include optional StellarAdmin font defaults with native fallbacks. Luma, Mira, Rhea and Vega use Inter; Nova uses Geist; Maia uses Figtree; Lyra uses JetBrains Mono for both body and headings; Sera uses Noto Sans for body text and Playfair Display for component titles. These are StellarAdmin defaults, not required upstream pairings. Rhea matches Luma.

Optionally load the selected family at weights 400–700, then link its theme bundle. For Sera:

```html
<link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Noto+Sans:wght@400..700&amp;family=Playfair+Display:wght@400..700&amp;display=swap"/>
<link rel="stylesheet" href="/_content/StellarAdmin.TagHelpers/stellar-admin.shadcn.sera.css" asp-append-version="true"/>
```

Sera’s `--sa-sera-font-display` stack falls back to Georgia, Times New Roman, Times and `serif`. Use `sa-font-heading` for headings in your own markup. Lyra falls back to the native mono stack below; Maia prefers Avenir Next, Avenir and Segoe UI before `system-ui, sans-serif`. Other shadcn body stacks use `system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif`. Apps can override `--font-sans`, `--font-mono`, or Sera’s heading variable with ordinary CSS.

### Parallax

Parallax uses cool blue-grey surfaces, a red-orange action accent, nested 6px and 4px corners, and elevated cards and overlays in both modes. Dark inputs recess below their cards. Space Grotesk sets headings and UI; JetBrains Mono sets explicitly marked numbers, identifiers and shortcuts.

Optionally load Space Grotesk (normal 400/500/600) and JetBrains Mono (normal 400/500) from your application layout, then link the Parallax bundle:

```html
<link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Space+Grotesk:wght@400;500;600&amp;family=JetBrains+Mono:wght@400;500&amp;display=swap"/>
<link rel="stylesheet" href="/_content/StellarAdmin.TagHelpers/stellar-admin.parallax.css" asp-append-version="true"/>
```

Fonts can be self-hosted; the stylesheet does not fetch them. Use the usual `dark` class for dark mode. Default controls are 32px high, small controls 26px, and large controls 40px. Table cells use 10px vertical padding. Use `font-mono tabular-nums` for data in your own markup. Menus use trailing checkmarks without a persistent selected fill, and horizontal line tabs show only the active underline.

Parallax separates accent fills (`--primary`) from accent text (`--sa-parallax-accent-ink`) so small orange labels remain legible in light mode. Coordinate the theme-private hover, active and tint values when customizing the accent. Chart colors use neutral and status roles.

### Aurora

Aurora uses cool grey-green surfaces, deep teal accents in light mode and bright teal in dark mode, square corners, and flat bordered cards. Sliders have square tracks and thumbs; switches, radio controls, avatars and status dots stay round. Dark inputs use a recessed surface.

Optionally load Archivo (headings and UI, normal 400/500/600) and IBM Plex Mono (numbers, identifiers and shortcuts, normal 400/500) from your application layout, then link the Aurora bundle:

```html
<link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Archivo:wght@400;500;600&amp;family=IBM+Plex+Mono:wght@400;500&amp;display=swap"/>
<link rel="stylesheet" href="/_content/StellarAdmin.TagHelpers/stellar-admin.aurora.css" asp-append-version="true"/>
```

The fonts can be self-hosted. The stylesheet does not fetch them; use the usual `dark` class to switch modes. Default controls are 30px high, small controls 24px, and large controls 38px. Tables use 9px vertical cell padding. Use `font-mono tabular-nums` for numbers and identifiers in your own markup, and provide larger touch targets in mobile layouts. Dropdown checkbox and radio items use trailing checkmarks without a persistent selected row fill. Default tabs match segmented controls; horizontal line tabs show only the active underline.

Coordinate theme-private `--sa-aurora-*` accent hover/active and tint companions when changing brand colours. Keep the existing component behaviors and responsive layouts.

### Concourse

Use `stellar-admin.concourse.css` as the single theme bundle. Optionally load Source Sans 3 (UI, weights 400/500/600/700) and IBM Plex Mono (identifiers and values, weights 400/500) from the app layout, self-hosted or through a font provider. The stylesheet does not fetch fonts. Existing tag helpers and the `.dark` class work unchanged.

Concourse has cool grey surfaces, a blue accent, 4px corners, and 34px default controls. Actions use colour changes without press movement; neutral hover feedback stays distinct from persistent selection. Use existing components rather than the handoff's `.cc-*` classes. Concourse-specific `--sa-concourse-*` variables are implementation details. When overriding the primary colour, coordinate its `--sa-concourse-primary-hover`, `--sa-concourse-primary-pressed`, and `--sa-concourse-primary-border` companions in both modes; destructive actions have corresponding hover, border, and foreground companions. Touch target sizing belongs to the app's responsive composition.

### Ledger

Use `stellar-admin.ledger.css` as the single theme bundle. Optionally load Lexend (UI, weights 300–700) and JetBrains Mono (identifiers and shortcuts, weights 400–500) from the app's layout, self-hosted or through a font provider. The library stylesheet does not fetch fonts. Existing tag helpers and the `.dark` class work unchanged, including shared surfaces used by Pro.

Preserve raised borders and shadows on primary, secondary, outline, and destructive buttons; ghost and link actions are flat. Do not reproduce the original prototype's `.ldg-*` classes. Use StellarAdmin's normal components. Ledger-specific `--sa-ledger-*` variables are implementation details rather than shared tokens. If customising primary/destructive colours, coordinate their `--sa-ledger-primary-hover`, `--sa-ledger-primary-border`, `--sa-ledger-destructive-hover`, `--sa-ledger-destructive-border`, and `--sa-ledger-destructive-foreground` companions in both modes.

### Ice

Use `stellar-admin.ice.css` as the single theme bundle. Optionally load IBM Plex Sans (UI, weights 400/500/600) and JetBrains Mono (data and shortcuts, weights 400/500/700) from the app layout, self-hosted or through a font provider. The stylesheet does not fetch fonts. Existing tag helpers and the `.dark` class work unchanged.

Ice uses square geometry, hairline borders, no elevation shadows, blue accents, and 28px default controls. Preserve neutral hover feedback, indicator-only dropdown check/radio selection, and outlined destructive actions; destructive confirmation buttons inside alert-dialog footers are filled. Use `font-mono tabular-nums` for numeric app content. Choose larger controls where the app needs more generous touch targets. Theme-private `--sa-ice-*` tokens retain separate readable accent, bright fill/marker, and light/dark ink roles; coordinate these when changing the accent.

### Observatory

Use `stellar-admin.observatory.css` as the single theme bundle. Optionally load IBM Plex Sans (normal 400/500/600) and IBM Plex Mono (normal 400/500) from the app layout, self-hosted or through a font provider. The stylesheet does not fetch fonts; existing tag helpers and the `.dark` class work unchanged.

Observatory uses cool neutral surfaces, blue/cyan accents, 4px corners, 32px default controls, and compact table spacing (7px vertical cell padding). Dark cards have no shadow. Compact is built in, with no density setting required. Use `font-mono tabular-nums` for numeric app content and larger controls where touch interaction needs them. Preserve indicator-only dropdown selection, neutral hover feedback, and matching default-tab/segmented-control surfaces. Theme-private `--sa-observatory-*` tokens are implementation details; coordinate accent hover/active and tint companions when changing brand colours.

### Meridian

Use `stellar-admin.meridian.css` as the single theme bundle. Optionally load Instrument Sans (display headings, normal 600), Work Sans (UI text, normal 400/500/600), and JetBrains Mono (data/shortcuts, normal 400/500) from the app layout, self-hosted or through a font provider. The stylesheet does not fetch fonts; existing tag helpers and the `.dark` class work unchanged.

Meridian uses warm paper/deep umber surfaces, brass accents, 2px corners, 32px default controls and 11px vertical table-cell padding. Small controls are 26px and large controls 40px. Keep the display/body/mono type roles distinct; use `font-mono tabular-nums` for numeric app content. Preserve trailing checkmarks for both dropdown checkbox and radio selection, neutral hover feedback, and matching default tabs/segmented controls. Choose larger controls for touch use. Coordinate theme-private `--sa-meridian-*` accent hover/active and tint companions when changing brand colours.

## Optional theme fonts

All fifteen themes work with only their StellarAdmin stylesheet. Their preferred families come first in the CSS stacks; if those fonts are unavailable, the browser uses native fallbacks automatically. Omit the Google Fonts stylesheet and preconnect links for zero font downloads. No JavaScript or additional fallback CSS is required. Font sizes, weights and spacing stay the same.

Aurora, Meridian and Parallax fall back to `"Helvetica Neue", Helvetica, Arial, "Liberation Sans", system-ui, sans-serif`. Concourse, Ice and Observatory use `system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif`. Ledger uses `"Avenir Next", Avenir, "Segoe UI", system-ui, sans-serif`; Avenir is optional and not guaranteed on every device. Meridian headings prefer Instrument Sans, then Work Sans, before the native stack.

All seven mono stacks fall back to `ui-monospace, "SFMono-Regular", Menlo, Consolas, "Liberation Mono", "DejaVu Sans Mono", monospace`. Native results depend on the operating system and installed fonts; do not promise identical rendering across platforms.

For the original typography, load only the selected theme’s families and weights from the app layout, using Google Fonts or self-hosted files. `display=swap` shows fallback text while loading and switches when ready, potentially changing wrapping. `display=optional` avoids a late swap but may keep native fonts for that page view. The StellarAdmin demos use `display=swap`. Fallback stacks alone do not reduce the download size when the app still requests web fonts.

## Dark mode

Dark mode is a **class-based variant** — every theme bundle already carries both the light (`:root`) and dark (`.dark`) token values, so there's no extra CSS and no configuration method.

Enable it by putting the **`dark` class on a containing element** (usually `<html>`); every descendant then reads the dark values:

```razor
<html lang="en" class="dark">
```

How the class gets there is up to the app — server-rendered from a saved preference, or client-side. A minimal client script that honors a saved choice and falls back to the OS setting, placed in `<head>` **before** the theme stylesheet:

```html
<script>
    (function () {
        var saved = null;
        try { saved = localStorage.getItem("theme"); } catch (e) { }
        var prefersDark = window.matchMedia("(prefers-color-scheme: dark)").matches;
        var dark = saved === "dark" || (saved !== "light" && prefersDark);
        document.documentElement.classList.toggle("dark", dark);
        document.documentElement.style.colorScheme = dark ? "dark" : "light";
    })();
</script>
```

Setting `color-scheme` alongside the class keeps native controls — scrollbars, date pickers, form elements — in step with the page.

## Customize theme values

Every color and radius is a CSS custom property; the compiled rules all reference `var(--…)`. Override by redeclaring the properties in the app's own stylesheet, **after** the theme `<link>` — no build tooling required:

```html
<link rel="stylesheet" href="/_content/StellarAdmin.TagHelpers/stellar-admin.shadcn.nova.css" asp-append-version="true"/>
<style>
  :root {
    --primary: oklch(0.6725 0.1362 42);
    --radius: 0.5rem;
    --font-sans: "JetBrains Mono", ui-monospace, monospace;
  }
  .dark {
    --primary: oklch(0.705 0.13 42);
  }
</style>
```

The variables you can override:

| Variable | What it colors |
|----------|----------------|
| `--background` / `--foreground` | The base surface and text color — page shell, sections, body text. |
| `--card` / `--card-foreground` | Raised surfaces: Card, dashboard panels, settings panels. |
| `--popover` / `--popover-foreground` | Overlay surfaces: Popover, DropdownMenu and similar floating components. |
| `--primary` / `--primary-foreground` | The brand color — default Button, selected states, active accents. |
| `--secondary` / `--secondary-foreground` | Filled but quieter elements: secondary buttons and badges. |
| `--muted` / `--muted-foreground` | Recessive surfaces and text: descriptions, placeholders, helper text, empty states. |
| `--accent` / `--accent-foreground` | Interaction highlights: ghost buttons, hovered rows, highlighted menu entries. |
| `--destructive` | Danger and error: destructive buttons, invalid form states, destructive menu items. |
| `--border` | Borders and separators between cards, menus, tables, layout. |
| `--input` | Borders and surfaces of form controls. |
| `--ring` | The focus ring on any focusable control. |
| `--chart-1` ... `--chart-5` | The default series palette for charts. |
| `--sidebar` / `--sidebar-foreground` | The Sidebar surface and its default text. |
| `--sidebar-primary` / `--sidebar-primary-foreground` | The sidebar's most prominent elements: active items, icon tiles, badges. |
| `--sidebar-accent` / `--sidebar-accent-foreground` | Hovered and selected sidebar entries. |
| `--sidebar-border` / `--sidebar-ring` | Borders and focus rings inside the sidebar. |
| `--radius` | The base corner radius the `radius-*` variables derive from. |
| `--font-sans` / `--font-mono` | The type families. |

Values live on `:root` with dark-mode overrides under `.dark` — override both when a color needs to differ between modes.

## Use the design tokens in your own markup

The theme bundle styles the `<sa-*>` components. It does **not** make token utilities available to your own markup: writing `class="bg-primary"` on your own `<div>` does nothing, because that utility only exists inside the prebuilt bundle.

If the app runs its own Tailwind v4 build, copy [`theme-tokens.css`](https://github.com/stellar-admin/stellar-admin/blob/master/src/StellarAdmin.TagHelpers/Client/css/theme-tokens.css) (`src/StellarAdmin.TagHelpers/Client/css/theme-tokens.css`) into the project and import it from the Tailwind entry stylesheet:

```css
@import "tailwindcss";
@import "./theme-tokens.css";
```

Now `bg-primary`, `text-muted-foreground`, `bg-card`, `rounded-lg`, `dark:*` and the rest work in your own markup, and adapt to theme changes and customizations automatically. The file carries only the token *vocabulary* — the values still come from the linked theme bundle at runtime, so keep the `<link>` in place.

| Use | Tokens |
|-----|--------|
| Primary action | `bg-primary` / `text-primary-foreground` |
| Surfaces | `bg-card` / `text-card-foreground`, `bg-popover` / `text-popover-foreground` |
| Muted / secondary | `text-muted-foreground`, `bg-secondary`, `bg-muted` |
| Accent (hover/active) | `bg-accent` / `text-accent-foreground` |
| Danger | `text-destructive` |
| Borders / inputs / focus | `border`, `bg-input`, `ring-ring` |

Also available: the `sidebar-*` and `chart-*` token families and the `--radius` variable.

```razor
<!-- good: adapts to theme + dark mode -->
<div class="rounded-lg border bg-card text-card-foreground p-4">...</div>

<!-- avoid: hard-coded palette colors don't follow the theme -->
<div class="rounded-lg border border-gray-200 bg-white text-gray-900 p-4">...</div>
```

## Menu surfaces (`Program.cs`)

Floating menu surfaces — Dropdown Menu content and sub-menus — have three app-wide appearance settings, configured once by chaining `ConfigureMenu` off `AddTagHelpers()`:

```csharp
using StellarAdmin;
using StellarAdmin.TagHelpers;

builder.Services.AddStellarAdmin()
    .AddTagHelpers()
    .ConfigureMenu(menu =>
    {
        menu.Color = MenuColor.Inverted;              // Default | Inverted
        menu.Appearance = MenuAppearance.Translucent; // Solid | Translucent
        menu.Accent = MenuAccent.Bold;                // Subtle | Bold
    });
```

| Setting | Values | Meaning |
|---------|--------|---------|
| `Color` | `Default`, `Inverted` | `Default` renders the menu in the page's current scheme; `Inverted` renders it in the dark scheme regardless of the page. |
| `Appearance` | `Solid`, `Translucent` | `Solid` is an opaque popover surface; `Translucent` is frosted, with a backdrop blur. |
| `Accent` | `Subtle`, `Bold` | The highlight on the focused or hovered item — `Subtle` uses the muted accent color, `Bold` a solid primary highlight. |

Defaults are `Color=Default`, `Appearance=Solid`, `Accent=Subtle` — only call `ConfigureMenu` to change them.

## Rules

1. Pick the theme by linking one `stellar-admin.<theme>.css` in the layout; switch themes by switching the `<link>`. No C# theme configuration exists.
2. Customize theme values by redeclaring the CSS custom properties (`--primary`, `--radius`, ...) in the app's own stylesheet, after the theme link. Override `.dark` too where the value should differ.
3. Enable dark mode with the `dark` class on an ancestor; don't write your own dark CSS — the tokens are already themed for both modes. Set `color-scheme` alongside it.
4. To use token utilities in your own markup, the app needs its own Tailwind build plus `theme-tokens.css`. Without that, `class="bg-primary"` silently does nothing.
5. Prefer semantic tokens (`bg-primary`, `text-muted-foreground`, `bg-card`, `border`, `text-destructive`) over hard-coded colors.
6. Configure menu options once via `ConfigureMenu`, chained off `.AddTagHelpers()`.
7. For one-off tweaks, override via the `class` attribute rather than editing the shipped bundles.
