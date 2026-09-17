# Plan: StellarAdmin Identity options builder — Users index end-to-end

**Index status:** completed. **Indexed:** 2026-09-05. Historical record; completion is recorded in the phase/progress notes below. Deferred items require a separately scoped task.

**Design doc (read first):** `stellar-admin-pro/docs/design/identity-configuration.md`
— API surface, semantics, rationale. Conventions: `docs/conventions/options-builders.md`
(workspace repo). **Grid spec:** `grid-field-expression-binding.md` (same directory) —
executed as Phase 2 of this plan, no longer standalone.

**Scope: the Users index page and its data grid working end-to-end** — columns
configured through the options builder, rendered by the grid via **expression binding
(`field-for`)** and with **`[Display]` attribute support** for column titles. No
string-field interim step.

**Explicitly future plans (this plan only acknowledges them):** `AddRoles`.
Column `Sortable()` shipped 2026-08-05 per its addendum plan below.
Page settings (title/subtitle) landed via the addendum below.
**Query and sidebar both shipped 2026-08-05** — the decided API is pinned in the
next section; both are implemented exactly as pinned (see Progress).

## Decided API: Query + Sidebar (both IMPLEMENTED 2026-08-05)

Names and semantics agreed in the design doc
(`stellar-admin-pro/docs/design/identity-configuration.md`), recorded here as the
implementation reference. Options follow the established pattern: public types,
read-only outside the builders (`internal` setters).

### Query hook

**IMPLEMENTED 2026-08-05** exactly as specified below (see Progress).

- **Builder:** `UsersIndexPageBuilder<TUser>.Query(Func<IQueryable<TUser>, IQueryable<TUser>> query)`
  — a method, not a property (it composes). Repeat calls **compose, outer wraps
  inner** (`existing is null ? query : q => query(existing(q))`), never replace.
- **Options:** `UsersIndexPageOptions<TUser>.Query` —
  `Func<IQueryable<TUser>, IQueryable<TUser>>? { get; internal set; }`; `null` =
  no interception.
- **Application:** `UsersController.Index` applies it to `userManager.Users`
  **before** `Count()` and `Skip/Take`, so filters affect totals consistently.
- `StellarAdmin.Identity` gains **no EF Core dependency** — the consumer's own EF
  reference provides `Include`/`Where`/etc.

### Sidebar

**Group + users item IMPLEMENTED 2026-08-05** (Jerrie started, completed in-session;
see Progress). Scope decision (Jerrie): **`Icon` and `Order` are deferred** — the
shell sidebar model (`SidebarItem` records + `Default.cshtml` in the OSS repo) has
no icon or cross-provider ordering support, and we don't ship dead knobs (the
`Sortable()` precedent). Add them when the shell model gains support. `RolesItem`
waits for `AddRoles`.

- **Builder entry:** `StellarAdminIdentityBuilder<TUser>.ConfigureSidebar(Action<IdentitySidebarBuilder> sidebar)`
  (`Configure*` = ships-by-default per conventions). All identity sidebar items
  configure here, not per-feature (provisional; revisit when `AddRoles` arrives).
- **`IdentitySidebarBuilder`** — section level (today's `SidebarGroupItem("Identity")`):
  - Properties: `Title`, `Visible` (deferred: `Icon`, `Order`)
  - Methods: `UsersItem(Action<IdentitySidebarItemBuilder>)`
    (future: `RolesItem(...)`)
- **`IdentitySidebarItemBuilder`** — per item:
  - Properties: `Label`, `Visible` (deferred: `Icon`, `Order`)
- **Options:** `StellarAdminIdentityOptions<TUser>.Sidebar` →
  `IdentitySidebarOptions` (non-generic) with `UsersItem`/`RolesItem` of type
  `IdentitySidebarItemOptions`.
- **Rendering:** `SidebarItemsProvider` stops hardcoding and reads the options.
- **Default cascade** (per conventions): item label ← page title ← library default
  ("Users"); section title defaults to "Identity". `Visible` is cosmetic only —
  actually removing a feature stays the root verb's job (`AddRoles` absent = no
  roles item regardless of sidebar config).

## Addendum plan: column `Sortable()` + Users index sorting (IMPLEMENTED and SIGNED OFF 2026-08-05)

