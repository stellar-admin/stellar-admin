---
component: Progress
tags: [sa-progress, sa-progress-label, sa-progress-value]
generated: true
---

# Progress

A progress bar that visualizes the completion of a task as a filled track. Compose it with the label and value subcomponents.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-progress>` | A progress bar that visualizes the completion of a task as a filled track. Compose it with the label and value subcomponents. |
| `<sa-progress-label>` | A text label for a progress bar, rendered as a `<span>`. |
| `<sa-progress-value>` | Displays a progress bar's value, rendered as a `<span>`; falls back to the computed completion percentage when no content is supplied. |

## Attributes

### `<sa-progress>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `maximum` | `int` | `100` | — |
| `minimum` | `int` | `0` | — |
| `value` | `int` | `0` | — |
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
