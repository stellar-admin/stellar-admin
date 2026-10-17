---
component: ToggleGroup
tags: [sa-toggle-group, sa-toggle-group-item]
generated: true
---

# ToggleGroup

Renders a group of toggle items that lets the user select one item or several.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-toggle-group>` | Renders a group of toggle items that lets the user select one item or several. |
| `<sa-toggle-group-item>` | Renders an item that can be selected in a toggle group. |

## Attributes

### `<sa-toggle-group>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `multiple` | `bool` | `false` | `true`, `false` |
| `variant` | `ToggleVariant` | `Default` | `Default`, `Outline` |
| `size` | `ToggleSize` | `Default` | `Default`, `Small`, `Large` |
| `orientation` | `ToggleGroupOrientation` | `Horizontal` | `Horizontal`, `Vertical` |
| `spacing` | `ToggleGroupSpacing` | `Default` | `Default`, `None`, `Small`, `Large` |
| `value` | `string` | — | — |
| `values` | `IEnumerable<string>` | — | — |
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

### `<sa-toggle-group-item>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `value` | `string` | — | — |
| `variant` | `ToggleVariant` | — | `Default`, `Outline` |
| `size` | `ToggleSize` | — | `Default`, `Small`, `Large` |
| `selected` | `bool` | — | `true`, `false` |
| `disabled` | `bool` | `false` | `true`, `false` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/ToggleGroup/_Intro.cshtml`*

```razor
<sa-toggle-group name="results-view" value="list">
    <sa-toggle-group-item value="list" aria-label="List view">
        <sa-icon name="list"/>
    </sa-toggle-group-item>
    <sa-toggle-group-item value="grid" aria-label="Grid view">
        <sa-icon name="layout-grid"/>
    </sa-toggle-group-item>
    <sa-toggle-group-item value="map" aria-label="Map view">
        <sa-icon name="map"/>
    </sa-toggle-group-item>
</sa-toggle-group>
```

*From `Pages/ToggleGroup/_ModelBinding.cshtml`*

```razor
<sa-toggle-group asp-for="ResultsView" spacing="ToggleGroupSpacing.None" variant="ToggleVariant.Outline">
    <sa-toggle-group-item value="list" aria-label="List view">
        <sa-icon name="list"/>
    </sa-toggle-group-item>
    <sa-toggle-group-item value="grid" aria-label="Grid view">
        <sa-icon name="layout-grid"/>
    </sa-toggle-group-item>
    <sa-toggle-group-item value="map" aria-label="Map view">
        <sa-icon name="map"/>
    </sa-toggle-group-item>
</sa-toggle-group>

<sa-toggle-group asp-for="Amenities" multiple="true" spacing="ToggleGroupSpacing.None" variant="ToggleVariant.Outline">
    <sa-toggle-group-item value="wifi" aria-label="Free Wi-Fi">
        <sa-icon name="wifi"/>
    </sa-toggle-group-item>
    <sa-toggle-group-item value="pool" aria-label="Pool">
        <sa-icon name="waves-horizontal"/>
    </sa-toggle-group-item>
    <sa-toggle-group-item value="parking" aria-label="Parking">
        <sa-icon name="square-parking"/>
    </sa-toggle-group-item>
</sa-toggle-group>

<sa-toggle-group asp-for="Sort" spacing="ToggleGroupSpacing.None" variant="ToggleVariant.Outline">
    <sa-toggle-group-item value="Recommended">
        <sa-icon name="sparkles"/>
        Recommended
    </sa-toggle-group-item>
    <sa-toggle-group-item value="Price">
        <sa-icon name="banknote"/>
        Price
    </sa-toggle-group-item>
    <sa-toggle-group-item value="Rating">
        <sa-icon name="star"/>
        Rating
    </sa-toggle-group-item>
</sa-toggle-group>

<sa-toggle-group asp-for="TripStyles" multiple="true" spacing="ToggleGroupSpacing.None" variant="ToggleVariant.Outline">
    <sa-toggle-group-item value="Beach">
        <sa-icon name="sailboat"/>
        Beach
    </sa-toggle-group-item>
    <sa-toggle-group-item value="City">
        <sa-icon name="building"/>
        City
    </sa-toggle-group-item>
    <sa-toggle-group-item value="Camping">
        <sa-icon name="tent"/>
        Camping
    </sa-toggle-group-item>
</sa-toggle-group>
```
