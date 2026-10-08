# Chip

Status: **proposed**. The API design below awaits review; no phase has started. Last updated: 2026-10-08.

Add `sa-chip` to `StellarAdmin.TagHelpers`: a compact token for a selected or entered value, with optional media and an optional remove button, that works on its own, inside an input-styled group and read-only, in all fifteen themes. Work follows the [port-shadcn-component](../../../.agents/skills/port-shadcn-component/SKILL.md) workflow and the [prototype-component](../../../.agents/skills/prototype-component/SKILL.md) skill, in phases with a review after each; approval of one phase does not authorize the next.

## Why

The Dashboard has two chip looks, built two ways, and neither is a library component:

- The [multi-select lookup editor](../multi-lookup-editor.md) renders each chip as an `sa-badge` with a plain `<button>` for Remove and `.sa-choice-media` for its media. The badge takes the theme's radius, but the Remove button (`rounded-sm`), the code box (`rounded-sm bg-muted`) and the avatar (round) have fixed shapes and colours. In Parallax the Remove button is square inside a pill and the code has almost no contrast with the chip; in Aurora the round avatars sit in square chips.
- `ToggleGroupEditor`'s chips appearance uses real `sa-toggle-group-item`s, but the Dashboard CSS forces a pill radius in every theme (`border-radius: calc(infinity * 1px)`) and uses the same fixed-shape media.

Neither matches the other within a theme, and both need Dashboard CSS for something every theme should decide.

## Research

shadcn has no standalone chip. Its only chip is part of the base-ui Combobox: `ComboboxChips` (an input-styled box), `ComboboxChip`, a remove button that is a real `Button` (`variant="ghost" size="icon-xs"`), and `ComboboxChipsInput`, built on Base UI's `Combobox.Chips`, `Combobox.Chip` and `Combobox.ChipRemove`. Upstream's Base UI source:

```tsx
<ComboboxPrimitive.Chips data-slot="combobox-chips" className={cn("cn-combobox-chips", className)} />
<ComboboxPrimitive.Chip data-slot="combobox-chip"
  className={cn("cn-combobox-chip has-disabled:pointer-events-none has-disabled:cursor-not-allowed has-disabled:opacity-50", className)}>
  {children}
  <ComboboxPrimitive.ChipRemove render={<Button variant="ghost" size="icon-xs" />}
    className="cn-combobox-chip-remove" data-slot="combobox-chip-remove">
    <XIcon className="cn-combobox-chip-indicator-icon pointer-events-none" />
  </ComboboxPrimitive.ChipRemove>
</ComboboxPrimitive.Chip>
```

The generated shadcn theme files already contain `.sa-combobox-chips`, `.sa-combobox-chip` and `.sa-combobox-chip-remove`. Each style sets the chip against its own input surface:

| Style | Input background | Chip background | Chip radius |
| --- | --- | --- | --- |
| Luma | `bg-input/50` | `bg-input` | `rounded-3xl` |
| Rhea | `bg-input/50` | `bg-input` | `rounded-2xl` |
| Mira | `bg-input/20` | `bg-muted-foreground/10` | `rounded-[calc(var(--radius-sm)-2px)]` |
| Nova | transparent | `bg-muted` | `rounded-sm` |
| Vega | transparent | `bg-muted` | `rounded-sm` |

The custom themes have no rules for them. Upstream designs the chip only for the inside of its chips box: no standalone use, no media, no read-only state.

Shadcn community registries build tag inputs (Emblor, Origin UI, shadcn-tag-input, Kibo UI Tags) rather than a chip component; Creative Tim publishes a standalone shadcn Chip with variants, sizes, icons, dismiss and an avatar.

Other systems:

| Library | Component | Relevant design |
| --- | --- | --- |
| Chakra v3 | Tag | `Root`, `StartElement`, `Label`, `EndElement`, `CloseTrigger` |
| Mantine | Pill, `Pill.Group`, PillsInput | Pill is designed to sit inside inputs; Mantine's Chip is a separate checkbox-style toggle |
| HeroUI v3 | Chip | Composition: an Avatar as first child, a CloseButton |
| React Aria | TagGroup, Tag | `<Button slot="remove">`; the group owns keyboard navigation, Backspace removal and selection |
| Fluent 2 | Tag, InteractionTag, TagGroup | Avatar or icon media, with the avatar shape as a setting |
| Carbon | Tag | Read-only (not focusable), dismissible (focus on the close button), selectable, operational |
| Material 3 | Chips | Assist, filter, input and suggestion; our case is the input chip, always in a set |
| Atlassian | Tag vs Lozenge | Removable tags apart from static status lozenges; tags moved to an outlined style |
| Ant Design | Tag | `icon`, `closable`, `closeIcon`, `bordered` |

Conclusions:

- Static labels (Badge, Lozenge) and removable tokens (Tag, Pill, Chip, Token) are separate components almost everywhere, so `sa-chip` is new rather than an `sa-badge` variant.
- Media at the start, the label, and a remove part at the end, as a real button, is the common anatomy, and matches upstream.
- The chip's fill depends on what it sits on, as upstream's per-style rules and Mantine's Pill show.
- Media shape follows the chip's shape, as Fluent's avatar shape setting shows.
- Selectable (filter) chips stay with `sa-toggle` and `sa-toggle-group`, as Mantine's Chip and Material's filter chips are separate from input chips.

## Proposed API

### Tags

| Tag | Renders | Purpose |
| --- | --- | --- |
| `sa-chip` | `<span data-slot="chip">` | The token: optional media, label text, and an optional remove button |
| `sa-chip-remove` | `<button type="button" data-slot="chip-remove">` | The remove button, styled as a ghost icon button; renders an `x` icon when it has no content |
| `sa-chip-group` | `<div data-slot="chip-group" data-appearance="…">` | Lays out chips and the controls beside them, and sets the surface the chips sit on |

### Enums

- `ChipGroupAppearance`: `Plain` (default), a wrapping row on whatever background contains it; `Input`, the input-styled box with border, input background, focus-within ring and invalid ring, as upstream's `ComboboxChips`.

### Attributes

- `sa-chip`: `disabled` (bool), which disables its remove button and dims the chip, as upstream's `has-disabled:` rules.
- `sa-chip-remove`: no attributes of its own; `aria-label` is required, and the tag helper throws without it, so every remove button is named ("Remove Lisbon").
- `sa-chip-group`: `appearance`, `invalid` (bool, sets `aria-invalid` for the invalid ring), `disabled` (bool, disables every chip's remove button).

There is no size enum in the first version: the chip has upstream's height (about 21px), which fits inside an input. A larger size can follow if a use needs one.

### Markup

```cshtml
<sa-chip-group appearance="ChipGroupAppearance.Input" aria-labelledby="destinations-label">
    <sa-chip>
        <code>LIS</code>
        Lisbon
        <sa-chip-remove aria-label="Remove Lisbon" />
    </sa-chip>
    <sa-chip>
        <sa-avatar name="Elena Marchetti" />
        Elena Marchetti
        <sa-chip-remove aria-label="Remove Elena Marchetti" />
    </sa-chip>
    <sa-chip>
        <sa-icon name="plane" />
        Flights
        <sa-chip-remove aria-label="Remove Flights" />
    </sa-chip>
    <sa-chip>
        <img src="/images/kyoto.jpg" alt="" />
        Kyoto
        <sa-chip-remove aria-label="Remove Kyoto" />
    </sa-chip>
    <sa-button variant="ButtonVariant.Ghost" size="ButtonSize.ExtraSmall">
        <sa-icon name="plus" />
        Add destination
    </sa-button>
</sa-chip-group>

@* Read-only: no remove buttons, on the page's own background *@
<sa-chip-group role="list" aria-labelledby="fixed-destinations-label">
    <sa-chip role="listitem">
        <code>CDG</code>
        Paris
    </sa-chip>
</sa-chip-group>
```

### Surfaces and contrast

The chip reads its fill, text colour and border from CSS variables (`--sa-chip-bg`, `--sa-chip-fg`, `--sa-chip-border`) with a standalone value; `sa-chip-group` with the `Input` appearance overrides them for the input surface. One chip works on a page, on a card and inside the input box without variants. For the shadcn styles, the input-surface values are upstream's `combobox-chip` values; the standalone values are new and set per theme.

Every theme must meet these on every surface (page, card, input box, read-only), in light and dark:

- Label text against the chip fill: at least 4.5:1.
- The remove icon against the chip fill, at rest: at least 3:1. Upstream's `opacity-50` may fail this and is checked.
- The chip against its surface: visibly distinct by fill or border. This is judged in the contact sheet; the measured fill difference is reported.

### Media

Media goes directly in the chip, before the label, with no wrapper or variant, as `sa-badge` and `sa-button` take icons. Each theme styles the chip's direct children by element:

- `> svg`: an icon, at about 14 to 16px.
- `> [data-slot=avatar]`: an `sa-avatar`, with or without a photo, sized to fit the chip. The chip's rule is more specific than the avatar's size class, so it wins; author utilities still win over both.
- `> img`: an image, sized like an avatar and covering its box. It is decorative beside the label, so `alt=""`.
- `> code`: a code, in the native `<code>` element. Its treatment (a filled box, an outline, or muted monospace text without a box) is decided in the prototype, with contrast checked on every surface.

The media's radius follows the chip's, inset by the chip's padding, so a round chip has round media and a square chip square media. Media must come before the label for the spacing to work, as with icons in a badge or button; the docs show the order. One kind of media per chip; other elements are left unsized. `ClassNames.Media` overrides from the Dashboard land on the media element itself, and author utilities beat component layers.

A `sa-chip-media` wrapper with a `ChipMediaVariant` was considered and dropped (2026-10-08): unlike shadcn's `ItemMedia` and `EmptyMedia`, a chip's media needs no frame of its own, so a variant would only repeat what the element already says, and native `<code>` names a code where a variant would leave it as bare text.

### Behaviour and accessibility

- No script. Removing a chip is the author's job (the Dashboard's editor element already does it); keyboard navigation between chips and Backspace removal belong to a later combobox, as React Aria's TagGroup and Base UI's Combobox own them.
- Only the remove button is focusable, as Carbon's dismissible tag. A chip without one is not in the tab order.
- The group sets no role by default: an input-styled group usually holds an Add button, which a list would not allow. Docs show `role="list"` and `role="listitem"` for a read-only set.
- `invalid` on the group sets `aria-invalid="true"`, which the `Input` appearance styles like an invalid input.

