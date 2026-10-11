# Theming StellarAdmin

**A theme is knob values.** The package ships one stylesheet, `stellar-admin.css`, which carries every component and the default theme. Any other look is a short CSS file of knob values — `--sa-accent`, `--sa-radius`, `--sa-density`, … — that the app owns and links after the stylesheet. Every colour, size and shadow is derived from the knobs, so a theme changes all components at once, in light and dark mode, without touching component CSS. Dark mode is a CSS class.

## The default theme

With no theme file, the app gets the default theme: a blue accent, near-neutral greys, 6px corners and Inter (falling back to system fonts). That is a complete, supported look; don't add a theme file unless the user wants something different.

```razor
<link rel="stylesheet" href="/_content/StellarAdmin.TagHelpers/stellar-admin.css" asp-append-version="true"/>
```

## A theme file

A theme file sets knobs in the `sa.theme` cascade layer, on `:root`:

```css
@layer sa.theme {
  :root {
    --sa-accent: oklch(0.5 0.14 245);
    --sa-radius: 4px;
    --sa-density: 0.9;
  }
}
```

Save it in the app (for example `wwwroot/css/theme.css`) and link it **after** `stellar-admin.css`:

```razor
<link rel="stylesheet" href="/_content/StellarAdmin.TagHelpers/stellar-admin.css" asp-append-version="true"/>
<link rel="stylesheet" href="~/css/theme.css" asp-append-version="true"/>
```

Set only the knobs that differ from the default; leave the rest out. Dark mode is derived from the same knobs, so a theme normally sets nothing for `.dark` (the optional `-dark` knobs exist for when it should differ). The installed package lists every knob, with its meaning and range, in `stellar-admin.knobs.json` (served at `/_content/StellarAdmin.TagHelpers/stellar-admin.knobs.json`).

To switch themes, switch the theme file's `<link>`. To go back to the default, remove it.

### Create or change a theme

Writing a theme — from a description ("warm, editorial, dense, green accent"), brand colours or a reference look — is the separate **stellar-admin-theme** skill, with the knob reference, worked examples and the theme builder link. If it isn't installed, suggest `npx skills add stellar-admin/stellar-admin --skill stellar-admin-theme`, or point the user to the [theme builder](https://www.stellaradmin.com/theme-builder): they pick a starting point, adjust the knobs on live components, and download the theme file.

## Fonts

The library never downloads fonts. Each font knob is a font stack whose first family is preferred and whose fallbacks are native, so a theme works without loading anything — it just renders in the fallback.

To get the intended typography, load the family from the app's layout, before the stylesheets, using Google Fonts or self-hosted files:

```html
<link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Lexend:wght@400..700&amp;display=swap"/>
```

The default theme prefers Inter; load Inter too if the app wants it rather than the system font. A theme file downloaded from the theme builder may instead start with an `@import url("https://fonts.googleapis.com/…")` line. That works only as the file's first statement in a file that is linked; inside a file that is itself `@import`ed, browsers drop it silently.

## Scoped knobs

Knobs re-derive everything inside the element they are set on, so part of a page can differ from the rest:

- `data-density="compact"` (0.8) or `data-density="comfortable"` (1.2) on any element changes the density of that section.
- `data-sa-scope` marks an element that sets its own knobs, such as a branded panel:

```razor
<section data-sa-scope style="--sa-accent: oklch(0.6 0.15 150); --sa-radius: 9999px;">
    <sa-button>Book now</sa-button>
</section>
```

- The `dark` class on any element switches that subtree to dark mode.

## Dark mode

Dark mode is a **class**: `stellar-admin.css` already carries both modes, so there is no extra CSS and no configuration method. Put the `dark` class on a containing element, usually `<html>`; every descendant then uses the dark values:

```razor
<html lang="en" class="dark">
```

How the class gets there is up to the app — server-rendered from a saved preference, or client-side. A minimal client script that honors a saved choice and falls back to the OS setting, placed in `<head>` **before** the stylesheets:

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

## Use the tokens in your own CSS

The knobs feed **foundation tokens** that the components read, and the app's own CSS can read them too, so custom markup follows the theme and dark mode:

```css
.trip-summary {
  padding: var(--sa-space-container);
  border: var(--sa-border-width) solid var(--sa-color-line);
  border-radius: var(--sa-radius-container);
  background: var(--sa-color-surface);
  color: var(--sa-color-ink);
}

.trip-summary small {
  color: var(--sa-color-ink-muted);
}
```

