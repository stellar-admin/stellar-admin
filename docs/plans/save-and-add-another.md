# Save and add another

Status: implemented, awaiting Jerrie's review (2026-10-20).

Let staff add several records in a row without starting Add again after each save, on the create page and with inline create. From the Voyager Travel session feedback.

Read the current create and inline-create code first, then propose the behaviour and any public API to Jerrie before implementing.

## Decisions (2026-10-20)

Explored in `sandbox/html/save-and-add-another.html`, a live create page and inline-create grid built from the admin panel's captured markup.

- Off by default. `create.EnableAddAnother()` turns it on for the create page and, with `UseInlineRow()`, the inline row; one setting covers both.
- Create page: a second submit button, "Save and add another", before the primary button. No "Add another" checkbox.
- Inline row: a third row action, "Save and add another", between Save and Cancel. Enter keeps saving and closing the row; no keep-adding mode.
- Confirmation is the usual "added" toast; no list of the records added.
- Values can carry over to the next record with `KeepForNext`.
- Shortcuts everywhere, on the create and edit pages and inline create and edit rows: Ctrl+Enter saves, Ctrl+Shift+Enter saves and adds another. Cmd works in place of Ctrl on every platform. Plain Enter still saves an inline row.
- The lookup create sheet is unchanged: it closes and selects the created item.

## API

On `ResourceCreateBuilder<TModel>`, following `EnableSearch` and `EnablePaging`:

```csharp
public ResourceAddAnotherBuilder<TModel> EnableAddAnother();
public ResourceCreateBuilder<TModel> EnableAddAnother(Action<ResourceAddAnotherBuilder<TModel>> configure);
```

A new `ResourceAddAnotherBuilder<TModel>`:

```csharp
// Carries the field's saved value into the next new record; one call per field
public ResourceAddAnotherBuilder<TModel> KeepForNext<TProp>(Expression<Func<TModel, TProp>> property);

// The button label; the default comes from ResourceCreateLabelsBuilder ("Save and add another")
public string? Label { set; }
```

```csharp
resource.AllowCreate(create =>
{
    create.Fields(fields => { /* ... */ });
    create.EnableAddAnother(another =>
    {
        another.KeepForNext(tour => tour.DestinationId);
        another.KeepForNext(tour => tour.Days);
    });
});
```

`KeepForNext` must name an editable create field; options validation fails otherwise.

## Behaviour

- Create page: the button posts `after=add-another`. A successful save queues the "added" toast and redirects back to Create (Post/Redirect/Get) with a new model from the factory and the kept values, carried in TempData. Focus goes to the first field. A failed save redisplays the form as today.
- Inline row: the row posts with `?another=true`. The response is the saved row and a new open row with the kept values. Rows added in the run stay next to the open row and are not dimmed; a row under the open row counts them. `AfterSave` applies once, when the run ends with Save, Cancel or Esc.
- Cancel on a new row is handled by `sel-table` itself: it removes the row in the capture phase and stops the click. The panel script already listens for that click in the capture phase on the document, before `sel-table` sees it, so no `sel-table` event was needed: the same listener ends the run.

## Implementation (2026-10-20)

- API as above: `ResourceCreateBuilder<TModel>.EnableAddAnother()` and its configure overload, `ResourceAddAnotherBuilder<TModel>` with `KeepForNext` and `Label`, and `ResourceAddAnotherOptions` on `ResourceCreateOptions.AddAnother`. The default label is `ResourceCreateLabelsBuilder.AddAnotherLabel` ("Save and add another"); the toast is the index's `CreatedMessage`. `AddAnotherOptionsValidator` rejects a `KeepForNext` that names no editable create field.
- Create page: the button is `name="after" value="add-another"`. The kept fields' posted form values go into TempData as JSON and bind onto the next factory model, so any type the form can post carries over. The form marks `data-focus-first-field="true"` after adding another.
- Inline row: the add another button posts `?another=True&added=N`; the new row's buttons carry the count, so a rejected save keeps it. The response (`_IndexAddAnotherRows`) renders the saved row and the new row with `_IndexDataGrid` twice, in grid order, and names the saved row in `SA-Saved-Row`; the script marks it `data-added`. The status row under the new row is rendered by the server.
- Shortcuts: the panel script handles Ctrl/Cmd+Enter and Ctrl/Cmd+Shift+Enter on `form[data-resource-form]` and in editing rows; a plain Enter in a create page field presses the submit button, since the add another button comes first. `Ctrl` in rendered shortcuts shows as ⌘ on a Mac (`data-mod-key`).
- Samples: AdminPlayground Destinations (create page, keeps the country) and Departments (inline).
- Docs: the consumer skill's forms, inline editing and customization references and its task table. The website documents only the UI library, so it has no admin panel page to update.

## Verification (2026-10-20)

- `dotnet test` on `StellarAdmin.slnx`: 1641 passed, including 13 new HTTP tests (`ResourceAddAnotherTests`) and 2 builder tests.
- Headless Chromium against AdminPlayground (port 5207, a scratch copy of `app.db`):
  - Create page: Ctrl+Shift+Enter saves, toasts and reopens the form with the country kept and focus on the first field.
  - Create page: Enter in a field and Ctrl+Enter both save and go to the index, and an invalid add another shows the errors.
  - Edit page: Ctrl+Enter saves.
  - Inline: Ctrl+Shift+Enter, Cmd+Shift+Enter and the button each add a row and keep the new row open with "N added". The added rows aren't dimmed, and a rejected save keeps the count.
  - Inline: Esc, X and Enter each end the run and reload the grid with nothing locked.
  - No console errors beyond the playground's intentional missing image.
- The `build/skill-validation` check passes.

Not yet done: Jerrie's review. Open points: the inline status row's text ("N added", the shortcut hint) is fixed English in `_IndexDataGrid.cshtml`, not a label; the prototype's "Kept from …" note under kept fields was not built.
