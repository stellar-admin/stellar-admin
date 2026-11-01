# Page layout

Status: completed (2026-11-01). The index page (2026-10-31) and the page header pass on the view, create and edit pages (2026-11-01) are implemented and committed. POCs in `sandbox/html/page-layout.html` and `sandbox/html/record-pages.html`.

A top-to-bottom rethink of the resource pages' layout (index, view, create and edit), so that everything a page can hold fits together: the page title, subtitle and actions, and on the index page the scopes, search, filters, grid and widgets. Everything is open, including moving the title and actions into the app header. It settles where the index page's top widgets go and whether they follow the search, scope and filters, the open question in [widgets](../widgets.md).

## Inputs

- Today's pages: `_IndexPage.cshtml`, `_ViewPage.cshtml` and `_FormPage.cshtml` in `src/StellarAdmin/Areas/StellarAdmin/Views/Shared/`, and `_Layout.cshtml`, whose app header shows the brand title, which the sidebar already shows.
- The filter bar, mostly settled in the `stellar-admin-filters` worktree: `sandbox/html/filter-bar.html` there. It's a GET form with pills (field, operator, value, remove), an outline Filter button with an F shortcut when empty, and an add button and Clear once there are filters. Its prototype puts it on its own row under the scopes and the search.
- Widgets: top, aside and bottom placements on every resource page (see [widgets](../widgets.md)).

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

## Page header pass (index, view, create and edit)

Designed with Jerrie on 2026-10-31 and 2026-11-01. POC: `sandbox/html/record-pages.html` (`record-pages/pages.js`, `pages.css`), Today and New for each page, built from markup captured from the AdminPlayground (the products index, view, create and edit pages, customers and form-mixed-editors). The product's Pricing and Inventory sections are prototype data that give the form some length.

Scope (Jerrie, 2026-11-01): only the page title, the page's buttons and the header they sit in. Fields, sections, cards, controls and the layouts of the pages stay as they are. The breadcrumb in the app header moved to [app header breadcrumb](../app-header-breadcrumb.md), the unsaved changes indicator to [unsaved changes](../unsaved-changes.md), and the line under the title to [page subtitles](../page-subtitles.md).

How the design was chosen: a first round had three directions per page (A: section cards with a save bar stuck to the bottom of the window; B: a sticky header holding the actions, with section jump tabs; C: one card with the actions in its footer). Jerrie chose B for its consistency (the actions in the same place on every page, always visible), after comparing how other admin panels and content tools place Save: content tools keep it at the top, admin frameworks at the bottom of the form, Shopify in a bar that appears on changes. The section jump tabs, the label and value rows on the view page and the sections in one card were dropped as layout changes outside the scope.

### Design

