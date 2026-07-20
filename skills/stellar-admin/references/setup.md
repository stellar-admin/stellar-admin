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

## Optional: use StellarAdmin's design tokens in your own markup

Steps 1–4 give you the prebuilt stylesheet, which styles the `<sa-*>` components. It does
**not** let your own markup use the design system — write `class="bg-primary"` in your own
Razor and nothing happens, because those tokens only exist inside the prebuilt bundle.

If your app already runs a Tailwind build, you can switch to **single-build mode**, where your
Tailwind build generates everything from one set of tokens into one stylesheet. Then
`bg-primary`, `text-muted-foreground`, `rounded-lg`, `dark:*` and the component variants all
work in your markup too.

Three changes:

**a. Opt in, in your `.csproj`:**

```xml
<PropertyGroup>
  <StellarAdminUIExportTailwindSources>true</StellarAdminUIExportTailwindSources>
</PropertyGroup>
```

This copies StellarAdmin's Tailwind sources into `obj/stellaradmin-ui/tailwind/` on build.

**b. Import them from your Tailwind entry stylesheet**, after `@import "tailwindcss"` (adjust
the relative path to reach your project's `obj/`):

```css
@import "tailwindcss";
@import "../../obj/stellaradmin-ui/tailwind/index.css";

@source "../../Pages/";
```

**c. Remove the CSS `<link>` from your layout.** Keep the `<script>`:

```razor
<link rel="stylesheet" href="/css/site.css" asp-append-version="true" />
<script defer src="/_content/StellarAdmin.UI/stellar-admin-ui.js" asp-append-version="true"></script>
```

Leaving both in place is the one thing that will actually bite you — you'd get every rule twice,
and the two stylesheets would compete.

Notes:

- **Run `dotnet build` before your first standalone Tailwind run.** `obj/stellaradmin-ui/` doesn't
  exist until the build creates it. If you drive Tailwind from an MSBuild target with
  `BeforeTargets="Build"` (the usual setup), ordering takes care of itself — the export runs
  earlier in the build, whatever you've named your target.
- **If your Tailwind target runs earlier than `CoreBuild`**, depend on the export explicitly:

  ```xml
  <Target Name="MyStyles" BeforeTargets="PrepareForBuild"
          DependsOnTargets="StellarAdminUICopyTailwind">
  ```

  Otherwise your Tailwind run fails with an unresolvable `@import` of `index.css`.
- **Requires Tailwind v4.** Keep reasonably close to the version StellarAdmin.UI ships with; on an
  older v4 release, any utility your version doesn't know is skipped silently rather than erroring.
- Two-bundle mode (steps 1–4) remains fully supported — single-build mode is opt-in, and you can
  switch back by reverting these three changes.
- Override any design token by redefining it in your own CSS, e.g. `--primary` or `--radius`.

## Quick smoke test

Drop this on a page; if it renders as a styled alert, setup is correct:

```razor
<sa-alert>
    <sa-alert-title>Success! You have configured StellarAdmin.UI correctly.</sa-alert-title>
</sa-alert>
```

## Online documentation

Full component docs and live examples: <https://www.duneui.com/docs>