Contrary to the design doc's "no sort infrastructure" line, **the grid side is
fully built** (`stellar-admin-pro/src/StellarAdmin.Pro/TagHelpers/DataGrid/`):
`<sa-data-grid-sort>` declares the link pattern (`{sort}`/`{dir}` placeholders,
`sort-by`/`sort-direction` current state, `preserve-query`), columns opt in via
`sortable` (`sort-field` defaults from the bound field name), and the grid renders
ghost-button header links, direction icons, and `aria-sort`. Everything missing is
on the Users index side.

**Settled decisions (Jerrie, 2026-08-05):**

1. Unknown or non-sortable `sortBy` query values are **silently ignored** (renders
   unsorted) — bookmarked URLs must not break when a column is removed.
2. `Sortable()` on an expression EF cannot translate (e.g. the `Roles` collection)
   **fails at request time** — no validation at configure time, consistent with
   `Add`'s no-validation stance.
3. Default (unconfigured markup) columns stay **unsorted** — deferred.
4. No `DefaultSort(...)` — deferred at the time; **shipped later the same day** (see
   the DefaultSort Progress entry). With neither `sortBy` nor a default sort the
   index stays unordered.

**Changes:**

- `IdentityGridColumn` gains `public bool Sortable { get; internal set; }` (default
  false); `UsersIndexPageColumnBuilder` gains chained `Sortable()` setting it
  (alphabetical: after `Format`... the file currently orders Title/Template/Format —
  match whatever grouping exists, don't reorder untouched members).
- `UsersController.Index` gains `string? sortBy = null, string? sortDir = null`.
  Resolution: first configured column with `Sortable && FieldName` equal to `sortBy`
  (OrdinalIgnoreCase); none → unsorted. Direction: `"desc"` → descending, anything
  else ascending. **Sort by the column's own captured expression** — no
  string-to-expression mapping layer:

  ```csharp
  var call = Expression.Call(
      typeof(Queryable),
      descending ? "OrderByDescending" : "OrderBy",
      [typeof(TUser), column.FieldExpression.ReturnType],
      query.Expression,
      Expression.Quote(column.FieldExpression));
  query = query.Provider.CreateQuery<TUser>(call);
  ```

  Applied **after** the `Query` interceptor, before `Count()`/`Skip/Take` (Count is
  order-insensitive; EF strips the OrderBy from the count query). Nested chains
  (`u => u.Department!.Name`) sort server-side for free.
- `IUsersIndexViewModel` gains `string? SortBy` and a sort direction member;
  `UsersIndexViewModel` ctor takes the **resolved** values from the controller
  (`SortBy` null unless it matched a sortable column, so the grid never marks a
  bogus column active). Direction type: `DataGridSortDirection` (Identity already
  references StellarAdmin.Pro).
- `_IndexDataGrid.cshtml`: add inside the grid

  ```cshtml
  <sa-data-grid-sort sort-by="@Model.SortBy" sort-direction="Model.SortDirection"
                     asp-route-sortBy="{sort}" asp-route-sortDir="{dir}"
                     asp-route-pageNo="1" preserve-query="true"/>
  ```

  (explicit route values beat preserved query params — documented on the tag
  helper — so `asp-route-pageNo="1"` resets to page 1 on sort change while
  `pageSize` carries over); the configured-columns loop adds
  `sortable="@column.Sortable"`. The default-columns branch is untouched
  (decision 3). The pager already has `preserve-query="true"`, so its links carry
  `sortBy`/`sortDir` automatically.
- Playground: mark `Email`, `UserName`, `AccessFailedCount` as `Sortable()`.

**Edge cases to note in XML docs / accept:**

- `FieldName` is the **leaf** name for nested chains, so two chains sharing a leaf
  (`Department.Name` + `Manager.Name`) collide — first match wins. Also verify
  during implementation what sort field the grid derives from `field-for` for a
  chain; if it differs from the identity-side `FieldName`, the mismatch just falls
  into the decision-1 "ignored" behavior.
- A `Sortable()` column whose expression has no field name (`FieldName == null`)
  can never match a `sortBy` value; the grid may additionally throw its own
  "requires a field" error at render — acceptable under decision 2.

**Verification (playground on 5206):** sorted first-row changes for
asc/desc on Email; `aria-sort` + active link on the sorted column; unknown
`sortBy=Nope` renders unsorted (200, no active column); pager links carry the sort
params; sorting from page 2 resets to page 1; `Roles` column stays link-free.

## Addendum plan: seeded default columns + `Clear()` (approved 2026-08-05)

Jerrie's revisit of the hardcoded-default-columns decision, triggered by "make the
default columns sortable": the dual path (markup defaults vs configured
definitions) would force the controller to duplicate knowledge of the defaults for
`sortBy` resolution, and every future per-column feature would pay again. Decision:
**seed the defaults as real `IdentityGridColumn` definitions** — one render loop,
one sort-resolution path.

