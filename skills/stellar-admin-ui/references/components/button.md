---
component: Button
tags: [sa-button, sa-button-link]
generated: true
---

# Button

Renders a button element for triggering actions.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-button>` | Renders a button element for triggering actions. |
| `<sa-button-link>` | Renders an anchor element styled as a button, with routing support. |

## Attributes

### `<sa-button>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `size` | `ButtonSize` | `Default` | `Default`, `ExtraSmall`, `Small`, `Large`, `Icon`, `IconExtraSmall`, `IconSmall`, `IconLarge` |
| `variant` | `ButtonVariant` | `Default` | `Default`, `Destructive`, `Outline`, `Secondary`, `Ghost`, `Link` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

### `<sa-button-link>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `size` | `ButtonSize` | `Default` | `Default`, `ExtraSmall`, `Small`, `Large`, `Icon`, `IconExtraSmall`, `IconSmall`, `IconLarge` |
| `variant` | `ButtonVariant` | `Default` | `Default`, `Destructive`, `Outline`, `Secondary`, `Ghost`, `Link` |
| `asp-action` | `string` | `null` | — |
| `asp-area` | `string` | `null` | — |
| `asp-controller` | `string` | `null` | — |
| `asp-fragment` | `string` | — | — |
| `asp-host` | `string` | — | — |
| `asp-page` | `string` | `null` | — |
| `asp-page-handler` | `string` | `null` | — |
| `asp-protocol` | `string` | — | — |
| `asp-route` | `string` | `null` | — |
| `asp-all-route-data` | `IDictionary<string, string?>` | — | — |
| `asp-route-*` | `IDictionary<string, string?>` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/Button/_Intro.cshtml`*

```razor
<sa-button variant="ButtonVariant.Outline">
    <sa-icon name="undo-2"/>
    Cancel Changes
</sa-button>
<sa-button>Save Changes</sa-button>
```

*From `Pages/LinkButton/_Intro.cshtml`*

```razor
<sa-button-link variant="ButtonVariant.Outline" href="#">
    <sa-icon name="tickets-plane"/>
    View Tickets
</sa-button-link>
<sa-button-link variant="ButtonVariant.Outline" size="ButtonSize.Icon" class="rounded-full" href="#" aria-label="Share">
    <sa-icon name="share"/>
</sa-button-link>
```

*From `Pages/LinkButton/_Url.cshtml`*

```razor
<!-- Specify the URL via the href attribute -->
<sa-button-link variant="ButtonVariant.Outline" href="https://dotnet.microsoft.com/" target="_blank">
    .NET Home Page
</sa-button-link>
<!-- Specify the Razor Page to navigate to -->
<sa-button-link variant="ButtonVariant.Outline" asp-page="/Booking/Confirmation" asp-route-id="123">
    Booking Confirmation
</sa-button-link>
<!-- Specify the MVC Controller Action to navigate to -->
<sa-button-link variant="ButtonVariant.Outline" asp-controller="Booking" asp-action="Confirmation"
               asp-route-id="123">
    Booking Confirmation
</sa-button-link>
```
