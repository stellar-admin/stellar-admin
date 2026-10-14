---
component: Label
tags: [sa-label]
generated: true
---

# Label

A caption for a form control, optionally bound to a model expression via `asp-for`.

## Attributes

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `asp-for` | `ModelExpression` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/Label/_WithField.cshtml`*

```razor
<sa-field>
    <sa-field-label for="label-with-field-email">Email</sa-field-label>
    <sa-input type="email" id="label-with-field-email" placeholder="ibn@voyager.travel"/>
    <sa-field-description>We only use this address for booking confirmations.</sa-field-description>
</sa-field>
```

*From `Pages/Label/_ModelBinding.cshtml`*

```razor
<div class="grid gap-2">
    <sa-label asp-for="TravellerName"/>
    <sa-input asp-for="TravellerName" render-field="false"/>
</div>
```
