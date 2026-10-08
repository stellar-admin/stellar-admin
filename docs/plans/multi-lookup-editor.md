# Multi-select lookup editor

Status: **active**. Phases 0 to 4 are done and awaiting review. Last updated: 2026-10-08.

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
- No limit in the editor (changed 2026-10-08): any number of items can be selected, and validation on the model, such as `[MaxLength]` or `[Length]`, rejects too many on post. A hard limit in the editor can follow if needed.
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

**Done (2026-10-08):**

- `ILookupSource<TEntity, TValue>.FindAsync` takes `IReadOnlyCollection<TValue>` and returns `IReadOnlyCollection<TEntity>`, leaving out values that don't exist.
- `LookupItems.FindAsync` returns `IReadOnlyList<ChoiceItem>`: empty for no value, one item for a single value, and for a collection one item per distinct non-null value, in value order. A missing value gets the placeholder item, as before. The generic items call the source once and match entities to values with the value selector.
- `EfCoreLookupItems` filters with `Enumerable.Contains` on a captured array, so EF Core sends the values as one parameter. The loaded-reference shortcut applies when there is one value.
- `LookupSheetEditorHandler` and `ResourceController.LookupSelection` take the first item. The playground airport and layout destination sources, the `CategoryLookupSource` fixture, the `UntypedItems` test double and the setup reference's `ILookupSource` sample use the new signatures.
- The type-mismatch exception still names `LookupSheetEditor`; phase 2 makes it name the editor in use.
- Checks: the solution builds with no new warnings. `StellarAdmin.Dashboard.Tests` 79 of 79, `StellarAdmin.Dashboard.IntegrationTests` 362 of 362 and `StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests` 73 of 73 pass; the EF Core query runs in `RejectedEdit_DisplaysChangedSelection` and `Create_DisplaysSelectionWithoutNavigation`. No new tests and no browser check.

## Phase 2: the editor on the server

- A `MultiLookupSheetEditor` with its handler, binding a collection field, and the List, Chips and Summary templates, including read-only.
- An empty-selection marker, so removing every item clears the collection.
- A rejected post shows the posted keys again with their labels, from one find call.
- A Multi lookup gallery in the playground, with destinations (airports) and guides on an in-memory model, in Voyager Travel content.

**Done (2026-10-08):**

- `MultiLookupSheetEditor` with `UseItems` (registered `ILookupSource` or custom `LookupItems`), `Editor(...)` with `MultiLookupFieldOptions` (`Layout`, `EmptyText`, `ShowMedia`), `Sheet(...)` with the shared `LookupSheetOptions`, and `ClassNames` (`MultiLookupSheetEditorClassNames` with `Media`). `MultiLookupSheetEditorLayout` is `List`, `Chips` and `Summary`; `Chips` is the default. EF Core items come from a `MultiLookupSheetEditorExtensions.UseItems<TContext, TEntity, TValue>` beside the single editor's.
- `MultiLookupSheetEditorHandler` resolves the items with one `LookupItems.FindAsync` call, so a rejected post shows the posted keys with their titles. The template `Editors/MultiLookupSheet` renders a hidden input per value and the checkbox group's `__sa_checkbox_group.` marker, which binds an empty collection when every item is removed. List rows are `sa-item`s with Remove and a dashed Add; Chips are badges with Remove inside an input group with Add; Summary names two items, ends with "and N more", counts them and has Clear. Read-only shows chips or rows without inputs, a read-only Summary shows the list, and an empty one shows None. An invalid field gets the destructive border.
- `dashboard-multi-lookup-sheet-editor` handles Remove and Clear: it removes the item and its input, swaps to the empty buttons when nothing is left, moves focus to the next Remove or the open button, and fires `change` on itself. The Add, Choose and Summary buttons don't open a sheet yet (phase 3).
- New lookup labels: `AddLabel` ("Add {field}"), `RemoveLabel` ("Remove", followed by the title) and `MoreText` ("and {Count} more"), with `Count` added to `LookupLabelContext`. The type-mismatch exception now says "The lookup on {field}" instead of naming `LookupSheetEditor`.
- The playground has a Multi lookup gallery (`MultiLookupGallery`, with `GalleryGuideLookupSource` and Voyager Travel guides) covering the three layouts, `[MinLength]`, `[MaxLength]`, unknown values, `EmptyText`, `ShowMedia`, class names and read-only.
- Not done: the consumer setup reference doesn't describe the editor yet. The overlapping avatars in a Summary clip each other's initials, as they did in the prototype.
- Checks: the solution builds. `StellarAdmin.Dashboard.Tests` 79 of 79, `StellarAdmin.Dashboard.IntegrationTests` 376 of 376 (14 new in `MultiLookupSheetEditorTests`) and `StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests` 73 of 73 pass. Headless Chromium against the playground gallery, at desktop width in light and dark and at phone width: Remove, Clear, focus, `change` and the posted form values (an emptied field posts only the marker) behave as described. The chips' Add label wrapped on phones and now truncates.

