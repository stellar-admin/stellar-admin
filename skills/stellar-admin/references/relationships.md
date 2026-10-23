# Relationships

How to edit and show related records: a foreign key picked from a list, a searchable lookup with "create new", many-to-many selections, and related records in columns and display fields.

## Contents

- [The related resource](#the-related-resource)
- [Foreign key as a select](#foreign-key-as-a-select)
- [Foreign key as a searchable lookup](#foreign-key-as-a-searchable-lookup)
- [Creating from a lookup](#creating-from-a-lookup)
- [Lookup sheet settings](#lookup-sheet-settings)
- [Many-to-many with MultiLookupSheetEditor](#many-to-many-with-multilookupsheeteditor)
- [Related types that aren't resources](#related-types-that-arent-resources)
- [Lookups without EF Core](#lookups-without-ef-core)
- [Items that depend on another field](#items-that-depend-on-another-field)
- [Showing related values](#showing-related-values)

On an EF Core entity form, a foreign key or many-to-many navigation whose related type is a registered resource needs no editor: it gets a lookup sheet, or a multi-lookup sheet. Configure an editor for a select or another choice editor, or for the lookup's settings. A foreign key without an editor elsewhere (a form model, a custom data source, or a related type with no resource) renders a number input; give it one of the editors below.

## The related resource

Related records come from their own resource. Register the related type as a resource, and say once how its records look when another record refers to them:

```csharp
admin.AddEfCoreResource<Customer>(resource =>
{
    resource.UseTitle(customer => customer.Name);   // optional: see the convention below
    resource.Reference(reference => reference
        .UseDescription(customer => customer.Email)
        .UseAvatar(customer => customer.PhotoUrl)
        .SearchOn(customer => customer.Name, customer => customer.Email)   // EF Core resources only
        .OrderBy(customer => customer.Name)                                 // EF Core resources only
        .Where(customer => customer.IsActive));                             // EF Core resources only
});
```

- **Title.** `UseTitle` sits on the resource builder next to `UseKey`. Without it, the title is the `string` property the class's `[DisplayColumn]` names, then a `string` property called `Name` or `Title`, then the key. An empty title shows the key.
- **Look.** `Reference(...)` has `UseDescription` and one kind of media: `UseIcon`, `UseAvatar`, `UseImage` or `UseCode`. A description shows a lookup's selection as a card.
- **EF Core resources** also have `SearchOn` (default: the index's `EnableSearch`, then the title), `OrderBy` (default: the index's `DefaultSortBy`, then the title) and `Where`, which limits the records any field may pick. Global query filters apply.
- **Keep the title to the record's own properties,** and to something EF Core can translate to SQL. A title EF Core can't translate (one that calls your own method) still shows, but the items are ordered by key and searches don't match it, and a configuration problem is logged; set `SearchOn` and `OrderBy` then.
- **A custom data source** needs nothing new: the items load through `ListAsync`, selected records by `ResourceListRequest.Keys`, lookup searches with `Search` and `Paging`, and every request sorted by the index's default sort. Give it a `UseKey`.
- **Values** are the related resource's key, so the field's type must match it.
- **Access.** Picking a related record doesn't need access to its resource: someone who edits bookings can pick a customer without managing customers.

Fields then refer to the resource. On an EF Core entity form, a foreign key or a many-to-many navigation takes its items from the related entity's resource: with no editor it gets `LookupSheetEditor` or `MultiLookupSheetEditor`, and a choice or lookup editor without items takes them from there too. A form model, or a resource with a custom data source, names the resource with `UseResource<T>()`:

```csharp
// EF Core entity form: the foreign key names the Customer resource
fields.Add(booking => booking.CustomerId);   // a lookup sheet of customers
fields.Add(booking => booking.CustomerId).UseLookupSheetEditor(lookup => lookup.EnableCreate());

// A form model: name the resource
fields.Add(form => form.CustomerId).UseSelectEditor(select => select.UseResource<Customer>());
```

- `UseResource<T>()` is on every choice editor, `LookupSheetEditor` and `MultiLookupSheetEditor`. Explicit `UseItems` or `UseResource` always wins over the inferred resource.
- A field without a `Title` or `[Display(Name)]` is labelled with the related resource's singular label ("Customer", not "CustomerId"). Collections keep their property's name.
- A field that refers to a type with no registered resource, or to an unavailable one, has no items, and a configuration problem is logged when the form renders. Register the resource, or give the editor `UseItems` ([below](#related-types-that-arent-resources)). A foreign key with no editor and no related resource keeps its number input instead, and an info problem is logged.
- A field that needs a different look from the resource's uses `UseItems`, or `UseSource` on a lookup ([below](#lookups-without-ef-core)).

## Foreign key as a select

For a short related list, use `SelectEditor` or any choice editor. The items are the related resource's records, named by their titles, in its order:

```csharp
fields.Add(tour => tour.GuideId).UseSelectEditor(select => select.EmptyChoiceText = "No guide yet");
```

- An optional key (`int? GuideId` without `[Required]`) gets a "Not set" choice; relabel it with `EmptyChoiceText`.
- `RadioGroupEditor` with `Appearance = RadioGroupAppearance.Cards` shows the related records as cards, with the resource's description and media.
- Share the configuration between create and edit with a helper method that takes the editor, typed by the form: `static void UseGuides(SelectEditor<Tour> select) => ...`.

## Foreign key as a searchable lookup

For a long related list, use `LookupSheetEditor`. The field shows the selection; Choose opens a sheet that searches as the user types and pages results.

```csharp
fields.Add(booking => booking.CustomerId).UseLookupSheetEditor(lookup =>
    lookup.Sheet(sheet => sheet.Title = "Choose a customer"));
```

- The search and order come from the related resource ([above](#the-related-resource)).
- On an EF Core edit page the selected record comes from the navigation (`Booking.Customer`), loaded with the record in the same query.

## Creating from a lookup

`lookup.EnableCreate()` adds a New button that opens the related resource's create form in a sheet. Saving closes the sheet and selects the new record.

```csharp
fields.Add(booking => booking.CustomerId).UseLookupSheetEditor(lookup => lookup.EnableCreate());
```

- The related type (`Customer`) must be registered as a resource with `AllowCreate`; otherwise the lookup has no New button, and a configuration problem is logged when the form renders. `EnableCreate<TResource>()` names the resource explicitly.
- The lookup's value must be that resource's key. EF Core resources report it automatically; a custom create handler must return `ResourceOperationResult.Success(key)`.
- Users the related resource's authorization rejects don't see New.
- The sheet's form always stacks its sections.

## Lookup sheet settings

| Method | Settings |
| --- | --- |
| `lookup.Field(field => ...)` (the field) | `Layout` (`LookupSheetEditorLayout.Card` or `Input`; `MultiLookupSheetEditorLayout.Chips`, `List` or `Summary` for multi), `EmptyText`, `AllowClear` (single only; defaults to optional fields), `ShowMedia` |
| `lookup.Sheet(sheet => ...)` (the search sheet) | `Title`, `SearchPlaceholder`, `MinimumSearchLength`, `PageSize` (20), `ShowMedia` |
| `lookup.EnableCreate()` / `EnableCreate<TResource>()` | New button ([above](#creating-from-a-lookup)) |

The fixed lookup texts ("Choose {field}", "New", "Load more") come from `admin.ConfigureResourceLabels(labels => labels.Lookup(lookup => lookup.ChooseLabel = context => $"Select {context.FieldLabel}"))`.

## Many-to-many with MultiLookupSheetEditor

`MultiLookupSheetEditor` selects several keys into a collection property (`List<int>`, `string[]`, ...). It takes the same `UseResource`, `UseItems`, `UseSource`, `Field`, `Sheet` and `EnableCreate` as the single lookup.

```csharp
fields.Add(form => form.DestinationIds).UseMultiLookupSheetEditor(lookup =>
{
    lookup.UseResource<Destination>();
    lookup.Field(field => field.Layout = MultiLookupSheetEditorLayout.Chips);
});
```

On an EF Core resource, add a many-to-many navigation as a field directly. The editor takes its items from the related entity's resource; the field shows the keys of the related entities, and saving adds and removes the join rows. The navigation needs no setter.

```csharp
public sealed class Tour
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<Destination> Destinations { get; } = [];
}

resource.AllowEdit(edit => edit.Fields(fields =>
{
    fields.Add(tour => tour.Name);
    fields.Add(tour => tour.Destinations);   // MultiLookupSheetEditor, from the Destination resource
}));
```

This works for navigations whose join table EF Core maps without an entity type, on create, edit and inline rows. A value that matches no related entity shows an error under the field. A join entity of your own, or a related entity with a composite key, is left out of the form, and a configuration problem is logged; use a form model with a key collection and a handler that loads and saves the join instead ([forms](forms.md#custom-create-and-edit-models)).

`[MinLength(1)]` and `[MaxLength(3)]` on the collection or navigation validate the number of selections. For a short list of choices, `CheckboxGroupEditor` or `ToggleGroupEditor` also edit a key collection or a many-to-many navigation. A many-to-many field with a single-value editor such as `SelectEditor` is left out of the form, and a configuration problem is logged; use one of these three editors. So is one with no editor when the related entity has no registered resource, or when it's on an inline row.

## Related types that aren't resources

A related type without admin pages of its own (countries, cities) keeps `UseItems`. The EF Core overload from `StellarAdmin.EntityFrameworkCore` takes the related entity, and loads from the registered DbContext that maps the entity (`items.UseDbContext<TContext>()` picks one when several do):

```csharp
using StellarAdmin.EntityFrameworkCore;

fields.Add(trip => trip.CountryId).UseSelectEditor(select =>
    select.UseItems<Country>(
        country => country.Id,
        country => country.Name,
        items => items.OrderBy(country => country.Name)));
```

- The value must have the field's type (`int` for an `int` or `int?` foreign key), or its element type for a collection. Otherwise the field keeps its default editor, and a configuration problem is logged.
- The query projects only value, title and the configured extras; it honours global query filters.
- The choice items builder: `OrderBy`, `UseDescription`, `UseGroup` (option groups in a select), `Where<TForm>` ([below](#items-that-depend-on-another-field)), and media with `UseIcon`, `UseAvatar`, `UseImage` or `UseCode`.
- The lookup items builder adds `SearchOn(...)` (the title by default) and `ReferenceFrom<TModel>(model => model.Navigation)`, which names the navigation that loads the selection on an edit page when the foreign key doesn't.
- Title, description and media selectors can read through references and combine values, for example `UseDescription(city => city.Country.Name)`. Don't add null checks for references: the edit page loads the references a selector reads, and a selector that reads a collection or passes the entity to a method gets a separate query instead.

## Lookups without EF Core

For a related type that isn't a resource and doesn't come from EF Core, write a source deriving from `LookupSource<TItem, TValue>`. It says what each item is and shows, and the lookup names only the source. A registered resource with a custom data source needs no source: use `UseResource<T>()`.

```csharp
fields.Add(trip => trip.AirportCode).UseLookupSheetEditor(lookup => lookup.UseSource<AirportSource>());

public sealed class AirportSource(AirportClient client) : LookupSource<Airport, string>
{
    public override string GetValue(Airport airport) => airport.Code;
    public override string GetTitle(Airport airport) => airport.City;
    public override string? GetDescription(Airport airport) => airport.Country;

    // The airports with these codes; leave out codes that don't exist
    public override Task<IReadOnlyCollection<Airport>> FindAsync(IReadOnlyCollection<string> values, CancellationToken cancellationToken) => client.FindAsync(values, cancellationToken);

    // query.Term (null when empty), query.Skip, query.Take; HasMore when more results follow
    public override async Task<LookupPage<Airport>> SearchAsync(LookupQuery query, CancellationToken cancellationToken) =>
        await client.SearchAsync(query.Term, query.Skip, query.Take, cancellationToken);
}
```

- The source is resolved from the request's services, or created with them when the app didn't register it, so its constructor can take services.
- Media: override one of `GetAvatar` (initials when it returns null), `GetCode`, `GetIcon` or `GetImage`. A source that overrides more than one, or whose value type isn't the field's, makes the field keep its default editor, and a configuration problem is logged.
- What a source shows is the same on every field that uses it; a field that needs another description or media uses another source, which can derive from the first and override only that.
- With a source, `EnableCreate()` needs `EnableCreate<TResource>()` when the item type isn't a registered resource.

## Items that depend on another field

`DependsOn` makes a field's items depend on another field of the same form, such as the cities of the selected country. It works with every choice editor, `LookupSheetEditor` and `MultiLookupSheetEditor`, on create and edit pages, create sheets and inline rows. Call it once per field the items depend on; dependencies chain (country → city → neighbourhoods).

```csharp
fields.Add(trip => trip.CountryId).UseSelectEditor(select =>
    select.UseItems<Country>(country => country.Id, country => country.Name));

fields.Add(trip => trip.CityId)
    .DependsOn(trip => trip.CountryId)
    .UseSelectEditor(select =>
        select.UseItems<City>(
            city => city.Id,
            city => city.Name,
            items => items.Where<Trip>((city, trip) => city.CountryId == trip.CountryId)));
```

- When the country changes, the city field reloads its items and its value is cleared. While the country is empty, the city field is disabled and shows "Select Country first"; set other text with `.DependsOn(trip => trip.CountryId, country => country.EmptyText = "Choose a country to see its cities")`.
- The form a filter or loader receives holds only the fields named in `DependsOn` (and the key on an edit page); every other property has its default value. A filter that reads `trip.Region` needs `.DependsOn(trip => trip.Region)` too.
- EF `Where<TForm>` is available on the choice and lookup items builders, with the form's type as the type argument. A related EF Core resource is filtered without it, because the editor knows the form: `select.UseResource<City>(items => items.Where((city, trip) => city.CountryId == trip.CountryId))`. A resource with a custom data source can't apply the filter: it is ignored, and a configuration problem is logged.
- The lookup sheet names its filter ("Filtered by Country: Japan").
- Saving doesn't check that the city belongs to the country; validate that in the app (a custom handler, or `IValidatableObject` on the model) when it matters.
- A `DependsOn` field that isn't an editable field of the same form, or that makes a cycle, is ignored and reported as a configuration problem.

Choice editors without EF Core take a loader whose first parameter is the form, typed by the editor:

```csharp
fields.Add(trip => trip.NeighbourhoodIds)
    .DependsOn(trip => trip.CityId)
    .UseToggleGroupEditor(group =>
        group.UseItems((trip, services, cancellationToken) =>
            services.GetRequiredService<CityGuide>().NeighbourhoodsAsync(trip.CityId!.Value, cancellationToken)));
```

A lookup without EF Core uses a source deriving from `LookupSource<TItem, TValue, TArgument>`, whose search receives an argument the field takes from the form:

```csharp
fields.Add(trip => trip.ArrivalAirport)
    .DependsOn(trip => trip.CountryId)
    .UseLookupSheetEditor(lookup => lookup.UseSource<CountryAirportSource>(trip => trip.CountryId));

public sealed class CountryAirportSource(AirportClient client) : LookupSource<Airport, string, int>
{
    public override string GetValue(Airport airport) => airport.Code;
    public override string GetTitle(Airport airport) => airport.Name;
    public override string? GetCode(Airport airport) => airport.Code;

    // Not filtered: the selected airports load whatever the country
    public override Task<IReadOnlyCollection<Airport>> FindAsync(IReadOnlyCollection<string> values, CancellationToken cancellationToken) => client.FindAsync(values, cancellationToken);

    public override Task<LookupPage<Airport>> SearchAsync(int countryId, LookupQuery query, CancellationToken cancellationToken) =>
        client.SearchAsync(countryId, query.Term, query.Skip, query.Take, cancellationToken);
}
```

- The argument must have the type the source searches with, or its nullable form (`trip.CountryId` for an `int?`); the sheet doesn't search while a field it depends on is empty. A source given no argument, or given one it doesn't take, makes the field keep its default editor, and a configuration problem is logged.
- A search that needs several values takes a record of them as its argument, such as `LookupSource<Hotel, int, HotelFilter>` with `trip => new HotelFilter(trip.CityId!.Value, trip.StarRating)`.

## Showing related values

A related record of a registered resource shows its title. Columns and display fields read through navigations; EF Core resources load them in the same query:

```csharp
columns.Add(booking => booking.Customer);   // header "Customer", the customer's title

view.Fields(fields =>
{
    fields.Add(booking => booking.Customer);
    fields.Add(booking => booking.Customer!.Email, field => field.Title = "Customer email");
});
```

- Sort a navigation column by one of the record's properties: `column.Sortable(booking => booking.Customer!.Name)`. On an EF Core resource, `Sortable()` without one is turned off, and a configuration problem is logged.
- A related entity whose type isn't a registered resource is left out, with a configuration problem logged, since it would show its type name. Show one of its properties instead (`booking => booking.Agent!.Name`), and set `Title`, because the default label is the last property's name.
- A title that reads through another navigation isn't loaded for a column or display field; keep it to the record's own properties.

A collection or many-to-many navigation shows as one badge per related record, named by its title:

```csharp
columns.Add(tour => tour.Destinations, column => column.UseDisplay<BadgeDisplay>());

view.Fields(fields => fields.Add(tour => tour.Destinations).UseDisplay<BadgeDisplay>());
```

Without `BadgeDisplay`, a collection of related records is left out and a configuration problem is logged. For a related type that isn't a resource, `UseText<TItem>` picks each badge's text; the type argument is the related entity, and with a mismatch the column or field is left out: `badge.UseText<Destination>(destination => destination.Name)`.

For one line of text instead of badges, use a computed column or field such as `columns.Add(tour => string.Join(", ", tour.Destinations.Select(destination => destination.Name)), column => column.Title = "Destinations")` ([columns](columns.md#computed-values), [fields](fields.md#computed-values)).

EF Core resources load the collection in the same query as the page or record. A collection column can't be `Sortable()` unless it sorts by an expression, such as `Sortable(tour => tour.Destinations.Count)`. To load collections in separate queries, configure the context with `UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)`. For a long list of related records (a customer's bookings), use row details or a view page override instead ([customization](customization.md)).
