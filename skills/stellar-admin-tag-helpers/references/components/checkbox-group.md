---
component: CheckboxGroup
tags: [sa-checkbox-group, sa-checkbox-group-item]
generated: true
---

# CheckboxGroup

A group of checkbox options bound to a collection.

<!-- structure:begin -->
## When to use

Prefer a [checkbox group](checkbox-group.md) when selected options should bind to one array or collection. Use an individual [checkbox input](input.md) for a boolean property.

## Binding and composition

Use `asp-for` for model binding, or `name` for a standalone group. Bound groups derive the full field name from the current template prefix and redisplay ModelState values before model values. Do not combine `asp-for` with `name`, `value`, or `values`. Use either child item tags or `asp-items`; `SelectListGroup` is not supported. Child content supplies the option label (or the choice-card title), and `description` supplies supporting text.

The `value` attributes are strings: use `value="express"` or `value="1"` for literals, and a Razor expression such as `value="@role.Id.ToString()"` for dynamic values. Option values remain strings even when `asp-for` binds to an integer, enum, GUID, or another supported model type; MVC converts submitted values to the model type. Values must be non-null and unique within the group. Display labels and descriptions are HTML-encoded; child Razor markup can supply rich labels. Avoid interactive elements inside option labels.

Both variants reuse the existing Field and Input HTML: a fieldset with a legend, a Field group, and horizontal fields containing the existing radio/checkbox indicators. `ChoiceCard` wraps each field in its label and places the input after its title and description. A group-level `class` applies to the fieldset; an item-level `class` applies to its field (Default) or outer label (ChoiceCard).

Checkbox groups bind one-dimensional `T[]`, `List<T>`, `IList<T>`, `ICollection<T>`, `IEnumerable<T>`, `IReadOnlyList<T>`, or `IReadOnlyCollection<T>` properties. Use writable properties. Elements support `string`, enums, `Guid`, `bool`, and numeric primitives; nullable value-type elements, binary `byte[]`, date/time, custom complex types, sets and immutable collections are not supported. Flags enums are individual collection values; the group does not combine selections into one flags scalar. Formatting and comparison use the current request culture.

Without `asp-for`, use `values="@(new[] { "insurance", "gift" })"`. `asp-items` uses `SelectListItem.Selected` only when no explicit selection is supplied. An empty `values` collection explicitly selects nothing. All checkbox inputs share one field name and submit their own values. There are no boolean `false` hidden inputs.

Register `services.AddStellarAdmin().AddTagHelpers()` to enable empty-selection binding. A separate hidden presence marker lets MVC clear an existing collection when every checkbox is unchecked, including nested properties. Keep that marker when constructing AJAX submissions (ordinary `FormData(form)` includes it). A disabled group omits its marker, and a fieldset disabled in the browser excludes it from form submission. Individual disabled options follow native HTML behavior and do not submit their values; they are not automatically preserved by the server.

Use `[MinLength(1)]` to reject an empty collection on the server (`[Required]` alone does not do that), and `[MaxLength]` to limit the number of selections. The group provides no client-side validation and does not generate browser validation constraints from model metadata. Applications can add their own client-side validation or rely on server-side validation. Each group renders one server validation message and propagates error state and description associations to its inputs.

<!-- structure:end -->

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-checkbox-group>` | A group of checkbox options bound to a collection. |
| `<sa-checkbox-group-item>` | An option in a checkbox group. |

## Attributes

### `<sa-checkbox-group>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `values` | `IEnumerable<string>` | — | — |
| `variant` | `CheckboxGroupVariant` | `Default` | `Default`, `ChoiceCard` |
| `description` | `string` | — | — |
| `disabled` | `bool` | — | `true`, `false` |
| `error` | `string` | — | — |
| `asp-for` | `ModelExpression` | — | — |
| `asp-items` | `IEnumerable<SelectListItem>` | — | — |
| `label` | `string` | — | — |
| `name` | `string` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

### `<sa-checkbox-group-item>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `description` | `string` | — | — |
| `disabled` | `bool` | — | `true`, `false` |
| `value` | `string` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/CheckboxGroup/_Intro.cshtml`*

```razor
<sa-checkbox-group name="intro-checkbox" values="@(["insurance"])" label="Extras">
    <sa-checkbox-group-item value="insurance" description="Protection for your parcel">Insurance</sa-checkbox-group-item>
    <sa-checkbox-group-item value="gift" description="A reusable gift box">Gift packaging</sa-checkbox-group-item>
</sa-checkbox-group>
```

*From `Pages/CheckboxGroup/_ChoiceCards.cshtml`*

```razor
<sa-checkbox-group name="choicecards-checkbox" values="@(["insurance"])" label="Extras" variant="CheckboxGroupVariant.ChoiceCard">
    <sa-checkbox-group-item value="insurance" description="Protection for your parcel">Insurance</sa-checkbox-group-item>
    <sa-checkbox-group-item value="gift" description="A reusable gift box">Gift packaging</sa-checkbox-group-item>
</sa-checkbox-group>
```

*From `Pages/CheckboxGroup/_CollectionBinding.cshtml`*

```razor
<sa-checkbox-group asp-for="Extras">
    <sa-checkbox-group-item value="1" description="Protection for your parcel">Insurance</sa-checkbox-group-item>
    <sa-checkbox-group-item value="2" description="A reusable gift box">Gift packaging</sa-checkbox-group-item>
    <sa-checkbox-group-item value="3" disabled="true">Same-day delivery (unavailable)</sa-checkbox-group-item>
</sa-checkbox-group>
```

*From `Pages/CheckboxGroup/_Items.cshtml`*

```razor
<sa-checkbox-group name="items-checkbox" label="Options"
    asp-items="@([new SelectListItem("First option", "first", true), new SelectListItem("Second option", "second")])" />
```

*From `Pages/CheckboxGroup/_Validation.cshtml`*

```razor
<sa-checkbox-group asp-for="Extras">
    <sa-checkbox-group-item value="1" description="Protection for your parcel">Insurance</sa-checkbox-group-item>
    <sa-checkbox-group-item value="2" description="A reusable gift box">Gift packaging</sa-checkbox-group-item>
</sa-checkbox-group>
```
