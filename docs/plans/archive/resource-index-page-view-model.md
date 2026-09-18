# Plan: ResourceIndexPageViewModel — regroup, rename, selected-vs-effective state

## Code audit — 2026-09-18

Current status: **completed**. ResourceIndexPageViewModel, PagedListViewModel, IndexPageSelection/Request/Result and the shared index views implement the regrouped model and selected/effective distinction.

This assessment uses the current checkout; earlier status, paths, permissions and verification notes below describe historical sessions. Runtime/browser and hosted release checks were not rerun for this documentation audit.

## Historical record

**Index status:** completed. **Indexed:** 2026-09-05. Historical record; completion is recorded in the phase/progress notes below. Deferred items require a separately scoped task.

## Progress

| Phase | Repo | State | Commit |
|---|---|---|---|
| 1 — Query layer: selected vs effective split | stellar-admin-pro | done | pro `c37a52f` |
| 2 — View model restructure + renames + default views adopt groups/Selection | stellar-admin-pro | done | pro `c37a52f` |
| 3 — Pager `selected-page-size` + grid wiring + usage sweep | stellar-admin-pro | done | pro `c37a52f` |
| 4 — (optional) `preserve-query-except` on sort links; stop pinning `pageNo=1` | stellar-admin-pro | done | pro `13adacf` |

Phases 1-3 landed as one milestone commit (2026-08-13) at Jerrie's direction; design
docs (`identity-configuration.md`, `identity-user-forms.md`) updated to the new type
names and the Selection concept in the same commit. Jerrie also added a permanent
minimal search+scopes `ConfigureUsers` block to the playground to exercise these
paths. His late polish: `IndexPageRequest` clamps via C# `field`-backed setters.

**Phase 4 as built (2026-08-13, pro `13adacf`, Jerrie kept it):**
`DataGridSortTagHelper` gained `preserve-query-except` (comma-separated, trimmed,
case-insensitive), applied before the "explicit route values win" check;
`_IndexDataGrid` and the DataGrid docs demo use `preserve-query-except="pageNo"` in
place of `asp-route-pageNo="1"` (Jerrie trimmed my explanatory Razor comment). The
sandbox spike pages keep their explicit-pin pattern on purpose. Verified: fresh sort
links are bare `?sortBy=...&sortDir=...`; from page 3 with pageSize/search/scope
selected, sort links keep those and drop pageNo; sort targets land on page 1; pager
unaffected. THE PLAN IS COMPLETE — remaining work lives in the Follow-ups list below.

Stop for review after every phase. Jerrie commits.

**Phase 2 as built (2026-08-13):** implemented per the design below; the int?-in-links
gotcha was real, so the scope tabs render the selected page size via
`?.ToString(CultureInfo.InvariantCulture)`. Verified against the playground with the
`ConfigureUsers` block temporarily uncommented (restored afterwards): default-scope tab
bare, unknown scope/sort healed AND scrubbed from tab/search links, canonical casing
round-trips (`sortBy=email` -> links carry `Email`), search box now carries selected
sort, search term preserved through sort links, dialog labels render via the subset
model. Known residue: `preserve-query` links (pager page-size selector, sort headers)
echo the *raw* request query, so an invalid `scope=nope` survives those specific links
until the user touches a scope tab or search — inherent to preserve-query copying
`Request.Query`; revisit only if it bothers anyone. Pager page links still pin
`pageSize=25` until Phase 3.

**Phase 3 as built (2026-08-13):** `DataGridPagerTagHelper` gained `selected-page-size`
(`long?`); page-navigation links substitute it and drop any route value containing the
`{pageSize}` placeholder when it is null, while the size-selector links still always
substitute their target. `page-size` keeps display duties and the placeholder still
requires it (message reworded). `_IndexDataGrid` passes
`selected-page-size="Model.Selection.PageSize"`. Docs demo `DataGrid/_Intro.cshtml`
updated to derive a nullable `selectedPageSize` from the query (default 3 stays
effective-only) and pass both attributes — it now teaches the split. Sandbox spike pages
need no changes (none use the `{pageSize}` placeholder). Verified on the playground
(fresh page links carry only `pageNo`; after choosing 50 they carry `pageSize=50`;
active option and range summary still driven by effective size) and on the DocsSamples
DataGrid page (clean fresh links; selector targets intact; selected size carried via
preserve-query on sort links). Pager markup change is href-only, so no VRT impact.

All work is in the pro repo; the OSS repo is untouched (the data grid tag helpers live
in `StellarAdmin.Pro`).

## Why

`IIndexViewModel` has grown into a flat 22-member bag, and its name undersells what it
is (the view model of a *resource index page*). Two structural problems:

