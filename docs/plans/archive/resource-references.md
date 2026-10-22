# Resource references

Status: completed (2026-10-23). Stage B of [configuration simplification](../configuration-simplification.md), committed in `0027612`, `d2c0f42` and `0019744` on the `configuration-simplification` branch, which isn't merged yet. Follow-ups are under [Open](#open).

## Goal

Related records come from their own resource. A resource says once how it looks when another record refers to it: its title, and optionally a description, media, search, ordering and which records may be picked. Choice and lookup editors take their items from that resource. On EF Core resources the related resource is inferred from the foreign key or many-to-many navigation, so the field needs only its editor. The record title also labels foreign-key fields and shows related records in columns, display fields and badges.

## Decisions (Jerrie, 2026-10-22)

1. Related types that aren't registered resources (Country and City without admin pages, say) keep today's `UseItems`. Stages A and E later shorten it. There is no pages-less "reference only" registration and no title guessing without a resource.
2. A per-field `Where` is always available on `UseResource<T>(...)`. It filters the related resource's items, so it works when that resource is an EF Core resource, whatever kind the form's resource is. When the related resource has a custom data source, the filter is ignored and a configuration problem is logged when the form renders, because the related resource is registered elsewhere and the compiler can't see its kind. Custom-source dependent lookups keep using `ILookupSource`.
3. Picking a related record doesn't require access to the related resource. Someone who edits products can pick a category without managing categories. The lookup's New button keeps its existing create-permission check.
4. Scope (left to Claude): lookups and choice editors backed by resources, the record title, foreign-key field labels, and related records shown in columns, display fields and badges. Page headings, breadcrumbs and delete confirmations that use the record title come later.
5. `UseTitle` stays on the resource builder, next to `UseKey`, not inside `Reference(...)`. The plan is approved as written.

## API

