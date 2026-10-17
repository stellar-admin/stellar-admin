---
component: Toggle
tags: [sa-toggle]
generated: true
---

# Toggle

Renders a button that can be pressed on or off.

## Attributes

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `form` | `string` | — | — |
| `variant` | `ToggleVariant` | `Default` | `Default`, `Outline` |
| `size` | `ToggleSize` | `Default` | `Default`, `Small`, `Large` |
| `value` | `string` | — | — |
| `column-span` | `string` | `1` | — |
| `column-span-lg` | `string` | — | — |
| `column-span-md` | `string` | — | — |
| `column-span-sm` | `string` | — | — |
| `column-start` | `string` | `auto` | — |
| `column-start-lg` | `string` | — | — |
| `column-start-md` | `string` | — | — |
| `column-start-sm` | `string` | — | — |
| `description` | `string` | — | — |
| `error` | `string` | — | — |
| `asp-for` | `ModelExpression` | — | — |
| `label` | `string` | — | — |
| `name` | `string` | — | — |
| `wrap-in-field` | `bool` | — | `true`, `false` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

## Examples

*From `Pages/Toggle/_Intro.cshtml`*

```razor
<sa-toggle aria-label="Save to wishlist">
    <sa-icon name="heart"/>
</sa-toggle>
```

*From `Pages/Toggle/_WithText.cshtml`*

```razor
<sa-toggle variant="ToggleVariant.Outline" aria-label="Free Wi-Fi">
    <sa-icon name="wifi"/>
    Free Wi-Fi
</sa-toggle>
```
