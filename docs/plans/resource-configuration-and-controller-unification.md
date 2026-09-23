# Resource configuration and controller unification

Status: revised rebuild plan agreed on 2026-09-19. Integration detachment is committed as `f936c29`; the resource reset is committed as `2f8b9d1` on `resource-redesign`. Steps 1 and 2 (resource registration and naming, then a working index page) are implemented. Step 3 (basic create) is committed as `ffb7538`. Dashboard test consolidation and the create factory callback are implemented. Global delegate-based label defaults and step 4 (advanced layouts) are implemented. Step 5 was split for review. Edit is committed as `71635f8`. Delete is committed as `dd892b4`. Operation results are committed as `de44966`. Split data source contracts and custom create are committed. Custom edit is committed as `c13fe5b`. Step 7 paging is committed as `0cdfb12`. HTMX index paging and deletion are committed as `70c6a2a`. Sorting is committed as `05af889`, and explicit query preservation as `e3e82c1`. Search is committed as `9bb3e2e`. Scopes are committed and pushed as `ffb2059`. EF Core checkpoint 1 is committed as `ffb6f85`: shared-controller registration, metadata keys, a read-only SQLite Product index, paging, sorting, and solution/build reattachment. Builder and feature-options inheritance review precedes expression configuration in checkpoint 2. This sequence supersedes the original three-phase plan; action-specific form models now come immediately after basic CRUD and before integrations.

## Objective

Build a simple standalone resource foundation in Dashboard, then make EF Core and Identity integrations use its builders and controller workflow. Resource identity comes from TResource. Labels derive from that type with optional SingularLabel and PluralLabel overrides. Integrations supply configuration and persistence/domain operations without introducing a separate form configuration system.

## Current baseline

The old resource builders, page/default options, controller base, query machinery, and related test projects have been removed. Dashboard retains its shell, Razor rendering, editors, field/layout definitions, and rendering models. These are reusable building blocks and may be simplified as the replacement API develops.

EF Core is reattached with a read-only implementation and new SQLite HTTP integration tests. Identity and IdentitySimplePlayground remain detached from the solution and build pipeline, with source referencing removed APIs retained for later adaptation. The active tests cover Core, TagHelpers, and replacement Dashboard resource configuration. The Dashboard integration suite verifies index rendering and create/edit flows through HTTP.

## Design rules

- Keep implementations simple. Reuse established working patterns when requested; do not add abstractions, flags, alternate response paths, or defensive machinery without a concrete requirement. Discuss a necessary departure before implementing it.

- Keep the shared resource implementation independent of EF Core and Identity. Prove it with an in-memory Product sample.
- Use a simple builder over resource configuration. Derive labels from TResource and optional SingularLabel/PluralLabel; explicit page labels take precedence. Do not restore captured defaults, reset machinery, or compatibility shims from the rejected implementation.
- Keep field and layout configuration on the same builder path for ordinary resources and integration defaults. Seed integration definitions through that builder before applying application configuration.
- Shared controller behavior owns HTTP flow, binding, validation redisplay, rendering, and redirects. Persistence and domain operations belong to the application or integration and resolve request-scoped services through DI.
- Keep TResource as the resource identity while allowing each form action to use its own typed model. Entity-backed forms remain the default. Loading and submission must explicitly handle mapping; do not introduce a speculative generic mapping framework.
- Follow the maintained options-builder conventions. Concrete public signatures beyond the agreed resource registration and label concepts remain implementation decisions.

## Rebuild sequence

Each step is a bounded implementation increment with focused tests and a compiling sample where applicable. Record actual verification as each step lands. This plan records the intended sequence and does not itself authorize implementation, commits, publishing, or deployment.

### 1. Resource registration and naming — completed

Introduce AddResource<TResource>(...), its builder, and resource options. Derive readable singular/plural labels from the type, allow SingularLabel and PluralLabel overrides, and establish straightforward override rules. Prove configuration resolution and isolation between resources. This step only registers configuration.

### 2. Working index page — completed

Use DashboardPlayground for a Product sample backed by an in-memory data source. Add column configuration and the minimum controller behavior needed to display products. Prove the full registration-to-controller-to-rendering path without integration dependencies.

### 3. Working create form — implemented, awaiting user review

Add typed field configuration, page titles, submit labels, model binding, validation, and saving through the sample's data source. Derive default labels from the resource labels and honor explicit overrides. Prove configured writable-field binding, invalid submission redisplay, and successful submission. Apply appropriate authorization and antiforgery protection as write actions are introduced.

### Before layout: foundation follow-ups — revised 2026-09-20

1. Consolidate Dashboard tests around the public builders and HTTP behavior. Remove redundant options/controller tests, retain a small unit suite for non-HTTP contracts, and establish this approach before adding factories or templates.
2. Add an optional create factory callback for resources without a parameterless constructor. Use it for GET and POST initialization before configured-field binding, retaining parameterless construction as the default.
3. Add overridable global label templates such as `Create {SingularLabel}` and `Edit {SingularLabel}`. Explicit page titles and submit labels take precedence. Keep resolution simple, without captured defaults or synchronization machinery.

Test consolidation, the creation factory, and global delegate-based labels are implemented and reviewed. Advanced layouts are implemented below. Continue proving subsequent features through focused HTTP scenarios rather than parallel options and controller unit tests.

### 4. Form layout

Add sections, rows, and groups to the same form builder. Exercise them in the Product sample using the retained rendering components. Prove layout configuration, binding through nested layout containers, and rendering at desktop/mobile widths.

### 5. Edit and delete

Edit was implemented first and committed after review. Delete was subsequently authorized on 2026-09-20 and is now implemented for review.

Extend the shared controller flow with record lookup, loading edit values, updates, deletion, missing-record handling, authorization, and antiforgery protection. Prove success and failure behavior using the in-memory sample and focused request tests.

### 6. Action-specific view models

Give Product separate create and edit models, including a form-only field. Prove typed field/layout configuration, initialization/loading, binding, validation, error redisplay, and explicit mapping to the resource through the normal controller workflow. Edit identifies the resource through the route independently of posted values. Keep the action model distinct from the page presentation model.

This step must establish the core capability before adding index features or reconnecting integrations. It should expose any assumptions that every form uses TResource early enough to correct them. Decide how selecting a different form model interacts with previously configured fields and page settings. Password and confirmation fields will later use this same capability in Identity, with appropriate sensitive-value redisplay handling.

#### Agreed implementation checkpoints — 2026-09-20

1. Introduce shared operation results in existing data source create, update, and delete operations. Map field and general validation errors to the form, preserve submitted values, return 404 for missing records, and redisplay the index with a summary for rejected deletes. Prove persistence failures through HTTP scenarios. Keep current resource models.
2. Settle the data source contract before adding handlers. Sketch ordinary and custom registrations together and decide how consumers avoid implementing unused operations. Implemented using a listing data source, per-action handler interfaces, and the combined IResourceCrudDataSource interface.
3. Implement custom create end-to-end with `Create<TModel, THandler>()`. A custom model requires a compatible handler. Do not expose a model-only overload. Callback registration returns the resource builder, and no-callback registration returns the action builder. Preserve typed fields/layouts and factory support. Keep the shared controller responsible for HTTP binding, validation, antiforgery, and responses. Add only the internal typed connection required to execute create.
4. Demonstrate a separate create model with password and confirmation in DashboardPlayground. Verify success, model validation, and handler validation with scenario integration tests. Pause for user review before custom edit.
5. Extend the reviewed design to `Edit<TModel, THandler>()`, including loading and route-key preservation. Prove custom create with ordinary edit and ordinary create with custom edit.

Ordinary create and edit continue to use TResource and the data source. Custom handlers contain persistence and model-loading behavior, without requiring an application controller. Expected persistence rejection uses operation results. Unexpected exceptions propagate. Error field names refer to model properties without the MVC binding prefix. Handler contracts and internal adapters remain design sketches until their checkpoint. Decide how errors for unrendered fields remain visible.

### 7. Index features

Design paging, sorting, searching, and scopes together, then implement and review them separately. Keep data access independent of EF and avoid assuming a provider supports EF asynchronous query methods. No index feature implementation is authorized by this plan update alone.

#### Agreed responsibility boundary — 2026-09-21

The shared builder declares index capabilities and presentation. The controller resolves URL parameters against that configuration and passes a normalized listing request to the data source. The data source decides what search text, sort fields, and scope identifiers mean and executes filtering, ordering, counting, and paging. The controller must not load all records and apply these operations itself.

Search and scope registration in the shared builder contain no query expressions or execution delegates. A future EF Core integration may provide an EnableSearch overload taking an expression and similar provider conveniences. That is deferred to integration adaptation. In-memory and external API sources implement their own semantics. Scope selection is not authorization. Mandatory access restrictions always apply in the data source independently of the selected scope.

#### Complete API design sketch

The following names and detailed behaviors are the working design from the discussion, to refine during each implementation checkpoint. The responsibility boundary above is agreed. Features remain unimplemented.

```csharp
resource.Index(index =>
{
    index.Columns(columns =>
    {
        columns.Add(product => product.Name, column => column.Sortable());
        columns.Add(product => product.Price, column => column.Sortable());
    });

    index.EnablePaging(paging =>
    {
        paging.PageSize = 25;
        paging.PageSizes = [10, 25, 50, 100];
    });

    index.DefaultSortBy(product => product.Name);
    // Alternatively: index.DefaultSortByDescending(product => product.Price);

    index.EnableSearch(search => search.Placeholder = "Search products...");

    index.EnableScopes(scopes =>
    {
        scopes.Add("all", "All products");
        scopes.Add("available", "Available");
        scopes.Add("out-of-stock", "Out of stock");
        scopes.DefaultScope = "all";
    });
});
```

No-callback Enable methods return the feature builder. Callback overloads return the index builder. Scalar configuration remains setter-only through the options pipeline. Start with one selected sort and one selected scope. Scope identifiers are explicit and stable, independent of displayed labels. Custom data sources implement the capabilities advertised by registration.

| Layer | Proposed models and members |
| --- | --- |
| MVC input | ResourceIndexQuery with nullable Page, PageSize, SortBy, SortDirection, Search, and Scope. Index binds this from the query string. |
| Data source request | ResourceListRequest with nullable Paging, Sort, Search, and Scope. ResourcePaging contains one-based Page and PageSize. ResourceSort contains Field and ResourceSortDirection (Ascending or Descending). |
| Data source result | ResourceListResult<TResource> contains Items and a long TotalCount after search/scope filtering but before paging. |
| Data source contract | ListAsync(ResourceListRequest request, CancellationToken cancellationToken) returns Task<ResourceListResult<TResource>>. CRUD handler contracts stay unchanged. |
| Configuration | Index options gain nullable ResourcePagingOptions, ResourceSearchOptions, ResourceScopesOptions, and default sort. Paging options contain PageSize and PageSizes. Search options contain a placeholder override. Scope options contain definitions (identifier/title) and a default identifier. Column options gain sortable configuration. |
| Presentation | Extend ResourceIndexPageViewModel<TResource> with nullable Paging, Search, and Sort plus a Scopes collection. Reuse or adjust the retained search/sort/scope view models. Add ResourceIndexPagingViewModel with Page, PageSize, TotalCount, TotalPages, and PageSizes. |

Null Paging means paging is disabled. Null Search means no search. Null Scope means no configured scope is selected. Null Sort delegates default ordering to the data source. Sort fields passed from HTTP are restricted to configured sortable identifiers. Typed default-sort selectors are resolved from configuration. Data sources must use deterministic ordering for paging, including a unique tie-breaker where the selected ordering is not unique. Numbered paging assumes an available total count. Cursor-only pagination is outside this increment.

The proposed controller policy is to default missing page/page size, use configured defaults for unsupported page sizes or unknown sorts/scopes, normalize whitespace-only search to null, and ignore parameters for disabled features. Malformed enabled numeric/enum parameters and overflow return 400. Define nonpositive page handling and concrete size/search limits when implementing normalization. A valid page beyond the last page redirects to the last available page (page 1 for no matches), retaining other index state. Account for concurrent changes without a redirect loop.

Keep query normalization in one internal helper reused for rejected-delete redisplay. Changing search, scope, sort, or page size resets page to 1. Page links preserve other state. Clearing search preserves scope and sort. Delete success and rejection preserve known index query values, including handling removal of the last item on a page. A rejected delete must retain its validation summary if the page needs adjustment. Configuration validation catches duplicate scope identifiers, missing default scopes, invalid page sizes, and invalid default-sort selectors.

New control labels use existing global label delegates and local overrides. The broader audit of existing hardcoded text remains deferred. Add types and members as their feature lands, using this shared design to avoid incompatible per-feature contracts.

#### Implementation checkpoints

