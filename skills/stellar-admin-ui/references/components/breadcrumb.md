---
component: Breadcrumb
tags: [sa-breadcrumb, sa-breadcrumb-ellipsis, sa-breadcrumb-item, sa-breadcrumb-link, sa-breadcrumb-list, sa-breadcrumb-page, sa-breadcrumb-separator]
generated: true
---

# Breadcrumb

Renders a breadcrumb that shows the path to the current page.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-breadcrumb>` | Renders a breadcrumb that shows the path to the current page. |
| `<sa-breadcrumb-ellipsis>` | Renders an ellipsis that stands in for breadcrumb items that aren't shown. |
| `<sa-breadcrumb-item>` | Renders an item in a breadcrumb list, which holds a link or the current page. |
| `<sa-breadcrumb-link>` | Renders a link in a breadcrumb item. |
| `<sa-breadcrumb-list>` | Renders the ordered list of items in a breadcrumb. |
| `<sa-breadcrumb-page>` | Renders the current page in a breadcrumb, as text that can't be clicked. |
| `<sa-breadcrumb-separator>` | Renders a separator between breadcrumb items. |

## Attributes

### `<sa-breadcrumb-link>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `asp-action` | `string` | — | — |
| `asp-area` | `string` | — | — |
| `asp-controller` | `string` | — | — |
| `asp-fragment` | `string` | — | — |
| `asp-host` | `string` | — | — |
| `asp-page` | `string` | — | — |
| `asp-page-handler` | `string` | — | — |
| `asp-protocol` | `string` | — | — |
| `asp-route` | `string` | — | — |
| `asp-all-route-data` | `IDictionary<string, string?>` | — | — |
| `asp-route-*` | `IDictionary<string, string?>` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/Breadcrumb/_Intro.cshtml`*

```razor
<sa-breadcrumb>
    <sa-breadcrumb-list>
        <sa-breadcrumb-item>
            <sa-breadcrumb-link href="#">Home</sa-breadcrumb-link>
        </sa-breadcrumb-item>
        <sa-breadcrumb-separator/>
        <sa-breadcrumb-item>
            <sa-breadcrumb-link href="#">Europe</sa-breadcrumb-link>
        </sa-breadcrumb-item>
        <sa-breadcrumb-separator/>
        <sa-breadcrumb-item>
            <sa-breadcrumb-link href="#">Italy</sa-breadcrumb-link>
        </sa-breadcrumb-item>
        <sa-breadcrumb-separator/>
        <sa-breadcrumb-page>Grand Hotel Venice</sa-breadcrumb-page>
    </sa-breadcrumb-list>
</sa-breadcrumb>
```

*From `Pages/Breadcrumb/_Collapsed.cshtml`*

```razor
<sa-breadcrumb>
    <sa-breadcrumb-list>
        <sa-breadcrumb-item>
            <sa-breadcrumb-link href="#">Home</sa-breadcrumb-link>
        </sa-breadcrumb-item>
        <sa-breadcrumb-separator/>
        <sa-breadcrumb-item>
            <sa-breadcrumb-ellipsis />
        </sa-breadcrumb-item>
        <sa-breadcrumb-separator/>
        <sa-breadcrumb-item>
            <sa-breadcrumb-link href="#">Trip #TRV-987</sa-breadcrumb-link>
        </sa-breadcrumb-item>
        <sa-breadcrumb-separator/>
        <sa-breadcrumb-page>Add Traveler Details</sa-breadcrumb-page>
    </sa-breadcrumb-list>
</sa-breadcrumb>
```
