---
component: Button
tags: [sa-button, sa-linkbutton]
generated: true
---

# Button

Renders a button element for triggering actions.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-button>` | Renders a button element for triggering actions. |
| `<sa-linkbutton>` | Renders an anchor element styled as a button, with routing support. |

## Attributes

### `<sa-button>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `size` | `ButtonSize` | `Default` | `Default`, `ExtraSmall`, `Small`, `Large`, `Icon`, `IconExtraSmall`, `IconSmall`, `IconLarge` |
| `variant` | `ButtonVariant` | `Default` | `Default`, `Destructive`, `Outline`, `Secondary`, `Ghost`, `Link` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

### `<sa-linkbutton>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `size` | `ButtonSize` | `Default` | `Default`, `ExtraSmall`, `Small`, `Large`, `Icon`, `IconExtraSmall`, `IconSmall`, `IconLarge` |
| `variant` | `ButtonVariant` | `Default` | `Default`, `Destructive`, `Outline`, `Secondary`, `Ghost`, `Link` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Example

*From `Pages/Button/_Intro.cshtml`*

```razor
<sa-button variant="ButtonVariant.Outline">
    <sa-icon name="undo-2"/>
    Cancel Changes
</sa-button>
<sa-button>Save Changes</sa-button>
```