1. **Paging and listing contract.** Introduce ResourceListRequest, ResourcePaging, ResourceListResult<TResource>, and the updated ListAsync signature. Migrate active sample/test sources. Add EnablePaging, options, query normalization, presentation model, and pagination controls. Prove paging disabled/enabled, allowed sizes, totals, empty/out-of-range pages, deterministic sample ordering, and delete behavior through focused HTTP scenarios. Exercise the sample with enough records at desktop/mobile widths. Pause for review.
2. **Sorting.** Add sortable columns, typed default-sort methods, ResourceSort/direction, query members, and links/indicators. The source applies ordering before paging. Verify permitted/default sorting, invalid selections, stable ordering, and page reset/state preservation. Pause for review.
3. **Searching.** Add EnableSearch, placeholder defaults/overrides, query/request members, and the search form. The source implements matching and counts filtered results. Verify search/clear behavior, empty results, and interaction with paging/sort. Pause for review.
4. **Scopes.** Add EnableScopes, named definitions/default scope, query/request members, and tabs using retained presentation models. The source applies the selected scope alongside search. Verify defaults, invalid identifiers, combined filtering/counts, page resets, and query preservation across controls and deletes. Pause for review before reconnecting integrations.

Use integration-first scenario tests for each increment, without duplicate options/controller suites. Record actual builds, tests, and browser checks when implementation occurs.

### 8. Reconnect integrations

Adapt EF Core first, then Identity. Reattach each integration, its sample, and appropriate new tests only after adapting it to the proven core. EF supplies database operations and provider-specific behavior. Identity seeds fields through the normal resource builder, applies user configuration through the same builder, and performs writes through UserManager and RoleManager.

Prove Identity user creation with an action-specific model for password and confirmation through the shared controller workflow. Preserve integration domain requirements, including error translation and self-deletion prevention, and review routes, view overrides, authorization, and EF reference behavior during adaptation. Identity integration is not complete while password fields require a separate form configuration or CRUD flow.

### EF Core implementation checkpoints — revised 2026-09-22

**Superseded design details:** the builder composition and separate EF options described below are replaced by the “EF builder and options design revision” at the end of this plan. The public expression API and data-source execution boundary remain the intended behavior.

The original planning authorization was to record the design and present the concrete builder API before coding checkpoint 1. No runtime changes are part of this planning increment. The previous extension-only proposal is superseded: returning ResourceBuilder<TEntity> would expose UseDataSource and allow replacing the EF source while leaving EF configuration behind.

Use a dedicated EfCoreResourceBuilder<TContext, TEntity> that composes the shared resource builder internally. Do not inherit from ResourceBuilder or expose the wrapped builder. AddEfCoreResource<TContext, TEntity>() returns the EF builder. Its callback overload returns StellarAdminDashboardBuilder. The builder exposes setter-only SingularLabel and PluralLabel, Index, ordinary and custom-model AllowCreate/AllowEdit, and AllowDelete. It omits UseDataSource and UseKey because the registration selects the EF source and EF metadata determines the primary key. Initially support one CLR primary-key property of type int, long, Guid, or string, matching the old integration. Reject unsupported key shapes clearly. Do not introduce composite route-key encoding in this increment.

The EF root builder uses EfCoreResourceIndexBuilder<TContext, TEntity> for Index. This builder forwards shared title/action labels, Columns, EnablePaging, and DefaultSortBy/DefaultSortByDescending to the shared configuration. It additionally exposes expression-based EnableSearch, EnableScopes with EfCoreResourceScopesBuilder<TContext, TEntity>, SortBy for column-specific sort overrides, and TransformQuery. Reuse the existing column, paging, search-settings, create, edit, delete, field, and layout builders. Add only the internal access needed for composition across the integration boundary, following existing friend-assembly conventions. Do not introduce a general builder inheritance framework.

Proposed usage (illustrative Product shape, not a required sample-model expansion):

```csharp
dashboard.AddEfCoreResource<AppDbContext, Product>(resource =>
{
    resource.SingularLabel = "Product";

    resource.Index(index =>
    {
        index.Columns(columns =>
        {
            columns.Add(p => p.Name, column => column.Sortable());
            columns.Add(p => p.Price, column => column.Sortable());
            columns.Add(p => p.CategoryId, column => column.Sortable(p => p.Category.Name));
        });

        index.DefaultSortBy(p => p.Name);
        index.TransformQuery(query => query.Include(p => p.Category));

        index.EnableSearch(
            term => p => p.Name.Contains(term),
            search => search.Placeholder = "Search products..."
        );

        index.EnableScopes(scopes =>
        {
            scopes.Add("all", "All products");
            scopes.Add("under-50", "Under 50", p => p.Price < 50);
            scopes.Add("50-and-over", "50 and over", p => p.Price >= 50);
            scopes.DefaultScope = "all";
        });

        index.EnablePaging();
    });

    resource.AllowCreate(create => create.Fields(fields =>
    {
        fields.Add(p => p.Name);
        fields.Add(p => p.Price);
    }));

    resource.AllowEdit(edit => edit.Fields(fields =>
    {
        fields.Add(p => p.Name);
        fields.Add(p => p.Price);
    }));

    resource.AllowDelete();
});
```

EnableSearch accepts Func<string, Expression<Func<TEntity, bool>>> and an optional configure overload using ResourceSearchBuilder<TEntity>. The overload without a configure callback returns that settings builder. There is no parameterless EF EnableSearch because EF needs a predicate to define its behavior. EnableScopes has no-callback and callback overloads. Its EF scopes builder supports Add(id, title) for an unfiltered scope and Add(id, title, Expression<Func<TEntity, bool>>) for a filtered scope, plus setter-only DefaultScope. Repeated feature enablement replaces both shared settings and corresponding EF expressions, preserving the current replacement semantics without preservation or model-switch machinery.

SortBy<TField, TSort>(Expression<Func<TEntity, TField>> column, Expression<Func<TEntity, TSort>> selector) identifies an existing column and supplies its database sort expression. It marks the column sortable. Ordinary sortable columns use their existing FieldExpression, with no duplicate configuration. DefaultSortBy continues selecting the column, so its override applies to both default and user-selected sorting. Keep column identity validation consistent with the current shared grid. Sort overrides replace earlier overrides for the same column. TransformQuery accepts Func<IQueryable<TEntity>, IQueryable<TEntity>> and composes in registration order. It customizes the index query only and must preserve entity results. It is not an authorization mechanism or an edit/delete lookup restriction.

ResourceOptions<TEntity> remains the authority for labels, columns, forms, enabled actions, and normalized index selections. A separate EfCoreResourceOptions<TContext, TEntity> stores the search predicate factory, scope predicate mappings, sort overrides, and index transformations. Configure both through the standard options pipeline. Do not inherit ResourceOptions, eagerly instantiate options, or duplicate UI defaults. The EF data source consumes both resolved options and the request-scoped DbContext. The controller, query model, operation results, views, and HTMX flow remain shared.

Index execution starts with the EF entity query, applies configured transformations, scope and search, counts matching records, applies ordering with primary-key tie-breaking, then paging and asynchronous materialization. Use no-tracking index reads. Pass expression trees to EF rather than compiling predicates or filtering materialized results. Translation remains provider-dependent and unsupported queries fail rather than silently falling back to client filtering. Mandatory restrictions must apply independently of scope selection and to record lookup, for example through DbContext global query filters.

1. **Basic EF registration and index.** Adapt the detached EF project to the dedicated root/index builder and shared controller registration. Implement key discovery, listing, paging, and ordinary column sorting. Prove a read-only EF Product resource in DashboardPlayground while Customer remains in-memory. Temporarily omit Product action registration until checkpoint 3. Remove obsolete EF controller/views and other references to deleted core APIs as necessary to compile. Reattach the compiling EF project and focused SQLite HTTP integration tests to the solution at this checkpoint so routine CI covers the new implementation immediately. Do not reconnect Identity. Review before expression features.
2. **Expression configuration.** Implement the proposed search, scopes, sort override, and index transformation APIs with EF-specific options. Prove combined filters, count-before-paging, stable ordering, default/explicit selections, and translated SQL behavior through SQLite HTTP scenarios. Reuse existing controller normalization and HTMX rendering. Review before writes.
3. **CRUD handlers.** Implement existing create/edit/delete contracts using EF and restore Product action registration. Preserve creation factories, configured-field binding, missing-record behavior, and validation redisplay. Reject writable keys, generated fields, and concurrency tokens as appropriate. Decide expected database-error translation into ResourceOperationResult explicitly. Do not turn arbitrary exceptions into validation errors. Custom form models retain their mandatory custom handlers and are not automatically mapped onto entities. Review before closeout.
4. **Integration closeout.** Verify solution/build-pipeline inclusion, update consumer documentation, and finish obsolete-code cleanup. Inventory the previous reference-field functionality and agree its follow-up scope explicitly rather than silently dropping or restoring it. Review authorization and view-override parity. Identity remains the subsequent integration, with the hardcoded-text audit after the rebuild sequence.

Verification remains integration-first through public registration and real MVC requests with an isolated SQLite database. Keep generic Dashboard tests EF-independent. Add only distinct configuration validation coverage beyond scenarios. Record actual builds/tests/browser checks per checkpoint. SQLite is a relational proof, not a guarantee that every expression translates identically on every provider.

Planning verification: inspected the current shared builders/options/controller contracts, detached EF registration/controller/options, and Playground configuration. Git working tree was clean and synchronized with origin/resource-redesign before this documentation edit. Only plan documents were changed. git diff --check passed for this increment. No application build, tests, or browser checks were run.

## Follow-up after the rebuild sequence

After steps 1–8, audit resource-facing text for remaining hardcoded wording, including the empty index message “No records found”. Make applicable text derive from the resource labels through the existing global delegate-based defaults and resource/page overrides. Review other empty states, action text, and confirmation messages as part of the same task. Requested on 2026-09-20 and deferred until the current list is complete. No rendering or label changes are included now.

## Verification and scope

Use integration-first TUnit coverage through public builders and HTTP, following the repository's Dashboard test strategy. Retain unit tests only for useful contracts that HTTP cannot express clearly. The shared Dashboard tests must not depend on EF or Identity. DashboardPlayground retains the host’s ASP.NET Core EF/Identity setup for future use, but its Product resource uses an independent in-memory data source. Run affected builds/tests per step and the active solution checks at delivery. Exercise rendered sample pages when adding UI behavior; leave the user's port 5205 process alone.

Update relevant maintained guidance and generated references when affected. Record commands, results, remaining decisions, and limitations here. Website work, unrelated backlog features, and new Identity workflows are outside this rebuild.

## Plan revision — 2026-09-19

Replaced the superseded implementation phases with the agreed eight-step rebuild sequence. Moved action-specific view models to step 6, immediately after edit/delete and before index features and integrations. Updated the plan index. Documentation only; no application builds or tests were rerun. Checked the documentation diff with git diff --check.

## Step 1 implementation — 2026-09-19

Added `AddResource<TResource>()` and its configuration overload, `ResourceBuilder<TResource>`, and `ResourceOptions<TResource>`. The builder is a facade over typed options registered through the standard options pattern. Humanizer.Core 2.14.1 supplies readable type names and English pluralization. A singular override affects the inferred plural. An explicit plural remains authoritative regardless of assignment order. Blank labels are rejected. Generic type arity is omitted from display labels.

Repeated resource registration composes callbacks in registration order. A no-argument call preserves configured values. Each resource type and service provider resolves a separate options instance. Defaults and application overrides can therefore share the same builder path without captured defaults or reset machinery. This step adds configuration only. Resource routes, navigation, page options, CRUD controllers, persistence, and the standalone sample remain for subsequent steps. EF and Identity remain detached.

Created a new Dashboard TUnit project through the CLI, added it to the solution, and introduced 16 tests covering default labels, compound/acronym/generic names, irregular plurals, explicit override order, updated singular fallback after reading a default, blank-label rejection, repeated registration, null callbacks, and isolation between resource types and service providers. Updated development commands and Dashboard setup notes.

Verification: Release solution build passed. The initial restore reported unavailable NuGet vulnerability metadata and existing source warnings. The final Release solution build completed with 12 warnings in unchanged source and zero errors. `dotnet test --solution StellarAdmin.slnx --no-build --configuration Release --minimum-expected-tests 1` passed all 158 tests, including 16 Dashboard cases. Tests ran outside the sandbox because local IPC was denied inside it. Solution-wide test discovery listed all 16 new cases. CSharpier and git diff --check passed. The consumer reference drift check passed. No browser checks apply to this configuration-only increment. Changes are uncommitted.

## Step 1 test approach revision — 2026-09-19

Moved the naming, override-order, fallback, and invalid-label cases from ResourceOptionsTests into StellarAdminDashboardBuilderExtensionsTests at the user's request. All resource configuration now goes through AddResource<TResource> and its builder callback. Tests resolve typed options only to observe results. The provider-isolation test also uses builder configuration and checks that providers resolve distinct instances. Removed ResourceOptionsTests. Blank-label cases now verify rejection during configuration resolution rather than directly inspecting options after a failed setter.

