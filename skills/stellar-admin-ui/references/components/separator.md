---
component: Separator
tags: [sa-separator]
generated: true
---

# Separator

A thin dividing line between sections of content, rendered as a `<div>` with `role="separator"`.

## Attributes

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `orientation` | `SeparatorOrientation` | `Horizontal` | `Horizontal`, `Vertical` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

## Example

*From `Pages/Separator/_Intro.cshtml`*

```razor
<sa-linkbutton variant="ButtonVariant.Outline" href="#">View bookings</sa-linkbutton>
<sa-separator/>
<sa-linkbutton variant="ButtonVariant.Outline" href="#">View past trips</sa-linkbutton>
```
