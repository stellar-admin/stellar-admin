# View pages

`resource.AllowView(view => ...)` adds a read-only detail page at `/{slug}/view/{id}` and a View (eye icon) action on each index row. Blocks above, beside or below the fields, such as related stats, are [widgets](widgets.md#view-create-and-edit-pages).

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
- The header shows Delete (with `AllowDelete`), Back and Edit (with `AllowEdit`).
- The page title is `view.Title` when set, else the record's title (`UseTitle` or the convention), else `View {singular label}` when the title would only be the key; the row action label is `index.ViewLabel`.
- Custom data sources need `IResourceViewHandler<T>` (`FindAsync`); `IResourceEditHandler<T>` includes it.
- `AllowView<TModel, THandler>(...)` shows a different model, loaded by `THandler : IResourceViewHandler<TModel>`.

To add content to the page (related records, actions), override it ([customization](customization.md#add-content-to-a-generated-page)); `sa-view-page` has `pre-page-actions`, `post-page-actions`, `pre-view-fields` and `post-view-fields` slots.

## Record summary

`resource.Summary(summary => ...)` shows a line of the record's key values under the title of the view and edit pages (not the create page). Items show in the order added, in one row that wraps.

```csharp
resource.Summary(summary =>
{
    summary.SeparateItems = true;                 // a "·" between every two items
    summary.Add(booking => booking.Reference);
    summary.Add(booking => booking.Customer);     // the customer's title
    summary.Add(booking => booking.Status, item => item.UseDisplay<BadgeDisplay>());
});

resource.Summary(summary =>
{
    summary.AddText("Total");                     // printed as given; no ":" is added
    summary.Add(booking => booking.Total, item => item.UseDisplay<NumberDisplay>(number => number.Format = "C"));
    summary.AddSeparator();                       // a separator at this point only
    summary.Add(booking => booking.DepartureDate);
});
```

- A field shows its value only, without a label; label it with `AddText(...)` before it. A summary of only text shows the same text for every record.
- A field renders its compact display, the same one index cells use (`GridDisplayTemplates/`): `UseDisplay<TDisplay>()`, else the property's `[UIHint]`, else the default display for the type. `UseBuiltInRendering()` formats with `[DisplayFormat]`. The item builder has no `Title`, `EmptyText` or spans.
- An empty value shows `[DisplayFormat(NullDisplayText = ...)]`, else a dash; the item stays.
- Use `SeparateItems` or `AddSeparator()`, not both: with `SeparateItems = true`, added separators are dropped and a configuration problem is reported. Separators are never inferred.
- `view.Summary(...)` and `edit.Summary(...)` replace the resource's summary on that page; `summary.Clear()` hides it. A page with its own model (`AllowView<TModel, THandler>`, `AllowEdit<TModel, THandler>`) doesn't get the resource's summary; give it its own with the page model's fields.
- The edit page summarizes the stored record, also after a save fails validation. EF Core resources load the references the summary reads.
