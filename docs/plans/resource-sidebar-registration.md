# Resource sidebar registration

Status: implemented, 2026-09-24.

## Goal

Every `AddResource<TResource>()` registration contributes one visible sidebar link to its index page. `AddEfCoreResource<TContext, TEntity>()` inherits the same behavior because it calls `AddResource<TEntity>()`. The link label defaults to the resource's resolved `PluralLabel`, including configuration applied after the resource registration but before options resolution. A consumer can configure the link label, group, display order, and visibility. All resource links are produced by one `ISidebarItemsProvider` for the Dashboard package.

## Public API

```csharp
dashboard.AddResource<Product>(resource =>
{
    resource.PluralLabel = "Products";
    resource.SidebarItem(item =>
    {
        item.Label = "Catalog";
        item.Group = "Commerce";
        item.Order = 20;
        item.Visible = true;
    });
});
```

`ResourceBuilderBase<TResource, TBuilder>.SidebarItem(Action<ResourceSidebarItemBuilder<TResource>> configure)` returns `TBuilder`, so the same method is available from both ordinary and EF Core resource builders. The nested builder has setter-only `Label`, `Group`, `Order`, and `Visible` properties and writes through `Services.Configure<ResourceOptions<TResource>>`, following the existing resource options pipeline. `ResourceOptions<TResource>.SidebarItem` holds a non-generic `ResourceSidebarItemOptions` object with defaults `Label = null`, `Group = "Resources"`, `Order = 0`, and `Visible = true`. The effective label is `SidebarItem.Label ?? PluralLabel`; it does not use the index page title. Blank labels and groups fail options validation. Setting `Visible = false` hides navigation only and leaves the route available.

The default order preserves resource registration order. Smaller `Order` values appear first within a group, with registration order breaking ties. Groups appear in the order their first resource was registered. `Order` does not reorder separate groups or links supplied by custom providers. Group names use exact ordinal matching. This scope keeps existing `ISidebarItemsProvider` implementations compatible.

## Implementation steps

1. Add the sidebar options and nested builder to the shared resource builder base. Validate nonblank explicit labels and groups through `ResourceOptions<TResource>` registration.
2. After controller registration in `AddResource<TResource>()`, register one internal resource descriptor keyed by `typeof(TResource)` and register the Dashboard resource sidebar provider once with `TryAddEnumerable`. A descriptor stores the controller name and a deferred accessor for `IOptions<ResourceOptions<TResource>>`. A second `AddResource<TResource>()` keeps the same descriptor, while subsequent configuration still updates its options.
3. At render time, the provider resolves each descriptor's final resource options, drops invisible entries, sorts entries within each group, and returns one `SidebarGroupItem` per nonempty group containing `SidebarActionLinkItem` links to the registered controller's `Index` action in the `StellarAdmin` area. Keep `SidebarViewComponent` and custom providers intact.
4. Add focused unit tests for one provider across several resources, plural-label fallback and late overrides, explicit label/group/order, stable ties, hidden entries, and repeated registration. Add HTTP integration assertions for rendered sidebar links and routes in both the ordinary resource host and the EF Core resource host.
5. Show resource sidebar configuration in `DashboardPlayground` using the user, role, customer, and product resources. Update development guidance after implementation and record the checks actually run here.

The detached `StellarAdmin.Dashboard.Identity` package currently has its own provider and is outside the active resource registration path. Its eventual reattachment can reuse the shared registration mechanism after its API is adapted to the current resource baseline.

## Implementation and verification

The shared resource builder base exposes `SidebarItem(...)`. `AddResource<TResource>()` adds one descriptor per resource type and registers one Dashboard `ISidebarItemsProvider`. The provider resolves final resource options when rendering, groups visible entries, and links each entry to its index action. `AddEfCoreResource<TContext, TEntity>()` uses the same path. `DashboardPlayground` places users and roles in Identity, and products and customers in Commerce, with explicit item order inside each group.

The affected Dashboard unit suite passed 28 tests, Dashboard HTTP integration suite passed 148 tests, and EF Core HTTP integration suite passed 57 tests through the direct TUnit executables. The playground built with one pre-existing unresolved XML `cref` warning in `ViewDataKeys.cs`. `dotnet test` could not start because its runner was denied permission to bind a local named pipe in the sandbox; the direct executables ran successfully. CSharpier formatted the touched library and test files. No browser review or full solution test run was performed.
