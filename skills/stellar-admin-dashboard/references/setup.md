# Dashboard setup notes

These preserved notes support the generated component references; full Dashboard guidance is still planned.

## Resource packages

The resource page helpers live in `StellarAdmin.Dashboard`, which is MIT licensed alongside the rest of StellarAdmin. These packages are currently available from the source repository; NuGet publication is a separate release step. Keep `StellarAdmin.TagHelpers` registered, reference the `StellarAdmin.Dashboard` project, add `using StellarAdmin.Dashboard;` in startup, and call `.AddDashboard()` on the `StellarAdminBuilder`. In `_ViewImports.cshtml`, add `@using StellarAdmin.Dashboard.TagHelpers` and `@addTagHelper *, StellarAdmin.Dashboard`.

## Resource registration

Register a resource through the Dashboard builder:

```csharp
using StellarAdmin.Dashboard;

builder.Services.AddStellarAdmin().AddDashboard(dashboard =>
{
    dashboard.AddResource<Product>(resource =>
    {
        resource.UseDataSource<ProductDataSource>();
        resource.Create(create => create.Fields(fields =>
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
                column.Format = "{0:0.00}";
            });
        }));
    });
    dashboard.AddResource<ProductCategory>(resource =>
    {
        resource.SingularLabel = "Category";
        resource.PluralLabel = "Catalog";
    });
});
```

Call `app.MapStellarAdmin()` to map Dashboard routes. The example's Product index is available at `/stellaradmin/Product`. The route uses the resource type name, independently of its display labels. Register a data source and columns for each resource whose index you want to display. The create form is available at `/stellaradmin/Product/Create`. Automatic sidebar entries, edit, and delete are not implemented yet.

`ProductDataSource` implements `IResourceDataSource<Product>` from `StellarAdmin.Dashboard.Resources`. It implements `Task<IReadOnlyList<Product>> ListAsync(CancellationToken cancellationToken)` and `Task CreateAsync(Product resource, CancellationToken cancellationToken)`. The shared controller forwards the request cancellation token and renders the returned records. `UseDataSource<TDataSource>()` registers the concrete source as scoped unless the application already registered it. Resolve its dependencies through its constructor. An existing concrete registration's lifetime is preserved. Repeated `UseDataSource` calls select the last source.

Index columns start empty. `Add` appends and `Clear` removes the configured columns. `Add(selector)` returns the column builder. `Add(selector, configure)` returns the columns builder. Column `Title` and `Format` are setter-only. Omitted titles use the selected property's display metadata/name. Omitted formats use its display metadata. Set `index.Title` to override the page title, which otherwise uses the current resource plural label. Empty results display an empty state. This first index implementation does not perform paging, sorting, or searching.

To customize a resource's index, add `Areas/StellarAdmin/Views/Product/Index.cshtml` to the application. The controller first searches for `Index` using normal MVC view lookup, including shared locations and configured view-location expanders, then falls back to `ResourceIndex` when no view is found. The built-in fallback lives in `Areas/StellarAdmin/Views/Shared/ResourceIndex.cshtml` and can also be overridden by the application. A custom view can use `ResourceIndexPageViewModel<Product>` from `StellarAdmin.Dashboard.Areas.StellarAdmin.ViewModels` and render `<sa-index-page model="Model" />` or supply its own markup.

The singular label defaults to the readable type name (`ProductCategory` becomes `Product category`). The plural defaults to its English plural (`Product categories`). Setting only `SingularLabel` also changes the inferred plural. An explicit `PluralLabel` takes precedence regardless of assignment order. Labels must be nonblank. Set both labels for other languages or domain-specific wording. Naming uses [Humanizer](https://github.com/Humanizr/Humanizer).

Resolve configuration through `IOptions<ResourceOptions<Product>>` (`Microsoft.Extensions.Options` and `StellarAdmin.Dashboard.Resources.Options`). The builder callback executes during registration. Its setter-only properties register configuration actions, which execute when options resolve. Each resource type and service provider gets its own options instance. Repeated registrations compose in assignment order, and later assignments to the same property win. A no-argument registration preserves existing configuration. Standard Configure, PostConfigure, and options validation remain available. This allows an integration to register defaults first and application code to override them afterwards.

The no-callback overload returns the resource builder. The callback overload returns the Dashboard builder:

```csharp
var resource = dashboard.AddResource<Product>();
resource.SingularLabel = "Item";
resource.PluralLabel = "Inventory";
```

## Create forms

Configure the create page through `resource.Create(create => ...)`. `create.Title` and `create.SubmitLabel` are optional setter-only properties, each defaulting to `"Create " + SingularLabel`. Fields start empty. `create.Fields(fields => ...)` configures them with typed selectors. `fields.Add(product => product.Name)` returns a field builder with a setter-only `Title`. The callback overload returns the fields builder. `Clear()` removes earlier fields. Labels and editor templates otherwise come from property metadata and types. Sections, groups, and rows are the next planned increment.

For this increment, the resource is a mutable reference type with a public parameterless constructor. Field selectors must name direct properties with public getters and setters. The controller constructs a resource for both GET and POST and binds only configured properties, using input names such as `Entity.Name`. ASP.NET Core model validation applies, including data annotations. Invalid submissions redisplay entered values and errors without calling the data source. POST requires an antiforgery token. A successful `CreateAsync` redirects to the index. The data source assigns generated values such as IDs. Action-specific form models and reporting domain validation failures are deferred.

The Product playground registers `ProductDataSource` as a singleton with synchronized in-memory storage so created products survive subsequent requests. Restarting the application resets its data. Production sources can retain the default scoped lifetime and persist through their own dependencies.

Override the create view with `Areas/StellarAdmin/Views/Product/Create.cshtml`. The controller uses the same normal MVC lookup as Index, falling back to `ResourceCreate`. The default uses `ResourceFormPageViewModel` and `<sa-form-page model="Model" />`, which renders the existing form editors and antiforgery token.
