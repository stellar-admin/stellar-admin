---
component: InputGroup
tags: [sa-input-group, sa-input-group-addon, sa-input-group-button, sa-input-group-input, sa-input-group-text, sa-input-group-textarea]
generated: true
---

# InputGroup

A container that groups an input with add-ons, buttons, or text so they render as a single combined field.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-input-group>` | A container that groups an input with add-ons, buttons, or text so they render as a single combined field. |
| `<sa-input-group-addon>` | A decoration attached to an input group, such as an icon, text, or button, aligned to one of the input's edges. Clicking the add-on focuses the group's input. |
| `<sa-input-group-button>` | A button styled to sit inside an input group, typically within an add-on. |
| `<sa-input-group-input>` | A text input rendered inside an input group, styled to blend into the group so add-ons appear within a single field. |
| `<sa-input-group-text>` | A run of text or an icon displayed inside an input group, typically within an add-on. |
| `<sa-input-group-textarea>` | A multi-line text input rendered inside an input group, styled to blend into the group. |

## Attributes

### `<sa-input-group-addon>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `align` | `InputGroupAddOnVariantAlignment` | `InlineStart` | `InlineStart`, `InlineEnd`, `BlockStart`, `BlockEnd` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

### `<sa-input-group-button>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `size` | `InputGroupButtonSize` | `ExtraSmall` | `ExtraSmall`, `Small`, `IconExtraSmall`, `IconSmall` |
| `variant` | `ButtonVariant` | `Ghost` | `Default`, `Destructive`, `Outline`, `Secondary`, `Ghost`, `Link` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-input-group-input>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `asp-for` | `ModelExpression` | — | — |
| `asp-format` | `string` | — | — |
| `form` | `string` | — | — |
| `type` | `string` | — | — |
| `value` | `string` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-input-group-textarea>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `asp-for` | `ModelExpression` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/InputGroup/_Icons.cshtml`*

```razor
<sa-input-group>
    <sa-input-group-input placeholder="Search..."/>
    <sa-input-group-addon>
        <sa-icon name="search"/>
    </sa-input-group-addon>
</sa-input-group>
<sa-input-group>
    <sa-input-group-input type="email" placeholder="Enter your email"/>
    <sa-input-group-addon>
        <sa-icon name="mail"/>
    </sa-input-group-addon>
</sa-input-group>
<sa-input-group>
    <sa-input-group-input placeholder="Card number"/>
    <sa-input-group-addon>
        <sa-icon name="credit-card"/>
    </sa-input-group-addon>
    <sa-input-group-addon align="InputGroupAddOnVariantAlignment.InlineEnd">
        <sa-icon name="check"/>
    </sa-input-group-addon>
</sa-input-group>
<sa-input-group>
    <sa-input-group-input placeholder="Card number"/>
    <sa-input-group-addon align="InputGroupAddOnVariantAlignment.InlineEnd">
        <sa-icon name="star"/>
        <sa-icon name="info"/>
    </sa-input-group-addon>
</sa-input-group>
```

*From `Pages/InputGroup/_Buttons.cshtml`*

```razor
<sa-input-group>
    <sa-input-group-input placeholder="https://x.com/shadcn" readonly/>
    <sa-input-group-addon align="InputGroupAddOnVariantAlignment.InlineEnd">
        <sa-input-group-button
            aria-label="Copy"
            title="Copy"
            size="InputGroupButtonSize.IconExtraSmall"
        >
            <sa-icon name="copy"/>
        </sa-input-group-button>
    </sa-input-group-addon>
</sa-input-group>
<sa-input-group class="[--radius:9999px]">
    <sa-input-group-addon class="text-muted-foreground pl-1.5">
        https://
    </sa-input-group-addon>
    <sa-input-group-input id="input-secure-19"/>
    <sa-input-group-addon align="InputGroupAddOnVariantAlignment.InlineEnd">
        <sa-input-group-button
            size="InputGroupButtonSize.IconExtraSmall"
        >
            <sa-icon name="star"/>
        </sa-input-group-button>
    </sa-input-group-addon>
</sa-input-group>
<sa-input-group>
    <sa-input-group-input placeholder="Type to search..."/>
    <sa-input-group-addon align="InputGroupAddOnVariantAlignment.InlineEnd">
        <sa-input-group-button variant="ButtonVariant.Secondary">Search</sa-input-group-button>
    </sa-input-group-addon>
</sa-input-group>
```
