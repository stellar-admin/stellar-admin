---
name: layout
description: >-
  Composes page layouts and app shells with StellarAdmin Tag Helpers in ASP.NET Core — the
  sidebar dashboard shell (wrapper + sidebar + inset), the app header, the page header,
  the Page Container / Stack / Group spacing primitives, and Card composition. Use when
  building a StellarAdmin page layout, an admin or dashboard shell, a navigation sidebar,
  a page header, or a card, or when the user mentions a StellarAdmin layout, sidebar,
  dashboard, app shell, app header, page header, or card.
metadata:
  author: StellarAdmin
---

# Composing layouts with StellarAdmin

Patterns for page structure. For a component's attributes and values, open its file under `../tag-helpers/references/components/` — for example `../tag-helpers/references/components/sidebar.md`, `.../app-header.md`, `.../page-header.md`, `.../layout.md`, `.../card.md`. For cross-cutting rules see `../tag-helpers/references/conventions.md`.

## The pieces, and where each belongs

| Level | Tag | Role |
|-------|-----|------|
| Shell | `<sa-sidebar-wrapper>` | Holds the sidebar and the inset. |
| Shell | `<sa-sidebar>` | The navigation panel. |
| Shell | `<sa-sidebar-inset>` | Everything that isn't the sidebar. |
| Chrome | `<sa-app-header>` | App-level bar — usually the first child of the inset. |
| Page | `<sa-page-container>` | Centers and width-constrains the page's content. |
| Page | `<sa-page-header>` | Page title, description, actions, sub-nav. |
| Content | `<sa-stack>` / `<sa-group>` | Vertical / horizontal spacing primitives. |
| Content | `<sa-card>` | A raised content panel. |

## The sidebar app shell (dashboard)

`<sa-sidebar-wrapper>` is the shell; it contains exactly two children — the `<sa-sidebar>` and the `<sa-sidebar-inset>` (your main content).

```razor
<sa-sidebar-wrapper>
    <sa-sidebar>
        <sa-sidebar-header>
            <sa-sidebar-menu>
                <sa-sidebar-menu-item>
                    <sa-sidebar-menu-link href="/" size="SidebarMenuLinkSize.Large">
                        <sa-icon name="compass"/>
                        <span class="truncate font-semibold">Voyager Travel</span>
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
                            <sa-sidebar-menu-link asp-page="/Dashboard">
                                <sa-icon name="layout-dashboard"/><span>Dashboard</span>
                            </sa-sidebar-menu-link>
                            <sa-sidebar-menu-badge>12</sa-sidebar-menu-badge>
                        </sa-sidebar-menu-item>
                        <sa-sidebar-menu-item>
                            <sa-sidebar-menu-link asp-page="/Destinations/Index">
                                <sa-icon name="map-pinned"/><span>Destinations</span>
                            </sa-sidebar-menu-link>
                            <sa-sidebar-menu-sub>
                                <sa-sidebar-menu-sub-item>
                                    <sa-sidebar-menu-sub-link asp-page="/Destinations/Europe">
                                        <span>Europe</span>
                                    </sa-sidebar-menu-sub-link>
                                </sa-sidebar-menu-sub-item>
                            </sa-sidebar-menu-sub>
                        </sa-sidebar-menu-item>
                    </sa-sidebar-menu>
                </sa-sidebar-group-content>
            </sa-sidebar-group>
        </sa-sidebar-content>

        <sa-sidebar-footer>
            <!-- user menu, often a sa-dropdown-menu over a sa-sidebar-menu-button -->
        </sa-sidebar-footer>
    </sa-sidebar>

    <sa-sidebar-inset>
        <sa-app-header>
            <sa-sidebar-trigger></sa-sidebar-trigger>
            <sa-app-header-separator/>
            <sa-breadcrumb>
                <sa-breadcrumb-list>
                    <sa-breadcrumb-item>
                        <sa-breadcrumb-link asp-page="/Bookings/Index">Bookings</sa-breadcrumb-link>
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

        <sa-page-container width="PageContainerWidth.Large">
            <!-- page header + page content -->
        </sa-page-container>
    </sa-sidebar-inset>
</sa-sidebar-wrapper>
```

Rules:
- Body order inside `<sa-sidebar>`: header → content (groups) → footer.
- Follow the menu hierarchy exactly — `sa-sidebar-menu` › `sa-sidebar-menu-item` › `sa-sidebar-menu-link` (or `-menu-button`); optional `-menu-badge`; nested submenus are `-menu-sub` › `-menu-sub-item` › `-menu-sub-link`. Don't flatten it.
- `<sa-sidebar-trigger>` is a real toggle — it issues the `--toggle-sidebar` command to the wrapper. Don't add `onclick`.
- Variants on `<sa-sidebar>`: `variant="SidebarVariant.Inset"` (main content as a floating card) or `SidebarVariant.Floating`; `side="SidebarSide.Right"`; `collapsible="SidebarCollapsible.Icon"` or `SidebarCollapsible.None`.
- `<sa-sidebar-menu-link>` supports `href` and the `asp-*` routing attributes, and marks itself active on the current route.

## The app header

`<sa-app-header>` renders a `<header>`. In a sidebar shell it is **typically the first child of `<sa-sidebar-inset>`**, so it spans the content area with the sidebar alongside — that placement is a convention, not a requirement. The canonical composition is: sidebar trigger → `<sa-app-header-separator/>` → breadcrumb → `<sa-app-header-actions>` for trailing content (notification buttons, avatar).