**Settled decisions (Jerrie, 2026-08-05):**

1. **`Add` extends the seeded defaults; new `Clear()`** on the columns builder
   starts a fully custom set. This supersedes the conventions rule "configuring a
   collection replaces its defaults" — the anti-pattern was appending to *unseen*
   defaults; seeded defaults are visible (inspectable on the options, documented),
   so extending is principled. Conventions doc updated in the same change.
2. **`Clear()` with no `Add`s renders an empty grid** (FAFO) — no fallback, since
   any fallback reintroduces the dual path. The null-vs-cleared distinction and the
   count-based `AreColumnsDefined` stand-in (and its standing constraint) dissolve:
   `AreColumnsDefined` is removed from options + view model + view, and the view's
   hardcoded default-column branch is deleted.
3. **Seeding mechanics:** at `AddIdentity<TUser>` time `TUser : class` only (the
   `IdentityUser<TKey>` constraint is on the controller), so the
   `UsersIndexPageOptions<TUser>` ctor builds the three expressions by reflection
   (`Expression.Parameter`/`Expression.Property`); a missing property (exotic
   TUser) skips that seed. Seeded columns: `UserName`, `Email`, `EmailConfirmed` —
   **all sortable**, titles left null so the metadata chain applies (header
   "Username" becomes "User Name"; consumer buddy-class metadata now affects
   default columns — consistency win).
4. **Library-seeded default sort: `UserName` ascending** — out-of-box stable
   paging. `index.DefaultSort(...)` replaces it (last wins). `Clear()` does NOT
   touch the default sort — `DefaultSort` is column-independent by design, so no
   dangling is possible (worst case: no active-header arrow).

**Changes:** options ctor seeding + `ClearColumns()` + `AreColumnsDefined` removal;
`UsersIndexPageColumnsBuilder.Clear()`; view model interface/impl drop
`AreColumnsDefined`; `_IndexDataGrid.cshtml` keeps only the loop; playground adds
`columns.Clear()` before its `Add`s; conventions doc rule + anti-pattern reworded.

**Verification (5206):** playground unchanged visually (Clear + 6 columns, Email
default sort from its `DefaultSort(Email)`); temporarily unconfigured → 3 seeded
sortable columns ("User Name"/"Email"/"Email Confirmed"), UserName active
ascending, buddy `[UIHint]` applies to the default EmailConfirmed; temporarily
Clear-only → empty grid renders 200.

## Current state (verified 2026-08-04 — re-read before starting, plan only the delta)

- `stellar-admin-pro/src/StellarAdmin.Identity/Builders/StellarAdminIdentityBuilder.cs`
  — empty, **non-generic** `public class StellarAdminIdentityBuilder { }`. Becomes
  `StellarAdminIdentityBuilder<TUser>`; the non-generic class goes away.
- `StellarAdminBuilderExtensions.cs` — four `AddIdentity` overloads exist
  (with/without configure × string-key/`TKey`); the configure overloads run the
  delegate **eagerly** and return `StellarAdminBuilder` for chaining. Keep both
  behaviors (eager is load-bearing for a future `AddRoles` gating registration); the
  delegate parameter becomes generic over `TUser`.
