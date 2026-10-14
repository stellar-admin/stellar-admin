# StellarAdmin setup notes

These preserved notes support the generated component references; full admin panel guidance is still planned.

## Resource packages

The resource page helpers live in `StellarAdmin`, which is MIT licensed alongside the rest of StellarAdmin. These packages are currently available from the source repository; NuGet publication is a separate release step. Reference the `StellarAdmin` project, add `using StellarAdmin;` in startup, and call `AddStellarAdmin(...)`, which also registers StellarAdmin.UI. In `_ViewImports.cshtml`, add `@using StellarAdmin` and `@addTagHelper *, StellarAdmin`.

## Admin panel theme

The admin panel uses the default theme: its layout links `stellar-admin.css`. For another look, add a theme file of CSS variables (written by hand or downloaded from the [theme builder](https://www.stellaradmin.com/theme-builder)) and any web font it needs as stylesheets in `Program.cs`:

```csharp
builder.Services.AddStellarAdmin(admin =>
{
    admin.AddStylesheet("https://fonts.googleapis.com/css2?family=Lexend:wght@400..700&display=swap");
    admin.AddStylesheet("~/css/theme.css");
});
```

The layout links `stellar-admin.css` first, then the registered stylesheets in registration order. A theme file sets its variables in `@layer sa.theme`, so it outranks the admin panel's own stylesheet whatever the order. The admin panel makes no font requests unless a font stylesheet is registered; without one, the theme's font stacks fall back to native fonts. The theme applies to the whole admin panel; it does not add a visitor theme picker. The stellar-admin-theme skill writes theme files; the stellar-admin-ui skill's theming guide covers fonts and dark mode.

## Resource registration

Register a resource through the `StellarAdminBuilder`:

```csharp
using StellarAdmin;
using StellarAdmin.Resources.Displays;

builder.Services.AddStellarAdmin(admin =>
{
    admin.AddResource<Product>(resource =>
    {
        resource.UseDataSource<ProductDataSource>();
        resource.AllowCreate(create => create.Fields(fields =>
        {
            fields.Add(product => product.Name);
            fields.Add(product => product.Price);
        }));
        resource.Index(index => index.Columns(columns =>
        {
            columns.Add(product => product.Name);
            columns.Add(product => product.Price, column =>
            {
                column.Title = "Unit price";
                column.UseDisplay<NumberDisplay>(number => number.Format = "0.00");
            });
        }));
    });
    admin.AddResource<ProductCategory>(resource =>
    {
        resource.SingularLabel = "Category";
        resource.PluralLabel = "Catalog";
    });
});
```

Call `app.MapStellarAdmin()` to map the admin panel routes. The example's Product index is available at `/stellaradmin/products`. The route uses the resource's slug, which defaults to the kebab-case plural of the type name (`OrderItem` becomes `order-items`) independently of its display labels. Pass a slug to `AddResource<T>("catalog")` or `AddEfCoreResource<TContext, T>("catalog")` to override it; slugs use lowercase letters and digits separated by single hyphens and must be unique. Generated admin panel URLs use lowercase action segments; requests match case-insensitively. Resource-specific view overrides live in `Areas/StellarAdmin/Views/{slug}/` and keep PascalCase action file names such as `Create.cshtml`. Register a data source and columns for each resource whose index you want to display. Register `.AllowCreate(...)` to enable `/stellaradmin/products/create`. Register `.AllowEdit(...)` and configure a key with `resource.UseKey(product => product.Id)` to enable `/stellaradmin/products/edit/{id}`. Register `.AllowDelete()` or `.AllowDelete(...)` to enable deletion through an antiforgery-protected POST with confirmation on the index and edit pages; deletion also requires a key. A rejected delete from the edit page returns to Edit with its errors, while a rejected index delete redisplays the index. Each resource adds a sidebar link to its index, configurable through `resource.SidebarItem(...)`.

`ProductDataSource` implements `IResourceCrudDataSource<Product>` from `StellarAdmin.Resources` for all CRUD operations. The combined interface includes `IResourceDataSource<Product>` (`ListAsync`), `IResourceCreateHandler<Product>` (`CreateAsync`), `IResourceEditHandler<Product>` (`FindAsync` and `UpdateAsync`), and `IResourceDeleteHandler<Product>` (`DeleteAsync`). A source can instead implement just the individual interfaces it supports. The shared controller forwards the request cancellation token. Unregistered actions have no buttons and direct requests are rejected, even when the data source implements the corresponding handler. Configuring a form or deletion without a matching implementation produces an options validation error. `UseDataSource<TDataSource>()` registers the concrete source as scoped unless the application already registered it. Resolve its dependencies through its constructor. An existing concrete registration's lifetime is preserved. Repeated `UseDataSource` calls select the last source.

Index columns start empty. `Add` appends and `Clear` removes the configured columns. `Add(selector)` returns the column builder. `Add(selector, configure)` returns the columns builder. Column `Title` and `EmptyText` are setter-only. Omitted titles use the selected property's display metadata/name. A column renders its cells with the same displays as a display field (see [Display fields](#display-fields)): through `UseDisplay<TDisplay>(display => ...)`; as the grid's plain text after `UseBuiltInRendering()`, formatted with `[DisplayFormat]`; through the grid display template its `[UIHint("Status")]` names, the view `GridDisplayTemplates/Status`; or through the default display for its type. A display's template in a cell is `GridDisplayTemplates/{TemplateName}`, so a custom display used in a column needs one, and an override goes at a path such as `Areas/StellarAdmin/Views/Shared/GridDisplayTemplates/Displays/Boolean.cshtml`. In a cell, `TextDisplay` ignores line breaks unless `LineClamp` is set, and a `NumberDisplay` column is right-aligned. Displays with a handler are not supported in columns and throw. A column can read through references, such as `product => product.Category.Name`; an EF Core resource's index loads the references on each column's path with the page, in the same query. An empty value shows `EmptyText`, then `[DisplayFormat(NullDisplayText)]`, then an empty cell. Set `index.Title` to override the page title, which otherwise uses the current resource plural label. Empty results display an empty state. Paging, sortable columns, and search are opt-in index features.

To customize a resource's index, add `Areas/StellarAdmin/Views/Product/Index.cshtml` to the application. The controller first searches for `Index` using normal MVC view lookup, including shared locations and configured view-location expanders, then falls back to `ResourceIndex` when no view is found. The built-in fallback lives in `Areas/StellarAdmin/Views/Shared/ResourceIndex.cshtml` and can also be overridden by the application. A custom view can use `ResourceIndexPageViewModel<Product>` from `StellarAdmin.Areas.StellarAdmin.ViewModels` and render `<sa-index-page model="Model" />` or supply its own markup.

The singular label defaults to the readable type name (`ProductCategory` becomes `Product category`). The plural defaults to its English plural (`Product categories`). Setting only `SingularLabel` also changes the inferred plural. An explicit `PluralLabel` takes precedence regardless of assignment order. Labels must be nonblank. Set both labels for other languages or domain-specific wording. Naming uses [Humanizer](https://github.com/Humanizr/Humanizer).

Resolve configuration through `IOptions<ResourceOptions<Product>>` (`Microsoft.Extensions.Options` and `StellarAdmin.Resources.Options`). The builder callback executes during registration. Its setter-only properties register configuration actions, which execute when options resolve. Each resource type and service provider gets its own options instance. Repeated `AddResource` registrations compose in assignment order, and later assignments to the same property win. A no-argument `AddResource` registration preserves existing configuration. Registering the same resource again under a different slug throws. Each `AllowCreate`, `AllowEdit`, or `AllowDelete` call replaces that action’s previous configuration; setters and field calls on the returned builder compose. Standard Configure, PostConfigure, and options validation remain available. This allows an integration to register defaults first and application code to override them afterwards.

The no-callback overload returns the resource builder. The callback overload returns the `StellarAdminBuilder`:

```csharp
var resource = admin.AddResource<Product>();
resource.SingularLabel = "Item";
resource.PluralLabel = "Inventory";
```

## Authorization

Admin panel pages are open unless the application requires authorization. Call `RequireAuthorization` on the `StellarAdminBuilder` to protect every page, or on a resource builder to protect one resource:

```csharp
builder.Services.AddStellarAdmin(admin =>
{
    admin.RequireAuthorization();

    admin.AddResource<Product>(resource =>
    {
        resource.RequireAuthorization(policy => policy.RequireRole("Catalog"));
    });
});
```

Both builders accept the same overloads as the framework's endpoint `RequireAuthorization`: no arguments for any authenticated user, policy names, `IAuthorizeData` such as `new AuthorizeAttribute { Roles = "Admin" }`, an `AuthorizationPolicy`, or a policy-builder callback. Requirements accumulate. A user must satisfy every admin panel requirement, every requirement of the resource, and every repeated call. EF Core resource builders provide the same methods.

Requirements are applied as endpoint metadata when `app.MapStellarAdmin()` maps the routes, so the application's authentication and authorization middleware enforce them for page requests and form posts alike. Anonymous users receive the authentication challenge and signed-in users without access receive the forbidden response. `MapStellarAdmin()` also returns the route's convention builder, so standard conventions such as `app.MapStellarAdmin().RequireAuthorization("Admin")` or `RequireHost(...)` can be added there.

The sidebar hides a resource's link when the current user does not satisfy that resource's requirements. Hiding a link with `resource.SidebarItem(item => item.Visible = false)` never protects the route. Custom `ISidebarItemsProvider` implementations return their items from `Task<SidebarItem[]> GetItemsAsync(HttpContext httpContext)`, which can inspect the current user. Scopes and data sources remain responsible for record-level access rules.

## Custom sidebar links

Add links to the application's own Razor Pages, MVC actions or any URL with `AddSidebarLink`:

```csharp
using StellarAdmin.Sidebar;

builder.Services.AddStellarAdmin(admin =>
{
    admin.AddSidebarLink("Overview", SidebarLinkTarget.Page("/Overview"));

    admin.AddSidebarLink("Invoices", SidebarLinkTarget.Action("Index", "Invoices"), link =>
    {
        link.Group = "Commerce";
        link.Order = 30;
        link.RequireAuthorization("Finance");
    });

    admin.AddSidebarLink("Status page", SidebarLinkTarget.Url("https://status.voyager.travel"), link =>
    {
        link.Group = "Help";
        link.OpenInNewTab = true;
    });
});
```

`SidebarLinkTarget.Page(page, area)` and `SidebarLinkTarget.Action(action, controller, area)` link outside any area unless an area is given, so they reach the host application's pages rather than the admin panel's. `SidebarLinkTarget.Url(url)` accepts absolute URLs and app-relative `~/` paths. Each link also appears in the command palette, which searches link labels and group names.

A link without a `Group` appears at the top level, without a heading. Links join a resource group with the same label, so the Invoices link above sits in the Commerce group with Commerce resources. Groups with the same label from any `ISidebarItemsProvider` merge into one, positioned where the label first appears. Links in a group sort by `Order`, which defaults to 0. With equal `Order` values, resource links and custom links do not interleave in call order: all resource links come before all custom links when the first `AddResource` call precedes the first `AddSidebarLink` call, and after them otherwise. Set `Order` to place a link among resources. `OpenInNewTab` opens the link in a new browser tab.

`RequireAuthorization` accepts the same overloads as the `StellarAdminBuilder` and hides the link from users who do not satisfy it. It does not protect the destination: host pages and actions are outside the admin panel's routes, so protect them with the application's own authorization. Linked host pages render with the application's layout unless they opt into the admin panel's.

To render a host Razor Page or MVC view inside the admin panel shell, with its sidebar, header, theme, command palette and sheets, set `Layout = StellarAdminLayouts.Default;` (namespace `StellarAdmin`) in the page or in a `_ViewStart.cshtml` beside it. The page keeps its own URL and authorization. `ViewData["Title"]` sets the browser title as `{title} - StellarAdmin`. Add `@addTagHelper *, StellarAdmin.UI` to the page's `_ViewImports.cshtml` to use components such as `sa-page-container` and `sa-page-header` so the body matches admin panel pages. A sidebar link to the page is marked active while it is open.

## Index sorting

Opt columns into sorting and optionally choose a default:

```csharp
resource.Index(index =>
{
    index.Columns(columns =>
    {
        columns.Add(product => product.Name, column => column.Sortable());
        columns.Add(product => product.Price, column => column.Sortable());
    });
    index.DefaultSortBy(product => product.Name);
    // Or: index.DefaultSortByDescending(product => product.Price);
});
```

The default must select a configured sortable column. `ResourceListRequest.Sort` supplies the field name and `ResourceSortDirection` to the data source; null leaves ordering to the source. Apply ordering before paging, with a unique tie-breaker for equal values. The shared controller does not sort returned rows. Header links toggle direction through HTMX, reset the page, and preserve page size. Paging and deletion preserve the selected ordering. Unknown fields fall back to the configured default; invalid directions return 400.

## Index searching

Enable search with the default placeholder or supply a resource-specific override:

```csharp
resource.Index(index => index.EnableSearch());
// Alternatively:
resource.Index(index => index.EnableSearch(search =>
    search.Placeholder = "Search product names..."));
```

The default placeholder is generated by `ResourceLabelOptions.Index.SearchPlaceholder` as `Search {PluralLabel}...`. Override it globally through `admin.ConfigureResourceLabels(labels => labels.Index(index => index.SearchPlaceholder = resource => $"Find {resource.PluralLabel}"))`, or locally through the search builder. The no-callback `EnableSearch()` overload returns the search builder.

The controller passes trimmed text in `ResourceListRequest.Search`; blank text and disabled search produce null. The data source decides how to match records. Filter before counting and paging so `ResourceListResult.TotalCount` describes all matching records. AdminPlayground demonstrates case-insensitive product-name matching.

The search input updates the grid through HTMX after a 400 ms typing delay and updates browser history. Searching and clearing reset paging while retaining explicitly selected page size and sorting. Paging, sorting, and deletion preserve search. Configured defaults are not added to navigation URLs.

## Index scopes

Enable named filters through the index builder:

```csharp
index.EnableScopes(scopes =>
{
    scopes.Add("all", "All products");
    scopes.Add("under-50", "Under 50");
    scopes.Add("50-and-over", "50 and over");
    scopes.DefaultScope = "all";
});
```

The no-callback overload returns the scopes builder. Identifiers are matched case-insensitively and passed to the data source in `ResourceListRequest.Scope` using their configured spelling. An omitted or unknown identifier uses `DefaultScope`. Without a configured default, it produces null. Disabled scopes also produce null. Scope identifiers must be distinct, and a configured default must identify an existing scope.

The data source decides how to apply each scope alongside search, before counting and paging. The shared builder does not accept filtering expressions. Scopes are navigation filters, not authorization. Data sources must always enforce mandatory access restrictions.

Tabs use HTMX navigation. Selecting a scope resets the page and retains search and explicitly selected sorting and page size. The default tab omits the scope parameter. Search, paging, sorting, and deletion preserve scope selection.

## Index row details

Let index rows expand to show more about a record. Each call replaces earlier row details configuration:

```csharp
// Renders with the index, with the row's resource as the partial's model
index.EnableRowDetails("_ProductSummary");

// Loads when the row first expands, with the model the handler returns for the row's key
index.EnableRowDetails<ProductNotes, ProductNotesHandler>("_ProductNotes", details =>
{
    details.Emphasis = TableRowDetailEmphasis.Rail;
    details.ExpandMode = TableRowDetailExpandMode.Single;
});

// Two or more panels show as tabs
index.EnableRowDetails(panels =>
{
    panels.Add("Summary", "_ProductSummary");
    panels.Add<ProductNotes, ProductNotesHandler>("Notes", "_ProductNotes");
});
```

A handler implements `IResourceRowDetailHandler<TModel>`, whose `FindAsync(id, cancellationToken)` returns the model, or null when the record no longer exists; the row then says so. Unregistered handlers are added as scoped services. Partial names resolve like any partial: put them in `Areas/StellarAdmin/Views/{slug}/` beside the resource's view overrides or in `Shared/`. Details that load on demand need a resource key, and the index throws without one. Links and forms inside the details navigate normally rather than swapping the grid.

The configure callback receives `ResourceRowDetailsBuilder<TResource>`, with `Animate` (default true), `Divider` (the line between a row and its details: default `Inset`, which starts and ends where the content does; `FullWidth` or `None`), `Emphasis` (default `Band`; `Rail` or `None`), `ExpandMode` (default `Multiple`), `Inset` (default `Aligned`), `RowClick` (default false; clicks on links, buttons and form controls keep their own behavior) and `Toggle` (default `Leading`; `Trailing` or `None`). The no-callback overloads return that builder.

`panels.Add(title, partialName)` adds a panel rendered with the index, and `panels.Add<TModel, THandler>(title, partialName)` one loaded through a handler. The title labels the tab. Each returns `ResourceRowDetailPanelBuilder`, whose `Name` identifies the panel and defaults to the title in lower case with dashes (`"Order history"` becomes `order-history`); names must be unique, and registration throws on a duplicate or an empty set. A single panel renders without tabs. With two or more, a first panel that loads on demand loads when the row expands, and the others when their tab is first shown. Each row's tabs work independently and load each panel once.

## Create forms

Configure the create page through `resource.AllowCreate(create => ...)`. `create.Title` and `create.SubmitLabel` are optional setter-only properties, each defaulting to `"Create " + SingularLabel`. Fields start empty. `create.Fields(fields => ...)` configures them with typed selectors. `fields.Add(product => product.Name)` returns a field builder with a setter-only `Title`. The callback overload returns the fields builder. `Clear()` removes earlier fields. Labels and editor templates otherwise come from property metadata and types. Use `AddSection` and `AddGroup` for nested layouts, and see [Form layout](#form-layout) for columns and spans. `AddDisplayField` and `AddDetails` show read-only values; see [Display fields](#display-fields). Global label delegates configured through `admin.ConfigureResourceLabels(...)` supply defaults, and page-level labels override them.

The form model should be a mutable reference type. By default, the controller uses its public parameterless constructor. Use `create.UseFactory(() => new Product(...))` for explicit initialization or models without a parameterless constructor. Field selectors must name direct properties with public getters and setters. The controller constructs a resource for both GET and POST and binds only configured properties, using input names such as `Entity.Name`. ASP.NET Core model validation applies, including data annotations. Invalid submissions redisplay entered values and errors without calling the data source. POST requires an antiforgery token. A successful `CreateAsync` redirects to the index. The data source assigns generated values such as IDs. Write operations return `ResourceOperationResult.Success()`, `NotFound()`, or `ValidationFailed(...)`; a create handler whose model isn't the resource can return `Success(key)` so a lookup can select what it created. Field error names are unprefixed model property names. Use null for summary errors. Errors for fields outside the form also appear in the summary. Rejected writes must not persist changes. Unexpected failures should throw.

The Product playground registers `ProductDataSource` as a singleton with synchronized in-memory storage so created products survive subsequent requests. Restarting the application resets its data. Production sources can retain the default scoped lifetime and persist through their own dependencies.

Override the create view with `Areas/StellarAdmin/Views/Product/Create.cshtml`. The controller uses the same normal MVC lookup as Index, falling back to `ResourceCreate`. The default uses `ResourceFormPageViewModel` and `<sa-form-page model="Model" />`, which renders the existing form editors and antiforgery token.

## Form layout

Create and edit forms lay out their fields on a grid. The form, each section and each group set their own column count, and each field, section and group sets how many of its parent's columns it spans. A section shows a title and uses the section layout. A group has no title or border; use it to give part of a form its own columns or to stack several fields in one cell.

```csharp
edit.Fields(fields =>
{
    fields.Columns(columns => columns.Large(3));

    fields.AddSection("Product", section =>
    {
        section.Layout = FormSectionLayout.Card;
        section.ColumnSpan(span => span.Large(2));
        section.Columns(columns => columns.Small(2));
        section.Add(product => product.Name).ColumnSpanFull();
        section.Add(product => product.Sku);
        section.Add(product => product.CategoryId);
    });

    fields.AddSection("Pricing", section =>
        section.AddGroup(group =>
        {
            group.Columns(2);
            group.Add(product => product.Price);
            group.Add(product => product.Cost);
        })
    );
});
```

- Breakpoints are `Base`, `Small` (30rem), `Medium` (40rem) and `Large` (56rem). They measure the width of the grid itself, not the viewport, so a narrow section or the create sheet keeps fewer columns.
- `Columns(n)` keeps one column below `Medium` and uses `n` from `Medium`. `Columns(columns => columns.Small(2).Large(3))` sets each breakpoint; an unset breakpoint uses the next smaller one. Columns default to 1, and counts run from 1 to 12.
- `ColumnSpan(n)` spans `n` columns at every breakpoint, and `ColumnSpan(span => span.Medium(2))` sets each breakpoint. `ColumnSpanFull()` spans the whole grid, and `BaseFull()`, `SmallFull()`, `MediumFull()` and `LargeFull()` do so at one breakpoint. A span larger than the grid is reduced to its column count. Spans default to 1.
- `ColumnStart(n)` starts an item at column `n` (1 to 12), and `ColumnStart(start => start.Medium(1).Large(3))` sets each breakpoint; `SmallAuto()`, `MediumAuto()` and `LargeAuto()` go back to following the item before. A start of 1 begins a new row. A start is ignored at a breakpoint where the grid has one column or the item would extend past the last column; the span is never reduced for it.
- Items fill the grid in order. The grid doesn't reorder items to fill gaps, so tab order follows the configuration.
- `section.Layout` overrides the form's `SectionLayout` for one section. The create sheet always stacks sections.

## Display fields

Create and edit forms can show read-only values beside their inputs. `AddDisplayField` adds one at the same level as `Add`, in the form, a section or a group, and `AddDetails` adds a titled section that holds only display fields and lays them out as a details list.

```csharp
edit.Fields(fields =>
{
    fields.Add(booking => booking.TravellerName);
    fields.AddDisplayField(booking => booking.Reference);
    fields.AddDisplayField(booking => booking.Total).UseDisplay<NumberDisplay>(number => number.Format = "N2");

    fields.AddDetails("Audit", details =>
    {
        details.Orientation = FieldOrientation.Horizontal;
        details.Variant = DetailsVariant.Divided;
        details.Columns(2);
        details.Add(booking => booking.CreatedAt);
        details.Add(booking => booking.CreatedBy);
        details.Add(booking => booking.UpdatedAt, field => field.EmptyText = "Never");
    });
});
```

- Display fields are never bound: a posted value for one is ignored, and the property needs only a public getter. The value comes from the page's model, so with `AllowCreate<TModel, THandler>` or `AllowEdit<TModel, THandler>` the property must be on `TModel`.
- A display field can read through references, such as `booking => booking.Customer.Name`. An EF Core resource's edit and view pages load the references on each display field's path with the record, in the same query; a null reference shows the empty text. Other data sources and custom models must load them themselves.
- `AddDisplayField` and `details.Add` return a display field builder with setter-only `Title` and `EmptyText`, `UseDisplay<TDisplay>()`, `UseBuiltInRendering()`, and the same `ColumnSpan`, `ColumnSpanFull` and `ColumnStart` methods as a field. The callback overloads return the fields or details builder.
- An empty value shows `EmptyText`, then `[DisplayFormat(NullDisplayText)]`, then a dash, and no display or template runs for it.
- A value renders, in order of precedence, through the display set with `UseDisplay<TDisplay>(display => ...)` (as `UseEditor` does for inputs); through `<sa-display-field>`'s own rendering after `UseBuiltInRendering()`; through the template its `[UIHint("Status")]` names, the view `DisplayTemplates/Status` with the value as its model (a missing template throws); or through the default display for its data type, then its type. Values with no default display, such as collections and GUIDs, use `<sa-display-field>`'s rendering, which joins collections with commas and applies `[DisplayFormat]`.
- Displays are in `StellarAdmin.Resources.Displays`. The built-in displays, and the properties they are the default for:
  - `EmailDisplay`, for `[EmailAddress]`: a `mailto:` link.
  - `LinkDisplay`, for `[Url]`: a link for `http` and `https` addresses, and text otherwise. `OpenInNewTab` (default `true`).
  - `PhoneDisplay`, for `[Phone]`: a `tel:` link that dials the digits.
  - `DateDisplay`, for `DateTime`, `DateTimeOffset`, `DateOnly` and `TimeOnly`: a `<time>` element. `Format`, a .NET format string such as `"d MMM yyyy"`, defaults to `[DisplayFormat]`, then `d MMM yyyy` for dates (including `[DataType(DataType.Date)]`), `HH:mm` for times and `d MMM yyyy, HH:mm` for both.
  - `BooleanDisplay`, for `bool`: `Appearance` (`BooleanDisplayAppearance.Icon`, the default, `Text` or `IconAndText`) shows a check or a muted cross, `TrueText` or `FalseText` (default `"Yes"` and `"No"`), or both; screen readers announce the text with the icon.
  - `EnumDisplay`, for enums other than flags: the member's `[Display]` name.
  - `NumberDisplay`, for numeric types: figures of equal width. `Format`, such as `"N2"` or `"C"`, defaults to `[DisplayFormat]`, then the number as is.
  - `TextDisplay`, for other strings: multiline text (`[DataType(DataType.MultilineText)]`) keeps its line breaks, and `LineClamp` truncates it after that many lines.
  - `BadgeDisplay`, never a default: the value as a badge, and each item of a collection as its own badge. `Variant` (a `BadgeVariant`, default `Secondary`) applies to every value.
- Formats are culture-sensitive and use the request's current culture. To change how a display renders everywhere, add a view at its path, such as `Areas/StellarAdmin/Views/Shared/DisplayTemplates/Displays/Boolean.cshtml`, to the application.

```csharp
details.Add(booking => booking.Cabin).UseDisplay<BadgeDisplay>(badge => badge.Variant = BadgeVariant.Outline);
```

- To choose a badge or other markup by value, write a display template and name it with `[UIHint]`; it applies wherever the property is displayed:

```razor
@* Views/Shared/DisplayTemplates/BookingStatus.cshtml, for [UIHint("BookingStatus")] *@
@using StellarAdmin.UI
@model BookingStatus
@addTagHelper *, StellarAdmin.UI

<sa-badge variant="@(Model switch { BookingStatus.Confirmed => BadgeVariant.Default, BookingStatus.Cancelled => BadgeVariant.Destructive, _ => BadgeVariant.Secondary })">@Model</sa-badge>
```

- A custom display derives from `FieldDisplay`, adds its own settings and names its template through `TemplateName`. The template, `DisplayTemplates/{TemplateName}`, inherits `StellarAdmin.Areas.StellarAdmin.FieldDisplayView<TDisplay>`: `Display` holds the configured settings, the model is the value, and `FormatValue(format)` formats it with a format string, then `[DisplayFormat]`, with enum members by their `[Display]` names. A display that needs request data, such as a record looked up by a key that has no navigation, also implements `IFieldDisplay<THandler>`; the handler derives from `FieldDisplayHandler<TDisplay>`, takes services through its constructor, and returns the data from `PrepareAsync`, which the template reads as `DisplayData`.

- A details section takes `Description`, `Layout`, `ColumnSpan` and `ColumnStart` like `AddSection`, and `Columns`, `Orientation` (`FieldOrientation`) and `Variant` (`DetailsVariant`) from `<sa-details>`.
- Display fields work on create pages too, where a value that isn't set yet shows its empty text.

## View pages

`resource.AllowView(view => ...)` adds a read-only page at `/{slug}/view/{id}` and an eye-icon View action on each index row, before Edit. Every field on it is a display field, laid out with the same sections, groups, columns, spans and starts as a form:

```csharp
resource.UseKey(booking => booking.Id);
resource.AllowView(view =>
{
    view.Title = "Booking";
    view.SectionLayout = FormSectionLayout.Split;
    view.Fields(fields =>
    {
        fields.Columns(2);
        fields.Add(booking => booking.Reference);
        fields.Add(booking => booking.Customer.Name, field => field.Title = "Customer");
        fields.AddSection("Audit", section =>
        {
            section.Orientation = FieldOrientation.Horizontal;
            section.Columns(2);
            section.Add(booking => booking.CreatedAt);
            section.AddGroup(group => group.Add(booking => booking.UpdatedAt, field => field.EmptyText = "Never"));
        });
    });
});
```

- `fields.Add` returns the display field builder described in [Display fields](#display-fields), plus `Orientation`. `AddSection` takes `Description`, `Layout`, `Orientation`, `Columns`, `ColumnSpan` and `ColumnStart`; `AddGroup` takes `Columns`, `ColumnSpan` and `ColumnStart`; `Clear()` removes earlier items. A section's `Orientation` applies to the display fields inside it, including nested sections, unless a field or nested section sets its own.
- The page needs a key (`UseKey`) and at least one field; a view without fields fails at startup. Display handlers run as on the edit page.
- The header offers Back, Delete when `AllowDelete` is configured (a rejected delete returns to the view page with its errors), and Edit when `AllowEdit` is configured.
- The data source loads the record through `IResourceViewHandler<TResource>.FindAsync`, which `IResourceEditHandler<T>` extends, so a source that can edit can also view. `AllowView<TModel, THandler>(...)` displays a custom model, with `THandler : IResourceViewHandler<TModel>`.
- `view.Title` defaults to `"View " + SingularLabel`; change the default with `labels.View(view => view.Title = ...)`. The row action's label is `index.ViewLabel`, defaulting to `labels.Index(index => index.ViewLabel = ...)`, then `"View " + SingularLabel`.
- Override the page with `Areas/StellarAdmin/Views/{Resource}/View.cshtml`, or a shared `ResourceView.cshtml`. Its model is `ResourceViewPageViewModel`, and `<sa-view-page model="Model" />` renders the standard page, with `pre-page-actions`, `post-page-actions`, `pre-view-fields` and `post-view-fields` slots.

## Custom create models

Pair a custom model with its handler at registration:

```csharp
resource.UseDataSource<CustomerDataSource>();
resource.AllowCreate<CreateCustomerModel, CreateCustomerHandler>(create =>
{
    create.Fields(fields =>
    {
        fields.Add(model => model.Name);
        fields.Add(model => model.Email);
        fields.Add(model => model.Password);
        fields.Add(model => model.PasswordConfirmation);
    });
});
resource.AllowEdit(edit => edit.Fields(fields => fields.Add(customer => customer.Name)));
```

`CreateCustomerHandler` implements `IResourceCreateHandler<CreateCustomerModel>` with `Task<ResourceOperationResult> CreateAsync(CreateCustomerModel model, CancellationToken cancellationToken)`. Its constructor can take application services. The handler takes precedence over the data source's create implementation, independent of where `UseDataSource` appears. Handlers default to scoped registration and preserve an existing concrete registration's lifetime. The data source can omit `IResourceCreateHandler<Customer>` entirely while continuing to implement ordinary edit and delete.

`AllowCreate<TModel, THandler>()` returns a `ResourceCreateBuilder<TModel>`. The callback overload returns the resource builder. There is no model-only overload. Fields, nested layouts, factory callbacks, model validation, view overrides, and labels work through the shared controller. Labels derive from the resource, not the form model. Mark password properties with `[DataType(DataType.Password)]` to render password editors, which leave their values empty on redisplay. Use data annotations such as `[Compare(nameof(Password))]` for confirmation validation.

Each create registration starts fresh, including fields, factory, page labels, and section layout. Calling ordinary `AllowCreate(...)` selects the resource model and data source again. A previously obtained typed builder cannot configure a different model after that selection changes.

AdminPlayground includes ordinary Product CRUD and a Customer resource with separate custom create and edit models. Its passwords demonstrate form-only validation and are not stored or used to create authentication accounts.


## Custom edit models

Pair an edit model with a handler independently of the create model:

```csharp
resource.UseKey(customer => customer.Id);
resource.AllowEdit<EditCustomerModel, EditCustomerHandler>(edit =>
{
    edit.Fields(fields =>
    {
        fields.Add(model => model.DisplayName);
        fields.Add(model => model.Email);
    });
});
```

`EditCustomerHandler` implements `IResourceEditHandler<EditCustomerModel>`. Its `FindAsync(string id, CancellationToken cancellationToken)` returns a detached editable model or null for a missing record. Both GET and POST load through this method, so the model needs no parameterless constructor. The POST binds only configured fields and calls `UpdateAsync(string id, EditCustomerModel model, CancellationToken cancellationToken)` after model validation succeeds. The handler maps that model to persistence and returns `ResourceOperationResult`. Report field errors using the edit model's property names. Rejected updates must leave persisted values unchanged.

The route supplies the authoritative ID to both handler methods. Ordinary resource-model edit also excludes the configured resource key property from binding. A custom model's configured properties are its form inputs, not the resource's key mapping. Handlers should use the supplied route ID to identify the record.

The explicit handler takes precedence over any data source edit implementation and is resolved from request services. Its concrete registration defaults to scoped and preserves an existing application registration. The data source can omit `IResourceEditHandler<Customer>`. The no-callback overload returns `ResourceEditBuilder<EditCustomerModel>`, and the callback overload returns the resource builder. Each registration replaces prior edit configuration and handler selection. Ordinary `AllowEdit(...)` returns to the resource model and data source.

Typed fields, layouts, resource label defaults, page overrides, model and handler validation, and antiforgery protection use the shared edit flow. Override the view with `Areas/StellarAdmin/Views/Customer/Edit.cshtml`, or let MVC fall back to `ResourceEdit`.

## Form field editors

Fields without `UseEditor` use MVC metadata-based editor template selection. The admin panel's data-type templates forward to the built-in editors below and apply the same inference, so most properties need no editor configuration. Strings, GUIDs and numbers render a text input, `[DataType(DataType.MultilineText)]` renders a text area, dates and times render the matching date or time input, a `bool` renders a checkbox, a `bool?` renders a Yes, No and Not set select, and a non-flags enum renders a select. Flags enums render a text input.

Select a built-in editor and its settings with `UseEditor<TEditor>()` or `UseEditor<TEditor>(editor => { ... })`. The editors are in `StellarAdmin.Resources.Editors`:

```csharp
using StellarAdmin.Resources.Editors;

edit.Fields(fields =>
{
    fields.Add(trip => trip.Price).UseEditor<TextInputEditor>(input => input.Prefix = "$");
    fields.Add(trip => trip.Notes).UseEditor<TextareaEditor>(textarea => textarea.Rows = 4);
    fields.Add(trip => trip.Cabin).UseEditor<RadioGroupEditor>(radio => radio.Appearance = RadioGroupAppearance.Cards);
    fields.Add(trip => trip.Amenities).UseEditor<CheckboxGroupEditor>();
    fields.Add(trip => trip.IsFeatured).UseEditor<ToggleEditor>();
    fields.Add(trip => trip.Rating).UseEditor<SliderEditor>(slider => slider.Step = 5);
});
```

| Editor | Renders | Settings |
| --- | --- | --- |
| `TextInputEditor` | `sa-input`, or `sa-input-group` with a prefix or suffix | `Type` (`TextInputType.Text`, `Email`, `Tel`, `Url`, `Password`, `Number`), `Placeholder`, `Prefix`, `Suffix`, `Min`, `Max`, `Step` |
| `TextareaEditor` | `sa-textarea` | `Placeholder`, `Rows` |
| `DateInputEditor` | `sa-input type="date"` | `Min`, `Max`, `Step` in days |
| `DateTimeInputEditor` | `sa-input type="datetime-local"` | `Min`, `Max`, `Step` as a `TimeSpan` |
| `TimeInputEditor` | `sa-input type="time"` | `Min`, `Max`, `Step` as a `TimeSpan` |
| `CheckboxEditor` | A single checkbox for a `bool` | None |
| `ToggleEditor` | `sa-switch` for a `bool` | None |
| `SelectEditor` | `sa-select` | `UseItems(...)`, `EmptyChoice`, `EmptyChoiceText` |
| `RadioGroupEditor` | `sa-radio-group` | `UseItems(...)`, `EmptyChoice`, `EmptyChoiceText`, `Appearance` (`RadioGroupAppearance.Default` or `Cards`), `Columns(...)`, `Flow` (`RadioGroupFlow.Across` or `Down`), `ClassNames.Media` |
| `CheckboxGroupEditor` | `sa-checkbox-group` for a collection property | `UseItems(...)`, `Appearance` (`CheckboxGroupAppearance.Default` or `Cards`), `Columns(...)`, `Flow` (`CheckboxGroupFlow.Across` or `Down`), `ClassNames.Media` |
| `SegmentedControlEditor` | `sa-segmented-control` that selects one value | `UseItems(...)`, `EmptyChoice`, `EmptyChoiceText`, `ClassNames.Media` |
| `ToggleGroupEditor` | `sa-toggle-group` that selects multiple values for a collection property and one value otherwise | `UseItems(...)`, `EmptyChoice`, `EmptyChoiceText`, `Appearance` (`ToggleGroupAppearance.Chips`, `Joined` or `Buttons`), `CheckPlacement` (`ToggleGroupCheckPlacement.ReplaceMedia`, `Start` or `End`), `ClassNames.Media` |
| `LookupSheetEditor` | A card or button showing the selection, which opens a sheet to search a long list | `UseItems(...)`, `Editor(...)` (`Layout`, `ShowMedia`, `EmptyText`, `AllowClear`), `Sheet(...)` (`Title`, `SearchPlaceholder`, `MinimumSearchLength`, `PageSize`, `ShowMedia`), `EnableCreate()`, `EnableCreate<TResource>()`, `ClassNames.Media` |
| `SliderEditor` | `sa-slider` for a whole number, with its value beside the label and marks under the track | `Min`, `Max`, `Step`, `ShowValue`, `ValueFormat`, `MarkInterval`, `MarkLabels` (`SliderMarkLabels.None`, `Ends` or `All`), `AddMark(value, label)` |
| `OneTimeCodeEditor` | `sa-input-otp`, one box per digit | `Length` |

Settings left unset are inferred from the property, and explicit settings win. `TextInputEditor` picks its type from `[DataType]` (email address, phone number, URL or password) and renders a number input for numeric properties, with `step="1"` for whole numbers and `step="any"` for `decimal`, `double` and `float`. `SliderEditor` takes `Min` and `Max` from `[Range]`, then falls back to 0 and 100. By default it shows the current value beside the label and labels the minimum and maximum under the track; `ValueFormat` such as `"{0} km"` formats both, `MarkInterval` adds ticks, `MarkLabels = SliderMarkLabels.None` removes the end labels, and `AddMark` replaces the generated marks with your own. `OneTimeCodeEditor` takes `Length` from the property's maximum length, then falls back to 6. A `DateTimeOffset` property renders a text input with the round-trip format, so its offset is kept.

The choice editors (`SelectEditor`, `RadioGroupEditor`, `CheckboxGroupEditor`, `SegmentedControlEditor` and `ToggleGroupEditor`) take their choices from `UseItems(...)` when it is called. Otherwise a non-flags enum property supplies its members, using `[Display(Name, Description, GroupName)]` for the choice text, description and group, and a Boolean property supplies Yes and No. A collection property takes its choices from its element type. Any other property type requires `UseItems(...)`. Each choice is a `ChoiceItem` with a `Value` and `Text`, and optionally a `Description`, `Disabled`, a `Group` (`ChoiceGroup` with `Text`, `Description` and `Disabled`) and `Media`; choices with equal groups share one group. Each editor displays the fields it supports and ignores the rest: descriptions appear in radio and checkbox groups, groups only in `SelectEditor`, as option groups that gather each group's choices at the position of its first choice, and media in every editor except `SelectEditor`.

The editor, not the source of its choices, adds an empty first choice that clears the value, with the text `EmptyChoiceText` ("Not set" by default). `EmptyChoice` decides when. `EmptyChoice.Auto`, the default, adds it for an optional value, such as an `int?`, a nullable enum or `bool?`, or a `string?`, without `[Required]`, and posting it binds null. A required value gets none, except that `SelectEditor` shows it while the value is unset, so that no other choice appears selected. `EmptyChoice.Include` always adds it and `EmptyChoice.Omit` never does. A collection property never gets one, and supplied choices that already have an empty value keep their own. The empty choice is selected while the value is null. Every editor also has `ClassNames` for additional CSS classes on its parts. Override a built-in editor's markup at `Areas/StellarAdmin/Views/Shared/EditorTemplates/Editors/<Name>.cshtml`, for example `Editors/TextInput.cshtml`.

`Media` is an `ItemMedia`: `new ItemMedia.Icon("plane")` for a registered icon in the text color, `new ItemMedia.Avatar(url)` for a round avatar that shows the text's initials when the URL is null, `new ItemMedia.Image(url)` for a square image with rounded corners, or `new ItemMedia.Code("LIS")` for a short monospace code. The media sits before the text, and leads a card at a larger size in the `Cards` appearances. A choice without media keeps no space for it. `ClassNames.Media` adds classes to every choice's media; a size utility such as `size-10` replaces the default size:

```csharp
fields.Add(trip => trip.GuideId).UseEditor<RadioGroupEditor>(guides =>
{
    guides.Appearance = RadioGroupAppearance.Cards;
    guides.UseItems([
        new ChoiceItem("ana", "Ana Ribeiro") { Description = "Lisbon walking tours.", Media = new ItemMedia.Avatar("/images/ana.jpg") },
        new ChoiceItem("lena", "Lena Fischer") { Media = new ItemMedia.Avatar(null) },
    ]);
});
```

`RadioGroupEditor` and `CheckboxGroupEditor` arrange their choices in columns with `Columns(n)` or `Columns(columns => columns.Small(2).Large(3))`. The breakpoints are the form's, but measured against the field's width, so a group in a narrow field stays in one column. The choices fill each row in turn; `Flow = CheckboxGroupFlow.Down` (or `RadioGroupFlow.Down`) fills each column before the next instead:

```csharp
fields.Add(user => user.RoleIds).UseEditor<CheckboxGroupEditor>(roles =>
{
    roles.UseItems<ApplicationDbContext, ApplicationRole, string>(role => role.Id, role => role.Name!);
    roles.Appearance = CheckboxGroupAppearance.Cards;
    roles.Columns(2);
});
```

`ToggleGroupEditor` suits short sets whose labels fit on a line. `Chips`, the default, shows rounded chips with a check mark when on, and `Buttons` shows separate toggles; both wrap onto more lines. `Joined` connects the toggles into one group that doesn't wrap, so keep it to a few short choices such as weekdays. A chip's check mark replaces its media while on, so the chip keeps its width; `CheckPlacement = ToggleGroupCheckPlacement.Start` shows it before the media instead, and `End` after the text.

`LookupSheetEditor` selects one value from a list too long for a select. The form posts a hidden value, and the selected item is resolved when the form renders. `lookup.Editor(...)` configures the field in the form and `lookup.Sheet(...)` the search sheet. The sheet searches as the user types, supports the arrow keys and Enter, marks the current selection, and loads `PageSize` items at a time (20 by default); with `MinimumSearchLength` above 0, it asks for that many characters before searching. A clear button appears when the editor's `AllowClear` is true or, by default, when the field is optional. While nothing is selected, the field shows a "Choose {field}" button, or `EmptyText`. Supply the items from a source registered in DI that implements `ILookupSource<TEntity, TValue>`, with selectors for each item's value and title:

```csharp
builder.Services.AddScoped<AirportLookupSource>();

fields.Add(trip => trip.DepartureAirport).UseEditor<LookupSheetEditor>(lookup =>
{
    lookup.Sheet(sheet => sheet.Title = "Select a departure airport");
    lookup.UseItems<AirportLookupSource, Airport, string>(
        airport => airport.Code,
        airport => airport.City,
        items => items.UseDescription(airport => airport.Country));
});

public sealed class AirportLookupSource(AppDbContext db) : ILookupSource<Airport, string>
{
    // Return the airports with these codes; leave out codes that don't exist
    public Task<IReadOnlyCollection<Airport>> FindAsync(IReadOnlyCollection<string> values, CancellationToken cancellationToken) => ...;

    // Return query.Take items after skipping query.Skip, filtered by query.Term (null when empty)
    public Task<LookupPage<Airport>> SearchAsync(LookupQuery query, CancellationToken cancellationToken) => ...;
}
```

`UseDescription` adds secondary text below each item's title. `UseCode(...)` displays a short code chip beside the title, `UseAvatar(...)` an avatar from an image URL, with the title's initials when the URL is null, `UseImage(...)` a square image, and `UseIcon(...)` a registered icon; a null or empty code, image URL or icon name leaves the item without media, and the last one called wins. `ClassNames.Media` adds classes to the media of the selection and the results. Custom `LookupItems` return each item as a `ChoiceItem`, with its value formatted in the current culture, its title as `Text`, and `Media`; lookups ignore `Disabled` and `Group`. Their `FindAsync` returns the selected items in the order of the field's values, and an empty list when nothing is selected. Their `MediaType` is the `ItemMedia` type the items display, such as `typeof(ItemMedia.Code)`, which shapes the loading rows. The editor's `Layout` is `LookupSheetEditorLayout.Card` when the items have a description and `Input` otherwise; a card shows the description, an input does not. `ShowMedia = false` on `Editor(...)` or `Sheet(...)` hides the media in the form or the results. A value the source cannot find is displayed as the value itself. The lookup's fixed text, such as "Choose {field}", "New", "None", "Load more" and the sheet's messages, comes from the callbacks on `labels.Lookup(...)` inside `admin.ConfigureResourceLabels(...)`, for example `labels.Lookup(lookup => lookup.ChooseLabel = context => $"Select {context.FieldLabel}")`; each receives a `LookupLabelContext` with `FieldLabel`, `MinimumSearchLength` and `Term`. `EmptyText`, `Title` and `SearchPlaceholder` still override them per field.

`EnableCreate()` adds a New button beside Choose while nothing is selected. It opens, in a sheet, the create form of the resource registered for the items' type (the `TEntity` of `UseItems`); `EnableCreate<TResource>()` names the resource instead, which custom `LookupItems` that don't override `ItemType` require. Saving closes the sheet and selects the new item, so the lookup's value must be that resource's key. The key comes from the resource's key selector when the create form uses the resource type, as EF Core resources do, or from `ResourceOperationResult.Success(key)` returned by a create handler with its own model; without a key, the sheet closes and nothing is selected. Rendering the form throws when no resource is registered for the type or the resource has no create form, and users the resource's authorization rejects don't see the button. A rejected save shows its errors in the sheet, and lookups inside that form open a second sheet over it, without a New button. The message shown when the form can't be posted comes from `labels.Sheet(sheet => sheet.SaveErrorDescription = "...")`.

For a custom form editor, create an editor class derived from `FieldEditor` (in `StellarAdmin.Resources.Editors`) that implements `IFieldEditor<MyEditorHandler>`. The editor holds the settings. Create a handler that implements `IFieldEditorHandler<MyEditor>`, or derives from `FieldEditorHandler<MyEditor>` when it loads no request data, and accept `MyEditor` plus any services it needs in its constructor. Select it with `fields.Add(model => model.Property).UseEditor<MyEditor>(editor => { /* settings */ });`. The handler's `TemplateName` selects an MVC editor template in `Views/Shared/EditorTemplates`. Its `PrepareAsync` can load request data. The template reads the configured editor from `FormFieldProperties.Editor` and request data from `FormFieldProperties.EditorData` in `ViewData[ViewDataKeys.FormFieldProperties]`.

## EF Core resources

Reference `StellarAdmin.EntityFrameworkCore` and import its namespace to register an EF entity through the shared resource controller and views:

```csharp
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources.Editors;

Action<SelectEditor> categoryItems = editor =>
    editor.UseItems<AppDbContext, Category, int>(
        category => category.Id,
        category => category.Name,
        items => items.OrderBy(category => category.Name));

admin.AddEfCoreResource<AppDbContext, Product>(resource =>
{
    resource.Index(index =>
    {
        index.Columns(columns =>
        {
            columns.Add(product => product.Name, column => column.Sortable(product => product.Name.ToLower()));
            columns.Add(product => product.Price, column => column.Sortable());
            columns.Add(product => product.CategoryId, column => column.Sortable());
        });
        index.DefaultSortBy(product => product.Name);
        index.EnableSearch(
            term => product => product.Name.Contains(term),
            search => search.Placeholder = "Search products..."
        );
        index.EnableScopes(scopes =>
        {
            scopes.Add("all", "All products");
            scopes.Add("under-50", "Under 50", product => product.Price < 50);
            scopes.Add("50-and-over", "50 and over", product => product.Price >= 50);
            scopes.DefaultScope = "all";
        });
        index.EnablePaging();
    });

    resource.AllowCreate(create => create.Fields(fields =>
    {
        fields.Add(product => product.Name);
        fields.Add(product => product.Price);
        fields.Add(product => product.CategoryId).UseEditor<SelectEditor>(categoryItems);
    }));
    resource.AllowEdit(edit => edit.Fields(fields =>
    {
        fields.Add(product => product.Name);
        fields.Add(product => product.Price);
        fields.Add(product => product.CategoryId).UseEditor<SelectEditor>(categoryItems);
    }));
    resource.AllowDelete();
});
```

Register AppDbContext with the application's chosen EF provider before serving resource requests. The EF builder selects its own data source and discovers the key from EF metadata, so it has no UseDataSource or UseKey methods. Supported keys are single public CLR properties of type int, long, Guid, or string. Labels and index titles use the shared defaults and overrides. Queries honor EF global query filters, count before paging, and append primary-key ordering to keep page results stable. Search predicates and sort selectors must translate through the chosen provider. Search filters the query before counting and paging. The predicate factory is required by `EnableSearch`. Its no-callback overload returns the shared search builder for placeholder configuration. Repeated calls replace the search configuration.

EF scopes accept an optional predicate on each entry. Omitting it leaves that scope unfiltered while retaining EF global query filters. The selected predicate combines with search before counting and paging. Scope predicates must translate through the chosen provider. The no-callback `EnableScopes()` overload returns the EF scopes builder. Repeated calls replace the entries and default selection. Scope identifiers, default selection, tabs, and navigation follow the shared scope behavior described above.

EF row details can also project the row's entity when they load, in one untracked query that honors global query filters: `index.EnableRowDetails("_ProductPricing", product => new ProductPricing(product.Name, product.Price))`, or `panels.Add("Pricing", "_ProductPricing", product => new ProductPricing(...))` inside `EnableRowDetails(panels => ...)`, where the EF index builder supplies `EfCoreRowDetailPanelsBuilder<TContext, TEntity>`. A missing or filtered entity shows that the record no longer exists. The projection must translate through the chosen provider. AdminPlayground's Products index combines a projected Pricing panel, an inline Overview panel and a handler-loaded Related panel.

Use `column.Sortable()` to enable ordering by the column's field, or `column.Sortable(selector)` to use a different ordering expression. The shared column builder stores the selector, and the data source decides how to apply it. EF translates it through the chosen provider. Both default and requested sorting use the override, with primary-key ordering breaking ties. Repeated calls replace the sorting configuration. Calling `Sortable()` after an override restores field ordering.

`UseEditor<SelectEditor>` selects the Razor select editor template for a form field. `UseItems(IEnumerable<ChoiceItem>)` takes a snapshot of fixed choices when configured, and `UseItems(IEnumerable<SelectListItem>)` converts existing select list items, posting an item's text when it has no value and ignoring its selected state, since the field's value decides the selection. `UseItems<TProvider>()` resolves a registered `IChoiceItemsProvider`, while `UseItems((services, cancellationToken) => ...)` accepts a request-aware asynchronous loader directly; both return `ChoiceItem`s. The EF Core `UseItems<TContext, TEntity, TValue>` extension works on every choice editor, requires value and text expressions, resolves the registered DbContext for each form request, and projects only the selected values without loading full entities. Values are formatted in the current culture, which posted values bind in. The configure callback receives `EfCoreChoiceItemsBuilder<TEntity, TValue>` with `OrderBy`, `UseDescription`, `UseGroup` (a null heading leaves the choice ungrouped), and `UseIcon`, `UseAvatar`, `UseImage` or `UseCode` for media, projected with the choices; as on the lookup builder, a null or empty value leaves a choice without media, except an avatar, which shows initials. The editor adds the empty choice for an optional reference such as `int? CategoryId`; set its `EmptyChoiceText` to relabel it. These choices work with entity fields and custom create/edit models, including rejected submissions. An index column configured for `CategoryId` displays and sorts by the foreign-key value. The database enforces foreign-key constraints when saving, and database errors propagate through the EF data source.

The EF Core package also adds `UseItems<TContext, TEntity, TValue>` to `LookupSheetEditor`, which needs no lookup source:

```csharp
fields.Add(product => product.CategoryId).UseEditor<LookupSheetEditor>(lookup =>
    lookup.UseItems<AppDbContext, Category, int>(
        category => category.Id,
        category => category.Name,
        items => items
            .SearchOn(category => category.Name, category => category.Code)
            .UseDescription(category => category.Description)
            .UseCode(category => category.Code)
            .OrderBy(category => category.Name)));
```

Search matches any `SearchOn` expression (the title by default) ignoring case, orders by `OrderBy` (the title by default) and then the value, and projects the value, title, description and media in SQL. The builder's media methods are `UseCode`, `UseAvatar`, `UseImage` and `UseIcon`. On an EF Core resource's edit page, the selected item comes from the reference navigation loaded with the entity in the same query. The navigation is inferred from a single-property foreign key on the field's property; set it with `ReferenceFrom<TModel>(model => model.Category)` otherwise. Create pages, custom edit models and fields without a navigation query the selected item by value.

The EF data source implements the shared create, edit, and delete handler interfaces. Actions remain disabled until `AllowCreate`, `AllowEdit`, or `AllowDelete` is called. The shared controller handles form binding, validation, antiforgery, and redirects. The EF source creates entities from the configured form model or factory. Edit forms load an untracked entity; saving reloads the tracked entity and copies only configured editable fields. EF form fields must be mapped scalar properties that are not keys, generated values, or concurrency tokens. A custom create or edit model still requires a custom handler through the shared generic `AllowCreate<TModel, THandler>` or `AllowEdit<TModel, THandler>` overload. Select item queries honor EF query filters or a custom provider's own data access rules. Missing records produce not-found results. A database concurrency conflict during save produces a general validation error when the record still exists, or not-found when it disappeared. This does not detect edits made between displaying a form and submitting it; that requires an application-specific handler or a future concurrency workflow. Other database exceptions propagate so applications can translate known failures through their own handlers or a future callback API.

Query transformations, event callbacks, and richer editor behaviors remain deferred. AdminPlayground demonstrates CRUD and a Product–Category lookup that creates categories, using its shared `ApplicationDbContext` and `app.db`. Product prices are mapped to integer cents so sorting executes in SQLite.
