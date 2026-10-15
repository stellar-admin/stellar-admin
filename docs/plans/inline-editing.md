# Inline editing on the resource index

Status: proposed (2026-10-15). Decisions recorded; no code written. Prototypes: `sandbox/html/inline-editing.html` (six concepts compared, with complexity ratings) and `sandbox/html/inline-row-edit.html` (row edit mode, inline add and the edit sheet). The row edit prototype follows the either/or decisions below; the sheet stands in for the form.

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
11. A row's details collapse while that row is being edited.
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
        edit.UseInline(inline => inline.DimOtherRows = true);           // default
    });

    resource.AllowCreate(create =>
    {
        create.Fields(...);
        create.UseFactory(() => new Destination { Active = false });
        create.UseInline(inline =>
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

- Without `UseInline()`, an operation keeps its page as today. The sheet later becomes another choice on the same builders (`edit.UseSheet()`), with the page as the default.
- The cell renders the field's editor through its grid template (decision 13), with the field's label as its accessible name and its validation shown compactly under the cell.
- Startup fails when an inline operation's field matches no column, or when a `[Required]` create model property has no column.
- `InlineCreatePosition` (`Top`, `Bottom`) and `InlineAddButton` (`Header`, `Grid`, `Both`) are new enums of their own.
- Save and Cancel labels join the existing index labels.

## Behaviour

- Edit: the row's Edit action loads the editing row from the edit loader (the edit model may differ from the grid's item) and posts the fields through the existing edit handler. On success the row comes back as it displays, reloaded through the index query. On failure the editing row comes back with errors under its cells, and errors the row cannot place appear in the message under the row.
- Add: an Add button inserts an editing row filled from `UseFactory`, scrolls to it and focuses its first field. Saving goes through the existing create handler. The new row stays where it was added until the grid next reloads, and a toast confirms it. Errors behave as for Edit.

## Work outline

- `UseInline()` on the edit and create builders, column matching and `EditsField`, and their startup checks; skipping the page registration and the View page's Edit button for inline operations.
- An endpoint that renders the editing row (edit and create), and one that saves the submitted fields, reusing `TryBindConfiguredFieldsAsync` and the existing handlers.
- Re-rendering one display row after a save.
- Resolving editor templates in `GridEditorTemplates/` for cells, with the development error and the empty production cell; binding only the rendered fields.
- Grid templates for the editors in decision 14, including the lookup trigger that opens the lookup sheet from a cell.
- The grid script: one editing row at a time, Enter and Escape, column widths held while editing, dimming, collapsing the row's details, and the Add buttons scrolling to and focusing the new row.
