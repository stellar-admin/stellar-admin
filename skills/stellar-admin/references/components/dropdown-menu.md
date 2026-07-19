---
component: DropdownMenu
tags: [sa-dropdown-menu, sa-dropdown-menu-checkbox-item, sa-dropdown-menu-content, sa-dropdown-menu-group, sa-dropdown-menu-item, sa-dropdown-menu-label, sa-dropdown-menu-radio-group, sa-dropdown-menu-radio-item, sa-dropdown-menu-separator, sa-dropdown-menu-shortcut, sa-dropdown-menu-sub, sa-dropdown-menu-sub-content, sa-dropdown-menu-sub-trigger, sa-dropdown-menu-trigger]
generated: true
---

# DropdownMenu

The root of a dropdown menu, pairing a trigger with its content and generating the shared id that links them.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-dropdown-menu>` | The root of a dropdown menu, pairing a trigger with its content and generating the shared id that links them. |
| `<sa-dropdown-menu-checkbox-item>` | A menu item with a checkable state, rendered as a `<div role="menuitemcheckbox">` with a check indicator. |
| `<sa-dropdown-menu-content>` | The popover panel that holds a dropdown menu's items, positioned relative to its trigger. |
| `<sa-dropdown-menu-group>` | Groups related menu items together, exposed to assistive technology as a `role="group"`. |
| `<sa-dropdown-menu-item>` | A selectable menu item. Renders as a `<div role="menuitem">`, or as an `<a role="menuitem">` when the author supplies a URL — either a raw `href` or ASP.NET routing attributes (`asp-page`, `asp-action`/`asp-controller`, `asp-route-*`, …). The `sel-dropdown-menu` web component activates items by `role`, so both elements behave identically. |
| `<sa-dropdown-menu-label>` | A non-interactive label used to caption a section of menu items. |
| `<sa-dropdown-menu-radio-group>` | Groups `sa-dropdown-menu-radio-item` children into a single-selection set and tracks which value is selected. |
| `<sa-dropdown-menu-radio-item>` | A single option within a `sa-dropdown-menu-radio-group`, rendered as a `<div role="menuitemradio">` with a selection indicator. |
| `<sa-dropdown-menu-separator>` | A horizontal divider that visually separates groups of menu items. |
| `<sa-dropdown-menu-shortcut>` | Displays a keyboard shortcut hint, aligned to the trailing edge of a menu item. |
| `<sa-dropdown-menu-sub>` | Wraps a submenu, pairing a `sa-dropdown-menu-sub-trigger` with its `sa-dropdown-menu-sub-content` and generating the shared id that links them. |
| `<sa-dropdown-menu-sub-content>` | The popover panel that holds a submenu's items, positioned relative to its sub-trigger. |
| `<sa-dropdown-menu-sub-trigger>` | The menu item that opens a submenu, rendered with a trailing chevron and wired to its sub-content panel. |
| `<sa-dropdown-menu-trigger>` | The button that toggles a dropdown menu and anchors its content, styled with button variant and size options. |

## Attributes

### `<sa-dropdown-menu-checkbox-item>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `checked` | `bool` | `false` | `true`, `false` |
| `close-on-click` | `bool` | — | `true`, `false` |
| `disabled` | `bool` | — | `true`, `false` |
| `inset` | `bool` | — | `true`, `false` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

