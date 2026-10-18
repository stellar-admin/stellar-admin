# StellarAdmin

<div align="center">
    <img src="assets/logo/stellar-admin-logo.svg">
</div>

StellarAdmin.UI is a collection of beautifully designed components based on [shadcn/ui](https://ui.shadcn.com/) which you can use to create CRUD screens in [ASP.NET Core](https://dotnet.microsoft.com/en-us/apps/aspnet) MVC and Razor Pages applications.

> **Note:** StellarAdmin is in 0.x, so any release may contain breaking changes. Pin the version you use and read the release notes before you upgrade.

<div align="center">

[![MIT License](https://img.shields.io/badge/license-MIT-blue.svg)](https://opensource.org/licenses/MIT) [![NuGet](https://img.shields.io/nuget/v/StellarAdmin.UI)](https://www.nuget.org/packages/StellarAdmin.UI/)

</div>

## Quick start

### 1. Install package

Install the `StellarAdmin.UI` NuGet package:

```bash
dotnet add package StellarAdmin.UI
```

### 2. Register services

Update your `Program.cs` (or `Startup.cs`) to register the StellarAdmin services.

```cs
using StellarAdmin.UI;

builder.Services.AddStellarAdminUI();
```

### 3. Update imports

Update your `_ViewImports.cshtml` to register the StellarAdmin Tag Helpers and import the `StellarAdmin.UI` namespace.

```razor
@using StellarAdmin.UI
@addTagHelper *, StellarAdmin.UI
```

### 4. Link the stylesheet and JavaScript file

Add the StellarAdmin stylesheet and the [StellarAdmin JavaScript file](https://www.stellaradmin.com/docs/ui/javascript) to your Razor page or layout.

```razor
<!DOCTYPE html>
<html lang="en">
<head>
    ...
    <link rel="stylesheet" href="/_content/StellarAdmin.UI/stellar-admin.css" asp-append-version="true"/>
    <script defer src="/_content/StellarAdmin.UI/stellar-admin.js" asp-append-version="true"></script>
</head>
<body>
    ...
</body>
</html>
```

> [!TIP]
> To change the default theme, create a theme file in the [theme builder](https://www.stellaradmin.com/theme-builder) and link it after the stylesheet. The [Theming page](https://www.stellaradmin.com/docs/ui/theming) provides more information on theming. 

### 5. Start using the Tag Helpers

Start using the StellarAdmin Tag Helpers inside your Razor Pages or MVC Views. For example, the code snippet below adds an alert to your page.

```razor
<sa-alert>
    <sa-alert-title>Success! You have configured StellarAdmin correctly.</sa-alert-title>
</sa-alert>
```

## Documentation

Documentation and code examples for all the Tag Helpers [can be found online](https://www.stellaradmin.com/docs/ui).

## Using StellarAdmin with AI agents

StellarAdmin ships agent skills for each product:

- The [StellarAdmin UI skill](skills/stellar-admin-ui/SKILL.md) teaches agents how to build MVC and Razor Pages UIs in `.cshtml` files, with component references and workflows for forms, layout, and theming. Its supporting documentation is bundled under `skills/stellar-admin-ui/references/`.
- The [StellarAdmin theme skill](skills/stellar-admin-theme/SKILL.md) writes theme files from a description or brand colours.
- The [admin panel setup skill](skills/stellar-admin-setup/SKILL.md) installs the admin panel, and the [admin panel skill](skills/stellar-admin/SKILL.md) builds resources, forms and the sidebar with it.

From your application's project directory, install them with [Vercel's skills CLI](https://github.com/vercel-labs/skills#readme) and choose the ones for the product you use:

```bash
npx skills add stellar-admin/stellar-admin
```

The skills are maintained alongside StellarAdmin; their references may include changes newer than your installed NuGet package. See [installation and compatibility](skills/README.md#install-the-skills) for prerequisites, agent selection, installing a single skill, and local-checkout installation.

`StellarAdmin.UI` also supplies the icon services in `StellarAdmin.UI.Icons`; configure icons, menus, and forms through `AddStellarAdminUI(ui => ...)`.

## Resource and admin packages

This repository also contains `StellarAdmin` (the admin panel: admin shell and resource screens) and `StellarAdmin.EntityFrameworkCore` (EF Core resources). Both are MIT licensed. Register the admin panel with `AddStellarAdmin(admin => ...)` and map it with `MapStellarAdmin()`; it registers StellarAdmin.UI itself, and `admin.ConfigureUI(...)` configures it. The AdminPlayground demonstrates Identity user and role management through ordinary resources. There is no paid tier. The packages are available from source; their NuGet publication is a separate release step.

The full solution includes a unified `docs/DocsSamples` app, the website demo exporter, AdminPlayground, and the admin panel and EF Core resource integration tests.

## Contributing

Start with [contributor guidance](AGENTS.md) and [development and verification](docs/development.md). This checkout includes the development skills, maintained designs, and plans; no workspace checkout is required. The public website remains a separate repository.
