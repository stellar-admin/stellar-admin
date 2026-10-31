# Page layout

Status: index page planned (2026-10-31). POC in `sandbox/html/page-layout.html`; no library changes yet. The view, create and edit pages are a later pass.

A top-to-bottom rethink of the resource pages' layout (index, view, create and edit), so that everything a page can hold fits together: the page title, subtitle and actions, and on the index page the scopes, search, filters, grid and widgets. Everything is open, including moving the title and actions into the app header. It settles where the index page's top widgets go and whether they follow the search, scope and filters, the open question in [widgets](widgets.md).

## Inputs

- Today's pages: `_IndexPage.cshtml`, `_ViewPage.cshtml` and `_FormPage.cshtml` in `src/StellarAdmin/Areas/StellarAdmin/Views/Shared/`, and `_Layout.cshtml`, whose app header shows the brand title, which the sidebar already shows.
- The filter bar, mostly settled in the `stellar-admin-filters` worktree: `sandbox/html/filter-bar.html` there. It's a GET form with pills (field, operator, value, remove), an outline Filter button with an F shortcut when empty, and an add button and Clear once there are filters. Its prototype puts it on its own row under the scopes and the search.
- Widgets: top, aside and bottom placements on every resource page (see [widgets](widgets.md)).

## POC

Index page first (Jerrie, 2026-10-30); the view, create and edit pages follow once it's settled. `sandbox/html/page-layout.html` renders design A; `page-layout/layout.js` and `layout.css` hold it. `?scopes=many&controls=hide` sets the panel's state from the URL.

Rules the design keeps: a widget is always the real widget card in the library's widget grid (the layout only places cards, it never reinterprets their content); top widgets sit between the page header and the list, never between a list control and the table; the scopes, search, filters, table and pager form one list unit; aside widgets line up with the top of the list.

- **A. The list is one panel.** Page header, top widgets, then one card holding the scopes, the search, the filter bar, the table and the pager. Its list controls are locked in (Jerrie, 2026-10-31):
  - The scopes are underline tabs across the top of the card, the indicator on the card's divider and the counts as small badges (chosen over folder tabs, a segmented control, pills and a menu button, 2026-10-30).
  - The search sits at the right of the scopes' row. The scopes that don't fit beside it move into a More tab, and the selected scope is always a tab.
  - The filter bar gets the row below, as one unit as the filters worktree has it: a Filter button when empty; pills, an add button and Clear once filters are active. It starts at the left, so the button turns into the pills in the same place.
  - Without scopes, the filter bar and the search share one row, the filter bar on the left and the search on the right.
  - A narrow card (under 44rem, a container query on the card) puts the search under the scopes; the card's width alone decides, so pages with different scopes look the same at a given width.
  - Rejected: the search in its own row under the scopes, the pills beside the search, the Filter button beside the search with the pills on a row below (the pills and their button on opposite sides), and the Filter button before the scopes. Accepted cost: a row holding only the Filter button on pages with scopes and no active filters, and an empty right side of the scopes' row when there's no search.
- **C. Title bar, sticky list controls**, the title, count and actions in a sticky top bar and the list's controls sticking under it, was removed from the prototype (Jerrie, 2026-10-31); A is the design.

Jerrie (2026-10-30): the title in the bar is attractive, but its costs (small title, no description, truncated record titles) are too big, so the title stays in the page; B (open page, no card around the list) was dropped. A research deck on how other products place the title is at https://claude.ai/artifact/HASCyqK4VGMVNthQUMi8dP.

The panel toggles only what the page holds, never where things go: the scopes (none, three, seven), their counts, the search, the filters (off, none, two, seven), the widgets, the theme and the mode.

Two earlier rounds (three directions across all four pages, then ten index sketches) were replaced: they moved blocks around without designing them, and several turned widgets into plain numbers, which a widget can't be.

Narrow screens (checked at 390px and 768px, 2026-10-31): the scopes keep the selected tab and More, the filter pills wrap, the table scrolls inside the card, and the page never scrolls sideways. Once the search is stacked it fills its row (Jerrie, 2026-10-31: "the best we'll get on a narrow screen").

## Implementation (index page)

Scope (Jerrie, 2026-10-31): the index page only; the view, create and edit pages get their own pass. The overflow into More belongs to the tab list in StellarAdmin.UI, on by default with an opt-out, not to the admin panel.

### Phase 1: tab list overflow (StellarAdmin.UI)

A horizontal `<sa-tab-list>` keeps as many tabs as fit on one line and moves the rest into a More menu at its end. It applies to both kinds of tabs: `<sa-tab-link>`s that link to pages and `<sa-tab>`s that switch panels.

