---
component: Sidebar
tags: [sa-sidebar, sa-sidebar-content, sa-sidebar-footer, sa-sidebar-group, sa-sidebar-group-content, sa-sidebar-group-label, sa-sidebar-header, sa-sidebar-inset, sa-sidebar-menu, sa-sidebar-menu-badge, sa-sidebar-menu-button, sa-sidebar-menu-item, sa-sidebar-menu-link, sa-sidebar-menu-sub, sa-sidebar-menu-sub-button, sa-sidebar-menu-sub-item, sa-sidebar-menu-sub-link, sa-sidebar-separator, sa-sidebar-trigger, sa-sidebar-wrapper]
generated: true
---

# Sidebar

The sidebar panel itself, hosting its header, content, and footer. On desktop it renders as a fixed panel that can collapse; on mobile it becomes an off-canvas drawer.

<!-- structure:begin -->

## Required structure

`<sa-sidebar-wrapper>` is the top-level container and holds **two** children:
the `<sa-sidebar>` and a `<sa-sidebar-inset>` for the page content.

```
sa-sidebar-wrapper
├── sa-sidebar
│   ├── sa-sidebar-header
│   ├── sa-sidebar-content
│   │   └── sa-sidebar-group
│   │       ├── sa-sidebar-group-label
│   │       └── sa-sidebar-group-content
│   │           └── sa-sidebar-menu
│   │               └── sa-sidebar-menu-item
│   │                   ├── sa-sidebar-menu-link      (or sa-sidebar-menu-button)
│   │                   ├── sa-sidebar-menu-badge     (optional)
│   │                   └── sa-sidebar-menu-sub       (optional nested menu)
│   │                       └── sa-sidebar-menu-sub-item
│   │                           └── sa-sidebar-menu-sub-link
│   └── sa-sidebar-footer
└── sa-sidebar-inset
    └── ... page content, usually starting with a header containing <sa-sidebar-trigger>
```

<!-- structure:end -->

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-sidebar>` | The sidebar panel itself, hosting its header, content, and footer. On desktop it renders as a fixed panel that can collapse; on mobile it becomes an off-canvas drawer. |
| `<sa-sidebar-content>` | The main scrollable content region of the sidebar; holds the sidebar's groups and menus. |
| `<sa-sidebar-footer>` | The footer region of the sidebar, pinned below its content; typically holds a user menu or secondary actions. |
| `<sa-sidebar-group>` | A titled section within the sidebar that groups related menu items together. |
| `<sa-sidebar-group-content>` | The content region of a sidebar group, wrapping the group's menu. |
| `<sa-sidebar-group-label>` | The label heading for a sidebar group. |
| `<sa-sidebar-header>` | The header region of the sidebar, pinned above its content; typically holds branding or a workspace switcher. |
| `<sa-sidebar-inset>` | The main content area shown alongside the sidebar, rendered as a `<main>` element. |
| `<sa-sidebar-menu>` | A list of menu items within a sidebar group, rendered as a list. |
| `<sa-sidebar-menu-badge>` | A small badge, typically a count, shown at the end of a sidebar menu item; hidden while the sidebar is collapsed to icons. |
| `<sa-sidebar-menu-button>` | A button rendered as an entry within a sidebar menu item. |
| `<sa-sidebar-menu-item>` | A single item within a sidebar menu, rendered as a list item. |
| `<sa-sidebar-menu-link>` | An anchor rendered as an entry within a sidebar menu item, with routing support; marks itself active when it matches the current route. |
| `<sa-sidebar-menu-sub>` | A nested submenu within a sidebar menu item, rendered as a list. |
| `<sa-sidebar-menu-sub-button>` | A button rendered as an entry within a nested sidebar submenu. |
| `<sa-sidebar-menu-sub-item>` | A single item within a nested sidebar submenu, rendered as a list item. |
| `<sa-sidebar-menu-sub-link>` | An anchor rendered as an entry within a nested sidebar submenu, with routing support; marks itself active when it matches the current route. |
| `<sa-sidebar-separator>` | A horizontal separator used to divide sections of the sidebar. |
| `<sa-sidebar-trigger>` | A button that toggles the open or collapsed state of its parent sidebar. |
| `<sa-sidebar-wrapper>` | The outermost sidebar container that provides layout and shared state for the sidebar and its inset content. Renders the `sel-sidebar` web component that nested triggers and the backdrop toggle. |

## Attributes

### `<sa-sidebar>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `variant` | `SidebarVariant` | `Sidebar` | `Sidebar`, `Floating`, `Inset` |
| `side` | `SidebarSide` | `Left` | `Left`, `Right` |
| `collapsible` | `SidebarCollapsible` | `Offcanvas` | `Offcanvas`, `Icon`, `None` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