Verification: `dotnet build tests/StellarAdmin.Dashboard.Tests --configuration Release -m:1` passed with one existing XML documentation warning. The initial combined dotnet run/build attempt failed without a diagnostic. Running the explicit build and then `dotnet run --project tests/StellarAdmin.Dashboard.Tests --configuration Release --no-build` passed all 16 cases. Test discovery via `-- --list-tests` passed. CSharpier formatting and git diff --check passed. Production code is unchanged. The solution-wide 158-test result above predates this test-only revision.

## Step 1 language-feature review — 2026-09-19

Reviewed and preserved the user's C# `field` conversion in ResourceOptions. Simplified builder construction with target-typed `new(options)` and made the builder's single-assignment constructor expression-bodied. Kept its explicit internal constructor because a primary constructor on this public class would expose construction publicly. Registration already uses C# extension blocks. Recorded the preference for modern stable C# features in the maintained C# conventions.

Verification: the Dashboard test project Release build passed with one existing XML documentation warning and no new diagnostics. All 16 builder tests passed via `dotnet run --project tests/StellarAdmin.Dashboard.Tests --configuration Release --no-build`. CSharpier formatted the two changed implementation files and git diff --check passed.

## Step 1 builder return correction — 2026-09-19

Changed the no-callback AddResource<TResource>() overload to return ResourceBuilder<TResource> and use the requested summary, "Adds a resource." The callback overload invokes that overload, configures the returned builder immediately, and returns the Dashboard builder. Repeated calls share the registered resource options instance, matching Dashboard's registration-time configuration pattern. The builder constructor remains internal.

This supersedes the earlier deferred-callback and per-provider options behavior recorded above. Options are now shared per resource type and service collection and exposed through IOptions<ResourceOptions<TResource>>. Providers built from the same collection share those options. Separate service collections remain isolated. No copying or parallel configuration path was introduced.

Updated the tests to cover returned-builder configuration, mixed overloads, callback return values, immediate blank-label rejection, and service-collection isolation. Updated Dashboard setup notes and the options-builder conventions to state the overload return pattern and configuration lifetime.

Verification: the Dashboard test project Release build passed with one existing XML documentation warning. All 18 builder tests passed using `dotnet run --project tests/StellarAdmin.Dashboard.Tests --configuration Release --no-build`, and discovery listed all 18 cases. CSharpier formatted the changed C# files and git diff --check passed. Earlier solution-wide results were not rerun for this focused correction.

## Step 1 setter-only builder correction — 2026-09-19

ResourceBuilder now wraps OptionsBuilder<ResourceOptions<TResource>>. Its scalar properties are setter-only and register Configure actions. AddResource uses AddOptions and returns the resource builder. The callback overload invokes the builder callback and returns the Dashboard builder. Removed singleton options lookup and Options.Create registration. This supersedes the preceding registration-time shared-options implementation. Configuration values are applied when options resolve, through the standard configuration, post-configuration, and validation pipeline. Options instances are separate per provider.

Removed the builder-read test and assertions. All configuration flows through setters, with values asserted through resolved options. Added coverage for standard Configure/PostConfigure composition, options validation, and provider isolation. Updated maintained conventions and setup notes to describe the corrected pattern.

Verification: the Dashboard test project Release build passed with one existing XML documentation warning. All 20 tests passed with `dotnet run --project tests/StellarAdmin.Dashboard.Tests --configuration Release --no-build`, and discovery listed 20 cases. CSharpier formatting and git diff --check passed. No full solution rerun for this focused correction.

## Step 1 direct service-collection builder — 2026-09-19

Simplified ResourceBuilder to hold IServiceCollection directly. Setter-only label properties call Configure<ResourceOptions<TResource>>. AddResource registers options with AddOptions and returns a builder over the service collection. The internal constructor and callback overload remain unchanged in behavior. This replaces the OptionsBuilder wrapper without changing the options pipeline or adding configuration state. Updated the maintained builder convention.

Verification: the Dashboard test project Release build passed with one existing XML documentation warning. All 20 existing tests passed with `dotnet run --project tests/StellarAdmin.Dashboard.Tests --configuration Release --no-build`. CSharpier formatted the two changed implementation files and git diff --check passed. No new tests or full solution rerun were needed for this equivalent implementation.

## Historical work records

The records below describe earlier states and verification. They do not override the current baseline or rebuild sequence above.

## Planning verification — 2026-09-19

The product working tree was clean before planning. Inspected the shared builders/options, Identity and EF controllers, registration code, existing layout infrastructure, and maintained follow-up records. This change only adds planning documentation and cross-references. Application builds, tests, and browser checks were not run for the plan.

## Phase 1 rollback — 2026-09-19

The user rejected the phase 1 approach and requested removal of the implementation, including their edits based on it. Restored the affected tracked source, solution, test fixture, design guidance, development guide, and consumer setup reference to HEAD. Removed the new delete builder, configuration reference, regression tests, and Identity unit test project introduced by phase 1. The earlier planning documents remain.

The preferred direction is a simple standalone resource builder with labels derived from the resource type and optional SingularLabel and PluralLabel overrides. EF and Identity may be temporarily detached while that foundation is developed. Neither detaching integrations nor rebuilding the core was performed as part of this rollback.

Verification: checked restored tracked files byte-for-byte against HEAD and confirmed that only the three planning documents remain in Git status. `git diff --check` passed. Builds and tests were not rerun for this rollback. The previous implementation’s test results do not validate a future replacement.

## Integration detachment — 2026-09-19

Created branch `resource-redesign`. Temporarily removed EF Core, Identity, IdentitySimplePlayground, both integration test projects, and their shared Dashboard.Testing host from the solution. The Dashboard integration suite also depends on the Identity playground, so leaving it attached would still build both integrations transitively. All six project directories and their references remain intact for later adaptation and reattachment. Core, TagHelpers, and Dashboard unit tests remain active.

Release package creation, validation, and the consumer smoke script now cover Core, TagHelpers, and Dashboard. Removed the Identity playground client install from release preparation. CI and release test discovery follow the reduced solution. No resource APIs or controllers changed.

Verification: Release solution build passed with 18 warnings in unchanged source and no errors. All 149 tests across the three active unit test assemblies passed. The test runner initially failed to bind its IPC socket inside the sandbox and passed when rerun outside it. Consumer reference drift check passed. A recursive project-reference check confirmed that none of the 13 active projects pulls in a detached project. Shell syntax validation for the consumer smoke script and `git diff --check` passed. The hosted release workflow and full packaged-consumer smoke test were not run.

## Resource layer reset — 2026-09-19

Authorized deletion of the old resource orchestration and related tests before rebuilding. Deleted all shared resource builders, the resource controller base, resource/page/default options, search/scope/sort configuration, expression utilities, and index query processing. Deleted the concrete ResourceIndexPageViewModel that assembled rendering state from those options. No replacement registration, builder, or controller was added.

Retained the Dashboard shell, page tag helpers, Razor views, editors, form field/section/group/row definitions, grid column definitions, and rendering view models/interface. Moved IndexPageSelection into the rendering view-model namespace because the retained index views use its selected URL values. It no longer belongs to query infrastructure. These retained pieces are building blocks, not a functioning resource CRUD pipeline.

Deleted Dashboard.Tests, Dashboard.IntegrationTests, Dashboard.EntityFrameworkCore.IntegrationTests, and Dashboard.Testing, including their project files and fixtures. Removed the last active Dashboard test project from the solution through the .NET CLI. Core and TagHelpers tests remain. EF and Identity library and playground sources remain detached and now reference deleted APIs. Their future adaptation is outstanding. No compatibility shim or replacement test host was introduced.

Verification: `dotnet build StellarAdmin.slnx --configuration Release -m:1` passed with 12 warnings in unchanged source and no errors. `dotnet test --solution StellarAdmin.slnx --no-build --configuration Release --minimum-expected-tests 1` passed all 142 remaining tests (run outside the sandbox for local IPC). Consumer reference drift check and `git diff --check` passed. No browser checks or hosted release run were performed. Passing Core/TagHelpers tests does not provide coverage for the retained Dashboard rendering.

## Step 2 implementation — 2026-09-19

Extracted the action-view lookup into a private `ResourceView(string action, object model)` helper, following the requested convention of trying the action name before `Resource{action}`. `Index` calls it with `nameof(Index)`; future actions can reuse it. Verification: Dashboard unit project Release build passed with the existing XML documentation warning, all 26 Dashboard unit tests passed, and CSharpier and `git diff --check` passed.

Follow-up: the shared resource controller now searches for `Index` through `ICompositeViewEngine.FindView` before falling back to `ResourceIndex`. Applications can provide `Areas/StellarAdmin/Views/Product/Index.cshtml`; normal shared locations and configured view-location expanders also participate. Documented this in the consumer setup reference. Two controller unit cases verify view selection with a test view engine. Verification for this follow-up: Dashboard unit and integration project Release builds passed (one existing XML documentation warning during the unit build); all 26 Dashboard unit tests and six HTTP integration cases passed, including actual rendering through the default fallback. CSharpier and `git diff --check` passed. The full solution suite and browser checks were not repeated for this follow-up.

Implemented the agreed `UseDataSource<TDataSource>()` API and `IResourceDataSource<TResource>` with its initial `ListAsync(CancellationToken)` operation. The concrete data source defaults to scoped registration and preserves any existing application registration. The controller resolves the selected source from the request scope. Write operations remain for the subsequent CRUD steps, including action-specific models before integration adaptation.

Added `Index` and typed `Columns` builders, including column `Title` and `Format`, append/clear behavior, and an optional page title. Column configuration runs through the resource options pipeline and creates separate column options for each resolved options instance. `AddResource<TResource>` registers the closed generic shared controller under the resource type name. `MapStellarAdmin` exposes `/stellaradmin/Product`, independently of display-label changes. Default page titles come directly from the current plural label. There is no captured-default state.

Simplified the retained index presentation model and shared Razor views to columns, items, title, and an empty state. The index no longer assumes create/edit/delete, sorting, search, scopes, or paging are present. These will return through the planned increments. Added the Product model and in-memory ProductDataSource to the user's DashboardPlayground, filled its existing configuration placeholder, mapped Dashboard routes, and linked Products from the host navigation. Preserved the user's EF/Identity host setup and Dashboard project reference. The detached StellarAdmin integration projects remain detached. Automatic resource sidebar entries are not part of this increment.

Added four builder/data-source registration tests and a separate Dashboard integration project with six HTTP test cases. HTTP coverage exercises real MVC routing and Razor rendering, property display metadata, column format/title overrides, encoded cell values, empty results, label/title precedence at a stable route, per-request source resolution, and an unregistered resource returning 404. Hosts and their in-memory state are private to each test. The integration suite does not reference DashboardPlayground, EF Core, or Identity.

Verification: `dotnet build StellarAdmin.slnx --configuration Release -m:1` passed with 12 existing warnings and zero errors. The final unit assertion refinement was then rebuilt with zero warnings/errors. `dotnet test --solution StellarAdmin.slnx --no-build --configuration Release --minimum-expected-tests 1` passed all 172 tests across four assemblies, including 24 Dashboard unit tests and six Dashboard integration cases. Solution-wide `--list-tests` discovered all 172. The solution runner used escalation for its local IPC sockets. Focused Dashboard unit and integration executables also passed in the sandbox. CSharpier and `git diff --check` passed. Consumer reference generation and the subsequent `--check` reported no drift.

Ran DashboardPlayground on an agent-owned port 5206 instance and verified the Product page in Chromium at 1440×1000 and 390×844. Both sizes showed all three seeded products, configured headers and decimal formatting, loaded stylesheets, and no horizontal page overflow. Captured screenshots at `/tmp/product-index-desktop.png` and `/tmp/product-index-mobile.png`. Used decimal sample formatting to avoid the invariant culture's generic currency symbol. Port 5205 was untouched. Changes are uncommitted. Step 3 (create form) is next.

## Step 3 implementation — 2026-09-20

Implemented the agreed basic create API: `resource.Create(...)`, setter-only Title and SubmitLabel, and explicit typed fields with Add, Clear, and field Title overrides. Configuration uses the standard options pipeline and creates independent field definitions per resolved options instance. Selectors reject methods, nested members, and properties without public getters/setters. Retained form field options and editor partials provide rendering. Advanced layout builders remain deferred until the user reviews and accepts the basic flow.

Added `CreateAsync(TResource, CancellationToken)` to the data source. The shared controller constructs a resource through its public parameterless constructor, binds only configured fields under the Entity prefix, validates through MVC, redisplays invalid submissions, and persists valid submissions before redirecting to Index. POST validates antiforgery tokens. Create uses the existing ResourceView helper and falls back to ResourceCreate. The index now links to Create. Both default form title and submit label derive from the current singular resource label.

