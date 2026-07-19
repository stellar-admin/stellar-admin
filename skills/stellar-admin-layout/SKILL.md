---
name: stellar-admin-layout
description: >-
  Composes page layouts and app shells with StellarAdmin.UI tag helpers in ASP.NET Core — the sidebar
  dashboard shell (wrapper + sidebar + inset with a trigger), the Container / Stack / Group
  spacing primitives, and Card composition. Use when building a StellarAdmin.UI page layout, an
  admin/dashboard shell, a navigation sidebar, or a card, or when the user mentions a StellarAdmin.UI
  layout, sidebar, dashboard, app shell, or card.
metadata:
  author: StellarAdmin.UI
---

# Composing layouts with StellarAdmin.UI

Patterns for page structure. For a component's attributes/values, open its file under
`../stellar-admin/references/components/` (`sidebar.md`, `layout.md`, `card.md`); for cross-cutting
rules see `../stellar-admin/references/conventions.md`.

## The sidebar app shell (dashboard)

`<sa-sidebar-wrapper>` is the shell; it contains exactly two children — the `<sa-sidebar>` and
the `<sa-sidebar-inset>` (your main content). The toggle lives in the inset's header.

```razor
<sa-sidebar-wrapper>
    <sa-sidebar>
        <sa-sidebar-header>
            <!-- brand / workspace switcher, usually a sa-sidebar-menu-link -->
        </sa-sidebar-header>
        <sa-sidebar-content>
            <sa-sidebar-group>
                <sa-sidebar-group-label>Platform</sa-sidebar-group-label>
                <sa-sidebar-group-content>
                    <sa-sidebar-menu>
                        <sa-sidebar-menu-item>
                            <sa-sidebar-menu-link href="/dashboard">
                                <sa-icon name="layout-dashboard" /><span>Dashboard</span>
                            </sa-sidebar-menu-link>
                            <sa-sidebar-menu-badge>12</sa-sidebar-menu-badge>
                        </sa-sidebar-menu-item>
                        <sa-sidebar-menu-item>
                            <sa-sidebar-menu-link href="/destinations">
                                <sa-icon name="map-pinned" /><span>Destinations</span>
                            </sa-sidebar-menu-link>
                            <sa-sidebar-menu-sub>
                                <sa-sidebar-menu-sub-item>
                                    <sa-sidebar-menu-sub-link href="/destinations/europe"><span>Europe</span></sa-sidebar-menu-sub-link>
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
        <header class="flex h-14 items-center gap-2 border-b px-4">
            <sa-sidebar-trigger></sa-sidebar-trigger>
            <sa-separator orientation="SeparatorOrientation.Vertical" class="mx-1 h-4" />
            <span class="text-sm font-medium">Dashboard</span>
        </header>
        <div class="p-4">
            <!-- page content -->
        </div>
    </sa-sidebar-inset>
</sa-sidebar-wrapper>
```

Rules:
- Body order inside `<sa-sidebar>`: header → content (groups) → footer.
- Follow the menu hierarchy exactly — `sa-sidebar-menu` › `sa-sidebar-menu-item` ›
  `sa-sidebar-menu-link` (or `-menu-button`); optional `-menu-badge`; nested submenus are
  `-menu-sub` › `-menu-sub-item` › `-menu-sub-link`. Don't flatten it.
- `<sa-sidebar-trigger>` is a real toggle (also Ctrl/⌘ + B) — don't add `onclick`.
- Variants on `<sa-sidebar>`: `variant="SidebarVariant.Inset"` (main content as a floating
  card), `SidebarVariant.Floating`; `side="SidebarSide.Right"`; `collapsible="SidebarCollapsible.Icon"`.
- `<sa-sidebar-menu-link>` supports routing (`href`/`asp-*`) and marks itself active on the
  current route.

## Spacing primitives: Container / Stack / Group

Reach for these instead of hand-rolling flex utilities, and control spacing with the
`gap`/`align`/`justify` enum attributes.

- **`<sa-container>`** — centered, width-constrained page wrapper. Outermost page-width element.
- **`<sa-stack>`** — vertical column (`flex-col`). `gap` (`StackGap`), `align` (`StackAlign`,
  default `Stretch`), `justify` (`StackJustify`).
- **`<sa-group>`** — horizontal row (`flex-row`). `gap` (`GroupGap`), `align` (`GroupAlign`),
  `justify` (`GroupJustify`).

```razor
<sa-container>
    <sa-stack gap="StackGap.Large">
        <sa-group justify="GroupJustify.SpaceBetween" align="GroupAlign.Center">
            <h1 class="text-xl font-semibold">Bookings</h1>
            <sa-button><sa-icon name="plus" />New booking</sa-button>
        </sa-group>
        <!-- content rows -->
    </sa-stack>
</sa-container>
```

Gap members: `ExtraSmall, Small, Default, Large, ExtraLarge`. Justify: `Start, Center, End,
SpaceBetween, SpaceAround`. Align: `Stretch, Start, Center, End, Baseline`.

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
        <form method="post">
            <sa-field-group>
                <sa-input asp-for="Email" />
                <sa-input asp-for="Password" />
            </sa-field-group>
        </form>
    </sa-card-content>
    <sa-card-footer class="flex-col gap-2">
        <sa-button type="submit" class="w-full">Login</sa-button>
    </sa-card-footer>
</sa-card>
```

- Dividers are opt-in utilities: `<sa-card-header class="border-b">` /
  `<sa-card-footer class="border-t">`.
- Adjust width/spacing with `class` (merged last, so it wins).
- Building the form inside the card? See the `stellar-admin-forms` skill.

## Rules

1. `<sa-sidebar-wrapper>` must hold both `<sa-sidebar>` and `<sa-sidebar-inset>`; page content
   and the trigger go in the inset.
2. Respect the sidebar and card tag hierarchies; don't flatten or reorder slots.
3. Use `<sa-container>`/`<sa-stack>`/`<sa-group>` + their `gap`/`align`/`justify` enums for
   layout rhythm rather than raw flex classes.
4. All variant/size/align/gap/justify attributes take fully-qualified enum values.
