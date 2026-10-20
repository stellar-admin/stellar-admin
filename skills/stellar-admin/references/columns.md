# Columns

Index columns are configured in `index.Columns(columns => ...)`. They start empty: only the columns you add are shown, in the order added.

## Contents

- [Adding columns](#adding-columns)
- [Values through references](#values-through-references)
- [How a cell renders](#how-a-cell-renders)
- [Built-in displays in cells](#built-in-displays-in-cells)
- [A badge styled by value](#a-badge-styled-by-value)
- [Empty values](#empty-values)

## Adding columns

```csharp
index.Columns(columns =>
{
    columns.Add(booking => booking.Reference);
    columns.Add(booking => booking.DepartureDate, column =>
    {
        column.Title = "Departs";
        column.Sortable();
    });
    columns.Add(booking => booking.Total, column => column.UseDisplay<NumberDisplay>(number => number.Format = "C"));
    columns.Add(booking => booking.Notes, column => column.EmptyText = "No notes");
});
```

| Column builder member | Effect |
| --- | --- |
| `Title` | Header text. Defaults to `[Display(Name)]`, else the humanized property name. |
| `EmptyText` | Text for null or empty values. |
| `Sortable()` / `Sortable(selector)` | Opts into sorting by the column's value, or by another expression ([index-pages](index-pages.md#sorting)). |
| `UseDisplay<TDisplay>()` / `UseDisplay<TDisplay>(display => ...)` | Renders cells with a display and its settings. |
| `UseBuiltInRendering()` | Renders the value as plain text, formatted with `[DisplayFormat]`. |

`columns.Add(selector)` without a callback returns the column builder, so `columns.Add(b => b.Total).Sortable();` works too. `columns.Clear()` removes earlier columns.

## Values through references

A column can read through navigation properties. EF Core resources load the references on each column's path in the same query; no `Include` is needed.

```csharp
columns.Add(booking => booking.Customer!.Name, column => column.Title = "Customer");
```

Use the null-forgiving `!` on nullable navigations; a null reference shows the empty text. Set a `Title`, because the default comes from the last property (`Name`). Sorting by such a column works in EF Core (`column.Sortable()`).

A column on a collection or many-to-many navigation (`tour => tour.Destinations`) is loaded with the page too. Show it with `BadgeDisplay` and `UseText<TItem>`, since without it each badge shows the entity's type name; it can't be `Sortable()` without a sort expression ([relationships](relationships.md#showing-related-values)).

Selecting a foreign key (`booking => booking.CustomerId`) shows the number; select the navigation's property instead.

## How a cell renders

In order of precedence:

1. The display set with `UseDisplay<TDisplay>(...)`.
2. Plain text after `UseBuiltInRendering()`.
3. The grid template named by the property's `[UIHint("Name")]`: the view `GridDisplayTemplates/Name.cshtml`.
4. The default display for the property's data type, then its CLR type (table below).
5. Plain text formatted with `[DisplayFormat]`.

A cell's template is always `GridDisplayTemplates/{TemplateName}.cshtml`, never `DisplayTemplates/`. MVC finds it in `Areas/StellarAdmin/Views/Shared/`, `Views/Shared/` or `Pages/Shared/`.

Displays with a handler (`IFieldDisplay<THandler>`) throw in columns; use them on forms and view pages only.

## Built-in displays in cells

All in `StellarAdmin.Resources.Displays`. The same displays are used by display fields ([fields](fields.md#built-in-displays)).

| Display | Default for | Settings |
| --- | --- | --- |
| `TextDisplay` | Strings | `LineClamp` (in a cell, line breaks show only when it is set) |
| `NumberDisplay` | Numeric types; right-aligned in cells | `Format` (`"C"`, `"N2"`, `"0.0"`), else `[DisplayFormat]` |
| `DateDisplay` | `DateTime`, `DateTimeOffset`, `DateOnly`, `TimeOnly` | `Format` (`"d MMM yyyy"`), else `[DisplayFormat]`, else `d MMM yyyy` / `HH:mm` / `d MMM yyyy, HH:mm`. `[DataType(DataType.Date)]` brings MVC's short date format (`10/14/2026` in en-US), so set `Format` for those |
| `BooleanDisplay` | `bool` | `Appearance` (`BooleanDisplayAppearance.Icon`, `Text`, `IconAndText`), `TrueText`, `FalseText` |
| `EnumDisplay` | Non-flags enums | Uses each member's `[Display(Name)]` |
| `EmailDisplay` | `[EmailAddress]` | A `mailto:` link |
| `PhoneDisplay` | `[Phone]` | A `tel:` link |
| `LinkDisplay` | `[Url]` | `OpenInNewTab` (default `true`) |
| `BadgeDisplay` | Never a default | `Variant` (`BadgeVariant` from `StellarAdmin.UI`, default `Secondary`), the same for every value; collections show one badge per item. `UseText<TItem>(item => text)` sets each badge's text, such as a related entity's name ([relationships](relationships.md#showing-related-values)) |
| `ImageDisplay` | Never a default | A thumbnail from a stored file name or URL: `ResolveUrl(value => url)` or `ResolveUrl(async (value, services, ct) => url)` (without it the value is the URL), `Size` (`ImageDisplaySize`, default `Small`, 32px), `AspectRatio` (`ImageAspectRatio` from `StellarAdmin.UI`, default `Square`), `Fit` (`ImageFit.Cover`, `Contain`), `Interaction` (`ImageDisplayInteraction.Lightbox`, `Preview`, `None`). An empty value shows a placeholder tile, an image that fails to load a broken-image tile |
| `IconDisplay` | Never a default | The icon a stored name refers to, with the name on hover; `ShowName = true` shows it beside the icon. A name that isn't a registered icon shows a dashed box |

Formats use the request's culture. `"C"` therefore shows the server culture's currency unless the app sets a request culture.

```csharp
columns.Add(booking => booking.Cabin, column => column.UseDisplay<BadgeDisplay>(badge => badge.Variant = BadgeVariant.Outline));
columns.Add(customer => customer.IsActive, column => column.UseDisplay<BooleanDisplay>(flag => flag.Appearance = BooleanDisplayAppearance.Text));
columns.Add(destination => destination.Photo, column => column.UseDisplay<ImageDisplay>(image => image.ResolveUrl(value => $"https://cdn.voyager.example/photos/{value}")));
```

## A badge styled by value

`BadgeDisplay` applies one variant to every value. To pick the variant per value, write a display class and its grid template.

```csharp
// Admin/BookingStatusDisplay.cs
using StellarAdmin.Resources.Displays;

namespace Voyager.Admin;

public sealed class BookingStatusDisplay : FieldDisplay
{
    public override string TemplateName => "BookingStatus";
}
```

```razor
@* Areas/StellarAdmin/Views/Shared/GridDisplayTemplates/BookingStatus.cshtml *@
@using StellarAdmin.Areas.StellarAdmin
@using StellarAdmin.UI
@using Voyager.Admin
@using Voyager.Models
@inherits FieldDisplayView<BookingStatusDisplay>
@{
    var variant = Model switch
    {
        BookingStatus.Confirmed => BadgeVariant.Default,
        BookingStatus.Cancelled => BadgeVariant.Destructive,
        _ => BadgeVariant.Secondary,
    };
}
<sa-badge variant="@variant">@FormatValue()</sa-badge>
```

```csharp
columns.Add(booking => booking.Status, column => column.UseDisplay<BookingStatusDisplay>());
```

- The template needs `@using` for the display class's namespace (`Voyager.Admin` here) and for `StellarAdmin.Areas.StellarAdmin`, where `FieldDisplayView<TDisplay>` lives.
- The template's `Model` is the cell value (as `object?`), `Display` is the configured display, and `FormatValue()` formats the value (enum members by their `[Display(Name)]`).
- The template needs the `StellarAdmin.UI` tag helpers: put it under `Areas/StellarAdmin/Views/` with an area `_ViewImports.cshtml` ([customization](customization.md#view-imports)), or add `@addTagHelper *, StellarAdmin.UI` to it.
- To show the same badge on a view page or as a form display field, add the same markup as `DisplayTemplates/BookingStatus.cshtml`; display fields use `DisplayTemplates/`.
- Alternative: mark the property `[UIHint("BookingStatus")]` and write `GridDisplayTemplates/BookingStatus.cshtml` with `@model BookingStatus`. The hint then applies wherever the property is displayed, and display fields require a `DisplayTemplates/BookingStatus.cshtml` too (a missing one throws).

## Empty values

A null or empty value shows the column's `EmptyText`, else `[DisplayFormat(NullDisplayText = "...")]`, else an empty cell. No display or template runs for it, unless the display renders empty values itself: `ImageDisplay` shows its placeholder tile, and a custom display can do the same by overriding `RendersEmptyValue` ([fields](fields.md#custom-displays)).
