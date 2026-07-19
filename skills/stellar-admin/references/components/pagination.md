---
component: Pagination
tags: [sa-pagination, sa-pagination-content, sa-pagination-ellipsis, sa-pagination-first, sa-pagination-item, sa-pagination-last, sa-pagination-link, sa-pagination-next, sa-pagination-previous]
generated: true
---

# Pagination

Navigation for moving between pages of content.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-pagination>` | Navigation for moving between pages of content. |
| `<sa-pagination-content>` | The list that holds the individual pagination items. |
| `<sa-pagination-ellipsis>` | A non-interactive item that indicates omitted pages within the pagination. |
| `<sa-pagination-first>` | A pagination link that navigates to the first page. |
| `<sa-pagination-item>` | A single item within the pagination list. |
| `<sa-pagination-last>` | A pagination link that navigates to the last page. |
| `<sa-pagination-link>` | A link to a specific page within the pagination. |
| `<sa-pagination-next>` | A pagination link that navigates to the next page. |
| `<sa-pagination-previous>` | A pagination link that navigates to the previous page. |

## Attributes

### `<sa-pagination-first>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `size` | `ButtonSize` | — | `Default`, `ExtraSmall`, `Small`, `Large`, `Icon`, `IconExtraSmall`, `IconSmall`, `IconLarge` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

### `<sa-pagination-last>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `size` | `ButtonSize` | — | `Default`, `ExtraSmall`, `Small`, `Large`, `Icon`, `IconExtraSmall`, `IconSmall`, `IconLarge` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-pagination-link>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `is-active` | `bool` | `false` | `true`, `false` |
| `size` | `ButtonSize` | `Default` | `Default`, `ExtraSmall`, `Small`, `Large`, `Icon`, `IconExtraSmall`, `IconSmall`, `IconLarge` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-pagination-next>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `size` | `ButtonSize` | — | `Default`, `ExtraSmall`, `Small`, `Large`, `Icon`, `IconExtraSmall`, `IconSmall`, `IconLarge` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-pagination-previous>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `size` | `ButtonSize` | — | `Default`, `ExtraSmall`, `Small`, `Large`, `Icon`, `IconExtraSmall`, `IconSmall`, `IconLarge` |
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
            <sa-pagination-link href="#" is-active="true">2</sa-pagination-link>
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
                                 is-active="true">
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
