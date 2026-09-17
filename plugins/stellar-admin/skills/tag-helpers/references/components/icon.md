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

## Examples

*From `Pages/Icon/_Intro.cshtml`*

```razor
<sa-icon name="rocket"/>
<sa-icon name="message-circle-heart"/>
<sa-icon name="banana"/>
<sa-icon name="timer"/>
```

*From `Pages/Icon/_Custom.cshtml`*

```razor
<sa-icon name="voyager-suitcase"/>
<sa-icon name="voyager-compass"/>
<sa-icon name="plane"/>
```
