---
component: Badge
tags: [sa-badge]
generated: true
---

# Badge

A small label used to highlight status, counts, or categories.

## Attributes

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `variant` | `BadgeVariant` | `Default` | `Default`, `Secondary`, `Destructive`, `Outline`, `Ghost`, `Link` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

## Example

*From `Pages/Badge/_Intro.cshtml`*

```razor
<sa-badge variant="BadgeVariant.Default">
    <sa-icon name="plane"/>
    Flight Booked
</sa-badge>
<sa-badge variant="BadgeVariant.Secondary">Requires Visa</sa-badge>
<sa-badge variant="BadgeVariant.Destructive">
    <sa-icon name="flame"/>
    Overbooked
</sa-badge>
```
