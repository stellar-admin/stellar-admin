---
component: Slider
tags: [sa-slider, sa-slider-mark, sa-slider-marks, sa-slider-value]
generated: true
---

# Slider

Renders an input that selects a number, or a range of numbers, by dragging thumbs along a track.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-slider>` | Renders an input that selects a number, or a range of numbers, by dragging thumbs along a track. |
| `<sa-slider-mark>` | Renders a mark at one value of a slider. |
| `<sa-slider-marks>` | Renders marks along a slider. |
| `<sa-slider-value>` | Renders the current value of a slider, which updates as the slider moves. |

## Attributes

### `<sa-slider>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `min` | `double` | `0` | — |
| `max` | `double` | `100` | — |
| `step` | `double` | `1` | — |
| `min-distance` | `double` | `0` | — |
| `orientation` | `SliderOrientation` | `Horizontal` | `Horizontal`, `Vertical` |
| `thumb-alignment` | `SliderThumbAlignment` | `Edge` | `Center`, `Edge` |
| `disabled` | `bool` | `false` | `true`, `false` |
| `readonly` | `bool` | `false` | `true`, `false` |
| `value` | `string` | — | — |
| `value-format` | `string` | — | — |
| `thumb-labels` | `string` | — | — |
| `form` | `string` | — | — |
| `class-names` | `SliderClassNames` | — | — |
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

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

### `<sa-slider-mark>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `value` | `double` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-slider-marks>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `interval` | `double` | — | — |
| `labels` | `SliderMarkLabels` | `None` | `None`, `Ends`, `All` |
| `show-ticks` | `bool` | `true` | `true`, `false` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-slider-value>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `for` | `string` | — | — |
| `index` | `int` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/Slider/_Range.cshtml`*

```razor
<sa-slider value="20,80" min="0" max="100" min-distance="10"/>
```

*From `Pages/Slider/_ModelBinding.cshtml`*

```razor
<sa-slider asp-for="MaximumDistanceFromCenter" max="100"/>
<sa-slider asp-for="PricePerNight" min="0" max="1000" step="50"/>
<sa-slider asp-for="GuestRatingBands" min="0" max="100" min-distance="5"/>
```

*From `Pages/Slider/_Value.cshtml`*

```razor
<sa-field>
    <div class="flex items-center justify-between gap-2">
        <sa-field-label asp-for="PricePerNight"/>
        <sa-slider-value/>
    </div>
    <sa-slider asp-for="PricePerNight" max="1000" step="50" value-format="${0}"
               thumb-labels="Minimum price,Maximum price"/>
    <sa-field-description asp-for="PricePerNight"/>
</sa-field>
<sa-field>
    <sa-field-label asp-for="MaximumDistanceFromCenter"/>
    <sa-slider asp-for="MaximumDistanceFromCenter" max="50" value-format="{0} km"/>
    <sa-field-description>
        Stays within <sa-slider-value/> of the city center.
    </sa-field-description>
</sa-field>
```

*From `Pages/Slider/_CustomMarks.cshtml`*

```razor
<sa-slider id="minimum-guest-rating" value="4" min="1" max="5">
    <sa-slider-marks>
        <sa-slider-mark value="1">Poor</sa-slider-mark>
        <sa-slider-mark value="3">Good</sa-slider-mark>
        <sa-slider-mark value="5">Excellent</sa-slider-mark>
    </sa-slider-marks>
</sa-slider>
<p class="text-sm">
    Showing stays rated <sa-slider-value for="minimum-guest-rating"/> stars or higher.
</p>
```
