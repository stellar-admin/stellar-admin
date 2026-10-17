---
component: PageHeader
tags: [sa-page-header, sa-page-header-actions, sa-page-header-description, sa-page-header-nav, sa-page-header-title]
generated: true
---

# PageHeader

Renders a header at the top of a page's content.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-page-header>` | Renders a header at the top of a page's content. |
| `<sa-page-header-actions>` | Renders the area of a page header that holds actions, such as buttons. |
| `<sa-page-header-description>` | Renders a short description below the title of a page header. |
| `<sa-page-header-nav>` | Renders a full-width navigation area at the bottom of a page header. |
| `<sa-page-header-title>` | Renders the title of a page header as the page's `<h1>` heading. |

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
            <sa-tab-link active="true" href="#">Overview</sa-tab-link>
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
