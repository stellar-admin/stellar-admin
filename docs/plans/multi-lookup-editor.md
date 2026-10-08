# Multi-select lookup editor

Status: **active**. Phase 0 is done; phase 1 is next. Last updated: 2026-10-08.

A lookup editor that selects several items from a searchable sheet, built on the single-select lookup editor and the [sheet stack](archive/sheet-stack.md). The design comes from the [multi-select lookup prototype](../../sandbox/html/multiselect-lookup.html) (commit 6569834). Each phase stops for review.

## Scope

- The field binds a collection of keys: `TValue[]`, `List<TValue>` or another collection MVC binds. These save today through plain models and custom create and edit handlers, such as the playground's `string[] RoleIds`.
- EF Core entities with a many-to-many navigation are not in this plan. `ValidateEntityFormFields` rejects navigation fields, MVC can't bind entities from posted keys, and `UpdateAsync` copies values rather than adding and removing items. That needs its own approval, because [generic-resources-follow-ups](generic-resources-follow-ups.md) keeps single-record references as the relationship scope.

## Design decisions

From the prototype sessions:

- Three field displays, as a `Layout` on the editor: List (card rows), Chips (an input that wraps chips) and Summary (an input with one line, such as "Lisbon, Porto and 3 more"). Chips is the default. A read-only Summary renders as List. On phones, chips wrap and Summary stays one line.
- The sheet uses the single lookup's results, with a trailing check on selected rows.
- An All | Selected segmented control sits under the search. Selected shows the count, and its view has Clear all, without undo. A row unchecked in the Selected view leaves the list.
- Each toggle updates the field straight away; there is no draft. One `change` event fires when the sheet closes, and only if the value changed.
- Keys: Enter toggles the highlighted row. Esc, Done, Ctrl+Enter (⌘↵ on a Mac) or the backdrop closes the sheet. Alt+N (⌥N) opens New.
- The footer has two rows: key hints above **New | Done**. On phones the hints row is hidden.
- Create is B1 + B3: New in the no-results state and in the footer. The form opens as the next sheet level, and the created item comes back checked, through `lookup-created`, with the search kept.
- Limit: the maximum comes from `[MaxLength]` or `[Length]`, with an option to override it. At the limit, unchecked rows and New are disabled, the sheet shows "Choose up to N" and the field shows "Up to N guides".
- `sel-command` keeps the highlight in place when a row is removed from the list.

## Phase 0: rename the single lookup to `LookupSheetEditor`

- `LookupEditor` becomes `LookupSheetEditor`, so it pairs with `MultiLookupSheetEditor` and leaves room for popover versions. Its own types follow: `LookupEditorHandler`, `LookupEditorLayout`, `LookupEditorData`, `LookupEditorClassNames`, the EF Core `LookupEditorExtensions`, the `Editors/Lookup` template and the `dashboard-lookup-editor` element.
- The types both editors share keep their names: `LookupItems`, `ILookupSource`, `LookupQuery`, `LookupResults`, `LookupPage`, `LookupSheetOptions` and `LookupFieldOptions`.
- The playground, the tests and the consumer setup reference (`skills/stellar-admin-dashboard/references/setup.md`) follow. No behaviour changes; the Dashboard packages aren't published, so no consumer breaks.

**Done (2026-10-08):**

- Renamed `LookupEditor`, `LookupEditorClassNames`, `LookupEditorData`, `LookupEditorHandler`, `LookupEditorLayout` and the EF Core `LookupEditorExtensions` to their `LookupSheetEditor` names, with their files. The template is `Editors/LookupSheet`, the element `dashboard-lookup-sheet-editor` (class `DashboardLookupSheetEditor`), and exception messages name `LookupSheetEditor`. `LookupEditorTests` became `LookupSheetEditorTests`. The playground, the other tests and the setup reference use the new names; prose that says "lookup editor" is unchanged, as are the sandbox prototypes and archived plans.
- Fixed `CreateFromLookup_SelectsCreatedEntity` in the EF Core integration tests, which had failed with a 404 since the sheet stack's phase 1 (8a4e111): it read the create URL from the opener's `hx-get`, which became `data-sheet-open` with a required `level`. It now reads `data-sheet-open` and expects `level=1`. That suite wasn't run in the sheet stack work.
- Checks: the solution builds. `StellarAdmin.Dashboard.Tests` 79 of 79, `StellarAdmin.Dashboard.IntegrationTests` 362 of 362 and `StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests` 73 of 73 pass. No browser check; the built script and the template both use the new element name.

## Phase 1: one find method for many values

- `ILookupSource<TEntity, TValue>.FindAsync` takes `IReadOnlyCollection<TValue>` and returns the entities it finds. There is no separate single-value method.
- `LookupItems.FindAsync` reads one value or a collection from `context.Value` and returns the items in value order. A value the source no longer has keeps its placeholder item.
- EF Core items query `WHERE value IN (…)` once. The edit page's loaded reference is used for a single value, as today.
- The single `LookupSheetEditor`, the playground's airport and layout destination sources and the test fixtures move to the new method. No behaviour changes.

## Phase 2: the editor on the server

- A `MultiLookupSheetEditor` with its handler, binding a collection field, and the List, Chips and Summary templates, including read-only.
- The limit from `[MaxLength]` or `[Length]`, with an override, and validation on post.
- An empty-selection marker, so removing every item clears the collection.
- A rejected post shows the posted keys again with their labels, from one find call.
- A Multi lookup gallery in the playground, with destinations (airports) and guides on an in-memory model, in Voyager Travel content.

## Phase 3: the sheet

- Multi-select results with trailing checks, All | Selected with the count and Clear all, Enter to toggle, Done and Ctrl+Enter to close, the two-row footer, and the limit.
- The field updates on each toggle and fires one `change` on close.
- The `sel-command` highlight fix in TagHelpers.
- Phone layout.

## Phase 4: create

- New in the no-results state and the footer, with Alt+N, disabled at the limit.
- The editor listens for `lookup-created` and adds the new item, checked, keeping the search.

## Not in this plan

- EF Core many-to-many navigations (see [Scope](#scope)).
- A popover version of the multi-select lookup.
- Join entities with extra columns, such as nights per destination. Those need a nested editor, not a lookup.
