---
component: ToggleGroup
tags: [sa-toggle-group, sa-toggle-group-item]
generated: true
---

# ToggleGroup

Groups a set of toggle items into a single-select or multi-select control.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-toggle-group>` | Groups a set of toggle items into a single-select or multi-select control. |
| `<sa-toggle-group-item>` | A single selectable item within a toggle group. |

## Attributes

### `<sa-toggle-group>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `type` | `ToggleGroupType` | `Single` | `Single`, `Multiple` |
| `variant` | `ToggleVariant` | `Default` | `Default`, `Outline` |
| `size` | `ToggleSize` | `Default` | `Default`, `Small`, `Large` |
| `orientation` | `ToggleGroupOrientation` | `Horizontal` | `Horizontal`, `Vertical` |
| `spacing` | `int` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

### `<sa-toggle-group-item>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `value` | `string` | — | — |
| `variant` | `ToggleVariant` | — | `Default`, `Outline` |
| `size` | `ToggleSize` | — | `Default`, `Small`, `Large` |
| `selected` | `bool` | — | `true`, `false` |
| `disabled` | `bool` | — | `true`, `false` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/ToggleGroup/_Intro.cshtml`*

```razor
<sa-toggle-group name="results-view" type="ToggleGroupType.Single">
    <sa-toggle-group-item value="list" selected="true" aria-label="List view">
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
<sa-toggle-group asp-for="ResultsView" type="ToggleGroupType.Single" spacing="0" variant="ToggleVariant.Outline">
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

<sa-toggle-group asp-for="Amenities" type="ToggleGroupType.Multiple" spacing="0" variant="ToggleVariant.Outline">
    <sa-toggle-group-item value="wifi" aria-label="Free Wi-Fi">
        <sa-icon name="wifi"/>
    </sa-toggle-group-item>
    <sa-toggle-group-item value="pool" aria-label="Pool">
        <sa-icon name="waves"/>
    </sa-toggle-group-item>
    <sa-toggle-group-item value="parking" aria-label="Parking">
        <sa-icon name="square-parking"/>
    </sa-toggle-group-item>
</sa-toggle-group>

<sa-toggle-group asp-for="Sort" type="ToggleGroupType.Single" spacing="0" variant="ToggleVariant.Outline">
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

<sa-toggle-group asp-for="TripStyles" type="ToggleGroupType.Multiple" spacing="0" variant="ToggleVariant.Outline">
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
