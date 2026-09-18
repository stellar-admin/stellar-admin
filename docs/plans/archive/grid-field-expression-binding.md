# Plan: Expression-bound columns for the data grid (`field-for`)

## Code audit — 2026-09-18

Current status: **completed**. `src/StellarAdmin.TagHelpers/TagHelpers/DataGrid/DataGridColumnTagHelper.cs` exposes `LambdaExpression FieldFor`, rejects simultaneous string/expression binding, extracts property chains and resolves metadata. Resource `_IndexDataGrid.cshtml` passes configured expressions into the grid. Nested binding subsequently extended this implementation; the old standalone phases are no longer pending.

This assessment uses the current checkout; earlier status, paths, permissions and verification notes below describe historical sessions. Runtime/browser and hosted release checks were not rerun for this documentation audit.

## Historical record

**Index status:** reference. **Indexed:** 2026-09-05. Research/specification record; read the current design before using historical examples.

**Status:** Not started as a standalone plan — this document is now the **spec for
Phase 2 of `identity-options-builder.md`** (same directory) and is executed from
there. The Identity Users index renders configured columns via
`field-for="column.FieldExpression"` directly; there is no string-field interim.
Open decision 2 below is **resolved: yes** — implement `[Display]`-based header
titles (chain: explicit `title`/header template → `ModelMetadata.DisplayName` →
PascalCase-split property name, for both `field` and `field-for` columns).
**Scope:** `stellar-admin-pro/src/StellarAdmin.Pro` (grid only). The Identity
options builder is a *future consumer*, not part of this plan.

## Goal

Let a column bind its field with a real C# lambda instead of a string, so that:

1. Page authors get IDE support (completion, compile-time checking, rename refactoring)
   for the field, which no commercial tag-helper grid offers (Telerik/Kendo and Syncfusion
   tag helpers are string-`field`; both vendors only achieve typed binding in their generic
   fluent HtmlHelper builders, e.g. `columns.Bound(p => p.Name)` / `columns.AddFor(m => m.OrderDate)`).