DashboardPlayground uses a mutable Product with data annotations and explicitly configures Name and Price. Its synchronized singleton in-memory data source preserves created records across requests and assigns IDs. Restarting resets the data. The existing host EF/Identity setup is unchanged. Action-specific models, business validation results, edit/delete, and integration adaptation remain later work.

Added builder tests for replacement, options isolation, and rejected selectors. HTTP tests cover default and overridden labels, configured fields, successful persistence and index redirect, exclusion of forged IDs, required/range/conversion failures with value redisplay, and missing antiforgery tokens. The existing index tests and data source fakes were adapted to the new operation.

Verification: Dashboard unit and HTTP integration Release builds passed, with the existing unresolved FormFieldOptions XML cref warning on the integration build. All 31 unit and 13 HTTP tests passed through their project executables. DashboardPlayground Release build passed. Chromium checks at 1440×1000 and 390×1000 verified form fields and no horizontal overflow, invalid POST errors, and a valid browser submission followed by the index showing the created product. Screenshots are `/tmp/resource-create-1440.png` and `/tmp/resource-create-390.png`. The agent-owned playground on port 5206 and browser were stopped. Port 5205 was untouched.

Final verification: the full Release solution build passed with 11 existing warnings and no errors. Consumer reference drift check passed. CSharpier formatted the touched C# files and `git diff --check` passed. The full solution test suite was not rerun. Changes are uncommitted.

## Integration-first test consolidation — 2026-09-20

Completed the testing follow-up first at the user's request, before factory callbacks and global label templates. Recorded the durable Dashboard strategy in `docs/conventions/unit-testing.md` and updated development guidance and the plan index. Production behavior is unchanged.

Reduced Dashboard coverage from 31 unit plus 13 HTTP cases to 10 unit plus 18 HTTP cases. The retained unit cases cover blank labels (four), unsupported field selectors (three), DI lifetime preservation and replacement (two), and consolidated resource/column/field/editor isolation across providers (one).

Coverage mapping and intentional removals:

- Default and explicit labels and ordinary builder configuration remain covered by existing rendered index/create tests. Strengthened create assertions to reject duplicate fields after clearing and rebuilding configuration.
- Moved column replacement to an HTTP test that checks the actual headers and cells. Added repeated registration cases using both builder overloads to verify singular-derived and explicit plural labels at the stable route.
- Replaced mocked controller/view-engine tests with compiled application Razor overrides for Index and Create on a second resource. Existing Product tests continue to verify shared fallback views. Enabled MVC Razor compilation in the integration project for these fixtures.
- Consolidated three provider-isolation tests into one. Removed separate service-collection isolation coverage because every HTTP scenario uses a private host.
- Removed low-value assertions about builder return references, null callback guards, standard options Configure/PostConfigure/Validate plumbing, and third-party acronym/generic naming details. These are deliberate coverage reductions, not claimed HTTP replacements. Retained irregular plural and override composition checks through rendering.

Verification: both Dashboard Release builds passed without warnings after correcting a namespace in the new Razor fixture. All 18 HTTP cases and 10 unit cases passed. The CI/release solution command `dotnet test --solution StellarAdmin.slnx --no-build --configuration Release --minimum-expected-tests 1` passed all 170 tests across four assemblies. Solution-wide discovery listed 170 tests. After moving the consolidated isolation case to ResourceBuilderTests, its explicit Release build and all 10 unit cases passed again. The combined run/build command had failed without diagnostics, so verification used separate build and no-build run commands. The solution runner required execution outside the sandbox because its local IPC socket was denied inside it. CSharpier and git diff --check passed. No browser checks or consumer-reference regeneration were needed for this test/documentation-only change. Changes are uncommitted.

## Scenario-oriented integration test structure — 2026-09-20

Reorganized the 18 Dashboard HTTP cases into `Resources/ResourceIndexTests.cs` (three), `ResourceCreateTests.cs` (seven), `ResourceConfigurationTests.cs` (six), and `ResourceViewOverrideTests.cs` (two). Renamed methods to `Scenario_ExpectedOutcome` without controller action prefixes. Extracted the existing host setup into `Infrastructure/DashboardTestHost.cs` and sample models, data sources, and per-test state into `Fixtures/`. The create form helper remains private to ResourceCreateTests. Real Razor overrides remain under `Areas/StellarAdmin/Views/CustomProduct`. Removed the old controller test files and their empty directory.

This is an organizational change. No scenarios or assertions were added or removed. Each test still creates and disposes its own host and state. Updated the testing conventions to distinguish production-type-based unit tests from feature/scenario integration tests, and documented the folders in development guidance.

Verification: `dotnet build tests/StellarAdmin.Dashboard.IntegrationTests --configuration Release -m:1` passed with zero warnings and errors. `dotnet run --project tests/StellarAdmin.Dashboard.IntegrationTests --configuration Release --no-build` passed all 18 cases. The same command with `-- --list-tests` discovered all 18 renamed cases. CSharpier and git diff --check passed. The earlier 170-test solution result predates this reorganization. No production changes, browser checks, or commits were made.

## HTML test helper cleanup — 2026-09-20

Added four internal extension helpers in `tests/StellarAdmin.Dashboard.IntegrationTests/Infrastructure/HtmlTestExtensions.cs`: GetDocumentAsync fetches a successful response, parses it, and disposes the response. ReadDocumentAsync parses a caller-owned response without asserting its status or disposing it. RequiredElement returns the first matching element or throws a descriptive exception containing the selector. TextContents returns trimmed text for all matching elements in document order.

Updated the four resource scenario classes to use these helpers for repeated parsing and HTML reads. Requests, response status assertions, binding names, and scenario expectations remain visible in each test. RequiredElement prevents a missing input from passing an empty-value assertion. Explicit absence and element-count assertions retain direct queries. PrepareForm stays private to ResourceCreateTests and now reuses response parsing and required-element lookup. No dependencies or additional test cases were introduced.

Verification: the integration project Release build passed with one existing unresolved XML cref warning in ViewDataKeys.cs and zero errors. `dotnet run --project tests/StellarAdmin.Dashboard.IntegrationTests --configuration Release --no-build` passed all 18 cases. CSharpier and git diff --check passed. The full solution suite was not rerun for this test-helper cleanup. Production code is unchanged. Changes are uncommitted.

## Creation factory callback — 2026-09-20

Added `create.UseFactory(Func<TResource>)` to the create builder. The nullable `ResourceCreateOptions<TResource>.Factory` is configured through the existing options pipeline. Both controller actions call `CreateInstance()`, which invokes the configured factory or falls back to `Activator.CreateInstance<TResource>()` when no factory is supplied. GET invokes the factory for initial form values; POST invokes it again before binding configured fields, then follows the existing validation and persistence flow. Invalid submissions redisplay the bound instance without invoking the factory again. The callback should return a new instance on each invocation.

DashboardPlayground demonstrates factory-provided initial pricing. Added three HTTP cases using InventoryItem, which requires a constructor argument: initial values render, a valid submission persists the fresh factory instance, and an invalid submission redisplays submitted values without persistence. Submission coverage also proves an unconfigured posted field cannot overwrite the factory's value. Existing Product cases continue exercising default parameterless construction. No new unit tests or dependencies were added.

Global label templates are next; advanced layouts remain on hold.

Verification: `dotnet build StellarAdmin.slnx --configuration Release -m:1` passed with 12 existing warnings and zero errors. After correcting a test expectation for MVC's empty-string-to-null binding, the integration project Release rebuild passed with zero warnings/errors and all 21 HTTP cases passed using `dotnet run --project tests/StellarAdmin.Dashboard.IntegrationTests --configuration Release --no-build`. Discovery listed the three new factory cases. All 10 Dashboard unit tests passed. CSharpier and `git diff --check` passed. The full solution test suite and browser checks were not run. Changes are uncommitted.

Follow-up verification after making Factory nullable and adding CreateInstance: the integration project Release build passed with the existing ViewDataKeys XML cref warning; all 21 HTTP cases passed again. CSharpier and git diff --check passed.

ResourceController now resolves options.Value once into a readonly field during construction and uses that field throughout. The integration project Release build passed with the existing ViewDataKeys XML cref warning; all 21 HTTP cases passed again. CSharpier and git diff --check passed.

## Global resource label callbacks — 2026-09-20

Implemented `dashboard.ConfigureResourceLabels(...)` with setter-only IndexTitle, CreateTitle, and CreateSubmitLabel callbacks. Each is a `Func<ResourceLabelContext, string>` receiving the resource’s effective SingularLabel and PluralLabel. ResourceLabelOptions supplies the existing default wording, and the builder uses the standard options configuration pipeline. ResourceController resolves label options once into a readonly field and invokes callbacks when constructing page models, only where an explicit resource title or submit label is absent. No parsed templates, extra dependencies, or captured defaults were introduced. Edit/delete settings remain deferred until those actions exist.

DashboardPlayground demonstrates an “Add Product” title and “Save” submit button. Added two HTTP cases for global labels and explicit resource precedence, and extended existing invalid-submission cases to verify callback-generated labels on redisplay. Existing scenarios continue verifying the built-in defaults. No new unit tests were added.

Verification: the initial parallel integration build stopped during restore without diagnostics. The explicit single-worker integration build with `--no-restore` passed with the existing ViewDataKeys XML cref warning. `dotnet build StellarAdmin.slnx --configuration Release --no-restore -m:1` passed with 12 existing warnings and zero errors, including DashboardPlayground. All 23 HTTP integration cases and all 10 Dashboard unit cases passed via their Release project executables with `--no-build`. CSharpier checked all eight touched C# files and `git diff --check` passed. The full solution test suite and browser checks were not run. Changes are uncommitted.

## Index create button label — 2026-09-20

Replaced the hardcoded index Create button text with a resolved view-model label. The global `IndexCreateLabel` callback defaults to “Create”; `resource.Index(index => index.CreateLabel = "...")` supplies an explicit resource override, with null falling back to the global callback. The callback receives the same effective resource labels as the index title. Extended existing HTTP scenarios to cover default text, global callback text, and resource override precedence without adding test cases.

Verification: the integration project Release build (`--no-restore -m:1`) passed with the existing ViewDataKeys XML cref warning and no errors. All 23 HTTP integration cases passed with `--no-build`. CSharpier and `git diff --check` passed. Full solution tests and browser checks were not run. Changes are uncommitted.

## Step 4: advanced form layouts — 2026-09-20

Added AddSection, AddRow, and AddGroup to ResourceFieldsBuilder, including callback overloads returning the parent and no-callback overloads returning the child builder. Sections support a title and optional Description. Nested builders share the existing typed Add field API, and Clear removes only the current scope's fields and containers. Root configuration still uses the standard options pipeline, with fresh containers and fields for each resolved options instance. No Identity or EF dependencies were introduced.

ResourceCreateOptions.Items is now the authoritative ordered layout tree. Fields is a read-only flattened projection used for the binding allowlist and field metadata. ResourceController passes the tree to the retained Razor partials, so nested fields retain the same Entity binding prefix, validation, persistence, and invalid-submission redisplay. Create.SectionLayout optionally overrides the application's form setting. No Razor, Tag Helper, or CSS changes were necessary.

DashboardPlayground demonstrates a Product details section with a description and two row columns, each containing a field group. The API remains straightforward for forms that need only fields:

```csharp
create.Fields(fields =>
{
    fields.AddSection("Product details", section =>
    {
        section.Description = "Information shown in the catalog.";
        section.AddRow(row =>
        {
            row.Add(product => product.Name);
            row.Add(product => product.Price);
        });
    });
});
```

Added three HTTP cases covering nested markup and field order, encoded section text, global/per-resource section layout selection, and clearing the entire layout. Extended the existing valid and invalid submission scenarios to use nested sections/rows/groups, including exclusion of a field removed by a nested Clear. Extended the existing provider-isolation unit case to include layout containers. Ordinary flat forms remain covered by the original rendering and factory scenarios. Dashboard now has 26 HTTP cases and 10 unit cases.

Verification: the initial integration build caught an incorrect ConfigureForms test setup, which was corrected. The full Release solution build with --no-restore -m:1 passed with 12 existing warnings and one new test nullability warning. That warning was corrected, and the subsequent integration Release build passed with zero warnings/errors. The CI command `dotnet test --solution StellarAdmin.slnx --no-build --configuration Release --minimum-expected-tests 1` passed all 178 tests across four assemblies. The solution runner ran outside the sandbox for its local IPC socket. CSharpier formatted the eight touched C# files and git diff --check passed.

Chromium checks at 1440×1000 and 390×1000 verified columns side by side on desktop and stacked on mobile, no horizontal overflow, invalid POST errors, and a successful browser submission followed by the index showing the new item. Screenshots were visually reviewed at /tmp/resource-layout-1440.png and /tmp/resource-layout-390.png. The agent-owned playground on port 5206 and browser were stopped. Port 5205 was untouched. Changes are uncommitted. Edit/delete is next, followed immediately by action-specific view models.


