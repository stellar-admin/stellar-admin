---
component: ButtonGroup
tags: [sa-button-group, sa-button-group-separator, sa-button-group-text]
generated: true
---

# ButtonGroup

Groups related buttons together as a single visual unit.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-button-group>` | Groups related buttons together as a single visual unit. |
| `<sa-button-group-separator>` | Renders a divider between items within a button group. |
| `<sa-button-group-text>` | Renders a non-interactive text label within a button group. |

## Attributes

### `<sa-button-group>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `orientation` | `ButtonGroupOrientation` | `Horizontal` | `Horizontal`, `Vertical` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

### `<sa-button-group-separator>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `orientation` | `SeparatorOrientation` | `Vertical` | `Horizontal`, `Vertical` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/ButtonGroup/_Basic.cshtml`*

```razor
<sa-button-group>
    <sa-button variant="ButtonVariant.Outline">
        Button
    </sa-button>
    <sa-button variant="ButtonVariant.Outline">
        Another Button
    </sa-button>
</sa-button-group>
```

*From `Pages/ButtonGroup/_WithInput.cshtml`*

```razor
<div class="flex flex-col gap-4">
    <sa-button-group>
        <sa-button variant="ButtonVariant.Outline">Button</sa-button>
        <sa-input placeholder="Type something here..." />
    </sa-button-group>
    <sa-button-group>
        <sa-input placeholder="Type something here..." />
        <sa-button variant="ButtonVariant.Outline">Button</sa-button>
    </sa-button-group>
</div>
```
