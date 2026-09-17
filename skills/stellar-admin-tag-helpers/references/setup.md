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

No separate Core package installation is needed. Theme selection is **not** part of registration: the theme is whichever stylesheet the layout links (step 4).

## 3. Register the tag helpers (`_ViewImports.cshtml`)

```razor
@using StellarAdmin.TagHelpers
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@addTagHelper *, StellarAdmin.TagHelpers
```

Without `@addTagHelper *, StellarAdmin.TagHelpers`, Razor treats `<sa-*>` as unknown HTML and emits it verbatim — no styling, no behavior. The `@using` is what lets you write enum values like `ButtonVariant.Outline` unqualified.

Keep the framework's own line (`Microsoft.AspNetCore.Mvc.TagHelpers`) — the default project templates already include it. StellarAdmin builds on it: `asp-for`, `asp-page`, `asp-route-*`, `asp-items` and `asp-append-version` are resolved by the framework's tag helpers, so without that line they render as literal attributes and model binding silently does nothing.

## 4. Reference the CSS and JS assets (your layout, e.g. `_Layout.cshtml`)

The assets are served from the package as static web assets under `_content/StellarAdmin.TagHelpers/`. The CSS ships as one self-contained bundle per theme (Observatory is the documentation default; upstream-derived themes use the `shadcn.` prefix); link exactly one — switching themes is switching the `<link>`:

```razor
<link rel="stylesheet" href="/_content/StellarAdmin.TagHelpers/stellar-admin.observatory.css" asp-append-version="true"/>
<script defer src="/_content/StellarAdmin.TagHelpers/stellar-admin.js" asp-append-version="true"></script>
```

- The **CSS** carries all component styling and the theme. Missing it → unstyled components.
- The **JS** bundle carries the `<sel-*>` web components, the `interestfor` polyfill, and the `window.stellarAdmin` helpers. Missing it → static components still render, but the dropdown menu's keyboard handling, the slider, the OTP input, the sidebar toggle and the table selection stop working. See `javascript.md`.

Both are **static web assets**, so the app must serve them — the default templates already call `app.MapStaticAssets()` (or `app.UseStaticFiles()` on older templates). If those two URLs 404, that's the missing piece rather than anything StellarAdmin-specific.

The package targets **net10.0**, so the app must be on .NET 10 or later.

## Optional: convert the layout and remove Bootstrap

Perform this only when the user selected layout conversion or explicitly requested it. Replace the default layout's Bootstrap-dependent markup with StellarAdmin components and appropriate application CSS. Check remaining pages and scripts for Bootstrap dependencies before removing its stylesheet, JavaScript references, or installed assets. If other pages still depend on Bootstrap, explain what remains and ask before expanding the conversion beyond the agreed layout scope. Do not remove unrelated stylesheets, scripts, or validation dependencies as cleanup.

For basic installation, retain Bootstrap and explain that CSS resets and overlapping styles can affect rendering when the frameworks coexist. Check both a StellarAdmin component and the existing layout; report any observed conflicts and offer a separate layout conversion rather than silently removing the existing framework. Keeping the markup and assets does not guarantee an unchanged appearance after adding a theme stylesheet.

## Optional: customize the theme

Every color and radius in the bundle is a CSS custom property. Redeclare the properties in the app's own CSS, *after* the theme stylesheet — no build tooling or imports required:

```css
:root {
  --primary: oklch(0.55 0.2 260);
  --radius: 0.5rem;
}
```

Values live on `:root`, with dark-mode overrides under `.dark`; the compiled rules all reference `var(--…)`, so redeclared values take effect everywhere. The full list of variables is in the [theming](theming.md) guide.

## Optional: use StellarAdmin's design tokens in your own markup

Steps 1–4 give you the prebuilt stylesheet, which styles the `<sa-*>` components. It does **not** let your own markup use the design system — write `class="bg-primary"` in your own Razor and nothing happens, because that utility only exists inside the prebuilt bundle.

If the app runs its own Tailwind v4 build, copy [`theme-tokens.css`](https://github.com/stellar-admin/stellar-admin/blob/master/src/StellarAdmin.TagHelpers/Client/css/theme-tokens.css) (`src/StellarAdmin.TagHelpers/Client/css/theme-tokens.css`) into the project and import it from the Tailwind entry stylesheet:

```css
@import "tailwindcss";
@import "./theme-tokens.css";

@source "../../Pages/";
```

Then `bg-primary`, `text-muted-foreground`, `rounded-lg`, `dark:*` and the rest work in the app's own markup. The file carries only the token *vocabulary* — the generated utilities compile to `var(--…)` references whose values come from the linked theme bundle at runtime, so the `<link>` from step 4 stays in place, and any `:root` customizations apply to both stylesheets.

## Quick smoke test

Drop this on a page; if it renders as a styled alert, setup is correct:

```razor
<sa-alert>
    <sa-alert-title>Success! You have configured StellarAdmin correctly.</sa-alert-title>
</sa-alert>
```

## Online documentation

Full component docs and live examples: <https://www.stellaradmin.com/docs/tag-helpers>