| Use | Tokens |
|-----|--------|
| Page and cards | `--sa-color-canvas` (page), `--sa-color-surface` (cards, popovers), `--sa-color-surface-subtle` (footers, table heads), `--sa-color-surface-sunken` (tracks, chips) |
| Text | `--sa-color-ink`, `--sa-color-ink-muted`, `--sa-color-ink-placeholder` |
| Lines | `--sa-color-line` (dividers, container edges), `--sa-color-line-control` (control borders) |
| Accent | `--sa-color-accent-fill` (solid fills) with `--sa-color-on-accent` (text on them), `--sa-color-accent-text` (accent as text) |
| Danger and status | `--sa-color-danger-fill`, `--sa-color-on-danger`, `--sa-color-danger-text`, `--sa-color-success`, `--sa-color-warning`, `--sa-color-info` |
| Washes | `--sa-color-hover`, `--sa-color-selected` |
| Shape | `--sa-radius-control`, `--sa-radius-container`, `--sa-radius-tag`, `--sa-border-width` |
| Space | `--sa-unit` (0.25rem × density), `--sa-space-container`, `--sa-control-h` |
| Type | `--sa-font-sans`, `--sa-font-heading`, `--sa-font-mono`, `--sa-text-xs`, `--sa-text-sm`, `--sa-text-base` |
| Shadows | `--sa-shadow-container`, `--sa-shadow-overlay` |

Read the tokens; don't set them. A theme sets knobs. A token name not listed here, and anything starting `--_`, is internal and may change.

### Tailwind utilities in your own markup

`stellar-admin.css` contains no utility classes: `class="bg-primary"` or `class="mt-4"` on your own `<div>` does nothing unless the app runs its own Tailwind v4 build. If it does, import the package's Tailwind adapter after Tailwind, with StellarAdmin's layer order declared first:

```css
@layer theme, base, sa.reset, sa.tokens, sa.components, components, sa.theme, sa.overrides, utilities;
@import "tailwindcss";
@import "<path>/stellar-admin.tailwind.css";
```

Copy `stellar-admin.tailwind.css` from the package's static assets (`_content/StellarAdmin.TagHelpers/stellar-admin.tailwind.css`) into the project, or reference it where the build can read it. It maps Tailwind's theme onto the tokens: spacing follows density, and the fonts, the small text sizes, the radius steps and the colours by their shadcn names (`bg-primary`, `text-muted-foreground`, `bg-card`, `border-border`, …) follow the theme and dark mode. The layer order keeps Tailwind's preflight below the components and your utilities above them. Keep linking `stellar-admin.css`; the adapter adds no rules.

```razor
<!-- good: adapts to the theme and dark mode -->
<div class="rounded-lg border bg-card text-card-foreground p-4">...</div>

<!-- avoid: hard-coded palette colours don't follow the theme -->
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

## Upgrading from theme bundles

Earlier packages shipped one bundle per theme (`stellar-admin.observatory.css`, `stellar-admin.shadcn.nova.css`, …), Dashboard's `ConfigureTheme`, and shadcn-named variables (`--primary`, `--radius`) for customization. If the installed package still has those bundles, follow that version's documentation instead of this guide. When upgrading, link `stellar-admin.css` instead, move any customization to knobs in a theme file, and replace overrides of `--primary`, `--background` and the other shadcn variables, which the components no longer read, with the matching knobs (`--sa-accent`, `--sa-neutral-hue`, …). The former themes are not reproduced; pick the closest look in the theme builder.

## Rules

1. `stellar-admin.css` alone is the default theme. Any other theme is a knob file the app owns, linked after it; switch themes by switching that `<link>`.
2. Set knobs, in `@layer sa.theme` on `:root`. Don't set foundation tokens, and don't write component rules to change a look a knob covers.
3. Set only the knobs that differ from the default, and let dark mode derive from them.
4. Load web fonts from the app layout; the stylesheet never fetches them.
5. Enable dark mode with the `dark` class on an ancestor, with `color-scheme` alongside it; don't write dark CSS of your own.
6. In your own CSS, read the `--sa-color-*`, `--sa-radius-*` and `--sa-space-*` tokens instead of hard-coded colours and sizes. Tailwind utilities need the app's own Tailwind build plus the adapter.
7. Configure menu options once via `ConfigureMenu`, chained off `.AddTagHelpers()`.
