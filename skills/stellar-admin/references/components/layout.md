---
component: Layout
tags: [sa-group, sa-container, sa-stack]
generated: true
---

# Layout

A horizontal flex layout that arranges its children in a row, with configurable alignment, spacing, and justification.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-group>` | A horizontal flex layout that arranges its children in a row, with configurable alignment, spacing, and justification. |
| `<sa-container>` | A centered, width-constrained wrapper that horizontally centers page content and applies responsive horizontal padding. |
| `<sa-stack>` | A vertical flex layout that arranges its children in a column, with configurable alignment, spacing, and justification. |

## Attributes

### `<sa-group>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `align` | `GroupAlign` | `Start` | `Stretch`, `Center`, `Start`, `End`, `Baseline` |
| `gap` | `GroupGap` | `Default` | `ExtraSmall`, `Small`, `Default`, `Large`, `ExtraLarge` |
| `justify` | `GroupJustify` | `Start` | `Center`, `Start`, `End`, `SpaceBetween`, `SpaceAround` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

### `<sa-stack>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `align` | `StackAlign` | `Stretch` | `Stretch`, `Center`, `Start`, `End` |
| `gap` | `StackGap` | `Default` | `ExtraSmall`, `Small`, `Default`, `Large`, `ExtraLarge` |
| `justify` | `StackJustify` | `Start` | `Center`, `Start`, `End`, `SpaceBetween`, `SpaceAround` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/Container/_Intro.cshtml`*

```razor
<sa-container class="rounded-lg border bg-muted py-8 text-center">
    <h2 class="text-lg font-semibold">Plan your next journey</h2>
    <p class="text-sm text-muted-foreground">
        This content sits inside a container — horizontally centered, width-constrained, with
        responsive horizontal padding.
    </p>
</sa-container>
```

*From `Pages/Container/_PageLayout.cshtml`*

```razor
<sa-container>
    <sa-stack gap="StackGap.Large">
        <sa-group justify="GroupJustify.SpaceBetween" align="GroupAlign.Center">
            <h1 class="text-xl font-semibold">Destinations</h1>
            <sa-button>
                <sa-icon name="plus"/>
                Add destination
            </sa-button>
        </sa-group>
        <p class="text-sm text-muted-foreground">
            Use a container to center and constrain your page, then Stack and Group handle the
            vertical and horizontal rhythm of the content inside it.
        </p>
    </sa-stack>
</sa-container>
```
