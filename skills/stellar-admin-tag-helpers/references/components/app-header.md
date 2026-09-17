---
component: AppHeader
tags: [sa-app-header, sa-app-header-actions, sa-app-header-separator]
generated: true
---

# AppHeader

The application's top navigation bar, rendered as a `<header>` element. Typically placed at the top of `<sa-sidebar-inset>` and composed with `<sa-sidebar-trigger>`, `<sa-app-header-separator>`, a breadcrumb, and `<sa-app-header-actions>`.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-app-header>` | The application's top navigation bar, rendered as a `<header>` element. Typically placed at the top of `<sa-sidebar-inset>` and composed with `<sa-sidebar-trigger>`, `<sa-app-header-separator>`, a breadcrumb, and `<sa-app-header-actions>`. |
| `<sa-app-header-actions>` | A right-aligned area at the end of `<sa-app-header>` for trailing content such as buttons or an avatar. |
| `<sa-app-header-separator>` | A vertical separator sized for use inside `<sa-app-header>`, typically between the sidebar trigger and the content that follows it. |

## Examples

*From `Pages/AppHeader/_Intro.cshtml`*

```razor
<sa-app-header>
    <span class="text-sm font-medium whitespace-nowrap">Voyager Travel</span>
    <sa-app-header-separator/>
    <sa-breadcrumb>
        <sa-breadcrumb-list>
            <sa-breadcrumb-item>
                <sa-breadcrumb-link href="#">Bookings</sa-breadcrumb-link>
            </sa-breadcrumb-item>
            <sa-breadcrumb-separator/>
            <sa-breadcrumb-page>Flights</sa-breadcrumb-page>
        </sa-breadcrumb-list>
    </sa-breadcrumb>
    <sa-app-header-actions>
        <sa-avatar name="Amelia Hart"/>
    </sa-app-header-actions>
</sa-app-header>
```

*From `Pages/AppHeader/_WithSidebar.cshtml`*

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
                        </sa-sidebar-menu-item>
                        <sa-sidebar-menu-item>
                            <sa-sidebar-menu-link href="#">
                                <sa-icon name="map-pinned"/>
                                <span>Destinations</span>
                            </sa-sidebar-menu-link>
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
        </sa-sidebar-content>
    </sa-sidebar>

    <sa-sidebar-inset>
        @* The canonical app header composition: trigger, separator, breadcrumb, trailing actions. *@
        <sa-app-header>
            <sa-sidebar-trigger></sa-sidebar-trigger>
            <sa-app-header-separator/>
            <sa-breadcrumb>
                <sa-breadcrumb-list>
                    <sa-breadcrumb-item>
                        <sa-breadcrumb-link href="#">Bookings</sa-breadcrumb-link>
                    </sa-breadcrumb-item>
                    <sa-breadcrumb-separator/>
                    <sa-breadcrumb-page>Flight Reservations</sa-breadcrumb-page>
                </sa-breadcrumb-list>
            </sa-breadcrumb>
            <sa-app-header-actions>
                <sa-button variant="ButtonVariant.Ghost" size="ButtonSize.IconSmall" aria-label="Notifications">
                    <sa-icon name="bell"/>
                </sa-button>
                <sa-avatar name="Amelia Hart"/>
            </sa-app-header-actions>
        </sa-app-header>
        <div class="p-4">
            <p class="text-sm text-muted-foreground">
                The app header is the first child of the inset, so it spans the content area with
                the sidebar alongside it. Toggle the sidebar with the trigger.
            </p>
        </div>
    </sa-sidebar-inset>
</sa-sidebar-wrapper>
```

*From `Pages/AppHeader/_Actions.cshtml`*

```razor
<sa-app-header>
    <span class="text-sm font-medium">Bookings</span>
    <sa-app-header-actions>
        <sa-button variant="ButtonVariant.Ghost" size="ButtonSize.IconSmall" aria-label="Search">
            <sa-icon name="search"/>
        </sa-button>
        <sa-button variant="ButtonVariant.Ghost" size="ButtonSize.IconSmall" aria-label="Notifications">
            <sa-icon name="bell"/>
        </sa-button>
        <sa-button variant="ButtonVariant.Outline" size="ButtonSize.Small">
            <sa-icon name="plus"/>
            New booking
        </sa-button>
        <sa-avatar name="Amelia Hart"/>
    </sa-app-header-actions>
</sa-app-header>
```
