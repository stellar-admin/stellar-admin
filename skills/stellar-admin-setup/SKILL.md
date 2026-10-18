---
name: stellar-admin-setup
description: "Install the StellarAdmin admin panel into an ASP.NET Core app: NuGet packages, AddStellarAdmin, MapStellarAdmin and middleware order, and checking that it renders. Asks before adding resources, sign-in protection, branding or a theme. Use when adding StellarAdmin to a project or when /stellaradmin returns 404, renders unstyled or shows raw tags. Building resources, forms, columns and the sidebar after installation is the stellar-admin skill."
metadata:
  author: StellarAdmin
---

# Installing the StellarAdmin admin panel

StellarAdmin adds an admin panel to an ASP.NET Core app: list, create, edit, view and delete pages for your entities, generated from C# configuration in `Program.cs`. You do not write those pages; you register resources and configure them.

Installation itself is only the packages, `AddStellarAdmin` and `MapStellarAdmin`. Resources, protection, branding and a theme are the user's decisions: ask about them, and don't add any the user hasn't chosen.

After installation, the **stellar-admin** skill covers resources, index pages, forms, editors, the sidebar and customization. If it isn't installed, suggest `npx skills add stellar-admin/stellar-admin --skill stellar-admin`.

## Version compatibility

This skill is maintained alongside StellarAdmin. It follows the repository's default branch and may describe changes newer than the application's installed packages. Check the package references (including `Directory.Packages.props` when present) before applying API examples. Installing the skill does not install or update packages.

## Checklist

Copy this list and tick it off:

```
- [ ] 1. Asked about resources, protection, name/logo and theme (skip what the request already says)
- [ ] 2. Packages referenced (StellarAdmin, plus StellarAdmin.EntityFrameworkCore for EF Core resources)
- [ ] 3. AddStellarAdmin() and app.MapStellarAdmin(), with static assets mapped
- [ ] 4. Chosen resources added, if any
- [ ] 5. Chosen protection added, if any
- [ ] 6. Chosen name, logo and theme added, if any
- [ ] 7. Built, run, and checked in real requests; reported what was configured and what was skipped
```

## 1. Ask first

Before changing files, ask the user in one message about each of these that their request doesn't already answer. Each one is optional, and "none" or "not now" is a valid answer:

- **Resources:** which entities to manage in the panel. When the app has a DbContext, list its entity types as candidates. Don't pick one for the user, and don't create entities or a DbContext to have something to show.
- **Protection:** only when the app has authentication. Offer any signed-in user, a role or policy (name the roles or policies the app already uses), or none for now. When the app has no authentication, don't ask and don't set it up; tell the user the panel will be open to everyone.
- **Name and logo:** the name shown in the sidebar and browser title, and a logo if the app has one.
- **Theme:** the default look, or a theme file (the **stellar-admin-theme** skill writes one).

When the request settles everything, don't ask. When the user declines or says to just install it, do steps 2, 3 and 7 only.

## 2. Packages

The app must target .NET 10 or later.

```bash
dotnet add package StellarAdmin
dotnet add package StellarAdmin.EntityFrameworkCore   # only when resources come from an EF Core DbContext
```

`StellarAdmin` brings in `StellarAdmin.UI`, the component library the panel is built with; don't add or configure it separately. With central package management, add the versions to `Directory.Packages.props` instead.

## 3. Register and map the panel

`AddStellarAdmin` registers MVC, the panel and the UI components. A panel without resources is valid: it renders with an empty sidebar.

```csharp
using StellarAdmin;

builder.Services.AddStellarAdmin(admin =>
{
    // resources, protection and branding go here when chosen
});

var app = builder.Build();

app.UseRouting();
app.UseAuthentication();   // only when the app already has authentication
app.UseAuthorization();

app.MapStaticAssets();     // or app.UseStaticFiles(); serves the panel's CSS and scripts
app.MapStellarAdmin();     // serves the panel at /stellaradmin
app.MapRazorPages();       // the app's own endpoints, in any order relative to MapStellarAdmin
```

- When the app has authentication, `UseAuthentication` and `UseAuthorization` must run before `MapStellarAdmin`, or protection added later won't apply.
- `MapStellarAdmin("/admin")` serves the panel under another path.
- Without static assets mapped, the panel renders as unstyled HTML.

## 4. Resources (when chosen)

Register each chosen entity inside `AddStellarAdmin`, following the stellar-admin skill's "Add a resource" checklist. For an EF Core entity use `AddEfCoreResource<TContext, TEntity>`; the DbContext comes first.

