# Inline editing

A small resource, whose two to four fields are all shown as columns, can be edited and added in the index grid's rows instead of on the edit and create pages. Resources with more fields keep using the pages ([forms](forms.md)).

## Contents

- [Turning it on](#turning-it-on)
- [Fields and columns](#fields-and-columns)
- [Adding rows](#adding-rows)
- [After a save](#after-a-save)
- [What the user sees](#what-the-user-sees)
- [Editors in cells](#editors-in-cells)
- [Labels](#labels)
- [Custom data sources](#custom-data-sources)

## Turning it on

Call `UseInlineRow()` on the edit or create builder. Edit and create choose independently, for example inline edit with create on a page:

```csharp
admin.AddEfCoreResource<AppDbContext, Destination>(resource =>
{
    resource.Index(index => index.Columns(columns =>
    {
        columns.Add(destination => destination.Name);
        columns.Add(destination => destination.Country!.Name, column => column.Title = "Country").EditsField(destination => destination.CountryId);
        columns.Add(destination => destination.BestSeason);
        columns.Add(destination => destination.Active);
    }));

    resource.AllowEdit(edit =>
    {
        edit.Fields(fields =>
        {
            fields.Add(destination => destination.Name);
            fields.Add(destination => destination.CountryId).UseEditor<LookupSheetEditor>(/* see relationships.md */);
            fields.Add(destination => destination.BestSeason);
            fields.Add(destination => destination.Active);
        });
        edit.UseInlineRow();
    });

    resource.AllowCreate(create =>
    {
        create.Fields(/* the same fields */);
        create.UseFactory(() => new Destination { Active = true });
        create.UseInlineRow(inline =>
        {
            inline.Position = InlineCreatePosition.Bottom;   // Top (default) or Bottom
            inline.AddButton = InlineAddButton.Grid;         // Header, Grid or Both (default)
            inline.DimOtherRows = false;                     // default true
            inline.AfterSave = InlineRowAfterSave.KeepInPlace; // ReloadGrid (default) or KeepInPlace
        });
    });
});
```

- An operation is edited either inline or on its page, never both. An inline operation's page is not registered, and with inline edit the view page shows no Edit button.
- `DimOtherRows` (default true) dims and locks the other rows and the page's controls (Add buttons, scopes, search, sort links and pager) while a row is edited or added, until it is saved or cancelled. With `edit.UseInlineRow(inline => inline.DimOtherRows = false)` they stay usable, and editing or adding another row cancels the first.
- `InlineCreatePosition`, `InlineAddButton` and `InlineRowAfterSave` are in `StellarAdmin.Resources`.
- Inline rows need a resource key. EF Core resources have one; `AddResource<T>` needs `UseKey`.

## Fields and columns

The operation's fields define what the row edits, and every field must match a column. A field matches the column with the same property path. A column showing a different path than the field it edits names the field with `EditsField`, as the Country column above shows `Country.Name` and edits `CountryId`. `EditsField("CountryId")` takes the name instead, for a field of a separate edit or create model (`AllowEdit<TModel, THandler>`).

Startup fails (a 500 on the first panel request) when:

- an inline field matches no column;
- a `[Required]` property of the create model has no field;
- the resource has no key.

Columns without a field keep showing their value in the editing row. Display fields (`AddDisplayField`) and read-only fields don't need a column.

## Adding rows

The Add button inserts a new row filled from `UseFactory`, at the top of the grid or at the bottom of the current page. `AddButton` puts it in the page header (labelled with `CreateLabel`), in a row of the grid, or both. A toast confirms the saved row.

## After a save

`AfterSave` on `edit.UseInlineRow(...)` and `create.UseInlineRow(...)` chooses what the grid shows after a row is saved:

- `InlineRowAfterSave.ReloadGrid` (default): the current page reloads with its search, scope, sort and paging, so the saved row lands where they put it. When the page shows it, it flashes and keeps focus. When it doesn't (it sorts onto another page, or the search or scope leaves it out), a toast says so (`NotShownMessage`), and focus goes to the Add button, or to the first row after an edit.
- `InlineRowAfterSave.KeepInPlace`: the saved row stays where it was edited or added until the grid next reloads, and the pager's count is updated. Suits quick entry into small lists.

## What the user sees

- The row's Edit action turns the row into fields, with Save and Cancel in place of the row actions. One row is edited at a time.
- Enter in a text field saves and Escape cancels. Cancel restores the row without a request.
- On success the row comes back as it displays, through the index query ([after a save](#after-a-save)). Field errors appear under their cells. Errors the row can't place, such as a `ResourceOperationResult` error without a field, appear in a message under the row.
- While dimming is on, paging, sorting, search and scopes are locked until the row is saved or cancelled; with it off, they discard an unsaved row without asking.
- A lookup column opens the lookup sheet from the row. Row details stay in place while their row is edited.
- On narrow screens the grid scrolls sideways; rows stay editable.

## Editors in cells

A cell renders the field's editor through a grid editor template, resolved in `GridEditorTemplates/` instead of `EditorTemplates/`. These editors have one: text input (including number, email, URL and the other text types), select, checkbox, toggle, date, time and date-time inputs, segmented control, lookup sheet, image (a thumbnail button) and icon (a small button that opens the picker). Textarea, radio group, checkbox group, toggle group, slider, one-time code and multi-lookup sheet do not.

A field whose editor has no grid template does not fail at startup: in development its cell shows an error naming the missing template, and in production the cell is empty. Saving binds only the fields whose cells rendered an editor, so it never overwrites a value; on create, a required field without an editor fails validation in the row message.

A custom editor ([editors](editors.md#overriding-and-custom-editors)) works in a cell once a view with the same `TemplateName` exists under `GridEditorTemplates/`, for example `Areas/StellarAdmin/Views/Shared/GridEditorTemplates/{TemplateName}.cshtml`. Render one compact input without a label; the cell supplies the field's label as its accessible name. To change a built-in cell editor, add `GridEditorTemplates/Editors/{Name}.cshtml`, for example `Editors/TextInput.cshtml`.

## Labels

| Index property | Global label (`ConfigureResourceLabels`, `labels.Index(...)`) | Default |
| --- | --- | --- |
| `index.SaveLabel` | `SaveLabel` | `Save` |
| `index.CancelLabel` | `CancelLabel` | `Cancel` |
| `index.CreatedMessage` | `CreatedMessage` | `{singular label} added` (the toast) |
| `index.SavedMessage` | `SavedMessage` | `{singular label} saved` (the toast for an edited row the reloaded page doesn't show) |
| `index.NotShownMessage` | `NotShownMessage` | `It isn't on this page with the current search, scope and sort.` (the toast's description) |
| `index.CreateLabel` | `CreateLabel` | `Create` (the Add buttons) |

## Custom data sources

A data source registered with `AddResource<T>` works inline through its existing create and edit handlers. After a save the panel reloads the row through `ListAsync` with `ResourceListRequest.Keys` set to the saved key, formatted in the invariant culture, and no search, scope or paging. Return only the records with those keys. A source that ignores `Keys` still works, since the panel picks the row by key, but it returns more than needed.
