# Generic resource follow-ups

## Checkbox group editor and playground user roles — 2026-09-27

Dashboard now has a `CheckboxGroupEditor` for collection fields. It shares request-time fixed, provider, and asynchronous choice loading with `SelectListEditor` through common options and editor bases; the existing select provider interface and `UseItems` syntax remain available. The EF Core checkbox overload uses the same query/projection loader and offers ordering without a select-only empty option. The DashboardPlayground create and edit user models carry `string[] RoleIds`; handlers validate IDs, resolve current names for Identity operations, and use a transaction for account and assignment writes. The checkbox binder now recognizes an all-unchecked submission after Dashboard filters configured fields. Verification: DashboardPlayground built successfully; its latest build emitted the existing `ViewDataKeys.cs` unresolved XML `cref` warning. All 7 focused Dashboard editor HTTP tests, 9 EF reference/choice integration tests, and 22 checkbox binder tests passed; `git diff --check` passed. The tracked playground database and all migrations were untouched. A direct playground Identity-handler runtime check was not completed because the temporary verification project could not restore two uncached package versions without network access.

The `Roles` legend is missing from the playground user form. Jerrie parked investigation of the label behavior on 2026-09-27; this work does not include a label fix. The playground database changed during review and is excluded from the code commit.

## EF Core select list options — 2026-09-27

`UseItems<TContext, TEntity, TValue>` now creates an internal `EfCoreSelectListItemsOptions<TEntity, TValue>` with the required value and text expressions and passes it to the public `EfCoreSelectListItemsBuilder<TEntity, TValue>`. The builder writes the optional ordering and empty-choice label into that options object. The extension reads the configured options when it builds the request-time loader. Consumer `UseItems` calls keep the same arguments and lambda syntax; explicitly named builder types need the new `TValue` argument. Verification: all 58 EF Core HTTP integration tests passed, including ordering and empty-choice rendering, and DashboardPlayground built. Both commands emitted the existing `ViewDataKeys.cs` unresolved XML `cref` warning. No database schema or data was changed.

## Select list item overloads — 2026-09-27

`SelectListEditorOptions` now uses `UseItems` for fixed `IEnumerable<SelectListItem>` choices, request-aware asynchronous loaders, and registered `ISelectListItemsProvider` implementations. The fixed-list overload snapshots the sequence when configured. The EF Core extension is also named `UseItems<TContext, TEntity, TValue>` and uses a C# extension block; the previous `UseItemsFrom` overloads and fixed-choice factory overload were removed. DashboardPlayground and the consumer setup reference use the new signatures. `InternalsVisibleTo` was unchanged. Verification: the Dashboard HTTP suite passed 167 tests, the EF Core HTTP suite passed 58 tests after the extension-block conversion, and DashboardPlayground built with no warnings or errors. No database schema or data was changed.

## EF Core select list items — 2026-09-27

`StellarAdmin.Dashboard.EntityFrameworkCore` extends `SelectListEditorOptions` with `UseItems<TContext, TEntity, TValue>(value, text, configure?)`. The required expressions project key and label values in one no-tracking query; keys are converted to invariant strings after materialization. `OrderBy` is optional and `IncludeEmptyOption(text)` explicitly adds a blank-valued first choice, with no empty choice by default. DashboardPlayground's department and category fields use this overload and no longer register dedicated item providers. The generic `UseItems<TProvider>()` path remains for custom sources. No database schema or data was changed. The playground built; the EF Core HTTP suite passed 58 tests, including the default-empty-option scenario, and the Dashboard HTTP suite passed 167 tests. The older sections below describe the earlier implementation stages.

`SelectListEditorOptions` stores one request-aware items loader. `UseItems(IEnumerable<SelectListItem>)` snapshots fixed choices when configured; the asynchronous callback, `UseItems<TProvider>()`, and the EF Core overload assign a loader for each request. The EF Core assembly still needs friend access to other Dashboard internals, and `InternalsVisibleTo` was left unchanged. After the earlier loader simplification, the Dashboard HTTP suite passed 167 tests and the EF Core HTTP suite passed 58 tests.

## Select list editor update — 2026-09-27

Dashboard now has `SelectListEditorOptions` for select fields. `UseItems` evaluates a choice factory when each form renders, and `UseItemsFrom<TProvider>` resolves a registered `ISelectListItemsProvider` for request-scoped, asynchronous choices. The redundant `ReferenceLookupEditor`, its options, template, provider interface, and resolver were removed after migrating the playground and EF reference tests to `SelectListEditorOptions`. DashboardPlayground uses the select list editor for department, category, language, and time zone fields, with language choices from both neutral and specific .NET cultures. The form model and database still enforce the existing 35-character length limit; membership in the rendered choice list is not validated on POST. Verification for the final removal is recorded below.

## Reference editor removal — 2026-09-27

