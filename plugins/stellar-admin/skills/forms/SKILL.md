---
name: forms
description: >-
  Builds accessible, model-bound forms with StellarAdmin Tag Helpers in ASP.NET Core MVC /
  Razor Pages — fields with labels, descriptions and validation, asp-for model binding,
  validation messages, field groups and fieldsets, choice cards, and the input family
  (text, select, textarea, switch, checkbox, radio, slider, toggle, OTP). Use when
  creating or editing a form in a .cshtml/.razor file that uses StellarAdmin, or when the
  user mentions StellarAdmin forms, fields, inputs, model binding, or validation.
metadata:
  author: StellarAdmin
---

# Building forms with StellarAdmin

This skill covers the *patterns* for StellarAdmin forms. For a specific component's attributes and values, open its file under `../tag-helpers/references/components/` — for example `../tag-helpers/references/components/field.md`, `.../input.md`, `.../select.md`. Read `../tag-helpers/references/conventions.md` for the cross-cutting rules (fully-qualified enums, attribute pass-through, model binding).

## The field is the unit of a form

A form field is a `<sa-field>` composing, **in this order**: label → input → description → error. (Horizontal checkbox/radio/switch rows invert the first two — input first, then the label — see "Choice cards" and the implicit-field note below.)

```razor
<sa-field>
    <sa-field-label for="email">Email</sa-field-label>
    <sa-input id="email" type="email" placeholder="you@example.com" />
    <sa-field-description>Where we send your confirmation.</sa-field-description>
    <sa-field-error>Enter a valid email address.</sa-field-error>
</sa-field>
```

Any StellarAdmin input goes in the input slot: `<sa-input>`, `<sa-select>`, `<sa-textarea>`, `<sa-switch>`, `<sa-slider>`, `<sa-toggle>`, `<sa-toggle-group>`, `<sa-input-otp>`.

### Which label tag?

- **Inside a `<sa-field>` → `<sa-field-label>`.** It renders the same `<label>` but is marked as the field's label, so horizontal and responsive field layouts can position it.
- **Outside a field → `<sa-label>`.** For a standalone control, or a caption for something that isn't a form control.
- **Most of the time, neither.** Inputs render their own label (see below).

## Explicit vs implicit — prefer implicit for model-bound forms

StellarAdmin inputs can **render their own wrapping field** (label + description + error) so you don't write `<sa-field>` by hand. This is the default, preferred style for model-bound forms.

An input renders an implicit field when **any** of `asp-for`, `label`, `description`, or `error` is present — **unless** it is already inside a `<sa-field>` (it never double-wraps). Force it either way with `render-field="true|false"`.

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

**Explicit** — use when you need full control over structure (write every part yourself, as in the first example). Inside an explicit `<sa-field>`, inputs never auto-wrap.

## Validation

The validation message element is **`<sa-field-error>`**.

**Automatic (model-bound) — preferred.** With a validated model property, an implicit input renders its error on its own; there's nothing else to wire:

```razor
<sa-input asp-for="Email" />   @* shows the model error for Email when invalid *@
```

In an explicit field, write every part yourself and bind the error to the same property:

```razor
<sa-field orientation="FieldOrientation.Horizontal">
    <sa-input asp-for="BedType" type="radio" value="king" id="bed-king" />
    <sa-field-label for="bed-king">1 King</sa-field-label>
    <sa-field-error asp-for="BedType" />
</sa-field>
```

Note the label is a real `<sa-field-label>` element here. Inside an explicit `<sa-field>` the input's own `label` / `description` / `error` **attributes render nothing** — they only take effect when the input builds its own field wrapper, which it never does inside a `<sa-field>`.

Server side, standard model validation applies (`[Required]`, `ModelState`, etc.):

```csharp
if (!ModelState.IsValid) return Page();
```

**Manual.** Set `aria-invalid="true"` on the input and provide the message explicitly:

```razor
<sa-input aria-invalid="true" type="email" />
<sa-field-error>Enter your email address</sa-field-error>
```

...or implicitly via the `error` attribute:

```razor
<sa-input label="Email address" error="Enter your email address" aria-invalid="true" type="email" />
```

