---
name: stellar-admin-setup
description: "Install the StellarAdmin admin panel into an ASP.NET Core app: NuGet packages, AddStellarAdmin, MapStellarAdmin and middleware order, sign-in protection, the first EF Core resource, a theme, and checking that it renders. Use when adding StellarAdmin to a project or when /stellaradmin returns 404, renders unstyled or shows raw tags. Building resources, forms, columns and the sidebar after installation is the stellar-admin skill."
metadata:
  author: StellarAdmin
---

# Installing the StellarAdmin admin panel

StellarAdmin adds an admin panel to an ASP.NET Core app: list, create, edit, view and delete pages for your entities, generated from C# configuration in `Program.cs`. You do not write those pages; you register resources and configure them.

After installation, the **stellar-admin** skill covers resources, index pages, forms, editors, the sidebar and customization. If it isn't installed, suggest `npx skills add stellar-admin/stellar-admin --skill stellar-admin`.

## Version compatibility

This skill is maintained alongside StellarAdmin. It follows the repository's default branch and may describe changes newer than the application's installed packages. Check the package references (including `Directory.Packages.props` when present) before applying API examples. Installing the skill does not install or update packages.

## Checklist

Copy this list and tick it off:

```
- [ ] 1. Packages referenced (StellarAdmin, plus StellarAdmin.EntityFrameworkCore for EF Core)
- [ ] 2. builder.Services.AddStellarAdmin(...) with at least one resource; every foreign key field has a choice editor
- [ ] 3. app.MapStellarAdmin() after UseAuthentication/UseAuthorization, with static assets mapped
- [ ] 4. Sign-in or role protection, if the app has authentication
- [ ] 5. The panel's name (and logo, if the app has one); theme, if wanted
- [ ] 6. Built, run, and /stellaradmin checked in a real request
```

## 1. Packages

The app must target .NET 10 or later.

```bash
dotnet add package StellarAdmin
dotnet add package StellarAdmin.EntityFrameworkCore   # when the entities live in an EF Core DbContext
```

`StellarAdmin` brings in `StellarAdmin.UI`, the component library the panel is built with; don't add or configure it separately. With central package management, add the versions to `Directory.Packages.props` instead.

## 2. Register the panel

`AddStellarAdmin` registers MVC, the panel and the UI components. Register each entity as a resource inside it. For an EF Core entity use `AddEfCoreResource<TContext, TEntity>`; the DbContext comes first.

```csharp
using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;

builder.Services.AddDbContext<VoyagerDbContext>(options => options.UseSqlite("Data Source=voyager.db"));

builder.Services.AddStellarAdmin(admin =>
{
    admin.AddEfCoreResource<VoyagerDbContext, Customer>(resource =>
    {
        resource.Index(index => index.Columns(columns =>
        {
            columns.Add(customer => customer.Name);
            columns.Add(customer => customer.Email);
            columns.Add(customer => customer.Country);
        }));
        resource.AllowCreate(create => create.Fields(fields =>
        {
            fields.Add(customer => customer.Name);
            fields.Add(customer => customer.Email);
            fields.Add(customer => customer.Country);
        }));
        resource.AllowEdit(edit => edit.Fields(fields =>
        {
            fields.Add(customer => customer.Name);
            fields.Add(customer => customer.Email);
            fields.Add(customer => customer.Country);
        }));
        resource.AllowDelete();
    });
});
```

- The DbContext must be registered with `AddDbContext` (any provider). The key is discovered from EF metadata: a single `int`, `long`, `Guid` or `string` property.
- Index columns start empty: a resource without columns shows an empty grid.
- Create, edit, view and delete are off until `AllowCreate`, `AllowEdit`, `AllowView` and `AllowDelete` are called. Each takes its own callback; they don't chain off each other.
- Entities that don't live in EF Core use `admin.AddResource<T>(resource => resource.UseDataSource<TDataSource>())`; see the stellar-admin skill.

### Foreign keys

A foreign key field such as `Booking.CustomerId` renders as a **number input** unless it gets a choice editor, so staff would have to type customer IDs. Give every foreign key field on the create and edit pages a `SelectEditor` that lists the related rows:

```csharp
using StellarAdmin.Resources.Editors;

fields.Add(booking => booking.CustomerId).UseEditor<SelectEditor>(select =>
    select.UseItems<VoyagerDbContext, Customer, int>(
        customer => customer.Id,
        customer => customer.Name,
        items => items.OrderBy(customer => customer.Name)));
```

