# Page subtitles: record summary

Status: in progress (2026-11-17). Implemented and tested; docs and consumer skills outstanding.

A summary of the record under the title of the view and edit pages, such as a product's sku and category under its name. Split out of [page layout](archive/page-layout.md)'s page header pass (Jerrie, 2026-11-01). It started as a subtitle; once the mockups showed badges, icons and statuses it became a summary of the record (Jerrie, 2026-11-17).

## Prototypes

- `sandbox/html/record-pages.html`: the original POC, `BAG-011 · Bags` under `Waxed Canvas Daypack`.
- `sandbox/html/page-subtitles.html` (2026-11-17): styles for the line (plain, mono identifier, badges, icon row, eyebrow, beside the title, status badge, copyable identifier, folding away when stuck), the line across the pages, fixed text against a per-record line, and other resources. Jerrie found most of them valid; the question was the API that lets developers compose them.

## Today

- `<sa-page-header-description>` shows a line under the title.
- `_FormPage.cshtml` renders `ResourceFormPageViewModel.Subtitle` in the description, but nothing sets the property, so it's always empty. `_ViewPage.cshtml` renders no description.
- Display templates render only the value. `<sa-display-field>` (`DisplayFieldTagHelper`) adds the label on the view page; the column header is the label in the grid.

## API

A `Summary(...)` scope on the resource, shown on the view and edit pages. A page replaces it with its own `Summary(...)` or hides it with `Clear()`.

```csharp
products.Summary(summary =>
{
    summary.SeparateItems = true;
    summary.Add(p => p.Sku);
    summary.Add(p => p.Category.Name);
    summary.Add(p => p.Status, item => item.UseDisplay<BadgeDisplay>());
});

products.AllowEdit(edit => edit.Summary(summary => summary.Clear()));
```

With text items and explicit separators:

```csharp
products.Summary(summary =>
{
    summary.AddText("Sku");
    summary.Add(p => p.Sku);
    summary.AddSeparator();
    summary.AddText("Category");
    summary.Add(p => p.Category.Name);
});
```

- `ResourceBuilderBase.Summary(Action<ResourceSummaryBuilder<TResource>>)`: the resource's summary, for the view and edit pages whose model is the resource. A page with its own model (`AllowView<TModel, THandler>`, `AllowEdit<TModel, THandler>`) sets its own.
- `ResourceViewBuilder.Summary(...)` and `ResourceEditBuilder.Summary(...)`: the page's summary, replacing the resource's.
- `ResourceSummaryBuilder<TModel>`:
  - `Add(expr)` returns a `ResourceSummaryItemBuilder`; `Add(expr, configure)` returns the summary builder, as `ResourceViewFieldsBuilder` does.
  - `AddText(string)`: plain text, printed as given. Nothing is added to it (no `:`). It labels an item, or gives fixed text.
  - `AddSeparator()`: a separator (`·`) at that point.
  - `SeparateItems` (bool, default `false`): a separator between every two items. Setting it and also calling `AddSeparator()` is a configuration problem.
  - `Clear()`: removes the items, so the page shows no summary.
- `ResourceSummaryItemBuilder`: dedicated, not `ResourceDisplayFieldBuilder` (whose `ColumnSpan` and `ColumnStart` mean nothing here). `UseDisplay<TDisplay>()`, `UseDisplay<TDisplay>(configure)` and `UseBuiltInRendering()`, as on the display field builder.

## Behaviour

- The summary renders under the title, in the page header, as a wrapping row of items.
- A field item renders the compact display template, the one index grid cells use today (`GridDisplayTemplates/`); no new template set. The [template folders](template-folders.md) plan renames the folders.
- Only the existing display types; no new item features (icons per item, copy buttons, mono styling).
- No separators are inferred: only `AddSeparator()` or `SeparateItems` adds them.
- An empty value shows the field's empty text, as on the view page; the item stays.
- The edit page shows the stored record, also after a save the server rejects, not the posted values.
- With EF Core, the record loads what the summary's expressions read (`Category` in the example), as for the view page's fields.

## Decided against

- A fixed subtitle or description property on the pages (`Subtitle`, `Description`): a summary with only an `AddText(...)` item gives fixed text on the view and edit pages.
- A line on the create page (no record) and on the index page (many records).
- Layout or placement options: one layout, under the title. No eyebrow above it, nor beside it.
- `ResourceFormPageViewModel.Subtitle` goes, replaced by the summary.

## Implementation (2026-11-17)

- Builders: `ResourceSummaryBuilder<TModel>`, `ResourceSummaryItemBuilder` (wraps the display field builder), `Summary(...)` on `ResourceBuilderBase`, `ResourceViewBuilder` and `ResourceEditBuilder`. Options are internal: `ResourceSummaryOptions` on the resource, view and edit options.
- `SummaryCheck`: gives a page whose model is the resource the resource's summary, unless it has its own; `SeparateItems` with `AddSeparator()` reports `SeparatorWithSeparateItems` (1110) and drops the added separators.
- The controller reads the summary's values and prepares its displays when it loads the record, before the edit POST binds (`PageSummaryViewModel` in view data). `_PageSummary.cshtml` renders it under the title in `_ViewPage` and `_FormPage`; `PageSummaryRenderer` renders a field's `GridDisplayTemplates/` template. CSS `.sa-page-summary` in the admin panel's `client.css`; it shows at every width, unlike a description in a narrow sticky header.
- EF Core: the view and edit pages' summary expressions join to `Include` paths with their display fields.
- AdminPlayground: a resource summary on products (view page) and an edit summary on the wrapped edit model.
- Checks run: new tests `ResourcePageSummaryTests` (9), `ResourceSummaryBuilderTests` (2), and two EF Core include tests; full `StellarAdmin.Tests` (360), `StellarAdmin.IntegrationTests` (770) and `StellarAdmin.EntityFrameworkCore.IntegrationTests` (172) pass. Checked in the AdminPlayground: view and edit pages, and a rejected edit save keeping the stored values.

## Outstanding

- The docs page, the consumer skill references and a docs screenshot.
