# Editable fields and display fields

Forms have two kinds of field. **Editable fields** (`Add`) render an input and are bound on save. **Display fields** (`AddDisplayField`, `AddDetails`) show a value read-only and are never bound. View pages use only display fields ([view-pages](view-pages.md)).

## Contents

- [Read-only values on a form](#read-only-values-on-a-form)
- [Computed values](#computed-values)
- [Details sections](#details-sections)
- [Read-only through metadata](#read-only-through-metadata)
- [How a display field renders](#how-a-display-field-renders)
- [Built-in displays](#built-in-displays)
- [Display templates](#display-templates)
- [Custom displays](#custom-displays)

## Read-only values on a form

There is no `ReadOnly()` method on the field builder. Show a value that must not change as a display field:

```csharp
resource.AllowEdit(edit => edit.Fields(fields =>
{
    fields.AddSection("Booking", section =>
    {
        section.Columns(3);
        section.AddDisplayField(booking => booking.Reference);
        section.AddDisplayField(booking => booking.CreatedAt, field => field.Title = "Created");
        section.Add(booking => booking.Status);
    });
    fields.AddDisplayField(booking => booking.Customer);
    fields.AddDisplayField(booking => booking.Total).UseDisplay<NumberDisplay>(number => number.Format = "C");
}));
```

- `AddDisplayField` works wherever `Add` does: the form, a section or a group, and it takes the same `ColumnSpan`, `ColumnSpanFull` and `ColumnStart`.
- Display field builder: `Title`, `EmptyText`, `Orientation` (`DisplayFieldOrientation.Vertical` or `Horizontal`), `UseDisplay<TDisplay>(...)`, `UseBuiltInRendering()`.
- Display fields can read through references (`booking => booking.Customer!.Email`). A related record of a registered resource (`booking => booking.Customer`) shows its title. EF Core resources load them with the record; custom data sources and models must load them.
- The property needs only a getter. A posted value is ignored.
- On create, values come from the new model (the factory's values), so unset values show the empty text.
- With a custom model (`AllowEdit<TModel, THandler>`), the property must be on `TModel`.

## Computed values

A display field, on a form, in details or on a view page, can show any expression of the record, not just a property:

```csharp
fields.AddDisplayField(booking => booking.Nights * booking.NightlyRate, field =>
{
    field.Title = "Total";
    field.UseDisplay<NumberDisplay>(number => number.Format = "C");
});
view.Fields(fields => fields.Add(tour => string.Join(", ", tour.Destinations.Select(destination => destination.Name)), field => field.Title = "Destinations"));
```

- A computed field has no property for a default label, so `Title` is required; without it the field is left out ([configuration problems](configuration-problems.md)).
- It renders like a property of the expression's type, with that type's default display; `[UIHint]` and `[DisplayFormat]` don't apply. A display handler receives the computed value as `context.Value` and the title in PascalCase (`Total`) as `context.FieldName`.
- EF Core resources load the navigations it reads from the record, as for computed columns ([columns](columns.md#computed-values)), but not those read through a nested lambda's parameter.
- Reading a property of a null value, or calling one of its methods, gives null (or zero or false), which shows the empty text; extension methods such as `Select` still throw on a null collection.

## Details sections

`AddDetails` adds a titled section that contains only display fields, laid out as a description list:

```csharp
fields.AddDetails("Audit", details =>
{
    details.Columns(2);
    details.Orientation = DetailsOrientation.Horizontal;
    details.Variant = DetailsVariant.Divided;
    details.Add(booking => booking.CreatedAt);
    details.Add(booking => booking.UpdatedAt, field => field.EmptyText = "Never");
});
```

Details take `Description`, `Layout`, `Columns`, `ColumnSpan`, `ColumnStart`, `Orientation` (`DetailsOrientation`) and `Variant` (`DetailsVariant.Default` or `Divided`). `DetailsOrientation`, `DisplayFieldOrientation`, `FieldOrientation`, `DetailsVariant` and `FormSectionLayout` are in `StellarAdmin.UI`. `AddDetails` exists on form field builders; view pages use `AddSection`.

## Read-only through metadata

A property marked `[ReadOnly(true)]` or `[Editable(false)]` renders its configured editor read-only when added with `Add`, and is not updated on save. Prefer display fields for values that are never edited: they need no attribute and format the value with a display.

## How a display field renders

In order of precedence:

1. The display set with `UseDisplay<TDisplay>(display => ...)`.
2. The display field's own text rendering after `UseBuiltInRendering()`.
3. The template named by `[UIHint("Name")]`: `DisplayTemplates/Name.cshtml`, with the value as its model. A missing template throws.
4. The default display for the property's data type, then its CLR type.
5. Text rendering, which joins other collections with commas and applies `[DisplayFormat]`. A related record of a registered resource shows its title, and a collection of them needs `BadgeDisplay`. Any other entity, collection of entities or class that doesn't override `ToString()` is left out with it or with no display, and a configuration problem is logged; show a property of the entity, or use `BadgeDisplay` with `UseText<TItem>` ([relationships](relationships.md#showing-related-values)).

An empty value shows `EmptyText`, else `[DisplayFormat(NullDisplayText)]`, else a dash, unless the display renders empty values itself (`ImageDisplay` shows "No image").

## Built-in displays

In `StellarAdmin.Resources.Displays`; the same displays render index cells ([columns](columns.md#built-in-displays-in-cells)).

| Display | Default for | Settings |
| --- | --- | --- |
| `TextDisplay` | Strings; multiline text keeps its line breaks | `LineClamp` |
| `HtmlDisplay` | `[DataType(DataType.Html)]` | The HTML, sanitized with `IRichTextSanitizer` and formatted as the rich text editor shows it ([editors](editors.md#rich-text)); `LineClamp` applies in index cells |
| `NumberDisplay` | Numeric types | `Format`, else `[DisplayFormat]` |
| `DateDisplay` | `DateTime`, `DateTimeOffset`, `DateOnly`, `TimeOnly` | `Format`, else `[DisplayFormat]`, else `d MMM yyyy`, `HH:mm` or `d MMM yyyy, HH:mm`. `[DataType(DataType.Date)]` brings MVC's short date format (`10/14/2026` in en-US), so set `Format` for those |
| `BooleanDisplay` | `bool` | `Appearance` (`BooleanDisplayAppearance.Icon`, `Text`, `IconAndText`), `TrueText`, `FalseText` |
| `EnumDisplay` | Non-flags enums | The member's `[Display(Name)]`, else its humanized name (`NorthAmerica` shows as "North America") |
| `EmailDisplay` | `[EmailAddress]` | `mailto:` link |
| `PhoneDisplay` | `[Phone]` | `tel:` link |
| `LinkDisplay` | `[Url]` | `OpenInNewTab` (default `true`) |
| `BadgeDisplay` | Lists of strings or numbers, and dictionaries the dictionary editor edits | `Variant` (`BadgeVariant`), the same for every value; collections show one badge per item, and dictionaries one per entry (`pt: Passeio por Lisboa`); `Limit` shows that many and the rest as a `+3` badge with them in its tooltip; long badges truncate. A related record of a registered resource shows its title, and `UseText<TItem>(item => text)` sets each badge's text for other types ([relationships](relationships.md#showing-related-values)) |
| `ImageDisplay` | Never a default | The image as a figure, from a stored file name or URL: `ResolveUrl(...)`, `Size` (maximum height, default `Medium`, 15rem), `AspectRatio` (default `Original`), `Fit`, `Interaction` (default `Lightbox`); see [columns](columns.md#built-in-displays-in-cells) |
| `IconDisplay` | Never a default | The icon a stored name refers to, with its name beside it; `ShowName = false` shows the name on hover only. A name that isn't a registered icon shows a dashed box |

## Display templates

To render a value differently everywhere it is displayed, mark the property `[UIHint("BookingStatus")]` and add `DisplayTemplates/BookingStatus.cshtml` (for display fields) and `GridDisplayTemplates/BookingStatus.cshtml` (for index cells), with `@model BookingStatus`. Put them in `Areas/StellarAdmin/Views/Shared/` (with the area `_ViewImports.cshtml`, see [customization](customization.md#view-imports)) or `Views/Shared/`.

To change a built-in display for the whole panel, add a view at its path, for example `Areas/StellarAdmin/Views/Shared/DisplayTemplates/Displays/Boolean.cshtml` (and `GridDisplayTemplates/Displays/Boolean.cshtml` for cells).

## Custom displays

A display class holds settings and names its template. Use one when a template needs settings, or to apply a template to some fields only rather than through `[UIHint]`.

```csharp
public sealed class StatusBadgeDisplay : FieldDisplay
{
    public override string TemplateName => "StatusBadge";
    public bool ShowIcon { get; set; }
}
```

```razor
@* Areas/StellarAdmin/Views/Shared/DisplayTemplates/StatusBadge.cshtml *@
@using StellarAdmin.Areas.StellarAdmin
@inherits FieldDisplayView<StatusBadgeDisplay>
<sa-badge variant="BadgeVariant.Outline">@if (Display.ShowIcon) { <sa-icon name="circle-check" /> } @FormatValue()</sa-badge>
```

```csharp
details.Add(booking => booking.Status, field => field.UseDisplay<StatusBadgeDisplay>(badge => badge.ShowIcon = true));
```

- The template inherits `FieldDisplayView<TDisplay>`: `Model` is the value (`object?`), `Display` the configured settings, `FormatValue(format)` the value formatted with a format string, else `[DisplayFormat]`, with enum members by their `[Display(Name)]`, else their humanized names.
- Display fields use `DisplayTemplates/{TemplateName}`; index columns use `GridDisplayTemplates/{TemplateName}`. Provide both if the display is used in both places.
- An empty value normally shows the field's empty text and skips the template. Override `public override bool RendersEmptyValue => true;` when the template shows something of its own for an empty value, such as initials for a missing avatar; the template then receives null and blank values.
- A display that needs request data (a lookup by an id that has no navigation) also implements `IFieldDisplay<THandler>`. The handler derives from `FieldDisplayHandler<TDisplay>`, takes services in its constructor, and returns data from `PrepareAsync(FieldDisplayContext context, CancellationToken cancellationToken)`; the template reads it as `DisplayData`. Displays with handlers work on forms and view pages, not in index columns.