## Phase 3: the sheet

- Multi-select results with trailing checks, All | Selected with the count and Clear all, Enter to toggle, Done and Ctrl+Enter to close, and the two-row footer.
- The field updates on each toggle and fires one `change` on close.
- The `sel-command` highlight fix in TagHelpers.
- Phone layout.

**Done (2026-10-08):**

- The field's Add, Choose and Summary buttons open the lookup sheet. `ResourceController.FindLookupField` accepts either editor through an internal `ILookupSheetEditor` (items, sheet options, media class), so `Lookup` and `LookupSheet` serve both; `LookupSelection` stays single-select.
- `ResourceLookupQuery.Selected` is now `string[]`, and every search sends the editor's current values, so results are checked by value. Load more no longer carries `selected` in its URL, because the picker sends the values with each request. `SelectedOnly` searches the selected items: one `FindAsync`, filtered by title or description in the current culture, in value order, without paging, with a `NoSelectedTitle` message when nothing matches.
- A new `MultiLookupSelection` action renders the field's items for a set of values with the `_MultiLookupItems` partial, which the editor template also uses. Each toggle updates the hidden inputs straight away and fetches the items again, the latest request winning; a removed item leaves at once.
- The multi-select sheet: All | Selected (a segmented control, with the count on Selected), Clear all in the Selected view, trailing checks, and a footer with key hints (hidden on phones) above Done. Enter toggles and selects the search text; Ctrl+Enter (⌘↵ on a Mac) and Done close. Unchecking in the Selected view removes the row, and an empty view searches again to show the message. The editor fires one `change` when the sheet closes, only if the values changed, and focuses its visible open button.
- New lookup labels: `AllLabel`, `SelectedLabel`, `ViewLabel`, `ClearAllLabel`, `DoneLabel`, `DoneHint`, `ToggleHint` and `NoSelectedTitle`.
- `sel-command` keeps the highlight in place when the active item is removed with nothing added in its place: the next item takes it, or else the previous one.
- The playground gallery's description mentions the sheet.
- Not done: the consumer setup reference still doesn't describe the editor. At 390px the sheet covers 75% of the width, the same as the single lookup's sheet.
- Checks: the solution builds with no new warnings. `StellarAdmin.Dashboard.Tests` 79 of 79, `StellarAdmin.Dashboard.IntegrationTests` 389 of 389 (13 new tests in `MultiLookupSheetEditorTests`, and the Load more test no longer expects `selected` in the URL) and `StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests` 73 of 73 pass. Headless Chromium against the playground gallery, at desktop width in light and dark and at phone width, with no script errors: Enter and click toggles, field updates, the Selected view, unchecking with the highlight kept, Clear all, Ctrl+Enter, Done, one `change` per changed session and none for an unchanged one, Load more checks, the Summary layout, and the posted values.

## Phase 4: create

- New in the no-results state and the footer, with Alt+N.
- The editor listens for `lookup-created` and adds the new item, checked, keeping the search.

**Done (2026-10-08):**

- `MultiLookupSheetEditor.EnableCreate()` and `EnableCreate<TResource>()`. The single editor's lookup of the creating resource, its create form and the user's authorization moved to an internal `LookupCreateResource`, which both editors use; its exception messages name the editor.
- The multi-select sheet shows New beside Done in the footer, with an Alt+N hint (⌥N on a Mac), and in the no-results state of the All view; the Selected view has none. New opens the resource's create form as the next sheet level. `ResourceLookupQuery.Level` carries the level of the form that contains the field, so the create form binds with the prefix of the level above it; the field's sheet URL and the sheet's results URL pass it on, and the results URL also carries `for`.
- The picker listens for `lookup-created` on its editor, renders the created item's row with the Selected view's search, selects its value, removes the no-results message, puts the row checked at the top of the results and keeps the search, selected for the next one. If the row can't be rendered, the key is selected without one.
- New lookup label: `NewHint` ("new"). New uses the existing `CreateLabel`.
- The playground's first destinations field in the Multi lookup gallery enables create, with the airport resource's create form.
- Not done: the consumer setup reference doesn't describe the editor. After a create, the highlight stays on the row that had it rather than moving to the new row.
- Checks: the solution builds with no new warnings. `StellarAdmin.Dashboard.Tests` 79 of 79, `StellarAdmin.Dashboard.IntegrationTests` 398 of 398 (9 new in `MultiLookupCreateTests`) and `StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests` 73 of 73 pass. Headless Chromium against the playground gallery at desktop and phone width, with no script errors: a search without results offers New, Alt+N opens the create form as level 2, and the created airport comes back checked at the top of the results with the search kept and selected, in the field and in the count; Done then fires one `change`.

## Not in this plan

- EF Core many-to-many navigations (see [Scope](#scope)).
- A popover version of the multi-select lookup.
- Join entities with extra columns, such as nights per destination. Those need a nested editor, not a lookup.
