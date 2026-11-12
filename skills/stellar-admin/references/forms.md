# Create and edit forms

`resource.AllowCreate(create => ...)` and `resource.AllowEdit(edit => ...)` generate the create and edit pages from the fields you configure. Don't write these pages by hand. Choosing inputs is covered in [editors](editors.md), read-only values in [fields](fields.md), and foreign keys in [relationships](relationships.md). A small resource can be edited and added in the index grid's rows instead ([inline editing](inline-editing.md)). Blocks above, beside or below a form, such as context for the record being edited, are [widgets](widgets.md#view-create-and-edit-pages).

## Contents

- [Fields](#fields)
- [Page settings](#page-settings)
- [Defaults and generated values](#defaults-and-generated-values)
- [Save and add another](#save-and-add-another)
- [Sharing fields between create and edit](#sharing-fields-between-create-and-edit)
- [Layout: columns, sections and groups](#layout-columns-sections-and-groups)
- [Validation](#validation)
- [EF Core form events](#ef-core-form-events)
- [Custom create and edit models](#custom-create-and-edit-models)
- [Fields without a setter](#fields-without-a-setter)

## Fields

```csharp
resource.AllowCreate(create => create.Fields(fields =>
{
    fields.Add(booking => booking.Destination);
    fields.Add(booking => booking.DepartureDate, field => field.Title = "Departs on");
    fields.Add(booking => booking.Notes).Description = "Visible to staff only.";
}));
```

- Fields start empty; only configured fields are shown and bound. Other properties keep their values (the factory's on create, the stored values on edit).
- A field selects a direct property with a public getter and setter. An EF Core many-to-many navigation is the exception: it needs no setter ([relationships](relationships.md#many-to-many-with-multilookupsheeteditor)). Paths through references (`b => b.Customer.Name`) are only for display fields.
- The input comes from the property's type and data annotations unless an editor method such as `UseTextareaEditor()` chooses one ([editors](editors.md)).
- Label: `field.Title`, else `[Display(Name)]`, else the property name split into words (`DepartureDate` reads "Departure Date"), as index headers show it.
- Validation and binding messages name a field by its label, `Title` included ("The Departure Date field is required.").
- Help text: `field.Description`, else `[Display(Description)]`.
- `fields.Add(selector)` returns the field builder; `fields.Add(selector, field => ...)` returns the fields builder. `fields.Clear()` removes earlier fields.
- On edit, the key property is never bound, even if configured.

## Page settings

| Property | Create default | Edit default |
| --- | --- | --- |
| `Title` | `Create {singular label}` | `Edit {singular label}` |
| `SubmitLabel` | `Create {singular label}` | `Save {singular label}` |
| `SectionLayout` | The app-wide forms setting | The app-wide forms setting |

`SectionLayout` is a `FormSectionLayout` from `StellarAdmin.UI`: `Stacked`, `Split` (title and description beside the fields) or `Card`. Set it for every form with `admin.ConfigureUI(ui => ui.ConfigureForms(forms => forms.SectionLayout = FormSectionLayout.Card))`, or per section with `section.Layout`.

## Defaults and generated values

New records come from the entity's parameterless constructor. Use `UseFactory` for defaults and generated values; don't use hidden fields:

```csharp
resource.AllowCreate(create =>
{
    create.UseFactory(() => new Booking
    {
        Reference = $"VT-{Random.Shared.Next(0, 1_000_000):D6}",
        DepartureDate = DateOnly.FromDateTime(DateTime.Today.AddDays(14)),
    });
    create.Fields(fields => { /* Reference is not a field, so the generated value is kept */ });
});
```

- Without a factory, `DateOnly` and `DateTime` fields start at 0001-01-01.
- The factory runs for both the GET and the POST, so a generated value shown on the form (as a display field) is regenerated on save. Generate values that must match what the user saw in a create handler instead.
- Values the database generates (identity keys, defaults) need nothing.

## Save and add another

For entering several records in a row, `EnableAddAnother` adds a "Save and add another" button before the submit button. It saves, shows the "added" toast and opens a new create form with focus on its first field. `KeepForNext` carries a field's saved value into the next record; the other fields start from the factory again:

```csharp
resource.AllowCreate(create =>
{
    create.Fields(fields =>
    {
        fields.Add(tour => tour.Name);
        fields.Add(tour => tour.DestinationId);
        fields.Add(tour => tour.Days);
    });
    create.EnableAddAnother(another =>
    {
        another.KeepForNext(tour => tour.DestinationId);
        another.KeepForNext(tour => tour.Days);
        another.Label = "Save, then the next tour"; // optional; default "Save and add another"
    });
});
```

- It is off by default. One call covers the create page and, with `UseInlineRow()`, the inline create row ([inline editing](inline-editing.md#save-and-add-another)).
- `KeepForNext` must name an editable field of the create form; otherwise it is ignored and a configuration problem is logged.
- The toast text is `index.CreatedMessage` ([inline editing](inline-editing.md#labels)), and the default label comes from `labels.Create(create => create.AddAnotherLabel = ...)`.
- The create sheet opened from a lookup has no such button: it closes and selects the created item.

Create and edit pages also take keyboard shortcuts: Ctrl+Enter or Cmd+Enter saves, and Ctrl+Shift+Enter or Cmd+Shift+Enter saves and adds another. A plain Enter in a field still presses the submit button, not "Save and add another".

## Sharing fields between create and edit

Create and edit have separate field lists. Share them with a helper method; the builder types are in `StellarAdmin.Resources.Builders`:

```csharp
using StellarAdmin.Resources.Builders;

resource.AllowCreate(create => create.Fields(BookingFields));
resource.AllowEdit(edit => edit.Fields(fields =>
{
    fields.AddDisplayField(booking => booking.Reference);
    fields.Add(booking => booking.Status);
    BookingFields(fields);
}));

static void BookingFields(ResourceFieldsBuilder<Booking> fields)
{
    fields.Add(booking => booking.Destination);
    fields.Add(booking => booking.DepartureDate);
}
```

## Layout: columns, sections and groups

Forms lay their fields out on a grid. The form, each section and each group set their own column count; each field, section and group sets how many of its parent's columns it spans.

```csharp
edit.Fields(fields =>
{
    fields.Columns(columns => columns.Large(2));         // two columns side by side on wide screens

    fields.AddSection("Trip", section =>
    {
        section.Description = "Where and when.";
        section.Columns(2);
        section.Add(booking => booking.Destination).ColumnSpanFull();
        section.Add(booking => booking.DepartureDate);
        section.Add(booking => booking.Travellers);
    });

    fields.AddSection("Customer", section =>
    {
        section.Add(booking => booking.CustomerId);
        section.Add(booking => booking.Notes);
    });

    fields.AddGroup(group =>
    {
        group.ColumnSpanFull();
        group.Columns(3);
        group.Add(booking => booking.Status);
        group.Add(booking => booking.Cabin);
    });
});
```

| Item | Members |
| --- | --- |
| Form (`fields`) | `Columns`, `Add`, `AddSection`, `AddGroup`, `AddDisplayField`, `AddDetails`, `Clear` |
| Section (`AddSection(title, ...)`) | Everything on the form, plus `Description`, `Layout`, `ColumnSpan`, `ColumnSpanFull`, `ColumnStart` |
| Group (`AddGroup(...)`) | Everything on the form, plus `ColumnSpan`, `ColumnSpanFull`, `ColumnStart`; no title or border |
| Field | `Title`, `Description`, `Use…Editor` methods, `UseEditor<TEditor>`, `DependsOn`, `ColumnSpan`, `ColumnSpanFull`, `ColumnStart` |

- Breakpoints: `Base`, `Small` (30rem), `Medium` (40rem), `Large` (56rem). They measure the grid's own width, not the viewport, so a narrow section or the create sheet keeps fewer columns.
- `Columns(n)` is one column below `Medium` and `n` from `Medium`. `Columns(columns => columns.Small(2).Large(3))` sets each breakpoint; unset ones inherit from the next smaller. Counts are 1 to 12; the default is 1.
- `ColumnSpan(n)`, `ColumnSpan(span => span.Medium(2))`, `ColumnSpanFull()` (and `BaseFull()`, `SmallFull()`, `MediumFull()`, `LargeFull()` on the span builder). A span wider than the grid is reduced.
- `ColumnStart(n)` or `ColumnStart(start => start.Medium(1).Large(3))` places an item; a start of 1 begins a new row. `SmallAuto()`, `MediumAuto()` and `LargeAuto()` return to normal flow.
- Items fill the grid in configuration order, which is also the tab order.
- Sections nest, and a section or group can hold display fields beside inputs. There are no tabs or wizard steps.

## Validation

Model validation runs on every post, including data annotations (`[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]`, `[Compare]`) and `IValidatableObject`. Invalid posts redisplay the form with the entered values and errors and don't reach the data source. Errors returned by a data source or handler through `ResourceOperationResult.ValidationFailed(field, message)` are shown the same way ([resources](resources.md#operation-results)).

Inputs post as `Entity.{Property}` (with a `Sheet` prefix in a create sheet opened from a lookup); this matters only for tests that post forms directly. Every post needs the antiforgery token the page renders.

## EF Core form events

On an EF Core resource, `AllowCreate` and `AllowEdit` return builders with `Events(...)` and `UseEvents<TEvents>()`. Use them for audit rows, values the form doesn't show, defaults that need services, rules that compare old and new values, saving through a service such as `UserManager`, and side effects after the save. The EF data source still copies the configured fields, binds many-to-many fields and reports concurrency conflicts.

```csharp
resource.AllowEdit(edit =>
{
    edit.Fields(fields =>
    {
        fields.Add(tour => tour.Name);
        fields.Add(tour => tour.Price);
    });
    edit.Events(events =>
        events.OnUpdating = (context, _) =>
        {
            var original = context.Entry.Property(tour => tour.Price).OriginalValue;
            if (context.Entity.Price < original / 2)
            {
                context.Fail(tour => tour.Price, "The price can't drop by more than half in one change.");
            }

            return Task.CompletedTask;
        });
});
```

| Event | Runs | Use for |
| --- | --- | --- |
| `OnInitialized` (create) | After the factory, wherever a create form is shown: the create page, a new inline row, a lookup's create sheet, the next form after save and add another (before the kept values). Not for a submitted form. | Defaults the user sees and can change, from services or the database. |
| `OnCreating`, `OnUpdating` | After the form's values are on the entity the context tracks, before `SaveChanges`, in one transaction with the save. | Audit rows (`context.Db.Add(...)`), values the form doesn't show (`CreatedBy`), rules, saving through a service. |
| `OnCreated`, `OnUpdated` | After the save is committed. | Emails and other side effects. An exception is logged and the form continues as saved. |
| `OnLoaded` (edit) | Each time the edit form loads the record, including before a submitted form is bound. | Filling form-only fields of a wrapped model. |

- Every context has `Db`, `Entity`, `Model` and `Services`. With a plain form, `Model` and `Entity` are the same object.
- The saving context adds `Entry` (original and current values), `Fail(message)` for a form error, `Fail(model => model.Property, message)` for a field error, and `Handled()` when the event saved the record itself. `Fail` rolls back everything, including what the event saved.
- The after-save events have no `Fail` or `Handled`; the record is already saved.
- `UseEvents<TEvents>()` takes a class derived from `EfCoreCreateEvents<TEntity, TModel>` or `EfCoreEditEvents<TEntity, TModel>` that overrides `CreatingAsync`, `UpdatingAsync` and the others. It's registered as a scoped service, so its constructor takes services. It replaces delegates set with `Events`.
- `Fields(...)` returns the core builder, so call `Events` as its own statement rather than chained after `Fields`.
- Events exist only on EF Core resources' create and edit forms. There are no delete or list events.

### Wrapped form models

A form that needs fields the entity doesn't have (a reason for the change, a password) wraps the entity in a model of its own, and names the property that holds it:

```csharp
public sealed class EditTourModel
{
    public Tour Tour { get; set; } = null!;

    [Required, StringLength(500)]
    public string Reason { get; set; } = "";
}

resource.AllowEdit<EditTourModel>(model => model.Tour, edit =>
{
    edit.Fields(fields =>
    {
        fields.Add(model => model.Tour.Name);
        fields.Add(model => model.Tour.DestinationId);
        fields.Add(model => model.Tour.Tags).UseCheckboxGroupEditor(_ => { });
        fields.Add(model => model.Reason);
    });
    edit.Events(events =>
        events.OnUpdating = (context, _) =>
        {
            context.Db.Add(new TourAudit { TourId = context.Entity.Id, Reason = context.Model.Reason });

            return Task.CompletedTask;
        });
});
```

- Fields under the entity's property save, look up, infer their resource and bind many-to-many as they do on the entity's own form. The other fields reach the events only.
- The create model's property must hold a new entity (`public Tour Tour { get; set; } = new();`, or `UseFactory`). The edit model is created with its parameterless constructor and holds the stored entity, which isn't tracked in `OnLoaded`.
- In `OnUpdating`, `context.Model.Tour` is the tracked entity, the same object as `context.Entity`.
- Labels of entity fields come from the entity's properties, not the form model's `[Display]`; set `field.Title` where needed. Validate entity fields with attributes on the entity, or with `IValidatableObject` on the form model, naming the field's path (`new ValidationResult(message, ["Tour.Name"])`).
- Index columns match wrapped fields without the property's name, so `model.Tour.Name` is edited inline by the `Name` column. A `[Required]` form-only property needs a field and column for an inline create row.
- Inputs post as `Entity.Tour.Name`.

## Custom create and edit models

When the form doesn't map one-to-one onto the entity and an EF Core resource's [form events](#ef-core-form-events) don't fit, or the resource has a custom data source, pair a form model with a handler:

```csharp
resource.AllowCreate<NewCustomerForm, NewCustomerHandler>(create => create.Fields(fields =>
{
    fields.Add(form => form.Name);
    fields.Add(form => form.Email);
    fields.Add(form => form.Password);
    fields.Add(form => form.ConfirmPassword);
}));

resource.AllowEdit<CustomerProfileForm, CustomerProfileHandler>(edit => edit.Fields(fields =>
{
    fields.Add(form => form.Name);
    fields.Add(form => form.TagIds);
}));
```

```csharp
public sealed class NewCustomerHandler(VoyagerDbContext db) : IResourceCreateHandler<NewCustomerForm>
{
    public async Task<ResourceOperationResult> CreateAsync(NewCustomerForm form, CancellationToken cancellationToken)
    {
        if (await db.Customers.AnyAsync(customer => customer.Email == form.Email, cancellationToken))
        {
            return ResourceOperationResult.ValidationFailed(nameof(form.Email), "A customer with this email already exists.");
        }

        var customer = new Customer { Name = form.Name, Email = form.Email };
        db.Customers.Add(customer);
        await db.SaveChangesAsync(cancellationToken);
        return ResourceOperationResult.Success(customer.Id.ToString());
    }
}
```

- A create handler implements `IResourceCreateHandler<TModel>`; an edit handler implements `IResourceEditHandler<TModel>` with `FindAsync(id, ...)` (return the form model, or null when missing) and `UpdateAsync(id, model, ...)`. Use the route `id`, not a key on the model.
- Handlers are registered as scoped services automatically and take services through their constructors. They replace the resource's own create or edit, and work with EF Core resources too.
- Return `Success(key)` from create so a lookup's "New" sheet can select the created record.
- Mark passwords `[DataType(DataType.Password)]`; they are left empty when the form redisplays.
- With a custom data source, the handler is also the place for save-time side effects.
- Labels and titles still come from the resource.

## Fields without a setter

A create or edit field's property needs a public setter. A field without one is left out, and a configuration problem is logged ([configuration problems](configuration-problems.md)). Many-to-many navigations on EF Core resources are the exception: they bind as the keys of their related entities.

For a property without a setter, or one stored in another shape than its editor edits (a collection changed through a method, a JSON column edited as a list), give the form a model with a settable property in the editor's shape. On an EF Core resource, wrap the entity and convert in `OnLoaded` and `OnUpdating` ([EF Core form events](#ef-core-form-events)); otherwise convert in a handler ([custom create and edit models](#custom-create-and-edit-models)).
