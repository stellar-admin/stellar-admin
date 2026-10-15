# Inline editing on the resource index

Status: in progress (2026-10-15). Phases 1 and 2 done; phases 3–5 outstanding. Prototypes: `sandbox/html/inline-editing.html` (six concepts compared, with complexity ratings) and `sandbox/html/inline-row-edit.html` (row edit mode, inline add and the edit sheet). The row edit prototype follows the either/or decisions below; the sheet stands in for the form.

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
9. New rows are added at the top by default, or at the bottom. Bottom means the bottom of the current page.
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
- Add: an Add button inserts an editing row filled from `UseFactory`, scrolls to it and focuses its first field. Saving goes through the existing create handler. The new row stays where it was added until the grid next reloads, and a toast confirms it. Errors behave as for Edit.

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
   - CSS names: `.sa-cell-check` holds a checkbox or a switch (the prototype's `.sa-cell-switch`); the grid asks for dimming with `data-inline-dim`.

   Verified: admin panel unit tests (178), HTTP integration tests (471, including `GridEditorTests`, which render cells through a `grid-cells` view override), EF Core integration tests (86) and UI unit tests (513) pass. No browser check yet: nothing renders an inline row until phase 3.
3. **Server endpoints.** `EditRow` and `CreateRow` (GET renders the editing row; POST binds only the rendered fields through the existing loader and handlers, and returns the display row, or the editing row with cell errors and a row message). The row posts with htmx (`hx-include="closest tr"`), since a `<form>` cannot sit inside a `<tr>`; the antiforgery token goes in a request header. Builds on the grid's row keys, edit templates and `rows-only` ([data grid inline editing](data-grid-inline-editing.md), stages 1–3). The grid item stays the resource, so columns without an editor keep their display; the endpoint passes the edit model to `GridEditorAsync`, which takes it as an argument, and the fields get the `rows[key]` prefix.
4. **Grid script and behaviour.** One editing row at a time, Enter and Escape, dimming (column widths, cancel without a request and the kept selection and details come from `sel-table`, stage 4 of [data grid inline editing](data-grid-inline-editing.md)), the Add buttons scrolling to and focusing the new row, discarding an unsaved row on reload, the saved flash and toast, and the lookup sheet opening from a cell.
5. **Samples, docs and verification.** An inline resource in AdminPlayground, docs pages and regenerated consumer skills, and browser checks of the flows across the theme fixtures at desktop and phone widths.
