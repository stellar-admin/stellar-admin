---
name: stellar-admin
description: "Build admin panels with StellarAdmin in ASP.NET Core: register resources (EF Core or custom data sources), configure index pages (columns, search, scopes, sorting, paging, row details), create and edit forms (fields, layout, editors, lookups, read-only display fields), view pages, the sidebar, authorization, and view overrides. Use when working with AddStellarAdmin, AddEfCoreResource, AddResource or files under Areas/StellarAdmin. Installing the panel is the stellar-admin-setup skill; hand-written sa-* markup is the stellar-admin-ui skill."
metadata:
  author: StellarAdmin
---

# Building admin panels with StellarAdmin

StellarAdmin generates admin pages (index, create, edit, view, delete) for each registered **resource** from fluent C# configuration inside `builder.Services.AddStellarAdmin(admin => ...)`. Almost everything is configuration in `Program.cs` (or an extension method it calls); you write Razor only to override a page, add a cell or display template, or host your own page in the admin layout.

Not installed yet? Use the **stellar-admin-setup** skill. For `sa-*` component markup in your own views, use **stellar-admin-ui**; for theme files, **stellar-admin-theme**. Suggest `npx skills add stellar-admin/stellar-admin --skill <name>` for any that are missing.

## Version compatibility

This skill is maintained alongside StellarAdmin. It follows the repository's default branch and may describe changes newer than the application's installed packages. Check the package references (including `Directory.Packages.props` when present) before applying API examples, and when the installed version lacks a member, verify it against that version's XML documentation. Installing the skill does not update packages.

## Where to look