## Step 5a: edit — 2026-09-20

Implemented edit only. Changes remain uncommitted for review. Delete has not been started.

### Configuration and data source

`resource.UseKey(product => product.Id)` selects a direct readable property. Its value is formatted with invariant culture for URLs. `resource.Edit(edit => edit.Fields(...))` uses the same field, section, row, and group builders as Create, with independent configuration. Shared form settings now live in ResourceFormOptions, inherited by ResourceCreateOptions to retain its factory. ResourceFieldsBuilder accepts a configuration callback and no longer assumes the create action.

IResourceDataSource now includes `FindAsync(string id, CancellationToken)` and `UpdateAsync(string id, TResource resource, CancellationToken)`. Lookup returns null for a missing record. Update returns false when the target no longer exists. Lookup results must remain unpersisted until a successful update. The in-memory sample returns a copy and replaces the stored record under its lock only when saving. Keys are explicit single properties in this increment. Composite-key configuration and optimistic concurrency handling are not implemented.

### Controller and rendering

Index rows have Edit links when a key selector is configured. Edit GET loads existing values. Edit POST loads a fresh instance, binds only configured edit fields, excludes the key property, validates, persists, and redirects to Index without the route key. The route id is explicitly bound from route data, preventing query/form values from selecting a different record. Missing records return 404 on GET and POST, including a record that disappears before update. POST requires antiforgery validation. Authorization remains controlled by the host configuration. No built-in Dashboard authorization policy was introduced in this increment.

Global EditTitle, EditSubmitLabel, and IndexEditLabel callbacks default to `Edit {SingularLabel}`, `Save {SingularLabel}`, and `Edit`. Edit Title/SubmitLabel and Index EditLabel override those defaults. Applications can provide Edit.cshtml through the existing resource view lookup, falling back to ResourceEdit.cshtml and the shared form renderer.

DashboardPlayground registers Product's Id key and a separate edit section/row with Name and Price. Create retains its existing factory and layout.

### Verification

Added edit HTTP scenarios for index links, existing values, nested layout, default/global/resource labels, successful updates, unconfigured field preservation, forged route/key values, required/range/conversion validation redisplay, antiforgery rejection, malformed/missing keys, disappearance before save, and an application Edit.cshtml override with a string key. Shared antiforgery form preparation was extracted from ResourceCreateTests. Added one unit test rejecting a computed key selector. Existing create scenarios still pass.

`dotnet build StellarAdmin.slnx --configuration Release --no-restore -m:1` passed. The final incremental build reported one existing unresolved XML cref warning in ViewDataKeys.cs and no errors. The initial full build reported 12 existing warnings. A project build without `-m:1` exited unsuccessfully without diagnostics, and the serial build passed. `dotnet test --solution StellarAdmin.slnx --no-build --configuration Release --minimum-expected-tests 1` passed all 194 tests, including 41 Dashboard HTTP cases and 11 Dashboard unit cases. The solution runner required execution outside the sandbox for its local IPC sockets. An initial HTTP run had three selector assertions counting ASP.NET Core's hidden input. Restricting those assertions to Entity fields resolved them. CSharpier and `git diff --check` passed.

Chromium verified index edit links, prefilled values, desktop/mobile layout at 1440×1000 and 390×1000, no horizontal overflow, invalid submission errors, successful browser save, index redirect, and reloading persisted values. Screenshots were visually inspected at /tmp/resource-edit-1440.png and /tmp/resource-edit-390.png. The agent-owned browser and playground on port 5206 were stopped. Port 5205 was untouched.

### Edit button review correction — 2026-09-20

Restored the pre-redesign index edit button from commit 97691a8: Outline variant, IconSmall size, square-pen icon, and an untitled action column. The configurable edit label now supplies aria-label and title. The initial text-only ghost button was an unintended presentation change. Updated the existing label assertion to verify the accessible name. The Dashboard integration project Release build passed with the existing ViewDataKeys XML cref warning, all 41 HTTP tests passed, and CSharpier and git diff checks passed. No new browser check was performed for this markup-only correction.

### StellarAdmin edit tooltip — 2026-09-20

Replaced the native title tooltip with sa-tooltip, linked through interestfor with a unique ID per rendered row. The button retains aria-label. Both use the configured IndexEditLabel, including the user’s updated default of `Edit {SingularLabel}`. Updated the existing HTTP scenario to verify tooltip association, configured content, and absence of title. The final Dashboard integration Release build passed with no warnings or errors, and all 41 HTTP tests passed. No new browser check was performed for this change.

The tooltip ID now uses `--resource-edit-{rowId}` instead of a random GUID, as requested. The row key is resolved once and reused for the edit route and tooltip ID. The Dashboard integration Release build and all 41 HTTP tests passed after this correction.

## Step 5b: delete implementation — 2026-09-20

Added `IResourceDataSource<TResource>.DeleteAsync(id, cancellationToken)`, returning false for a missing resource. The shared controller accepts an antiforgery-protected POST, takes the key only from the route, returns 404 for missing resources or unconfigured keys, and redirects to the index with the route id cleared. Host authorization remains the application's responsibility, as with the existing actions.

The index now offers an outline trash icon button beside edit, with a StellarAdmin tooltip identified by the resource key. It reuses the existing shared StellarAdmin alert dialog for confirmation. Cancelling leaves the resource untouched. Confirming submits the row's normal POST form. No HTMX dependency was introduced. This increment exposes deletion from the index. The retained form-page delete affordance is not wired into edit.

Global delegate defaults cover `DeleteTitle`, `DeleteMessage`, `DeleteConfirmLabel`, `DeleteCancelLabel`, and `IndexDeleteLabel`. Resource overrides are available through `resource.Delete(...)` and `resource.Index(index => index.DeleteLabel = ...)`. The existing hardcoded-text audit remains deferred until after the rebuild sequence.

Updated active sample and test data sources. The sample now handles creating a resource after deleting every existing item. Added ten integration cases covering label defaults and overrides, rendered confirmation, route-key protection, successful deletion and redirect, missing records including disappearance after rendering, GET rejection, missing antiforgery tokens, and resources without keys. Existing create and edit scenarios remain intact.

Verification: the final Release solution build with `--no-restore -m:1` passed with one existing XML documentation warning in `ViewDataKeys`. The solution test command `dotnet test --solution StellarAdmin.slnx --no-build --configuration Release --minimum-expected-tests 1` passed all 204 cases, including 51 Dashboard HTTP cases. CSharpier formatted the touched C# files and `git diff --check` passed. An isolated Chromium session exercised the sample at desktop and mobile widths, checked confirmation and cancellation, deleted all three sample records, and successfully created a resource afterward. The agent-owned sample server on port 5206 and browser were stopped. Changes are uncommitted. Action-specific view models are next after review.


## Step 6a: operation results — 2026-09-20

Implemented the first agreed checkpoint without introducing action-specific models or handlers. CreateAsync, UpdateAsync, and DeleteAsync now return ResourceOperationResult with Success(), NotFound(), or ValidationFailed(...) outcomes. ResourceValidationError carries a model property name and message, with null representing an operation-level error. Result construction snapshots errors and rejects empty error collections or blank messages. Expected failures must not persist changes. Unexpected exceptions remain exceptions.

The shared controller maps configured field errors to the Entity binding prefix, redisplays create/edit with attempted values, and puts general errors and errors for unrendered fields into the summary. Delete validation failures reload the index in the same response and show all messages in its summary. This preserves ModelState without TempData. Refreshing that POST can resubmit the delete. Missing-resource results return 404, while successful operations retain their existing redirects. Applications overriding the full index view can render the errors through the standard MVC validation summary.

Updated all active sample and fixture data sources to the new return contract. Added three HTTP scenarios covering create/edit value preservation, field and general errors, unrendered-field fallback, unchanged persistence after rejection, and rejected deletion with encoded messages and the record still visible. Existing success, not-found, model-validation, antiforgery, and view-override scenarios remain passing. No duplicate unit suite was added.

Verification: the final Release solution build (`dotnet build StellarAdmin.slnx --configuration Release --no-restore -m:1`) passed with one existing unresolved XML cref warning in ViewDataKeys. The initial full build reported 12 existing warnings. `dotnet test --solution StellarAdmin.slnx --no-build --configuration Release --minimum-expected-tests 1` passed all 207 tests, including 54 Dashboard HTTP scenarios. The runner initially failed under the sandbox's IPC socket restrictions and passed with escalated execution. CSharpier and `git diff --check` passed. No browser check was performed. Changes remain uncommitted.

Next: review the data source contract for unused operations before implementing custom create. Custom model and handler pairing is agreed, but interface composition and internal adapter details remain proposals.

## Step 6b–d: split contracts and custom create — 2026-09-20

Implemented the reviewed interface composition: IResourceDataSource supplies ListAsync, IResourceCreateHandler supplies CreateAsync, IResourceEditHandler supplies FindAsync and UpdateAsync, and IResourceDeleteHandler supplies DeleteAsync. IResourceCrudDataSource combines them for ordinary CRUD. Migrated active sample and fixture sources. Detached EF/Identity projects remain untouched.

Added Create<TModel, THandler>() and its callback overload. The model and compatible handler are required together. The shared controller binds the selected model, applies its data annotations, maps operation-result errors, and retains antiforgery and view-override behavior. A typed delegate resolves the handler from request services and dispatches the model. No separate controller or general dispatch framework was added. Explicit handlers take precedence over source capabilities regardless of data source registration order. Concrete handler registrations preserve application-selected lifetimes and otherwise default to scoped.

The initial implementation below was superseded by the explicit action registration follow-up on 2026-09-21. Create options have a common form base and a typed factory implementation. Repeated configuration for one model composes. Selecting a different model clears fields and factory while retaining page labels and section layout. Ordinary Create selects TResource and removes the explicit handler. Retained builders reject configuration if their model no longer matches the selected model. Unsupported actions are hidden on the index and reject direct requests. Final options validation catches configured create/edit/delete actions without the required implementation.

DashboardPlayground retains Product as ordinary CRUD and adds Customer with CreateCustomerModel and CreateCustomerHandler, while editing Customer through the data source. Password and confirmation demonstrate model-only fields and validation. They are not persisted and this sample does not create authentication accounts. Duplicate emails demonstrate handler-level field errors. CustomerDataSource implements listing, edit, and delete without an unused CreateAsync method.

Added HTTP scenarios covering typed fields/layouts, factory initialization without a parameterless constructor, repeated custom configuration, handler precedence, registration order, allowlisted binding, model and handler validation, password-free redisplay, ordinary editing with custom create, antiforgery, and unsupported actions. Added only three focused unit cases for invalid action configuration. Existing scenarios continue to cover ordinary CRUD and overrides.

Review checkpoint: custom create is ready for review. Custom edit models remain unimplemented until this approach is reviewed. Next is Create's reviewed counterpart, Edit<TModel, THandler>(), including typed loading and route-key protection.

Verification: Release solution build passed with one existing unresolved XML cref warning in ViewDataKeys. The final `dotnet test --solution StellarAdmin.slnx --configuration Release --minimum-expected-tests 1` passed all 221 tests, including 65 Dashboard HTTP scenarios. Tests required execution outside the sandbox for local IPC. CSharpier formatted changed C# and `git diff --check` passed. An isolated Chromium session verified desktop/mobile layout, confirmation validation, duplicate-email rejection, empty password values on redisplay, successful creation, and ordinary editing. Desktop and mobile screenshots were inspected. The agent-owned browser and playground on port 5206 were stopped. Changes remain uncommitted.


## Explicit action registration — 2026-09-21

Following review, create, edit, and delete now require explicit registration. Their options remain null until the corresponding builder method is called. Removed CreateConfigured, EditConfigured, DeleteConfigured, and SelectCreateModel. Each action registration creates fresh options when the options pipeline executes; repeated registrations discard earlier action configuration, including labels, layout, fields, factory, and custom create handler. Setters on the returned action builder still compose. Added no-callback Edit() and Delete() overloads, matching Create(). No model-switch preservation remains.

Controller UI and request guards require the action's options. Handler implementation alone no longer exposes write actions. Existing options validation still rejects registered actions without a matching implementation. Playground resources explicitly register deletion. Updated scenarios to configure a form through one action builder, and extended HTTP coverage to verify unregistered actions with both read-only and full CRUD sources, as well as repeated ordinary/custom create registration replacing the prior form. Custom edit remains deferred pending review.