- **A sticky page header on every resource page.** The page header sticks to the top of the window as the page scrolls, with a rule under it and a translucent canvas background, so the title and the page's actions stay in view. On a phone it keeps the title and the actions and hides the description.
- **The title names the record** on the view and edit pages. The page's own `Title` setting wins, then the record's title (`UseTitle`, or the `[DisplayColumn]`, `Name` or `Title` property), then today's label pattern (`ConfigureResourceLabels`, View Product and Edit Product). A record whose title would only be its key gets the label pattern, since the developer needs to give the resource a proper title. The browser tab's title follows the page title.
- **Index page:** the title and Create, as today, in the sticky header.
- **View page:** Delete, Back and Edit in the header. Delete is an outline destructive button (outline, not ghost and not today's solid destructive). Edit stays the primary button. Back stays until the [app header breadcrumb](../app-header-breadcrumb.md) lands.
- **Edit and create pages:** Cancel and Save (Add on create, and Save and add another when enabled) move from the bottom of the form to the header, in the same place as Edit and Create. The form loses its bottom row of buttons. The edit page has no Delete button (Jerrie, 2026-11-01): people reach it from the index page, which has Delete on each row, and the header can't move a destructive button away from Save the way the footer could.
- **The order of the header's actions:** on the form page, the `form-leading-actions` slot, `pre-form-actions`, Cancel, Save and add another, Save, `post-form-actions`; on the view page, `pre-page-actions`, Delete, Back, Edit, `post-page-actions`, with Delete first to keep it as far from Edit as the header allows; on the index page, as today.
- **Widgets:** the edit and create pages' aside widget column already sticks; it now sticks under the header instead of sliding beneath it.

### Phase 1: StellarAdmin.UI

- `ButtonVariant.OutlineDestructive`, appended at the end of the enum: an outline button with the danger text colour and a soft danger fill on hover, in `button.css`, reading the existing danger tokens.
- `Sticky` (`sticky`) on `<sa-page-header>`, off by default so other pages don't change: the header sticks to the top with a rule under it and a translucent canvas background, and on a phone hides its breadcrumb and description. It reaches the page's edges with negative margins of the page container's padding, so it does so only as a direct child of `<sa-page-container>`.
- The page container's padding becomes a foundation token (for example `--sa-space-page`, 5 units and 8 from the md breakpoint), read by `page-container.css` and `page-header.css` and listed in `docs/design/tokens.md`.
- Tag helper tests, DocsSamples demos (the variant, a sticky header over a long page), the website's Button and Page header pages, the generated skills reference and the public API files.

Checkpoint with Jerrie before phase 2.

Phase 1 implemented and committed (2026-11-01). `ButtonVariant.OutlineDestructive` renders `sa-button-variant-outline-destructive`, which shares the outline variant's rules and sets the danger text colour, the soft danger fill on hover and active, and the danger focus ring. `sticky` renders `data-sticky`; the header sits at z-index 15, above the content's raised parts (10) and under the sidebar's backdrop (20), with 5 units above the title (3 on a phone) and 4 below. `--sa-space-page` is in `tokens.css` and `page-container.css` reads it. DocsSamples: an outline destructive button beside each destructive one in every Button and LinkButton demo that shows the variants (Sizes and Variants, Icon Left, Icon Right, Icon Only, Spinner, Invalid States), and a new PageHeader Sticky demo (registered in `DemoGenerator`). Website: a sentence on the variant on the Button and Link Button pages, and a Sticky header section and the `sticky` attribute on the Page header page. Verified: `StellarAdmin.UI.Tests` (810, with new `ButtonTagHelperTests` and `PageHeaderTagHelperTests`) and `StellarAdmin.UI.IntegrationTests` (19) pass; CDP screenshots of the Sticky demo scrolled at 1280 and 390px, and of the button variants in light and dark mode; `docs generate demos` (the website diff is the two pages, the new demo and the usual stylesheet hash change) and `skills generate`/`skills check` (the button variant lists and the page header's new attribute table). Not checked: the website pages in its dev server, and the theme fixtures.

### Phase 2: StellarAdmin (admin panel)

- `_IndexPage.cshtml`, `_ViewPage.cshtml` and `_FormPage.cshtml`: `sticky` on the page header and the actions in the order above. On the form page, the buttons sit in the header, outside the form, and name it with their `form` attribute.
- `ResourceEdit.cshtml`: Delete as `ButtonVariant.OutlineDestructive`, still through `form-leading-actions`, which now renders in the header. The customization reference and the docs describe the slots' new place.
- `ResourceController`: the view and edit page titles as above, with the record's title read from the loaded record.
- `client.css`: the widget column's offset under the header.
- `stellar-admin-panel.js`: measure the header's height for the widget column's offset; the Ctrl/Cmd+Enter and Ctrl/Cmd+Shift+Enter shortcuts find the submit buttons through the form's elements (`form.elements`) instead of inside the form.
- Tests: the page rendering tests for the moved buttons and slots and the titles (page setting, record title, key-only fallback); the shortcuts with the buttons outside the form. CDP checks at 1440, 768 and 390px, with and without widgets, in the theme fixtures and dark mode, and the inset sidebar variant, where the main area is a rounded card and the sticky header hasn't been checked yet. The docs screenshots of these pages are regenerated through write-docs-screenshot-specs.

