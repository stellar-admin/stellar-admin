# Setting up StellarAdmin Tag Helpers

StellarAdmin Tag Helpers ships as the **`StellarAdmin.TagHelpers`** NuGet package. A project needs four things wired up before any `<sa-*>` tag helper renders correctly. If components render as plain, unstyled HTML, one of these steps is missing (usually 3 or 4).

## Choose the installation scope

Before changing a project for installation, ask the user which scope they want unless their request already makes the choice explicit:

> Would you like a basic installation that keeps your current layout and Bootstrap, or should I also convert the default layout to StellarAdmin and remove Bootstrap where it is no longer needed?

Use a structured choice or picker when available; otherwise ask a concise text question with both options.

- **Basic installation:** complete steps 1–4. Add the necessary asset references to the existing layout, preserving its structure, navigation, page markup, existing stylesheets, and scripts. Do not remove Bootstrap or redesign the app shell.
- **Installation with layout conversion:** complete steps 1–4, then adapt the default layout using the [layout guide](layout.md) and perform the Bootstrap cleanup below. Preserve application routes, content, Razor sections, and behavior while converting the presentation.

Wait for the choice before making installation changes; an unanswered question does not authorize layout conversion. Honor a scope already specified by the user without asking again. This choice applies to installation, not every later component edit.

## 1. Install the package

```bash
dotnet add package StellarAdmin.TagHelpers
```

## 2. Register services (`Program.cs`)

```csharp
using StellarAdmin;
using StellarAdmin.TagHelpers;

builder.Services.AddStellarAdmin().AddTagHelpers();
```

`AddStellarAdmin()` (namespace `StellarAdmin`) comes from `StellarAdmin.Core`, which is installed transitively with `StellarAdmin.TagHelpers`. It creates the shared builder and registers `IconOptions`, with the **Lucide** icon pack as the default. `.AddTagHelpers()` (namespace `StellarAdmin.TagHelpers`) comes from `StellarAdmin.TagHelpers` and registers the tag helper options. Keep both calls in the registration chain.

No separate Core package installation is needed. Theme selection is **not** part of registration: the theme is whichever stylesheets the layout links (step 4).

## 3. Register the tag helpers (`_ViewImports.cshtml`)

```razor
@using StellarAdmin.TagHelpers
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@addTagHelper *, StellarAdmin.TagHelpers
```

Without `@addTagHelper *, StellarAdmin.TagHelpers`, Razor treats `<sa-*>` as unknown HTML and emits it verbatim — no styling, no behavior. The `@using` is what lets you write enum values like `ButtonVariant.Outline` unqualified.

Keep the framework's own line (`Microsoft.AspNetCore.Mvc.TagHelpers`) — the default project templates already include it. StellarAdmin builds on it: `asp-for`, `asp-page`, `asp-route-*`, `asp-items` and `asp-append-version` are resolved by the framework's tag helpers, so without that line they render as literal attributes and model binding silently does nothing.

## 4. Reference the CSS and JS assets (your layout, e.g. `_Layout.cshtml`)

The assets are served from the package as static web assets under `_content/StellarAdmin.TagHelpers/`. `stellar-admin.css` carries every component and the default theme:

```razor
<link rel="stylesheet" href="/_content/StellarAdmin.TagHelpers/stellar-admin.css" asp-append-version="true"/>
<script defer src="/_content/StellarAdmin.TagHelpers/stellar-admin.js" asp-append-version="true"></script>
```

- The **CSS** carries all component styling and the default theme. Missing it → unstyled components. Another theme is an optional file of CSS variables linked after it; see [theming](theming.md).
- The **JS** bundle carries the `<sel-*>` web components, the `interestfor` polyfill, and the `window.stellarAdmin` helpers. Missing it → static components still render, but the dropdown menu's keyboard handling, the slider, the OTP input, the sidebar toggle and the table selection stop working. See `javascript.md`.

Both are **static web assets**, so the app must serve them — the default templates already call `app.MapStaticAssets()` (or `app.UseStaticFiles()` on older templates). If those two URLs 404, that's the missing piece rather than anything StellarAdmin-specific.

The package targets **net10.0**, so the app must be on .NET 10 or later. The stylesheet needs Chrome and Edge 119, Safari 18 or Firefox 128 or later (it uses relative colour syntax, `color-mix()`, cascade layers, native nesting and `:has()` without fallbacks); in older browsers colours fail silently.

## Optional: convert the layout and remove Bootstrap

Perform this only when the user selected layout conversion or explicitly requested it. Replace the default layout's Bootstrap-dependent markup with StellarAdmin components and appropriate application CSS. Check remaining pages and scripts for Bootstrap dependencies before removing its stylesheet, JavaScript references, or installed assets. If other pages still depend on Bootstrap, explain what remains and ask before expanding the conversion beyond the agreed layout scope. Do not remove unrelated stylesheets, scripts, or validation dependencies as cleanup.

For basic installation, retain Bootstrap and explain that CSS resets and overlapping styles can affect rendering when the frameworks coexist. Check both a StellarAdmin component and the existing layout; report any observed conflicts and offer a separate layout conversion rather than silently removing the existing framework. Keeping the markup and assets does not guarantee an unchanged appearance after adding the StellarAdmin stylesheet.

## Optional: use another theme

The default theme needs nothing more. For another look, write a theme file of CSS variables (or download one from the [theme builder](https://www.stellaradmin.com/theme-builder)) and link it after `stellar-admin.css`:

```razor
<link rel="stylesheet" href="/_content/StellarAdmin.TagHelpers/stellar-admin.css" asp-append-version="true"/>
<link rel="stylesheet" href="~/css/theme.css" asp-append-version="true"/>
```

```css
@layer sa.theme {
  :root {
    --sa-accent: oklch(0.55 0.2 260);
    --sa-radius: 4px;
  }
}
```

The [theming](theming.md) guide covers theme files, fonts, dark mode and using the tokens in the app's own CSS. Writing a theme is the **stellar-admin-theme** skill's job.

## Optional: Tailwind utilities in your own markup

Steps 1–4 style the `<sa-*>` components. `stellar-admin.css` contains no utility classes, so `class="bg-primary"` or `class="mt-4"` in your own Razor does nothing unless the app runs its own Tailwind v4 build. If it does, declare StellarAdmin's layer order first and import the package's Tailwind adapter after Tailwind:

```css
@layer theme, base, sa.reset, sa.tokens, sa.components, components, sa.theme, sa.overrides, utilities;
@import "tailwindcss";
@import "<path>/stellar-admin.tailwind.css";
```

The adapter maps Tailwind's spacing, fonts, small text sizes, radius steps and the shadcn colour names (`bg-primary`, `text-muted-foreground`, …) onto StellarAdmin's tokens, so those utilities follow the theme, density and dark mode. Keep the `<link>` from step 4; the adapter adds no rules. Without a Tailwind build, style your own markup with the tokens in plain CSS (see [theming](theming.md#use-the-tokens-in-your-own-css)).

## Quick smoke test

Drop this on a page; if it renders as a styled alert, setup is correct:

```razor
<sa-alert>
    <sa-alert-title>Success! You have configured StellarAdmin correctly.</sa-alert-title>
</sa-alert>
```

## Online documentation

Full component docs and live examples: <https://www.stellaradmin.com/docs/tag-helpers>