Verification: Release solution build passed with the existing unresolved XML cref warning in ViewDataKeys. The final `dotnet test --solution StellarAdmin.slnx --configuration Release --minimum-expected-tests 1` passed all 227 tests, including 71 Dashboard HTTP scenarios. The first test run identified two view-override fixtures relying on implicitly enabled actions; both now explicitly register their forms. Tests required local IPC access outside the sandbox. CSharpier and `git diff --check` passed. No new browser check was performed for this configuration follow-up. Changes remain uncommitted.

## Action opt-in naming — 2026-09-21

Renamed resource builder registration methods to AllowCreate, AllowEdit, and AllowDelete to make action enablement explicit. All callback and no-callback overloads, including custom create model/handler pairing, retain their existing behavior and return types. Updated the playground, existing tests, current consumer guidance, and builder conventions. This supersedes the earlier bare-noun action names. Custom edit remains deferred pending review.

Verification: Release solution build with `--no-restore -m:1` passed with 12 existing warnings and no errors. `dotnet test --solution StellarAdmin.slnx --no-build --configuration Release --minimum-expected-tests 1` passed all 227 tests, including 71 Dashboard HTTP scenarios. Tests ran with local IPC access outside the sandbox. CSharpier formatted the touched C# files and `git diff --check` passed. Changes remain uncommitted.


## Step 6e: custom edit models — 2026-09-21

Implemented AllowEdit<TModel, THandler>() and its callback overload, requiring IResourceEditHandler<TModel>. Typed edit options and the shared controller use the selected model for loading, allowlisted binding, validation, and saving. FindAsync supplies a detached model on GET and POST, including models without a parameterless constructor. Explicit handlers resolve from request services and take precedence over source implementations regardless of registration order. Existing concrete registrations retain their lifetime, with scoped as the default. Ordinary AllowEdit continues using TResource and the data source.

Each edit registration replaces prior fields, layout, labels, and handler selection. The route ID remains authoritative for lookup and update. Ordinary resource-model edits retain the existing key-property binding exclusion. Custom model fields belong to that model, so resource key property names do not implicitly exclude unrelated fields. Existing view overrides, labels, antiforgery checks, and operation-result error handling remain shared.

DashboardPlayground now demonstrates separate CreateCustomerModel and EditCustomerModel handlers. The edit model maps DisplayName to Customer.Name and preserves duplicate-email field validation. CustomerDataSource no longer implements the edit handler interface, while Product remains the ordinary CRUD example. Updated current development and consumer guidance.

Added 11 HTTP scenarios for typed loading and layouts, custom-handler precedence with and without a source edit implementation, configured-field binding and route identity, annotation and handler errors without persistence, missing records on GET/POST and during update, antiforgery rejection, and repeated custom/ordinary edit registration. No new unit tests were added.

Verification: Release solution build with --no-restore -m:1 passed. The initial build reported 12 existing warnings and the incremental build reported the existing ViewDataKeys XML cref warning. The solution test command passed all 238 tests, including 82 Dashboard HTTP scenarios. The runner required local IPC access outside the sandbox. An isolated Chromium session verified the Customer edit screen at desktop and mobile widths, creation through the separate create model, duplicate-email rejection with preserved values, successful update, and reloading saved values. Screenshots were inspected. CSharpier and git diff --check passed. Changes remain uncommitted. This completes the custom edit checkpoint for review.


## Index design plan update — 2026-09-21

Recorded the complete index API sketch and the agreed data-source execution boundary. Search/scope expression overloads are deferred to future EF integration. Paging, sorting, searching, and scopes have separate implementation/review checkpoints. Custom edit is committed as c13fe5b. This update changes documentation only. Validation: git diff --check. No application builds, tests, or browser checks were run.

## Paging implementation — 2026-09-21

Implemented the paging checkpoint following explicit user authorization. `Index(index => index.EnablePaging(...))` enables paging with a default size of 25 and available sizes 10, 25, 50, and 100. The no-callback overload returns its paging builder. Configuration rejects nonpositive or duplicate sizes and requires the default size to appear in the available sizes. Paging options resolve through the existing options pipeline and retain provider isolation.

`IResourceDataSource<TResource>.ListAsync` now accepts `ResourceListRequest` and returns `ResourceListResult<TResource>` with the requested items and a long total count before paging. A null `Paging` requests all matching resources. Active sample and test sources have been migrated. Sources own ordering and slicing. The in-memory samples use Id ordering, and the Product playground has 37 seeded rows with 10/25/50 page-size choices. Customer remains an example with paging disabled. EF and Identity remain detached.

The controller defaults missing values, falls back to the configured size for unsupported numeric sizes, ignores disabled paging parameters, and rejects malformed values, nonpositive pages, and offsets exceeding Int32.MaxValue with 400. Page numbers are one-based. There is no additional hardcoded page-size cap beyond the configured allowlist. Out-of-range pages redirect down to the last available page, or page 1 for no records. Redirects only reduce the page number, preventing a redirect cycle if records disappear concurrently. Delete forms preserve page and size. A rejected delete retains its validation summary and can reload the adjusted page once without redirecting.

The index reuses the existing data-grid pager for navigation, totals, and page-size selection. Page-size changes return to page 1. Its existing wording, including record-count and page-size labels, belongs to the deferred hardcoded-text audit. No new wording was added in this increment. Dashboard-specific footer CSS lets controls wrap on narrow screens while keeping the record range on one line.

Verification: Release solution build passed with existing warnings. `dotnet test --solution StellarAdmin.slnx --configuration Release --minimum-expected-tests 1` passed all 263 tests, including 102 Dashboard HTTP scenarios. New paging scenarios cover defaults, disabled paging, selected and unsupported sizes, stable ordering, totals, empty and single-page results, invalid inputs, out-of-range redirects, and successful/rejected delete navigation. Configuration validation and provider isolation are covered by the existing builder suite. Builds and tests required unsandboxed local IPC. Dashboard CSS was built directly and its output inspected. Chromium checks on an agent-owned playground at port 5206 exercised next-page navigation, changing size back to page 1, and the final page at 1440px and 390px widths. Screenshots: `/tmp/resource-paging-desktop.png` and `/tmp/resource-paging-mobile.png`. Port 5205 was untouched.

Pause for paging review before starting sorting. No commit or push was requested.

## Paging binding review — 2026-09-22

Retained normal MVC query model binding and removed paging's ModelState mutations. Binding errors return 400 even when paging is disabled. Successfully bound paging values are ignored when the feature is disabled. This supersedes the earlier policy of ignoring malformed disabled paging parameters. Defaults and allowlist normalization remain unchanged.

Verification: all 104 Dashboard HTTP scenarios passed through `dotnet run --project tests/StellarAdmin.Dashboard.IntegrationTests --configuration Release`, including disabled paging with valid values and binding errors. The runner required unsandboxed local IPC. CSharpier and git diff --check passed. No commit or push.

Moved the binding-error checks to the start of the Index and Delete controller actions following review. TryCreateListRequest now handles only paging defaults and constraints and does not access ModelState. Reverification on 2026-09-22: all 104 Dashboard HTTP scenarios passed. CSharpier and git diff --check passed.


## HTMX index restoration — 2026-09-22

Restored the previous implementation's redirect-and-select approach after review. Paging and page-size links use HTMX to select and replace `#index-page-data-grid`; edit navigation remains ordinary navigation. Delete forms use the existing asynchronous StellarAdmin confirmation dialog, retain antiforgery tokens, and suppress pushing the delete URL. The controller is unchanged: HTMX follows its existing redirect and extracts the grid from the resulting index page. Validation errors sit inside the grid wrapper so rejected deletes show their errors during the swap.

Removed the proposed direct-fragment response, additional controller HTMX checks, `afterDelete` flag, new wrapper partial, and tests specific to that discarded design. Full-page view resolution and the existing grid partial remain unchanged. The user approved this checkpoint for commit. Sorting is next. Verification for this simplified version is recorded below.

Verification of the simplified implementation: Dashboard integration and playground builds passed; all 104 existing Dashboard HTTP scenarios passed; `git diff --check` passed. Chromium verified paging, page-size selection, back/forward history, cancellation, repeated confirmed deletion, and refreshing an emptied last page through the existing redirects without reloading the document. As in the previous interaction pattern, deletion does not change browser history: when the last page disappears the grid shows the preceding page while the address retains the requested page. The commit contains only the three index Razor views and plan documentation. Commit authorized on 2026-09-22; no push requested.


## Sorting implementation — 2026-09-22

Added column.Sortable, Index.DefaultSortBy, and Index.DefaultSortByDescending. Defaults must select a configured sortable column. ResourceListRequest.Sort carries a ResourceSort with the canonical field name and Ascending/Descending direction; null leaves ordering to the data source. The Product playground demonstrates sortable Name and Price columns with a Name default. Its data source orders before paging and uses Id to break ties.

Reused the existing sa-data-grid-sort component for header links, icons, and aria-sort. Links inherit the existing HTMX grid behavior. Sort changes reset the page and preserve page size; pagination, page-size changes, successful/rejected deletion, and out-of-range redirects preserve sorting. SortBy matches configured sortable fields case-insensitively. Unknown or nonsortable fields fall back to the configured default. SortDirection accepts asc/desc case-insensitively; missing direction means ascending for an explicitly selected field, and invalid directions return 400. MVC binds the query strings normally; no ModelState mutation or custom binder was introduced.

Verification: the Debug solution build with --no-restore -m:1 passed with existing warnings. All 278 solution tests passed, including 116 Dashboard HTTP scenarios. Added 12 sorting HTTP scenarios and one builder configuration-validation test. The test runner required local IPC access outside the sandbox. Chromium verified ascending/descending toggles, page reset, paging, back navigation, deletion, and mobile sorting without reloading the document. The agent-owned playground on port 5206 and browser were stopped; port 5205 was untouched. CSharpier and git diff --check passed. Sorting was subsequently committed as `05af889`. Pause before search.


## Explicit index query preservation — 2026-09-22

Restored the previous implementation's query-preservation pattern after reviewing master. The grid uses the existing preserve-query attributes, sorting excludes page, and the pager receives a selected page size only when supplied by the user. The index view model carries the existing ResourceIndexQuery separately from the effective paging and sorting state. Delete forms, successful delete redirects, and out-of-range redirects preserve explicit choices without adding configured defaults. No new query framework or changes to data-source execution were introduced.

Verification: the Dashboard integration project build passed with the existing ViewDataKeys XML documentation warning. All 120 Dashboard HTTP scenarios passed, including four new regression cases for default navigation, successful/rejected deletion, and out-of-range redirects; existing explicit-selection scenarios still pass. The test runner required local IPC access outside the sandbox. CSharpier and git diff --check passed. Browser checks were not repeated for this URL-generation change. Changes are uncommitted.


## Search implementation — 2026-09-22

Added EnableSearch() and its callback overload, ResourceSearchOptions/ResourceSearchBuilder, and nullable Search on the MVC query and data-source request. Search is opt-in; the controller trims text and treats blank or disabled search as null. Sources own matching and filter before counting and paging. DashboardPlayground enables Product search with case-insensitive name matching. The retained ResourceIndexSearchViewModel supplies the input. Placeholder text comes from the global IndexSearchPlaceholder delegate unless locally overridden.

Reused the previous branch's HTMX input pattern: input changed delay:400ms, grid target/select, and pushed history. Search resets paging and retains explicit sort/page-size choices. Grid navigation refreshes the search header through hx-select-oob so later searches use the latest choices. Paging/sorting, successful/rejected deletion, and out-of-range redirects retain search. No controller HTMX branching or new query framework was added.

Verification: Dashboard integration and playground builds passed (the Dashboard build retained the existing ViewDataKeys XML documentation warning). All 131 Dashboard HTTP scenarios passed, including 11 new search cases covering filtering/counting/paging, blank terms, no matches, disabled search, placeholder defaults/overrides, query preservation, deletion, and out-of-range redirects. An initial test assertion incorrectly expected no table cell for the existing empty state; corrected it to expect the rendered empty-state text. Chromium verified live search, paging, sort then search, clearing, browser back, mobile search, and deleting filtered rows without a full document reload. Default query parameters remained omitted. The agent-owned playground and browser were stopped; port 5205 was untouched. CSharpier and git diff --check passed. Changes are uncommitted; pause for review before scopes.

## Step 7 scopes — 2026-09-22

Search was committed as `9bb3e2e`. Added EnableScopes and its callback overload, named scope definitions with an optional DefaultScope, and Scope on the MVC query and data-source request. Identifiers match case-insensitively and are passed in their configured spelling. Omitted or unknown identifiers use the configured default, or null when no default exists. Disabled scopes are ignored. Options validation rejects duplicate identifiers and a default that does not identify a configured scope.

Reused ResourceIndexScopeViewModel and the previous implementation's boosted tab links. Search refreshes the tabs out of band, and grid navigation refreshes both tabs and the search input to retain current selections. Scope changes reset paging and preserve search and explicit sorting/page size. The default tab omits the scope parameter. Delete and out-of-range redirects preserve scope. No custom HTMX response branches, query framework, or new JavaScript were introduced.

