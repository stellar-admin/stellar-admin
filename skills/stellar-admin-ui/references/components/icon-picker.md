---
component: IconPicker
tags: [sa-icon-picker]
generated: true
---

# IconPicker

Renders a control for choosing an icon, which posts the icon's name.

<!-- structure:begin -->
## Composition and behavior

`<sa-icon-picker>` edits a string that holds an icon's name, and posts it from a hidden input named by `asp-for` or `name`. The icons come either from child `<sa-icon name="...">` elements (registered icons, rendered on the server) or from `icons-url`, never both and never neither; either mistake throws. The current icon is rendered on the server, so the value shows before anything loads.

The default `Picker` layout is a select-style trigger that opens a popover with a search box, a grid of icons and a status line. Search matches names and keywords. Keyboard: ↓ from the search box into the grid, arrow keys to move, Enter to choose, ↑ from the first row back to the search box, Escape to clear the search and then close. An optional picker (no `required`, or a bound property without `[Required]`) offers Clear. `layout="IconPickerLayout.Inline"` shows every icon as a tile in a single-select toggle group instead; it needs child icons and throws with `icons-url`. Keep inline pickers to a couple of dozen icons.

`icons-url` is fetched once, on first open, with a GET and no query string, and the picker searches and scrolls the whole set in the browser. It must answer with JSON:

```json
{ "icons": [{ "name": "wifi", "svg": "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 24 24\">...</svg>", "keywords": ["internet", "wireless"] }] }
```

`name` and `svg` are required and `keywords` is optional. `svg` is complete markup with an `<svg>` root, so icons can come from any source; the picker drops anything else and strips scripts and event attributes. A failed request shows an error in the popover and retries on the next open. Cache the response, with a version in the URL when the set can change.

A stored value that isn't one of the icons still shows and posts unchanged, so validate a newly chosen name on the server. Set `disabled` to show the value without letting the user change it; a disabled picker doesn't post, like a native control. `aria-label` and `aria-describedby` go on the trigger (inline: on the toggle group); use `wrap-in-field="false"` in an editing table row.
<!-- structure:end -->

## Attributes

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `disabled` | `bool` | `false` | `true`, `false` |
| `form` | `string` | — | — |
| `icons-url` | `string` | — | — |
| `layout` | `IconPickerLayout` | `Picker` | `Picker`, `Inline` |
| `placeholder` | `string` | — | — |
| `required` | `bool` | — | `true`, `false` |
| `value` | `string` | — | — |
| `class-names` | `IconPickerClassNames` | — | — |
| `column-span` | `string` | `1` | — |
| `column-span-lg` | `string` | — | — |
| `column-span-md` | `string` | — | — |
| `column-span-sm` | `string` | — | — |
| `column-start` | `string` | `auto` | — |
| `column-start-lg` | `string` | — | — |
| `column-start-md` | `string` | — | — |
| `column-start-sm` | `string` | — | — |
| `description` | `string` | — | — |
| `error` | `string` | — | — |
| `asp-for` | `ModelExpression` | — | — |
| `label` | `string` | — | — |
| `name` | `string` | — | — |
| `wrap-in-field` | `bool` | — | `true`, `false` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

## Examples

*From `Pages/IconPicker/_Intro.cshtml`*

```razor
<sa-icon-picker name="AmenityIcon" value="waves-ladder" label="Amenity icon"
                description="Shown beside the amenity on the hotel page.">
    <sa-icon name="wifi" />
    <sa-icon name="waves-ladder" />
    <sa-icon name="coffee" />
    <sa-icon name="utensils" />
    <sa-icon name="wine" />
    <sa-icon name="square-parking" />
    <sa-icon name="dumbbell" />
    <sa-icon name="bath" />
    <sa-icon name="snowflake" />
    <sa-icon name="tv" />
    <sa-icon name="dog" />
    <sa-icon name="baby" />
    <sa-icon name="bus" />
    <sa-icon name="plane-takeoff" />
    <sa-icon name="bike" />
    <sa-icon name="luggage" />
</sa-icon-picker>
```

*From `Pages/IconPicker/_Inline.cshtml`*

```razor
<sa-icon-picker name="TripStyleIcon" value="sailboat" layout="IconPickerLayout.Inline" label="Trip style icon">
    <sa-icon name="sailboat" />
    <sa-icon name="mountain" />
    <sa-icon name="tent" />
    <sa-icon name="tree-palm" />
    <sa-icon name="camera" />
    <sa-icon name="map" />
</sa-icon-picker>
```

*From `Pages/IconPicker/_IconsUrl.cshtml`*

```razor
<sa-icon-picker name="CategoryIcon" value="compass" icons-url="/IconPicker?handler=Icons" label="Category icon"
                description="Choose from every registered icon." />
```

*From `Pages/IconPicker/_ModelBinding.cshtml`*

```razor
<sa-icon-picker asp-for="Icon">
    <sa-icon name="mountain" />
    <sa-icon name="sailboat" />
    <sa-icon name="tent" />
    <sa-icon name="tree-palm" />
    <sa-icon name="bike" />
    <sa-icon name="camera" />
</sa-icon-picker>
```
