---
component: Pagination
tags: [sa-pagination, sa-pagination-content, sa-pagination-ellipsis, sa-pagination-first, sa-pagination-item, sa-pagination-last, sa-pagination-link, sa-pagination-next, sa-pagination-previous]
generated: true
---

# Pagination

Renders navigation for moving between pages of content.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-pagination>` | Renders navigation for moving between pages of content. |
| `<sa-pagination-content>` | Renders the list that holds the pagination items. |
| `<sa-pagination-ellipsis>` | Renders a marker for pages that the pagination leaves out. |
| `<sa-pagination-first>` | Renders a pagination link to the first page. |
| `<sa-pagination-item>` | Renders an item in the pagination list. |
| `<sa-pagination-last>` | Renders a pagination link to the last page. |
| `<sa-pagination-link>` | Renders a pagination link to a page. |
| `<sa-pagination-next>` | Renders a pagination link to the next page. |
| `<sa-pagination-previous>` | Renders a pagination link to the previous page. |

## Attributes

### `<sa-pagination-first>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `size` | `ButtonSize` | `Default` | `Default`, `ExtraSmall`, `Small`, `Large`, `Icon`, `IconExtraSmall`, `IconSmall`, `IconLarge` |
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

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

### `<sa-pagination-last>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `size` | `ButtonSize` | `Default` | `Default`, `ExtraSmall`, `Small`, `Large`, `Icon`, `IconExtraSmall`, `IconSmall`, `IconLarge` |
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

### `<sa-pagination-link>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `active` | `bool` | `false` | `true`, `false` |
| `size` | `ButtonSize` | `Default` | `Default`, `ExtraSmall`, `Small`, `Large`, `Icon`, `IconExtraSmall`, `IconSmall`, `IconLarge` |
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

### `<sa-pagination-next>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `size` | `ButtonSize` | `Default` | `Default`, `ExtraSmall`, `Small`, `Large`, `Icon`, `IconExtraSmall`, `IconSmall`, `IconLarge` |
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

### `<sa-pagination-previous>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `size` | `ButtonSize` | `Default` | `Default`, `ExtraSmall`, `Small`, `Large`, `Icon`, `IconExtraSmall`, `IconSmall`, `IconLarge` |
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

*From `Pages/Pagination/_Intro.cshtml`*

```razor
<sa-pagination>
    <sa-pagination-content>
        <sa-pagination-item>
            <sa-pagination-first href="#"/>
        </sa-pagination-item>
        <sa-pagination-item>
            <sa-pagination-previous href="#"/>
        </sa-pagination-item>
        <sa-pagination-item>
            <sa-pagination-link href="#">1</sa-pagination-link>
        </sa-pagination-item>
        <sa-pagination-item>
            <sa-pagination-link href="#" active="true">2</sa-pagination-link>
        </sa-pagination-item>
        <sa-pagination-item>
            <sa-pagination-link href="#">3</sa-pagination-link>
        </sa-pagination-item>
        <sa-pagination-item>
            <sa-pagination-ellipsis/>
        </sa-pagination-item>
        <sa-pagination-item>
            <sa-pagination-link href="#">10</sa-pagination-link>
        </sa-pagination-item>
        <sa-pagination-item>
            <sa-pagination-link href="#">11</sa-pagination-link>
        </sa-pagination-item>
        <sa-pagination-item>
            <sa-pagination-next href="#"/>
        </sa-pagination-item>
        <sa-pagination-item>
            <sa-pagination-last href="#"/>
        </sa-pagination-item>
    </sa-pagination-content>
</sa-pagination>
```

*From `Pages/Pagination/_Url.cshtml`*

```razor
<sa-pagination>
    <sa-pagination-content>
        <sa-pagination-item>
            <sa-pagination-previous
                asp-controller="Booking"
                asp-action="List"
                asp-route-page="1"/>
        </sa-pagination-item>
        <sa-pagination-item>
            <sa-pagination-link asp-controller="Booking"
                                 asp-action="List"
                                 asp-route-page="1">
                1
            </sa-pagination-link>
        </sa-pagination-item>
        <sa-pagination-item>
            <sa-pagination-link asp-controller="Booking"
                                 asp-action="List"
                                 asp-route-page="2"
                                 active="true">
                2
            </sa-pagination-link>
        </sa-pagination-item>
        <sa-pagination-item>
            <sa-pagination-link asp-controller="Booking"
                                 asp-action="List"
                                 asp-route-page="3">
                3
            </sa-pagination-link>
        </sa-pagination-item>
        <sa-pagination-item>
            <sa-pagination-ellipsis/>
        </sa-pagination-item>
        <sa-pagination-item>
            <sa-pagination-link asp-controller="Booking"
                                 asp-action="List"
                                 asp-route-page="10">
                10
            </sa-pagination-link>
        </sa-pagination-item>
        <sa-pagination-item>
            <sa-pagination-link asp-controller="Booking"
                                 asp-action="List"
                                 asp-route-page="11">
                11
            </sa-pagination-link>
        </sa-pagination-item>
        <sa-pagination-item>
            <sa-pagination-next asp-controller="Booking"
                                 asp-action="List"
                                 asp-route-page="3"/>
        </sa-pagination-item>
    </sa-pagination-content>
</sa-pagination>
```
