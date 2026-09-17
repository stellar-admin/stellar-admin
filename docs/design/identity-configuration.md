# Design: StellarAdmin Identity configuration

The consumer-facing configuration API for `StellarAdmin.Dashboard.Identity`, following the [options builder conventions](../conventions/options-builders.md) in this repository. **V1 scope is the Users index page and its data grid end-to-end, and it has shipped: column configuration rendered through grid expression binding (`field-for`) with `[Display]` attribute support for column titles, seeded default columns, page settings (`Title`/`Subtitle`), the sidebar group + users item, query transformation, sorting (column `Sortable()` + page `DefaultSortBy`), scopes, and opt-in search (all landed 2026-08-04/05).** The other levels (roles, sidebar icon/order) are designed here so future plans can add them without reshaping the API. **The 2026-08-10 resource layer refactor made the page machinery entity-agnostic — see [Resource layer](#resource-layer) for the vocabulary and the current class names; history sections may still narrate decisions using the old `Users*` names they were made under.**

## Target usage

```csharp
builder.Services.AddStellarAdmin().AddDashboard(dashboard => dashboard.AddIdentity<ApplicationUser, ApplicationRole>(identity =>
{
    identity.ConfigureSidebar(sidebar =>
    {
        sidebar.Title = "User management";
        sidebar.UsersItem(item => item.Label = "Accounts");
        sidebar.RolesItem(item => item.Visible = false);
    });

    identity.ConfigureUsers(users =>
    {
        users.Index(index =>
        {
            index.Title = "Users";
            index.Subtitle = "People who can sign in to your application";
            index.DefaultSortBy(u => u.Email);
            index.TransformQuery(q => q.Include(u => u.Department));
            index.EnableSearch(
                (q, term) => q.Where(u => u.Email!.Contains(term)),
                search => search.Placeholder = "Search e-mail..."
            );
            index.Scopes(scopes =>
            {
                scopes.Add("All");
                scopes.Add("Unconfirmed", u => !u.EmailConfirmed).Default();
            });
            index.Columns(columns =>
            {
                columns.Clear();
                columns.Add(u => u.UserName).Sortable();
                columns.Add(u => u.Email).Title("E-mail address");
                columns.Add(u => u.EmailConfirmed);
                columns.Add(u => u.Department!.Name);   // nested chains bind end-to-end
            });
        });
    });
}));
```

Everything infers from `TUser` — no type annotations inside the lambda. The no-configure overload `AddIdentity<ApplicationUser, ApplicationRole>()` keeps working and yields the defaults.

## Builder surface

The existing empty `StellarAdminIdentityBuilder` becomes generic; the configure delegate runs **eagerly** inside `AddIdentity` (already the case in the current sketch). Eager execution is load-bearing: registration work (controllers, the resource registrations) happens inside `AddIdentity` itself, which a lazy options callback could not do.

```
StellarAdminIdentityBuilder<TUser, TRole, TKey>      // the one root builder — every
├─ ConfigureSidebar(Action<IdentitySidebarBuilder>)  // AddIdentity overload names both
├─ ConfigureUsers(Action<ResourceBuilder<TUser>>)    // entity types (see Roles)
└─ ConfigureRoles(Action<ResourceBuilder<TRole>>)

IdentitySidebarBuilder                 // section-level settings are direct properties
├─ Title, Visible                     (section = today's SidebarGroupItem("Identity"))
├─ UsersItem(Action<SidebarItemBuilder>)
└─ RolesItem(Action<SidebarItemBuilder>)
     └─ Label, Visible
     // Icon + Order (section and item) deferred until the shell model renders them — see Sidebar

ResourceBuilder<TEntity>               // one shared builder serves users and roles
├─ Index(Action<IndexPageBuilder<TEntity>>)
├─ Create(...), Edit(...)              // shipped 2026-08-06 — see identity-user-forms.md
└─ (future) Details(...)

IndexPageBuilder<TEntity>
├─ Title, Subtitle                     // scalar page settings = properties
├─ DefaultPageSize, PageSizeOptions    // paging defaults (25 / 25,50,100); empty options = no selector
├─ Columns(Action<IndexPageColumnsBuilder<TEntity>>)
├─ DefaultSortBy(Expression<Func<TEntity, TProp>>)             // last call wins…
├─ DefaultSortByDescending(Expression<Func<TEntity, TProp>>)   // …across both methods
├─ EnableSearch(Func<IQueryable<TEntity>, string, IQueryable<TEntity>>,   // opt-in feature gate;
│               Action<IndexPageSearchBuilder<TEntity>>? = null)   // query required — see Search
├─ Scopes(Action<IndexPageScopesBuilder<TEntity>>)
└─ TransformQuery(Func<IQueryable<TEntity>, IQueryable<TEntity>>)   // composes on repeat calls

IndexPageColumnsBuilder<TEntity>
├─ Add<TProp>(Expression<Func<TEntity, TProp>>) → IndexPageColumnBuilder
│    └─ .Title(string) .Template(string) .Format(string) .Sortable()   // EF-style chaining
└─ Clear()                             // drop the seeded defaults; empty is honored literally

IndexPageScopesBuilder<TEntity>
├─ Add(string title, Expression<Func<TEntity, bool>>? predicate = null)
│    → IndexPageScopeBuilder<TEntity>
│         └─ .Default() .Slug(string)  // last Default() wins; slug overrides title-derived value
└─ Clear()

IndexPageSearchBuilder<TEntity>
└─ Placeholder                         // scalar setting = property; the query is EnableSearch's argument
```

`EnableSearch` is the first **nested feature gate** (conventions: `Enable*` opt-in below root — the call gates the search box and the `search` request parameter, not just an option value) and the first use of the **essential-data-as-required-argument** rule: the search query is the signature's required parameter, settings live in the optional configure lambda. `TransformQuery`/`DefaultSortBy(Descending)` were renamed from `Query`/`DefaultSort(Descending)` (2026-08-05) per the leaf-verb naming rule — behavior methods are verb phrases for what they register.

## Options model

Builders are facades over eagerly-built options objects registered as instance singletons (the eager-execution requirement above rules out `services.Configure` callbacks). Since the resource layer refactor there is **one options object per resource plus one shared root**: non-generic `StellarAdminIdentityOptions` holds the sidebar, and `IdentityUsersOptions<TUser, TKey>` (a `ResourceOptions<TUser>`) is its own singleton that `UsersController` injects directly. A non-generic `IdentityResourceRegistration` per resource lets the sidebar treat them uniformly.

The options types are **public** (decision Jerrie, 2026-08-05 — enables constructor injection into `UsersController` and lets consumers who override views read the configuration) but **read-only outside the builders**: scalar settings have `internal` setters, and collections are exposed as `IReadOnlyList` over internal backing lists. Configuration is builder-only — nothing may mutate options at runtime.

Page settings implemented 2026-08-05: `IndexPageBuilder.Title`/`Subtitle` (settable properties per conventions). Title defaults to "Users" (public `EffectiveTitle` — one place, shared by page header and sidebar label cascade); a null subtitle renders no `<sa-page-header-description>` element.

- `StellarAdminIdentityOptions` (non-generic) → `Sidebar` (`IdentitySidebarOptions`: `Title`, `Visible`, `UsersItem` of `SidebarItemOptions` with `Label`/`Visible`; `RolesItem` arrives with the `TRole` overload).
- `IdentityUsersOptions<TUser, TKey> : ResourceOptions<TUser>` → `IndexPage` (`IndexPageOptions<TUser>`: `Title`, `Subtitle`, `Func<IQueryable<TUser>, IQueryable<TUser>>? QueryTransform`, `Columns` and `Scopes` — both seeded/extendable collections, see below — `Search` — a nullable `IndexPageSearchOptions<TUser>`, null = disabled — and `DefaultSort` — a nullable `DataGridDefaultSortOptions` with `FieldExpression`, best-effort `FieldName`, and `Descending`, seeded to ascending `UserName`), plus `CreatePage`/`EditPage` (see identity-user-forms.md). The seed strings live in `IdentityUsersOptions` as `IndexPageDefaults`/`FormPageDefaults` records — the one file in the library that knows the word "user".
- Columns are stored **non-generically** so the view model interface (`IResourceIndexPageViewModel`) can expose them without knowing the entity type:

  ```csharp
  public sealed class DataGridColumnOptions   // class, not record: the chained column
  {                                           // builder mutates it via internal setters
      public LambdaExpression FieldExpression { get; }   // as captured — field-for pass-through
      public string? FieldName { get; }       // extracted at Add() time; null when the body
                                              // is not a member access; leaf name for chains
      public string? Format { get; internal set; }
      public bool Sortable { get; internal set; }
      public string? Template { get; internal set; }
      public string? Title { get; internal set; }        // null = defaulted (see below)
  }
  ```

  Scopes (`IndexPageScopeOptions<TEntity>`: `Title`, `Slug`, `Predicate`, `IsDefault`) are generic (the predicate needs the entity type), so the view model maps them to non-generic `ResourceIndexScopeViewModel` records (slug, title, is-active, query-value) for `IResourceIndexPageViewModel`.

## Columns: semantics (v1 focus)

- **`Add` captures `Expression<Func<TUser, TProp>>`** (generic `TProp`, so no boxing `Convert` nodes) **without validating it** (decision Jerrie, 2026-08-04, superseding the earlier throw-at-configuration-time design). **Nested properties work end-to-end** (grid chain binding landed 2026-08-04, plan `grid-nested-field-binding.md`): `u => u.Department!.Name` renders the leaf value, with metadata (`[Display]`, `[UIHint]`, `[DisplayFormat]`) resolved from the leaf property and a null intermediate rendering as an empty cell. Expression shapes the grid cannot bind (method calls, indexers) fail when the users index page renders, not at startup. `DataGridColumnOptions.FieldName` is best-effort (`null` when the body is not a member access; leaf name for chains).
- **Defaults are seeded as real column definitions** (decision Jerrie, 2026-08-05, superseding both the earlier "defaults live as hardcoded view markup + `AreColumnsDefined` branch" design and its "never add `Clear()`" constraint — seeding makes the defaults *seen*, which is what that constraint protected). The `IndexPageOptions` constructor seeds `UserName`, `Email`, and `EmailConfirmed` — all sortable, each skipped if `TUser` lacks the property — plus the ascending-`UserName` default sort, so paging is stable out of the box. One rendering code path: the view always loops `Model.Columns`; there is no default-markup branch. Per conventions, `Add` **extends** the seeded defaults; a fully custom set starts with `columns.Clear()`, and clearing without adding is honored literally (a grid with no columns). `Clear()` does not touch the default sort.
- **`Title` resolution is the grid's job.** The identity layer passes a title only when `.Title(...)` was called; otherwise the grid's header default chain applies: explicit `title`/header template → `ModelMetadata.DisplayName` (`[Display(Name = ...)]`, buddy-class aware) → PascalCase-split property name (`EmailConfirmed` → "Email Confirmed"). One authoritative source per string.
- **Rendering**: `_IndexDataGrid.cshtml` (the whole-grid partial that `Users/Index.cshtml` renders, and the consumer's markup-override unit — grid and columns must share one file because tag helper `ParentTag` binding is lexical and `context.Items` doesn't cross partial boundaries) loops the column definitions emitting expression-bound fields: `<sa-data-grid-column title="@column.Title" field-for="column.FieldExpression" template="@column.Template" format="@column.Format" sortable="column.Sortable"/>`. The grid `field-for` support is specified in `docs/plans/grid-field-expression-binding.md` and implemented as part of the same effort. The whole field pipeline applies unchanged: `[UIHint]`/buddy-class templates, `[DisplayFormat]`, explicit `template`/`format` overrides — plus metadata on an empty grid and compiled getters, which expression binding enables.
- **`Sortable()` (shipped 2026-08-05)**: chained method marking the column sortable — its header renders a sort link (the grid's `sa-data-grid-sort` machinery), and the index accepts `sortBy`/`sortDir` query parameters. The controller resolves `sortBy` against sortable columns' field names case-insensitively (unknown or non-sortable values are **silently ignored** so bookmarked URLs survive column removal) and sorts by the column's **captured expression** (`Expression.Call` on `Queryable` — nested chains sort server-side with no string-to-expression mapping), applied after the query transform and before count/paging. Expressions the provider cannot translate fail at request time — no configure-time validation, consistent with `Add`. Sorting resets to page 1; page size and other query params carry over. Since the default columns are seeded as real sortable definitions, they sort through the same path — the earlier "no sort links on built-in default columns" deferral is moot.
- **`DefaultSortBy` / `DefaultSortByDescending` (shipped 2026-08-05, renamed from `DefaultSort`/`DefaultSortDescending` per the leaf-verb naming rule)**: page-level methods mirroring LINQ's `OrderBy`/`OrderByDescending` (no direction enum in the configuration API), applied when the request specifies no sort field or one that matches no sortable column. Seeded to ascending `UserName` so paging is stable with zero configuration. **Last call wins** across both methods (a setting, unlike `TransformQuery` which composes). The expression is captured unvalidated and is independent of the columns, so sorting by a non-displayed property works. When its field name resolves to a sortable column, that column renders as the active sort (header arrow, direction toggle); otherwise the sort is data-only.

## Query transformation (`TransformQuery`, shipped 2026-08-05)

`Func<IQueryable<TUser>, IQueryable<TUser>>` — the consumer calls EF Core operators (`Include`, `Where`, ...) from their own EF reference; `StellarAdmin.Dashboard.Identity` gains no EF Core dependency (`Include` returns `IIncludableQueryable<TUser, TProp>`, which is an `IQueryable<TUser>`, so consumer-side operators flow through the plain-LINQ signature). The controller applies it to `userManager.Users` **first** — before the scope predicate, search, sort, `Count()`, and paging — so filters affect totals consistently. Repeat `TransformQuery(...)` calls compose (outer wraps inner) rather than replace; `null` (never called) means no transformation. Stored as `IndexPageOptions.QueryTransform`.

## Scopes (shipped 2026-08-05)

Named, mutually exclusive slices of the user list rendered as tabs above the grid (`sa-tab-list`, line variant). `scopes.Add(title, predicate?)` — an omitted predicate means an unfiltered scope ("All"); the query-string slug is derived from the title (lowercased, non-alphanumerics collapsed to dashes) and overridable via the chained `.Slug(...)` (e.g. to keep URLs stable under localized titles, matched case-insensitively). `.Default()` marks the scope applied when the request carries no scope — last call wins; with no marked scope the first configured one is the default (internal `EffectiveDefaultScope`). An **unknown** requested scope also falls back to the default scope rather than an unfiltered list, so bookmarked URLs keep working when a scope is removed. The predicate is applied after the query transform and before search/sort/count. Tabs render only when scopes are configured; tab links carry the current sort and page size and reset paging.

**No seeded default scopes** (decision Jerrie, 2026-08-05): out of the box the page renders no scope tabs and the list is unfiltered — scopes exist only when configured. (An earlier `Scopes` XML doc claiming seeded `Active`/`Inactive`/`All` scopes was aspirational and has been corrected.)

## Search (shipped 2026-08-05)

**Opt-in via `index.EnableSearch(query, configure?)`** — without the call no search box renders and the `search` request parameter is ignored. The `Func<IQueryable<TUser>, string, IQueryable<TUser>>` query is the **required argument** (compile-time enforced): the filter sent to the database is always an explicit consumer choice that can be backed by the right indexes — there is deliberately no seeded default search, and the previously hardcoded `UserName`/`Email` `Contains` filter is gone. Repeat calls reconfigure search from scratch. `IndexPageSearchOptions<TEntity>` is immutable where it matters (`Query` is get-only via an internal constructor — a half-configured search is unrepresentable); `Placeholder` defaults to "Search..." via public `EffectivePlaceholder`. The controller applies the search term after the scope predicate and before sort/count/paging; the view renders an htmx-boosted input (400 ms debounce) that preserves the active scope and page size and pushes the URL.

## Sidebar (group + users item shipped 2026-08-05)

Section properties + per-item builders as shown above; `SidebarItemsProvider` reads `IdentitySidebarOptions` plus the registered `IdentityResourceRegistration` list (it is **non-generic** since the resource layer refactor — each registration carries a `Func<string>` so the label cascade still reaches its resource's index page title, and the group is omitted when no item is visible). Defaults cascade per conventions: item label defaults from the page title, which defaults from the library ("Users" — one authoritative site: public `IndexPageOptions.EffectiveTitle`, shared with the page header); section title defaults to "Identity". `Visible` is cosmetic only — actually removing a feature is the root verb's job; a hidden section or hidden users item just renders no sidebar entry while the screens stay routable.

**Deferred (decision Jerrie, 2026-08-05): `Icon` and `Order`** — the shell sidebar model (`SidebarItem` records + the sidebar view in the OSS repo) has no icon or cross-provider ordering support yet, and we don't ship knobs before the feature exists (the `Sortable()` precedent). Add them to the builders when the shell model gains support. `RolesItem` arrives with the `TRole` overload.

Provisional (per conventions doc): all sidebar items configured together under `ConfigureSidebar`, not per-feature. Revisit if the visibility/enablement cascade gets awkward once the Roles pages land.

## Request pipeline (controller order of operations)

`userManager.Users` → `QueryTransform` → scope predicate → search query → sort (explicit `sortBy` if it resolves to a sortable column, else the default sort) → `Count()` → skip/take. Filters therefore always affect the total; sort never does.

## Roles (built 2026-08-10)

**`ConfigureRoles`, not `AddRoles` — the Roles screens are not opt-in.** The earlier recommendation in this section (opt-in `AddRoles`) is superseded. The Roles screens come with `AddIdentity` itself: a host gets the Roles controllers, routes and sidebar item whether or not it calls `ConfigureRoles`. This keeps the conventions' `Configure*` promise (the feature exists either way).

**Both entity types are always required** (decided 2026-08-10, revising the first-built shape). The overloads are `AddIdentity<TUser, TRole>()` and `AddIdentity<TUser, TRole, TKey>()`, each with a configure variant — there is no users-only `AddIdentity<TUser>()`. This mirrors stock `services.AddIdentity<TUser, TRole>()`, which also names both types, and follows the all-or-nothing pattern OpenIddict uses for its entity set (`ReplaceDefaultEntities<TApplication, TAuthorization, TScope, TToken, TKey>()` takes every entity or none). It also halves the overload set and leaves a single root builder class. The consequence: a host that set Identity up with `AddDefaultIdentity<TUser>()` must add `.AddRoles<IdentityRole>()`, because StellarAdmin needs `RoleManager<TRole>` — registration throws a clear message when it is absent. (`AddIdentity<TUser, TKey>()` could not exist anyway: it has the same arity as `AddIdentity<TUser, TRole>()`, and C# forbids overloads differing only by constraints.)

`ConfigureRoles(Action<ResourceBuilder<TRole>>)` lives on `StellarAdminIdentityBuilder<TUser, TRole, TKey>`, the only root builder; each fluent method returns its own type. Roles reuses the resource layer wholesale: `ResourceBuilder<TRole>`, `IdentityRolesOptions<TRole, TKey> : ResourceOptions<TRole>` (the role seed strings), `RolesController<TRole, TKey> : ResourceControllerBase<TRole>`, and the shared `Views/Shared/` partials. The build-out was Phase 11 of `docs/plans/archive/identity-resource-layer.md`; no shared class changed.

## Delete (built 2026-08-11, confirmation reworked 2026-08-12)

Both resources delete from two places, with two deliberately different mechanisms:

- **Index grid**: each row's delete button posts straight from the grid with htmx — `hx-post` to the resource's `Delete` action, the antiforgery token supplied via `hx-vals` (`IAntiforgery` is injected in `_IndexDataGrid.cshtml`), and the re-rendered `#result` swapped in place so scope, page, sort and search survive the delete. Confirmation rides htmx's own gate: `hx-confirm="js:confirmResourceDelete(this)"` awaits the promise returned by `confirmResourceDelete`, which lives in `Shared/_IndexDeleteDialog.cshtml` next to the single `<sa-alert-dialog>` it opens (`window.stellarAdmin.alertDialog(...).confirmAsync()`). The per-row message comes from the button's `data-delete-message` (`ResourceOptions` delete message); the dialog labels come from the index view model. `_IndexPage.cshtml` renders the dialog partial outside `#result` on purpose — grid swaps must not destroy it.
- **Edit page**: fully server-rendered, no script. The edit page fills the form's `post-form-actions` slot with the trigger — a `type="button"` invoker beside the submit button — and renders `Shared/_FormDeleteDialog.cshtml` (model `ResourceFormDeleteDialogViewModel`, built by `ResourceControllerBase.BuildFormDeleteDialogViewModel`) outside the form: just the dialog, cancelled with a `method="dialog"` form and confirmed with a plain `method="post"` form to the `Delete` action.

**A first cut was built and replaced.** The original index confirmation (2026-08-11) was a delegated-listener TypeScript module (`data-delete-trigger` buttons calling `htmx.ajax` from script) bundled by a rolldown + TypeScript toolchain in `Client/`. The rework (2026-08-12) declares the whole behavior on the button with `hx-confirm`, so the module and its toolchain were removed: the `Client/` build is again only Tailwind plus the htmx vendor copy (`scripts/copy-js.mjs`). Don't reintroduce a bundler for a single page script — a future need for real client modules is a fresh build-tooling decision.

## Resource layer

Extracted 2026-08-10 (phases 1–10 of the identity resource layer plan) so the Roles pages reuse the Users machinery instead of copying it. The decisions worth not re-arguing:

- **Vocabulary**: `TEntity` names the data type; **Resource** names one CRUD section over one entity type (`ResourceOptions<TEntity>`, `ResourceBuilder<TEntity>`). "Section" is not used — the sidebar code owns that word.
- **One shared builder per page type, used directly by every resource — no `Users*` subclasses, therefore no CRTP.** No page builder has a user-only member. If one ever becomes necessary, it will be a constrained C# extension member returning the same shared type, so the fluent chain survives.
- **The shared resource layer lives in the `StellarAdmin.Dashboard` assembly** under the `StellarAdmin.Dashboard.Resources.*` namespaces (extracted 2026-08-14 as its own `StellarAdmin.Resources` assembly — it started as `Options/Base`/`Builders/Base` folders inside the Identity project — and merged into the application package on 2026-08-16, renamed `StellarAdmin.Dashboard` on 2026-09-16). Everything the Identity layer touches is `public`/`protected`, never `internal` — the assembly boundary now enforces what used to be a by-hand rule, and the surface is deliberately public so consumers can build CRUD screens for their own entity types on it.
- **Constraints follow the layer** (decided 2026-08-11): the shared resource layer (`StellarAdmin.Dashboard.Resources`) stays `TEntity : class` — it is entity-agnostic by design — while the Identity-side classes (`IdentityUsersOptions<TUser, TKey>`, `IdentityRolesOptions<TRole, TKey>`, `StellarAdminIdentityBuilder<TUser, TRole, TKey>`) constrain to `IdentityUser<TKey>` / `IdentityRole<TKey>`, matching the controllers and the `AddIdentity` entry points, so library-authored defaults can read Identity members directly instead of having selectors threaded in from the registration. This mirrors ASP.NET Core Identity (managers `class`, `UserStoreBase` constrained) and OpenIddict (core `class`, storage providers constrained): the constraint lives at the layer that knows the framework. `IdentityUser<TKey>` is the lowest type exposing `UserName`/`Email`, so the constraint necessarily carries the `TKey` parameter; consumers never write it — the `AddIdentity` overloads infer or supply it. Research: `docs/plans/identity-entity-constraint-research.md`.
- **The shared controller base has protected helpers only, no actions** — Create genuinely differs between users (password) and roles.
- **Only underscore-prefixed files go in `Areas/StellarAdmin/Views/Shared/`** — an action-named view there becomes the area-wide fallback for every controller, including the OSS Shell's.
- **The form binding prefix is the `ResourceFormPageViewModel.BindingPrefix` constant** (`nameof(ResourceFormPageViewModel.Entity)`), derived at every dependent site so a rename is a compile error, never a silent binding break.

## View overrides (reviewed 2026-08-10)

How an application customizes the shipped screens, decided as a set once both resources existed (the escape-hatch review deferred from the resource layer plan; full option analysis in `docs/plans/identity-view-override-hatches.md`). Everything rides two stock ASP.NET Core mechanisms — the ASP.NET Core Identity default UI pattern, no bespoke resolution scheme:

1. **Exact-path shadowing**: an app `.cshtml` at the same path as an RCL view wins.
2. **Search-order interception**: `<partial name="..."/>` resolves at request time, searching `/Areas/{area}/Views/{controller}/` before `/Views/Shared/` — so an app file in a controller folder overrides a partial for that one resource, even though the RCL only ships it in `Shared/`.

The ladder, coarse to fine (app paths under `Areas/StellarAdmin/Views/`):

| To change... | App file / config | Scope |
|---|---|---|
| One grid cell | `.Template("X")` or `[UIHint("X")]` + `GridDisplayTemplates/X.cshtml` | one column |
| One form field | `.Template("X")` / `[UIHint]` / by type + `EditorTemplates/X.cshtml` | one field |
| The grid | `Users/_IndexDataGrid.cshtml` (one resource) or `Shared/_IndexDataGrid.cshtml` (all) | grid |
| Page chrome | `Shared/_IndexPage.cshtml`, `Shared/_IndexDeleteDialog.cshtml`, `Shared/_FormPage.cshtml`, `Shared/_FormFields.cshtml`, `Shared/_FormDeleteDialog.cshtml` | all resources |
| The whole page | `Users/Index.cshtml`, `Users/Edit.cshtml`, `Users/Create.cshtml`, `Roles/...` | one page |
| The shell layout | `Shared/_Layout.cshtml` (shadows the OSS Shell's) | everything |

The page views render through templated tag helpers (2026-08-13): the form pages through `<sa-form-page model="Model">` and the index pages through `<sa-index-page model="Model">`. A whole-page override therefore need not copy the chrome — the page keeps the tag helper and fills slot outlets instead. `_FormPage` has four: `pre-form-fields` / `post-form-fields` around the field list and `pre-form-actions` / `post-form-actions` around the submit button; the shipped pages use them for the edit delete trigger (`post-form-actions`) and the create password fieldset (`post-form-fields`). `_IndexPage` has two: `pre-page-actions` / `post-page-actions` around the create button in the page header. A page's own `@addTagHelper *, StellarAdmin.Dashboard.Identity` line is needed when shadowing, since app views use the app's `_ViewImports`.

**The typed model** (decided 2026-08-10): an overriding index view declares the concrete class to work with the entity type directly — the controller passes exactly that type, so it binds:

```cshtml
@model ResourceIndexPageViewModel<ApplicationUser>

<p>@Model.Title — page @Model.Page.PageNo of @Model.Page.TotalPages</p>
@foreach (var user in Model.Page.Items)
{
    <a asp-action="Edit" asp-route-id="@Model.GetRowId(user)">@user.Email</a>
}
```

The class's shape follows one rule: **adapted values are members; unadapted data is the object it already is.** `Title`, `CreateLabel`, and the grouped `Empty` / `Search` / `Sort` / `DeleteDialog` records and `Scopes` (regrouped 2026-08-13; the groups stay non-generic so interface and class share the same instances) involve resolution or mapping, so they are public members; the paging data has none, so `Page` exposes the `PagedListViewModel<TEntity>` as-is rather than flattening five forwarding properties (the interface sees it through the non-generic `IPagedListViewModel`). `Selection` exposes the query layer's `IndexPageSelection` unwrapped — the validated values the user explicitly chose, the only ones links round-trip, so URLs never pin an unselected default. `IResourceIndexPageViewModel` is **internal plumbing, not consumer surface** — it exists only because the shipped views, shared between resources, cannot declare an open-generic `@model`; its object-typed members are explicit implementations on the class. (An app override of a `Shared/` partial spanning all resources is inherently untyped and may still use it — the edge case, not the documented story.) `ResourceFormPageViewModel` needs no equivalent: it is non-generic by design and `Entity` is the documented cast. Inside a grid column, `Html.GridItem<ApplicationUser>()` already returns the typed row, and any configuration not on a view model is reachable with `@inject IdentityUsersOptions<ApplicationUser, string>` — the options are public partly for this.

**Prerequisite**: an override view compiles in the app, so the RCL's `_ViewImports.cshtml` does not apply to it. The app needs its own `Areas/StellarAdmin/Views/_ViewImports.cshtml`:

```cshtml
@using StellarAdmin.Dashboard.Identity.Areas.StellarAdmin.ViewModels
@using StellarAdmin.Dashboard.Areas.StellarAdmin.ViewModels
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@addTagHelper *, StellarAdmin.TagHelpers
@addTagHelper *, StellarAdmin.Dashboard
```

Two boundaries of the design, accepted knowingly:

- **The grid is an atomic override unit.** The row-actions column cannot be split into its own partial — tag helper `ParentTag` binding is lexical and `context.Items` does not cross partial boundaries — so adding a row action means copying `_IndexDataGrid.cshtml` (~50 lines the app then owns). A copied grid's delete button keeps working because `confirmResourceDelete`, supplied by `Shared/_IndexDeleteDialog.cshtml`, stays on the page — that function is part of the compatibility surface alongside the view models and partial names. The delete action shipped (2026-08-11) without a `RowActions` builder knob; the copy-the-grid hatch covered its requirements, so add the knob when a user asks.
- **Shadowing is point-in-time.** An override freezes the markup it copied; library markup improvements stop reaching that page. That makes the view models and the partial names/models the compatibility surface — keep the partials small and the view models additive.