Phase 2 implemented and committed (2026-11-01). The three page partials set `sticky`; the form page's buttons render in the header's actions in the order above and name the form (`id="resource-form"`) with `form`; the view and edit pages' Delete is `ButtonVariant.OutlineDestructive`. `ResourceOptions.PageTitle` compiles `UseTitle` or the named-property convention (`ResourceTitle.FindNamedProperty`, the convention without the key), and `ResourceController.FindRecordTitle` uses it on the view and edit pages when the page shows the resource's own type, falling back to the label for an empty title, another model or a title that reads a navigation the record didn't load (a `NullReferenceException`). A rejected edit keeps the title taken before binding. `client.css` drops the ruled header of the split and stacked layouts and the form footer's rules, and the form's widget column sticks at the header's height, which `stellar-admin-panel.js` measures into `--_page-header-height` on the page container (a `ResizeObserver`, and again on `htmx:load`); the save shortcuts find their buttons in `form.elements`. Website: the slots' new place on Page Tag Helpers, the Delete variant on Overriding pages, and the record title on Forms, View pages, Titles and references, Global labels and Resources. Skills: `customization.md`, `resources.md` and `view-pages.md`. Verified: `StellarAdmin.IntegrationTests` (750, with the new `ResourcePageHeaderTests`: the sticky header on all four pages, the view page's title and actions, the edit page's actions outside the form, the stored title after a rejected save, the page `Title` setting winning, and the label for a key-only title; the title, add another and delete tests updated), `StellarAdmin.Tests` (340), `StellarAdmin.EntityFrameworkCore.IntegrationTests` (151) and `StellarAdmin.ScenarioTesting.Tests` (23) pass. CDP on the AdminPlayground (:5192): the products index (25 rows), view and edit and the form-sections edit page scrolled at 1440, 768 and 390px, in dark mode, the Ops and Soft fixtures and the inset sidebar variant; the widget column's `top` is the header's height plus 16px; Ctrl+Enter, Ctrl+Shift+Enter and Enter press the right buttons on a create page with add another. The docs screenshots were regenerated (`docs generate screenshots`, 115 images changed, the check reports none open, unused or misnamed). Not checked: the website pages in its dev server, the Ledger fixture, and a create page's top widget beside the sticky header.

