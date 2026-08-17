# StellarAdmin Tag Helpers

<div align="center">
    <img src="assets/logo/stellar-admin-logo.svg">
</div>

StellarAdmin Tag Helpers is a collection of beautifully designed components based on [shadcn/ui](https://ui.shadcn.com/) which you can use to create CRUD screens in [ASP.NET Core](https://dotnet.microsoft.com/en-us/apps/aspnet) MVC and Razor Pages applications.

<div align="center">

[![MIT License](https://img.shields.io/badge/license-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![NuGet](https://img.shields.io/nuget/v/StellarAdmin.TagHelpers)](https://www.nuget.org/packages/StellarAdmin.TagHelpers/)

</div>

## Quick start

### 1. Install package

Install the `StellarAdmin.TagHelpers` NuGet package:

```bash
dotnet add package StellarAdmin.TagHelpers
```

### 2. Register services

Update your `Program.cs` (or `Startup.cs`) to register the StellarAdmin services.

```cs
using StellarAdmin;
using StellarAdmin.TagHelpers;

builder.Services.AddStellarAdmin().AddUI();
```

### 3. Update imports

Update your `_ViewImports.cshtml` to register the StellarAdmin Tag Helpers and import the `StellarAdmin.TagHelpers` namespace.

```razor
@using StellarAdmin.TagHelpers
@addTagHelper *, StellarAdmin.TagHelpers
```

### 4. Add stylesheet and JavaScript file

StellarAdmin ships one stylesheet per theme. Add exactly one theme stylesheet (`/_content/StellarAdmin.TagHelpers/stellar-admin.<theme>.css`) and the JavaScript file (`/_content/StellarAdmin.TagHelpers/stellar-admin.js`) to your Razor layout. A theme is just a stylesheet — pick one of `vega`, `nova`, `luma`, `lyra`, `maia`, `mira`, `rhea` or `sera`, and switch themes by switching the `<link>`.

```razor
<!DOCTYPE html>
<html lang="en">
<head>
    ...
    <link rel="stylesheet" href="/_content/StellarAdmin.TagHelpers/stellar-admin.nova.css" asp-append-version="true"/>
    <script defer src="/_content/StellarAdmin.TagHelpers/stellar-admin.js" asp-append-version="true"></script>
</head>
<body>
    ...
</body>
</html>
```

### 5. (Optional) Remove 3rd party stylesheets

Using StellarAdmin along with 3rd party CSS libraries like Bootstrap will almost certainly result in incorrect rendering of the StellarAdmin components, since these libraries apply their own styling which may override the styling applied by StellarAdmin.

As such, we **strongly recommend** that you remove 3rd party stylesheets and only depend on the CSS styling applied by StellarAdmin.

### 6. Start using the Tag Helpers

Start using the StellarAdmin Tag Helpers inside your Razor Pages or MVC Views. For example, the code snippet below adds an alert to your page.

```razor
<sa-alert>
    <sa-alert-title>Success! You have configured StellarAdmin correctly.</sa-alert-title>
</sa-alert>
```

## Customizing the theme

Every color and radius in the stylesheet is a CSS custom property. To customize a theme, redeclare
the properties in your own CSS — no build tooling required:

```css
:root {
  --primary: oklch(0.55 0.2 260);
  --radius: 0.5rem;
}
```

## Using the design tokens in your own markup

The stylesheet above styles the `<sa-*>` components. If you also want to write
`class="bg-primary"` or `dark:...` in your *own* Razor and have it match, and your app runs a
Tailwind v4 build, copy
[`theme-tokens.css`](src/StellarAdmin.TagHelpers/Client/css/theme-tokens.css) into your project and import
it from your Tailwind entry stylesheet:

```css
@import "tailwindcss";
@import "./theme-tokens.css";
```

It contains only the token *vocabulary* — the utilities it enables compile to `var(--…)`
references whose values come from the StellarAdmin stylesheet at runtime. Keep the `<link>`
from step 4 in place.

## Documentation

Documentation and code examples for all the Tag Helpers [can be found online](https://www.stellaradmin.com/docs/tag-helpers).

## Using StellarAdmin with AI agents (Claude Code)

StellarAdmin ships a set of [Claude Code](https://claude.com/claude-code) skills that teach an AI agent how to build UIs with StellarAdmin — the component catalog, the library's conventions, and task workflows for forms, layout, and theming.

Install them from this repository, which doubles as a plugin marketplace:

```bash
/plugin marketplace add stellar-admin/stellar-admin
/plugin install stellar-admin@stellar-admin
```

This adds the `stellar-admin` skill (auto-activates when you edit `.cshtml` / `.razor` files) along with `stellar-admin-forms`, `stellar-admin-layout`, and `stellar-admin-theming`.
