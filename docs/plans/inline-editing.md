# Inline editing on the resource index

Status: done (2026-10-16). Phases 1–5 done. Prototypes: `sandbox/html/inline-editing.html` (six concepts compared, with complexity ratings) and `sandbox/html/inline-row-edit.html` (row edit mode, inline add and the edit sheet). The row edit prototype follows the either/or decisions below; the sheet stands in for the form.

## Goal

Let a small resource (two to four fields, all shown as columns) be edited and added directly in the index grid, without leaving the page. Resources with many fields keep using the edit page, or the edit sheet once it exists.

## Decisions (Jerrie, 2026-10-15)

1. Each operation is edited either inline or through a form, never both. The form is the full editing experience, shown as a page today and optionally as a sheet once the sheet exists. There is no hand-off from an inline row to the form.
2. Edit and create choose independently: for example, inline edit with create on a page.
3. Inline edit uses row edit mode. The row's Edit action turns the row into fields, with Save and Cancel in place of the actions.
4. The operation's field list defines what the row edits. Every field must match a column, matched by field name, so a separate edit model (`AllowEdit<TModel, THandler>`) works too. A column that shows a different path than the field it edits names that field (`EditsField`). A field that matches no column fails at startup. Columns are not marked editable otherwise.
5. An inline operation has no fallback page. Its edit or create page is not registered, and the View page shows no Edit button when edit is inline. Startup fails when a `[Required]` property of the create model has no column. Server errors the row cannot show appear in a message under the row, without a link.
6. A lookup column opens the lookup sheet from the row.
7. Row edit stays on at every width. On narrow screens the grid scrolls sideways.
8. Dimming the other rows while one is edited or added is an option, on by default.
9. New rows are added at the top by default, or at the bottom. Bottom means the bottom of the current page. Changed 2026-10-16 (Jerrie): this places the empty row; after a save, the grid reloads its current page by default (`AfterSave = InlineRowAfterSave.ReloadGrid`), and `KeepInPlace` keeps the saved row where it was until the next reload, for edit and create alike.
10. The Add button for inline create is shown in the page header, in the grid, or both (the default). The grid's button sits in the first row for Top and the last row for Bottom. Either button scrolls to the new row and focuses its first field.
11. ~~A row's details collapse while that row is being edited.~~ Superseded 2026-10-15: the details stay in place, open or closed, while the row is edited ([data grid inline editing](data-grid-inline-editing.md), decision 6).
12. An unsaved editing row is discarded silently when the grid reloads (paging, sorting, search or scopes).
13. A cell uses its own editor template, separate from the form's. As displays do with `DisplayTemplates/` and `GridDisplayTemplates/`, the editor handler keeps its single `TemplateName`, resolved in `EditorTemplates/` for a form and in `GridEditorTemplates/` for a cell. The editor API does not change; a custom editor works in the grid by adding a view with the same name under `GridEditorTemplates/`.
14. In v1 these built-in editors get grid templates: text input (including number and the other text types), select, checkbox, toggle, date, time and date-time inputs, segmented control and lookup sheet. Textarea, radio group, checkbox group, toggle group, slider, one-time code and multi-lookup sheet do not.
15. A field whose editor has no grid template does not fail at startup. In development its cell shows an error naming the missing template; in production the cell is empty. Saving binds only the fields whose cells rendered an editor, so a missing editor never overwrites a value; on create, a required field without one fails validation in the row message.

## API

```csharp
admin.AddEfCoreResource<AppDbContext, Destination>(resource =>
{
    resource.AllowEdit(edit =>
    {
        edit.Fields(fields =>
        {
            fields.Add(d => d.Name);
            fields.Add(d => d.CountryId).UseEditor<LookupSheetEditor>(...);
            fields.Add(d => d.BestSeason);
            fields.Add(d => d.Active);
        });
        edit.UseInlineRow(inline => inline.DimOtherRows = true);           // default
    });

    resource.AllowCreate(create =>
    {
        create.Fields(...);
        create.UseFactory(() => new Destination { Active = false });
        create.UseInlineRow(inline =>
        {
            inline.Position = InlineCreatePosition.Top;                 // Top (default) or Bottom
            inline.AddButton = InlineAddButton.Both;                     // Header, Grid or Both (default)
            inline.DimOtherRows = true;                                  // default
        });
    });

    resource.Index(index => index.Columns(columns =>
    {
        columns.Add(d => d.Name);
        columns.Add(d => d.Country!.Name, column => column.Title = "Country").EditsField(d => d.CountryId);
        columns.Add(d => d.BestSeason);
        columns.Add(d => d.Active);
    }));
});
```

