---
component: InputOtp
tags: [sa-input-otp, sa-input-otp-group, sa-input-otp-separator, sa-input-otp-slot]
generated: true
---

# InputOtp

Renders a one-time code input with a slot for each character.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-input-otp>` | Renders a one-time code input with a slot for each character. |
| `<sa-input-otp-group>` | Renders a group of slots in an `<sa-input-otp>`. |
| `<sa-input-otp-separator>` | Renders a separator between groups of slots in an `<sa-input-otp>`. |
| `<sa-input-otp-slot>` | Renders a slot that shows one character of the code in an `<sa-input-otp>`. |

## Attributes

### `<sa-input-otp>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `maxlength` | `int` | — | — |
| `pattern` | `string` | `^\d*$` | — |
| `groups` | `string` | — | — |
| `inputmode` | `string` | `numeric` | — |
| `disabled` | `bool` | `false` | `true`, `false` |
| `readonly` | `bool` | `false` | `true`, `false` |
| `aria-invalid` | `bool` | — | `true`, `false` |
| `value` | `string` | — | — |
| `form` | `string` | — | — |
| `class-names` | `InputOtpClassNames` | — | — |
| `column-span` | `string` | `1` | — |
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
| `name` | `string` | — | — |
| `wrap-in-field` | `bool` | — | `true`, `false` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-input-otp-slot>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `index` | `int` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/InputOtp/_Intro.cshtml`*

```razor
<sa-input-otp maxlength="6"/>
```

*From `Pages/InputOtp/_Composition.cshtml`*

```razor
<sa-input-otp maxlength="6">
    <sa-input-otp-group>
        <sa-input-otp-slot/>
        <sa-input-otp-slot/>
        <sa-input-otp-slot/>
    </sa-input-otp-group>
    <sa-input-otp-separator/>
    <sa-input-otp-group>
        <sa-input-otp-slot/>
        <sa-input-otp-slot/>
        <sa-input-otp-slot/>
    </sa-input-otp-group>
</sa-input-otp>
```

*From `Pages/InputOtp/_ModelBinding.cshtml`*

```razor
<sa-input-otp asp-for="OneTimePassword" groups="3,3"/>
```
