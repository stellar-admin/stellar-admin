---
component: Chip
tags: [sa-chip, sa-chip-group, sa-chip-remove]
generated: true
---

# Chip

A compact token for a selected or entered value, with optional media before its label and an optional remove button after it.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-chip>` | A compact token for a selected or entered value, with optional media before its label and an optional remove button after it. |
| `<sa-chip-group>` | Lays out chips and the controls beside them, on the page or in an input-styled box. |
| `<sa-chip-remove>` | The button that removes a chip, placed after its label. It renders a close icon when it has no content, and requires an `aria-label` that names the chip, such as "Remove Lisbon". |

## Attributes

### `<sa-chip>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `disabled` | `bool` | — | `true`, `false` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

### `<sa-chip-group>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `appearance` | `ChipGroupAppearance` | `Plain` | `Plain`, `Input` |
| `disabled` | `bool` | — | `true`, `false` |
| `invalid` | `bool` | — | `true`, `false` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/Chip/_Intro.cshtml`*

```razor
<sa-field>
    <sa-field-title id="trip-destinations-label">Destinations</sa-field-title>
    <sa-chip-group appearance="ChipGroupAppearance.Input" aria-labelledby="trip-destinations-label">
        <sa-chip>
            Lisbon
            <sa-chip-remove aria-label="Remove Lisbon"/>
        </sa-chip>
        <sa-chip>
            Kyoto
            <sa-chip-remove aria-label="Remove Kyoto"/>
        </sa-chip>
        <sa-chip>
            Cape Town
            <sa-chip-remove aria-label="Remove Cape Town"/>
        </sa-chip>
        <sa-button variant="ButtonVariant.Ghost" size="ButtonSize.ExtraSmall">
            <sa-icon name="plus"/>
            Add destination
        </sa-button>
    </sa-chip-group>
</sa-field>
```

*From `Pages/Chip/_Media.cshtml`*

```razor
<sa-chip-group>
    <sa-chip>
        <sa-icon name="plane"/>
        Flights
    </sa-chip>
    <sa-chip>
        <sa-avatar src="/avatars/avatar-1.jpg"/>
        Elena Marchetti
    </sa-chip>
    <sa-chip>
        <sa-avatar name="Priya Nair"/>
        Priya Nair
    </sa-chip>
    <sa-chip>
        <img src="/cities/kyoto.jpg" alt=""/>
        Kyoto
    </sa-chip>
    <sa-chip>
        <code>CPT</code>
        Cape Town
    </sa-chip>
</sa-chip-group>
```

*From `Pages/Chip/_Removable.cshtml`*

```razor
<sa-field>
    <sa-field-title id="tour-filters-label">Filters</sa-field-title>
    <sa-chip-group aria-labelledby="tour-filters-label">
        <sa-chip>
            <sa-icon name="map-pin"/>
            Southern Europe
            <sa-chip-remove aria-label="Remove Southern Europe"/>
        </sa-chip>
        <sa-chip>
            <sa-icon name="calendar"/>
            May to June
            <sa-chip-remove aria-label="Remove May to June"/>
        </sa-chip>
        <sa-chip>
            <sa-icon name="users"/>
            2 travellers
            <sa-chip-remove aria-label="Remove 2 travellers"/>
        </sa-chip>
        <sa-button variant="ButtonVariant.Ghost" size="ButtonSize.ExtraSmall">
            Clear all
        </sa-button>
    </sa-chip-group>
</sa-field>
```

*From `Pages/Chip/_ReadOnly.cshtml`*

```razor
<sa-field>
    <sa-field-title id="booking-travellers-label">Travellers</sa-field-title>
    <sa-chip-group role="list" aria-labelledby="booking-travellers-label">
        <sa-chip role="listitem">
            <sa-avatar src="/avatars/avatar-1.jpg"/>
            Elena Marchetti
        </sa-chip>
        <sa-chip role="listitem">
            <sa-avatar src="/avatars/avatar-2.jpg"/>
            Tomás Herrera
        </sa-chip>
        <sa-chip role="listitem">
            <sa-avatar name="Priya Nair"/>
            Priya Nair
        </sa-chip>
    </sa-chip-group>
</sa-field>
```

*From `Pages/Chip/_Invalid.cshtml`*

```razor
<sa-field>
    <sa-field-title id="tour-stops-label">Tour stops</sa-field-title>
    <sa-chip-group
        appearance="ChipGroupAppearance.Input"
        invalid="true"
        aria-labelledby="tour-stops-label"
        aria-describedby="tour-stops-error"
    >
        <sa-chip>
            Chiang Mai
            <sa-chip-remove aria-label="Remove Chiang Mai"/>
        </sa-chip>
        <sa-button variant="ButtonVariant.Ghost" size="ButtonSize.ExtraSmall">
            <sa-icon name="plus"/>
            Add stop
        </sa-button>
    </sa-chip-group>
    <sa-field-error id="tour-stops-error">Choose at least two stops for a multi-city tour.</sa-field-error>
</sa-field>
```