- `Areas/StellarAdmin/Views/Users/Index.cshtml` — page chrome only; the grid moved to
  `_IndexDataGrid.cshtml` (whole `<sa-data-grid>` incl. empty state + pager), rendered
  via `<partial name="_IndexDataGrid"/>`. This partial is the consumer markup-override
  unit AND the render site for the column `@foreach` — grid and columns stay in one
  file, which is required (tag helper `ParentTag` binding is lexical; `context.Items`
  doesn't cross partial boundaries).
- `_IndexDataGrid.cshtml` — three hardcoded `<sa-data-grid-column title="..."
  field="..."/>` elements to be replaced by the loop.
- `UsersController<TUser, TKey>.Index` pages `userManager.Users` (`Count()` +
  `Skip/Take`); `IUsersIndexViewModel` (+ `UsersIndexViewModel<TUser, TKey>`) exposes
  `Rows`/paging and will grow `IReadOnlyList<IdentityGridColumn> Columns`.
- Grid: `DataGridColumnTagHelper` resolves field metadata + display templates on the
  collect pass into `DataGridContext` caches (`FieldMetadata`, `GridDisplayTemplates`);
  no expression support yet — that's what `grid-field-expression-binding.md` specifies.
- Test host: `sandbox/IdentitySimplePlayground` — `Program.cs` already calls
  `.AddStellarAdmin().AddIdentity<ApplicationUser>(identityBuilder => { })`;
  `ApplicationUser` has a `[ModelMetadataType]` buddy class with
  `[UIHint("EmailConfirmed")]`, and `Pages/Shared/GridDisplayTemplates/EmailConfirmed.cshtml`
  exists — ideal for proving the template pipeline survives.

## Progress

- **Addendum (2026-08-05): `DefaultSort`, COMPLETE and SIGNED OFF (2026-08-05,
  reviewed and committed by Jerrie).** Closes the unordered-paging gap left by the
  Sortable addendum's decision 4.
  - API (approved design, LINQ mirror — no direction enum in the config API):
    `UsersIndexPageBuilder<TUser>.DefaultSort<TProp>(Expression<Func<TUser, TProp>>)`
    + `DefaultSortDescending<TProp>(...)`; **repeat calls replace (last wins)** —
    a setting, unlike `Query` which composes. Expression captured unvalidated
    (fails at request time on every unsorted render if untranslatable);
    independent of the columns, so a non-displayed property works.
  - Options: `UsersIndexPageOptions<TUser>.DefaultSort` → new public read-only
    `IdentityGridDefaultSort` (`FieldExpression`, `FieldName`, `Descending`),
    internal ctor. `ExtractFieldName` moved from the columns builder to shared
    internal `FieldExpressionHelper` (Builders/).
  - Controller priority: explicit matched `sortBy` wins → else default sort
    (including the ignored-`sortBy` fallback) → else unordered. `ApplySort` now
    takes a bare `LambdaExpression`. Display: when the default's `FieldName`
    resolves to a sortable column (via the same `ResolveSortColumn`), that column
    reports as the active sort — header arrow + direction toggle behave exactly
    like an explicit sort; otherwise data-only sorting, no active header. No view
    or view-model changes needed.
  - Playground: `index.DefaultSort(u => u.Email)` (Jerrie switched it from
    UserName post-review). Verified on 5206: bare page shows `aria-sort` on the
    default column with toggle-to-desc link and row order byte-identical to the
    explicit equivalent; explicit `sortBy` overrides; `sortBy=Nope` falls back to
    the default (not unordered). Formatted; builds clean; 5206 stopped.
- **Addendum (2026-08-05): column `Sortable()` + Users index sorting, COMPLETE and
  SIGNED OFF (2026-08-05, reviewed and committed by Jerrie).** Implemented exactly
  per the addendum plan section above; all its verification points passed on 5206
  (asc/desc first-row flip on Email, `aria-sort` + active link + direction toggle,
  exactly three sort links with non-sortable columns link-free, unknown/non-sortable
  `sortBy` → 200 unsorted, pager carries sort params, sorting from page 2 resets to
  `pageNo=1` while keeping `pageSize`). Jerrie added `.Sortable()` to the
  playground's `EmailConfirmed` column post-review. **Still deferred (Jerrie's
  decision 3):** sort links on the built-in default columns (needs its own
  fieldname→expression resolution path — the controller only knows configured
  columns). `DefaultSort(...)` (decision 4) shipped later the same day — see its
  entry. Accepted edge cases stand as documented in the plan section
  (nested-chain leaf-name collisions; grid-derived chain sort field unverified — no
  nested sortable column exists yet).
