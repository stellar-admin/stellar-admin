# Resource classes

Status: closed without implementation (2026-10-23). Stage G of [configuration simplification](../configuration-simplification.md#g-resource-classes-optional). Jerrie chose to keep plain extension methods, which the stellar-admin consumer skill already teaches under "Organising the configuration" in `skills/stellar-admin/SKILL.md`. The API below is kept for reference.

## Goal

A larger app can give each resource its own class, as it does for EF Core's `IEntityTypeConfiguration<T>`, and add every resource in an assembly with one call. The lambda style stays as it is.

## What apps do today

Each resource is added with a lambda (`admin.AddResource<Trip>(resource => ...)`, `admin.AddEfCoreResource<Product>(resource => ...)`). The playground already organises larger configuration by hand: one static class per resource, with an extension method (`admin.AddTripResource()`) called from `Program.cs`, and shared field helpers as private static methods.

So a resource class adds three things:

- A convention that apps and agents recognise, in place of a hand-made one.
- Adding every resource in an assembly with one call.
- The slug kept with the resource, in place of the call in `Program.cs`.

It doesn't remove type arguments. The configuration simplification plan says helpers that become members stop naming builder types, but that isn't so: `AddPlaceFields(ResourceFieldsBuilder<Trip> fields)` names the builder as a member or as a static method.

## Proposed API

```csharp
// Core (StellarAdmin.Resources)
public abstract class ResourceDefinition
{
    protected internal abstract void AddTo(StellarAdminBuilder admin);
}

public abstract class Resource<TResource> : ResourceDefinition
{
    protected virtual string? Slug => null;                        // null: the default slug, as AddResource<TResource>()
    protected abstract void Configure(ResourceBuilder<TResource> resource);
}

// Core (StellarAdmin, on StellarAdminBuilder)
public StellarAdminBuilder AddResource(ResourceDefinition resource);
public StellarAdminBuilder AddResourcesFromAssembly(Assembly assembly);

// EF Core (StellarAdmin.EntityFrameworkCore)
public abstract class EfCoreResource<TEntity> : ResourceDefinition where TEntity : class
{
    protected virtual string? Slug => null;
    protected abstract void Configure(EfCoreResourceBuilder<TEntity> resource);
}
```

Usage:

```csharp
internal sealed class TripResource : Resource<Trip>
{
    protected override void Configure(ResourceBuilder<Trip> resource)
    {
        resource.UseDataSource<TripDataSource>();
        resource.UseKey(trip => trip.Id);
        resource.AllowCreate(create => create.Fields(AddPlaceFields));
    }

    private static void AddPlaceFields(ResourceFieldsBuilder<Trip> fields) { ... }
}

internal sealed class ProductResource : EfCoreResource<Product>
{
    protected override string? Slug => "catalog";

    protected override void Configure(EfCoreResourceBuilder<Product> resource) { ... }
}

admin.AddResource(new TripResource());
admin.AddResourcesFromAssembly(typeof(Program).Assembly);
```

- **An instance, not a type argument.** `admin.AddResource<TripResource>()` can't be used, because `AddResource<TResource>()` already adds the model type, and constraints don't tell two overloads apart. The instance follows EF Core's `ApplyConfiguration(new ProductConfiguration())`.
- **One base class for both packages.** `ResourceDefinition.AddTo` is `protected internal`, so the EF Core package can override it (as `protected sealed override`), and `AddResource` and the assembly scan treat both kinds alike. `Resource<TResource>` calls `AddResource<TResource>(slug)` and then `Configure`. `EfCoreResource<TEntity>` calls `AddEfCoreResource<TEntity>(slug)` and then `Configure`.
- **The assembly scan.** It adds each concrete, non-generic `ResourceDefinition` that has a parameterless constructor, whether public or not, in full type name order. That order fixes the sidebar position of resources with no `Order`. A type with no parameterless constructor is skipped.
- **Configuration only.** A class takes no constructor services, and it isn't resolved per request, so lifetimes don't mix. Data sources, handlers and lookup sources register themselves already. Other app services, such as the playground's in-memory stores, stay in `Program.cs`.
- **Problem locations.** A problem in `Configure` points at its line in the class. A slug problem points at the `AddResource` or `AddResourcesFromAssembly` call, because the slug isn't read from a line.
- **Adding the same model twice.** A class and a lambda for the same model behave as two lambdas do today: the configuration adds up, and the slug check reports a duplicate slug.

## Steps

1. Core: add `ResourceDefinition`, `Resource<TResource>`, `AddResource(ResourceDefinition)` and `AddResourcesFromAssembly`, with XML docs and `PublicAPI.Unshipped.txt`. Add HTTP tests for a class resource, the slug override, the scan (abstract, generic and constructor-less types are skipped; type name order) and a problem's location.
2. EF Core: add `EfCoreResource<TEntity>`, with tests.
3. Turn the playground's Trip and Category registrations into classes, and add them with `AddResourcesFromAssembly`. Teach the class style in `skills/stellar-admin/references/resources.md`, next to the lambda style. Check the website docs, read-only.
4. Close this plan and stage G.

## Decisions (Jerrie)

1. Close stage G with no change (2026-10-23). It adds little over one static extension method per resource: one-call registration, a recognised convention and the slug kept with the resource. That doesn't outweigh a second registration style to teach and maintain, about six public members, and a reflection scan that fixes the sidebar order. It doesn't remove type arguments either. If G is revisited, register with `AddResourceDefinition<TripResourceDefinition>()`, with bases named `ResourceDefinition<TResource>` and `EfCoreResourceDefinition<TEntity>`, `where T : ResourceDefinition, new()`, in place of the instance form above. The scan would run in full type name order, and the playground would convert only Trip and Category.
2. Teach the extension pattern for larger apps. It was already taught (`009a1e8`), so nothing changes.
