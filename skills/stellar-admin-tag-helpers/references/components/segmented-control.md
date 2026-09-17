---
component: SegmentedControl
tags: [sa-segmented-control, sa-segmented-control-item]
generated: true
---

# SegmentedControl

A group of radio buttons styled as a segmented control.

<!-- structure:begin -->
## Composition and behavior

Put `asp-for` on the parent, or use `name` and optional initial `value` for an unbound group. Each child supplies a distinct `value` and label content. Binding takes precedence over the parent's `name` and `value`.

The outer component renders a `div` with `role="radiogroup"`; each item is a label containing a native radio. Supply `aria-label` or `aria-labelledby` for unbound groups. `aria-describedby` on the parent is also applied to each radio. Binding automatically renders the shared field label, description, and validation message. Use `label`, `description`, and `error` for explicit field text. Use `render-field="false"` or nest inside `<sa-field>` for explicit composition.

Attach standard `onchange` or a `change` event listener to the parent. Read `event.target.value`; `event.currentTarget` is the parent. Native changes bubble once per new selection. Initialization, reselecting the current value, reset, and programmatic `checked` assignments do not emit change events. No StellarAdmin JavaScript is required.

Use the component for a single choice from a short list. It has one horizontal appearance matching default tabs, with native radio keyboard behavior. Parent `disabled` disables all options; `required` defaults to model metadata when bound.
<!-- structure:end -->

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-segmented-control>` | A group of radio buttons styled as a segmented control. |
| `<sa-segmented-control-item>` | A labeled radio button within a segmented control. |

## Attributes

### `<sa-segmented-control>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `disabled` | `bool` | — | `true`, `false` |
| `required` | `bool` | — | `true`, `false` |
| `value` | `string` | — | — |
| `description` | `string` | — | — |
| `error` | `string` | — | — |
| `asp-for` | `ModelExpression` | — | — |
| `label` | `string` | — | — |
| `name` | `string` | `Name` | — |
| `render-field` | `bool` | — | `true`, `false` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-segmented-control-item>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `disabled` | `bool` | — | `true`, `false` |
| `value` | `string` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/SegmentedControl/_Intro.cshtml`*

```razor
<sa-segmented-control name="travel-mode" value="Flights" aria-label="Travel mode">
    <sa-segmented-control-item value="Flights">Flights</sa-segmented-control-item>
    <sa-segmented-control-item value="Stays">Stays</sa-segmented-control-item>
    <sa-segmented-control-item value="Cars">Cars</sa-segmented-control-item>
</sa-segmented-control>
```

*From `Pages/SegmentedControl/_Icons.cshtml`*

```razor
<sa-segmented-control name="travel-mode-icons" value="Flights" aria-label="Travel mode">
    <sa-segmented-control-item value="Flights">
        <sa-icon name="plane" aria-hidden="true" />
        Flights
    </sa-segmented-control-item>
    <sa-segmented-control-item value="Stays">
        <sa-icon name="bed-double" aria-hidden="true" />
        Stays
    </sa-segmented-control-item>
    <sa-segmented-control-item value="Cars">
        <sa-icon name="car-front" aria-hidden="true" />
        Cars
    </sa-segmented-control-item>
</sa-segmented-control>
```

*From `Pages/SegmentedControl/_ModelBinding.cshtml`*

```razor
<sa-segmented-control asp-for="Cabin">
    <sa-segmented-control-item value="Economy">Economy</sa-segmented-control-item>
    <sa-segmented-control-item value="Business">Business</sa-segmented-control-item>
    <sa-segmented-control-item value="First">First</sa-segmented-control-item>
</sa-segmented-control>
```

*From `Pages/SegmentedControl/_ChangeEvent.cshtml`*

```razor
<sa-stack>
    <sa-segmented-control name="trip-view" value="List" aria-label="Trip view"
                          onchange="document.getElementById('trip-view-result').textContent = event.target.value">
        <sa-segmented-control-item value="List">List</sa-segmented-control-item>
        <sa-segmented-control-item value="Map">Map</sa-segmented-control-item>
    </sa-segmented-control>
    <p>Selected view: <output id="trip-view-result" aria-live="polite">List</output></p>
</sa-stack>
```
