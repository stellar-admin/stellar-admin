---
component: Spinner
tags: [sa-spinner]
generated: true
---

# Spinner

An animated spinning icon that indicates a loading or busy state.

## Attributes

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Example

*From `Pages/Spinner/_InButtons.cshtml`*

```razor
<div class="flex flex-wrap items-center gap-4">
    <sa-button>
        <sa-spinner/>
        Submit
    </sa-button>
    <sa-button disabled>
        <sa-spinner/>
        Disabled
    </sa-button>
    <sa-button variant="ButtonVariant.Outline" disabled>
        <sa-spinner/>
        Outline
    </sa-button>
    <sa-button variant="ButtonVariant.Outline" size="ButtonSize.Icon" disabled>
        <sa-spinner/>
        <span class="sr-only">Loading...</span>
    </sa-button>
</div>
```