1. **No grouping.** Search is three loose properties, delete-dialog labels are three
   more, paging is five (which the typed class already groups as
   `PagedListViewModel<TEntity>` — the interface flattens it back out). The delete
   dialog partial consumes 3 of 22 members.
2. **Effective and selected state are conflated.** `IndexPageResult.ActiveSortField` /
   `SortDirection` are *effective* values — populated from `DefaultSort` even when the
   user never sorted — yet the views write them into links (scope tabs carry
   `sortBy`/`sortDir`/`pageSize`; the pager substitutes the current page size into
   every page link). Result: defaults get pinned into URLs the user never chose.
   `IndexPageRequest.PageNo`/`PageSize` apply defaults at binding time, so "absent"
   and "user typed 25" are indistinguishable today.

## Design

### The principle: URLs carry only user selections

- **Effective state** — what the query actually applied after defaults and healing
  (default sort, default-scope fallback for unknown slugs, clamped page number,
  default page size). Drives **rendering**: sort indicators, active scope tab, pager
  numbers, range summary, search input value.
- **Selected state** — what the user explicitly put in the request *and* that resolved
  validly. Drives **link generation**: only these values round-trip in URLs; `null`
  means "omit the parameter".

Selection rules (computed in `IndexPageQuery.Execute`):

| Value | Selected when | Carried as |
|---|---|---|
| Scope | `request.Scope` non-empty AND resolves to a scope whose slug matches it (case-insensitive) | the scope's canonical `Slug` |
| SortBy | `request.SortBy` resolves via `ResolveSortableColumn` | the column's canonical `FieldName` |
| SortDir | SortBy is selected AND `request.SortDir` was supplied | normalized `"asc"`/`"desc"` (omitting it is lossless — ascending is the default) |
| PageSize | `request.PageSize` explicitly bound (request property becomes `int?`) | the number |
| PageNo | never | — no link carries the *current* page: scope/search/sort links reset paging by omission; pager links write their *target* page |
| Search | — | not in the Selection record: no link carries it (the search input round-trips it; `preserve-query` preserves it elsewhere) |

Because every generated link writes only selections, `preserve-query` on the sort and
pager links stays self-consistently clean: the current query string can only contain
values the user chose.

An unknown scope or sort field heals to the default (existing behavior) *and* drops
out of Selection, so following any link from a stale bookmarked URL scrubs the dead
parameter — exactly the "bookmarked URLs keep working" intent.

### Query layer (`Infrastructure/Query/`)

**`IndexPageRequest`** — `PageNo` and `PageSize` become `int?` (clamped to >= 1 when
non-null); no other change. Defaults move out of the request.

**New `IndexPageSelection`** record (same namespace):

```csharp
/// <summary>
///     The index page parameters the user explicitly selected, validated against the
///     page's configuration. A null member was not selected (or did not resolve) and
///     must be omitted from generated links, so URLs only pin values the user chose.
/// </summary>
public sealed record IndexPageSelection(
    string? Scope,
    string? SortBy,
    string? SortDirection, // "asc"/"desc" query text; null when SortBy is null or the direction was implicit
    int? PageSize
);
```

**`IndexPageResult<TEntity>`** — gains `int PageSize` (effective; controllers
currently read it off the request, which can no longer supply it) and
`IndexPageSelection Selection`. Existing members stay effective values.

**`IndexPageQuery`** — owns the page-size default (`private const int DefaultPageSize
= 25`; follow-up: move into `IndexPageOptions` together with the hardcoded
`page-size-options="25,50,100"`), computes effective pageNo/pageSize from the nullable
request values, and builds the Selection per the table above.

### View model layer (`Areas/StellarAdmin/ViewModels/`)

Renames (pre-release, breaking is fine — do it in one pass with the regroup so the
override contract only breaks once):

| Today | Becomes |
|---|---|
| `Internal/IIndexViewModel` | `Internal/IResourceIndexPageViewModel` |
| `IndexViewModel<TEntity>` | `ResourceIndexPageViewModel<TEntity>` |
| `IndexScopeViewModel` | `ResourceIndexScopeViewModel` (+ new `QueryValue`) |
| — (new) | `ResourceIndexSearchViewModel`, `ResourceIndexSortViewModel`, `ResourceIndexEmptyViewModel`, `ResourceDeleteDialogViewModel`, `Internal/IPagedListViewModel` |

`PagedListViewModel<TModel>` keeps its name (generic utility, not index-specific).
`FormViewModel`/`DeleteViewModel` etc. follow the `Resource*` scheme later (see
Follow-ups).

