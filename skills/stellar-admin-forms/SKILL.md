---
name: stellar-admin-forms
description: >-
  Builds accessible, model-bound forms with StellarAdmin.UI tag helpers in ASP.NET Core MVC / Razor
  Pages — fields with labels/descriptions/validation, asp-for model binding, validation
  messages, field groups and fieldsets, and the input family (text, select, textarea, switch,
  checkbox, radio, OTP). Use when creating or editing a form in a .cshtml/.razor file that uses
  StellarAdmin.UI, or when the user mentions StellarAdmin.UI forms, fields, inputs, model binding, or validation.
metadata:
  author: StellarAdmin.UI
paths:
  - "**/*.cshtml"
  - "**/*.razor"
---

# Building forms with StellarAdmin.UI

This skill covers the *patterns* for StellarAdmin.UI forms. For a specific component's attributes and
values, open its file under `../stellar-admin/references/components/` (e.g. `field.md`, `input.md`,
`select.md`). Read `../stellar-admin/references/conventions.md` for the cross-cutting rules (enums are
fully-qualified, `class` wins, etc.).

## The field is the unit of a form

A form field is a `<sa-field>` composing, **in this order**: label → input → description →
error.

```razor
<sa-field>
    <sa-field-label for="email">Email</sa-field-label>
    <sa-input id="email" type="email" placeholder="you@example.com" />
    <sa-field-description>Where we send your confirmation.</sa-field-description>
    <sa-field-error>Enter a valid email address.</sa-field-error>
</sa-field>
```

Any StellarAdmin.UI input goes in the input slot: `<sa-input>`, `<sa-select>`, `<sa-textarea>`,
`<sa-switch>`, `<sa-input-otp>`.

## Explicit vs implicit — prefer implicit for model-bound forms

StellarAdmin.UI inputs can **render their own wrapping field** (label + description + error) so you don't
write `<sa-field>` by hand. This is the default, preferred style for model-bound forms.

An input renders an implicit field when **any** of `asp-for`, `label`, `description`, or `error`
is present — **unless** it is already inside a `<sa-field>` (it never double-wraps). Force it
either way with `render-field="true|false"`.

**Implicit + model binding (the recommended default):**

```razor
@model CheckoutModel
<sa-field-set>
    <sa-field-group>
        <sa-input asp-for="Email" />
        <sa-input asp-for="Password" />
        <sa-select asp-for="CabinClass" asp-items="@Html.GetEnumSelectList<CabinClass>()"></sa-select>
        <sa-textarea asp-for="Notes" />
    </sa-field-group>
</sa-field-set>
```

The label, description, placeholder, and input type all come from the model:

```csharp
[Display(Name = "Email address",
         Description = "Where we send your booking confirmation",
         Prompt = "you@example.com")]   // Prompt -> placeholder
[DataType(DataType.EmailAddress)]        // DataType -> input type
[Required]
public string? Email { get; set; }
```

**Implicit without a model** — supply the copy via attributes:

```razor
<sa-input label="Email address"
           description="Where we send your confirmation"
           placeholder="you@example.com"
           type="email" />
```

**Explicit** — use when you need full control over structure/markup (write every part yourself,
as in the first example above). Inside an explicit `<sa-field>`, inputs never auto-wrap.

## Validation

The validation message element is **`<sa-field-error>`**.

**Automatic (model-bound) — preferred.** With a validated model property, an implicit input
renders its error on its own; there's nothing else to wire:

```razor
<sa-input asp-for="Email" />   @* shows the model error for Email when invalid *@
```

In an explicit field, bind the error to the same property:

```razor
<sa-field>
    <sa-field-label asp-for="BedType">Bed type</sa-field-label>
    <sa-input asp-for="BedType" type="radio" value="king" label="1 King" />
    <sa-field-error asp-for="BedType" />
</sa-field>
```

Server side, standard model validation applies (`[Required]`, `ModelState`, etc.):

```csharp
if (!ModelState.IsValid) return Page();
```

**Manual.** Set `aria-invalid="true"` on the input and provide the message — explicitly:

```razor
<sa-input aria-invalid="true" type="email" />
<sa-field-error>Enter your email address</sa-field-error>
```

…or implicitly via the `error` attribute:

```razor
<sa-input label="Email address" error="Enter your email address" aria-invalid="true" type="email" />
```

(jQuery-unobtrusive-validation's `input-validation-error` class also triggers the error styling.)

## Grouping: `<sa-field-group>` and `<sa-field-set>`

- `<sa-field-group>` — spacing container for a set of fields (nestable).
- `<sa-field-set>` — a semantic fieldset; holds a `<sa-field-legend>` (heading) and optional
  `<sa-field-description>`, then a `<sa-field-group>`.
- `<sa-field-separator />` — divider between fieldsets.

```razor
<sa-field-set>
    <sa-field-legend>Address</sa-field-legend>
    <sa-field-description>We use this to deliver your tickets.</sa-field-description>
    <sa-field-group>
        <sa-input asp-for="Street" />
        <div class="grid grid-cols-2 gap-4">
            <sa-input asp-for="City" />
            <sa-input asp-for="PostalCode" />
        </div>
    </sa-field-group>
</sa-field-set>
```

## The input family (what tag to use)

| Need | Use |
|------|-----|
| Text / email / password / number / date… | `<sa-input>` with `type="…"` (or `[DataType]` via `asp-for`) |
| **Checkbox** | `<sa-input type="checkbox" asp-for="…">` — there is **no** `<sa-checkbox>` tag |
| **Radio** | `<sa-input type="radio" asp-for="…" value="…" label="…">` — no `<sa-radio>` tag |
| Dropdown | `<sa-select asp-for="…" asp-items="…">` (supports inline `<option>` / `<optgroup>`) |
| Multi-line | `<sa-textarea asp-for="…">` |
| On/off toggle | `<sa-switch asp-for="…">` |
| One-time code | `<sa-input-otp asp-for="…" groups="3,3">` |

**Checkbox / radio groups:** put `data-slot="checkbox-group"` (or `radio-group`) on the enclosing
`<sa-field-group>`, inside a `<sa-field-set>` with a `<sa-field-legend>`, one
`<sa-input asp-for="…">` per option.

**Horizontal fields** (checkbox/switch/radio rows): set `orientation="FieldOrientation.Horizontal"`
on the `<sa-field>` and wrap the label + description in `<sa-field-content>`.

## Rules & gotchas

1. **Prefer implicit + `asp-for`** for model-bound forms; fall back to explicit `<sa-field>`
   only when you need custom structure.
2. Implicit wrapping fires on `asp-for`/`label`/`description`/`error`, but **not** inside an
   existing `<sa-field>`. Override with `render-field`.
3. The error tag is `<sa-field-error>`. Bind `asp-for` for automatic validation; otherwise set
   `aria-invalid="true"` and supply the message.
4. Checkboxes and radios are `<sa-input type="checkbox|radio">`, not dedicated tags.
5. Explicit field order is label → input → description → error.
6. Enum attributes are fully-qualified (`FieldOrientation.Horizontal`).
7. Wrap the form in a standard `<form method="post">`; StellarAdmin.UI adds no form element of its own.
