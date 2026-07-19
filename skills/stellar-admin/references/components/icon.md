---
component: Icon
tags: [sa-icon]
generated: true
---

# Icon

Renders an SVG icon from the active icon pack by name.

## Attributes

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `name` | `string` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Example

*From `Pages/Icon/_Intro.cshtml`*

```razor
<sa-icon name="rocket"/>
<sa-icon name="message-circle-heart"/>
<sa-icon name="banana"/>
<sa-icon name="timer"/>
```
