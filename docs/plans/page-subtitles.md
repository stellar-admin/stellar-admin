# Page subtitles

Status: proposed (2026-11-01).

A line under the title of the view, create and edit pages, such as a product's sku and category under its name. Split out of [page layout](page-layout.md)'s page header pass (Jerrie, 2026-11-01). The POC `sandbox/html/record-pages.html` shows it as `BAG-011 · Bags` under `Waxed Canvas Daypack`.

- `<sa-page-header-description>` already shows a line under the title. What's missing is configuration that produces it.
- `_FormPage.cshtml` renders `ResourceFormPageViewModel.Subtitle` in the description, but nothing sets the property, so it's always empty. `_ViewPage.cshtml` renders no description.
- Two shapes, from the review on 2026-11-01:
  - A fixed text per page (`edit.Subtitle = "..."`), filling the existing property. The same text for every record.
  - A per-record subtitle set like `UseTitle` (`resource.UseSubtitle(p => $"{p.Sku} · {p.Category.Name}")`), evaluated on the loaded record on the view and edit pages. New public API; with EF Core, the record has to load what the expression reads.
- Open: which shape, or both; whether the create page gets one; whether the index page's header gets a description too.

To be filled out before implementing.