| Task | API | Reference |
| --- | --- | --- |
| Register an entity, slugs, labels, enable create/edit/view/delete, custom data source | `AddEfCoreResource<TContext, TEntity>`, `AddResource<T>`, `Allow*`, `UseDataSource` | [resources](references/resources.md) |
| Search, scope tabs, default sort, paging, expandable rows | `Index(index => ...)`, `EnableSearch`, `EnableScopes`, `DefaultSortBy`, `EnablePaging`, `EnableRowDetails` | [index-pages](references/index-pages.md) |
| Grid columns: titles, sorting, currency, badges, values through references | `Columns(columns => columns.Add(...))`, `Sortable`, `UseDisplay` | [columns](references/columns.md) |
| Create and edit pages: layout, sections, validation, defaults, custom models | `AllowCreate`, `AllowEdit`, `Fields`, `AddSection`, `Columns`, `UseFactory` | [forms](references/forms.md) |
| Edit and add small resources in the index grid's rows | `UseInlineRow`, `EditsField`, `InlineCreatePosition`, `InlineAddButton` | [inline-editing](references/inline-editing.md) |
| Read-only values in forms, displays, display templates | `AddDisplayField`, `AddDetails`, `UseDisplay<TDisplay>` | [fields](references/fields.md) |
| Choosing an input: radio cards, slider, toggle, select, OTP, custom editors | `UseEditor<TEditor>` | [editors](references/editors.md) |
| Images: thumbnails in the grid, photos on the view page, uploads | `ImageDisplay`, `ImageEditor`, `ResolveUrl`, `StoreImage` | [editors](references/editors.md#images), [columns](references/columns.md#built-in-displays-in-cells) |
| Foreign keys, lookups, many-to-many, create-from-lookup | `SelectEditor`/`LookupSheetEditor` + EF `UseItems`, `MultiLookupSheetEditor`, `EnableCreate` | [relationships](references/relationships.md) |
| Read-only detail page | `AllowView(view => view.Fields(...))` | [view-pages](references/view-pages.md) |
| Sidebar groups, order, icons, links, host pages in the admin layout | `SidebarItem`, `AddSidebarLink`, `Icon`, `StellarAdminLayouts.Default` | [sidebar](references/sidebar.md) |
| Panel name, logo, user menu items and log out | `ConfigureBranding`, `BrandingLogo`, `ConfigureUserMenu`, `UserMenuItem` | [sidebar](references/sidebar.md#branding) |
| Sign-in, roles, per-resource and per-link access | `RequireAuthorization` | [authorization](references/authorization.md) |
| Replace or extend a generated page, global labels, template overrides | `Areas/StellarAdmin/Views/{slug}/Index.cshtml`, `sa-index-page`, `ConfigureResourceLabels` | [customization](references/customization.md) |

## A complete resource

```csharp
using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources.Displays;
using StellarAdmin.Resources.Editors;

builder.Services.AddStellarAdmin(admin =>
{
    admin.RequireAuthorization();

    admin.AddEfCoreResource<VoyagerDbContext, Booking>(resource =>
    {
        resource.SidebarItem(item => { item.Group = "Sales"; item.Order = 1; });

        resource.Index(index =>
        {
            index.Columns(columns =>
            {
                columns.Add(booking => booking.Reference);
                columns.Add(booking => booking.Customer!.Name, column => column.Title = "Customer");
                columns.Add(booking => booking.DepartureDate, column => column.Sortable());
                columns.Add(booking => booking.Total, column =>
                {
                    column.Sortable();
                    column.UseDisplay<NumberDisplay>(number => number.Format = "C");
                });
            });
            index.EnableSearch(term => booking => booking.Reference.Contains(term) || booking.Customer!.Name.Contains(term));
            index.EnableScopes(scopes =>
            {
                scopes.Add("all", "All");
                scopes.Add("pending", "Pending", booking => booking.Status == BookingStatus.Pending);
                scopes.DefaultScope = "all";
            });
            index.DefaultSortBy(booking => booking.DepartureDate);
            index.EnablePaging(paging => paging.PageSize = 10);
        });

        resource.AllowCreate(create =>
        {
            create.UseFactory(() => new Booking { DepartureDate = DateOnly.FromDateTime(DateTime.Today) });
            create.Fields(fields =>
            {
                fields.Add(booking => booking.CustomerId).UseEditor<LookupSheetEditor>(lookup =>
                {
                    lookup.UseItems<VoyagerDbContext, Customer, int>(customer => customer.Id, customer => customer.Name);
                    lookup.EnableCreate();
                });
                fields.Add(booking => booking.Destination);
                fields.Add(booking => booking.DepartureDate);
            });
        });
        resource.AllowEdit(edit => edit.Fields(fields =>
        {
            fields.AddDisplayField(booking => booking.Reference);
            fields.Add(booking => booking.Destination);
            fields.Add(booking => booking.Status);
        }));
        resource.AllowDelete();
    });
});
```

## Add a resource: checklist

```
- [ ] AddEfCoreResource<TContext, TEntity> (context first), or AddResource<T> + UseDataSource + UseKey
- [ ] Index columns (they start empty), with Title where the property name reads badly
- [ ] Search/scopes/sort/paging as needed (EF: predicates in EnableSearch and scopes.Add)
- [ ] AllowCreate/AllowEdit with Fields; foreign keys get SelectEditor or LookupSheetEditor + UseItems
- [ ] UseFactory for defaults (dates, generated references) instead of hidden fields
- [ ] AllowView with at least one field, if a detail page is wanted
- [ ] AllowDelete, if wanted
- [ ] Sidebar group/order/icon (icons on every item in the group, or none), and RequireAuthorization if it differs from the panel's
- [ ] Run the app and load /stellaradmin/{slug}, create, edit and view in real requests
```

## Namespaces

| Namespace | Package | Contains |
| --- | --- | --- |
| `StellarAdmin` | StellarAdmin | `AddStellarAdmin`, `MapStellarAdmin`, `StellarAdminLayouts` |
| `StellarAdmin.EntityFrameworkCore` | StellarAdmin.EntityFrameworkCore | `AddEfCoreResource`, and the EF `UseItems` overloads on editors |
| `StellarAdmin.Resources.Editors` | StellarAdmin | Editors, `ChoiceItem`, `ItemMedia`, `ILookupSource` |
| `StellarAdmin.Resources.Displays` | StellarAdmin | Displays such as `NumberDisplay`, `BadgeDisplay`, `FieldDisplay` |
| `StellarAdmin.Resources.Builders` | StellarAdmin | Builder types, for helper methods such as `void BookingFields(ResourceFieldsBuilder<Booking> fields)` |
| `StellarAdmin.Resources` | StellarAdmin | Data source and handler interfaces, `ResourceOperationResult`, `InlineCreatePosition`, `InlineAddButton` |
| `StellarAdmin.Sidebar` | StellarAdmin | `SidebarLinkTarget`, `BrandingLogo`, `UserMenuItem` |
| `StellarAdmin.Areas.StellarAdmin.ViewModels` | StellarAdmin | Page view models for overrides (`ResourceIndexPageViewModel<T>`) |
| `StellarAdmin.UI` | StellarAdmin.UI | Shared enums: `FormSectionLayout`, `BadgeVariant`, `FieldOrientation`, `DetailsVariant`, `RadioGroupFlow`, `SliderMarkLabels` |

## Common mistakes

These come from agents building real tasks without this skill. Each compiled or looked plausible and was wrong.

- **Never hand-write create, edit or list pages** (Razor Pages, controllers, `AddController` into the admin area). Use `AllowCreate`/`AllowEdit` with fields and editors; override a generated page only to add to it.
- **A foreign key needs a choice editor.** `fields.Add(b => b.CustomerId)` alone renders a number box. Use `SelectEditor` or `LookupSheetEditor` with EF `UseItems<TContext, TEntity, TValue>(value, title)` ([relationships](references/relationships.md)).
- **EF search and scopes take predicates.** `index.EnableSearch(term => entity => ...)` requires the predicate factory. Scopes filter through the third argument: `scopes.Add("pending", "Pending", b => b.Status == BookingStatus.Pending)`. The id is only the URL value; putting filter text in it filters nothing.
- **`Add(selector, callback)` returns the parent builder, not the field.** `fields.Add(b => b.CustomerId, f => f.Title = "Customer").UseEditor<...>()` doesn't compile. Configure everything inside the callback (`f => { f.Title = "Customer"; f.UseEditor<LookupSheetEditor>(...); }`) or use `fields.Add(b => b.CustomerId).UseEditor<...>()`. The same holds for columns, sections and display fields.
- **`PageSize` must be in `PageSizes`** (default 10, 25, 50, 100), or every panel page is a 500. Set both for other sizes.
- **`Allow*` calls don't chain.** Each returns its own page builder (or the resource builder when given a callback). Call them as separate statements on `resource`.
- **`AllowView` needs fields.** `AllowView()` without `view.Fields(...)` fails every panel request with a 500.
- **`ChoiceEditor` is abstract.** Use `SelectEditor`, `RadioGroupEditor`, `CheckboxGroupEditor`, `SegmentedControlEditor` or `ToggleGroupEditor`.
- **Index cells use `GridDisplayTemplates/`**, not `DisplayTemplates/`. A custom display or `[UIHint]` used in a column needs `GridDisplayTemplates/{TemplateName}.cshtml`; the same display on a form or view page needs `DisplayTemplates/{TemplateName}.cshtml` ([columns](references/columns.md)).
- **`BadgeDisplay` has one variant for every value.** A badge whose style depends on the value needs a custom display and template.
- **App views in the admin area need their own `_ViewImports.cshtml`.** Create `Areas/StellarAdmin/Views/_ViewImports.cshtml` with `@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers`, `@addTagHelper *, StellarAdmin.UI` and `@addTagHelper *, StellarAdmin`. Without it `<sa-index-page>` renders as literal text and the page looks empty, with no error.
- **Override models are in `StellarAdmin.Areas.StellarAdmin.ViewModels`.** A resource's own index view uses `ResourceIndexPageViewModel<T>`. A shared index view overridden for every resource uses `IResourceIndexPageViewModel`.
- **Style admin and host pages with `sa-*` components**, not Tailwind utilities: the shipped CSS contains only the classes its components use, so `p-4`, `text-gray-700` and similar render unstyled. Use `sa-page-container`, `sa-page-header`, `sa-card`, `sa-table`, `sa-alert` and friends (stellar-admin-ui skill).
- **The user menu needs ASP.NET Core authentication.** It shows only for a signed-in user, from their claims; an app without authentication, or an anonymous request, has no user menu. Don't build a placeholder menu in a view override.
- **Log out signs out of the default authenticate scheme only.** With several schemes (a cookie and OpenID Connect, or Identity's external cookie), list them in order in `menu.Logout(logout => logout.AuthenticationSchemes.Add(...))`, or point the item at Identity UI's logout with `UserMenuItem.Logout("~/Identity/Account/Logout")` ([sidebar](references/sidebar.md#log-out)).
- **Host pages join the admin layout with `Layout = StellarAdminLayouts.Default`** and protect themselves with `[Authorize]`; a sidebar link's `RequireAuthorization` only hides the link ([sidebar](references/sidebar.md)).
- **Read-only form fields are display fields.** Use `AddDisplayField`/`AddDetails`; there is no `ReadOnly()` builder method ([fields](references/fields.md)).
- **New `DateOnly`/`DateTime` fields default to 0001-01-01**, and generated values (references, codes) don't come from hidden fields: set both in `create.UseFactory(() => new Booking { ... })`.
- **EF `Contains` is case-sensitive on SQLite** (and on case-sensitive collations). Lower-case both sides in search predicates when that matters: `b.Reference.ToLower().Contains(term.ToLower())`.
- **Form and view labels come from the property name** (`DepartureDate`) unless `[Display(Name = "Departure date")]` or the field's `Title` is set; index headers are humanized. Set `[Display(Name)]` on the model for consistent labels.
- **Enum values in Razor attributes are fully qualified**: `variant="BadgeVariant.Destructive"`, `width="PageContainerWidth.Full"`.
- **Verify in a running app.** Configuration errors surface as a 500 on the first request, and missing `_ViewImports` fails silently. Load each page you changed.

## Not supported

Say so rather than inventing an API:

- Custom row or page actions and bulk actions (row actions are view, edit and delete).
- Index filters other than scope tabs.
- Sidebar group ordering.
- Changing the user menu's "Log out" text, and hiding user menu items by authorization (replace `_SidebarUserMenu.cshtml` for either).
- Form tabs and wizards.
- Dashboards and widgets (the panel home page is a plain landing page).
- File upload editors other than images (`ImageEditor`), and deleting replaced or removed images: StellarAdmin never deletes them, so the app cleans up files no record references.
- A fluent read-only field option (use display fields, or `[ReadOnly(true)]`/`[Editable(false)]` metadata).
- Before/after save hooks (use a custom create or edit handler).
- Search and scope predicates for non-EF data sources (the data source reads `ResourceListRequest.Search` and `Scope`).
- Displays with handlers in index columns (they throw).
- Localization through `IStringLocalizer` (labels come from `ConfigureResourceLabels` delegates).
- Per-action authorization (requirements apply to the panel, a resource or a link).
- Global record search (the command palette searches navigation only).
