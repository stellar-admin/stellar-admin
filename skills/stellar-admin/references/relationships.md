# Relationships

How to edit and show related records: a foreign key picked from a list, a searchable lookup with "create new", many-to-many selections, and related values in columns and display fields.

## Contents

- [Foreign key as a select](#foreign-key-as-a-select)
- [Foreign key as a searchable lookup](#foreign-key-as-a-searchable-lookup)
- [Creating from a lookup](#creating-from-a-lookup)
- [Lookup sheet settings](#lookup-sheet-settings)
- [Many-to-many with MultiLookupSheetEditor](#many-to-many-with-multilookupsheeteditor)
- [Lookups without EF Core](#lookups-without-ef-core)
- [Showing related values](#showing-related-values)

A foreign-key field without an editor renders a number input. Always give it one of the editors below.

## Foreign key as a select

For a short related list, use `SelectEditor` (or any choice editor) with the EF Core `UseItems` from `StellarAdmin.EntityFrameworkCore`. The type arguments are the context, the related entity and the key type:

```csharp
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources.Editors;

fields.Add(tour => tour.GuideId).UseEditor<SelectEditor>(select =>
    select.UseItems<VoyagerDbContext, Guide, int>(
        guide => guide.Id,
        guide => guide.Name,
        items => items.OrderBy(guide => guide.Name)));
```

- The query projects only value, text and the configured extras; it honours global query filters.
- The items builder: `OrderBy`, `UseDescription`, `UseGroup` (option groups in a select), and media with `UseIcon`, `UseAvatar`, `UseImage` or `UseCode`.
- An optional key (`int? GuideId` without `[Required]`) gets a "Not set" choice; relabel it with `EmptyChoiceText`.
- `RadioGroupEditor` with `Appearance = RadioGroupAppearance.Cards` and the same `UseItems` shows the related rows as cards.
- Share the configuration between create and edit with an `Action<SelectEditor>` variable or a helper method.

## Foreign key as a searchable lookup

For a long related list, use `LookupSheetEditor`. The field shows the selection; Choose opens a sheet that searches as the user types and pages results.

```csharp
fields.Add(booking => booking.CustomerId).UseEditor<LookupSheetEditor>(lookup =>
{
    lookup.UseItems<VoyagerDbContext, Customer, int>(
        customer => customer.Id,
        customer => customer.Name,
        items => items
            .SearchOn(customer => customer.Name, customer => customer.Email)
            .UseDescription(customer => customer.Email)
            .OrderBy(customer => customer.Name));
    lookup.Sheet(sheet => sheet.Title = "Choose a customer");
});
```

- `SearchOn(...)` lists the searched properties, ignoring case (the title by default). `OrderBy` defaults to the title.
- `UseDescription` adds secondary text and switches the field to a card layout. Media: `UseCode`, `UseAvatar`, `UseImage`, `UseIcon`.
- On an EF Core edit page the selected item comes from the navigation (`Booking.Customer`), loaded with the record. The navigation is inferred from the foreign key; name it with `items.ReferenceFrom<Booking>(booking => booking.Customer)` when it can't be inferred.

## Creating from a lookup

`lookup.EnableCreate()` adds a New button that opens the related resource's create form in a sheet. Saving closes the sheet and selects the new record.

```csharp
lookup.UseItems<VoyagerDbContext, Customer, int>(customer => customer.Id, customer => customer.Name);
lookup.EnableCreate();
```

- The related type (`Customer`) must be registered as a resource with `AllowCreate`; otherwise the form throws when it renders. `EnableCreate<TResource>()` names the resource explicitly.
- The lookup's value must be that resource's key. EF Core resources report it automatically; a custom create handler must return `ResourceOperationResult.Success(key)`.
- Users the related resource's authorization rejects don't see New.
- The sheet's form always stacks its sections.

## Lookup sheet settings

| Method | Settings |
| --- | --- |
| `lookup.Editor(editor => ...)` (the field) | `Layout` (`LookupSheetEditorLayout.Card` or `Input`; `MultiLookupSheetEditorLayout.Chips`, `List` or `Summary` for multi), `EmptyText`, `AllowClear` (single only; defaults to optional fields), `ShowMedia` |
| `lookup.Sheet(sheet => ...)` (the search sheet) | `Title`, `SearchPlaceholder`, `MinimumSearchLength`, `PageSize` (20), `ShowMedia` |
| `lookup.EnableCreate()` / `EnableCreate<TResource>()` | New button ([above](#creating-from-a-lookup)) |

The fixed lookup texts ("Choose {field}", "New", "Load more") come from `admin.ConfigureResourceLabels(labels => labels.Lookup(lookup => lookup.ChooseLabel = context => $"Select {context.FieldLabel}"))`.

## Many-to-many with MultiLookupSheetEditor

`MultiLookupSheetEditor` selects several keys into a collection property (`List<int>`, `string[]`, ...). It takes the same `UseItems`, `Editor`, `Sheet` and `EnableCreate` as the single lookup.

```csharp
fields.Add(form => form.DestinationIds).UseEditor<MultiLookupSheetEditor>(lookup =>
{
    lookup.UseItems<VoyagerDbContext, Destination, int>(
        destination => destination.Id,
        destination => destination.Name,
        items => items.UseCode(destination => destination.AirportCode));
    lookup.Editor(editor => editor.Layout = MultiLookupSheetEditorLayout.Chips);
});
```

EF Core resources bind only mapped scalar properties, so a many-to-many navigation can't be a field directly. Use a form model with a key collection and a handler that loads and saves the join ([forms](forms.md#custom-create-and-edit-models)):

```csharp
public sealed class TourForm
{
    public string Name { get; set; } = "";
    public List<int> DestinationIds { get; set; } = [];
}

public sealed class TourEditHandler(VoyagerDbContext db) : IResourceEditHandler<TourForm>
{
    public async Task<TourForm?> FindAsync(string id, CancellationToken cancellationToken) =>
        await db.Tours.Where(tour => tour.Id == int.Parse(id))
            .Select(tour => new TourForm { Name = tour.Name, DestinationIds = tour.Destinations.Select(d => d.Id).ToList() })
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<ResourceOperationResult> UpdateAsync(string id, TourForm form, CancellationToken cancellationToken)
    {
        var tour = await db.Tours.Include(t => t.Destinations).SingleOrDefaultAsync(t => t.Id == int.Parse(id), cancellationToken);
        if (tour is null)
        {
            return ResourceOperationResult.NotFound();
        }

        tour.Name = form.Name;
        tour.Destinations = await db.Destinations.Where(d => form.DestinationIds.Contains(d.Id)).ToListAsync(cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ResourceOperationResult.Success();
    }
}

resource.AllowEdit<TourForm, TourEditHandler>(edit => edit.Fields(fields =>
{
    fields.Add(form => form.Name);
    fields.Add(form => form.DestinationIds).UseEditor<MultiLookupSheetEditor>(lookup => { /* as above */ });
}));
```

`[MinLength(1)]` and `[MaxLength(3)]` on the collection validate the number of selections. For a short list of choices, `CheckboxGroupEditor` or `ToggleGroupEditor` with EF `UseItems` also bind a key collection.

## Lookups without EF Core

Register a source implementing `ILookupSource<TEntity, TValue>` and pass it as the first type argument:

```csharp
builder.Services.AddScoped<AirportLookupSource>();

fields.Add(trip => trip.AirportCode).UseEditor<LookupSheetEditor>(lookup =>
    lookup.UseItems<AirportLookupSource, Airport, string>(
        airport => airport.Code,
        airport => airport.City,
        items => items.UseDescription(airport => airport.Country)));

public sealed class AirportLookupSource(AirportClient client) : ILookupSource<Airport, string>
{
    // The airports with these codes; leave out codes that don't exist
    public Task<IReadOnlyCollection<Airport>> FindAsync(IReadOnlyCollection<string> values, CancellationToken cancellationToken) => client.FindAsync(values, cancellationToken);

    // query.Term (null when empty), query.Skip, query.Take; HasMore when more results follow
    public async Task<LookupPage<Airport>> SearchAsync(LookupQuery query, CancellationToken cancellationToken) =>
        await client.SearchAsync(query.Term, query.Skip, query.Take, cancellationToken);
}
```

The non-EF items builder has `UseDescription`, `UseCode`, `UseAvatar`, `UseImage` and `UseIcon` (each returns nothing, so call one per statement). With a custom source, `EnableCreate()` needs `EnableCreate<TResource>()` when the item type isn't a registered resource.

## Showing related values

Columns and display fields read through navigations; EF Core resources load them in the same query:

```csharp
columns.Add(booking => booking.Customer!.Name, column => column.Title = "Customer");

view.Fields(fields =>
{
    fields.Add(booking => booking.Customer!.Name, field => field.Title = "Customer");
    fields.Add(booking => booking.Customer!.Email, field => field.Title = "Customer email");
});
```

Set `Title`, because the default label is the last property's name. Lists of related records (a customer's bookings) have no built-in display; show them in row details or a view page override ([customization](customization.md)).
