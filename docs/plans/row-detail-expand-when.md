# Row detail expand rules

Status: deferred (2026-10-12): no demonstrated need. Revisit when an application needs rows that cannot expand; see [Deferral](#deferral) for the route to take then. Follows [dashboard-row-details](archive/dashboard-row-details.md), which deferred `ExpandWhen`, and builds on [table-row-details](oss/archive/table-row-details.md).

## Goal

Let an index say, per row, whether the row can expand, and optionally why not. A row that cannot expand shows no toggle, or a dimmed toggle whose tooltip gives the reason, built from the row's own data.

## Deferral

Jerrie deferred this on 2026-10-12, before any code was written, because it adds API for a need nobody has shown. Until then, a row whose details have nothing to show can expand to an empty state written in its partial, which already has the row's data (prototype option 5).

Comparable admin panels and data grids (recalled, not checked against current docs) offer at most a per-row boolean that hides the toggle, with no reason: React Admin `isRowExpandable`, Ant Design Table `expandable.rowExpandable`, AG Grid `isRowMaster`, MUI X Data Grid (a null detail panel), PrimeReact (a per-row expander function). Filament and Laravel Nova have no comparable expandable rows.

When a requirement comes up, start from the smallest version, which supersedes the API below:

- Dashboard only: `details.ExpandWhen(Func<TResource, bool>)`: rows it rejects get no toggle, as in the libraries above; no reason, dimmed toggle or tooltip unless the requirement asks for one.
- The data grid cannot skip a row's detail row from outside, so the library gains one marker element rendered by the row template, such as `<sa-data-grid-row-detail-unavailable />`, which makes the grid omit that row's detail row and toggle. Grid users get it through an ordinary `@if`.
- Not an `expand-when` lambda attribute on `sa-data-grid-row-detail` (it needs a `Func<object, …>` cast and does not read like Razor), not `unavailable-reason` on `sa-table-row-detail-toggle` (hand-written tables can omit the detail row already), and no internal-only hook from TagHelpers to the Dashboard.

The sections below record the proposal as reviewed, including the reason variant, for reference.

## Decisions (Jerrie, 2026-10-12)

1. Feedback: no indicator when there is no reason (prototype option 1), a dimmed toggle with the reason as its tooltip when there is one (option 3). Prototype: `sandbox/html/row-details-expand-when.html`.
2. The rule returns a result object with whether the row can expand and an optional reason, so the reason can use the row's data (`$"{product.Name} …"`), rather than a predicate with one static reason.

## Proposed (awaiting confirmation)

3. The result type is `RowDetailExpansion` with `Allow()` and `Deny(string? reason = null)`, and lives in TagHelpers so the data grid can use it; the Dashboard reuses it.
4. The rule is synchronous and runs on the row's item, the one already loaded for the grid. No async overload: that would be a call per row. On EF Core resources, navigation properties the index does not load are null in the rule.
5. The rule shapes the UI only. The row detail endpoint receives an id, not the row's item, so it does not run the rule; a handler that must not reveal details enforces that itself (return null). The docs say so.
6. One rule for the row's whole details. Hiding single panels per row (`panel.ShowWhen(...)`) is out of scope.
7. The `pointer-events` fix for dimmed buttons stays scoped to row detail toggles. `button.css` sets `pointer-events: none` on `aria-disabled` buttons, which is what stops their tooltips; other components (pager links, menu items, tab links) rely on it to block clicks, so a global change is a separate decision.

## API

```csharp
public readonly record struct RowDetailExpansion
{
    public bool CanExpand { get; }
    public string? Reason { get; }

    public static RowDetailExpansion Allow();
    public static RowDetailExpansion Deny(string? reason = null);
}
```

Dashboard:

```csharp
index.EnableRowDetails(panels => { ... }, details =>
{
    details.ExpandWhen(product => product.Price >= 10
        ? RowDetailExpansion.Allow()
        : RowDetailExpansion.Deny($"{product.Name} is priced per unit, so it has no pricing details"));

    // Or, without a reason:
    details.ExpandWhen(product => product.Price >= 10);
});
```

Tag helpers:

```razor
@* Data grid: the rule runs on each row's item *@
<sa-data-grid-row-detail expand-when="(Func<object, RowDetailExpansion>)(item => ...)">

@* Hand-written tables: omit the detail row; for a reason, render a dimmed toggle *@
<sa-table-row-detail-toggle unavailable-reason="@($"{booking.Id} has no itinerary yet")" />
```

## Phase 1 — Library

- `RowDetailExpansion` (TagHelpers, `StellarAdmin.TagHelpers` namespace).
- `sa-table-row-detail-toggle unavailable-reason="…"`: renders the toggle with `aria-disabled="true"`, an accessible name built from the reason, no `aria-expanded`, and an `sa-tooltip` carrying the reason through `interestfor`. The row has no `sa-table-row-detail`. Without the attribute, behavior is unchanged.
- `sa-data-grid-row-detail expand-when`: `Func<object, RowDetailExpansion>`, evaluated per row before the row template runs. A denied row renders no detail row and does not run the template; its toggle cell is empty, or holds the dimmed toggle when there is a reason. `expanded-keys` ignores denied rows. A trailing toggle column and `toggle="None"` behave the same way.
- CSS (`table.css`): `.sa-table-row-detail-toggle[aria-disabled="true"]` keeps pointer events (for the tooltip) with a not-allowed cursor. `sel-table-row-details` needs no change: row click and expand-all already skip rows without a detail row, and the toggle without a detail row does nothing.
- Expand-all stays in the header even when no row on the page can expand; it then does nothing.
- Tests: toggle tag helper (reason, accessible name, tooltip, no `aria-expanded`), data grid (denied rows render no detail row and no template output, empty cell versus dimmed toggle, `expanded-keys` ignores denied rows, the template is not run for denied rows).
- DocsSamples: Table "Row Details Unavailable" (one row without a toggle, one with a reason) and DataGrid "Row Details Expand When". Export to the website with sections on the table and data grid pages and the API tables; skills examples.

## Phase 2 — Dashboard

- `ResourceRowDetailsBuilder<TResource>.ExpandWhen(Func<TResource, RowDetailExpansion>)` and `ExpandWhen(Func<TResource, bool>)`; repeated calls replace the rule. Stored on `ResourceRowDetailsOptions` as an internal `Func<object, RowDetailExpansion>`.
- `_IndexDataGrid` passes it to `expand-when`; the row counter for panel ids is unaffected.
- Integration tests: denied rows render no detail row and no partial, the reason renders as the toggle's tooltip, the bool overload, and an inline partial is not rendered for a denied row.
- Playground: Products denies rows under 10.00 with a reason naming the product; Customers denies one case without a reason.
- Docs: the Dashboard skill's `setup.md` row details section (the rule, sync, UI only).

## Out of scope

- Per-panel rules, async rules, and running the rule on the row detail endpoint.
- Changing `pointer-events` for `aria-disabled` buttons outside row detail toggles.

## Verification log

None yet.
