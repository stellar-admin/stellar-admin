# Index page refreshing

Status: open (2026-10-31).

Rework how the index page refreshes with htmx so that a change replaces only what it changes. Today the whole page flashes as it refreshes everything, the widgets included.

- The scopes are boosted links (`hx-boost:inherited` on `#index-page-scopes`) with no target, so picking a scope swaps the whole body: the header, the card and every widget region load again.
- The search swaps `#index-page-data-grid` and, out of band, the scopes and every widget region (`_IndexPageList.cshtml`). The grid's paging, sorting and page size links swap the grid and, out of band, the search and the scopes (`_IndexPageGrid.cshtml`).
- After a search and then a scope click, the search box is empty although the URL, the input's `value` attribute and the rows keep the term (found 2026-10-31 on the committed code).
- Related: [widgets](widgets.md) left open whether widgets refresh when the grid's search or scope changes; the filter bar from the `stellar-admin-filters` worktree will add another control that refreshes the grid.

To be filled out before implementing.
