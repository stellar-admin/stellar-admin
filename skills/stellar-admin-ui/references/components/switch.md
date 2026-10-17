---
component: Switch
tags: [sa-switch]
generated: true
---

# Switch

A toggle control that switches between on and off states. Backed by a native checkbox so its value model-binds and posts back like any other checkbox, with the visuals driven entirely by CSS.

## Attributes

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `form` | `string` | — | — |
| `size` | `SwitchSize` | `Default` | `Default`, `Small` |
| `value` | `string` | — | — |
| `column-span` | `string` | — | — |
| `column-span-lg` | `string` | — | — |
| `column-span-md` | `string` | — | — |
| `column-span-sm` | `string` | — | — |
| `column-start` | `string` | `auto` | — |
| `column-start-lg` | `string` | — | — |
| `column-start-md` | `string` | — | — |
| `column-start-sm` | `string` | — | — |
| `description` | `string` | — | — |
| `error` | `string` | — | — |
| `asp-for` | `ModelExpression` | — | — |
| `label` | `string` | — | — |
| `name` | `string` | `Name` | — |
| `wrap-in-field` | `bool` | — | `true`, `false` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

## Examples

*From `Pages/Switch/_Intro.cshtml`*

```razor
<sa-field orientation="FieldOrientation.Horizontal">
    <sa-switch id="intro-notifications" checked/>
    <sa-field-content>
        <sa-field-label for="intro-notifications">Flight alerts</sa-field-label>
        <sa-field-description>
            Receive an email when a flight on your itinerary changes.
        </sa-field-description>
    </sa-field-content>
</sa-field>
```

*From `Pages/Switch/_ModelBinding.cshtml`*

```razor
<sa-switch asp-for="EmailNotifications"/>
```
