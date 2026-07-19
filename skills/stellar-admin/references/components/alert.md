---
component: Alert
tags: [sa-alert, sa-alert-action, sa-alert-description, sa-alert-title]
generated: true
---

# Alert

A callout that displays a short, important message to the user, optionally with an icon, title, and description.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-alert>` | A callout that displays a short, important message to the user, optionally with an icon, title, and description. |
| `<sa-alert-action>` | A region within an alert for interactive elements such as buttons or links. |
| `<sa-alert-description>` | The descriptive body text of an alert, shown beneath the title. |
| `<sa-alert-title>` | The title heading of an alert. |

## Attributes

### `<sa-alert>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `description` | `string` | — | — |
| `icon` | `string` | — | — |
| `title` | `string` | — | — |
| `variant` | `AlertVariant` | `Default` | `Default`, `Destructive` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

## Examples

*From `Pages/Alert/_Intro.cshtml`*

```razor
<sa-alert>
    <sa-icon name="circle-check"/>
    <sa-alert-title>Booking Confirmed</sa-alert-title>
    <sa-alert-description>
        <p>Your trip to Paris has been successfully booked. Check your email for your e-tickets and itinerary.</p>
    </sa-alert-description>
    <sa-alert-action>
        <sa-button size="ButtonSize.ExtraSmall" variant="ButtonVariant.Outline">
            <sa-icon name="tickets-plane"/>
            View Tickets
        </sa-button>
    </sa-alert-action>
</sa-alert>
```

*From `Pages/Alert/_Actions.cshtml`*

```razor
<div class="mx-auto flex w-full max-w-lg flex-col gap-4">
    <sa-alert>
        <sa-icon name="circle-alert"/>
        <sa-alert-title>The selected emails have been marked as spam.</sa-alert-title>
        <sa-alert-action>
            <sa-button size="ButtonSize.ExtraSmall">Undo</sa-button>
        </sa-alert-action>
    </sa-alert>
    <sa-alert>
        <sa-icon name="circle-alert"/>
        <sa-alert-title>The selected emails have been marked as spam.</sa-alert-title>
        <sa-alert-description>
            This is a very long alert title that demonstrates how the component
            handles extended text content.
        </sa-alert-description>
        <sa-alert-action>
            <sa-badge variant="BadgeVariant.Secondary">Badge</sa-badge>
        </sa-alert-action>
    </sa-alert>
</div>
```