(jQuery-unobtrusive-validation's `input-validation-error` class also triggers the error styling.)

## Grouping: `<sa-field-group>` and `<sa-field-set>`

- `<sa-field-group>` — spacing container for a set of fields (nestable).
- `<sa-field-set>` — a semantic fieldset; holds a `<sa-field-legend>` (heading) and optional `<sa-field-description>`, then a `<sa-field-group>`.
- `<sa-field-content>` — a flex column grouping label + description, for horizontal fields.
- `<sa-field-separator />` — a horizontal divider **between fields** inside a group, optionally with content (such as a label) centered on the line.

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
| Text / email / password / number / date... | `<sa-input>` with `type="..."` (or `[DataType]` via `asp-for`) |
| **Checkbox** | `<sa-input type="checkbox" asp-for="...">` — there is **no** `<sa-checkbox>` tag |
| **Radio** | `<sa-input type="radio" asp-for="..." value="..." label="...">` — no `<sa-radio>` tag |
| Dropdown | `<sa-select asp-for="..." asp-items="...">` (supports inline `<option>` / `<optgroup>`) |
| Multi-line | `<sa-textarea asp-for="...">` |
| On/off toggle | `<sa-switch asp-for="...">` |
| Pressed-state button | `<sa-toggle asp-for="...">`, `<sa-toggle-group type="ToggleGroupType.Single">` |
| Numeric range | `<sa-slider asp-for="..." min="0" max="100">` |
| One-time code | `<sa-input-otp asp-for="..." groups="3,3">` |
| Input with an addon | `<sa-input-group>` with `<sa-input-group-input>` + `<sa-input-group-addon>` |

Bound to a `bool`, `<sa-input asp-for="...">` renders a checkbox without an explicit `type`.

**Checkbox / radio groups:** put `data-slot="checkbox-group"` (or `radio-group`) on the enclosing `<sa-field-group>`, inside a `<sa-field-set>` with a `<sa-field-legend>`, one `<sa-input>` per option.

**Horizontal fields** (checkbox/switch/radio rows): in an **explicit** field, set `orientation="FieldOrientation.Horizontal"` on the `<sa-field>` and wrap the label and description in `<sa-field-content>`.

You don't need to do this for **implicit** fields: a checkbox, radio or switch that renders its own field is laid out horizontally with the input first, automatically. So a bool property is just:

```razor
<sa-input asp-for="RememberMe" />   @* checkbox, horizontal row, label + description, all automatic *@
```

`render-field` only toggles the wrapping field on or off — it doesn't control orientation. To change the layout, write the field explicitly.

## Choice cards

For a richer selectable row, wrap a horizontal field in a `<sa-field-label>` so the whole card is clickable:

```razor
<sa-field-set class="w-full max-w-md">
    <sa-field-legend>Trip Add-ons</sa-field-legend>
    <sa-field-description>Enhance your trip with optional extras.</sa-field-description>
    <sa-field-group data-slot="checkbox-group">
        <sa-field-label for="addon-insurance">
            <sa-field orientation="FieldOrientation.Horizontal">
                <sa-field-content>
                    <sa-field-title>Travel Insurance</sa-field-title>
                    <sa-field-description>
                        Covers cancellations, medical costs, and lost luggage
                    </sa-field-description>
                </sa-field-content>
                <sa-input type="checkbox" name="addons" value="insurance" id="addon-insurance" checked/>
            </sa-field>
        </sa-field-label>
        <!-- more options -->
    </sa-field-group>
</sa-field-set>
```

The same shape with `data-slot="radio-group"` and `type="radio"` gives radio choice cards.

## Rules and gotchas

1. **Prefer implicit + `asp-for`** for model-bound forms; fall back to explicit `<sa-field>` only when you need custom structure.
2. Implicit wrapping fires on `asp-for` / `label` / `description` / `error`, but **not** inside an existing `<sa-field>`. Override with `render-field`.
3. The error tag is `<sa-field-error>`. Bind `asp-for` for automatic validation; otherwise set `aria-invalid="true"` and supply the message.
4. Checkboxes and radios are `<sa-input type="checkbox|radio">`, not dedicated tags. There is no `<sa-checkbox>`, `<sa-radio>`, `<sa-checkbox-group>` or `<sa-radio-group>` element.
5. Explicit field order is label → input → description → error — except horizontal checkbox/radio/switch rows, which put the input first.
6. Inside a field use `<sa-field-label>`; outside one use `<sa-label>`. Inside an explicit `<sa-field>`, an input's `label` / `description` / `error` attributes render nothing.
7. Enum attributes are fully-qualified (`FieldOrientation.Horizontal`).
8. Wrap the form in a standard `<form method="post">`; StellarAdmin adds no form element of its own.
