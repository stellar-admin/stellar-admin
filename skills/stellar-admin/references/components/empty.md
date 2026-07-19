---
component: Empty
tags: [sa-empty, sa-empty-content, sa-empty-description, sa-empty-header, sa-empty-media, sa-empty-title]
generated: true
---

# Empty

An empty-state container that communicates the absence of content, composed of a header, media, title, description, and content subcomponents.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-empty>` | An empty-state container that communicates the absence of content, composed of a header, media, title, description, and content subcomponents. |
| `<sa-empty-content>` | The content region of an empty state; typically contains actions or supplementary elements below the header. |
| `<sa-empty-description>` | A line of muted descriptive text within an empty state header. |
| `<sa-empty-header>` | The header region of an empty state; typically contains the media, title, and description. |
| `<sa-empty-media>` | The media region of an empty state, displaying an icon or illustration above the title. |
| `<sa-empty-title>` | The title text within an empty state header. |

## Attributes

### `<sa-empty-media>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `variant` | `EmptyMediaVariant` | `Default` | `Default`, `Icon` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

## Example

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
