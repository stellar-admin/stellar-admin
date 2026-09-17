---
component: Input
tags: [sa-input]
generated: true
---

# Input

A form input. Renders a styled `<input>` for text-like types, and a styled checkbox or radio button (with its indicator) when the type is `checkbox` or `radio`. Supports model binding via `asp-for`.

## Attributes

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `form` | `string` | — | — |
| `asp-format` | `string` | — | — |
| `type` | `string` | — | — |
| `value` | `string` | — | — |
| `description` | `string` | — | — |
| `error` | `string` | — | — |
| `asp-for` | `ModelExpression` | — | — |
| `label` | `string` | — | — |
| `name` | `string` | `Name` | — |
| `render-field` | `bool` | — | `true`, `false` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/Input/_Intro.cshtml`*

```razor
<sa-input placeholder="Enter your email address" type="email"/>
```

*From `Pages/Input/_ModelBinding.cshtml`*

```razor
<sa-input asp-for="Email"/>
```

*From `Pages/Checkbox/_Intro.cshtml`*

```razor
<sa-field orientation="FieldOrientation.Horizontal">
    <sa-input id="intro-terms" type="checkbox" checked/>
    <sa-field-content>
        <sa-field-label for="intro-terms">Accept terms and conditions</sa-field-label>
        <sa-field-description>
            By clicking this checkbox, you agree to the terms and conditions.
        </sa-field-description>
    </sa-field-content>
</sa-field>
```

*From `Pages/Radio/_Intro.cshtml`*

```razor
<sa-field-set class="w-full">
    <sa-field-legend>Bed Preference</sa-field-legend>
    <sa-field-description>
        Choose your preferred bed configuration.
    </sa-field-description>
    <sa-field-group data-slot="radio-group">
        <sa-field orientation="FieldOrientation.Horizontal">
            <sa-input type="radio" name="bed-preference" value="king" id="bed-king" />
            <sa-field-content>
                <sa-field-label for="bed-king">
                    1 King
                </sa-field-label>
                <sa-field-description>
                    Large bed for couples or solo travelers
                </sa-field-description>
            </sa-field-content>
        </sa-field>
        <sa-field orientation="FieldOrientation.Horizontal">
            <sa-input type="radio" name="bed-preference" value="queen" id="bed-queen" />
            <sa-field-content>
                <sa-field-label for="bed-queen">
                    1 Queen
                </sa-field-label>
                <sa-field-description>
                    Standard bed for up to two people
                </sa-field-description>
            </sa-field-content>
        </sa-field>
        <sa-field orientation="FieldOrientation.Horizontal">
            <sa-input type="radio" name="bed-preference" value="single" id="bed-single" checked />
            <sa-field-content>
                <sa-field-label for="bed-single">
                    2 Single
                </sa-field-label>
                <sa-field-description>
                    Ideal for friends or colleagues
                </sa-field-description>
            </sa-field-content>
        </sa-field>
    </sa-field-group>
</sa-field-set>
```