2. The planned Identity options builder — roughly
   `AddIdentity<TUser>(b => b.User.Columns.Add(u => u.Email))` — can capture
   `Expression<Func<TUser, TProp>>` at DI time and the Users Index view can pass each
   stored `LambdaExpression` to the grid **as-is**, no string round-trip:
   `<sa-data-grid-column title="@column.Title" field-for="column.FieldExpression"/>` inside
   a `@foreach`. (Non-string tag helper attributes are bound as C#, so a loop variable works.)

## Research findings (verified by spike, 2026-08-03)

A scratch project confirmed both mechanisms compile AND behave at runtime (the scratchpad
spike is gone; the essentials are inlined below):

- **`ModelExpression` cannot be item-rooted.** The Razor compiler emits
  `CreateModelExpression(__model => __model.X)` with `__model` statically typed as the
  *page* model; nested tag helpers cannot re-root it and tag helpers cannot be generic.
  `for="Bookings[0].Confirmed"` does work (and `For.Metadata` yields the property's
  `ModelMetadata` without evaluating `.Model`), but it forces items to be a page-model
  member expression — breaks the `items="rows"` local-variable pattern — so it was rejected.
- **The chosen mechanism works:** a tag helper property typed
  `System.Linq.Expressions.LambdaExpression` accepts an explicitly-typed lambda attribute
  value via C# 10 natural-type lambda conversion:

  ```csharp
  [HtmlAttributeName("field-for")]
  public LambdaExpression? FieldFor { get; set; }
  ```

  ```cshtml
  <sa-data-grid-column field-for="(Booking b) => b.Status"/>   @* hand-written page *@
  <sa-data-grid-column field-for="column.FieldExpression"/>    @* Identity pass-through *@
  ```

  Verified: compiles in a Razor Page, and at runtime
  `FieldFor is { Body: MemberExpression { Member: PropertyInfo prop } }` yields the
  correct `PropertyInfo` (name + declaring type). The explicit parameter type is required
  on hand-written pages (no target type to infer from); the Identity builder infers it
  from `TUser`, so that call site has zero annotation tax.
- **Boxing caveat:** a lambda built as `Expression<Func<T, object?>>` wraps value-type
  members in `UnaryExpression(Convert)`; unwrap before matching `MemberExpression`.
  (Attribute-authored lambdas with a concrete property type won't have this, but
  expressions arriving from a builder might.)

## Current architecture (as of this writing — re-read the code first; only plan the delta)

`DataGridColumnTagHelper` (`src/StellarAdmin.Pro/TagHelpers/DataGrid/DataGridColumnTagHelper.cs`):

- Two passes: collect (`gridContext.CurrentRow is null`) registers `DataGridColumn` and
  pre-resolves expensive lookups; row passes render cells. A **fresh tag-helper instance
  runs each pass**, so all cross-pass state lives on `DataGridContext`
  (`GridDisplayTemplates: Dictionary<string, IView>` keyed by template name,
  `FieldMetadata: Dictionary<string, ModelMetadata?>` keyed by field name).
- Field pipeline: value via reflection-compiled getters in `DataGridFieldGetters`
  (static `ConcurrentDictionary<(Type, string), Func<object, object?>>`, keyed on the
  *runtime* item type); metadata via `IModelMetadataProvider.GetMetadataForType(gridContext.ItemType)`
  where `ItemType` is sniffed from the first non-null item in `DataGridTagHelper`
  (⇒ empty grid = no metadata today); cell renders through
  `Template ?? metadata?.TemplateHint` grid display template, else
  `Format ?? metadata?.DisplayFormatString`, else `ToString()`.

## Design

New attribute on `sa-data-grid-column`:

- `field-for` (`LambdaExpression?`). Mutually exclusive with `field` — throw at collect
  if both set. All existing `field`-dependent features work identically: extract the
  `PropertyInfo` and treat its `Name` as the field name, feeding the **same**
  `FieldMetadata` / `GridDisplayTemplates` caches, `sortable`/`sort-field` defaulting,
  `template`/`[UIHint]`/`format`/`[DisplayFormat]` precedence. No new pipeline.
- **Extraction rule:** unwrap a `Convert`/`ConvertChecked` unary node, then require
  `MemberExpression` whose `Member` is a `PropertyInfo` and whose target is the lambda
  parameter — i.e. **direct properties only**. Chains (`u => u.Profile.City`) throw a
  clear `InvalidOperationException` at collect time ("only direct properties of the row
  item are supported"); field names, sort fields, metadata keys, and getter caches all
  assume a single property. Chains are a possible future extension, out of scope.
- **Metadata from the declaring/parameter type, not row inference:** use the lambda's
  parameter type (`FieldFor.Parameters[0].Type`) for
  `GetMetadataForType(...)` instead of `gridContext.ItemType`. Two wins: metadata (and
  `[UIHint]` templates) work on an **empty grid**, and no dependency on first-row sniffing.
  String-`field` columns keep the existing `ItemType` path.
- **Compiled getter:** at collect, `FieldFor.Compile()` once and cache on
  `DataGridContext` (e.g. `Dictionary<string, Func<object, object?>>` keyed by field
  name, wrapping the typed delegate). Row passes use it instead of
  `DataGridFieldGetters`. Careful: invoking a typed delegate on `object` needs a
  wrapper — either build `Expression.Lambda<Func<object, object?>>` around the original
  body with a cast parameter, or `DynamicInvoke` (slow — don't). Prefer rebuilding the
  expression with `Expression.Convert`.
- **Null rows / wrong runtime type:** getter receives `currentRow.Item`; if an item is
  not assignable to the parameter type, let the cast throw — that's a genuine usage error.
  Null item ⇒ null value (existing behavior).

### Open decisions — confirm with Jerrie before implementing

1. **Attribute name:** RESOLVED (2026-08-04) — `field-for`, confirmed by Jerrie
   (mirrors `asp-for`, sits beside `field`).
2. **Title default from metadata:** RESOLVED (2026-08-04) — implement. A column
   without `title`/header template defaults to `Metadata.DisplayName`
   (`[Display(Name = ...)]`, buddy-class aware), falling back to the PascalCase-split
   property name. Applies to string-`field` columns too (metadata is already resolved
   for them); note this changes their current behavior of rendering an empty header.
3. Whether `sa-data-grid-selection`'s key field should also accept an expression
   (probably later, keep out of scope unless he asks).

## Identity integration contract (context only — not in this plan's scope)

The options builder will store per-column `LambdaExpression FieldExpression` (captured
as `Expression<Func<TUser, TProp>>`, so typically *no* Convert node) plus optional
title/config, exposed non-generically (e.g. via `IUsersIndexViewModel.Columns`), and
`Areas/StellarAdmin/Views/Users/Index.cshtml` will `@foreach` columns emitting
`field-for="column.FieldExpression"`. Nothing in the grid should depend on Identity;
the only contract is: `field-for` accepts any single-property `LambdaExpression`
regardless of how it was constructed. Defaults (UserName/Email/EmailConfirmed) become
internal lambdas on `IdentityUser<TKey>` — same code path as user-configured columns.

## Phases (stop for review after each — no blanket approval)

1. **Phase 1 — `field-for` core.** Attribute + extraction + validation (mutual
   exclusion, direct-property rule, boxing unwrap) + field-name integration with the
   existing metadata/template/format/sort pipeline, still using `DataGridFieldGetters`
   for value access. Verify in the `sandbox/DataGridSpike` `GridDisplay.cshtml` page
   (add expression-bound variants of the existing columns incl. a `[UIHint]` one and a
   `[ModelMetadataType]` buddy-class one; confirm identical rendered output to their
   string-`field` twins, plus a page proving metadata works on an **empty** grid).
2. **Phase 2 — compiled getter + declaring-type metadata.** Cache the compiled accessor
   on `DataGridContext`; switch metadata lookup for expression columns to the parameter
   type. Re-verify Phase 1 pages unchanged.
3. **Phase 3 (optional, only if decision 2 says yes) — title from `DisplayName`.**

## Verification / environment notes for the implementing session

- Build: `DOTNET_SYSTEM_NET_DISABLEIPV6=1 dotnet build -p:AllowMissingPrunePackageData=true`.
- Run sandbox: `ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS=http://localhost:5206 dotnet run --no-launch-profile` — port **5206** only (5205 is Jerrie's), and stop your instances when done.
- Format only touched files, via the OSS repo: `dotnet csharpier format <paths>` from `stellar-admin/`.
- Headless checks: drive system chromium over CDP (no Playwright) if DOM assertions are needed.
- Never `git commit` — Jerrie reviews and commits himself.