After phase 2 (2026-11-01, Jerrie): the edit page's Delete button is removed and the view page's Delete moves before Back. `ResourceEdit.cshtml` renders `<sa-form-page>` alone. The controller still sets `ResourceFormPageViewModel.Delete` (origin `edit`), so an overridden edit page can add the button and `_FormDeleteDialog`, as Overriding pages shows; a rejected delete from there still returns to the edit page with the errors. Website: Deleting records, Resources, View pages, Page Tag Helpers (the `pre-page-actions` place and the edit page note) and Overriding pages (the section is now Adding a Delete button to the edit page). Skills: `customization.md`, `resources.md`, `view-pages.md`. Verified: `StellarAdmin.IntegrationTests` (749: the edit page tests now check there's no Delete, the view page test checks Delete comes before Back, and the redundant edit page without deletion test is removed), `skills check`.

### Decisions (Jerrie, 2026-11-01)

- The sticky header is an option of `<sa-page-header>` in StellarAdmin.UI, with the page container's padding as a foundation token.
- The edit page's Delete is the same outline destructive button as the view page's, in the header, not in a menu. Replaced after phase 2: the edit page has no Delete button, and the view page's Delete comes first.
- The view page keeps its Back button until the app header breadcrumb lands.
- The new member is `ButtonVariant.OutlineDestructive`.
- The title order: the page's `Title` setting, the record's title, the label pattern; a key-only title falls back to the label pattern.
- Two phases, UI then admin panel, with a checkpoint between them.

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

Phase 1 implemented (2026-10-31). Beyond the bullets above: it re-fits when the host's parent resizes too, because a list only as wide as its tabs doesn't grow when there's room again, and it allows half a pixel for fractional tab widths (without it, lists that exactly fit showed More). DocsSamples has Overflow, Overflow None and Panels Overflow demos, registered in `DemoGenerator`. Verified: `StellarAdmin.UI.Tests` (807), `StellarAdmin.UI.IntegrationTests` (19), `StellarAdmin.Tests` (340) and `StellarAdmin.IntegrationTests` (740) pass; a CDP run against DocsSamples on :5206 at 1280px and 390px checked that the selected tab stays shown, the menu holds the hidden tabs, widening and narrowing (also in a content-sized flex parent) move tabs in and out, a cloned (swapped-in) list fits itself, picking a panel tab from the menu selects, shows and focuses it, the arrow keys skip hidden tabs, every other Tabs demo shows all its tabs, there are no ResizeObserver errors and the page never scrolls sideways. The website export (`docs generate demos`) and `skills generate`/`skills check` ran; the website diff is the Tabs page, the three new demos and the usual asset hash changes.

### Phase 2: index page layout (StellarAdmin)

`_IndexPage.cshtml` follows design A.

- The scopes and the search move out of `<sa-page-header-nav>` into the list card, above the grid: the scopes as a line `<sa-tab-list>` on the left (overflow on), the search on the right of the same row, 16rem wide. Without scopes, the search row is the filter bar's row.
- The card: the page wraps the list controls and `#index-page-data-grid` in one `sa-card`, and admin panel CSS drops the grid's own card frame inside it (border, radius, shadow), so the pager stays the card's footer and the data grid needs no new API. The search stays outside the swapped grid, so typing never loses focus; the scopes stay an out-of-band swap as today.
- Narrow cards: a container query on the card (under 44rem) puts the search under the scopes at full width, the tabs running edge to edge with their own divider.
- The filter bar's row: the layout leaves the row under the scopes for the filter bar from the `stellar-admin-filters` worktree, which renders there when that work merges (and shares the search's row when there are no scopes). Until then the row is not rendered.
- Widgets: no change to their placement; aside widgets line up with the card's top because the scopes are now inside it.
- Tests and checks: update the index page's rendering tests; CDP screenshots at 390, 768 and 1440px with no scopes, three and seven, with and without search and widgets; the docs screenshots of the index page that the move changes, through write-docs-screenshot-specs.

Phase 2 implemented (2026-10-31). The card and its controls are in the new `_IndexPageList.cshtml` partial, which also computes the search's out-of-band regions; the CSS is in `Client/css/client.css` (`sa-resource-index-list`, `sa-resource-index-controls`, `sa-resource-index-search`). The scopes' row is recognized with `:has(> .sa-tabs)`, not an attribute, because Razor rendered `data-scopes="False"` inside the card's content instead of leaving it out. Verified: `StellarAdmin.IntegrationTests` (741, with a new test that the scopes and search render in the card above the grid and the widget test's aside check updated) and `StellarAdmin.EntityFrameworkCore.IntegrationTests` (151) pass; CDP screenshots on the `index-pages-overview` scenario (two scopes and search) at 1440, 768 and 390px and on `default` (search only) at 1440 and 390px, with five scopes added in the page: the More menu takes the scopes that don't fit, typing in the search keeps focus and swaps the grid inside the card, and picking a scope keeps one card and the page never scrolls sideways. The docs screenshots were regenerated (all 69 specs) and checked. Found, not caused by this change: the search box empties after a search and then a scope click, recorded in [index page refreshing](../index-page-refreshing.md).

### Not in this plan

- Scope counts: the POC shows them, but they're not included (Jerrie, 2026-10-31); a scope has no count today and each one would cost a count query per page load.
- Hiding the Filter button's F hint on touch screens and Clear's line of its own with seven filters belong to the filters work.
- The breadcrumb in the app header, moved to [app header breadcrumb](../app-header-breadcrumb.md) (Jerrie, 2026-11-01).
- An unsaved changes indicator on the create and edit pages, moved to [unsaved changes](../unsaved-changes.md) (Jerrie, 2026-11-01).
- A subtitle under the page title, moved to [page subtitles](../page-subtitles.md) (Jerrie, 2026-11-01).
