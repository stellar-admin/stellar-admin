# Configuration simplification

Status: in progress (2026-10-23). Branch `configuration-simplification`. Stage B is done (2026-10-23): [resource-references](archive/resource-references.md). Stage C is done (2026-10-23): [foreign-key-editors](archive/foreign-key-editors.md). Stages A and D to G are research only; each gets its own plan, and Jerrie's approval, before work starts. All stages ship on this one branch.

## Goal

Make the admin panel configuration read cleanly. Developer experience comes first. No call should need more than one type argument, and the common foreign-key cases should need none. Today some calls take five (`UseItems<TripAirportSource, TripAirport, string, int, Trip>`), and the EF Core ones restate a context the resource already names.

## Why there are so many type arguments

Research on 2026-10-22, from the current source, the AdminPlayground registrations, the consumer references and the earlier decisions ([public API review](archive/public-api-review.md) D9 and the type-inference follow-up in [dependent fields](archive/dependent-fields.md#open)).

1. **Editors drop the field's types.** `ResourceFieldBuilder<TResource>.UseEditor<TEditor>(Action<TEditor>)` creates a non-generic editor with `new()`. The editor doesn't know the form's type, the property's type or the EF Core context, so every items call names them again. Inside `AddEfCoreResource<ApplicationDbContext, Product>` the playground still writes `UseItems<ApplicationDbContext, Category, int>`. The same loss causes `Where<Trip>` and `ReferenceFrom<Booking>` on the EF items builders, `BadgeDisplay.UseText<Tag>`, and loaders written as `(Trip trip, IServiceProvider services, CancellationToken cancellationToken) =>`.
2. **Related items are described per field, not per entity.** `UserResourceRegistration` configures the same Department select (12 lines) on create and again on edit, and the same roles checkbox group twice. Products shares its Category and Tag lookups through helper methods. The field editor gallery's `GalleryAirport` is a resource and also has a separate `GalleryAirportLookupSource` that repeats what its data source does.
3. **C# infers nothing from constraints.** `AllowCreate<TModel, THandler>` (also `AllowEdit`, `AllowView`, `EnableRowDetails`, `panels.Add<TModel, THandler>`) and `UseItems<TSource, TEntity, TValue>` restate what the handler's or source's interface already says. Partial type inference is only a C# proposal, and C# 14 extension blocks don't help.
4. **Conventions aren't used.** A foreign key without an editor renders a number input. The relationships reference tells developers to always give one an editor, although EF Core knows the related entity.

Counted in the playground, samples and tests: 81 explicit two-argument calls (`AllowCreate` 22, `AddEfCoreResource` 19, `AllowEdit` 18, `Add` 6, `EnableRowDetails` 5, `AllowView` 3, plus DI calls), and every `UseItems` call site takes three or five.

## Stages

| Stage | Change | Status |
| --- | --- | --- |
| B | Related records come from their own resource | Done: [resource-references](archive/resource-references.md) |
| C | EF Core conventions pick an editor for foreign keys | Done: [foreign-key-editors](archive/foreign-key-editors.md) |
| E | The DbContext is found, not named | Research |
| A | Fields keep their types; each editor gets its own method | Research |
| D | Lookup sources that aren't resources own their value and title | Research |
| F | Handlers name their model once | Research |
| G | Resource classes | Research, optional |

The order is a recommendation (2026-10-22): B, C and E are the biggest wins and mostly additive. A is the structural fix and the largest breaking change, and D lands with it. F and G are optional.

## A. Fields keep their types; each editor gets its own method

`fields.Add(p => p.CategoryId)` already infers `TProperty` and then drops it. Keep it: return a `ResourceFieldBuilder<TResource, TProperty>` and give the built-in editors their own methods, so the editor configuration is generic over the form and the property.

```csharp
fields.Add(trip => trip.CityId)
    .DependsOn(trip => trip.CountryId)
    .UseSelect(select => select.UseItems((trip, services, cancellationToken) => ...));   // no lambda types

fields.Add(product => product.Tags).UseCheckboxGroup(group => group.Columns(2));
```

- Removes the form's type (`Where<Trip>`, `ReferenceFrom<Booking>`, the fifth type argument), the value type (inferred from the property, or its element type for collections), and the hand-typed loader parameters.
- The same applies to displays: a typed column or display field builder lets `BadgeDisplay` on `p => p.Tags` take `tag => tag.Name` without `<Tag>`.
- `UseEditor<TEditor>` and `UseDisplay<TDisplay>` stay for custom editors and displays.
- Prior art: FluentValidation's `RuleFor(x => x.Email).EmailAddress()`, where the rule builder carries `<T, TProperty>` and validators are extension methods on it. AutoMapper's `ForMember` carries three inferred types the same way. EF Core's `Property(b => b.Url)` returns a `PropertyBuilder<TProperty>`.
- Method names follow the [options builder conventions](../conventions/options-builders.md): behavior is a verb on the leaf, so `UseSelect`, `UseLookupSheet`, `UseCheckboxGroup`.
- Costs: a breaking change across every built-in editor, the consumer references and the samples. Helper methods that take a builder get one more type parameter (`ResourceFieldBuilder<Trip, int?>`), which G reduces. A nullable foreign key (`int?`) against a non-nullable key (`int`) needs overloads or a conversion rule. Collection fields (`List<int>`, `int[]`, `ICollection<Tag>`) need care because C# generics are invariant.
- The dependent-fields plan recorded the same idea (`UseLookupSheet(lookup => ...)` with a `LookupSheetEditor<TModel, TProperty>`) and Jerrie's constraint that items stay on the editor, not the field. This proposal keeps that.

## C. EF Core conventions pick an editor for foreign keys

With B in place, an EF Core foreign key or many-to-many navigation whose related type is a registered resource needs no editor call. A single-valued foreign key gets a select or a lookup sheet, and a many-to-many navigation gets a checkbox group or a multi-lookup sheet. Explicit configuration is only for a different editor or settings.

- Prior art: react-admin's `ReferenceInput` defaults to an autocomplete input, and Django renders a select for every `ForeignKey`.
- To decide in C's plan: when to choose a select over a lookup sheet (a row-count threshold measured at render, a setting on the related resource's reference, or always one of them), and whether a foreign key to a type that isn't a resource keeps the number input.
- The EF Core PostConfigure in `StellarAdminBuilderExtensions.ConfigureEfCoreResource` already inspects form fields against the model (many-to-many binders, unsupported fields). The convention fits there, after B's foreign-key inference.

## D. Lookup sources that aren't resources own their value and title

For items that aren't a resource (airports from an external API), the source would be a class that defines the value, title and media itself, so the field names only the source.

```csharp
public sealed class AirportLookup(AirportClient client) : LookupSource<Airport, string>
{
    public override string Value(Airport airport) => airport.Code;
    public override string Title(Airport airport) => airport.Name;
    ...
}

lookup.UseSource<AirportLookup>();
lookup.UseSource<CountryAirportLookup>(trip => trip.CountryId);   // value-based, as in dependent fields
```

- The worst case today, `UseItems<TripAirportSource, TripAirport, string, int, Trip>(...)`, becomes one type argument.
- The argument's type can't be checked against the source at compile time without a second type argument. Options for D's plan: check it when the options resolve and log a configuration problem (as every other mismatch is), or keep two type arguments for value-based sources.
- Best done with A, which supplies the form's type to the argument lambda.
- `ILookupSource<TEntity, TValue>` and `ILookupSource<TEntity, TValue, TArgument>` were shipped by the dependent fields work. D's plan decides whether they stay alongside the new base class.

## E. The DbContext is found, not named

`TContext` never appears in a lambda a developer writes. It is only used to resolve the context from DI (`EfCoreResourceDataSource`, `EfCoreLookupItems`, `EfCoreChoiceItemsLoader`, `ManyToManyFieldBinder`, `EfCoreRowDetailLoader`). It can be found instead: the registered `DbContext` whose model maps the entity.

```csharp
admin.AddEfCoreResource<Product>(resource => ...);
admin.UseDbContext<ReportingDbContext>();   // only for apps where more than one context maps the entity, or to pin one
select.UseItems<Category>(category => category.Id, category => category.Name);   // with A supplying the value type
```

- Resolution happens where the EF Core PostConfigure already opens a scope to read the model. Which contexts are registered can be read from the service collection (`DbContextOptions<TContext>` registrations) when `AddStellarAdmin` builds.
- An entity mapped by no context, or by several without `UseDbContext`, is a configuration problem that makes the resource unavailable, like the existing `EntityNotMapped`.
- This removes a type argument rather than inferring it, so it doesn't conflict with D9, which rejected typed lambda parameters such as `(AppDbContext db) => db.Customers`.

## F. Handlers name their model once

`AllowCreate<CreateUserModel, CreateUserHandler>` names the model twice: once as a type argument and once in the handler's `IResourceCreateHandler<CreateUserModel>`. Options for F's plan:

- `AllowCreate<CreateUserModel>(create => create.UseHandler<CreateUserHandler>())`, with a mismatch logged as a configuration problem.
- Resolve `IResourceCreateHandler<CreateUserModel>` from DI, registered by the app as MediatR and FluentValidation do: `AllowCreate<CreateUserModel>()`.

Small win (two to one) across `AllowCreate`, `AllowEdit`, `AllowView`, `EnableRowDetails` and `panels.Add`.

## G. Resource classes (optional)

A class per resource, as in EF Core's `IEntityTypeConfiguration<T>`, FluentValidation's `AbstractValidator<T>`, AutoMapper's `Profile` and the Filament, Nova and Django admin resources:

```csharp
internal sealed class ProductResource : EfCoreResource<Product>
{
    public override void Configure(EfCoreResourceBuilder<Product> resource) { ... }
}

admin.AddResource<ProductResource>();
admin.AddResourcesFromAssembly(typeof(Program).Assembly);
```

- The types are named once, on the class. Shared field helpers become members, so their signatures don't name builder types (`ResourceFieldsBuilder<Trip>`), which matters more once A adds a type parameter.
- The lambda style stays for small apps. This is about organising larger apps, not about type arguments.
- Configuration only: the class isn't resolved per request and takes no services, so lifetimes don't mix.

## Prior art (how other admin frameworks configure related records)

- **Laravel Nova:** `BelongsTo::make('User')` uses the User resource's `$title` and `$search`. A `relatableQuery` on the related resource limits what can be picked. Closest to B.
- **react-admin:** `<ReferenceInput source="author_id" reference="authors" />` defaults to an autocomplete input whose option text is the authors resource's `recordRepresentation`. Closest to B with C.
- **Django admin:** a `ForeignKey` renders a select of `__str__` values. `autocomplete_fields` requires the related `ModelAdmin` to define `search_fields`, and the system checks report it when missing.
- **Filament:** resources have a `$recordTitleAttribute`, but each `Select::make('author_id')->relationship('author', 'name')` repeats the title attribute, which is today's StellarAdmin shape.

## Relation to earlier decisions

- **D9 (2026-10-17)** kept explicit EF `UseItems` type arguments rather than a set selector with a typed lambda parameter. None of these stages reintroduces typed lambda parameters. B and C remove most `UseItems` calls, E removes `TContext`, and A supplies types through the builder.
- **Dependent fields (2026-10-22)** recorded the same inference problem as a follow-up and the constraint that items stay on the editor. A keeps that.

## Decisions (Jerrie)

- 2026-10-22: One branch, `configuration-simplification`, for all stages, delivered stage by stage.
- 2026-10-22: Stage B is planned first. Its decisions are in [resource-references](archive/resource-references.md#decisions-jerrie-2026-10-22).