The separate reference editor had the same `SelectListItem` data and Razor output as `SelectListEditor`, with only a provider requirement. Department and category item providers now implement `ISelectListItemsProvider` directly and expose `GetItemsAsync`; the editor and EF reference scenarios use `UseItemsFrom<TProvider>`. No database schema or data was changed. The user's existing `DashboardPlayground.csproj` and `app.db` working-tree changes were left untouched. After the provider terminology cleanup, `DashboardPlayground` built successfully; all 57 EF Core and 167 Dashboard HTTP integration tests passed. CSharpier and `git diff --check` passed for the affected files.

## Sidebar update — 2026-09-24

Resource sidebar registration now uses one shared Dashboard provider for ordinary and EF Core resources. DashboardPlayground's Identity user and role examples use that same provider. The dedicated Identity package was removed on 2026-09-27, so its migration item below is superseded. Other deferred items remain parked. The older audit and backlog below record historical source and proposals.

## Code audit — 2026-09-18

Current status: **parked**. All six deferred items remain relevant. EF registration still creates its own `ResourceSidebarProvider`; Identity has its separate provider. `EfCoreResourceController.cs` owns CRUD without a configurable operations class. `EfCoreReferenceBuilder.cs` exposes only `Choices`, `EfCoreReference.cs` requires a dependent navigation and materializes all choices, and `EditorOptions.cs`/`RadioEditorOptions.cs` expose class settings rather than masks or choice sources. The controller excludes concurrency-token properties from editable fields and does not round-trip GET tokens. Current sources are under `src/StellarAdmin.Dashboard*`, not a separate Pro repository.

This assessment uses the current checkout; earlier status, paths, permissions and verification notes below describe historical sessions. Runtime/browser and hosted release checks were not rerun for this documentation audit.

## Historical record

Status: parked. Last updated: 2026-09-10. No active implementation task. Jerrie closed the current session to work on other things; these items preserve intent for future agents and require a new task selection before implementation.

## Completed baseline

StellarAdmin handles basic EF Core CRUD, configured columns and writable fields, validation, form sections/groups/rows, scalar and enum editors, typed editor part classes, and EF single-record references through a select. IdentitySimplePlayground demonstrates Category and Product in its existing ApplicationDbContext. OSS input part classes and website examples are complete. See the [archived implementation record](archive/generic-resources-brainstorming.md) for rationale and verification.

Workspace, Pro, OSS, and website use `feature/generic-resources`; consumer skills were untouched. Relevant heads at closeout: Pro `afa2e5c`, OSS `4ced920`, website `d57587ab`; workspace `ae0ec45` records references, followed by the closeout documentation commit. These are feature-branch changes, not a merge or release. Recheck actual branch/status and remote state before resuming; do not assume historical branch instructions override a new task.

## Deferred backlog

### 1. Shared Dashboard sidebar registration

Priority: the remaining cleanup from the original effort. All Dashboard packages should add standard sidebar items through a simple method on StellarAdminDashboardBuilder, instead of registering providers for each controller/resource. Keep ISidebarItemsProvider as an escape hatch for custom behavior. The method signature remains open; propose it before implementing.

The resource-specific part is implemented in [resource sidebar registration](resource-sidebar-registration.md). It covers resources registered through `AddResource` or `AddEfCoreResource`, including DashboardPlayground's Identity example. The former package adaptation is superseded.

The historical starting paths included the removed Identity project. Current sidebar rendering consumes providers in `src/StellarAdmin.Dashboard/Areas/StellarAdmin/ViewComponents/SidebarViewComponent.cs`.

Any future sidebar changes should preserve ordering/grouping and existing custom providers, and verify sidebar output when multiple resources are registered.

### 2. Optional EF operations class

Planning update 2026-09-19: the shared operations/controller work is now covered by [resource configuration and controller unification](archive/resource-configuration-and-controller-unification.md). That plan covers EF and Identity together and records action-specific form models as deferred. The phase 1 implementation was reverted at the user’s request. The plan needs revision around a standalone resource foundation. Operations/controller work has not started. The other follow-ups remain parked.

StellarAdmin must continue providing default reading and writing. Jerrie liked an optional operations class but explicitly deferred it. A consumer should be able to override one operation and retain/call defaults for the others, with a scoped DbContext. `UseOperations<T>()` is a historical sketch, not an agreed public contract.

Start in `src/StellarAdmin.Dashboard.EntityFrameworkCore/EfCoreResourceController.cs` and `EfCoreResourceBuilder.cs`. Design a common resource query for constraints that must apply to index and direct edit/delete requests; the existing index TransformQuery alone is not an access boundary. Preserve configured-field binding, validation redisplay, authorization, antiforgery, cancellation, and current view customization. Propose the API and DI lifetime before coding.

### 3. Reference editor selection and large choice lists

The initial reference always uses Select; no editor-selection API was introduced. Later options may include autocomplete, radios, choice cards, and custom templates. The current dropdown loads all selectable key/label pairs and is intended for small datasets. Agree editor configuration and search/paging behavior together before implementation.