It works without a sidebar too — just place it at the top of the page.

## The page header

`<sa-page-header>` renders the page's own heading block, inside the page container. Children go in this order, which is also the top-to-bottom order on narrow viewports:

```razor
<sa-page-header>
    <sa-breadcrumb>...</sa-breadcrumb>                      @* optional *@
    <sa-page-header-title>Flight Reservations</sa-page-header-title>
    <sa-page-header-description>
        Manage flight bookings, fare classes, and seat inventory.
    </sa-page-header-description>
    <sa-page-header-actions>
        <sa-button variant="ButtonVariant.Outline"><sa-icon name="download"/>Export</sa-button>
        <sa-button><sa-icon name="plus"/>New reservation</sa-button>
    </sa-page-header-actions>
    <sa-page-header-nav>
        <sa-tab-list>...</sa-tab-list>                      @* optional sub-navigation *@
    </sa-page-header-nav>
</sa-page-header>
```

`<sa-page-header-title>` renders the page's `<h1>`; inline trailing content such as an `<sa-badge>` goes directly after the title text.

Don't confuse the two headers: `<sa-app-header>` is app chrome (persistent, holds the sidebar trigger); `<sa-page-header>` is page content (changes per page, holds the `<h1>`).

## Spacing primitives: Page Container / Stack / Group

Reach for these instead of hand-rolling flex utilities, and control spacing with the `gap` / `align` / `justify` enum attributes.

- **`<sa-page-container>`** — centered page wrapper: horizontal centering, a consistent gutter, vertical spacing between its children, and an optional max width. `width` (`PageContainerWidth`): `Full` (default), `Large`, `Medium`, `Small`.
- **`<sa-stack>`** — vertical column (`flex-col`). `gap` (`StackGap`), `align` (`StackAlign`), `justify` (`StackJustify`).
- **`<sa-group>`** — horizontal row (`flex-row`). `gap` (`GroupGap`), `align` (`GroupAlign`), `justify` (`GroupJustify`).

```razor
<sa-page-container width="PageContainerWidth.Medium">
    <sa-page-header>
        <sa-page-header-title>Bookings</sa-page-header-title>
    </sa-page-header>
    <sa-stack gap="StackGap.Large">
        <sa-group justify="GroupJustify.SpaceBetween" align="GroupAlign.Center">
            <span class="text-sm text-muted-foreground">12 active bookings</span>
            <sa-button><sa-icon name="plus"/>New booking</sa-button>
        </sa-group>
        <!-- content rows -->
    </sa-stack>
</sa-page-container>
```

Gap members: `ExtraSmall, Small, Default, Large, ExtraLarge`. Justify: `Start, Center, End, SpaceBetween, SpaceAround`. Align: `Stretch, Start, Center, End` (`GroupAlign` adds `Baseline`).

Note there is **no `<sa-container>`** — the page wrapper is `<sa-page-container>`.

## Card composition

Slots in fixed order: header (title / description / optional action) → content → footer.

```razor
<sa-card class="mx-auto w-full max-w-sm">
    <sa-card-header>
        <sa-card-title>Login to your account</sa-card-title>
        <sa-card-description>Enter your email below to sign in.</sa-card-description>
        <sa-card-action>
            <!-- optional top-right slot, e.g. a sa-dropdown-menu trigger -->
        </sa-card-action>
    </sa-card-header>
    <sa-card-content>
        <form method="post" id="login-form">
            <sa-field-group>
                <sa-input asp-for="Email" />
                <sa-input asp-for="Password" />
            </sa-field-group>
        </form>
    </sa-card-content>
    <sa-card-footer class="flex-col gap-2">
        <sa-button type="submit" form="login-form" class="w-full">Login</sa-button>
    </sa-card-footer>
</sa-card>
```

- **A submit button in the footer is outside the form** — the `<form>` lives in `<sa-card-content>`, so give the form an `id` and point the button at it with the native `form` attribute, as above. (It passes straight through to the `<button>`; see conventions.md §3.) The alternative is to wrap the whole card in the `<form>` instead.
- Dividers are opt-in utilities: `<sa-card-header class="border-b">` / `<sa-card-footer class="border-t">`.
- `size="CardSize.Small"` tightens the padding.
- The `class` values above (`mx-auto`, `w-full`, `max-w-sm`, `flex-col gap-2`) are Tailwind utilities, so they only take effect if the app runs its own Tailwind build — see conventions.md §4. Without one, size and center the card with your own CSS class instead.
- Building the form inside the card? See the [forms](../forms/SKILL.md) skill.

## Rules

1. `<sa-sidebar-wrapper>` must hold both `<sa-sidebar>` and `<sa-sidebar-inset>`; the app header, page content and the trigger all go in the inset.
2. Respect the sidebar, page-header and card tag hierarchies; don't flatten or reorder slots.
3. Use `<sa-page-container>` / `<sa-stack>` / `<sa-group>` and their `gap` / `align` / `justify` enums for layout rhythm rather than raw flex classes.
4. All variant / size / width / align / gap / justify attributes take fully-qualified enum values.
5. Utility classes you add (`w-full`, `max-w-sm`) are appended to the component's own classes — but only resolve if the app runs its own Tailwind build. See conventions.md §4.
