---
component: Marker
tags: [sa-marker, sa-marker-button, sa-marker-content, sa-marker-icon, sa-marker-link]
generated: true
---

# Marker

An inline note in a conversation, such as a status update, a system message or a labeled divider.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-marker>` | An inline note in a conversation, such as a status update, a system message or a labeled divider. |
| `<sa-marker-button>` | A marker rendered as a button, making the whole marker activatable. |
| `<sa-marker-content>` | The text content of a marker. |
| `<sa-marker-icon>` | A decorative icon alongside the marker content, hidden from assistive technology. |
| `<sa-marker-link>` | A marker rendered as an anchor, making the whole marker a clickable link. |

## Attributes

### `<sa-marker>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `variant` | `MarkerVariant` | `Default` | `Default`, `Border`, `Separator` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

### `<sa-marker-button>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `variant` | `MarkerVariant` | `Default` | `Default`, `Border`, `Separator` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-marker-link>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `variant` | `MarkerVariant` | `Default` | `Default`, `Border`, `Separator` |
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

*From `Pages/Marker/_Intro.cshtml`*

```razor
<div class="flex w-full flex-col gap-6">
    <sa-marker variant="MarkerVariant.Separator">
        <sa-marker-content>Today</sa-marker-content>
    </sa-marker>
    <sa-message>
        <sa-message-avatar>
            <sa-avatar src="/avatars/avatar-2.jpg" name="Mika Tanaka"/>
        </sa-message-avatar>
        <sa-message-content>
            <sa-bubble variant="BubbleVariant.Secondary">
                <sa-bubble-content>
                    Your Kyoto itinerary is confirmed — 12 to 19 April.
                </sa-bubble-content>
            </sa-bubble>
        </sa-message-content>
    </sa-message>
    <sa-marker>
        <sa-marker-icon>
            <sa-icon name="ticket"/>
        </sa-marker-icon>
        <sa-marker-content>E-tickets issued for 2 travellers</sa-marker-content>
    </sa-marker>
    <sa-message align="MessageAlign.End">
        <sa-message-avatar>
            <sa-avatar src="/avatars/avatar-1.jpg" name="Priya Raman"/>
        </sa-message-avatar>
        <sa-message-content>
            <sa-bubble>
                <sa-bubble-content>
                    Wonderful — could you add the Arashiyama day trip as well?
                </sa-bubble-content>
            </sa-bubble>
        </sa-message-content>
    </sa-message>
</div>
```

*From `Pages/Marker/_Variants.cshtml`*

```razor
<div class="flex w-full flex-col gap-6">
    <sa-marker>
        <sa-marker-icon>
            <sa-icon name="circle-check"/>
        </sa-marker-icon>
        <sa-marker-content>Booking reference VYG-4821 confirmed</sa-marker-content>
    </sa-marker>
    <sa-marker variant="MarkerVariant.Border">
        <sa-marker-icon>
            <sa-icon name="credit-card"/>
        </sa-marker-icon>
        <sa-marker-content>Deposit of €450 received</sa-marker-content>
    </sa-marker>
    <sa-marker variant="MarkerVariant.Separator">
        <sa-marker-content>Tuesday, 12 April</sa-marker-content>
    </sa-marker>
</div>
```

*From `Pages/Marker/_Icon.cshtml`*

```razor
<div class="flex w-full flex-col gap-4">
    <sa-marker>
        <sa-marker-icon>
            <sa-icon name="plane-takeoff"/>
        </sa-marker-icon>
        <sa-marker-content>Flight NH212 departs Heathrow at 11:40</sa-marker-content>
    </sa-marker>
    <sa-marker>
        <sa-marker-icon>
            <sa-icon name="hotel"/>
        </sa-marker-icon>
        <sa-marker-content>Two nights at the Kanra Kyoto added to the itinerary</sa-marker-content>
    </sa-marker>
    <sa-marker>
        <sa-marker-icon>
            <sa-icon name="paperclip"/>
        </sa-marker-icon>
        <sa-marker-content>Mika attached kyoto-itinerary.pdf</sa-marker-content>
    </sa-marker>
</div>
```

*From `Pages/Marker/_Status.cshtml`*

```razor
<div class="flex w-full flex-col gap-6">
    <sa-message align="MessageAlign.End">
        <sa-message-avatar>
            <sa-avatar src="/avatars/avatar-1.jpg" name="Priya Raman"/>
        </sa-message-avatar>
        <sa-message-content>
            <sa-bubble>
                <sa-bubble-content>
                    Could you check whether the Sagano railway still has seats on the 14:00?
                </sa-bubble-content>
            </sa-bubble>
        </sa-message-content>
    </sa-message>
    <sa-marker role="status">
        <sa-marker-icon>
            <sa-spinner/>
        </sa-marker-icon>
        <sa-marker-content>Checking availability with the operator</sa-marker-content>
    </sa-marker>
</div>
```

*From `Pages/Marker/_Link.cshtml`*

```razor
<div class="flex w-full flex-col gap-6">
    <sa-message>
        <sa-message-avatar>
            <sa-avatar src="/avatars/avatar-2.jpg" name="Mika Tanaka"/>
        </sa-message-avatar>
        <sa-message-content>
            <sa-bubble variant="BubbleVariant.Secondary">
                <sa-bubble-content>
                    I've put the full day-by-day plan together for you.
                </sa-bubble-content>
            </sa-bubble>
        </sa-message-content>
    </sa-message>
    <sa-marker-link href="#">
        <sa-marker-icon>
            <sa-icon name="file-text"/>
        </sa-marker-icon>
        <sa-marker-content>Open the Kyoto itinerary</sa-marker-content>
    </sa-marker-link>
    <sa-marker-link href="#">
        <sa-marker-icon>
            <sa-icon name="map-pin"/>
        </sa-marker-icon>
        <sa-marker-content>View the Arashiyama walking route</sa-marker-content>
    </sa-marker-link>
</div>
```