- Without `UseInlineRow()`, an operation keeps its page as today. Each choice names what the user sees: the sheet later becomes `UseSheet()` on the same builders, and a dialog could become `UseDialog()`; the page stays the default. The name avoids "Form": the row is not the full form, while the page and the sheet are.
- The cell renders the field's editor through its grid template (decision 13), with the field's label as its accessible name and its validation shown compactly under the cell.
- Startup fails when an inline operation's field matches no column, or when a `[Required]` create model property has no column.
- `InlineCreatePosition` (`Top`, `Bottom`) and `InlineAddButton` (`Header`, `Grid`, `Both`) are new enums of their own.
- Save and Cancel labels join the existing index labels.

## Behaviour

- Edit: the row's Edit action loads the editing row from the edit loader (the edit model may differ from the grid's item) and posts the fields through the existing edit handler. On success the row comes back as it displays, reloaded through the index query. On failure the editing row comes back with errors under its cells, and errors the row cannot place appear in the message under the row.
- Add: an Add button inserts an editing row filled from `UseFactory`, scrolls to it and focuses its first field. Saving goes through the existing create handler. After a save the grid reloads its current page, or with `KeepInPlace` the row stays where it was until the grid next reloads; a toast confirms a new row. Errors behave as for Edit.

## Implementation phases

Each phase ends at a checkpoint for review before the next starts.

1. **Configuration and startup checks** (StellarAdmin, no UI). `UseInlineRow(...)` on `ResourceEditBuilder` and `ResourceCreateBuilder` with their settings builders, the `InlineCreatePosition` and `InlineAddButton` enums, and `EditsField(...)` on `ResourceColumnBuilder`. Startup checks: every field of an inline operation matches a column (by name or `EditsField`), and every `[Required]` create model property has a column. For an inline operation, `Edit`/`Create` return 404, the grid's Edit links and the View page's Edit button go, and the header Add follows `AddButton`. Save and Cancel index labels. Unit tests for the builders and the checks.
   Done 2026-10-15, with these choices made during the phase:
   - The header Create link is hidden for inline create, and the Add buttons (header and grid, following `AddButton`) arrive in phase 4 with their behaviour, so no button leads to a missing page.
   - A lookup field elsewhere that creates items of an inline-create resource still opens that resource's create sheet; the sheet belongs to the lookup, not the create page.
   - Read-only fields are left out of the column match.
   - `EditsField` also takes a field name, for a property of a separate edit or create model that the resource does not have.
   - The checks are an options validator (`InlineRowOptionsValidator`) that names each field it rejects. Resource options now use `ValidateOnStart()`, so these and the existing resource checks fail when the application starts rather than on the first request; three EF Core configuration tests now expect the failure from host start.
   - Save and Cancel labels: `CancelLabel` and `SaveLabel` on the index builder and on `ConfigureResourceLabels` index labels.

   Verified: admin panel unit tests (178), HTTP integration tests (464, including `ResourceInlineRowTests`) and EF Core integration tests (86) pass, and AdminPlayground starts with startup validation.
