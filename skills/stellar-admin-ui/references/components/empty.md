---
component: Empty
tags: [sa-empty, sa-empty-content, sa-empty-description, sa-empty-header, sa-empty-media, sa-empty-title]
generated: true
---

# Empty

Renders an empty state, which tells the user there is no content to show.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-empty>` | Renders an empty state, which tells the user there is no content to show. |
| `<sa-empty-content>` | Renders the content of an empty state below its header, such as actions. |
| `<sa-empty-description>` | Renders the description in an empty state's header. |
| `<sa-empty-header>` | Renders the header of an empty state, which holds the media, title and description. |
| `<sa-empty-media>` | Renders an icon or illustration above an empty state's title. |
| `<sa-empty-title>` | Renders the title in an empty state's header. |

## Attributes

### `<sa-empty-media>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `variant` | `EmptyMediaVariant` | `Default` | `Default`, `Icon` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

## Examples

*From `Pages/Empty/_Intro.cshtml`*

```razor
<sa-empty>
    <sa-empty-header>
        <sa-empty-media variant="EmptyMediaVariant.Icon">
            <sa-icon name="search"/>
        </sa-empty-media>
        <sa-empty-title>No Destinations Match Your Search</sa-empty-title>
        <sa-empty-description>
            We couldn't find any flights, hotels, or packages matching your filters. Try adjusting your dates,
            increasing your search radius, or selecting a different airport.
        </sa-empty-description>
    </sa-empty-header>
    <sa-empty-content>
        <sa-button variant="ButtonVariant.Outline">
            <sa-icon name="funnel-x"/>
            Reset Search Filters
        </sa-button>
    </sa-empty-content>
</sa-empty>
```

*From `Pages/Empty/_WithIcon.cshtml`*

```razor
<sa-empty class="border border-dashed">
    <sa-empty-header>
        <sa-empty-media variant="EmptyMediaVariant.Icon">
            <sa-icon name="folder"/>
        </sa-empty-media>
        <sa-empty-title>No itineraries yet</sa-empty-title>
        <sa-empty-description>
            No itineraries have been created for this trip yet. Get started by <a href="#">creating the first one</a>.
        </sa-empty-description>
    </sa-empty-header>
    <sa-empty-content>
        <sa-button variant="ButtonVariant.Outline">
            <sa-icon name="plus"/>
            New Itinerary
        </sa-button>
    </sa-empty-content>
</sa-empty>
```