DashboardPlayground demonstrates All products, Under 50, and 50 and over. Its data source applies the selected price filter alongside name search before counting and paging. Updated the handwritten consumer setup reference. EF/Identity remain detached and unchanged.

Verification: Dashboard integration and playground builds passed, with only the existing ViewDataKeys XML documentation warning where Dashboard rebuilt. All 141 Dashboard integration scenarios and 22 Dashboard configuration tests passed. The active solution test command subsequently passed all 305 tests. Ten new HTTP cases cover default/unknown/case-insensitive/disabled scopes, combined filtering and totals, page reset, query preservation, clearing, delete success/rejection, and out-of-range redirects. Two configuration cases cover duplicate identifiers and a missing default target. Chromium verified tabs, combined search, paging, sorting, clearing, default URLs, browser back, mobile overflow, and deletion without a full document reload. Initial browser-script attempts failed on a malformed CSS selector, which was corrected before the successful run. Agent-owned sample and browser processes were stopped, and port 5205 was untouched. CSharpier and git diff --check passed. Changes remain uncommitted for review before integration adaptation.

## EF Core checkpoint 1 implementation — 2026-09-22

Implemented AddEfCoreResource<TContext, TEntity> and dedicated resource/index builders using composition over the existing shared builders. The current public surface is labels, index title/columns, default sorting, and paging. Action methods and labels arrive with the CRUD checkpoint rather than exposing unusable handlers now. UseDataSource and UseKey are absent. Existing friend-assembly access was sufficient, so no shared builder API or controller changes were needed. No EF expression options are introduced until checkpoint 2.

The scoped EF data source uses no-tracking queries, global filters, count-before-paging, expression-based column ordering, primary-key tie-breaking, and asynchronous database materialization. Shared options derive the primary key from EF metadata when resolved. Metadata inspection creates and disposes its own DI scope so cached options do not retain a scoped DbContext. Supported key shapes remain a single public CLR int, long, Guid, or string property. The generic controller and existing views handle routing, normalization, redirects, rendering, and HTMX.

DashboardPlayground Product now uses ProductDbContext with a separate in-memory SQLite database and the same 37 seed records. It resets at app startup and leaves the existing Identity database untouched. Two-decimal prices map to integer cents for database ordering. Product is read-only for this checkpoint and temporarily omits search/scopes. Customer remains on the existing in-memory custom-model handlers. Removed the obsolete ProductDataSource.

Reattached the EF library and a new EF Core integration test project to the solution. CI discovers its tests through the existing solution command. Release verification now packs and validates EF alongside the other three packages, while publishing remains limited to Core and TagHelpers. Removed the obsolete EF controller, Razor views, default options, and reference implementation that depended on deleted core APIs.

Reference follow-up inventory: the old AddReference API linked a scalar FK, navigation, and display selector. It replaced displayed FK values and ordering with target labels, included navigations, loaded selectable records with optional query transformation and sorting, retained unavailable current selections as disabled choices, and rejected invalid/required selections during binding. Its metadata validation checked compatible FK/navigation relationships. These capabilities are intentionally not implemented in this read-only checkpoint and require explicit API review before restoration. The old integration also offered per-resource authorization and sidebar registration, which remain parity decisions for closeout rather than silently returning in this increment. Historical source is available in ffb2059.

Verification: the EF library and DashboardPlayground built successfully. The active solution test command passed all 316 tests, including 11 new SQLite HTTP scenarios covering nonconventional metadata keys, global filters, unpaged results, disabled forms, invalid paging, empty results, out-of-range redirects, default/requested sorting, stable ties, and SQL count/order/limit execution. The first run exposed an incorrect test assertion against the document title, corrected to the visible page heading. SQLite test dependencies were aligned to the playground's 10.0.11 provider after 10.0.10 surfaced a native SQLite vulnerability warning. The final solution run no longer reports that warning. The existing unrelated ViewDataKeys XML documentation warning remains.

Chromium verified the seeded catalog, totals, paging, ascending/descending price sorting, omission of default query values, disabled action/search controls, mobile width, and retained document state during HTMX navigation. An initial browser check selected a paging link containing sortBy instead of the column header and was corrected by restricting the selector to header links. Agent-owned server and browser processes were stopped. Port 5205 was untouched. CSharpier and git diff --check passed. A Release EF package was successfully generated under /tmp/stellar-ef-pack. The hosted GitHub workflow and package validator were not run. No commit or push is included. Pause for review before expression configuration.


## EF builder and options design revision — 2026-09-22

Checkpoint 1 is committed as `ffb6f85`. This revision records the proposed structure following review; no runtime changes are made or authorized by this plan entry. It supersedes the earlier forwarding-builder design and separate EfCoreResourceOptions containing parallel search, scope, and sorting mappings.

### One configuration graph

Keep ResourceOptions<TEntity> as the single options-pipeline root used by the controller and EF data source. Its index contains the actual configured feature objects, including specialized EF objects where required. Do not register an independent EF options graph or copy shared settings between options instances. Inheritance of an options type does not automatically make IOptions<Base> and IOptions<Derived> resolve the same instance; avoid that problem by configuring only the existing root.

Make the existing feature options extensible where needed. EfCoreResourceSearchOptions<TEntity> derives from ResourceSearchOptions and adds the predicate factory beside the inherited placeholder. EfCoreResourceScopeOptions<TEntity> derives from ResourceScopeOptions and adds the optional predicate beside the inherited ID and title. The existing scopes collection stores those same scope objects, and DefaultScope still refers to that collection. Unfiltered scopes have a null predicate. EF reads the selected scope's predicate from the selected entry; there is no second scope dictionary to synchronize.

For sorting, keep the selected/default column and direction in the existing shared configuration. Ordinary EF sorting already uses the column's FieldExpression. Store an optional override expression on that column's configuration rather than in a separate mapping. During the sorting checkpoint, determine whether a shared SortExpression property is appropriate as provider-neutral expression metadata or whether the column needs an EF subtype. Do not create a second columns collection or overload DisplayExpression with a sorting-only meaning. Preserve typed selector capture without compiling expressions for database filtering or ordering.

Index-level TransformQuery needs an owner too: propose EfCoreResourceIndexOptions<TEntity> deriving from ResourceIndexOptions and stored as ResourceOptions<TEntity>.Index. The EF registration selects that index type through the normal deferred options pipeline before its index callbacks run. Shared configuration accesses the base properties on that same object; the EF source accesses the specialized index and feature types. No hidden Index property, duplicate root registration, repeated model-switch preservation, or controller EF dependency. Establish the index once for the EF registration rather than replacing it on every Index callback. Repeated feature enabling continues to replace that whole feature, including its expressions.

### Shared builder implementation

Extract ResourceBuilderBase<TEntity, TBuilder> and ResourceIndexBuilderBase<TEntity, TBuilder> for shared configuration methods, using the concrete builder type parameter for fluent returns. Constructors remain internal and builders register Configure actions. Keep Index callbacks on the concrete resource builders so normal and EF callbacks receive their own index API without an additional index-builder generic parameter. Keep UseDataSource and UseKey exclusively on the normal concrete resource builder. Put provider-specific search and scope entry points on the respective concrete index builders; reuse shared leaf builders for settings such as the placeholder and paging. Existing action configuration moves to the shared implementation without enabling EF CRUD before its checkpoint.

### Revised next steps

1. Refactor only the shared resource/index builder implementation and prove the existing normal and EF public APIs still compile and work. Do not add expression features in this increment. Review the resulting base classes before continuing.
2. Prove the options design with EF search: add the specialized index/search options under the existing root, implement the agreed expression overload, and demonstrate shared placeholder rendering plus database filtering/counting/paging from the same configuration. Cover this through the EF HTTP scenarios and retain normal resource behavior. Verify repeated registration/configuration and service-provider isolation without creating an options-only test suite.
3. Add scopes with predicates on the scope entries, then sort overrides on column configuration and query transformations on the specialized index, reviewing each small increment. Preserve the previously agreed public API unless the concrete column design needs a separate review. Query execution remains transformations, scope/search, count, stable sorting, paging, and asynchronous materialization.
4. Continue the existing EF CRUD checkpoint through shared action handlers, followed by integration closeout and the deferred Identity adaptation. Existing reference, authorization, sidebar, and hardcoded-text follow-ups remain deferred.

Validation for this planning revision: inspected the current shared/EF builders and options; git diff --check passed. No runtime files changed, and no builds or tests were run. The earlier 316 passing tests describe checkpoint 1, not an implementation of this proposal.


## Shared builder bases implementation — 2026-09-22

Implemented the first increment of the revised design. ResourceBuilderBase<TResource, TBuilder> owns labels and shared action configuration; ResourceIndexBuilderBase<TResource, TBuilder> owns page/action labels, columns, default sorting, and paging. Concrete builders supply themselves as the fluent return type. Their constructors remain internal, configuration remains deferred through Configure<ResourceOptions<TResource>>, and existing friend-assembly access lets the EF builders use the same service collection without widening constructor access.

Normal and EF resource builders each retain their own typed Index callback. UseDataSource and UseKey remain on the normal builder; parameterless search and scope registration remain on the normal index builder. Removed the EF builders' private shared-builder fields and forwarding methods. EF registration returns its builder using the service collection directly. No options, controllers, data sources, or views changed.

Inherited action methods and index action labels are now available on the EF builder. This does not enable actions by default or implement EF persistence handlers: ordinary actions still require a matching data-source handler under the existing configuration validation. Explicit custom create/edit model-handler pairs use the existing shared configuration path. Consumer guidance now distinguishes shared action registration from the deferred built-in EF CRUD implementation.

Verification: dotnet test --solution StellarAdmin.slnx --minimum-expected-tests 1 passed all 316 tests across five test projects, including the existing normal and EF HTTP scenarios. The command required local build/cache and IPC access outside the sandbox. The existing ViewDataKeys FormFieldOptions XML cref warning remains. CSharpier formatted all seven touched C# files and git diff --check passed. No new tests were added for this implementation-only extraction; no browser checks were repeated. Pause for review before the specialized-options/search increment. No commit or push requested.


## EF Core search implementation — 2026-09-22

Shared builder bases are committed as `0004768`. Implemented the next approved increment: EF EnableSearch requires a Func<string, Expression<Func<TEntity, bool>>> predicate factory. The no-callback overload returns ResourceSearchBuilder<TEntity>, and the callback overload returns the EF index builder. Both use the existing Configure<ResourceOptions<TEntity>> pipeline.

ResourceSearchOptions and ResourceIndexOptions are extensible. EF registration selects EfCoreResourceIndexOptions<TEntity> once before user callbacks, using an internal setter on the shared root's Index. EfCoreResourceSearchOptions<TEntity> stores the predicate factory alongside the inherited placeholder on that same options graph. The index subtype currently adds no members and provides the agreed owner for later query transformations. Repeated EnableSearch replaces the entire search object. Repeated Index callbacks preserve columns and other index settings. No separate EF options root or controller changes were introduced.

The EF data source applies the predicate using IQueryable.Where before counting, ordering, and paging. Existing MVC normalization, shared views, label defaults, and HTMX search behavior remain in use. DashboardPlayground Product now demonstrates name search and a local placeholder. Consumer setup guidance includes the expression API. Scopes, sorting overrides, transformations, and built-in EF CRUD remain subsequent increments.

Verification: dotnet test --solution StellarAdmin.slnx --minimum-expected-tests 1 passed all 322 tests across five projects. Six new cases cover database search/count/paging, blank search, global-filter exclusion, replacement of predicate and placeholder across Index callbacks, filtered out-of-range redirects, and isolation between providers built from one registration. SQL assertions verify filtering in both count and paged queries. DashboardPlayground built successfully with zero warnings or errors. CSharpier and git diff --check passed. Commands required local build/cache and test IPC access outside the sandbox. Browser checks were not repeated because the shared UI and HTMX configuration are unchanged. Changes are uncommitted for review.


## EF Core scopes implementation — 2026-09-22

EF search is committed as `822084b`. Added EnableScopes callback and no-callback overloads to the EF index builder and an EfCoreResourceScopesBuilder<TEntity> with Add(id, title, optional predicate) and DefaultScope. EfCoreResourceScopeOptions<TEntity> derives from the now-extensible shared scope record and holds its predicate beside the existing identifier and title. The shared scopes collection remains the sole configuration source. Repeated EnableScopes replaces the entries and default selection through the existing deferred options pipeline.

The EF data source applies the selected entry's predicate before search, counting, ordering, and paging. A null predicate leaves the query unfiltered by that scope while retaining global query filters. Existing controller normalization, validation, scope tabs, URLs, and HTMX remain unchanged. DashboardPlayground Product demonstrates All products, Under 50, and 50 and over. Updated the consumer setup reference and development guide. Sorting overrides, query transformations, and EF CRUD remain subsequent increments.

