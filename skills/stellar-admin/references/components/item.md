---
component: Item
tags: [sa-item, sa-item-actions, sa-item-content, sa-item-description, sa-item-footer, sa-item-group, sa-item-header, sa-item-media, sa-item-separator, sa-item-title, sa-link-item]
generated: true
---

# Item

A flexible row for presenting content, combining media, a title, description, and actions.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-item>` | A flexible row for presenting content, combining media, a title, description, and actions. |
| `<sa-item-actions>` | The region of an item that holds action controls such as buttons, aligned to its trailing edge. |
| `<sa-item-content>` | The main content region of an item; typically wraps the title and description. |
| `<sa-item-description>` | The secondary descriptive text of an item, rendered beneath its title. |
| `<sa-item-footer>` | The footer region of an item, spanning its full width beneath the main content. |
| `<sa-item-group>` | A vertical list container that groups related items together. |
| `<sa-item-header>` | The header region of an item, spanning its full width above the main content. |
| `<sa-item-media>` | The leading media region of an item, holding an icon, image, or avatar. |
| `<sa-item-separator>` | A horizontal divider used to separate items within a group. |
| `<sa-item-title>` | The primary title text of an item. |
| `<sa-link-item>` | An item rendered as an anchor, making the entire row a clickable link. |

## Attributes

### `<sa-item>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `size` | `ItemSize` | `Default` | `Default`, `Small`, `ExtraSmall` |
| `variant` | `ItemVariant` | `Default` | `Default`, `Outline`, `Muted` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

### `<sa-item-media>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `variant` | `ItemMediaVariant` | `Default` | `Default`, `Icon`, `Image` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-link-item>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `size` | `ItemSize` | `Default` | `Default`, `Small`, `ExtraSmall` |
| `variant` | `ItemVariant` | `Default` | `Default`, `Outline`, `Muted` |
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
<sa-link-item variant="ItemVariant.Outline" size="ItemSize.Small" href="#">
    <sa-item-media>
        <sa-icon name="luggage" class="size-5"/>
    </sa-item-media>
    <sa-item-content>
        <sa-item-title>Baggage policy updated.</sa-item-title>
    </sa-item-content>
    <sa-item-actions>
        <sa-icon name="chevron-right" class="size-4"/>
    </sa-item-actions>
</sa-link-item>
```

*From `Pages/Item/_Link.cshtml`*

```razor
<sa-link-item asp-controller="Booking" asp-action="Manage" asp-route-id="123">
    <sa-item-content>
        <sa-item-title>Manage My Booking</sa-item-title>
        <sa-item-description>
            View, change, or cancel your existing reservations.
        </sa-item-description>
    </sa-item-content>
    <sa-item-actions>
        <sa-icon name="chevron-right" class="size-4"/>
    </sa-item-actions>
</sa-link-item>
<sa-link-item variant="ItemVariant.Outline" href="#" target="_blank" rel="noopener noreferrer">
    <sa-item-content>
        <sa-item-title>View Current Visa Requirements</sa-item-title>
        <sa-item-description>
            Opens the official government travel site in a new tab.
        </sa-item-description>
    </sa-item-content>
    <sa-item-actions>
        <sa-icon name="external-link" class="size-4"/>
    </sa-item-actions>
</sa-link-item>
```
