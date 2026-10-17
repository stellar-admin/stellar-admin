---
component: Progress
tags: [sa-progress, sa-progress-label, sa-progress-value]
generated: true
---

# Progress

Renders a progress bar that shows how far a task has progressed.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-progress>` | Renders a progress bar that shows how far a task has progressed. |
| `<sa-progress-label>` | Renders a label for a progress bar. |
| `<sa-progress-value>` | Renders the value of a progress bar. |

## Attributes

### `<sa-progress>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `max` | `double` | `100` | — |
| `min` | `double` | `0` | — |
| `value` | `double` | `0` | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/Progress/_Intro.cshtml`*

```razor
<div class="flex w-full flex-col gap-4">
    <sa-progress value="0"/>
    <sa-progress value="25"/>
    <sa-progress value="50"/>
    <sa-progress value="75"/>
    <sa-progress value="100"/>
</div>
```

*From `Pages/Progress/_WithLabel.cshtml`*

```razor
<sa-progress value="56">
    <sa-progress-label>Upload progress</sa-progress-label>
    <sa-progress-value/>
</sa-progress>
```
