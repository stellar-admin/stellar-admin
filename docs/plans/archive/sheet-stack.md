# Stacked sheets for the lookup editor

Status: **completed**. All three phases are implemented, reviewed and committed (8a4e111, f8623b5 and the phase 3 commit). The items under [Not in this plan](#not-in-this-plan) are unscheduled. Last updated: 2026-10-08.

The Dashboard currently allows two sheets. `_Layout` renders two fixed `_RemoteSheet`s, `dashboard-sheet` and `dashboard-nested-sheet`. A create form's lookups open in the second sheet and hide New, so creating stops one level down. This plan replaces the fixed pair with a stack of any depth, styled with the recede concept from the [stacked sheets prototype](../../../sandbox/html/sheet-stack.html) (commit 6f5895b), and applies it to the existing single-select `LookupEditor` before the multi-select work starts. Each phase stops for review.

## Phase 1: one sheet per level

- `_Layout` renders a single `<template>` of the remote sheet instead of the two sheets. The script clones it for each level when an opener runs, `showModal()`s it, and removes it on close.
- Openers stop naming a sheet. `commandfor="dashboard-sheet"` and `hx-target="#dashboard-sheet-content"` become a `data-sheet-open` opener, and the script targets the new level's content. Fixed IDs inside sheet content, such as `#dashboard-sheet-results`, become relative targets.
- Each level gets its own binding prefix (`Sheet1`, `Sheet2`, …) instead of `Sheet`, so field IDs and names don't collide between nested create forms. `dashboard-lookup-created` then finds its editor within the level below.
- `ViewDataKeys.InCreateSheet` becomes the depth. The lookup template uses it for the prefix and the form name, and no longer hides New inside a create sheet.
- Verify: the existing lookup and create flows still work at depth 1, and a nested create (tour → guide → city) selects each new record in the field below.

**Done (2026-10-08, 8a4e111):**

- `_Layout` renders `<template id="dashboard-sheet-template">` holding one `_RemoteSheet`. `DashboardRemoteSheet.open(opener)` imports it, renames the `dashboard-sheet` ids and `commandfor` to `dashboard-sheet-{level}`, appends it to the body, `showModal()`s it and loads the opener's URL; the close event aborts its requests and removes the level. A document click listener opens a level for any `[data-sheet-open]` button.
- Lookup openers carry `data-sheet-open="{url}"` instead of `commandfor`, `command`, `hx-get` and `hx-target`. The search results' id is `{for}-results`; the create form posts to `closest [data-sheet='content']`, and Cancel is `data-sheet="close"`.
- `CreateSheet` GET and POST take `level` (1 or more, otherwise 404). `CreateSheetViewModel` has the level, and its prefix is `Sheet` at level 1 and `Sheet{n}` above. `ViewDataKeys.InCreateSheet` became `SheetLevel` (an int); a lookup in a create sheet now shows New, which opens `level + 1`.
- DashboardPlayground: the gallery's airport create form has an optional Hub airport lookup with New, so airports nest without limit.
- Checks: Dashboard integration tests 362 of 362 pass, with new tests for the level-2 prefix (GET and POST) and a missing or zero level. In headless Chromium on the lookup gallery: three create levels open with real clicks, Cancel closes only the top one, each created airport is selected in the field below (QQC in level 2, QQB in level 1, QQA on the page), and Choose at level 2 picks a search result. The page scroll lock drops when a level closes while lower ones are open, as expected until phase 2.

## Phase 2: recede and stack behaviour

- Recede styling in the library CSS: each covered level moves 40px left and scales down 4.5% per level, using the individual `translate` and `scale` properties so the enter animation is unaffected. A single base scrim; covered sheets dim.
- Esc closes only the top level: the stack handles `keydown` itself, because Chrome groups dialogs opened without user activation and closes them together.
- Fix the `sel-dialog` scroll lock in TagHelpers: closing a level must not unlock the page while lower levels are open.
- Clicking a covered sheet's visible edge closes the levels above it.
- Phones: the top sheet is full width and covered levels are hidden.
- Verify: five deep in headless Chromium, in light and dark mode, with Esc, click-back and the scroll lock, at desktop and phone widths, in at least two themes.

**Done (2026-10-08, f8623b5):**

- `_RemoteSheet` gives the sheet the class `sa-sheet-stack-level`, and the Dashboard stylesheet styles it: covered levels (`data-under`) move `--sa-sheet-shift × 40px` left and scale down 4.5% per level from the right edge, with rounded corners and a dimming `::after`; levels after the first have no backdrop. The exit animation keeps a closing sheet in the top layer (`display` and `overlay` transition with `allow-discrete`). Below 40rem, levels above the first fill the width and covered levels hide.
- The script restacks on every open and close, numbers level ids with a counter, and removes a closed level once its animations finish. Esc (keydown, not the dialog's cancel) closes only the top level unless a popover in it is open. A click on the top level's backdrop over a covered level closes the levels above it, and hovering there lightens the dim and shows a pointer; both are off below 40rem.
- `sel-dialog` (TagHelpers) keeps the page locked while any `dialog:modal` is open, and measures the scrollbar only when the lock starts.
- Checks: Dashboard integration tests 362 of 362 pass. In headless Chromium on the lookup gallery, five levels deep with real clicks and keys: Esc closes 5 → 4 → 3, click-back on level 1's edge closes levels 2 and 3, a click inside the top sheet closes nothing, and the page stays locked until the last level closes; the same in shadcn.vega dark. At 390px the top level fills the width, covered levels are hidden and click-back is off. The phase 1 create flow still selects each created airport in the field below.

## Phase 3: hand-back contract

- A level returns its result (a created item's key) to the owner of the level below through one event. The single-select editor replaces its value; a future multi-select editor will add the item to its selection. No multi-select code goes in this phase.

**Done (2026-10-08):**

- `dashboard-lookup-created` closes its level and dispatches a bubbling `lookup-created` event with `detail.key` from the hidden input it names. `dashboard-lookup-editor` listens for it and selects the item; `selectCreated` became private. A multi-select editor will listen for the same event and add the item.
- Checks: in headless Chromium the three-level create flow still selects each created airport in the field below (QQC, QQB, then QQA on the page), and Choose at level 2 still selects a search result. No server code changed.

## Not in this plan

- The breadcrumb trail, a depth limit, New in the lookup search sheet footer, and turning New off per field. Each can follow as separate work.
- The multi-select lookup editor.
