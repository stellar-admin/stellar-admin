---
component: DropdownMenu
tags: [sa-dropdown-menu, sa-dropdown-menu-checkbox-item, sa-dropdown-menu-content, sa-dropdown-menu-group, sa-dropdown-menu-item, sa-dropdown-menu-item-link, sa-dropdown-menu-label, sa-dropdown-menu-radio-group, sa-dropdown-menu-radio-item, sa-dropdown-menu-separator, sa-dropdown-menu-shortcut, sa-dropdown-menu-sub, sa-dropdown-menu-sub-content, sa-dropdown-menu-sub-trigger, sa-dropdown-menu-trigger]
generated: true
---

# DropdownMenu

Renders a dropdown menu, which pairs a trigger with its content.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-dropdown-menu>` | Renders a dropdown menu, which pairs a trigger with its content. |
| `<sa-dropdown-menu-checkbox-item>` | Renders a dropdown menu item that can be checked and unchecked. |
| `<sa-dropdown-menu-content>` | Renders the panel that holds a dropdown menu's items. |
| `<sa-dropdown-menu-group>` | Groups related items in a dropdown menu. |
| `<sa-dropdown-menu-item>` | Renders a dropdown menu item that performs an action. |
| `<sa-dropdown-menu-item-link>` | Renders a dropdown menu item that links to another page. |
| `<sa-dropdown-menu-label>` | Renders a label for a section of dropdown menu items. |
| `<sa-dropdown-menu-radio-group>` | Groups `sa-dropdown-menu-radio-item` elements so that only one can be selected. |
| `<sa-dropdown-menu-radio-item>` | Renders an option in an `sa-dropdown-menu-radio-group`. |
| `<sa-dropdown-menu-separator>` | Renders a line between groups of dropdown menu items. |
| `<sa-dropdown-menu-shortcut>` | Renders a keyboard shortcut hint at the end of a dropdown menu item. |
| `<sa-dropdown-menu-sub>` | Renders a submenu, which pairs an `sa-dropdown-menu-sub-trigger` with its `sa-dropdown-menu-sub-content`. |
| `<sa-dropdown-menu-sub-content>` | Renders the panel that holds a submenu's items. |
| `<sa-dropdown-menu-sub-trigger>` | Renders the dropdown menu item that opens a submenu. |
| `<sa-dropdown-menu-trigger>` | Renders the button that opens and closes a dropdown menu. |

## Attributes

### `<sa-dropdown-menu-checkbox-item>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `checked` | `bool` | `false` | `true`, `false` |
| `close-on-click` | `bool` | `false` | `true`, `false` |
| `disabled` | `bool` | `false` | `true`, `false` |
| `inset` | `bool` | `false` | `true`, `false` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

### `<sa-dropdown-menu-content>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `align` | `DropdownMenuAlign` | `Start` | `Start`, `Center`, `End`, `CenterWithin` |
| `side` | `DropdownMenuSide` | `Bottom` | `Top`, `Right`, `Bottom`, `Left` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-dropdown-menu-item>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `close-on-click` | `bool` | `true` | `true`, `false` |
| `disabled` | `bool` | `false` | `true`, `false` |
| `inset` | `bool` | `false` | `true`, `false` |
| `variant` | `DropdownMenuItemVariant` | `Default` | `Default`, `Destructive` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-dropdown-menu-item-link>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `close-on-click` | `bool` | `true` | `true`, `false` |
| `disabled` | `bool` | `false` | `true`, `false` |
| `inset` | `bool` | `false` | `true`, `false` |
| `variant` | `DropdownMenuItemVariant` | `Default` | `Default`, `Destructive` |
| `asp-action` | `string` | — | — |
| `asp-area` | `string` | — | — |
| `asp-controller` | `string` | — | — |
| `asp-fragment` | `string` | — | — |
| `asp-host` | `string` | — | — |
| `asp-page` | `string` | — | — |
| `asp-page-handler` | `string` | — | — |
| `asp-protocol` | `string` | — | — |
| `asp-route` | `string` | — | — |
| `asp-all-route-data` | `IDictionary<string, string?>` | — | — |
| `asp-route-*` | `IDictionary<string, string?>` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-dropdown-menu-label>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `inset` | `bool` | `false` | `true`, `false` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-dropdown-menu-radio-group>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `value` | `string` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-dropdown-menu-radio-item>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `close-on-click` | `bool` | `false` | `true`, `false` |
| `disabled` | `bool` | `false` | `true`, `false` |
| `value` | `string` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-dropdown-menu-sub-content>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `align` | `DropdownMenuAlign` | `Start` | `Start`, `Center`, `End`, `CenterWithin` |
| `side` | `DropdownMenuSide` | `Right` | `Top`, `Right`, `Bottom`, `Left` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-dropdown-menu-sub-trigger>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `inset` | `bool` | `false` | `true`, `false` |
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
            <sa-icon name="trash"/>
            Cancel Trip
        </sa-dropdown-menu-item>
    </sa-dropdown-menu-content>
</sa-dropdown-menu>
```
