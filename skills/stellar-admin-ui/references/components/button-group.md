---
component: ButtonGroup
tags: [sa-button-group, sa-button-group-separator, sa-button-group-text]
generated: true
---

# ButtonGroup

Renders related buttons as a single unit.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-button-group>` | Renders related buttons as a single unit. |
| `<sa-button-group-separator>` | Renders a separator between items in a button group. |
| `<sa-button-group-text>` | Renders a text label in a button group. |

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
| `orientation` | `ButtonGroupSeparatorOrientation` | `Vertical` | `Horizontal`, `Vertical` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/ButtonGroup/_Basic.cshtml`*

```razor
<sa-button-group>
    <sa-button variant="ButtonVariant.Outline">
        Flights
    </sa-button>
    <sa-button variant="ButtonVariant.Outline">
        Hotels
    </sa-button>
</sa-button-group>
```

*From `Pages/ButtonGroup/_WithInput.cshtml`*

```razor
<div class="flex flex-col gap-4">
    <sa-button-group>
        <sa-button variant="ButtonVariant.Outline">Search</sa-button>
        <sa-input placeholder="Search destinations..." />
    </sa-button-group>
    <sa-button-group>
        <sa-input placeholder="Enter promo code" />
        <sa-button variant="ButtonVariant.Outline">Apply</sa-button>
    </sa-button-group>
</div>
```
