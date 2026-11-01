---
component: Separator
tags: [sa-separator]
generated: true
---

# Separator

Renders a thin line between sections of content.

## Attributes

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `orientation` | `SeparatorOrientation` | `Horizontal` | `Horizontal`, `Vertical` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

## Example

*From `Pages/Separator/_Intro.cshtml`*

```razor
<sa-link-button variant="ButtonVariant.Outline" href="#">View bookings</sa-link-button>
<sa-separator/>
<sa-link-button variant="ButtonVariant.Outline" href="#">View past trips</sa-link-button>
```
