---
component: Details
tags: [sa-details, sa-display-field]
generated: true
---

# Details

Lays out read-only display fields in a description list.

<!-- structure:begin -->
## When to use

Use `<sa-details>` to show a record's values read-only: a view page, an audit section on an edit page, or the expanded panel of a [table](table.md) row detail. Inside `<sa-details>` each field renders a `<dt>`/`<dd>` pair in a `<dl>`. A `<sa-display-field>` can also sit among form inputs (for example in a [`<sa-grid>`](grid.md)); outside `<sa-details>` it renders `<div>`s and takes an input's label style and a control's height so the row lines up. Its column spans apply only directly in `<sa-details>` or `<sa-grid>`.

## Values

Bind a field with `asp-for`; the label comes from `[Display(Name)]` unless `label` is set. Either `label` or `asp-for` is required. The value is chosen in this order:

1. Child content replaces the value (a field with `label` and child content needs no `asp-for`).
2. An empty value (null, a blank string, or an empty collection) shows `empty-text`, else `[DisplayFormat(NullDisplayText)]`, else a dash, in a muted color. Zero and `false` are values.
3. A display template named by `template` or `[UIHint]`, found like a partial view at `DisplayTemplates/{name}` with the value as its model. Templates are used by name only, never by type (MVC's built-in `bool` checkbox template never applies); a missing template throws.
4. Built-in formatting: `format` or `[DisplayFormat(DataFormatString)]`; enums by their member's `[Display]` name; a `bool` as a check or cross icon with screen-reader "Yes"/"No"; collections joined with commas; `[EmailAddress]` as a `mailto:` link; `[Url]` linked only for `http`/`https`; `[DataType(DataType.MultilineText)]` keeps line breaks. `href` links the value to any URL.

`secondary` appends muted text after a non-empty value, such as "by Amelia Hart" after an updated date.

## Columns and spans

Columns and spans work as in [`<sa-grid>`](grid.md). The columns respond to the list's container width, not the viewport. `columns` sets the count from 40rem; the list has one column below that unless `columns-base` or `columns-sm` is set. `columns-base`, `columns-sm`, `columns-md` and `columns-lg` set the count at the smallest width, 30rem, 40rem and 56rem, and an unset width inherits the next smaller one. Set either `columns` or `columns-md`, not both. Counts are 1 to 12.

`column-span`, `column-span-sm`, `column-span-md` and `column-span-lg` set a field's span the same way: a number from 1 to 12 or `full` for every column (for example `column-span="full" column-span-lg="2"`). Each span is limited to the column count at that width; an invalid value throws when the page renders.

`orientation` on `<sa-details>` (`Vertical` or `Horizontal`) applies to every field unless a field sets its own. `variant="DetailsVariant.Divided"` separates the fields with rules, which suits a short horizontal list such as audit information.

The same breakpoints and inline grid variables are available from the public `ResponsiveGrid` helper (`ResolveColumns`, `ResolveSpan`, `ColumnsStyle`, `SpanStyle`, `RowsStyle`) for a custom grid.

<!-- structure:end -->

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-details>` | Lays out read-only display fields in a description list. |
| `<sa-display-field>` | Displays a labelled read-only value. The value comes from child content, a display template, or the model property selected by `asp-for`. |

## Attributes

### `<sa-details>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `columns` | `int` | — | — |
| `columns-base` | `int` | — | — |
| `columns-lg` | `int` | — | — |
| `columns-md` | `int` | — | — |
| `columns-sm` | `int` | — | — |
| `orientation` | `FieldOrientation` | `Vertical` | `Vertical`, `Horizontal` |
| `variant` | `DetailsVariant` | `Default` | `Default`, `Divided` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

### `<sa-display-field>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `column-span` | `string` | — | — |
| `column-span-lg` | `string` | — | — |
| `column-span-md` | `string` | — | — |
| `column-span-sm` | `string` | — | — |
| `empty-text` | `string` | — | — |
| `asp-for` | `ModelExpression` | — | — |
| `format` | `string` | — | — |
| `href` | `string` | — | — |
| `label` | `string` | — | — |
| `orientation` | `FieldOrientation` | `Vertical` | `Vertical`, `Horizontal` |
| `secondary` | `string` | — | — |
| `template` | `string` | `[UIHint]` | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/Details/_Intro.cshtml`*

```razor
<sa-details columns="2">
    <sa-display-field asp-for="Reference" />
    <sa-display-field asp-for="Status" />
    <sa-display-field asp-for="LeadTraveller" />
    <sa-display-field asp-for="Email" />
    <sa-display-field asp-for="Destination" href="#destination-@Model.DestinationId" />
    <sa-display-field asp-for="Departure" />
    <sa-display-field asp-for="Party" />
    <sa-display-field asp-for="Total" />
    <sa-display-field asp-for="PromoCode" />
</sa-details>
```

*From `Pages/Details/_Columns.cshtml`*

```razor
<sa-details columns="3" columns-sm="2" columns-lg="4">
    <sa-display-field asp-for="Reference" />
    <sa-display-field asp-for="LeadTraveller" />
    <sa-display-field asp-for="Email" column-span="2" />
    <sa-display-field asp-for="Departure" />
    <sa-display-field asp-for="Return" />
    <sa-display-field asp-for="Cabin" />
    <sa-display-field asp-for="Party" />
    <sa-display-field asp-for="Notes" column-span="full" column-span-lg="3" />
    <sa-display-field asp-for="Total" />
</sa-details>
```

*From `Pages/Details/_EmptyValues.cshtml`*

```razor
<sa-details columns="2">
    @* A dash by default *@
    <sa-display-field asp-for="PromoCode" />
    @* [DisplayFormat(NullDisplayText = "None")] *@
    <sa-display-field asp-for="DietaryNeeds" />
    <sa-display-field asp-for="ArchivedAt" empty-text="Not archived" />
    @* Zero and false are values *@
    <sa-display-field asp-for="Infants" />
</sa-details>
```

*From `Pages/Details/_Templates.cshtml`*

```razor
<sa-details columns="3">
    @* [UIHint("BookingStatus")] renders DisplayTemplates/BookingStatus *@
    <sa-display-field asp-for="Status" />
    @* A named template *@
    <sa-display-field asp-for="Reference" template="Reference" />
    @* Muted text after the value *@
    <sa-display-field asp-for="UpdatedAt" secondary="by @Model.UpdatedBy" />
    @* Child content replaces the value *@
    <sa-display-field asp-for="Interests">
        @foreach (var interest in Model.Interests)
        {
            <sa-badge variant="BadgeVariant.Secondary">@interest</sa-badge>
        }
    </sa-display-field>
    <sa-display-field asp-for="TravelInsurance">
        @(Model.TravelInsurance ? "Included" : "Not included")
    </sa-display-field>
    <sa-display-field label="Duration">7 nights</sa-display-field>
</sa-details>
```

*From `Pages/Details/_InForm.cshtml`*

```razor
<form>
    <sa-form-section title="Booking" description="Who is travelling and where.">
        <sa-grid columns="2">
            <sa-display-field asp-for="Reference" />
            <sa-input asp-for="LeadTraveller" />
            <sa-input asp-for="Email" />
            <sa-display-field asp-for="Status" />
            <sa-textarea asp-for="Notes" rows="3" column-span="full" />
        </sa-grid>
    </sa-form-section>
    <sa-form-section title="Audit" description="Maintained automatically.">
        <sa-details columns="2">
            <sa-display-field asp-for="CreatedAt" secondary="by @Model.CreatedBy" />
            <sa-display-field asp-for="UpdatedAt" secondary="by @Model.UpdatedBy" />
            <sa-display-field asp-for="Version" />
            <sa-display-field asp-for="ArchivedAt" />
        </sa-details>
    </sa-form-section>
</form>
```

*From `Pages/Details/_RecordPage.cshtml`*

```razor
<sa-page-header class="mb-8">
    <sa-page-header-title>@Model.Reference</sa-page-header-title>
    <sa-page-header-description>@Model.Destination · @Model.Departure.ToString("d MMM") – @Model.Return.ToString("d MMM yyyy")</sa-page-header-description>
    <sa-page-header-actions>
        <sa-button variant="ButtonVariant.Outline">Cancel booking</sa-button>
        <sa-button>Edit</sa-button>
    </sa-page-header-actions>
</sa-page-header>

<sa-form-section title="Booking" description="Who is travelling and where.">
    <sa-details columns="2">
        <sa-display-field asp-for="Reference" />
        <sa-display-field asp-for="Status" />
        <sa-display-field asp-for="LeadTraveller" />
        <sa-display-field asp-for="Email" />
        <sa-display-field asp-for="Destination" href="#destination-@Model.DestinationId" />
        <sa-display-field asp-for="Departure" />
        <sa-display-field asp-for="Party" />
        <sa-display-field asp-for="Total" />
        <sa-display-field asp-for="PromoCode" />
    </sa-details>
</sa-form-section>
<sa-form-section title="Notes">
    <sa-details>
        <sa-display-field asp-for="Notes" />
    </sa-details>
</sa-form-section>
<sa-form-section title="Audit" description="Maintained automatically.">
    <sa-details variant="DetailsVariant.Divided" orientation="FieldOrientation.Horizontal">
        <sa-display-field asp-for="CreatedAt" secondary="by @Model.CreatedBy" />
        <sa-display-field asp-for="UpdatedAt" secondary="by @Model.UpdatedBy" />
        <sa-display-field asp-for="Version" />
        <sa-display-field asp-for="ArchivedAt" />
    </sa-details>
</sa-form-section>
```
