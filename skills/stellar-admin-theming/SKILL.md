---
name: stellar-admin-theming
description: >-
  Configures the look of a StellarAdmin.UI app — selecting a theme pack, enabling dark mode, tuning the
  menu color / appearance / accent, and styling with StellarAdmin.UI's semantic color tokens. Use when
  the user wants to change the StellarAdmin.UI theme, add or toggle dark mode, adjust menu/dropdown
  appearance, or asks which colors/classes to use with StellarAdmin.UI.
metadata:
  author: StellarAdmin.UI
---

# Theming StellarAdmin.UI

Most theming is configured **once at startup** off `AddStellarAdmin()`; dark mode is a CSS class; and
day-to-day styling means using semantic tokens instead of hard-coded colors.

## Configure at startup (`Program.cs`)

Chain configuration onto `AddStellarAdmin()`. Vega (theme) and Lucide (icons) are applied by default —
only call these to override.

```csharp
using StellarAdmin.UI;
using StellarAdmin.UI.TagHelpers;

builder.Services.AddStellarAdmin()
    .UseTheme<VegaThemePack>()          // pick a theme pack (default: Vega)
    .ConfigureMenu(menu =>
    {
        menu.Color = MenuColor.Inverted;            // Default | Inverted
        menu.Appearance = MenuAppearance.Translucent; // Solid | Translucent (frosted glass)
        menu.Accent = MenuAccent.Bold;              // Subtle | Bold
    });
```

- **Theme packs** available: `VegaThemePack`, `NovaThemePack`, `LumaThemePack`, `LyraThemePack`,
  `MaiaThemePack`, `MiraThemePack`, `RheaThemePack`, `SeraThemePack`. Each is a full palette; pick
  one with `UseTheme<T>()`.
- **Menu options** apply to floating menu surfaces (Dropdown Menu, and future menu families).
  Defaults are `Color=Default`, `Appearance=Solid`, `Accent=Subtle`.
- Enum values are fully-qualified (`MenuColor.Inverted`, etc.).

## Dark mode

Dark mode is a **class-based Tailwind variant** — the shipped `stellar-admin-ui.css` already carries both
the light (`:root`) and dark (`.dark`) token values, so there's no extra CSS or config method.

Enable it by putting the **`dark` class on an ancestor** (usually `<html>` or `<body>`); every
descendant then reads the dark values:

```razor
<html class="dark">
```

To make it user-toggleable, add the `dark` class yourself — server-side from a preference, or a
small client script (e.g. reading `prefers-color-scheme` / a saved setting). StellarAdmin.UI ships the
token values; the toggle logic is the app's.

## Style with semantic tokens, not hard-coded colors

StellarAdmin.UI components use CSS-variable-backed **semantic tokens**, exposed as Tailwind utilities.
Prefer these so your markup stays consistent and adapts to the theme + dark mode automatically:

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

1. Configure the theme pack and menu options once, chained off `AddStellarAdmin()` in `Program.cs`.
2. Vega + Lucide are the defaults — only call `UseTheme<>` / `AddIconPack<>` to change them.
3. Enable dark mode with the `dark` class on an ancestor; don't add your own dark CSS — the
   tokens are already themed for both modes.
4. Prefer semantic tokens (`bg-primary`, `text-muted-foreground`, `bg-card`, `border`,
   `text-destructive`) over hard-coded colors.
5. For one-off tweaks, override via the `class` attribute (merged last, so it wins) rather than
   editing theme packs or CSS variables by hand.
