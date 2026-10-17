---
component: Item
tags: [sa-item, sa-item-actions, sa-item-content, sa-item-description, sa-item-footer, sa-item-group, sa-item-header, sa-item-link, sa-item-media, sa-item-separator, sa-item-title]
generated: true
---

# Item

Renders a row that combines media, a title, a description and actions.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-item>` | Renders a row that combines media, a title, a description and actions. |
| `<sa-item-actions>` | Renders the area of an item that holds actions, such as buttons. |
| `<sa-item-content>` | Renders the main content of an item, which usually holds its title and description. |
| `<sa-item-description>` | Renders the description of an item, below its title. |
| `<sa-item-footer>` | Renders the footer of an item, across its full width below the content. |
| `<sa-item-group>` | Renders a list of related items. |
| `<sa-item-header>` | Renders the header of an item, across its full width above the content. |
| `<sa-item-link>` | Renders an item as a link, so the whole row can be clicked. |
| `<sa-item-media>` | Renders the media of an item, such as an icon, image or avatar, at its start. |
| `<sa-item-separator>` | Renders a horizontal line between items in a group. |
| `<sa-item-title>` | Renders the title of an item. |

## Attributes

### `<sa-item>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `size` | `ItemSize` | `Default` | `Default`, `ExtraSmall`, `Small` |
| `variant` | `ItemVariant` | `Default` | `Default`, `Outline`, `Muted` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

### `<sa-item-link>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `size` | `ItemSize` | `Default` | `Default`, `ExtraSmall`, `Small` |
| `variant` | `ItemVariant` | `Default` | `Default`, `Outline`, `Muted` |
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

### `<sa-item-media>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `variant` | `ItemMediaVariant` | `Default` | `Default`, `Icon`, `Image` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/Item/_Intro.cshtml`*

```razor
<sa-item variant="ItemVariant.Outline">
    <sa-item-content>
        <sa-item-title>Flight to Paris</sa-item-title>
        <sa-item-description>
            Departs 10:15, Gate A4
        </sa-item-description>
    </sa-item-content>
    <sa-item-actions>
        <sa-button variant="ButtonVariant.Outline" size="ButtonSize.Small">
            View Details
        </sa-button>
    </sa-item-actions>
</sa-item>
<sa-item-link variant="ItemVariant.Outline" size="ItemSize.Small" href="#">
    <sa-item-media>
        <sa-icon name="luggage" class="size-5"/>
    </sa-item-media>
    <sa-item-content>
        <sa-item-title>Baggage policy updated.</sa-item-title>
    </sa-item-content>
    <sa-item-actions>
        <sa-icon name="chevron-right" class="size-4"/>
    </sa-item-actions>
</sa-item-link>
```

*From `Pages/Item/_Link.cshtml`*

```razor
<sa-item-link asp-controller="Booking" asp-action="Manage" asp-route-id="123">
    <sa-item-content>
        <sa-item-title>Manage My Booking</sa-item-title>
        <sa-item-description>
            View, change, or cancel your existing reservations.
        </sa-item-description>
    </sa-item-content>
    <sa-item-actions>
        <sa-icon name="chevron-right" class="size-4"/>
    </sa-item-actions>
</sa-item-link>
<sa-item-link variant="ItemVariant.Outline" href="#" target="_blank" rel="noopener noreferrer">
    <sa-item-content>
        <sa-item-title>View Current Visa Requirements</sa-item-title>
        <sa-item-description>
            Opens the official government travel site in a new tab.
        </sa-item-description>
    </sa-item-content>
    <sa-item-actions>
        <sa-icon name="external-link" class="size-4"/>
    </sa-item-actions>
</sa-item-link>
```
