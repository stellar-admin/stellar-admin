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

## Using StellarAdmin with AI agents (Claude Code)

StellarAdmin ships a set of [Claude Code](https://claude.com/claude-code) skills that teach an AI agent how to build UIs with StellarAdmin — the component catalog, the library's conventions, and task workflows for forms, layout, and theming.

The skills live in the [stellar-admin/skills](https://github.com/stellar-admin/skills) repository, which doubles as a plugin marketplace:

```bash
/plugin marketplace add stellar-admin/skills
/plugin install stellar-admin@stellar-admin
```

This adds the `tag-helpers` skill (auto-activates when you edit `.cshtml` / `.razor` files) along with `forms`, `layout`, and `theming`.

`StellarAdmin.Core` supplies the shared `AddStellarAdmin()` entry point, builder, and icon services in `StellarAdmin.Icons`. It is installed transitively with TagHelpers or Dashboard; no separate installation is needed.

## Resource and admin packages

This repository also contains `StellarAdmin.Dashboard` (admin shell and resource screens), `StellarAdmin.Dashboard.Identity` (Identity management), and `StellarAdmin.Dashboard.EntityFrameworkCore` (EF Core resources). All are MIT licensed. Register the integrated admin application with `AddStellarAdmin().AddDashboard()`. There is no paid tier. These packages are available from source; their NuGet publication is a separate release step.

The full solution includes a unified `docs/DocsSamples` app, the website demo exporter, and the Identity playground used by the resource integration tests.