- **Addendum (2026-08-05): Query interception hook, COMPLETE and SIGNED OFF
  (2026-08-05).** Implemented exactly per the Decided API section (Jerrie did the
  playground prep — `ApplicationUser.Roles` skip navigation over
  `IdentityUserRole<string>`, `UserRoles` grid display template, `AddIdentity` role
  type fixed to `ApplicationRole`; the hook itself delivered in-session at his
  request):
  - `UsersIndexPageOptions<TUser>.Query`
    (`Func<IQueryable<TUser>, IQueryable<TUser>>?`, internal setter, null = no
    interception); `UsersIndexPageBuilder<TUser>.Query(...)` composes on repeat
    calls (outer wraps inner); `UsersController.Index` applies it to
    `userManager.Users` once, before `Count()` and `Skip/Take`. No EF Core
    dependency added — `Include` returns `IIncludableQueryable<TUser, TProp>`,
    which is an `IQueryable<TUser>`, so consumer-side EF operators flow through.
  - Playground: `index.Query(q => q.Include(u => u.Roles));` +
    `columns.Add(u => u.Roles)` (Jerrie's placement, after UserName) rendering via
    the `[UIHint("UserRoles")]` template.
  - Verified on 5206 against real data (51 users / 50 role rows): page 1 = 24 ×
    "1 Roles" + 1 × "0 Roles", page 2 = 25 × "1 Roles" — non-zero counts prove the
    Include ran, since without it the initialized-empty nav list always renders 0.
    Touched files CSharpier-formatted; builds clean; 5206 stopped.
- **Addendum (2026-08-05): sidebar group + users item, COMPLETE and SIGNED OFF
  (2026-08-05).** Jerrie wrote
  `ConfigureSidebar` + the builder/options skeletons; completion delivered
  (uncommitted, pro repo) per the Decided API section above (Icon/Order deferred —
  Jerrie's scope call, no dead knobs while the shell model can't render them):
  - `IdentitySidebarOptions` (`Title`, `UsersItem`, `Visible = true`) +
    `IdentitySidebarItemOptions` (`Label`, `Visible = true`) — public, internal
    setters. `IdentitySidebarBuilder` gains `Title`/`Visible` write-through
    properties + `UsersItem(Action<IdentitySidebarItemBuilder>)`; new
    `IdentitySidebarItemBuilder` (`Label`/`Visible`).
  - `SidebarItemsProvider` is now **generic** (`SidebarItemsProvider<TUser>`,
    injects `StellarAdminIdentityOptions<TUser>`; registration closes it over
    `TUser`). Hidden section OR hidden users item → empty result (the users item is
    the only item; comment notes per-item filtering once more items exist).
    Cosmetic-only: `/admin/users` stays routable when hidden.
  - **Label cascade** lives in the provider: item `Label` → page title → "Users".
    The "Users" fallback moved from the view model into internal
    `UsersIndexPageOptions.EffectiveTitle` so page header + sidebar share ONE
    default site (evolves the addendum's "resolved in the view model" note — same
    one-place principle, now two consumers).
  - Section title default "Identity" resolved in the provider.
  - Playground configures `sidebar.Title = "User management"` (item label left to
    cascade → "Team members"). Verified on 5206 (server-rendered HTML): configured
    title + cascaded label + `/admin/Users` href with `data-active`; item label
    override ("Accounts") beats cascade; `Visible = false` renders no group while
    the page stays routable; empty configure → "Identity"/"Users" defaults.
    Touched files CSharpier-formatted; builds clean (0 errors); 5206 stopped.
- **Addendum (2026-08-05): page settings + public options, COMPLETE and SIGNED
  OFF.** Jerrie-requested follow-up (small enough to live here instead of a new
  plan). Post-review incident: Rider silently reverted
  `ViewModels/Internal/UserViewModels.cs` to its committed state after the work was
  verified (the Title/Subtitle interface members vanished; the implementation file
  kept its explicit impls → CS0539). Jerrie confirmed he didn't touch it; members
  restored, build clean. Not investigating unless it recurs.
  - `UsersIndexPageBuilder<TUser>` gains settable properties `Title`/`Subtitle`
    (scalars-as-properties per conventions), writing through to
    `UsersIndexPageOptions<TUser>` (`{ get; internal set; }`).
  - **Options types made public** (Jerrie's decision — no reason to stay internal):
    `StellarAdminIdentityOptions<TUser>` + `UsersIndexPageOptions<TUser>` are public
    but read-only outside the builders — internal setters, `Columns` exposed as
    `IReadOnlyList` over a private list with an internal `AddColumn` (protects the
    count-based `AreColumnsDefined` stand-in from runtime mutation).
    `UsersController` now takes the options via constructor injection (the
    `RequestServices` workaround is gone); `UsersIndexViewModel`'s ctor is public
    again.
  - `IUsersIndexViewModel` gains `Title` (defaulted to "Users" in the view model —
    single place) and `Subtitle` (null = no element). `Index.cshtml` renders
    `@Model.Title` and conditionally `<sa-page-header-description>`.
  - Playground config sets Title "Team members" / Subtitle. Verified on 5206:
    configured case renders both (h1/p data-slots); empty-configure case renders
    default "Users" with NO description element and the default grid columns;
    `phase3-verify.mjs` 14/14 after the change. Touched files CSharpier-formatted;
    builds clean; 5206 stopped. Design doc updated (v1 scope line, options-model
    public/read-only note, page-settings note).
- **Phase 3: COMPLETE and SIGNED OFF (2026-08-04) — END OF PLAN.** Jerrie approved starting Phase 3 with one deviation from the
  design doc: **`Add<TProp>` does NOT validate the expression** — nested properties
  are allowed and fail at the data grid level (grid `field-for` is still
  direct-property-only); proper nested support is a follow-up. Delivered
  (uncommitted, pro repo):
  - `UsersIndexPageColumnsBuilder.Add`: validation removed; `ExtractFieldName` is
    best-effort (leaf member name, else null). `IdentityGridColumn.FieldName` is now
    `string?` (nothing consumes it yet); file reordered to the member-ordering
    convention while touched.
  - `AddIdentity<TUser, TKey>()` registers `StellarAdminIdentityOptions<TUser>` as an
    **instance singleton**, reusing an already-registered instance on repeat calls so
    every call configures the same options (consistent with the method's other
    idempotency checks).
  - `UsersController.Index` resolves the options via `HttpContext.RequestServices`
    (the options type is internal, so it cannot appear in the public ctor signature)
    and passes `options.UsersIndexPage` to `UsersIndexViewModel` (new **internal**
    ctor param). `IUsersIndexViewModel` gains `AreColumnsDefined` +
    `IReadOnlyList<IdentityGridColumn> Columns` (explicitly implemented, delegating
    to the options — the count-based stand-in logic stays in one place).
  - `_IndexDataGrid.cshtml` branches on `Model.AreColumnsDefined`: false → the
    hardcoded three columns unchanged; true → `@foreach` emitting
    `<sa-data-grid-column title="@column.Title" field-for="column.FieldExpression"
    template="@column.Template" format="@column.Format"/>`.
  - **Grid fix this forced** (`DataGridColumnTagHelper`): Razor writes a
    string-bound tag-helper attribute whose value is a null expression as `""`, not
    null (`template="@column.Template"` tried to resolve template `''`). ProcessAsync
    now normalizes empty `Title`/`Template`/`Format` to null so the metadata default
    chain still applies. `LambdaExpression`-typed `field-for` is unaffected (non-string
    props compile as C# expressions, null stays null).
  - Playground: `ApplicationUserMeta` gains `[Display(Name = "Phone")]` on
    `PhoneNumber`; `Program.cs` configures five columns (reorder, `.Title(...)`,
    `[Display]` title, `.Format("{0} attempts")`, `[UIHint]` badge).
  - Verified on 5206 (server-rendered HTML assertions, scratchpad
    `phase3-verify.mjs`): (a) configured — 14/14: header order/titles
    ("E-mail address", "User Name", "Phone", "Access Failed Count",
    "Email Confirmed"), 25 rows, format applied, badge template, pager; (b) empty
    configure lambda — default hardcoded columns render identically (headers
    Username/Email/Email Confirmed, badge intact); (c) nested
    `columns.Add(u => u.UserName!.Length)` — **startup succeeds**, users page fails
    with the grid's field-for InvalidOperationException naming the expression.
    DataGridSpike regression re-run after the tag-helper change: 17/17 + both
    FieldForBad error cases. Touched files CSharpier-formatted; builds clean;
    5206 stopped. Design doc Columns section updated to record the new
    no-validation decision.
- **Phase 2: COMPLETE (2026-08-04), signed off (Jerrie approved proceeding to
  Phase 3).** Delivered (uncommitted, pro repo):
  - `DataGridColumnTagHelper`: new `field-for` (`LambdaExpression?`) attribute —
    mutually exclusive with `field` (throws), direct-property-only extraction with
    boxing-`Convert` unwrap (clear error on chains), field name feeds the existing
    metadata/template/format/sort pipeline unchanged. Metadata for expression columns
    resolves from the lambda **parameter type** (works on an empty grid); string
    fields keep the runtime-item-type path. Compiled `Func<object, object?>` accessor
    built at collect, cached in new `DataGridContext.FieldGetters`, used by row passes.
  - **Header title default chain** (both `field` and `field-for`): explicit `title` /
    header template → `ModelMetadata.DisplayName` (`[Display]`, buddy-class aware) →
    PascalCase-split property name (`SplitPascalCase`, handles acronyms: `UserID` →
    "User ID"). Behavior change is safe: grep confirmed no existing markup omits
    `title` on a field column. The file was reordered to the (now confirmed)
    csharp-file-organization convention as part of the edit.
  - Spike verification: new `sandbox/DataGridSpike/Pages/FieldFor.cshtml` (string vs
    expression twin grids byte-identical; `[Display]`/split-name/string-field header
    defaults; buddy-class `[UIHint]` via field-for; empty-grid metadata) — 17/17
    assertions pass (script: scratchpad `fieldfor-verify.mjs`, session-ephemeral);
    `FieldForBad.cshtml` proves both designed errors. New `SpikeCrewMember` model
    carries the `[Display]` case. DocsSamples `/DataGrid` regression re-verified
    manually (headers/sort/aria-sort/pager/selection/currency all intact —
    the old `pagesize-verify.mjs` scratchpad script no longer exists).
  - Attribute name `field-for` confirmed by Jerrie; `[Display]` titles were already
    decided. Touched files CSharpier-formatted; servers on 5206 stopped.
  - Review tweak (Jerrie, 2026-08-04): metadata lookup uses the
    `GetMetadataForProperty(type, fieldName)` **extension method** on
    `IModelMetadataProvider` (ModelMetadataProviderExtensions, Mvc.Core) instead of
    `GetMetadataForType(type).Properties[name]` — same lookup, but a typo'd string
    `field` now fails fast at collect (ArgumentException) rather than at row-value
    time. Deliberate; do not revert. Re-verified: 17/17 spike assertions + both
    error cases pass.
  - Getter unification (Jerrie-requested, 2026-08-04): `DataGridFieldGetters` is now
    the single app-lifetime getter cache with two `GetValue` overloads —
    `(item, field)` keyed on the **runtime** item type (string fields; polymorphic
    lists get a getter per concrete type) and `(item, declaredType, property)` keyed
    on the lambda **parameter** type (expression fields; preserves C# lambda
    semantics for explicit-interface/shadowed properties). The two key spaces may
    overlap but compile identical getters. `DataGridContext.FieldGetters` and the
    tag helper's `ResolveFieldGetter` are deleted (they recompiled per request);
    the row-pass branch lives in one documented `GetFieldValue` helper. The
    deliberate asymmetry is documented in both files — don't "simplify" it to one
    key strategy. Re-verified: 17/17 + both error cases.
- **Phase 1: COMPLETE and SIGNED OFF (2026-08-04)** — Jerrie reviewed and is happy with
  the current state. Delivered (uncommitted at sign-off time):
  - `Builders/`: `StellarAdminIdentityBuilder<TUser>` (`ConfigureUsers`) →
    `IdentityUsersBuilder<TUser>` (`Index`) → `UsersIndexPageBuilder<TUser>`
    (`Columns`) → `UsersIndexPageColumnsBuilder<TUser>` (`Add<TProp>` with
    direct-property validation + boxing-Convert unwrap) → `UsersIndexPageColumnBuilder`
    (`Title`/`Template`/`Format` chaining). Namespace `StellarAdmin.Identity`.
  - `Options/`: `StellarAdminIdentityOptions<TUser>` + `UsersIndexPageOptions<TUser>`
    (**internal**; public-later is non-breaking) and public sealed
    `IdentityGridColumn` (`FieldExpression`, `FieldName`, internally-settable
    `Title`/`Template`/`Format`).
  - `AddIdentity` overloads take `Action<StellarAdminIdentityBuilder<TUser>>` (eager,
    return `StellarAdminBuilder`); the parameterless ones return the generic builder.
    Options built but NOT registered in DI yet (Phase 3).
  - Playground `Program.cs` carries the usage prototype (Add/Title chaining on
    `ApplicationUser`). Builds clean; touched files CSharpier-formatted.
  - Naming evolved during review: Jerrie renamed the `UsersIndex*` family to
    `UsersIndexPage*`; `IdentityUsersBuilder.Index()` stays `Index()` for now.
  - **No default column seeding** (Jerrie's decision): zero configured columns ⇒ the
    view renders its hardcoded default markup; `UsersIndexPageOptions.AreColumnsDefined`
    (`Count > 0`) drives the branch. NOTE the constraint recorded in the design doc:
    that check is only valid while the columns builder has no clear/remove API — never
    add one without conferring with Jerrie. (He flagged that I deviated from his
    null-by-default instruction here; the count-based stand-in is accepted "for now".)

## Phases (stop for Jerrie's review after each — no blanket approval)

### Phase 1 — Identity API skeleton + usage prototype (compile-only)

Create the builder/options types per the design doc — `StellarAdminIdentityBuilder<TUser>`,
`IdentityUsersBuilder<TUser>`, `UsersIndexPageBuilder<TUser>` (columns only for now),
`UsersIndexPageColumnsBuilder<TUser>` + `UsersIndexPageColumnBuilder`,
`IdentityGridColumn` — with **no
runtime wiring**. Update the `AddIdentity` configure overloads to the generic
delegate. Fill the playground's existing empty configure lambda with the design doc's
target usage (columns only, adapted to `ApplicationUser`'s real properties) and build.
**Checkpoint:** Jerrie test-drives the IntelliSense/typing feel in Rider before any
plumbing exists. Naming/shape feedback lands here, where it's cheapest.

### Phase 2 — Grid: `field-for` + `[Display]` titles

Implement `grid-field-expression-binding.md` (it contains the verified research and
detailed design — treat it as the spec for this phase), plus the now-decided
`[Display]` support:

- `field-for` (`LambdaExpression`) attribute on `sa-data-grid-column`: mutually
  exclusive with `field`, direct-property-only extraction (unwrap boxing `Convert`),
  property name feeds the existing `FieldMetadata`/`GridDisplayTemplates` caches,
  metadata resolved from the lambda's parameter type (works on an empty grid),
  compiled getter cached on `DataGridContext`.
- **Header title default chain** (applies to `field` AND `field-for` columns): explicit
  `title` attr / header template → `ModelMetadata.DisplayName` (`[Display(Name)]`,
  buddy-class aware) → PascalCase-split property name (`EmailConfirmed` →
  "Email Confirmed"). Resolved at collect. This resolves the grid plan's open
  decision 2 in favor of implementing it. Attribute name **`field-for` confirmed by
  Jerrie 2026-08-04** (grid plan open decision 1 resolved).

Verify in `sandbox/DataGridSpike` (`GridDisplay.cshtml`): expression-bound twins of the
string-field columns render identically; `[UIHint]` + buddy-class cases; a `[Display]`
title; a no-attribute PascalCase-split title; metadata on an empty grid. Run the
existing DocsSamples regression (`pagesize-verify.mjs`, 21 checks) since the column
helper changes.
**Checkpoint.**

### Phase 3 — Wire end-to-end

- `Add<TProp>` semantics per design doc: capture `Expression<Func<TUser, TProp>>`,
  validate direct-property at configuration time (startup failure with a clear
  message). **No default seeding** — unconfigured means zero stored columns, and the
  view renders its hardcoded default markup (decision 2026-08-04; already implemented
  in Phase 1: `UsersIndexPageOptions.AreColumnsDefined`).
- Options registered as an eager singleton (design doc explains why not
  `services.Configure`), consumed by `UsersController`, exposed via
  `IUsersIndexViewModel` as `Columns` + `AreColumnsDefined`.
- `_IndexDataGrid.cshtml` branches: `AreColumnsDefined == false` → keep today's
  hardcoded three columns as-is; `true` → `@foreach (var column in Model.Columns)
  { <sa-data-grid-column title="@column.Title" field-for="column.FieldExpression"
  template="@column.Template" format="@column.Format"/> }` (null attrs = defaults).
- Verify in the playground on **5206**: (a) empty configure lambda renders the same
  grid as today's hardcoded columns (incl. the `EmailConfirmed` `[UIHint]` badge);
  (b) configured path — reordered columns, `.Title(...)` override, a `[Display]`-titled
  column, `.Format(...)` — renders as configured; (c) a nested-property `Add` fails at
  startup with the designed message. Headless DOM assertions via CDP.
**Checkpoint — end of plan.**

## Environment notes

- Build: `DOTNET_SYSTEM_NET_DISABLEIPV6=1 dotnet build -p:AllowMissingPrunePackageData=true`.
- Run playground/samples: `ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS=http://localhost:5206 dotnet run --no-launch-profile` — port **5206** only (5205 is Jerrie's); stop your instances when done.
- Headless DOM checks via system chromium over CDP (no Playwright).
- Format only touched files via the OSS repo: `dotnet csharpier format <paths>` from `stellar-admin/`.
- Never `git commit` — Jerrie reviews and commits himself (changes span
  stellar-admin-pro and possibly the workspace repo; run git from within each).
