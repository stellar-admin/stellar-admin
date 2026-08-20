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

builder.Services.AddStellarAdmin().AddTagHelpers();
```

### 3. Update imports

Update your `_ViewImports.cshtml` to register the StellarAdmin Tag Helpers and import the `StellarAdmin.TagHelpers` namespace.

```razor
@using StellarAdmin.TagHelpers
@addTagHelper *, StellarAdmin.TagHelpers
```

### 4. Link a theme stylesheet and JavaScript file

StellarAdmin Tag Helpers comes with the same themes as shadcn/ui, namely Vega, Nova, Maia, Lyra, Mira, Luma, Sera, and Rhea. You must add the stylesheet for the theme you want to use to your Razor layout. The URL for the theme is in the format (`/_content/StellarAdmin.TagHelpers/stellar-admin.<theme>.css`). 

StellarAdmin TagHelpers also comes with minimal JavaScript which adds interactivity to some of the Tag Helpers via Web Components. To enable this, you must also include the `stellar-admin.js` script in your layout.

The example below demonstrates how to include the script and the stylesheet for the **Nova** theme.

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

> [!TIP]
> All the Tag Helper examples on our [documentation website](https://www.stellaradmin.com/docs/tag-helpers) allows you to preview the examples in each of the different themes. Just select the _Theme_ picker above any of the examples.
> 
> You can also go the [shadcn/ui Create page](https://ui.shadcn.com/create) and use their _Style_ picker, which correspond with the StellarAdmin themes. This will give you a good idea of the look-and-feel of each of the themes. 

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

Install them from this repository, which doubles as a plugin marketplace:

```bash
/plugin marketplace add stellar-admin/stellar-admin
/plugin install stellar-admin@stellar-admin
```

This adds the `stellar-admin` skill (auto-activates when you edit `.cshtml` / `.razor` files) along with `stellar-admin-forms`, `stellar-admin-layout`, and `stellar-admin-theming`.
