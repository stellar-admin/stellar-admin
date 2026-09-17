# StellarAdmin Tag Helpers

<div align="center">
    <img src="assets/logo/stellar-admin-logo.svg">
</div>

StellarAdmin Tag Helpers is a collection of beautifully designed components based on [shadcn/ui](https://ui.shadcn.com/) which you can use to create CRUD screens in [ASP.NET Core](https://dotnet.microsoft.com/en-us/apps/aspnet) MVC and Razor Pages applications.

<div align="center">

[![MIT License](https://img.shields.io/badge/license-MIT-blue.svg)](https://opensource.org/licenses/MIT) [![NuGet](https://img.shields.io/nuget/v/StellarAdmin.TagHelpers)](https://www.nuget.org/packages/StellarAdmin.TagHelpers/)

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

builder.Services.AddStellarAdmin().AddTagHelpers();
```

### 3. Update imports

Update your `_ViewImports.cshtml` to register the StellarAdmin Tag Helpers and import the `StellarAdmin.TagHelpers` namespace.

```razor
@using StellarAdmin.TagHelpers
@addTagHelper *, StellarAdmin.TagHelpers
```

### 4. Link a theme stylesheet and JavaScript file

You must add the stylesheet for the [theme](https://www.stellaradmin.com/docs/tag-helpers/theming) you want to use, as well as the [StellarAdmin JavaScript file](https://www.stellaradmin.com/docs/tag-helpers/javascript) to your Razor page. The example below demonstrates how to include the script and the stylesheet for the Observatory theme.

```razor
<!DOCTYPE html>
<html lang="en">
<head>
    ...
    <link rel="stylesheet" href="/_content/StellarAdmin.TagHelpers/stellar-admin.observatory.css" asp-append-version="true"/>
    <script defer src="/_content/StellarAdmin.TagHelpers/stellar-admin.js" asp-append-version="true"></script>
</head>
<body>
    ...
</body>
</html>
```

> [!TIP]
> You can find more information and see the available themes on the [Theming](https://www.stellaradmin.com/docs/tag-helpers/theming) page.

### 5. Start using the Tag Helpers

Start using the StellarAdmin Tag Helpers inside your Razor Pages or MVC Views. For example, the code snippet below adds an alert to your page.

```razor
<sa-alert>
    <sa-alert-title>Success! You have configured StellarAdmin correctly.</sa-alert-title>
</sa-alert>
```

## Documentation

Documentation and code examples for all the Tag Helpers [can be found online](https://www.stellaradmin.com/docs/tag-helpers).

## Using StellarAdmin with AI agents

The [StellarAdmin Tag Helpers skill](skills/stellar-admin-tag-helpers/SKILL.md) teaches agents how to build MVC and Razor Pages UIs in `.cshtml` files, with component references and workflows for forms, layout, and theming. Its supporting documentation is bundled under `skills/stellar-admin-tag-helpers/references/`. Dashboard guidance is planned separately; see the [consumer skills overview](skills/README.md).

From your application's project directory, install the skill with [Vercel's skills CLI](https://github.com/vercel-labs/skills#readme):

```bash
npx skills add stellar-admin/stellar-admin --skill stellar-admin-tag-helpers
```

The skill is maintained alongside StellarAdmin; its references may include changes newer than your installed NuGet package. See [installation and compatibility](skills/README.md#install-the-tag-helpers-skill) for prerequisites, agent selection, and local-checkout installation.

`StellarAdmin.Core` supplies the shared `AddStellarAdmin()` entry point, builder, and icon services in `StellarAdmin.Icons`. It is installed transitively with TagHelpers or Dashboard; no separate installation is needed.

## Resource and admin packages

This repository also contains `StellarAdmin.Dashboard` (admin shell and resource screens), `StellarAdmin.Dashboard.Identity` (Identity management), and `StellarAdmin.Dashboard.EntityFrameworkCore` (EF Core resources). All are MIT licensed. Register the integrated admin application with `AddStellarAdmin().AddDashboard()`. There is no paid tier. These packages are available from source; their NuGet publication is a separate release step.

The full solution includes a unified `docs/DocsSamples` app, the website demo exporter, and the Identity playground used by the resource integration tests.

## Contributing

Start with [contributor guidance](AGENTS.md) and [development and verification](docs/development.md). This checkout includes the development skills, maintained designs, and plans; no workspace checkout is required. The public website remains a separate repository.