New sub-records — all non-generic so the interface and the typed class share the same
instances (the moment a group needs `TEntity` it inherits the interface/class dual
shape; that is the rule for what stays on the root):

```csharp
/// <summary>The search box of an index page.</summary>
/// <param name="Term">The search term entered, or null when the list is unfiltered.</param>
/// <param name="Placeholder">The search box placeholder.</param>
public sealed record ResourceIndexSearchViewModel(string? Term, string Placeholder);

/// <summary>The sort actually applied to an index page, including a configured default sort.</summary>
public sealed record ResourceIndexSortViewModel(string? By, DataGridSortDirection Direction);

/// <summary>The empty state of an index page.</summary>
public sealed record ResourceIndexEmptyViewModel(string Icon, string Title, string Description);

/// <summary>The labels of a delete confirmation dialog.</summary>
public sealed record ResourceDeleteDialogViewModel(string Title, string ConfirmLabel, string CancelLabel);

/// <summary>A scope tab of an index page.</summary>
/// <param name="QueryValue">The value the tab's link carries in the scope parameter —
///     the slug, or null for the default scope so its link stays parameter-free.</param>
public sealed record ResourceIndexScopeViewModel(string Slug, string Title, bool IsActive, string? QueryValue);
```

`Internal/IPagedListViewModel` — non-generic read view so the interface stops
flattening paging; `PagedListViewModel<TModel>` implements it (`Items` via explicit
interface implementation — `IReadOnlyList<T>` is covariant and `TModel : class`, so
`IReadOnlyList<object>` comes for free):

```csharp
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IPagedListViewModel
{
    IReadOnlyList<object> Items { get; }
    long PageNo { get; }
    int PageSize { get; }
    long Total { get; }
    long TotalPages { get; }
}
```

The root shrinks from 22 members to 12, grouped:

```csharp
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IResourceIndexPageViewModel
{
    // Page chrome
    string Title { get; }
    string? Subtitle { get; }
    string CreateLabel { get; }

    // Regions (effective state - drives rendering)
    IReadOnlyList<DataGridColumnOptions> Columns { get; }
    ResourceDeleteDialogViewModel DeleteDialog { get; }
    ResourceIndexEmptyViewModel Empty { get; }
    IPagedListViewModel Page { get; }
    IReadOnlyList<ResourceIndexScopeViewModel> Scopes { get; }
    ResourceIndexSearchViewModel? Search { get; }  // null = search not enabled
    ResourceIndexSortViewModel Sort { get; }

    // Selected state - the link contract; null members are omitted from links
    IndexPageSelection Selection { get; }

    // Entity-touching row functions (the reason the interface/class dual exists)
    string GetDeleteMessage(object row);
    string GetRowId(object row);
}
```

The nullable `Search` group absorbs `SearchEnabled` (null = disabled) and the
"meaningful only when enabled" doc caveats become structure. `ActiveScopeSlug`
disappears from the root (`Scopes[].IsActive` covers rendering, `Selection.Scope`
covers links). `SortDirectionQueryValue` disappears (`Selection.SortDirection` is the
link-facing form).

**`ResourceIndexPageViewModel<TEntity>`** mirrors the groups with typed `Page`
(`PagedListViewModel<TEntity>`), typed `GetDeleteMessage`/`GetRowId`, and explicit
interface implementations — same shape as today, fewer members. Its constructor
simplifies to consume the query result directly, shrinking both controllers:

```csharp
public ResourceIndexPageViewModel(
    IndexPageResult<TEntity> result,
    IndexPageOptions<TEntity> indexPageOptions,
    DeleteOptions<TEntity> deleteOptions,
    Func<TEntity, string> rowId
)
```

It builds `PagedListViewModel<TEntity>` from the result internally, exposes
`result.Selection` as `Selection`, and derives the sub-records from the options. The
`Selection` property reuses the query-layer `IndexPageSelection` type directly rather
than wrapping it in a parallel VM record — one source of truth for "what round-trips";
the Infrastructure.Query namespace is already part of the public extensibility
surface. (Alternative considered: a dedicated `ResourceIndexSelectionViewModel`
mirror; rejected as pure ceremony unless the shapes diverge later.)

### Default views

**`_IndexPage.cshtml`** (`@model IResourceIndexPageViewModel`)

- Scope tabs: `asp-route-scope="@scope.QueryValue"` (default tab's link is
  parameter-free), and carry `@Model.Selection.SortBy` / `@Model.Selection.SortDirection`
  / `Model.Selection.PageSize` instead of effective values — null omits the parameter.
