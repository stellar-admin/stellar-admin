---
name: stellar-admin-theming
description: >-
  Configures the look of a StellarAdmin.UI app — picking a theme stylesheet, customizing theme
  values, enabling dark mode, tuning the menu color / appearance / accent, and styling with
  StellarAdmin.UI's semantic color tokens. Use when the user wants to change the StellarAdmin.UI
  theme, add or toggle dark mode, adjust menu/dropdown appearance, or asks which colors/classes
  to use with StellarAdmin.UI.
metadata:
  author: StellarAdmin.UI
---

# Theming StellarAdmin.UI

**A theme is a stylesheet.** The package ships one self-contained CSS bundle per theme; the
layout links exactly one, and switching themes means switching that `<link>`. Nothing about the
theme is configured in C#. Dark mode is a CSS class, and day-to-day styling means using semantic
tokens instead of hard-coded colors.

## Pick a theme (the layout `<link>`)

Available themes: `vega`, `nova`, `luma`, `lyra`, `maia`, `mira`, `rhea`, `sera`. Each bundle is
a full palette:

```razor
<link rel="stylesheet" href="/_content/StellarAdmin.UI/stellar-admin-ui.nova.css" asp-append-version="true" />
```

To change the theme, change `nova` to another theme name — that's the whole operation.

## Customize theme values

Every color and radius is a CSS custom property; the compiled rules all reference `var(--…)`.
Override by redeclaring properties in the app's own stylesheet — no build tooling required:

```css
:root {
  --primary: oklch(0.55 0.2 260);
  --radius: 0.5rem;
}

.dark {
  --primary: oklch(0.7 0.18 260);
}
```

## Menu appearance (`Program.cs`)

Floating menu surfaces (Dropdown Menu, and future menu families) have app-wide options, chained
off `.AddTagHelpers()`:

```csharp
using StellarAdmin;
using StellarAdmin.UI;
using StellarAdmin.UI.TagHelpers;

builder.Services.AddStellarAdmin()
    .AddTagHelpers()
    .ConfigureMenu(menu =>
    {
        menu.Color = MenuColor.Inverted;              // Default | Inverted
        menu.Appearance = MenuAppearance.Translucent; // Solid | Translucent (frosted glass)
        menu.Accent = MenuAccent.Bold;                // Subtle | Bold
    });
```

Defaults are `Color=Default`, `Appearance=Solid`, `Accent=Subtle` — only call `ConfigureMenu`
to override.

## Dark mode

Dark mode is a **class-based Tailwind variant** — every theme bundle already carries both the
light (`:root`) and dark (`.dark`) token values, so there's no extra CSS or config method.

Enable it by putting the **`dark` class on an ancestor** (usually `<html>` or `<body>`); every
descendant then reads the dark values:

```razor
<html class="dark">
```

To make it user-toggleable, add the `dark` class yourself — server-side from a preference, or a
small client script (e.g. reading `prefers-color-scheme` / a saved setting). StellarAdmin.UI ships
the token values; the toggle logic is the app's.

## Style with semantic tokens, not hard-coded colors

StellarAdmin.UI components use CSS-variable-backed **semantic tokens**, exposed as Tailwind
utilities. Prefer these so your markup stays consistent and adapts to the theme + dark mode
automatically (using them in the app's *own* markup requires the app's Tailwind build to import
StellarAdmin's `theme-tokens.css` — see the setup reference):

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
<div class="rounded-lg border bg-card text-card-foreground p-4">…</div>

<!-- avoid: hard-coded palette colors don't follow the theme -->
<div class="rounded-lg border border-gray-200 bg-white text-gray-900 p-4">…</div>
```

## Rules

1. Pick the theme by linking one `stellar-admin-ui.<theme>.css` in the layout; switch themes by
   switching the `<link>`. No C# theme configuration exists.
2. Customize theme values by redeclaring the CSS custom properties (`--primary`, `--radius`, …)
   in the app's own stylesheet.
3. Configure menu options once via `ConfigureMenu`, chained off `.AddTagHelpers()` in `Program.cs`.
4. Enable dark mode with the `dark` class on an ancestor; don't add your own dark CSS — the
   tokens are already themed for both modes.
5. Prefer semantic tokens (`bg-primary`, `text-muted-foreground`, `bg-card`, `border`,
   `text-destructive`) over hard-coded colors.
6. For one-off tweaks, override via the `class` attribute (author utilities out-rank component
   rules, so they win) rather than editing the shipped bundles.
