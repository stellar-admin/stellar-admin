# Plan: Data grid nested-property field binding

## Code audit — 2026-09-18

Current status: **completed**. DataGridColumnTagHelper walks expression/string property chains and resolves leaf metadata; the current grid is in TagHelpers rather than the historical Pro project.

This assessment uses the current checkout; earlier status, paths, permissions and verification notes below describe historical sessions. Runtime/browser and hosted release checks were not rerun for this documentation audit.

## Historical record

**Index status:** completed. **Indexed:** 2026-09-05. Historical record; completion is recorded in the phase/progress notes below. Deferred items require a separately scoped task.

**Goal:** `field-for="(Booking b) => b.Customer.Name"` and (decision 2 permitting)
`field="Customer.Name"` render nested property values, with metadata (`[Display]`,
`[UIHint]`, `[DisplayFormat]`) resolved from the leaf property. Closes the known gap
from the identity options builder plan: `columns.Add(u => u.Department!.Name)` is
accepted by the builder (Jerrie's decision, 2026-08-04) but currently 500s when the
grid renders. After this plan, those columns just work — no identity-layer change
required.

**Repos touched:** `stellar-admin-pro` only (grid + spike; tiny identity design-doc
note). Plan lives in the workspace repo.

## Current state (verified 2026-08-04)

- `DataGridColumnTagHelper.ExtractProperty` requires
  `MemberExpression { Expression: ParameterExpression }` — chains throw the designed
  "Only a property selected directly on the row item" error at collect.
- String fields fail earlier: `GetFieldMetadata` →
  `GetMetadataForProperty(type, "Customer.Name")` throws `ArgumentException` (no such
  property) at collect on a non-empty grid; the getter would also fail
  (`type.GetProperty("Customer.Name")` is null).
- `DataGridFieldGetters`: single app-lifetime cache,
  `ConcurrentDictionary<(Type, string), Func<object, object?>>`, two `GetValue`
  overloads — string path keys on **runtime** item type, expression path keys on the
  lambda **parameter** type with the extracted `PropertyInfo`. Overlap-safe because
  both compile identical getters. (Preserve this property — see design.)
- `sa-data-grid-selection` `key-field` reads through the string getter path
  (`DataGridTagHelper.cs:291`), so it inherits whatever the string path supports.
- `sort-field`/`{sort}` URLs pass strings verbatim — dotted values already work.
- Empty-string attribute normalization (title/template/format → null) is in place.

## Design

### 1. Expression path (`field-for`)

`ExtractProperty` becomes `ExtractPropertyChain`: unwrap the boxing `Convert`, then
walk `MemberExpression`s down to the `ParameterExpression`, requiring every link's
`Member` to be a `PropertyInfo`. Produces the chain root-first
(`[Customer, Name]`) plus the dotted path `"Customer.Name"`. Anything else (method
calls, indexers, casts mid-chain) keeps throwing the existing error, message updated
to say a property or property chain is supported.

- **Field name** (metadata cache key, sort default, template requirement) = the
  dotted path.
- The getter is compiled from the `PropertyInfo` chain itself, so C# expression
  semantics (explicit interface implementations, shadowed properties) are preserved
  per segment — same rationale as the existing single-property asymmetry.

### 2. String path (`field="Customer.Name"`) — decision 2

Split on `'.'`; resolve segment 1 by reflection on the item's **runtime** type (as
today), subsequent segments on the previous property's declared type. A segment with
no matching public property throws the existing clear message, extended to name the
failing segment. Selection `key-field` inherits dotted support automatically.

### 3. Getter compilation with null-propagation

One shared `CreateFieldGetter(Type type, IReadOnlyList<PropertyInfo> chain)` used by
both paths (this keeps the overlapping-cache-key safety argument: same (type, dotted
path) ⇒ identical compiled getter). Cache key stays `(Type, string)` with the dotted
path as the string. Compiled shape, per decision 1 (recommend render-empty):

```
(object item) => {
    var v0 = ((T)item).Customer;      // segment 1
    if (v0 == null) return null;      // null check only for nullable segment types
    return (object?)v0.Name;          // leaf — no null check needed on the result
}
```

