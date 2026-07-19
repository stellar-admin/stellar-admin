---
component: InputOtp
tags: [sa-input-otp, sa-input-otp-group, sa-input-otp-separator, sa-input-otp-slot]
generated: true
---

# InputOtp

A segmented one-time-code input. A single real `<input>` holds the whole code and posts it as one form value; the presentational slot cells (one per character) display the value and the active caret. The `sel-input-otp` web component distributes the value into the cells and drives the active / caret state once hydrated.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-input-otp>` | A segmented one-time-code input. A single real `<input>` holds the whole code and posts it as one form value; the presentational slot cells (one per character) display the value and the active caret. The `sel-input-otp` web component distributes the value into the cells and drives the active / caret state once hydrated. |
| `<sa-input-otp-group>` | A group of `sa-input-otp-slot`s within a `sa-input-otp`. Groups are separated by a `sa-input-otp-separator`. |
| `<sa-input-otp-separator>` | A separator placed between `sa-input-otp-group`s. Renders a Lucide `minus` icon by default; supply child content to override it. |
| `<sa-input-otp-slot>` | A single presentational slot cell within a `sa-input-otp`. Displays one character of the code and the active caret. The slot self-assigns its index from the parent's running counter unless an explicit `index` is set. |

## Attributes

### `<sa-input-otp>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `max-length` | `int` | — | — |
| `pattern` | `string` | — | — |
| `groups` | `string` | — | — |
| `inputmode` | `string` | — | — |
| `disabled` | `bool` | `false` | `true`, `false` |
| `aria-invalid` | `bool` | — | `true`, `false` |
| `value` | `string` | — | — |
| `form` | `string` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-input-otp-slot>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `index` | `int` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/InputOtp/_Intro.cshtml`*

```razor
<sa-input-otp max-length="6"/>
```

*From `Pages/InputOtp/_Composition.cshtml`*

```razor
<sa-input-otp max-length="6">
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