- Search box `hx-get`: `Url.Action("Index", new { scope = Model.Selection.Scope,
  sortBy = Model.Selection.SortBy, sortDir = Model.Selection.SortDirection,
  pageSize = Model.Selection.PageSize })` — `Url.Action` drops nulls. This *adds*
  sort-carrying to search (today searching silently discards an explicit sort while
  scope-switching keeps it — treating that as an oversight, see Decisions).
- Search region renders when `Model.Search is not null`; placeholder/value from the
  record.
- Delete dialog partial: `<partial name="_IndexDeleteDialog" model="Model.DeleteDialog"/>`.
- Razor gotcha to verify during implementation: `asp-route-pageSize="@Model.Selection.PageSize"`
  with a null `int?` must omit the parameter like null strings do; if the nullable
  value type renders as an empty string instead, fall back to
  `@(Model.Selection.PageSize?.ToString(CultureInfo.InvariantCulture))`.

**`_IndexDataGrid.cshtml`** (keeps the root model — its footprint is rows + columns +
sort + paging + empty + both row functions, i.e. most of the model; a grid sub-model
would just replicate the generic dual shape)

- `items="Model.Page.Items"`; sort declaration from `Model.Sort.By`/`Model.Sort.Direction`
  (still effective — a default sort must show its indicator and toggle, existing
  behavior); empty state from `Model.Empty`; pager from `Model.Page` plus
  `selected-page-size="Model.Selection.PageSize"` (Phase 3).

**`_IndexDeleteDialog.cshtml`** — `@model ResourceDeleteDialogViewModel`, three
properties, nothing else changes. Narrowing is deliberate: a dialog override wanting
page context is really an `_IndexPage` override.

**`Users/Index.cshtml` / `Roles/Index.cshtml` / `IndexPageTagHelper`** — mechanical
rename of the model type.

### Pager change (`StellarAdmin.Pro`, Phase 3)

`DataGridPagerTagHelper` gains `selected-page-size` (`long?`):

