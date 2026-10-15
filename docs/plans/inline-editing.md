# Inline editing on the resource index

Status: proposed (2026-10-15). Decisions recorded; no code written. Prototypes: `sandbox/html/inline-editing.html` (six concepts compared, with complexity ratings) and `sandbox/html/inline-row-edit.html` (row edit mode, inline add and the edit sheet).

## Goal

Let a small resource (two to four fields, all shown as columns) be edited and added directly in the index grid, without leaving the page. Resources with many fields keep using the edit page, or the edit sheet once it exists.

## Decisions (Jerrie, 2026-10-15)

1. Row edit mode only for now (prototype option A). The row's Edit action turns the row into fields, with Save and Cancel in place of the actions. Handing a row off to the edit sheet (option C) becomes an option once the edit sheet is built.
2. Columns are editable only when marked explicitly.
3. A lookup column opens the lookup sheet from the row.
4. Row edit stays on at every width. On narrow screens the grid scrolls sideways.
5. Inline add is an option. New rows are added at the top by default, or at the bottom.
6. Dimming the other rows while one is edited or added is an option, on by default.
7. The feature boundary is `index.EnableInlineEditing(...)`.
8. When edit uses its own model (`AllowEdit<TModel, THandler>`), an editable column is matched to the edit field by field name. A column that matches no edit field fails at startup.
9. Inline add is a separate opt-in inside inline editing.
10. The page header's Add button always opens the create page when create is allowed, and is hidden when it is not, as today. Inline add always has its own "Add" button inside the grid, in the first row for Top and the last row for Bottom.
11. Required create fields that are not editable columns are accepted for v1. Saving then fails with a message linking to the create page. Developers should enable inline add only when every required field can be shown inline.
12. A row's details collapse while that row is being edited.
13. An unsaved editing row is discarded silently when the grid reloads (paging, sorting, search or scopes).

## API

```csharp
admin.AddEfCoreResource<AppDbContext, Destination>(resource =>
{
    resource.AllowEdit(edit => edit.Fields(fields =>
    {
        fields.Add(d => d.Name);
        fields.Add(d => d.CountryId).UseEditor<LookupSheetEditor>(...);
        fields.Add(d => d.BestSeason);
        fields.Add(d => d.Active);
    }));
    resource.AllowCreate(create => create.UseFactory(() => new Destination { Active = true }));

    resource.Index(index =>
    {
        index.Columns(columns =>
        {
            columns.Add(d => d.Name).Editable();
            columns.Add(d => d.Country!.Name, column => column.Title = "Country").Editable(d => d.CountryId);
            columns.Add(d => d.BestSeason).Editable();
            columns.Add(d => d.Active).Editable();
        });

        index.EnableInlineEditing(inline =>
        {
            inline.DimOtherRows = true;                                       // default
            inline.EnableCreate(create => create.Position = InlineCreatePosition.Top); // Top (default) or Bottom
        });
    });
});
```

- `column.Editable()` edits the edit form's field with the same path. `column.Editable(d => d.CountryId)` edits a different field than the column shows. The cell reuses that field's editor, label and validation in a compact form.
- Startup fails when inline editing is enabled without `AllowEdit`, when `EnableCreate` is used without `AllowCreate`, or when an editable column matches no edit field.
- `InlineCreatePosition` is a new enum of its own (`Top`, `Bottom`).
- Save and Cancel labels join the existing index labels.

## Behaviour

- Edit: the row's Edit action loads the editing row from the edit loader (the edit model may differ from the grid's item) and posts only the editable fields through the existing edit handler. On success the row comes back as it displays, reloaded through the index query. On failure the editing row comes back with errors under its cells. Errors on fields the row does not show appear in a message under the row, with a link to the edit page.
- Add: the grid's own Add button inserts an editing row filled from the create form's `UseFactory`. Saving goes through the existing create handler. The new row stays where it was added until the grid next reloads, and a toast confirms it. Errors behave as for Edit, linking to the create page.
- The edit and create pages stay registered: they are the fallback for errors the row cannot show, and the View page's Edit button still opens the edit page.

## Work outline

- Column editability and its startup checks.
- An endpoint that renders the editing row (edit and create), and one that saves the submitted fields, reusing `TryBindConfiguredFieldsAsync` and the existing handlers.
- Re-rendering one display row after a save.
- Compact cell variants of the field editors, and the lookup trigger that opens the lookup sheet from a cell.
- The grid script: one editing row at a time, Enter and Escape, column widths held while editing, dimming, and collapsing the row's details.
