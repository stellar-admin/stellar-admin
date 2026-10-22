# View pages

`resource.AllowView(view => ...)` adds a read-only detail page at `/{slug}/view/{id}` and a View (eye icon) action on each index row.

```csharp
resource.AllowView(view =>
{
    view.Title = "Booking";
    view.SectionLayout = FormSectionLayout.Split;
    view.Fields(fields =>
    {
        fields.Columns(2);
        fields.AddSection("Trip", section =>
        {
            section.Add(booking => booking.Reference);
            section.Add(booking => booking.Destination);
            section.Add(booking => booking.DepartureDate);
            section.Add(booking => booking.Cabin);
            section.Add(booking => booking.Travellers);
            section.Add(booking => booking.Status, field => field.UseDisplay<BookingStatusDisplay>());
            section.Add(booking => booking.Total, field => field.UseDisplay<NumberDisplay>(number => number.Format = "C"));
        });
        fields.AddSection("Customer", section =>
        {
            section.Add(booking => booking.Customer);   // the customer's title
            section.Add(booking => booking.Customer!.Email, field => field.Title = "Email");
            section.Add(booking => booking.Notes, field => field.EmptyText = "No notes");
        });
        fields.AddSection("Audit", section =>
        {
            section.Orientation = FieldOrientation.Horizontal;
            section.Add(booking => booking.CreatedAt);
        });
    });
});
```

- **Fields are required.** `AllowView()` without `view.Fields(...)` leaves the resource without a view page and logs a configuration problem ([configuration problems](configuration-problems.md)). Every property must be listed; there is no "all fields" option.
- Every field is a display field: `fields.Add` returns the display field builder (`Title`, `EmptyText`, `Orientation`, `UseDisplay`, `UseBuiltInRendering`, spans and starts). Rendering and displays are in [fields](fields.md).
- Layout uses the same `Columns`, `AddSection`, `AddGroup`, spans and starts as forms ([forms](forms.md#layout-columns-sections-and-groups)). Sections take `Description`, `Layout`, `Orientation`, `Columns`, `ColumnSpan` and `ColumnStart`; a section's `Orientation` applies to the fields inside it. View pages have no `AddDetails`; use sections.
- References (`booking.Customer!.Email`) are loaded with the record for EF Core resources. A related record of a registered resource (`booking => booking.Customer`) shows its title, and a collection of them shows in badges with `UseDisplay<BadgeDisplay>()` ([relationships](relationships.md#showing-related-values)).
- A custom display used here needs `DisplayTemplates/{TemplateName}.cshtml` (index cells use `GridDisplayTemplates/`).
- The header shows Back, Delete (with `AllowDelete`) and Edit (with `AllowEdit`).
- `view.Title` defaults to `View {singular label}`; the row action label is `index.ViewLabel`.
- Custom data sources need `IResourceViewHandler<T>` (`FindAsync`); `IResourceEditHandler<T>` includes it.
- `AllowView<TModel, THandler>(...)` shows a different model, loaded by `THandler : IResourceViewHandler<TModel>`.

To add content to the page (related records, actions), override it ([customization](customization.md#add-content-to-a-generated-page)); `sa-view-page` has `pre-page-actions`, `post-page-actions`, `pre-view-fields` and `post-view-fields` slots.
