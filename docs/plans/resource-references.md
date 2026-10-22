# Resource references

Status: approved (2026-10-22), in progress. Stage B of [configuration simplification](configuration-simplification.md), on the `configuration-simplification` branch.

## Goal

Related records come from their own resource. A resource says once how it looks when another record refers to it: its title, and optionally a description, media, search, ordering and which records may be picked. Choice and lookup editors take their items from that resource. On EF Core resources the related resource is inferred from the foreign key or many-to-many navigation, so the field needs only its editor. The record title also labels foreign-key fields and shows related records in columns, display fields and badges.

## Decisions (Jerrie, 2026-10-22)

1. Related types that aren't registered resources (Country and City without admin pages, say) keep today's `UseItems`. Stages A and E later shorten it. There is no pages-less "reference only" registration and no title guessing without a resource.
2. A per-field `Where` is always available on `UseResource<T>(...)`. It filters the related resource's items, so it works when that resource is an EF Core resource, whatever kind the form's resource is. When the related resource has a custom data source, the filter is ignored and a configuration problem is logged when the form renders, because the related resource is registered elsewhere and the compiler can't see its kind. Custom-source dependent lookups keep using `ILookupSource`.
3. Picking a related record doesn't require access to the related resource. Someone who edits products can pick a category without managing categories. The lookup's New button keeps its existing create-permission check.
4. Scope (left to Claude): lookups and choice editors backed by resources, the record title, foreign-key field labels, and related records shown in columns, display fields and badges. Page headings, breadcrumbs and delete confirmations that use the record title come later.
5. `UseTitle` stays on the resource builder, next to `UseKey`, not inside `Reference(...)`. The plan is approved as written.

## API

The resource describes itself once. `UseTitle` sits next to `UseKey` because the title identifies the record everywhere, not only in lookups (the later page headings use it too). `Reference` is a bare noun, as the [options builder conventions](../conventions/options-builders.md) ask for a structural drill-down below the root (`Index`, `SidebarItem`).

```csharp
admin.AddEfCoreResource<ApplicationDbContext, Category>(resource =>
{
    resource.UseTitle(category => category.Name);
    resource.Reference(reference =>
    {
        reference.UseDescription(category => "Category #" + category.Id);
        reference.UseAvatar(category => null);
        reference.SearchOn(category => category.Name, category => category.Description);   // EF Core only
        reference.OrderBy(category => category.Name);                                       // EF Core only
        reference.Where(category => category.IsActive);                                     // EF Core only
    });
});
```

- `UseTitle(Expression<Func<TResource, string>>)` is on every resource builder. Without it, the title is the property `[DisplayColumn]` names on the class, then a public `string` property called `Name` or `Title`, then the key.
- `Reference(...)` on a custom-data-source resource has `UseDescription` and the media methods (`UseIcon`, `UseAvatar`, `UseImage`, `UseCode`). The EF Core resource builder's `Reference(...)` adds `SearchOn`, `OrderBy` and `Where`. The builder types differ, so the compiler enforces this.

Fields refer to the resource:

```csharp
// EF Core entity form: the related resource comes from the foreign key or many-to-many navigation
fields.Add(product => product.CategoryId).UseEditor<LookupSheetEditor>(lookup => lookup.EnableCreate());
fields.Add(product => product.Tags).UseEditor<CheckboxGroupEditor>(group => group.Columns(2));

// A form model, or a resource with a custom data source: name the related resource
section.Add(model => model.DepartmentId).UseEditor<SelectEditor>(select => select.UseResource<Department>());
section.Add(model => model.RoleIds).UseEditor<CheckboxGroupEditor>(group => group.UseResource<ApplicationRole>());

// A filter for this field only: name the related resource, because the filter lambda needs its type
fields.Add(trip => trip.CityId)
    .DependsOn(trip => trip.CountryId)
    .UseEditor<SelectEditor>(select =>
        select.UseResource<City>(items => items.Where<Trip>((city, trip) => city.CountryId == trip.CountryId)));
```

- `UseResource<TResource>()` and `UseResource<TResource>(Action<ResourceItemsBuilder<TResource>>)` are on `ChoiceEditor` (select, radio group, checkbox group, segmented control, toggle group), `LookupSheetEditor` and `MultiLookupSheetEditor`. The items builder has only `Where<TModel>`. A field that needs a different look from the resource's reference uses `UseItems`.
- Inference on EF Core entity forms applies when a choice or lookup editor has no items: a single-property foreign key gives its principal entity, and a many-to-many navigation gives its target entity. Explicit `UseItems` or `UseResource` always wins.
- `EnableCreate()` already finds the related resource by the items' type, so it works with both.

Related records in columns, display fields and badges:

```csharp
columns.Add(product => product.Category);              // header "Category", the category's title
section.Add(product => product.Tags).UseDisplay<BadgeDisplay>();   // a badge per tag title, no UseText<Tag>
```

- A foreign-key field with no `[Display(Name)]` and no `Title` is labelled with the related resource's singular label, capitalized ("Category", not "Category id").

## Behaviour