- API: `Overflow` (`overflow`), a new `TabListOverflow` enum with `Menu` (the default) and `None`, which keeps today's behaviour. `MoreLabel` (`more-label`) sets the More trigger's text, default "More". Vertical lists ignore both. An enum rather than a bool so that other modes, such as scrolling, can follow (Jerrie, 2026-10-31).
- Markup: the tag helper renders the More trigger (a tab-styled button with a chevron, `popovertarget`, hidden until needed) at the end of the list and an empty dropdown menu after it. `sel-tab-list` fills the menu with copies of the hidden tabs on every fit: a link tab's item is a link with the tab's attributes (so `href` and htmx attributes carry over), a panel tab's item selects that tab. The host becomes a `sel-tab-list` web component, so lists that htmx swaps in fit themselves with no re-initialization.
- Fitting (`sel-tab-list.js`): measures the tabs and reserves the selected tab first (`aria-selected="true"`), then adds the others in order while they and the More trigger fit; the rest get `hidden`, and so do the menu items of the tabs that are shown. More is hidden when everything fits. It re-fits on a `ResizeObserver` of the host and after `document.fonts.ready`.
- Before the script runs the list clips instead of widening the page (`overflow: hidden; contain: inline-size` on a list that has a menu), the fix the POC needed when the tabs pushed `main` to 1384px.
- Panel tabs: choosing an item selects its tab through `sel-tabs` (which then always shows as a tab), and the arrow keys skip hidden tabs.
- Styling: both variants (default and line); the More trigger looks like a tab.
- Tests: tag helper tests for the default menu, `None`, vertical lists and `more-label`; a CDP check of fitting, the selected tab staying visible, resizing, htmx swaps and selecting a panel tab from the menu.
- Docs: DocsSamples demo, the website's Tabs page (overflow section, opt-out), the generated skills reference.

Phase 1 implemented (2026-10-31, uncommitted). Beyond the bullets above: it re-fits when the host's parent resizes too, because a list only as wide as its tabs doesn't grow when there's room again, and it allows half a pixel for fractional tab widths (without it, lists that exactly fit showed More). DocsSamples has Overflow, Overflow None and Panels Overflow demos, registered in `DemoGenerator`. Verified: `StellarAdmin.UI.Tests` (807), `StellarAdmin.UI.IntegrationTests` (19), `StellarAdmin.Tests` (340) and `StellarAdmin.IntegrationTests` (740) pass; a CDP run against DocsSamples on :5206 at 1280px and 390px checked that the selected tab stays shown, the menu holds the hidden tabs, widening and narrowing (also in a content-sized flex parent) move tabs in and out, a cloned (swapped-in) list fits itself, picking a panel tab from the menu selects, shows and focuses it, the arrow keys skip hidden tabs, every other Tabs demo shows all its tabs, there are no ResizeObserver errors and the page never scrolls sideways. The website export (`docs generate demos`) and `skills generate`/`skills check` ran; the website diff is the Tabs page, the three new demos and the usual asset hash changes.

### Phase 2: index page layout (StellarAdmin)

`_IndexPage.cshtml` follows design A.

- The scopes and the search move out of `<sa-page-header-nav>` into the list card, above the grid: the scopes as a line `<sa-tab-list>` on the left (overflow on), the search on the right of the same row, 16rem wide. Without scopes, the search row is the filter bar's row.
- The card: the page wraps the list controls and `#index-page-data-grid` in one `sa-card`, and admin panel CSS drops the grid's own card frame inside it (border, radius, shadow), so the pager stays the card's footer and the data grid needs no new API. The search stays outside the swapped grid, so typing never loses focus; the scopes stay an out-of-band swap as today.
- Narrow cards: a container query on the card (under 44rem) puts the search under the scopes at full width, the tabs running edge to edge with their own divider.
- The filter bar's row: the layout leaves the row under the scopes for the filter bar from the `stellar-admin-filters` worktree, which renders there when that work merges (and shares the search's row when there are no scopes). Until then the row is not rendered.
- Widgets: no change to their placement; aside widgets line up with the card's top because the scopes are now inside it.
- Tests and checks: update the index page's rendering tests; CDP screenshots at 390, 768 and 1440px with no scopes, three and seven, with and without search and widgets; the docs screenshots of the index page that the move changes, through write-docs-screenshot-specs.

### Not in this plan

- Scope counts: the POC shows them, but they're not included (Jerrie, 2026-10-31); a scope has no count today and each one would cost a count query per page load.
- Hiding the Filter button's F hint on touch screens and Clear's line of its own with seven filters belong to the filters work.
- The view, create and edit pages.
