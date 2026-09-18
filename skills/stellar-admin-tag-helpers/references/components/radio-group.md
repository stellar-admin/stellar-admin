---
component: RadioGroup
tags: [sa-radio-group, sa-radio-group-item]
generated: true
---

# RadioGroup

A group of radio options bound to a scalar value.

<!-- structure:begin -->
## When to use

Prefer a [radio group](radio-group.md) when related options share one bound property, label, and validation message. Use individual [radio inputs](input.md) when you need to compose the fields and layout yourself.

## Binding and composition

Use `asp-for` for model binding, or `name` for a standalone group. Bound groups derive the full field name from the current template prefix and redisplay ModelState values before model values. Do not combine `asp-for` with `name`, `value`, or `values`. Use either child item tags or `asp-items`; `SelectListGroup` is not supported. Child content supplies the option label (or the choice-card title), and `description` supplies supporting text.

The `value` attributes are strings: use `value="express"` or `value="1"` for literals, and a Razor expression such as `value="@role.Id.ToString()"` for dynamic values. Option values remain strings even when `asp-for` binds to an integer, enum, GUID, or another supported model type; MVC converts submitted values to the model type. Values must be non-null and unique within the group. Display labels and descriptions are HTML-encoded; child Razor markup can supply rich labels. Avoid interactive elements inside option labels.

Both variants reuse the existing Field and Input HTML: a fieldset with a legend, a Field group, and horizontal fields containing the existing radio/checkbox indicators. `ChoiceCard` wraps each field in its label and places the input after its title and description. A group-level `class` applies to the fieldset; an item-level `class` applies to its field (Default) or outer label (ChoiceCard).

Radio groups bind a scalar `string`, enum, `Guid`, `bool`, or numeric primitive (`byte` through `ulong`, `float`, `double`, `decimal`), including nullable value types. Date/time and custom complex types are not supported in this initial API. Formatting and comparison use the current request culture, matching MVC form binding. Enum names and numeric enum submissions resolve to the same selection.

Without `asp-for`, `value` selects an option. With `asp-items`, `SelectListItem.Selected` is used only when neither bound nor explicitly selected. Omit selection to start unchecked. A radio group permits only one selected option. Null is represented by no selection, not an option with a null value.

Use a nullable property with `[Required]` when the server must reject no selection. The group provides no client-side validation and does not generate browser validation constraints from model metadata. Applications can add their own client-side validation or rely on server-side validation. Each group renders one server validation message and propagates error state and description associations to its inputs.

<!-- structure:end -->

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-radio-group>` | A group of radio options bound to a scalar value. |
| `<sa-radio-group-item>` | An option in a radio group. |

## Attributes

### `<sa-radio-group>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `value` | `string` | — | — |
| `variant` | `RadioGroupVariant` | `Default` | `Default`, `ChoiceCard` |
| `description` | `string` | — | — |
| `disabled` | `bool` | — | `true`, `false` |
| `error` | `string` | — | — |
| `asp-for` | `ModelExpression` | — | — |
| `asp-items` | `IEnumerable<SelectListItem>` | — | — |
| `label` | `string` | — | — |
| `name` | `string` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

### `<sa-radio-group-item>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `description` | `string` | — | — |
| `disabled` | `bool` | — | `true`, `false` |
| `value` | `string` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/RadioGroup/_Intro.cshtml`*

```razor
<sa-radio-group name="intro-radio" value="express" label="Delivery">
    <sa-radio-group-item value="standard" description="Arrives in five days">Standard</sa-radio-group-item>
    <sa-radio-group-item value="express" description="Arrives tomorrow">Express</sa-radio-group-item>
</sa-radio-group>
```

*From `Pages/RadioGroup/_ChoiceCards.cshtml`*

```razor
<sa-radio-group name="choicecards-radio" value="express" label="Delivery" variant="RadioGroupVariant.ChoiceCard">
    <sa-radio-group-item value="standard" description="Arrives in five days">Standard</sa-radio-group-item>
    <sa-radio-group-item value="express" description="Arrives tomorrow">Express</sa-radio-group-item>
</sa-radio-group>
```

*From `Pages/RadioGroup/_ModelBinding.cshtml`*

```razor
<sa-radio-group asp-for="DeliveryMethod">
    <sa-radio-group-item value="@Index.Delivery.Standard" description="Arrives in five days">Standard</sa-radio-group-item>
    <sa-radio-group-item value="@Index.Delivery.Express" description="Arrives tomorrow">Express</sa-radio-group-item>
</sa-radio-group>
```

*From `Pages/RadioGroup/_Items.cshtml`*

```razor
<sa-radio-group name="items-radio" label="Options"
    asp-items="@([new SelectListItem("First option", "first", true), new SelectListItem("Second option", "second")])" />
```

*From `Pages/RadioGroup/_Validation.cshtml`*

```razor
<sa-radio-group asp-for="DeliveryMethod">
    <sa-radio-group-item value="Standard" description="Arrives in five days">Standard</sa-radio-group-item>
    <sa-radio-group-item value="Express" description="Arrives tomorrow">Express</sa-radio-group-item>
</sa-radio-group>
```