The type arguments are the DbContext, the related entity and the key type (`int` for `int` and `int?` keys). For a long list, `LookupSheetEditor` takes the same `UseItems` and adds search and "create new"; see the stellar-admin skill's relationships reference. In the index, show the related name with `columns.Add(booking => booking.Customer!.Name, column => column.Title = "Customer")` rather than the key.

## 3. Map the routes

```csharp
var app = builder.Build();

app.UseRouting();
app.UseAuthentication();   // when the app has authentication
app.UseAuthorization();

app.MapStaticAssets();     // or app.UseStaticFiles(); serves the panel's CSS and scripts
app.MapStellarAdmin();     // serves the panel at /stellaradmin
app.MapRazorPages();       // the app's own endpoints, in any order relative to MapStellarAdmin
```

- `MapStellarAdmin("/admin")` serves the panel under another path.
- Resource pages are at `/stellaradmin/{slug}`, where the slug is the kebab-case plural of the type name: `Booking` → `/stellaradmin/bookings`, `OrderItem` → `/stellaradmin/order-items`. Pass a slug to override it: `AddEfCoreResource<VoyagerDbContext, Booking>("reservations", resource => ...)`.
- Without static assets mapped, the panel renders as unstyled HTML.

## 4. Protect it

The panel is open to everyone until you require authorization. Requirements are enforced by the app's own authentication and authorization middleware, so those must be configured and run before `MapStellarAdmin`.

```csharp
builder.Services.AddStellarAdmin(admin =>
{
    admin.RequireAuthorization(policy => policy.RequireRole("Staff"));   // or RequireAuthorization() for any signed-in user
    // resources...
});
```

`RequireAuthorization` takes the same arguments as endpoint `RequireAuthorization`: none, policy names, an `AuthorizationPolicy`, `IAuthorizeData` or a policy-builder callback. `app.MapStellarAdmin().RequireAuthorization("Staff")` works too. Anonymous users get the authentication challenge; signed-in users without access get the forbidden response. With cookie authentication those are redirects to `LoginPath` and `AccessDeniedPath` (`/Account/Login` and `/Account/AccessDenied` by default), so the app needs those pages or other paths configured in `AddCookie(options => ...)`. Per-resource rules are in the stellar-admin skill's authorization reference.

## 5. Name, logo and theme

The sidebar header, the app header and the browser title show the application name made readable (`Voyager.Web` becomes "Voyager Web"). Set the app's real name, and its logo if it has one:

```csharp
using StellarAdmin.Sidebar;

builder.Services.AddStellarAdmin(admin =>
{
    admin.ConfigureBranding(branding =>
    {
        branding.Title = "Voyager Travel";
        branding.Logo = BrandingLogo.Image("~/img/logo.svg");   // or BrandingLogo.Icon("compass"), or BrandingLogo.Wordmark(...)
    });
});
```

When the app has authentication, the sidebar footer shows the signed-in user with a log out item that signs out of the default authenticate scheme. Apps with several schemes (OpenID Connect) or with Identity UI configure it with `ConfigureUserMenu`; see the stellar-admin skill's sidebar reference.

The panel uses the default theme. To change the look, add a theme file of CSS variables and any web font it needs:

```csharp
builder.Services.AddStellarAdmin(admin =>
{
    admin.AddStylesheet("https://fonts.googleapis.com/css2?family=Lexend:wght@400..700&display=swap");
    admin.AddStylesheet("~/css/theme.css");
});
```

Stylesheets load after the panel's own, in registration order; `admin.AddScript("~/js/admin.js")` adds scripts the same way. The **stellar-admin-theme** skill writes theme files from a description or brand colours.

## 6. Verify in a real request

Building is not enough: configuration errors surface on the first request. Run the app and fetch the pages (sign in first if you protected the panel):

1. `/stellaradmin` returns 200 with a sidebar listing each resource.
2. `/stellaradmin/{slug}` shows the grid with the configured columns and rows.
3. `/stellaradmin/{slug}/create` and `/stellaradmin/{slug}/edit/{id}` render and save, for resources that allow them. Foreign keys show a list of related rows, not a number box.
4. The page is styled. Raw `<sa-...>` tags in the HTML mean a view is missing its `_ViewImports.cshtml` tag helper registrations (see the stellar-admin skill's customization reference).
5. The sidebar header shows the app's name. When signed in, the sidebar footer shows the user, and Log out signs them out.

Options validation errors (for example `AllowView` without fields, or a default sort on a column that isn't sortable) fail every panel request with a 500 and name the problem in the log.

## Troubleshooting

If a check fails, see [troubleshooting](references/troubleshooting.md) for symptoms and their causes.