Verification: dotnet test --solution StellarAdmin.slnx --minimum-expected-tests 1 passed all 330 tests across five projects. Eight new HTTP cases cover default and unknown scopes, case-insensitive selection, combined search, unfiltered scopes, second-page results, global-filter exclusion, configuration replacement, and filtered out-of-range redirects. SQL assertions verify scope filtering in count and paged queries. The initial run exposed two incorrect test expectations for the existing singular count label and redirect URL, corrected before the passing run. DashboardPlayground built successfully with zero warnings or errors. CSharpier and git diff --check passed. The initial solution build retained the existing unrelated ViewDataKeys XML cref warning. Browser checks were not repeated because shared rendering and HTMX code are unchanged. No commit or push is included. Pause for review before sorting overrides.


## EF Core sorting expressions implementation — 2026-09-22

EF scope predicates are committed as `1ab050b`. Following API review, sorting configuration lives on ResourceColumnBuilder<TResource>. Sortable() enables ordering by the column field, and Sortable<TSort>(selector) enables ordering by a typed expression. These replace the former Sortable setter. ResourceColumnsBuilder returns and accepts the generic builder. Removed the proposed index-level SortBy method and its column lookup validation. DefaultSortBy remains on the index to select initial ordering.

A provider-neutral nullable SortExpression on DataGridColumnOptions stores the override alongside the field. The shared builder captures the expression, and the data source decides how to apply it. EF orders by SortExpression when present, falling back to FieldExpression. Repeated Sortable calls replace the sorting configuration, with the parameterless overload clearing any previous override. Existing default/requested direction handling and primary-key tie-breaking remain unchanged. No controller, view, URL, or HTMX changes were needed.

DashboardPlayground configures name sorting through column.Sortable(product => product.Name.ToLower()) while displaying the original name. Updated active callers and consumer guidance. Query transformations and EF CRUD remain subsequent checkpoints.

Verification: dotnet test --solution StellarAdmin.slnx --minimum-expected-tests 1 passed all 335 tests across five projects. Five new HTTP cases cover default and descending expression ordering, stable paged ties, combined scope/search filtering, replacement of sorting expressions, restoration of field ordering, and original displayed values. SQL assertions verify translated expression ordering before paging. Removed the two tests for the eliminated index-level lookup validation. DashboardPlayground built with zero warnings and errors. The solution build retained the existing unrelated ViewDataKeys XML cref warning. CSharpier and git diff --check passed. Browser checks were not repeated because rendering and HTMX are unchanged. Changes remain uncommitted for review.


## EF Core CRUD implementation — 2026-09-23

Column sorting is committed as `2ed6648`. The user deferred the TransformQuery increment pending a broader data-source event and callback model. Implemented the next CRUD checkpoint through the existing shared `AllowCreate`, `AllowEdit`, and `AllowDelete` API. The EF source now implements `IResourceCrudDataSource<TEntity>`; no new public action builder or controller path was added. DashboardPlayground Product enables all three actions with Name and Price form fields while Customer keeps its custom create/edit models.

Create persists the model supplied by the existing controller factory/binding flow. Edit GET and POST load untracked entities so invalid submission cannot change the DbContext. On successful POST, EF reloads the tracked entity by its supported metadata key and copies only configured editable properties before saving. Delete loads the target through a query that respects global query filters. Invalid or filtered keys yield not-found. Configured EF form fields must be mapped scalar properties with public setters, excluding keys, generated values, and concurrency tokens. EF validates these fields when resource options are finalized, after builder callbacks and alongside metadata key discovery. Invalid action configuration fails when options are resolved, before a form is served. The data source reads edit property metadata only when applying an update. Model validation and antiforgery remain in the shared controller. Only `DbUpdateConcurrencyException` is translated: a missing record yields not-found, while an existing changed record yields a general validation error. Stale browser forms are not detected because the shared form does not carry an original concurrency token. Other database exceptions propagate rather than being silently converted to user input errors. A future callback model can expose explicit interception and application-specific error translation.

Verification: `dotnet test --solution StellarAdmin.slnx --minimum-expected-tests 1` passed all 348 tests across five projects. Thirteen new EF SQLite scenarios cover enabled controls and forms, creation and factory values, model validation, configured-field binding and key protection, detached edit behavior, filtered and malformed keys, deletion, antiforgery, and rejection of primary-key, generated, and concurrency-token form fields. DashboardPlayground built with zero warnings and errors. CSharpier and `git diff --check` passed. The playground was started on agent-owned port 5216; an antiforgery-protected create POST returned 302, and the new Product appeared in the index with its decimal price 12.50. The agent-owned server was stopped, and port 5205 was untouched. Browser checks were not repeated because create/edit/delete rendering and HTMX use the existing shared controller and views. No commit or push requested.

After review, moved EF form-field validation from data-source construction to options finalization beside key discovery. Early options configuration still selects the EF index type; post-configuration now sees the completed action forms. The data source reads edit property metadata only while updating and no longer calls a value-returning method solely to validate create fields. The EF integration suite passed 43 tests, and the full solution passed 348 tests across five projects after this change. CSharpier and `git diff --check` passed; no additional browser check was needed for this options-pipeline change.

Split the protected-field configuration check into three independently named tests for a primary key on create, a concurrency token on edit, and a generated value on edit. The EF integration suite still passes all 43 tests.

## EF-owned object fields — 2026-09-23

Implemented the reviewed owned-object increment before reference fields. Resource field selectors now accept property chains with public getters and a public setter on the final property. Form field names and sortable column identifiers use the full path, such as `Details.Sku`; default sorting can select the same path. The shared controller binds only configured full input names, including their parent prefixes, so a forged sibling value within an owned object remains excluded. EF validates that intermediate properties are owned navigations or complex properties and that the final scalar property is writable, nongenerated, not a key, and not a concurrency token. Editing copies only configured leaf values into the tracked owned object instead of replacing that object. The example uses an initialized required owned object; automatically creating a null intermediate object is outside this increment.

DashboardPlayground Product now has an EF-owned Details object with a SKU configured for index display/sorting and create/edit forms. Six SQLite HTTP scenarios cover default and requested SKU ordering, rendering an existing edit value, create binding with a forged sibling value, edit preservation of an unconfigured sibling and top-level property, and nested-field validation without persistence. Existing reference-choice hooks remain untouched; reference fields are the next separately reviewed increment.

Verification: the Debug solution build passed with 12 existing warnings and no errors. All 354 tests passed across the five active projects (Core 66, TagHelpers 76, Dashboard unit 22, Dashboard HTTP 141, EF SQLite HTTP 49) by running the project executables directly; `dotnet test --solution --no-build` could not start inside the sandbox because its named-pipe IPC bind was denied. CSharpier formatted touched C# files and `git diff --check` passed. No browser check was repeated because the shared HTML and styling are unchanged. No commit or push was requested.

## EF Core reference fields — 2026-09-23

Implemented the reviewed single-record EF reference increment. `EfCoreResourceBuilder.AddReference(foreignKey, navigation, display)` records a single-key dependent navigation. It does not add fields or columns: the existing create/edit field and index column builders still decide where the foreign key appears. EF metadata validation checks that the selectors match the mapped relationship and supported principal key type. EF-specific reference definitions are held in a separate options type containing only those mappings; shared resource labels, actions, forms, and columns remain in the existing `ResourceOptions<TEntity>` graph.

The shared controller now asks an optional `IResourceReferenceLookupProvider<TResource>` for configured fields when rendering create/edit GETs and rejected POSTs. The EF data source supplies sorted reference lookup items for EF entity forms. The database enforces foreign-key constraints when saving. Database errors propagate through the EF data source. Custom action models do not acquire EF lookups by matching a foreign-key name. Configured form fields and index columns default their titles to the navigation name while preserving explicit overrides. Configured index columns show the related label and sort by it unless a column sorting expression explicitly overrides it. The data source includes the navigation when rendering such a column. The shared controller and Razor views remain the only resource page path. Target query filters apply to lookups. This first slice does not add configurable lookup filtering, alternate lookup sources, autocomplete, or preservation of an existing reference hidden by a target query filter. Those need separate API review.

DashboardPlayground now demonstrates an optional Product–Category reference, with a select on create/edit and a label column on the index. The initial seven SQLite HTTP scenarios covered label sorting, sorted select choices, create persistence and forged-field exclusion, unknown-key rejection without persistence, edit selection, edit updates, and rejected edit redisplay. The Debug solution build passed with 12 existing warnings and no errors. All 361 tests passed across five active projects (Core 66, TagHelpers 76, Dashboard unit 22, Dashboard HTTP 141, EF SQLite HTTP 56) by running the project executables directly. CSharpier formatted the touched C# files and `git diff --check` passed. No browser check was repeated because the existing select and grid components are exercised by the HTTP scenarios. No commit or push was requested.

After API review, renamed the optional form-reference provider to `IResourceReferenceLookupProvider<TResource>` with `GetLookupsAsync`, and carried the lookup terminology through the controller and form views. The Debug solution build passed with zero warnings and errors using serial MSBuild. The Dashboard HTTP suite passed 141 tests, and the EF SQLite HTTP suite passed 56 tests. The consumer skill generator reported no drift. No commit or push was requested.

After further review, removed the pre-save reference existence checks from create and edit, including the unused `IsValidAsync` implementation and the two HTTP tests that expected field errors for unknown keys. The database now enforces foreign-key constraints on save, with database errors left for the application's future error-handling integration. The serial Debug solution build passed with one existing XML documentation warning and no errors. The EF SQLite HTTP suite passed 54 tests and the Dashboard HTTP suite passed 141 tests. CSharpier, the consumer skill generator drift check, and `git diff --check` passed. No commit or push was requested.

## Typed resource form editors — 2026-09-23

Added `fields.Add(...).UseEditor<TEditor>(...)` as an explicit editor selection and settings API. `ResourceEditor` names the MVC editor template by its CLR type and can prepare request data before synchronous Razor rendering. The shared controller prepares configured editors for create/edit GETs and rejected POSTs, independent of the resource data source and action model type. Fields without a configured editor still use MVC metadata-based editor template selection. `ReferenceLookupEditor` uses an application-registered `IReferenceLookupProvider`, and the provider supplies fresh select items per request. The shared controller no longer asks the CRUD data source for lookups; `IResourceReferenceLookupProvider<TResource>` and the EF data source's lookup path were removed. EF resource reference metadata no longer changes form field titles. A custom editor template in the Dashboard HTTP test application proves typed settings and application view resolution. EF SQLite HTTP scenarios cover entity and custom-model reference forms, persistence, and lookup reload after validation failure. DashboardPlayground registers a category lookup provider and explicitly selects the reference editor on its Product forms.

The existing EF `AddReference` index-column substitution and navigation loading remain temporarily so the current index continues to work. The user deferred replacing that behavior until EF index query callbacks allow an explicit `Include`. At that point, index columns can select the navigation label directly, and the remaining `AddReference` API can be removed. No index query callback was added in this increment.

Verification: the serial Debug solution build passed; the initial build reported the existing ViewDataKeys XML documentation warning, and the final incremental build reported no warnings or errors. The Dashboard HTTP suite passed 142 tests, the EF SQLite HTTP suite passed 56 tests, and the Dashboard unit suite passed 22 tests by running their executables directly; `dotnet test` could not start inside the sandbox because its named-pipe IPC bind was denied. The consumer skill generator `--check` reported no drift. CSharpier formatted the changed library and test C# files, and `git diff --check` passed. No browser check, commit, or push was requested.

## Remove remaining EF reference mapping — 2026-09-23

At the user's request, removed `AddReference` and its EF reference metadata, index label substitution, and navigation `Include`. A configured `CategoryId` index column now displays and sorts by its scalar value. The Product create/edit forms still select `ReferenceLookupEditor` explicitly; the editor and lookup provider need no resource-level reference registration. Updated DashboardPlayground and the consumer setup example accordingly. EF index query callbacks remain deferred, so a label column backed by a navigation is not demonstrated here. The detached Identity playground still references its obsolete APIs and is outside this increment.

Verification: the serial Debug solution build passed with 12 existing warnings and zero errors. All 56 EF SQLite HTTP integration tests passed, including the CategoryId index and reference editor create/edit scenarios. CSharpier formatted the touched C# files, the consumer skill generator `--check` reported no drift, and `git diff --check` passed. No browser check, commit, or push was requested.

Removed the redundant `FormFieldOptions.Template` property. `UseEditor<TEditor>` now stores only the editor, and the form view reads its `TemplateName` when rendering; fields without a resource editor still pass no explicit template to MVC. The serial Debug solution build passed with 12 existing warnings and zero errors. Dashboard HTTP (142) and EF SQLite HTTP (56) integration tests passed, covering custom editor templates and metadata-selected editors. No new tests were added for this rendering-only cleanup. No commit or push was requested.
