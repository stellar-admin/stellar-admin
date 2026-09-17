---
component: FormRow
tags: [sa-form-row]
generated: true
---

# FormRow

Arranges form content in equal-width columns that stack in narrow containers.

## Attributes

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Example

*From `Pages/FormRow/_Intro.cshtml`*

```razor
<sa-form-row>
    <sa-field>
        <sa-field-label for="intro-FirstName">First name</sa-field-label>
        <sa-input id="intro-FirstName" name="FirstName" />
        <sa-field-description>Use the name on your travel document.</sa-field-description>
    </sa-field>
    <sa-field>
        <sa-field-label for="intro-LastName">Last name</sa-field-label>
        <sa-input id="intro-LastName" name="LastName" />
    </sa-field>
</sa-form-row>
```