Start in `src/StellarAdmin.Dashboard.EntityFrameworkCore/EfCoreReferenceBuilder.cs`, `ReferenceChoicesBuilder.cs`, and `src/StellarAdmin.Dashboard/Areas/StellarAdmin/Views/Shared/EditorTemplates/Reference.cshtml`. Preserve nullable/required behavior, ModelState selection, help text, class mappings, read-only protection, and submitted-key validation. The current behavior shows an assigned filtered-out value as a disabled selected option and rejects it on a writable save; reconsider only through an explicit product decision.

### 4. External reference sources

Support sources outside EF, such as a DI-created service calling a web API. This was explicitly deferred; there is no public source contract yet. Decide key/label types, choice searching, existing-value resolution, validation, cancellation, failures, and service lifetime before implementation. The target should not need its own admin resource.

Start with the reference flow in `src/StellarAdmin.Dashboard.EntityFrameworkCore/EfCoreResourceController.cs`, `src/StellarAdmin.Dashboard/Areas/StellarAdmin/ViewModels/ResourceFormPageViewModel.cs`, and `src/StellarAdmin.Dashboard/Areas/StellarAdmin/FormFieldProperties.cs`. Keep EF dependencies in the EF package and request-specific choices out of singleton resource options. Historical lookup interfaces in the archived plan are brainstorming only.

### 5. EF references without navigation properties

Jerrie asked about entities exposing only a foreign-key property and then explicitly parked this. Current AddReference requires a mapped dependent navigation and direct property selectors; a foreign key alone remains an ordinary scalar field. EF relationships can exist without navigations, but Include requires a navigation.

A possible overload would accept source FK, target key, and target display selectors. An explicit SQL join/projection could carry the entity and label together, using a left join for optional references and separate presentation storage for the label. This is a proposal, not an approved API or implementation. Confirm permission to use joins in this case because the original requirement explicitly called for Include. Preserve the underlying efficiency requirement: no separate queries to resolve assigned labels, even in batches. Queries to populate selectable choices are separately allowed.

Start in `src/StellarAdmin.Dashboard.EntityFrameworkCore/EfCoreReference.cs`, `EfCoreResourceBuilder.cs`, and `src/StellarAdmin.Dashboard/Resources/Options/DataGridColumnOptions.cs`. Verify server-side label sorting, paging, optional/missing targets, and query counts if this is pursued.

### 6. Richer typed EditorOptions

Superseded on 2026-10-01 by the [field editor catalog](archive/field-editor-catalog.md), which added typed editors with their own settings and choice sources.

EditorOptions currently carries typed ClassNames; RadioEditorOptions adds per-option part classes. Jerrie also suggested future behavioral options such as string masks and explicit select/radio items. Masks and radio choices remain deferred, with a brief reminder already in code. Decide which editor-specific types are needed without turning the common options into a bag of unrelated settings. Editor options currently configure the selected editor; Template/MVC metadata select the editor itself.

The 2026-09-27 select list editor implements explicit select choices through its own typed options; masks and radio choices remain deferred.

Start in `src/StellarAdmin.Dashboard/Resources/Options/EditorOptions.cs`, `RadioEditorOptions.cs`, and the shared editor templates. Preserve repeated configuration composition, common-to-specialized upgrades, option isolation, and clear errors for incompatible specialized options. Coordinate with reference editor/source design to avoid competing choice APIs.

## Other known limitations

Stale-form optimistic concurrency detection is not implemented: edit reloads the entity on POST and does not round-trip a concurrency token from GET. Composite/shadow keys, collection references, and nested/inline editing are also outside the implemented scope. These are recorded limitations, not approved tasks. Single-record references remain the relationship scope unless Jerrie expands it.

## Resuming and verification

Read product AGENTS.md, the affected repo guides, and the public options-builder conventions. Inspect current code/status, select the item Jerrie actually requested, and turn its open decisions into a concrete API review where needed. Use DashboardPlayground for current demos; the former IdentitySimplePlayground was removed. Preserve logical blank-line spacing and file encoding. Do not start the other backlog items automatically.

The baseline resource tests run from the product root with `dotnet test --solution StellarAdmin.slnx --configuration Release --minimum-expected-tests 1`. The TUnit Dashboard unit and integration projects and EF integration project cover form builders, CRUD, references, query counts, editor rendering/binding, and form layouts using isolated temporary databases; the [EF runner migration](archive/ef-test-migration.md) records the replacement coverage. Use the SDK and conditional guidance in [development and verification](../development.md); the migration record distinguishes current verification from historical runs and existing warnings. For rendering changes, check desktop/mobile and dark mode; for reference changes, retain assertions that assigned labels are loaded with the entity and choice queries project only key/label. Do not alter the user's playground database for test fixtures or stop their process on port 5205.

At this closeout only documentation changed; prior implementation checks were recorded, not rerun. All repos were clean before the documentation edits. Pushes of the closeout and outstanding reference commits are part of the authorized session handoff; future commits/pushes still require authorization for their own task.
