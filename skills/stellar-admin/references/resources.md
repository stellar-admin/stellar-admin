# Resources

A resource is one entity type exposed in the admin panel, with its own index page and, when enabled, create, edit, view and delete pages.

## Contents

- [EF Core resources](#ef-core-resources)
- [Slugs and labels](#slugs-and-labels)
- [Enabling pages](#enabling-pages)
- [Delete](#delete)
- [Custom data sources](#custom-data-sources)
- [Operation results](#operation-results)
- [How configuration composes](#how-configuration-composes)

## EF Core resources

Use `AddEfCoreResource<TContext, TEntity>` from `StellarAdmin.EntityFrameworkCore` for any entity in a registered DbContext. The context comes first.

```csharp
using StellarAdmin.EntityFrameworkCore;

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
```

- The key comes from EF metadata: a single public `int`, `long`, `Guid` or `string` property. EF resources have no `UseKey` or `UseDataSource`.
- Queries honour global query filters, count before paging and add primary-key ordering so pages are stable.
- References on a column's or display field's path (`booking => booking.Customer!.Name`) are loaded in the same query; no `Include` is needed.
- Create builds the entity with its parameterless constructor or `create.UseFactory(...)`, binds the configured fields and saves. Edit loads the entity, copies only the configured editable fields onto the tracked entity and saves.
- Form fields must be mapped scalar properties that are not keys, generated values or concurrency tokens, or many-to-many navigations whose join table has no entity type ([relationships](relationships.md#many-to-many-with-multilookupsheeteditor)). Other collections and navigations need a custom model and handler ([forms](forms.md#custom-create-and-edit-models)).
- A concurrency conflict on save shows a general error (or not found if the record was deleted). Other database errors, such as a foreign-key violation, propagate as exceptions.
- Search and scope predicates and sort selectors must translate through the provider.

## Slugs and labels

| Setting | Default | Change it with |
| --- | --- | --- |
| Slug (URL segment) | Kebab-case plural of the type name: `OrderItem` → `order-items` | `AddEfCoreResource<VoyagerDbContext, Booking>("reservations", resource => ...)` or `AddResource<T>("slug", ...)` |
| Singular label | Humanized type name: `OrderItem` → `Order item` | `resource.SingularLabel = "Reservation";` |
| Plural label | English plural of the singular label | `resource.PluralLabel = "Reservations";` |
| Sidebar label | Plural label | `resource.SidebarItem(item => item.Label = "...")` |

Slugs are lowercase letters and digits separated by single hyphens and must be unique. Setting only `SingularLabel` also changes the inferred plural; an explicit `PluralLabel` always wins. Page titles and button labels ("Create booking", "Edit booking") derive from the labels; change their patterns for every resource with `ConfigureResourceLabels` ([customization](customization.md#global-labels)).

## Enabling pages

Every page except the index is off until enabled. Each call takes its own callback and returns the resource builder, so call them as separate statements:

| Call | Adds | Needs |
| --- | --- | --- |
| `resource.Index(index => ...)` | Columns, search, scopes, sorting, paging ([index-pages](index-pages.md)) | Columns, or the grid is empty |
| `resource.AllowCreate(create => ...)` | `/{slug}/create` and a Create button | Fields ([forms](forms.md)) |
| `resource.AllowEdit(edit => ...)` | `/{slug}/edit/{id}` and an Edit row action | Fields and a key |
| `resource.AllowView(view => ...)` | `/{slug}/view/{id}` and a View row action | At least one field and a key ([view-pages](view-pages.md)) |
| `resource.AllowDelete()` | Delete on index rows, the edit page and the view page, with confirmation | A key |

The overloads without a callback (`AllowCreate()`, `AllowEdit()`) return the page builder instead, for configuration spread over several statements:

```csharp
var create = resource.AllowCreate();
create.Title = "New booking";
create.Fields(fields => fields.Add(booking => booking.Destination));
```

Calling an `Allow*` method again replaces that page's configuration.

## Delete

```csharp
resource.AllowDelete(delete =>
{
    delete.Title = "Delete booking?";
    delete.Message = "The booking and its payment history will be removed.";
    delete.ConfirmLabel = "Delete booking";
    delete.CancelLabel = "Keep";
});
```

Delete posts with an antiforgery token after a confirmation dialog. A rejected delete (validation errors from the data source) redisplays the page it came from with the errors. There is no delete hook; to block or cascade, use a custom data source or a database constraint.

## Custom data sources

For entities outside EF Core, register with `AddResource<T>` and supply a data source and a key:

```csharp
using StellarAdmin.Resources;

admin.AddResource<Supplier>(resource =>
{
    resource.UseDataSource<SupplierDataSource>();
    resource.UseKey(supplier => supplier.Code);
    resource.Index(index =>
    {
        index.Columns(columns => columns.Add(supplier => supplier.Name, column => column.Sortable()));
        index.EnableSearch();
        index.EnablePaging();
    });
    resource.AllowCreate(create => create.Fields(fields => fields.Add(supplier => supplier.Name)));
    resource.AllowEdit(edit => edit.Fields(fields => fields.Add(supplier => supplier.Name)));
    resource.AllowDelete();
});
```

The data source implements `IResourceCrudDataSource<T>`, or only the interfaces for the pages it supports:

| Interface | Method | Used by |
| --- | --- | --- |
| `IResourceDataSource<T>` | `Task<ResourceListResult<T>> ListAsync(ResourceListRequest request, CancellationToken cancellationToken)` | Index |
| `IResourceCreateHandler<T>` | `Task<ResourceOperationResult> CreateAsync(T model, CancellationToken cancellationToken)` | Create |
| `IResourceViewHandler<T>` | `Task<T?> FindAsync(string id, CancellationToken cancellationToken)` | View |
| `IResourceEditHandler<T>` (extends view) | `FindAsync`, and `Task<ResourceOperationResult> UpdateAsync(string id, T model, CancellationToken cancellationToken)` | Edit, and View |
| `IResourceDeleteHandler<T>` | `Task<ResourceOperationResult> DeleteAsync(string id, CancellationToken cancellationToken)` | Delete |

`ListAsync` receives a `ResourceListRequest` with `Search` (trimmed text or null), `Scope` (the selected scope id or null), `Sort` (`Field` and `Direction`, or null) and `Paging` (`Page`, `PageSize`, or null). Apply search, scope and sort before paging, and return `new ResourceListResult<T>(items, totalCount)` where `totalCount` counts all matching records. The panel does not sort or filter returned rows itself. With inline rows, `Keys` is set after a save to reload only those records ([inline editing](inline-editing.md#custom-data-sources)).

`UseDataSource<TDataSource>()` registers the source as scoped unless the app already registered it, so it can take services such as a DbContext or an HTTP client in its constructor. Configuring a page whose interface the source doesn't implement fails options validation.

## Operation results

Write methods return a `ResourceOperationResult`:

| Result | Meaning |
| --- | --- |
| `ResourceOperationResult.Success()` | Saved; redirects to the index. |
| `ResourceOperationResult.Success(key)` | Saved, reporting the new record's key; lets a lookup select a record created from it ([relationships](relationships.md#creating-from-a-lookup)). |
| `ResourceOperationResult.NotFound()` | The record no longer exists. |
| `ResourceOperationResult.ValidationFailed(nameof(Booking.Reference), "Reference is already in use.")` | Redisplays the form with the error on that field; pass `null` as the field for a summary error. |

Field names are unprefixed model property names. Rejected writes must not persist anything. Unexpected failures should throw.

## How configuration composes

Builder property setters register options configuration that runs when options resolve, so registering the same resource type again adds to the earlier configuration, and later assignments win. This lets a shared extension method set defaults and the app override them. Registering the same type under a different slug throws. The resolved `ResourceOptions<T>` (`StellarAdmin.Resources.Options`) is read-only: a custom data source can read it through `IOptions<ResourceOptions<T>>`, but configuration goes through the builders.
