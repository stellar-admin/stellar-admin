# Create and edit forms

`resource.AllowCreate(create => ...)` and `resource.AllowEdit(edit => ...)` generate the create and edit pages from the fields you configure. Don't write these pages by hand. Choosing inputs is covered in [editors](editors.md), read-only values in [fields](fields.md), and foreign keys in [relationships](relationships.md). A small resource can be edited and added in the index grid's rows instead ([inline editing](inline-editing.md)).

## Contents

- [Fields](#fields)
- [Page settings](#page-settings)
- [Defaults and generated values](#defaults-and-generated-values)
- [Save and add another](#save-and-add-another)
- [Sharing fields between create and edit](#sharing-fields-between-create-and-edit)
- [Layout: columns, sections and groups](#layout-columns-sections-and-groups)
- [Validation](#validation)
- [Custom create and edit models](#custom-create-and-edit-models)
- [Fields bound by code](#fields-bound-by-code)

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
- The input comes from the property's type and data annotations unless `UseEditor<TEditor>()` chooses one ([editors](editors.md)).
- Label: `field.Title`, else `[Display(Name)]`, else the raw property name (`DepartureDate`). Add `[Display(Name = "Departure date")]` to the model, or set `Title`, for readable labels.
- Prefer `[Display(Name)]` for a readable name: validation messages use it too ("The Departure date field is required."). `Title` changes only the label, so the messages of most fields keep the raw property name.
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
| Field | `Title`, `Description`, `UseEditor`, `ColumnSpan`, `ColumnSpanFull`, `ColumnStart` |

- Breakpoints: `Base`, `Small` (30rem), `Medium` (40rem), `Large` (56rem). They measure the grid's own width, not the viewport, so a narrow section or the create sheet keeps fewer columns.
- `Columns(n)` is one column below `Medium` and `n` from `Medium`. `Columns(columns => columns.Small(2).Large(3))` sets each breakpoint; unset ones inherit from the next smaller. Counts are 1 to 12; the default is 1.
- `ColumnSpan(n)`, `ColumnSpan(span => span.Medium(2))`, `ColumnSpanFull()` (and `BaseFull()`, `SmallFull()`, `MediumFull()`, `LargeFull()` on the span builder). A span wider than the grid is reduced.
- `ColumnStart(n)` or `ColumnStart(start => start.Medium(1).Large(3))` places an item; a start of 1 begins a new row. `SmallAuto()`, `MediumAuto()` and `LargeAuto()` return to normal flow.
- Items fill the grid in configuration order, which is also the tab order.
- Sections nest, and a section or group can hold display fields beside inputs. There are no tabs or wizard steps.

## Validation

Model validation runs on every post, including data annotations (`[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]`, `[Compare]`) and `IValidatableObject`. Invalid posts redisplay the form with the entered values and errors and don't reach the data source. Errors returned by a data source or handler through `ResourceOperationResult.ValidationFailed(field, message)` are shown the same way ([resources](resources.md#operation-results)).

Inputs post as `Entity.{Property}` (with a `Sheet` prefix in a create sheet opened from a lookup); this matters only for tests that post forms directly. Every post needs the antiforgery token the page renders.

## Custom create and edit models

When the form doesn't map one-to-one onto the entity (a password and confirmation, a many-to-many relationship through a join entity of your own, extra side effects), pair a form model with a handler:

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
- This is also the place for save-time side effects; there are no before/after save hooks.
- Labels and titles still come from the resource.

## Fields bound by code

A `FieldBinder` reads and binds a field whose posted values aren't its property's value, such as the keys of a collection of entities. MVC never binds a field with a binder, so its property needs no setter. EF Core resources attach one to many-to-many navigations automatically; a custom data source can attach its own with `UseBinder` after the resource is configured:

```csharp
builder.Services.AddOptions<ResourceOptions<Tour>>().PostConfigure(options =>
{
    foreach (var field in options.Edit?.Fields ?? [])
    {
        if (field.FieldName == nameof(Tour.DestinationCodes))
        {
            field.UseBinder(new DestinationCodesBinder());
        }
    }
});

public sealed class DestinationCodesBinder : FieldBinder
{
    // The value the editor shows
    public override object? GetValue(FieldBindingContext context) => ((Tour)context.Model).DestinationCodes.ToList();

    public override Task BindAsync(FieldBindingContext context, CancellationToken cancellationToken)
    {
        var codes = ((Tour)context.Model).DestinationCodes;
        codes.Clear();
        codes.AddRange(context.Values.Where(value => !string.IsNullOrEmpty(value))!);
        return Task.CompletedTask;
    }
}
```

- One binder instance serves every request; get services from `context.Services`.
- `context.AddError(message)` shows an error under the field. Validation attributes on the property run after binding.
- A create or edit field whose property has no public setter and no binder is left out, and a configuration problem is logged ([configuration problems](configuration-problems.md)).
