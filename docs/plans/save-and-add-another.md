# Save and add another

Status: API decided, not implemented (2026-10-20).

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
- Cancel on a new row is handled by `sel-table` itself: it removes the row in the capture phase and stops the click, so the panel never hears of it. Ending a run (and its `ReloadGrid` reload) needs the panel script to see that cancel, for example through an event `sel-table` dispatches when it removes a new row. The prototype listens in the capture phase above `sel-table`.

Not yet done: implementation, samples, website docs and the consumer reference.