```csharp
using StellarAdmin.EntityFrameworkCore;

builder.Services.AddStellarAdmin(admin =>
{
    admin.AddEfCoreResource<VoyagerDbContext, Customer>(resource =>
    {
        resource.Index(index => index.Columns(columns =>
        {
            columns.Add(customer => customer.Name);
            columns.Add(customer => customer.Email);
        }));
        resource.AllowCreate(create => create.Fields(fields =>
        {
            fields.Add(customer => customer.Name);
            fields.Add(customer => customer.Email);
        }));
        resource.AllowEdit(edit => edit.Fields(fields =>
        {
            fields.Add(customer => customer.Name);
            fields.Add(customer => customer.Email);
        }));
        resource.AllowDelete();
    });
});
```

- The DbContext must be registered with `AddDbContext` (any provider). The key is discovered from EF metadata: a single `int`, `long`, `Guid` or `string` property.
- Index columns start empty, and create, edit, view and delete are off until `AllowCreate`, `AllowEdit`, `AllowView` and `AllowDelete` are called. Add only the pages the user wants.
- A foreign key field such as `Booking.CustomerId` renders as a number input unless it gets a `SelectEditor` or `LookupSheetEditor` with EF `UseItems`; see the stellar-admin skill's relationships reference.
- Resource pages are at `/stellaradmin/{slug}`, where the slug is the kebab-case plural of the type name: `Booking` → `/stellaradmin/bookings`, `OrderItem` → `/stellaradmin/order-items`.
- Entities that don't live in EF Core use `admin.AddResource<T>(resource => resource.UseDataSource<TDataSource>())`; see the stellar-admin skill.

## 5. Protection (when chosen)

The panel is open to everyone until you require authorization. Add the access the user chose:

```csharp
builder.Services.AddStellarAdmin(admin =>
{
    admin.RequireAuthorization(policy => policy.RequireRole("Staff"));   // or RequireAuthorization() for any signed-in user
});
```

`RequireAuthorization` takes the same arguments as endpoint `RequireAuthorization`: none, policy names, an `AuthorizationPolicy`, `IAuthorizeData` or a policy-builder callback. `app.MapStellarAdmin().RequireAuthorization("Staff")` works too. Anonymous users get the authentication challenge; signed-in users without access get the forbidden response. With cookie authentication those are redirects to `LoginPath` and `AccessDeniedPath` (`/Account/Login` and `/Account/AccessDenied` by default), so the app needs those pages or other paths configured in `AddCookie(options => ...)`. Per-resource rules are in the stellar-admin skill's authorization reference.

When the app has authentication, the sidebar footer shows the signed-in user with a log out item that signs out of the default authenticate scheme. Apps with several schemes (OpenID Connect) or with Identity UI configure it with `ConfigureUserMenu`; see the stellar-admin skill's sidebar reference.

## 6. Name, logo and theme (when chosen)

By default the sidebar header, the app header and the browser title show the application name made readable (`Voyager.Web` becomes "Voyager Web"). To set a name or logo:

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

The panel uses the default theme. To change the look, add a theme file of CSS variables and any web font it needs:

```csharp
builder.Services.AddStellarAdmin(admin =>
{
    admin.AddStylesheet("https://fonts.googleapis.com/css2?family=Lexend:wght@400..700&display=swap");
    admin.AddStylesheet("~/css/theme.css");
});
```

Stylesheets load after the panel's own, in registration order; `admin.AddScript("~/js/admin.js")` adds scripts the same way. The **stellar-admin-theme** skill writes theme files from a description or brand colours.

## 7. Verify in a real request

Building is not enough: configuration errors surface on the first request. Run the app and fetch the pages (sign in first if you protected the panel):

1. `/stellaradmin` returns 200 and is styled. In Development it shows a setup checklist of the steps the app hasn't configured yet; that's expected. Raw `<sa-...>` tags in the HTML mean a view is missing its `_ViewImports.cshtml` tag helper registrations (see the stellar-admin skill's customization reference).
2. The sidebar header shows the app's name.
3. With resources: the sidebar lists each one, `/stellaradmin/{slug}` shows the grid with the configured columns and rows, and `/stellaradmin/{slug}/create` and `/stellaradmin/{slug}/edit/{id}` render and save for resources that allow them. Foreign keys show a list of related rows, not a number box.
4. With protection: an anonymous request gets the challenge, a signed-in user without access is forbidden, an allowed user gets 200, and Log out in the sidebar footer signs them out.

Options validation errors (for example `AllowView` without fields, or a default sort on a column that isn't sortable) fail every panel request with a 500 and name the problem in the log.

Finish by telling the user what was configured and what was skipped. When the panel has no protection, say plainly that anyone who can reach the app can use it. When no resources were added, say that the stellar-admin skill adds them later.

## Troubleshooting

If a check fails, see [troubleshooting](references/troubleshooting.md) for symptoms and their causes.
