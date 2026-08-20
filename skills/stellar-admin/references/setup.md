# Setting up StellarAdmin.UI

StellarAdmin.UI ships as the `StellarAdmin.UI` NuGet package. A project needs four things wired up
before any `<sa-*>` tag helper will render correctly. If components render as
plain, unstyled HTML, one of these steps is missing (usually 3 or 4).

## 1. Install the package

```bash
dotnet add package StellarAdmin.UI
```

## 2. Register services (`Program.cs`)

```csharp
using StellarAdmin;
using StellarAdmin.UI;

builder.Services.AddStellarAdmin().AddTagHelpers();
```

`AddStellarAdmin()` (from `StellarAdmin.Core`) creates the shared builder; `.AddTagHelpers()` registers
the tag helper services — the icon manager, with the **Lucide** icon pack as the default. Without
`.AddTagHelpers()`, tag helpers that render icons fail to resolve their services. Theme selection is
not part of registration: the theme is whichever stylesheet the layout links (step 4).

## 3. Register the tag helpers (`_ViewImports.cshtml`)

```razor
@using StellarAdmin.UI.TagHelpers
@addTagHelper *, StellarAdmin.UI
```

Without `@addTagHelper *, StellarAdmin.UI`, Razor treats `<sa-*>` as unknown HTML and
emits it verbatim (no styling, no behavior).

## 4. Reference the CSS and JS assets (your layout, e.g. `_Layout.cshtml`)

The assets are served from the package as static web assets under `_content/StellarAdmin.UI/`.
The CSS ships as one self-contained bundle per theme (`vega`, `nova`, `luma`, `lyra`, `maia`,
`mira`, `rhea`, `sera`); link exactly one — switching themes is switching the `<link>`:

```razor
<link rel="stylesheet" href="/_content/StellarAdmin.UI/stellar-admin-ui.nova.css" asp-append-version="true" />
<script defer src="/_content/StellarAdmin.UI/stellar-admin-ui.js" asp-append-version="true"></script>
```

- The **CSS** carries all component styling and the theme. Missing it → unstyled
  components.
- The **JS** bundle carries the `<sel-*>` web components (and the Invoker Commands
  polyfill). Missing it → static components render, but overlays (Sheet, Dialog,
  DropdownMenu) and the sidebar toggle won't open/close.

## 5. Remove conflicting CSS frameworks (strongly recommended)

Using StellarAdmin.UI alongside another CSS framework (Bootstrap being the common one)
will almost always break rendering — their resets and utilities fight StellarAdmin.UI's.
Remove third-party stylesheets and rely on the StellarAdmin.UI bundle alone.

## Optional: customize the theme

Every color and radius in the bundle is a CSS custom property. Redeclare the properties in the
app's own CSS — no build tooling or imports required:

```css
:root {
  --primary: oklch(0.55 0.2 260);
  --radius: 0.5rem;
}
```

Values live on `:root` (with dark-mode overrides under `.dark`); the compiled rules all reference
`var(--…)`, so redeclared values take effect everywhere.

## Optional: use StellarAdmin's design tokens in your own markup

Steps 1–4 give you the prebuilt stylesheet, which styles the `<sa-*>` components. It does
**not** let your own markup use the design system — write `class="bg-primary"` in your own
Razor and nothing happens, because those tokens only exist inside the prebuilt bundle.

If the app runs its own Tailwind v4 build, copy `theme-tokens.css` from the StellarAdmin
repository (`src/StellarAdmin.UI/Client/css/theme-tokens.css`) into the project and import it
from the Tailwind entry stylesheet:

```css
@import "tailwindcss";
@import "./theme-tokens.css";

@source "../../Pages/";
```

Then `bg-primary`, `text-muted-foreground`, `rounded-lg`, `dark:*` and the rest work in the
app's own markup. The file carries only the token *vocabulary* — the generated utilities compile
to `var(--…)` references whose values come from the linked StellarAdmin.UI bundle at runtime, so
the `<link>` from step 4 stays in place, and any `:root` customizations apply to both stylesheets.

## Quick smoke test

Drop this on a page; if it renders as a styled alert, setup is correct:

```razor
<sa-alert>
    <sa-alert-title>Success! You have configured StellarAdmin.UI correctly.</sa-alert-title>
</sa-alert>
```

## Online documentation

Full component docs and live examples: <https://www.duneui.com/docs>
