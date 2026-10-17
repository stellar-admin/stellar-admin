---
component: Textarea
tags: [sa-textarea]
generated: true
---

# Textarea

A styled multi-line text input that grows with its content. Supports model binding via `asp-for`.

## Attributes

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `column-span` | `string` | — | — |
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

## Examples

*From `Pages/Textarea/_Intro.cshtml`*

```razor
<sa-field>
    <sa-field-label>Describe your experience</sa-field-label>
    <sa-textarea
        placeholder="The view from the balcony was breathtaking, but the breakfast service was a bit slow..."/>
    <sa-field-description>
        Your feedback helps other travelers make better choices. Be as descriptive as possible!
    </sa-field-description>
</sa-field>
```

*From `Pages/Textarea/_ModelBinding.cshtml`*

```razor
<sa-textarea asp-for="Review"/>
```