- Page-navigation links substitute it for `{pageSize}`; when null, any route value
  containing the `{pageSize}` placeholder is *removed* from that link (instead of
  today's unconditional substitution of `page-size`).
- `page-size` keeps its display duties (range summary, active page-size option) and
  stays required whenever a `{pageSize}` placeholder exists.
- Page-size-selector links are unchanged — clicking a size IS a selection, so they
  always substitute their target size.
- Breaking behavior shift: page links no longer implicitly carry the current size.
  Usage sweep: `docs/DocsSamples/Pages/DataGrid/_Intro.cshtml` and the
  `sandbox/DataGridSpike` pages — set `selected-page-size` where the demo should keep
  carrying the size, or leave them clean. XML docs updated to describe the
  selected-vs-effective split.

### Optional polish (Phase 4)

Sort-header clicks currently pin `asp-route-pageNo="1"` into the URL (it exists to
override a stale preserved `pageNo`). Add `preserve-query-except` (comma-separated
keys) to `DataGridSortTagHelper`; `_IndexDataGrid` then uses
`preserve-query-except="pageNo"` and drops the explicit `asp-route-pageNo="1"` —
absence *is* page 1. Same attribute could later serve the pager if a need appears;
scoped to the sort helper for now.

## Decisions taken (flag if you disagree)

1. **Search carries the selected sort** — searching no longer discards an explicit
   sort (scope tabs already preserve it; the asymmetry looks accidental).
2. **Default scope tab link is parameter-free** (`QueryValue = null`) — clicking
   "All" yields `/users`, not `/users?scope=all`.
3. **`IndexPageSelection` is exposed directly on the view model** — no parallel VM
   wrapper record.
4. **`IndexScopeViewModel` renamed now** rather than in the later `Resource*`
   follow-up, since this change already breaks every consumer of the scope record.
5. **Phase 4 is worth doing** — it completes the "URLs contain only selections"
   principle cheaply; skip it if the extra tag-helper attribute feels premature.

## Phases

### Phase 1 — Query layer split

`IndexPageRequest` nullable paging; `IndexPageSelection`; `IndexPageResult` gains
`PageSize` + `Selection`; `IndexPageQuery` computes both sides and owns the default
page size. Controllers updated mechanically (read `result.PageSize`; no VM change
yet — pass-through so the phase compiles standalone). No rendered-output change.

Verify: build (`-p:AllowMissingPrunePackageData=true`); playground boots on :5206 and
the Users/Roles pages behave as before.

### Phase 2 — View model restructure + views

All renames, sub-records, `IPagedListViewModel`, simplified constructor; both
controllers shrink to the 4-arg construction; the three shared views + Users/Roles
`Index.cshtml` + `IndexPageTagHelper` adopt the groups; links switch to Selection
values; search URL gains sort-carrying; delete dialog partial gets its subset model.

Verify (playground :5206, CDP):
- Fresh `/StellarAdmin/Users` -> next page: URL is exactly `?pageNo=2` (pager still
  appends `pageSize` until Phase 3 — expected residue, gone next phase).
- Sort by a column, switch scope: `sortBy`/`sortDir` carried; nothing else appears.
- Default-sort rendering: indicator still shows on the default-sorted column, URL
  stays clean.
- Unknown `?scope=nope&sortBy=bogus`: page heals to defaults AND following any link
  drops both parameters.
- Search with a sort active keeps the sort; scope tab "All" link is parameter-free.
- Delete dialog still opens with per-row message; VRT pass for the index pages.
- Restore playground app.db afterwards if any POSTs ran; stop the 5206 server.

### Phase 3 — Pager selected-page-size

Tag helper change + `_IndexDataGrid` wiring + docs/sandbox usage sweep.

Verify: fresh index -> page 2 has no `pageSize`; pick size 50 -> subsequent page
links carry `pageSize=50`; range summary + active size option unaffected; DataGrid
docs demo still pages correctly.

### Phase 4 — (optional) preserve-query-except

As designed above. Verify sort clicks produce `?sortBy=email&sortDir=desc` with no
`pageNo`, and that sorting while on page 3 still lands on page 1.

## Follow-ups (deferred, not in this plan)

- `Resource*` naming sweep: DONE 2026-08-13, pro `7552e7e` (both rounds in one
  commit): `FormViewModel` -> `ResourceFormPageViewModel` plus the delete-dialog
  resolution below; types, files, views, tag helper, controllers, helper methods,
  and design docs all updated. Still open from the sweep discussion:
  - The delete-model design question: RESOLVED 2026-08-13 view-names-first, Jerrie
    chose strict page symmetry over inheritance/composition (types too simple to
    justify it). `_DeleteConfirmDialog.cshtml` -> `_FormDeleteDialog.cshtml`
    (completing the `_Form*` family alongside `_FormPage`/`_FormFields`);
    `ResourceDeleteDialogViewModel` -> `ResourceIndexDeleteDialogViewModel`,
    `ResourceDeleteViewModel` -> `ResourceFormDeleteDialogViewModel` — two
    independent types, the duplicated label triple accepted deliberately. Helper
    methods aligned: `BuildFormViewModel` -> `BuildFormPageViewModel`,
    `BuildDeleteViewModel` -> `BuildFormDeleteDialogViewModel`. The dialog element
    id `delete-confirm-dialog` was left as-is (markup, not naming scope).
  - `UserCreateViewModel` / `CreateUserPasswordInput`: stay user-named (not
    resource-layer), but check the duplicated Password/ConfirmPassword annotated
    properties and whether both types are still needed. DONE 2026-08-13 (pro
    `798d43e`): both types stay, but the duplication is gone —
    `UserCreateViewModel` now composes `CreateUserPasswordInput` as its
    `PasswordInput` property (single owner of the annotated password contract);
    the view renders `m.PasswordInput.*`, the POST parameter binds the
    `PasswordInput.` prefix via its parameter name (signature unchanged), and
    identity policy errors map to the `PasswordInput.Password` key. Verified on
    the playground: compare error lands on ConfirmPassword, policy error on
    Password, no db write.
- Default page size and the page-size options (`25,50,100` hardcoded in
  `_IndexDataGrid`) move into `IndexPageOptions`: DONE 2026-08-13, pro
  `8334f3e`. `IndexPageOptions` gained seeded `DefaultPageSize` (25) and
  `PageSizeOptions` ([25, 50, 100]); `IndexPageBuilder` exposes both as validated
  settable properties; `IndexPageQuery` reads the default from options;
  `IResourceIndexPageViewModel` gained `PageSizeOptions` and `_IndexDataGrid`
  renders the selector from it (invariant-joined). An empty list renders no
  selector (the pager tag helper already treated no options as no selector).
  Deliberately no validation that `DefaultPageSize` appears in `PageSizeOptions` —
  same stance as a hand-typed `pageSize=37`: works, selector just shows no active
  choice. Verified on the playground: defaults unchanged; custom default stays
  out of page links (effective, not selected) until a size is chosen; empty
  options drop the selector.
- Update the identity design doc(s) in the pro repo that reference
  `IndexViewModel` / the flat property list, and the `identity-resource-layer` /
  `identity-view-override-hatches` plan references, once the rename lands.
  DONE for the living design docs (pro `7552e7e`); the completed plan files keep
  their old names deliberately — they are point-in-time records.
