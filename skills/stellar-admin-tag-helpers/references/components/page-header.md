---
component: PageHeader
tags: [sa-page-header, sa-page-header-actions, sa-page-header-description, sa-page-header-nav, sa-page-header-title]
generated: true
---

# PageHeader

A header section at the top of a page's content, rendered as a `<header>` element. Composed with an optional `<sa-breadcrumb>`, `<sa-page-header-title>`, `<sa-page-header-description>`, `<sa-page-header-actions>`, and `<sa-page-header-nav>` — in that order, which is also the top-to-bottom order on narrow viewports.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-page-header>` | A header section at the top of a page's content, rendered as a `<header>` element. Composed with an optional `<sa-breadcrumb>`, `<sa-page-header-title>`, `<sa-page-header-description>`, `<sa-page-header-actions>`, and `<sa-page-header-nav>` — in that order, which is also the top-to-bottom order on narrow viewports. |
| `<sa-page-header-actions>` | An action area inside `<sa-page-header>` for content such as buttons. Aligned to the end of the title row on regular viewports and placed below the title and description on narrow viewports. |
| `<sa-page-header-description>` | A short description below the title of `<sa-page-header>`, rendered as a `<p>` element. |
| `<sa-page-header-nav>` | A full-width navigation area at the bottom of `<sa-page-header>`, rendered as a `<nav>` element. It positions whatever navigation the page provides — typically an `<sa-tab-list>` — without imposing any styling of its own. |
| `<sa-page-header-title>` | The page title inside `<sa-page-header>`, rendered as the page's `<h1>` element. Inline trailing content such as an `<sa-badge>` can be placed directly after the title text. |

## Examples

*From `Pages/PageHeader/_Intro.cshtml`*

```razor
<sa-page-header>
    <sa-page-header-title>Flight Reservations</sa-page-header-title>
    <sa-page-header-description>
        Manage flight bookings, fare classes, and seat inventory across all Voyager Travel routes.
    </sa-page-header-description>
    <sa-page-header-actions>
        <sa-button variant="ButtonVariant.Outline">
            <sa-icon name="download"/>
            Export
        </sa-button>
        <sa-button>
            <sa-icon name="plus"/>
            New reservation
        </sa-button>
    </sa-page-header-actions>
</sa-page-header>
```

*From `Pages/PageHeader/_Breadcrumb.cshtml`*

```razor
<sa-page-header>
    <sa-breadcrumb>
        <sa-breadcrumb-list>
            <sa-breadcrumb-item>
                <sa-breadcrumb-link href="#">Operations</sa-breadcrumb-link>
            </sa-breadcrumb-item>
            <sa-breadcrumb-separator/>
            <sa-breadcrumb-item>
                <sa-breadcrumb-link href="#">Flights</sa-breadcrumb-link>
            </sa-breadcrumb-item>
            <sa-breadcrumb-separator/>
            <sa-breadcrumb-page>Flight Reservations</sa-breadcrumb-page>
        </sa-breadcrumb-list>
    </sa-breadcrumb>
    <sa-page-header-title>Flight Reservations</sa-page-header-title>
    <sa-page-header-description>
        Manage flight bookings, fare classes, and seat inventory across all Voyager Travel routes.
    </sa-page-header-description>
    <sa-page-header-actions>
        <sa-button>
            <sa-icon name="plus"/>
            New reservation
        </sa-button>
    </sa-page-header-actions>
</sa-page-header>
```

*From `Pages/PageHeader/_Nav.cshtml`*

```razor
<sa-page-header>
    <sa-page-header-title>Hotel Partners</sa-page-header-title>
    <sa-page-header-description>
        Partner hotels, contracted rates, and seasonal availability.
    </sa-page-header-description>
    <sa-page-header-actions>
        <sa-button variant="ButtonVariant.Outline">
            <sa-icon name="plus"/>
            Add partner
        </sa-button>
    </sa-page-header-actions>
    <sa-page-header-nav aria-label="Page sections">
        <sa-tab-list variant="TabListVariant.Line">
            <sa-tab-link is-active="true" href="#">Overview</sa-tab-link>
            <sa-tab-link href="#">
                Contracts
                <sa-badge variant="BadgeVariant.Secondary">36</sa-badge>
            </sa-tab-link>
            <sa-tab-link href="#">Rate plans</sa-tab-link>
            <sa-tab-link href="#">Settings</sa-tab-link>
        </sa-tab-list>
    </sa-page-header-nav>
</sa-page-header>
```