- **The value is the related resource's key.** EF Core resources use their primary key. Custom-data-source resources use `UseKey`. A field whose type doesn't match the key reports the existing `UnsuitableEditor` problem.
- **Custom data sources need nothing new.** Selected items load with `ListAsync` and `ResourceListRequest.Keys`. Lookup searches pass `Search` and `Paging`. Choice editors list everything with no paging. The source's own order applies.
- **EF Core resources** reuse the EF lookup query and choice loader, with the primary key as the value, the title, the reference settings, and global query filters.
- **Search:** `reference.SearchOn`, then the resource's `EnableSearch` predicate, then the title.
- **Order:** `reference.OrderBy`, then the index's `DefaultSortBy`, then the title.
- **What may be picked:** `reference.Where` applies to every field that refers to the resource. A per-field `Where` adds to it. Both apply to searches and choice lists, not to the selected items, as the existing EF `Where` does.
- **Authorization:** the related resource's `RequireAuthorization` isn't applied to its items (decision 3).
- **Resolution at render.** Other resources aren't known while a resource's options are built, so a field's related resource is found when the form renders or the lookup searches, as `LookupCreateResource` already does.

Configuration problems, logged and worked around:

- `UseResource<T>` or an inferred foreign key names a type with no registered resource, or an unavailable one: the field shows no items, as an editor without items does today. The fix names `UseItems` and registering the resource.
- A per-field `Where` or EF Core `Reference` settings meet a related resource with a custom data source: the filter is ignored.
- `UseTitle` selects a value EF Core can't translate: reported by the EF Core resource, falling back to the convention.

## Implementation

- **Core options.** `ResourceOptions<TResource>` gains the title expression and the reference settings. The EF Core package reaches core only through public API (there is no `InternalsVisibleTo`), so the hook it needs is public integration surface, kept minimal, like `ResourceOptions.ReportProblem`.
- **Registration.** `ResourceRegistration` gains a factory that builds a reference for its resource: lookup items, choice items and a title formatter for a record. The core default works through `IResourceDataSource<TResource>.ListAsync`. The EF Core resource replaces it in its PostConfigure with one built on `EfCoreLookupItems` and the `EfCoreChoiceItemsLoader` projection, using the primary key, the title expression and the reference settings.
- **Field side.** A `ResourceLookupItems` (a `LookupItems` subclass) and a choice items loader that find the registration by type per request and delegate to its reference, carrying the per-field `Where`.
- **Inference.** In `ConfigureEfCoreResource`'s PostConfigure, next to `UseManyToManyBinders`, for entity forms (create and edit with `ModelType == TEntity`): fields with a choice or lookup editor and no items get resource items for the principal or target type. Many-to-many binders keep their existing checks.
- **Labels.** The foreign-key label default is resolved at render from the related registration, below any explicit title or `[Display(Name)]`.
- **Displays.** Column and display field rendering formats a value whose type is a registered resource with its title, and `BadgeDisplay` without `UseText` uses the item type's title. EF Core includes the navigation a column or display field selects (check `FindDisplayIncludePaths` covers a bare navigation).

## Steps

1. Record title: `UseTitle`, the conventions, and the registration's reference factory with the core data-source implementation.
2. `UseResource<T>` on the choice and lookup editors, with the configuration problems.
3. EF Core: the reference builder (`SearchOn`, `OrderBy`, `Where`), the EF Core reference, the per-field `Where`, and foreign-key and many-to-many inference.
4. Foreign-key labels, columns, display fields and badges.
5. Samples and consumer references.
6. Verification and this plan's record.

Checkpoint with Jerrie after step 2 (the core API in use) and after step 5 (the samples).

## Samples

- **Users:** Department select and roles checkbox group with `UseResource<T>` on create and edit.
- **Products:** Category lookup and Tags checkbox group and multi-lookup by inference. Category gets `UseTitle`/`Reference` with its description and avatar. The Category column becomes `columns.Add(product => product.Category)`, and the Tags badge loses `UseText<Tag>`.
- **Field editor gallery:** the `GalleryAirport` lookups use `UseResource<GalleryAirport>()`, and `GalleryAirportLookupSource` is deleted.
- **Trips:** unchanged. `TripAirport` isn't a resource and its lookup is value-based.

## Tests

- Admin panel integration tests (`tests/StellarAdmin.IntegrationTests/Resources`): a custom-data-source resource as a reference for a select, a lookup (find, search, paging) and a multi-lookup. Title conventions, `UseTitle`, description and media. A missing or unavailable resource, and a per-field `Where` on a custom source, each report their problem. Picking works for a user the related resource's authorization rejects.
- EF Core integration tests (`tests/StellarAdmin.EntityFrameworkCore.IntegrationTests/Resources`): foreign-key and many-to-many inference on create, edit and inline rows. `SearchOn`, `OrderBy`, `Where` and their fallbacks. Per-field `Where` with `DependsOn`. Global query filters. `EnableCreate` with an inferred type. Foreign-key labels, navigation columns, display fields and badges.
- Unit tests only for invalid configuration, per the [admin panel test strategy](../conventions/unit-testing.md#admin-panel-test-strategy).

## Documentation

- Consumer references in `skills/stellar-admin/references/`: `relationships.md` leads with resource-backed lookups and keeps `UseItems` for types that aren't resources. `resources.md` gains `UseTitle` and `Reference`. `fields.md`, `columns.md` and `view-pages.md` cover the labels, navigation columns and badges. Then `dotnet run --project util/SkillsGenerator`, its `--check`, and the skill validation in `build/skill-validation`.
- The admin panel has no website docs yet, so there is no website change.

## Open

- Later: page headings, breadcrumbs and delete confirmations that use the record title. Linking a related record in a column to its view page.

## Verification

Not started.
