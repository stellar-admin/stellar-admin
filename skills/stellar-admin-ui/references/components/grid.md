---
component: Grid
tags: [sa-grid, sa-grid-item]
generated: true
---

# Grid

Lays out fields and other content in responsive columns.

<!-- structure:begin -->
## When to use

Use `<sa-grid>` to lay out a form's fields in columns; it replaces hand-written `grid` divs and stacks in narrow containers. The columns respond to the grid's container width, not the viewport. `columns` sets the count from 40rem; the grid has one column below that unless `columns-base` or `columns-sm` is set. `columns-base`, `columns-sm`, `columns-md` and `columns-lg` set the count at the smallest width, 30rem, 40rem and 56rem, and an unset width inherits the next smaller one. Set either `columns` or `columns-md`, not both. Counts are 1 to 12.

## Spans and starts

Fields carry their own span: `<sa-field>`, every input that renders its own field (`<sa-input asp-for>`, `<sa-select>`, `<sa-textarea>`, …, also with `render-field="false"`) and `<sa-display-field>` take `column-span`, `column-span-sm`, `column-span-md` and `column-span-lg`. Each is a number from 1 to 12 or `full`, inherits upward like the column counts, and is limited to the column count at that width. Placed directly in `<sa-grid>`, each such field is wrapped in a grid item that carries its span; nested deeper (inside a section or an `<sa-field>`), it is not.

The same elements take `column-start`, `column-start-sm`, `column-start-md` and `column-start-lg`: the column the item starts at, from 1 to 12, or `auto` to follow the previous item (the default). Use `column-start="1"` to begin a new row, or a higher start to leave a gap; the next item continues after it, so source and tab order are kept. Starts inherit upward like spans, `auto` resets a width, and a start is ignored at a width where the grid has one column or the item would extend past the last column (the span is never shrunk for it).

Wrap anything else, such as a [`<sa-form-section>`](form-section.md) or [`<sa-details>`](details.md), in `<sa-grid-item>` to give it a span or a start. `<sa-grid-item>` must be a direct child of `<sa-grid>`. An invalid span or start value, or an `<sa-grid-item>` outside `<sa-grid>`, throws when the page renders.

```razor
<sa-grid columns="3" columns-sm="2">
    <sa-input asp-for="FirstName" />
    <sa-input asp-for="Email" column-span="2" />
    <sa-display-field asp-for="Reference" column-start="1" />
    <sa-grid-item column-span="full" column-span-lg="2">
        <sa-details columns="2" columns-sm="2">...</sa-details>
    </sa-grid-item>
</sa-grid>
```

A `<sa-details>` nested in a grid item measures its own width, so set `columns-sm` when it sits in a narrow column.

<!-- structure:end -->

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-grid>` | Lays out fields and other content in responsive columns. |
| `<sa-grid-item>` | Places content, such as a form section or a details list, in an `<sa-grid>` with its own column span and start. |

## Attributes

### `<sa-grid>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `columns` | `int` | — | — |
| `columns-base` | `int` | — | — |
| `columns-lg` | `int` | — | — |
| `columns-md` | `int` | — | — |
| `columns-sm` | `int` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-grid-item>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `column-span` | `string` | — | — |
| `column-span-lg` | `string` | — | — |
| `column-span-md` | `string` | — | — |
| `column-span-sm` | `string` | — | — |
| `column-start` | `string` | `auto` | — |
| `column-start-lg` | `string` | — | — |
| `column-start-md` | `string` | — | — |
| `column-start-sm` | `string` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/Grid/_Intro.cshtml`*

```razor
<sa-grid columns="2">
    <sa-input asp-for="FirstName" />
    <sa-input asp-for="LastName" />
    <sa-input asp-for="Email" column-span="full" description="We send the itinerary here." />
    <sa-input asp-for="Phone" />
    <sa-input asp-for="Departure" />
</sa-grid>
```

*From `Pages/Grid/_Spans.cshtml`*

```razor
<sa-grid columns="3" columns-sm="2" columns-lg="4">
    <sa-input asp-for="FirstName" />
    <sa-input asp-for="LastName" />
    <sa-input asp-for="Email" column-span="2" />
    <sa-input asp-for="Departure" />
    <sa-input asp-for="Return" />
    <sa-select asp-for="Cabin" column-span-lg="2">
        <option value="economy">Economy</option>
        <option value="premium-economy">Premium Economy</option>
        <option value="business">Business</option>
    </sa-select>
    <sa-textarea asp-for="Notes" rows="3" column-span="full" />
</sa-grid>
```

*From `Pages/Grid/_Starts.cshtml`*

```razor
<sa-grid columns="3">
    <sa-input asp-for="FirstName" />
    <sa-input asp-for="LastName" />
    <sa-input asp-for="Departure" column-start="1" />
    <sa-input asp-for="Return" />
    <sa-input asp-for="Phone" column-start="2" column-span="2" />
</sa-grid>
```

*From `Pages/Grid/_DisplayFields.cshtml`*

```razor
<sa-grid columns="3">
    <sa-display-field asp-for="Reference" />
    <sa-input asp-for="FirstName" />
    <sa-input asp-for="LastName" />
    <sa-display-field asp-for="Status">
        <sa-badge>@Model.Status</sa-badge>
    </sa-display-field>
    <sa-input asp-for="Email" column-span="2" />
    <sa-field column-span="full">
        <sa-field-label for="grid-display-note">Note for the traveller</sa-field-label>
        <sa-input id="grid-display-note" name="TravellerNote" />
        <sa-field-description>Shown on the itinerary.</sa-field-description>
    </sa-field>
</sa-grid>
```

*From `Pages/Grid/_GridItems.cshtml`*

```razor
<sa-grid columns="3">
    <sa-input asp-for="FirstName" />
    <sa-input asp-for="LastName" />
    <sa-input asp-for="Phone" />
    <sa-input asp-for="SendConfirmation" column-span="full" />
    <sa-grid-item column-span="full" column-span-lg="2">
        <sa-form-section title="Audit" description="Maintained automatically." layout="FormSectionLayout.Stacked">
            <sa-details columns="2" columns-sm="2">
                <sa-display-field asp-for="CreatedAt" secondary="by @Model.CreatedBy" />
                <sa-display-field asp-for="UpdatedAt" secondary="by @Model.UpdatedBy" />
            </sa-details>
        </sa-form-section>
    </sa-grid-item>
    <sa-grid-item column-span="full" column-span-lg="1">
        <sa-form-section title="Payment" layout="FormSectionLayout.Stacked">
            <sa-details orientation="DetailsOrientation.Horizontal">
                <sa-display-field asp-for="Total" />
                <sa-display-field asp-for="Paid" />
                <sa-display-field asp-for="Balance" />
            </sa-details>
        </sa-form-section>
    </sa-grid-item>
</sa-grid>
```
