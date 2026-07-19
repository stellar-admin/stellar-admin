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
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

## Examples

*From `Pages/Switch/_Intro.cshtml`*

```razor
<sa-field orientation="FieldOrientation.Horizontal">
    <sa-switch id="intro-notifications" checked/>
    <sa-field-content>
        <sa-field-label for="intro-notifications">Email notifications</sa-field-label>
        <sa-field-description>
            Receive emails about your account activity.
        </sa-field-description>
    </sa-field-content>
</sa-field>
```

*From `Pages/Switch/_ModelBinding.cshtml`*

```razor
<sa-switch asp-for="EmailNotifications"/>
```
