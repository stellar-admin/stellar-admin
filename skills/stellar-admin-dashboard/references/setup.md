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

Call `app.MapStellarAdmin()` to map Dashboard routes. The example's Product index is available at `/stellaradmin/Product`. The route uses the resource type name, independently of its display labels. Register a data source and columns for each resource whose index you want to display. Register `.AllowCreate(...)` to enable `/stellaradmin/Product/Create`. Register `.AllowEdit(...)` and configure a key with `resource.UseKey(product => product.Id)` to enable `/stellaradmin/Product/Edit/{id}`. Register `.AllowDelete()` or `.AllowDelete(...)` to enable deletion through an antiforgery-protected POST and an index confirmation dialog; deletion also requires a key. Automatic sidebar entries remain deferred.

`ProductDataSource` implements `IResourceCrudDataSource<Product>` from `StellarAdmin.Dashboard.Resources` for all CRUD operations. The combined interface includes `IResourceDataSource<Product>` (`ListAsync`), `IResourceCreateHandler<Product>` (`CreateAsync`), `IResourceEditHandler<Product>` (`FindAsync` and `UpdateAsync`), and `IResourceDeleteHandler<Product>` (`DeleteAsync`). A source can instead implement just the individual interfaces it supports. The shared controller forwards the request cancellation token. Unregistered actions have no buttons and direct requests are rejected, even when the data source implements the corresponding handler. Configuring a form or deletion without a matching implementation produces an options validation error. `UseDataSource<TDataSource>()` registers the concrete source as scoped unless the application already registered it. Resolve its dependencies through its constructor. An existing concrete registration's lifetime is preserved. Repeated `UseDataSource` calls select the last source.

Index columns start empty. `Add` appends and `Clear` removes the configured columns. `Add(selector)` returns the column builder. `Add(selector, configure)` returns the columns builder. Column `Title` and `Format` are setter-only. Omitted titles use the selected property's display metadata/name. Omitted formats use its display metadata. Set `index.Title` to override the page title, which otherwise uses the current resource plural label. Empty results display an empty state. This first index implementation does not perform paging, sorting, or searching.

To customize a resource's index, add `Areas/StellarAdmin/Views/Product/Index.cshtml` to the application. The controller first searches for `Index` using normal MVC view lookup, including shared locations and configured view-location expanders, then falls back to `ResourceIndex` when no view is found. The built-in fallback lives in `Areas/StellarAdmin/Views/Shared/ResourceIndex.cshtml` and can also be overridden by the application. A custom view can use `ResourceIndexPageViewModel<Product>` from `StellarAdmin.Dashboard.Areas.StellarAdmin.ViewModels` and render `<sa-index-page model="Model" />` or supply its own markup.

The singular label defaults to the readable type name (`ProductCategory` becomes `Product category`). The plural defaults to its English plural (`Product categories`). Setting only `SingularLabel` also changes the inferred plural. An explicit `PluralLabel` takes precedence regardless of assignment order. Labels must be nonblank. Set both labels for other languages or domain-specific wording. Naming uses [Humanizer](https://github.com/Humanizr/Humanizer).

Resolve configuration through `IOptions<ResourceOptions<Product>>` (`Microsoft.Extensions.Options` and `StellarAdmin.Dashboard.Resources.Options`). The builder callback executes during registration. Its setter-only properties register configuration actions, which execute when options resolve. Each resource type and service provider gets its own options instance. Repeated `AddResource` registrations compose in assignment order, and later assignments to the same property win. A no-argument `AddResource` registration preserves existing configuration. Each `AllowCreate`, `AllowEdit`, or `AllowDelete` call replaces that action’s previous configuration; setters and field calls on the returned builder compose. Standard Configure, PostConfigure, and options validation remain available. This allows an integration to register defaults first and application code to override them afterwards.

The no-callback overload returns the resource builder. The callback overload returns the Dashboard builder:

```csharp
var resource = dashboard.AddResource<Product>();
resource.SingularLabel = "Item";
resource.PluralLabel = "Inventory";
```

## Create forms

Configure the create page through `resource.AllowCreate(create => ...)`. `create.Title` and `create.SubmitLabel` are optional setter-only properties, each defaulting to `"Create " + SingularLabel`. Fields start empty. `create.Fields(fields => ...)` configures them with typed selectors. `fields.Add(product => product.Name)` returns a field builder with a setter-only `Title`. The callback overload returns the fields builder. `Clear()` removes earlier fields. Labels and editor templates otherwise come from property metadata and types. Use `AddSection`, `AddGroup`, and `AddRow` for nested layouts. Global label delegates configured through `dashboard.ConfigureResourceLabels(...)` supply defaults, and page-level labels override them.

The form model should be a mutable reference type. By default, the controller uses its public parameterless constructor. Use `create.UseFactory(() => new Product(...))` for explicit initialization or models without a parameterless constructor. Field selectors must name direct properties with public getters and setters. The controller constructs a resource for both GET and POST and binds only configured properties, using input names such as `Entity.Name`. ASP.NET Core model validation applies, including data annotations. Invalid submissions redisplay entered values and errors without calling the data source. POST requires an antiforgery token. A successful `CreateAsync` redirects to the index. The data source assigns generated values such as IDs. Write operations return `ResourceOperationResult.Success()`, `NotFound()`, or `ValidationFailed(...)`. Field error names are unprefixed model property names. Use null for summary errors. Errors for fields outside the form also appear in the summary. Rejected writes must not persist changes. Unexpected failures should throw.

The Product playground registers `ProductDataSource` as a singleton with synchronized in-memory storage so created products survive subsequent requests. Restarting the application resets its data. Production sources can retain the default scoped lifetime and persist through their own dependencies.

Override the create view with `Areas/StellarAdmin/Views/Product/Create.cshtml`. The controller uses the same normal MVC lookup as Index, falling back to `ResourceCreate`. The default uses `ResourceFormPageViewModel` and `<sa-form-page model="Model" />`, which renders the existing form editors and antiforgery token.

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

DashboardPlayground includes ordinary Product CRUD and a Customer resource with separate custom create and edit models. Its passwords demonstrate form-only validation and are not stored or used to create authentication accounts.


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