### `<sa-dropdown-menu-content>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `position` | `PositionArea` | `BottomSpanRight` | `TopCenter`, `TopSpanLeft`, `TopSpanRight`, `Top`, `LeftCenter`, `LeftSpanTop`, `LeftSpanBottom`, `Left`, `BottomCenter`, `BottomSpanLeft`, `BottomSpanRight`, `Bottom`, `RightCenter`, `RightSpanTop`, `RightSpanBottom`, `Right`, `TopLeft`, `TopRight`, `BottomLeft`, `BottomRight` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-dropdown-menu-item>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `close-on-click` | `bool` | — | `true`, `false` |
| `disabled` | `bool` | — | `true`, `false` |
| `href` | `string` | — | — |
| `inset` | `bool` | — | `true`, `false` |
| `variant` | `DropdownMenuItemVariant` | `Default` | `Default`, `Destructive` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-dropdown-menu-label>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `inset` | `bool` | — | `true`, `false` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-dropdown-menu-radio-group>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `value` | `string` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-dropdown-menu-radio-item>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `close-on-click` | `bool` | — | `true`, `false` |
| `disabled` | `bool` | — | `true`, `false` |
| `value` | `string` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-dropdown-menu-sub-content>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `position` | `PositionArea` | `RightSpanBottom` | `TopCenter`, `TopSpanLeft`, `TopSpanRight`, `Top`, `LeftCenter`, `LeftSpanTop`, `LeftSpanBottom`, `Left`, `BottomCenter`, `BottomSpanLeft`, `BottomSpanRight`, `Bottom`, `RightCenter`, `RightSpanTop`, `RightSpanBottom`, `Right`, `TopLeft`, `TopRight`, `BottomLeft`, `BottomRight` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-dropdown-menu-sub-trigger>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `inset` | `bool` | — | `true`, `false` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-dropdown-menu-trigger>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `size` | `ButtonSize` | `Default` | `Default`, `ExtraSmall`, `Small`, `Large`, `Icon`, `IconExtraSmall`, `IconSmall`, `IconLarge` |
| `variant` | `ButtonVariant` | `Outline` | `Default`, `Destructive`, `Outline`, `Secondary`, `Ghost`, `Link` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/DropdownMenu/_Intro.cshtml`*

```razor
<sa-dropdown-menu>
    <sa-dropdown-menu-trigger variant="ButtonVariant.Outline">
        <sa-icon name="circle-user-round" class="text-muted-foreground"/>
        Ibn Battuta
    </sa-dropdown-menu-trigger>
    <sa-dropdown-menu-content class="w-56">
        <sa-dropdown-menu-label>My Account</sa-dropdown-menu-label>
        <sa-dropdown-menu-separator/>
        <sa-dropdown-menu-group>
            <sa-dropdown-menu-item>
                <sa-icon name="user"/>
                Profile
                <sa-dropdown-menu-shortcut>⇧⌘P</sa-dropdown-menu-shortcut>
            </sa-dropdown-menu-item>
            <sa-dropdown-menu-item>
                <sa-icon name="luggage"/>
                My Bookings
                <sa-dropdown-menu-shortcut>⌘B</sa-dropdown-menu-shortcut>
            </sa-dropdown-menu-item>
            <sa-dropdown-menu-item>
                <sa-icon name="settings"/>
                Settings
                <sa-dropdown-menu-shortcut>⌘,</sa-dropdown-menu-shortcut>
            </sa-dropdown-menu-item>
        </sa-dropdown-menu-group>
        <sa-dropdown-menu-separator/>
        <sa-dropdown-menu-item variant="DropdownMenuItemVariant.Destructive">
            <sa-icon name="log-out"/>
            Log out
            <sa-dropdown-menu-shortcut>⇧⌘Q</sa-dropdown-menu-shortcut>
        </sa-dropdown-menu-item>
    </sa-dropdown-menu-content>
</sa-dropdown-menu>
```

*From `Pages/DropdownMenu/_CheckboxItems.cshtml`*

```razor
<sa-dropdown-menu>
    <sa-dropdown-menu-trigger variant="ButtonVariant.Outline">
        <sa-icon name="sliders-horizontal" class="text-muted-foreground"/>
        Trip Filters
    </sa-dropdown-menu-trigger>
    <sa-dropdown-menu-content class="w-56">
        <sa-dropdown-menu-label>Show Categories</sa-dropdown-menu-label>
        <sa-dropdown-menu-separator/>
        <sa-dropdown-menu-checkbox-item checked="true">Flights</sa-dropdown-menu-checkbox-item>
        <sa-dropdown-menu-checkbox-item checked="true">Accommodation</sa-dropdown-menu-checkbox-item>
        <sa-dropdown-menu-checkbox-item>Car Rental</sa-dropdown-menu-checkbox-item>
    </sa-dropdown-menu-content>
</sa-dropdown-menu>
```

*From `Pages/DropdownMenu/_Submenu.cshtml`*

```razor
<sa-dropdown-menu>
    <sa-dropdown-menu-trigger variant="ButtonVariant.Outline">
        <sa-icon name="ellipsis" class="text-muted-foreground"/>
        Trip Actions
    </sa-dropdown-menu-trigger>
    <sa-dropdown-menu-content class="w-56">
        <sa-dropdown-menu-item>
            <sa-icon name="ticket"/>
            View Tickets
        </sa-dropdown-menu-item>
        <sa-dropdown-menu-item>
            <sa-icon name="calendar-plus"/>
            Add to Calendar
        </sa-dropdown-menu-item>
        <sa-dropdown-menu-separator/>
        <sa-dropdown-menu-sub>
            <sa-dropdown-menu-sub-trigger>
                <sa-icon name="map-pin"/>
                Add Destination
            </sa-dropdown-menu-sub-trigger>
            <sa-dropdown-menu-sub-content class="w-44">
                <sa-dropdown-menu-item>Paris</sa-dropdown-menu-item>
                <sa-dropdown-menu-item>Bangkok</sa-dropdown-menu-item>
                <sa-dropdown-menu-item>Kyoto</sa-dropdown-menu-item>
                <sa-dropdown-menu-item>Cape Town</sa-dropdown-menu-item>
            </sa-dropdown-menu-sub-content>
        </sa-dropdown-menu-sub>
        <sa-dropdown-menu-separator/>
        <sa-dropdown-menu-item variant="DropdownMenuItemVariant.Destructive">
            <sa-icon name="trash-2"/>
            Cancel Trip
        </sa-dropdown-menu-item>
    </sa-dropdown-menu-content>
</sa-dropdown-menu>
```
