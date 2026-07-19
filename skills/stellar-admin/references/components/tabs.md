---
component: Tabs
tags: [sa-tab-link, sa-tab-list]
generated: true
---

# Tabs

A single tab within a `<sa-tab-list>`, rendered as a link to its target view.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-tab-link>` | A single tab within a `<sa-tab-list>`, rendered as a link to its target view. |
| `<sa-tab-list>` | A list of tabs, each linking to a different view or page. |

## Attributes

### `<sa-tab-link>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `disabled` | `bool` | `false` | `true`, `false` |
| `is-active` | `bool` | — | `true`, `false` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

### `<sa-tab-list>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `orientation` | `TabListOrientation` | `Horizontal` | `Horizontal`, `Vertical` |
| `variant` | `TabListVariant` | `Default` | `Default`, `Line` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/Tabs/_Intro.cshtml`*

```razor
<sa-tab-list>
    <sa-tab-link href="#">Flights</sa-tab-link>
    <sa-tab-link href="#">Accommodation</sa-tab-link>
    <sa-tab-link href="#">Car Rental</sa-tab-link>
</sa-tab-list>
```

*From `Pages/Tabs/_Icons.cshtml`*

```razor
<sa-tab-list>
    <sa-tab-link href="#">
        <sa-icon name="plane"/>
        Flights
    </sa-tab-link>
    <sa-tab-link href="#">
        <sa-icon name="bed-double"/>
        Accommodation
    </sa-tab-link>
    <sa-tab-link href="#">
        <sa-icon name="car-front"/>
        Car Rental
    </sa-tab-link>
</sa-tab-list>
```

*From `Pages/Tabs/_Url.cshtml`*

```razor
<sa-tab-list>
    <sa-tab-link asp-controller="Search" asp-route-category="flights">
        Flights
    </sa-tab-link>
    <sa-tab-link asp-controller="Search" asp-route-category="accommodation">
        Accommodation
    </sa-tab-link>
    <sa-tab-link asp-controller="Search" asp-route-category="carrental">
        Car Rental
    </sa-tab-link>
</sa-tab-list>
```