2. **Grid editor templates.** Resolve an editor's `TemplateName` in `GridEditorTemplates/` for cells, with the development error and the empty production cell. Grid templates for the v1 editors (decision 14). The cell CSS from the prototype moves into the library: `.sa-cell-editor`, `.sa-cell-switch`, the lookup trigger, `.sa-cell-error`, the editing row, the row message, the add row and the saved flash.
   Done 2026-10-15, with these choices made during the phase:
   - `Html.GridEditorAsync(field)` (public, hidden from IntelliSense like the other view support types) renders a field's cell editor, as `Html.Editor` does for a form, so the phase 3 row view and application overrides can use it. It prepares the editor's data itself and appends the field's error (`.sa-cell-error`) under the editor; `FieldEditorView.CellErrorId` gives grid templates the id for `aria-describedby`.
   - A field without an editor resolves its grid template by the property's `[UIHint]`, `[DataType]` and type, as MVC does for forms, so `GridEditorTemplates/` also has the type templates (`String`, `Int32`, `Boolean`, `Enum`, `DateOnly` and the rest). A `[DataType(MultilineText)]` property falls back to the `String` template, a text input; only an explicit `TextareaEditor` shows the missing template.
   - Cells ignore the editor's form class names.
   - A lookup in a cell is a single trigger button (the selection, or the choose text) that opens the form's lookup sheet; it has no Clear or New button. Phase 3 sets which form's lookup it opens (`ViewDataKeys.LookupForm`).
   - Text inputs keep the default control height, as in the prototype; selects, switches and the lookup trigger are small.
   - CSS names: `.sa-cell-check` holds a checkbox or a switch (the prototype's `.sa-cell-switch`); the grid asks for dimming with `data-inline-dim` (per operation since phase 4).

   Verified: admin panel unit tests (178), HTTP integration tests (471, including `GridEditorTests`, which render cells through a `grid-cells` view override), EF Core integration tests (86) and UI unit tests (513) pass. No browser check yet: nothing renders an inline row until phase 3.
3. **Server endpoints.** `EditRow` and `CreateRow` (GET renders the editing row; POST binds only the rendered fields through the existing loader and handlers, and returns the display row, or the editing row with cell errors and a row message). The row posts with htmx (`hx-include="closest tr"`), since a `<form>` cannot sit inside a `<tr>`; the antiforgery token goes in a request header. Builds on the grid's row keys, edit templates and `rows-only` ([data grid inline editing](data-grid-inline-editing.md), stages 1–3). The grid item stays the resource, so columns without an editor keep their display; the endpoint passes the edit model to `GridEditorAsync`, which takes it as an argument, and the fields get the `rows[key]` prefix.
   Done 2026-10-16 (committed as 62ff920), with these choices made during the phase:
   - Routes: `GET`/`POST {resource}/editrow/{id}` and `{resource}/createrow`, rendering `_IndexDataGrid` with `rows-only`. A saved row comes back through the index query: `ResourceListRequest.Keys` (new, Jerrie) asks the data source for those keys only, without scope, search or paging; the EF Core source filters on the primary key, and the controller also picks the row by key, so a custom source that ignores `Keys` still works. A saved row that cannot be loaded (such as one created without a key) answers `HX-Refresh`. Unknown keys, and row requests for an operation that is not inline, return 404.
   - The grid gets `id="index-page-grid"` and, with inline rows, `key-field` (the `UseKey` property), so row requests' details ids match the page's. Startup now fails when an inline operation's resource has no key.
   - A column has an edit template in a row response when it edits a field of the row being rendered (the page never renders editing rows, decision 12); the actions column always has one with inline rows, which also switches on `sel-table`'s editing behaviour on the page. `Html.GridEditorAsync(field, model)` takes the row's edit model; the grid item stays the resource.
   - New rows: columns without an edit template are empty in the new item's row, a `sa-data-grid` change (Jerrie), so a separate create model never meets the resource's display columns. The DocsSamples New Row sample shows those cells empty.
   - Actions: an inline Edit icon button (`hx-get`, swapping the row), and in the editing row Save (check, `data-row-save`) and Cancel (x, `data-row-cancel`) icon buttons with tooltips, as in the prototype. The row posts with `hx-include="closest tr"` and carries the antiforgery token as a hidden input in the actions cell (instead of a header, so no header name configuration is involved). An edited row's Save swaps in only `tr[data-key]` and `tr.sa-row-message`, and refreshes the details' content with `hx-select-oob` unless a panel loads on demand (a reloaded on-demand panel would not load again while open); a new row swaps in its display and details rows.
   - Row message: errors no cell showed (the handler's unplaced errors, and fields whose cells rendered no editor; `GridEditorRenderer` records the cells it rendered) go in a `tr.sa-row-message` after the editing row. The admin script removes a row's message on Cancel and when a save's response swaps the row.
   - `sa-data-grid` treats an empty `key-field` as unset (Razor writes a null string expression as empty).
   Verified: admin panel unit tests (180, one new: inline rows without a key), HTTP integration tests (483, twelve new in `ResourceInlineRowTests`: edit row, save, cell error, row message, edit fields only, unknown key, no inline rows, new row, create, create error, refresh), EF Core integration tests (87, one new: the saved row's reference column comes back through the index query) and UI unit tests (533, one new and one extended) pass; consumer references show no drift. Headless Chromium on AdminPlayground with Categories temporarily inline (reverted, database restored): edit, cancel without a request, edit again and save with the details row kept, a cell error, the row message going on cancel and on save, a new row (inserted by script, since the Add buttons are phase 4) cancelled and saved with errors first (10 passing). Not done: the consumer docs for `ResourceListRequest.Keys` and inline rows (phase 5).
4. **Grid script and behaviour.** One editing row at a time, Enter and Escape, dimming (column widths, cancel without a request and the kept selection and details come from `sel-table`, stage 4 of [data grid inline editing](data-grid-inline-editing.md)), the Add buttons scrolling to and focusing the new row, discarding an unsaved row on reload, the saved flash and toast, and the lookup sheet opening from a cell.
   Done 2026-10-16, committed as ca6c2e0, with these choices made during the phase:
   - The grid's Add button sits in `sa-data-grid-add-row` (new in StellarAdmin.UI, Jerrie), a full-width row first or last in the body (`position`: `DataGridAddRowPosition.Top` or `Bottom`), also rendered when the grid is empty and left out with `rows-only`; it is hidden while a new row is in edit mode, and dims with the other rows while a row is edited (changed after phase 5, Jerrie: hiding it while editing moved every row up). Its styles moved from the admin stylesheet to `table.css`. The grid's empty-state row is marked `data-empty` and steps aside while the body has a keyed row, such as a new row added to an empty grid.
   - Add buttons request `CreateRow` with htmx: the grid's inserts the new row after its add row at the top or before it at the bottom; the header's (`CreateLabel`, shown for `Header` and `Both`) targets the grid's add row when there is one, or else the start or end of the body.
   - The admin script (`stellar-admin-panel.js`): Edit or Add first cancels a row being edited; Enter in a text input saves and Escape cancels, focusing the row's Edit button or the Add button. After a swap, an added row scrolls into view with its first field focused, an edited row's first field is focused and selected, a rejected save focuses the first invalid field, and a saved row flashes (`data-saved`) with its Edit button focused. htmx sends the events of a swapped-out row's button to the document, so the script sets up this work in `htmx:before:swap`, keyed by the request.
   - Dimming: the grid carries `data-inline-dim` with `edit` and/or `create` for the operations whose `DimOtherRows` is on; an edited row's message and details are not dimmed. Razor renders the attribute empty when neither dims. Changed after phase 5 (Jerrie): dimming also locks. The script marks the other rows, the Add buttons, the scopes, the search, the sort links and the pager `data-inline-locked` and `inert` (the attribute dims them, replacing the CSS `:has` rule) until the row is saved or cancelled; without dimming, they stay usable and Edit or Add cancels the open row first.
   - Toast: a created row queues a success toast through `IToastNotifier`, with the new index label `CreatedMessage` (default `"{SingularLabel} added"`). The admin layout now has `<sa-toaster />`, and the script shows the toasts of every htmx response's `SA-Toasts` header.
   - Lookup from a cell: the sheet's load sets its own swap and drops the inherited `hx-select`, target selection and history, since a cell's opener sits inside `#index-page-data-grid` and inherited its attributes, which emptied the sheet.
   - The actions column of a grid with inline rows (`.sa-inline-row-actions`) keeps room for Save and Cancel, since its width stays while a row is edited.
   - Discarding on reload needs no script: the page never renders editing rows, so a grid reload replaces them.

   Verified: UI unit tests (537, four new for the add row), admin panel unit tests (180), HTTP integration tests (492, nine new: Add buttons for each `AddButton` and `Position`, the dimming attribute, the created toast and its label) and EF Core integration tests (87) pass; consumer references regenerated with no drift. Headless Chromium on AdminPlayground with Products temporarily inline (edit and create, the Category column `EditsField(CategoryId)`; reverted, database restored), at 1280 and 390 px wide, 12 of 12 each: add row and header Add, focus and selection, dimming, actions width, one row at a time, Escape, Enter with the flash, the grid's and header's Add, the lookup sheet from a new row's cell, saving the new row in place with the toast, and a sort reload discarding an editing row.
5. **Samples, docs and verification.** An inline resource in AdminPlayground, docs pages and regenerated consumer skills, and browser checks of the flows across the theme fixtures at desktop and phone widths.
   Done 2026-10-16, not committed:
   - AdminPlayground: Departments edit and add inline (one field, as the entity has only a name; more fields need a migration).
   - Docs: the website has no admin panel pages, so the consumer docs are the stellar-admin skill: a new `references/inline-editing.md` (turning it on, fields and columns with `EditsField` and the startup failures, adding rows, behaviour, editors in cells and `GridEditorTemplates/`, labels, `ResourceListRequest.Keys` for custom data sources), linked from `SKILL.md` (task table, namespaces), forms, index pages, editors and resources, and listed in `skills/README.md` and the skills guide. DocsSamples has a DataGrid "Add Row" sample (`DataGrid/_AddRow`), in the generator list and the UI skill's curated examples.
   - Fixes found on the way: the global labels builder had no `CreatedMessage` (`labels.Index(index => index.CreatedMessage = ...)`), now added. A rejected save showed the field's old value instead of what was typed: htmx's settle step gives a swapped-in input the old input's attributes, and htmx refocuses the field before restoring them, so the focused input keeps the old value. The Save button swaps with `settle:0`. `sa-data-grid-add-row`'s `position` is now nullable like the library's other enum properties: Razor prefixes the type name onto a non-nullable enum's value, so `position="DataGridAddRowPosition.Bottom"` didn't compile.

   Verified: HTTP integration tests (493, one new case: the created toast from the global label), admin panel unit tests (180), EF Core integration tests (87), UI unit tests (537) and UI integration tests (19) pass; consumer references regenerated with no drift; the skill validator passes (85 Markdown files). DocsSamples renders the add row sample. Headless Chromium on AdminPlayground's Departments (database restored afterwards) with the default theme and the Ledger, Ops and Soft fixtures, light and dark, at 1280 and 390 px: 112 of 112 checks (the add row and header Add, no page overflow, edit focus and selection, dimming, actions width, Escape, Enter with the flash, a rejected empty name keeping the empty value with the cell error and focus, the grid's Add cancelling an edit, saving a new row in place with the toast). Screenshots reviewed for the default, Ops, Soft and Ledger themes at both widths.

## After phase 5

- 2026-10-16, committed as 380987f: the grid's add row stays in place and dims while a row is edited, instead of disappearing and moving every row up; it is hidden only while a new row is added.
- 2026-10-16, committed as dcd4bf2: dimming locks the other rows and the page's controls (see phase 4's dimming note).
- 2026-10-16, not committed: `AfterSave` (`InlineRowAfterSave.ReloadGrid`, the default, or `KeepInPlace`) on both inline builders (decision 9). The Edit and Add buttons pass the page's query (page, page size, search, scope, sort) to the row requests, and the Save URL keeps it. A reload responds with the index for that query, retargeted to `#index-page-data-grid` with `HX-Retarget`, `HX-Reswap` and `HX-Reselect`, and names a saved row the page shows in `SA-Saved-Row`, which the script flashes and focuses; a row the page doesn't show gets a toast with the new `NotShownMessage` description (an edited row's toast title is the new `SavedMessage`), and focus goes to the Add button or the first row. An edited row's details aren't refreshed out of band on a reload, as the reloaded grid brings them. `KeepInPlace` keeps the row swap and reloads the grid's footer from the page's address, so the pager's count follows.

  Verified: HTTP integration tests (500, seven new: the query on the Edit and Add buttons and the Save URL, the reload for edit and create with the saved row shown or not, the toasts and the configured messages; three renamed to `KeepInPlace`), admin panel unit tests (180) and EF Core integration tests (87, the saved row through the index query now on `KeepInPlace`). Headless Chromium on Departments (a copy of the database), the four themes light and dark at 1280 and 390 px, 72 of 72 each: the earlier flows plus a new row saved and reloaded in name order with the flash and focus, and a new row the search hides with its toast and focus on the Add button. With Departments temporarily on `KeepInPlace` (reverted): a new row stayed at the top of a descending sort and flashed, the pager went from 1–25 of 37 to 1–25 of 38, and an edited row stayed in place.