### `<sa-sidebar-menu-button>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `size` | `SidebarMenuButtonSize` | `Default` | `Default`, `Small`, `Large` |
| `variant` | `SidebarMenuButtonVariant` | `Default` | `Default`, `Outline` |
| `is-active` | `bool` | `false` | `true`, `false` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-sidebar-menu-link>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `size` | `SidebarMenuLinkSize` | `Default` | `Default`, `Small`, `Large` |
| `variant` | `SidebarMenuLinkVariant` | `Default` | `Default`, `Outline` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-sidebar-menu-sub-button>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `size` | `SidebarMenuSubLinkSize` | `Medium` | `Small`, `Medium` |
| `is-active` | `bool` | `false` | `true`, `false` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-sidebar-menu-sub-link>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `size` | `SidebarMenuSubLinkSize` | `Medium` | `Small`, `Medium` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Example

*From `Pages/Sidebar/_Intro.cshtml`*

```razor
<sa-sidebar-wrapper>
    <sa-sidebar>
        <sa-sidebar-header>
            <sa-sidebar-menu>
                <sa-sidebar-menu-item>
                    <sa-sidebar-menu-link href="#" size="SidebarMenuLinkSize.Large">
                        <div
                            class="flex aspect-square size-8 items-center justify-center rounded-lg bg-primary text-primary-foreground">
                            <sa-icon name="compass"/>
                        </div>
                        <div class="grid flex-1 text-left text-sm leading-tight">
                            <span class="truncate font-semibold">Voyager Travel</span>
                            <span class="truncate text-xs text-muted-foreground">Admin Console</span>
                        </div>
                    </sa-sidebar-menu-link>
                </sa-sidebar-menu-item>
            </sa-sidebar-menu>
        </sa-sidebar-header>

        <sa-sidebar-content>
            <sa-sidebar-group>
                <sa-sidebar-group-label>Platform</sa-sidebar-group-label>
                <sa-sidebar-group-content>
                    <sa-sidebar-menu>
                        @* Active item — points back at this page so it resolves as the active route. *@
                        <sa-sidebar-menu-item>
                            <sa-sidebar-menu-link href="#">
                                <sa-icon name="layout-dashboard"/>
                                <span>Dashboard</span>
                            </sa-sidebar-menu-link>
                        </sa-sidebar-menu-item>
                        <sa-sidebar-menu-item>
                            <sa-sidebar-menu-link href="#">
                                <sa-icon name="ticket"/>
                                <span>Bookings</span>
                            </sa-sidebar-menu-link>
                            <sa-sidebar-menu-badge>12</sa-sidebar-menu-badge>
                        </sa-sidebar-menu-item>
                        @* Menu item with a nested sub-menu. *@
                        <sa-sidebar-menu-item>
                            <sa-sidebar-menu-link href="#">
                                <sa-icon name="map-pinned"/>
                                <span>Destinations</span>
                            </sa-sidebar-menu-link>
                            <sa-sidebar-menu-sub>
                                <sa-sidebar-menu-sub-item>
                                    <sa-sidebar-menu-sub-link href="#"><span>Europe</span>
                                    </sa-sidebar-menu-sub-link>
                                </sa-sidebar-menu-sub-item>
                                <sa-sidebar-menu-sub-item>
                                    <sa-sidebar-menu-sub-link href="#"><span>Asia Pacific</span>
                                    </sa-sidebar-menu-sub-link>
                                </sa-sidebar-menu-sub-item>
                                <sa-sidebar-menu-sub-item>
                                    <sa-sidebar-menu-sub-link href="#"><span>The Americas</span>
                                    </sa-sidebar-menu-sub-link>
                                </sa-sidebar-menu-sub-item>
                            </sa-sidebar-menu-sub>
                        </sa-sidebar-menu-item>
                        <sa-sidebar-menu-item>
                            <sa-sidebar-menu-link href="#">
                                <sa-icon name="users"/>
                                <span>Customers</span>
                            </sa-sidebar-menu-link>
                        </sa-sidebar-menu-item>
                    </sa-sidebar-menu>
                </sa-sidebar-group-content>
            </sa-sidebar-group>

            <sa-sidebar-group>
                <sa-sidebar-group-label>Operations</sa-sidebar-group-label>
                <sa-sidebar-group-content>
                    <sa-sidebar-menu>
                        <sa-sidebar-menu-item>
                            <sa-sidebar-menu-link href="#">
                                <sa-icon name="plane-takeoff"/>
                                <span>Flights</span>
                            </sa-sidebar-menu-link>
                            <sa-sidebar-menu-badge>3</sa-sidebar-menu-badge>
                        </sa-sidebar-menu-item>
                        <sa-sidebar-menu-item>
                            <sa-sidebar-menu-link href="#">
                                <sa-icon name="hotel"/>
                                <span>Hotels</span>
                            </sa-sidebar-menu-link>
                        </sa-sidebar-menu-item>
                        <sa-sidebar-menu-item>
                            <sa-sidebar-menu-link href="#">
                                <sa-icon name="calendar-days"/>
                                <span>Itineraries</span>
                            </sa-sidebar-menu-link>
                        </sa-sidebar-menu-item>
                    </sa-sidebar-menu>
                </sa-sidebar-group-content>
            </sa-sidebar-group>

            <sa-sidebar-separator/>

            <sa-sidebar-group>
                <sa-sidebar-group-label>Insights</sa-sidebar-group-label>
                <sa-sidebar-group-content>
                    <sa-sidebar-menu>
                        <sa-sidebar-menu-item>
                            <sa-sidebar-menu-link href="#">
                                <sa-icon name="chart-line"/>
                                <span>Reports</span>
                            </sa-sidebar-menu-link>
                        </sa-sidebar-menu-item>
                        <sa-sidebar-menu-item>
                            <sa-sidebar-menu-link href="#">
                                <sa-icon name="file-text"/>
                                <span>Invoices</span>
                            </sa-sidebar-menu-link>
                        </sa-sidebar-menu-item>
                    </sa-sidebar-menu>
                </sa-sidebar-group-content>
            </sa-sidebar-group>
        </sa-sidebar-content>

        <sa-sidebar-footer>
            <sa-sidebar-menu>
                <sa-sidebar-menu-item>
                    <sa-dropdown-menu id="nav-user-menu">
                        <sa-sidebar-menu-button size="SidebarMenuButtonSize.Large" popovertarget="nav-user-menu" aria-haspopup="menu">
                            <div
                                class="flex aspect-square size-8 items-center justify-center rounded-lg bg-muted text-foreground">
                                <sa-icon name="user"/>
                            </div>
                            <div class="grid flex-1 text-left text-sm leading-tight">
                                <span class="truncate font-semibold">Amelia Hart</span>
                                <span class="truncate text-xs text-muted-foreground">amelia@voyager.travel</span>
                            </div>
                            <sa-icon name="chevrons-up-down" class="ml-auto"/>
                        </sa-sidebar-menu-button>
                        <sa-dropdown-menu-content class="w-56" position="PositionArea.RightSpanTop">
                            <sa-dropdown-menu-label class="p-0 font-normal">
                                <div class="flex items-center gap-2 px-1 py-1.5 text-left text-sm">
                                    <div
                                        class="flex aspect-square size-8 items-center justify-center rounded-lg bg-muted text-foreground">
                                        <sa-icon name="user"/>
                                    </div>
                                    <div class="grid flex-1 text-left text-sm leading-tight">
                                        <span class="truncate font-semibold">Amelia Hart</span>
                                        <span class="truncate text-xs text-muted-foreground">amelia@voyager.travel</span>
                                    </div>
                                </div>
                            </sa-dropdown-menu-label>
                            <sa-dropdown-menu-separator/>
                            <sa-dropdown-menu-group>
                                <sa-dropdown-menu-item>
                                    <sa-icon name="badge-check"/>
                                    Account
                                </sa-dropdown-menu-item>
                                <sa-dropdown-menu-item>
                                    <sa-icon name="credit-card"/>
                                    Billing
                                </sa-dropdown-menu-item>
                                <sa-dropdown-menu-item>
                                    <sa-icon name="bell"/>
                                    Notifications
                                </sa-dropdown-menu-item>
                            </sa-dropdown-menu-group>
                            <sa-dropdown-menu-separator/>
                            <sa-dropdown-menu-item variant="DropdownMenuItemVariant.Destructive">
                                <sa-icon name="log-out"/>
                                Log out
                            </sa-dropdown-menu-item>
                        </sa-dropdown-menu-content>
                    </sa-dropdown-menu>
                </sa-sidebar-menu-item>
            </sa-sidebar-menu>
        </sa-sidebar-footer>
    </sa-sidebar>

    <sa-sidebar-inset>
        <header class="flex h-14 items-center gap-2 border-b px-4">
            <sa-sidebar-trigger></sa-sidebar-trigger>
            <sa-separator orientation="SeparatorOrientation.Vertical" class="mx-1 h-4"/>
            <span class="text-sm font-medium">Dashboard</span>
        </header>
        <div class="p-4">
            <p class="text-sm text-muted-foreground">
                A full-featured sidebar — header, grouped navigation with labels, an active
                item, badges, a nested sub-menu, a separator, and a user footer. Toggle it
                with the trigger (Ctrl/⌘ + B).
            </p>
        </div>
    </sa-sidebar-inset>
</sa-sidebar-wrapper>
```
