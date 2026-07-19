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
using StellarAdmin.UI;

builder.Services.AddStellarAdmin();
```

`AddStellarAdmin()` registers the theme manager, the CSS class merger, the icon manager,
and applies the defaults: the **Vega** theme pack and the **Lucide** icon pack.

## 3. Register the tag helpers (`_ViewImports.cshtml`)

```razor
@using StellarAdmin.UI.TagHelpers
@addTagHelper *, StellarAdmin.UI
```

Without `@addTagHelper *, StellarAdmin.UI`, Razor treats `<sa-*>` as unknown HTML and
emits it verbatim (no styling, no behavior).

## 4. Reference the CSS and JS assets (your layout, e.g. `_Layout.cshtml`)

The assets are served from the package as static web assets under `_content/StellarAdmin.UI/`:

```razor
<link rel="stylesheet" href="/_content/StellarAdmin.UI/stellar-admin-ui.css" asp-append-version="true" />
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
Remove third-party stylesheets and rely on `stellar-admin-ui.css` alone.

## Quick smoke test

Drop this on a page; if it renders as a styled alert, setup is correct:

```razor
<sa-alert>
    <sa-alert-title>Success! You have configured StellarAdmin.UI correctly.</sa-alert-title>
</sa-alert>
```

## Online documentation

Full component docs and live examples: <https://www.duneui.com/docs>