Null checks are emitted only for reference-type / `Nullable<T>` intermediate
segments (a non-nullable struct can't be null; `Expression.Equal` against null
wouldn't compile). `Nullable<T>` intermediates access `.Value` after the check.

### 4. Metadata: segment-wise walk

`GetFieldMetadata` resolves the **leaf** metadata by walking the chain through the
provider: `GetMetadataForProperty(sourceType, "Customer")` → its `ModelType` →
`GetMetadataForProperty(modelType, "Name")`. Each hop goes through the framework
pipeline, so buddy classes (`[ModelMetadataType]`) and custom providers are honored
at every level. Cache stays keyed by the dotted path in
`gridContext.FieldMetadata`. Expression columns walk from the lambda parameter type
(empty-grid support intact); string columns from the runtime `ItemType`. A typo'd
segment keeps failing fast at collect (the extension method throws), now with the
correct container type per segment.

### 5. Header title default

Chain unchanged: explicit `title`/header template → leaf `ModelMetadata.DisplayName`
→ PascalCase-split **leaf** property name (decision 3; `"Customer.Name"` → "Name",
not "Customer. Name"). `SplitPascalCase` input switches to the leaf segment.

### 6. What deliberately does NOT change

- `sort-field`/pager/selection markup contracts.
- The two-key-strategy asymmetry in `DataGridFieldGetters` (runtime vs declared
  type) — it extends to chains as-is (only the *first* segment's resolution differs
  between the paths).
- Identity layer: no code change. `IdentityGridColumn.FieldName` stays best-effort
  leaf-name (nothing consumes it). Design doc's Columns note gets a one-line update:
  nested properties now supported end-to-end.

## Decisions (all confirmed by Jerrie, 2026-08-04)

1. **Null mid-chain: render empty** — null propagates into the existing null-value
   handling (empty cell / template receives null), matching a null leaf value today.
2. **String-field dotted support: yes** — `field="Customer.Name"` works, keeping
   `field`/`field-for` symmetric; selection `key-field` shares the code path.
3. **Default header for nested: leaf name** — `b.Customer.Name` → "Name";
   disambiguate with `title`/`.Title()` or `[Display]` on the leaf.

## Progress

- **PLAN COMPLETE (2026-08-05): Phase 1 signed off by Jerrie; Phase 2 (docs-only)
  done** — design doc Columns note updated (nested works end-to-end, leaf metadata,
  null-mid-chain renders empty, unbindable shapes fail at render), memory updated.
  All changes uncommitted for Jerrie's review.
- **Phase 1: COMPLETE (2026-08-04), signed off 2026-08-05.** Delivered
  (uncommitted, pro repo):
  - `DataGridColumnTagHelper`: `ExtractProperty` → `ExtractPropertyChain` (root-first
    `PropertyInfo[]`, walks MemberExpressions to the parameter; updated error message
    "Only a property or chain of properties…"); `fieldName` = dotted path joined from
    the chain; `GetFieldMetadata` walks dotted paths segment-by-segment through
    `GetMetadataForProperty` (leaf metadata returned, buddy classes honored per hop,
    typo'd segments fail at collect); header default splits the **leaf** segment;
    `GetFieldValue` passes the chain + dotted name to the getter cache.
  - `DataGridFieldGetters`: expression overload now
    `GetValue(item, declaredType, field, propertyChain)` (dotted name passed in to
    avoid a per-row join); string `CreateFieldGetter` resolves dotted paths by
    reflection (first segment on the runtime type, rest on declared property types,
    per-segment error message); both paths compile through one
    `CreateFieldGetter(type, chain)` using `BuildChainAccess` — nested blocks with a
    variable per nullable intermediate and a null-conditional (null mid-chain →
    null), non-nullable struct intermediates read straight through. Overlap-safety
    argument preserved (same (type, dotted path) ⇒ identical getter).
  - Spike: new `SpikeVoyage.cs` (`SpikeVoyage` → `SpikeSkipper?` → `SpikeHarbor?`,
    leaf `[Display]`/`[DisplayFormat]`/`[UIHint("SpikeDate")]`), new
    `NestedFields.cshtml` (A/B string-vs-expression twins over 1- and 2-level
    chains, null-mid-chain rows, header defaults, empty grid). `FieldForBad.cshtml`
    restructured: the old chained case is now *valid*, so the default case is a
    method call (`b.Reference.Trim()`); new `?case=string-typo` proves collect-time
    failure ("The property System.String.Missing could not be found").
  - Verified on 5206 (scratchpad `nested-verify.mjs`): 17/17 — twins byte-identical,
    leaf format "14.2 m", `[UIHint]` template receives null mid-chain ("Not
    scheduled"), empty cells for null intermediates, leaf headers
    ("Skipper"/"Years At Sea"/"City"), empty-grid chain headers. Error cases: both
    attrs, method call, string typo — all throw as designed. Regressions: Phase 2
    `fieldfor-verify.mjs` 17/17; identity playground `phase3-verify.mjs` 14/14;
    Selection/GridDisplay/Sorting smoke 200s. Touched files CSharpier-formatted;
    builds clean (0 warnings); 5206 stopped.

## Phases

### Phase 1 — Grid implementation + spike verification

`DataGridColumnTagHelper` (`ExtractPropertyChain`, metadata walk, leaf title) and
`DataGridFieldGetters` (chain compilation, null-propagation, doc updates for the
extended key strategy). Verify in `sandbox/DataGridSpike`, new `NestedFields.cshtml`
page + a nested model (e.g. give `SpikeBooking` scenarios a `SpikeCustomer` with a
nested `SpikePort`), asserting:

- string vs expression twin grids over a nested property render identically
  (byte-identical tables, as `FieldFor.cshtml` does);
- two-level chain (`b.Customer.HomePort.City`);
- null mid-chain renders an empty cell (and a templated nested column receives
  null);
- leaf `[Display]` title, leaf `[UIHint]` template, leaf `[DisplayFormat]`;
- default header = split leaf name;
- empty grid: nested `field-for` headers/metadata still resolve;
- `FieldForBad` still rejects a method call (`b => b.Reference.Trim()`) with the
  updated message; string field with a typo'd middle segment fails at collect;
- re-run `fieldfor-verify.mjs` (17 assertions) + FieldForBad cases — no regression;
- selection `key-field` smoke check unaffected.

Server on **5206**, stopped afterwards; touched files CSharpier-formatted.
**Checkpoint.**

### Phase 2 — Docs only (decision Jerrie, 2026-08-04)

**No playground nested-property demo** — Jerrie will add real nested properties to
his own models at a later stage; the spike coverage suffices. Update
`stellar-admin-pro/docs/design/identity-configuration.md` Columns note (nested now
works). Update this plan + memory. **Checkpoint — end of plan.**

## Environment notes

Same as the identity plan: build with `DOTNET_SYSTEM_NET_DISABLEIPV6=1` +
`-p:AllowMissingPrunePackageData=true`; run on 5206 only, stop when done; format via
`dotnet csharpier format <paths>` from `stellar-admin/`; never `git commit`.