### Upstream deviations

- A standalone chip, a plain group, media styling and a read-only state: upstream has none of them.
- `data-slot` names are `chip`, `chip-remove` and `chip-group`, not `combobox-*`, because the chip is not part of a combobox here. A later `sa-combobox` reuses these parts.
- No chips input part (`ComboboxChipsInput`) until a combobox needs one.

### Toggle group chips

`ToggleGroupEditor`'s chips appearance stays a toggle group, because its chips are selectable, not removable. Its forced pill radius and fixed media shapes are replaced so it reads as the same family as `sa-chip` in each theme: the radius and media shape come from the theme's chip treatment instead of Dashboard CSS. How (a shared radius variable, or a toggle-group chips appearance in TagHelpers) is settled in phase 1 with the prototype in front of us.

## Open decisions

- How the shadcn styles get their chip rules without re-running ThemeGenerator: per-theme `custom.css` additions that map upstream's `combobox-chip` values onto `sa-chip` selectors, or a ThemeGenerator mapping as its own task. Recommended: `custom.css`, recorded per theme.
- The code treatment inside a chip.
- Whether the toggle-group chips get a TagHelpers appearance or only a shared variable.

## Phases

| Phase | Scope | Status |
| --- | --- | --- |
| 0 | API design (this document) | awaiting review |
| 1 | Prototype and contrast check | not started |
| 2 | CSS, tag helpers, custom-theme coverage and tests | not started |
| 3 | DocsSamples demos, website docs and consumer skills reference | not started |
| 4 | Dashboard adoption | not started |

### Phase 1: prototype and contrast check

- `sandbox/html/chip.html`, per the prototype-component skill: chips with every media kind, with and without remove, disabled, on the page, on a card, inside an `Input` group (including invalid and focused), and read-only; plus toggle-group chips beside them for comparison.
- A capture script, built on the Multi lookup contact sheet, that renders the page in all fifteen themes, light and dark, into one grid, and measures the contrast pairs above, flagging any below the threshold.
- Settle the open decisions with the grid in front of us.

### Phase 2: CSS, tag helpers and tests

- Structural rules in `Client/css/components.css`; themed rules for the eight shadcn styles from upstream's values; hand-authored rules for Aurora, Concourse, Ice, Ledger, Meridian, Observatory and Parallax, recorded in their theme specifications; `util/theme-coverage/coverage.json` reviewed for every new hook.
- `TagHelpers/Chip/`: the three tag helpers and `ChipGroupAppearance`, with XML docs and TagHelpers unit tests.
- Re-run the capture and contrast check against the built bundles.

### Phase 3: docs

- DocsSamples pages in Voyager Travel content: intro, media, removable, in an input group, read-only, disabled, invalid.
- Website docs page and exports, per [website integration](../../repos/website.md); the consumer skills reference regenerated.

### Phase 4: Dashboard adoption

- The multi-select lookup editor renders its chips, editable and read-only, with `sa-chip-group`, `sa-chip` and `sa-chip-remove`, its media rendered directly in the chip, and its Add and Choose buttons with `sa-button` or `sa-input-group-button` instead of plain buttons.
- The toggle-group chips appearance follows the chip treatment, and the Dashboard's pill rule goes.
- The Multi lookup and Toggle group contact sheets re-run across all themes.

## Source paths

- Tag helpers: `src/StellarAdmin.TagHelpers/TagHelpers/Chip/` (new).
- Styles: `src/StellarAdmin.TagHelpers/Client/css/components.css`, `src/StellarAdmin.TagHelpers/Client/css/themes/*.css`, `util/ThemeGenerator/Themes/*.custom.css`.
- Coverage: `util/theme-coverage/coverage.json`.
- Dashboard: `src/StellarAdmin.Dashboard/Areas/StellarAdmin/Views/Shared/_MultiLookupItems.cshtml`, `EditorTemplates/Editors/MultiLookupSheet.cshtml`, `EditorTemplates/Editors/ToggleGroup.cshtml`, `src/StellarAdmin.Dashboard/Client/css/client.css`.

## Phase log

- 2026-10-08: research and API proposal written.
- 2026-10-08: media goes directly in the chip, styled by element; `sa-chip-media` and `ChipMediaVariant` dropped.