The resource describes itself once. `UseTitle` sits next to `UseKey` because the title identifies the record everywhere, not only in lookups (the later page headings use it too). `Reference` is a bare noun, as the [options builder conventions](../../conventions/options-builders.md) ask for a structural drill-down below the root (`Index`, `SidebarItem`).

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
- **Field side.** Editors store a `ReferencedResource` (the type and the per-field `Where`) and get the items from the `ResourceReferences` service when their form renders. See [Progress](#progress).
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
- Unit tests only for invalid configuration, per the [admin panel test strategy](../../conventions/unit-testing.md#admin-panel-test-strategy).

## Documentation

- Consumer references in `skills/stellar-admin/references/`: `relationships.md` leads with resource-backed lookups and keeps `UseItems` for types that aren't resources. `resources.md` gains `UseTitle` and `Reference`. `fields.md`, `columns.md` and `view-pages.md` cover the labels, navigation columns and badges. Then `dotnet run --project util/SkillsGenerator`, its `--check`, and the skill validation in `build/skill-validation`.
- The admin panel has no website docs yet, so there is no website change.

## Open

- Later: page headings, breadcrumbs and delete confirmations that use the record title. Linking a related record in a column to its view page.
- Undecided: a public interface so custom editors get foreign-key inference; today they name their resource with `UseResource`.
- Not rerun: the skill evaluation tasks in `build/skill-evals/admin-panel/`.

## Progress

Steps 1 and 2 done (2026-10-22), reworked the same day from push to pull at Jerrie's request, committed in `0027612`. Step 3 done (2026-10-22), committed in `d2c0f42`. Steps 4 and 5 done (2026-10-23), committed in `0019744`. Step 6 done (2026-10-23). Afterwards, `ResourceReferences.FormatTitle` became internal at Jerrie's request (2026-10-23): code such as a row-details partial can call it on a record whose title's navigations weren't loaded, and quietly get the wrong title.

- `UseTitle` on every resource builder, `Reference(...)` on the core resource builder (`ResourceReferenceBuilderBase` shared with the coming EF Core builder), and `UseResource<T>()` with `ResourceItemsBuilder<T>.Where<TModel>` on `ChoiceEditor`, `LookupSheetEditor` and `MultiLookupSheetEditor`.
- Pull, not push (Jerrie, 2026-10-22). Configuration records only which resource a field refers to: `ReferencedResource.For<T>()`, the type and the field's filters, which lookups expose as `Resource`. Editors ask the public `ResourceReferences` service for the items when their form renders (`GetItems(resource)`, cached per field). The lookup handlers and the sheet, search and selection endpoints resolve the items once per request and pass them to the templates, which no longer read `editor.Items`. A custom editor uses the same two pieces, with no interface or binding step. The first version bound a proxy to the app's services from the options factory through an internal `IResourceItemsEditor`, which would have shut out custom editors.
- Integration surface for step 3: `ResourceOptions.Title` (resolved, conventions included), `ResourceOptions.Reference`, `ResourceOptions.UseReferenceItems(...)` and the abstract `ResourceReferenceItems` (a `LookupItems` with `ListAsync` for choice editors).
- Problems are reported when the form renders, at the `UseResource` line: no registered resource, an unavailable resource or one without a key (the field has no items), and a per-field `Where` on a data-source resource (the filter is ignored).
- `UseTitle` takes `Expression<Func<TResource, string?>>`, so nullable properties need no `!`. An empty title falls back to the key.
- Step 3: `EfCoreResourceBuilder.Reference(...)` takes `EfCoreResourceReferenceBuilder<TEntity>`, which adds `SearchOn`, `OrderBy` and `Where` to the shared media and description methods. The EF Core resource sets its reference items in its post-configuration: `EfCoreLookupItems` now derives from `ResourceReferenceItems` (with `ListAsync` for choice editors), valued by the primary key, with the fallbacks above and per-field filters through `EfCoreFormFilter`. The data source resolves `UseResource` lookups through `ResourceReferences` to include the selected reference with the edited entity.
- Inference goes through a new public `FormFieldOptions.InferResource(Type)`, which sets the resource only on a built-in choice or lookup editor without items, with the field's line as the problem source. The EF Core resource calls it for single-property foreign keys and many-to-many navigations on entity create and edit forms (inline rows use the edit form). Custom editors name their resource with `UseResource`.
- Deviation: a `UseTitle` EF Core can't translate is still shown (EF Core evaluates the final projection on the client), but the reference orders by key and searches don't match the title, instead of falling back to the title convention, which the EF Core package can't reach through public API. Reported as `UntranslatableTitle` (1086).

- Step 4: `ResourceReferences.FormatTitle(object record)` names a record of a registered resource, including an EF Core lazy-loading proxy. A related record without text of its own gets the text display by default, and `FieldDisplayView.FormatValue` and the badge templates show its title, so `columns.Add(product => product.Category)` and `UseDisplay<BadgeDisplay>()` on `Tags` need no `UseText`. The value check now lets through a registered resource's record, and a collection of them shown in badges; a collection without badges is still left out, with the fix `UseDisplay<BadgeDisplay>()`.
- Labels resolve at render: a field without a title or `[Display(Name)]` whose choice or lookup editor takes its items from a resource is labelled with the resource's singular label, capitalized. It applies to the field, its lookup sheet, its validation messages built by the controller, and the "Select ... first" text of dependent fields. MVC's own messages for attributes such as `[Required]` still use the property's name.
- Labels apply to single values only: a collection such as `Tags` keeps its plural property name rather than the resource's singular label.
- `Sortable()` on an EF Core column that shows a related record (`product => product.Category`) is turned off and reported (`SortableNavigationColumn`, 1087), as a collection column already was; sort by a property with `Sortable(selector)`.
- Step 5 samples: Users take Department and Roles with `UseResource<T>()`. Products infer Category and Tags from the foreign key and many-to-many navigation, with the category's description and avatar on the Category resource's `Reference(...)`; the Category column and view field show the navigation, and the Tags badges lose `UseText<Tag>`. The airport resource's own Hub lookup uses `UseResource<GalleryAirport>()`, with `UseKey`, `UseTitle` and a description, and its data source now honours keys, search and paging.
- Deviation: `GalleryAirportLookupSource` stays. The lookup gallery shows a different description and media on each field, which is what per-field `UseItems` is for; one resource has one look.
- Consumer references: `relationships.md` leads with the related resource (title, `Reference`, inference, `UseResource`, labels) and keeps `UseItems` and `ILookupSource` for types that aren't resources. `resources.md` gains a record title section; `columns.md`, `fields.md`, `view-pages.md`, `editors.md` and `SKILL.md` (task table, complete example, checklist, common mistakes) follow. The skill evaluation tasks in `build/skill-evals/admin-panel/` weren't rerun.
- A title that reads through another navigation, such as `UseTitle(c => c.Parent != null ? c.Parent.Name + " / " + c.Name : c.Name)`, was first loaded only by lookups: a column or display field that showed the record didn't load `Parent`, so it showed the wrong text, or failed the page with `c.Parent!.Name`. Fixed after step 6 (2026-10-23, Jerrie's go-ahead): each EF Core resource registers its title with the EF Core package's internal `EfCoreResourceTitles`, and a column or display field that shows related records also includes the navigations their title reads, such as `Category.Parent`. A title that dereferences a navigation that is null in the database, such as `c.Parent!.Name` on a category without a parent, still fails in memory, as it would in any other C# code.

## Verification

- 2026-10-23, after step 6: two more `ResourceRelatedResourceTests` (an index column and a view field showing a category whose title reads its parent) pass, and fail with the title includes turned off. All 1,865 tests pass in Release, and the admin playground builds.
- 2026-10-23, step 6: the solution builds in Release with no new warnings (the remaining ones predate the branch, or follow the test suites' existing nullable `IsEquivalentTo` pattern), and CSharpier reports the branch's 61 changed C# files formatted. `dotnet test --solution StellarAdmin.slnx --configuration Release` passes all 1,863 tests in the five test projects. `dotnet run --project util/SkillsGenerator -- --check` reports no drift, and the skill validation (8 tests, 4 skills and 92 Markdown files) passes. No new browser check; the step 5 smoke run stands.
- 2026-10-23, step 5: the playground builds, and a smoke run on port 5207 rendered the products index (Category column titles), edit (lookup labelled "Category" with its description), view (tag badges) and the airport Hub lookup search; `app.db` was unchanged. `dotnet run --project util/SkillsGenerator` and its `--check` (no drift), and the skill validation (8 tests, 4 skills and 92 Markdown files) pass. One more EF Core test (a sortable navigation column) and a label assertion pass: admin panel integration (664), unit (308) and EF Core integration (140).
- 2026-10-23, step 4: three more `ResourceRelatedResourceTests` (foreign-key label, navigation column titles, many-to-many badges), a core label test with a custom singular label, and three `ValueTextCheckTests` pass. The admin panel integration (664), unit (308) and EF Core integration (139) suites pass, and the admin playground builds.
- 2026-10-22, step 3: `ResourceRelatedResourceTests` (10 tests: foreign-key select and lookup inference with the selection included on edit, `EnableCreate` with an inferred type, many-to-many checkbox group inference, a missing related resource, `SearchOn`/`OrderBy`/`Where`, the index search and default sort fallbacks, a per-field `Where` on a self-reference, a global query filter, and an untranslatable title) pass. The admin panel integration (663), unit (305) and EF Core integration (136) suites pass, and the admin playground builds.
- 2026-10-22, steps 1 and 2 after the pull rework: `ResourceReferenceTests` (12 tests: select and lookup items, search with paging and default sort, multi-lookup, `UseTitle`, `[DisplayColumn]` and key titles, a missing resource, a resource without a key, an ignored `Where`, authorization, and a custom editor using `ResourceReferences`) pass. The admin panel integration (663), unit (305) and EF Core integration (126) suites pass.
