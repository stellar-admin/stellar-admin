---
component: Bubble
tags: [sa-bubble, sa-bubble-button-content, sa-bubble-content, sa-bubble-group, sa-bubble-link-content, sa-bubble-reactions]
generated: true
---

# Bubble

A framed message in a conversation, holding its content and any reactions.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-bubble>` | A framed message in a conversation, holding its content and any reactions. |
| `<sa-bubble-button-content>` | Bubble content rendered as a button, making the whole bubble activatable. |
| `<sa-bubble-content>` | The framed surface of a bubble, holding the message text. |
| `<sa-bubble-group>` | A container that stacks consecutive bubbles from the same sender. |
| `<sa-bubble-link-content>` | Bubble content rendered as an anchor, making the whole bubble a clickable link. |
| `<sa-bubble-reactions>` | A row of emoji reactions or small actions, anchored to an edge of the bubble. |

## Attributes

### `<sa-bubble>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `align` | `BubbleAlign` | `Start` | `Start`, `End` |
| `variant` | `BubbleVariant` | `Default` | `Default`, `Secondary`, `Muted`, `Tinted`, `Outline`, `Ghost`, `Destructive` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

### `<sa-bubble-link-content>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
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

### `<sa-bubble-reactions>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `align` | `BubbleReactionsAlign` | `End` | `Start`, `End` |
| `side` | `BubbleReactionsSide` | `Bottom` | `Top`, `Bottom` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/Bubble/_Intro.cshtml`*

```razor
<sa-bubble-group class="w-full">
    <sa-bubble variant="BubbleVariant.Secondary">
        <sa-bubble-content>
            Morning! Your Kyoto itinerary is ready — 12 to 19 April.
        </sa-bubble-content>
    </sa-bubble>
    <sa-bubble align="BubbleAlign.End">
        <sa-bubble-content>
            That's great. Does it still include the Arashiyama day trip?
        </sa-bubble-content>
    </sa-bubble>
    <sa-bubble variant="BubbleVariant.Secondary">
        <sa-bubble-content>
            It does — the bamboo grove in the morning, then the Sagano railway at 14:00.
        </sa-bubble-content>
    </sa-bubble>
</sa-bubble-group>
```

*From `Pages/Bubble/_Group.cshtml`*

```razor
<sa-bubble-group>
    <sa-bubble variant="BubbleVariant.Secondary">
        <sa-bubble-content>Your e-tickets are attached to the itinerary.</sa-bubble-content>
    </sa-bubble>
    <sa-bubble variant="BubbleVariant.Secondary">
        <sa-bubble-content>Check-in opens 24 hours before departure.</sa-bubble-content>
    </sa-bubble>
    <sa-bubble variant="BubbleVariant.Secondary">
        <sa-bubble-content>Let me know if you'd like seats together.</sa-bubble-content>
    </sa-bubble>
</sa-bubble-group>
<sa-bubble-group>
    <sa-bubble align="BubbleAlign.End">
        <sa-bubble-content>Yes please — two seats by the window.</sa-bubble-content>
    </sa-bubble>
    <sa-bubble align="BubbleAlign.End">
        <sa-bubble-content>And a vegetarian meal for both of us.</sa-bubble-content>
    </sa-bubble>
</sa-bubble-group>
```

*From `Pages/Bubble/_Reactions.cshtml`*

```razor
<sa-bubble variant="BubbleVariant.Secondary">
    <sa-bubble-content>
        Upgraded you both to a garden-view room in Kyoto.
    </sa-bubble-content>
    <sa-bubble-reactions role="img" aria-label="Reactions: thumbs up, party popper">
        <span>&#128077;</span>
        <span>&#127881;</span>
    </sa-bubble-reactions>
</sa-bubble>
<sa-bubble variant="BubbleVariant.Secondary">
    <sa-bubble-content>
        The Sagano railway tickets are confirmed.
    </sa-bubble-content>
    <sa-bubble-reactions side="BubbleReactionsSide.Top" align="BubbleReactionsAlign.Start"
                         role="img" aria-label="Reactions: steam train">
        <span>&#128646;</span>
    </sa-bubble-reactions>
</sa-bubble>
<sa-bubble align="BubbleAlign.End">
    <sa-bubble-content>
        Booked — see you in April.
    </sa-bubble-content>
    <sa-bubble-reactions align="BubbleReactionsAlign.Start" role="img" aria-label="Reactions: red heart">
        <span>&#10084;&#65039;</span>
    </sa-bubble-reactions>
</sa-bubble>
```

*From `Pages/Bubble/_Link.cshtml`*

```razor
<sa-bubble align="BubbleAlign.End">
    <sa-bubble-content>
        Could you send me the travel documents?
    </sa-bubble-content>
</sa-bubble>
<sa-bubble variant="BubbleVariant.Secondary">
    <sa-bubble-content>
        All three are ready:
    </sa-bubble-content>
</sa-bubble>
<sa-bubble variant="BubbleVariant.Outline">
    <sa-bubble-link-content asp-page="/Booking/Document" asp-route-id="e-ticket">
        E-ticket — Cape Town to Osaka
    </sa-bubble-link-content>
</sa-bubble>
<sa-bubble variant="BubbleVariant.Outline">
    <sa-bubble-link-content asp-page="/Booking/Document" asp-route-id="hotel-voucher">
        Hotel voucher — Kyoto, 12 to 19 April
    </sa-bubble-link-content>
</sa-bubble>
<sa-bubble variant="BubbleVariant.Outline">
    <sa-bubble-link-content asp-page="/Booking/Document" asp-route-id="transfer">
        Airport transfer confirmation
    </sa-bubble-link-content>
</sa-bubble>
<sa-bubble variant="BubbleVariant.Secondary">
    <sa-bubble-content>
        Worth a read before you fly:
    </sa-bubble-content>
</sa-bubble>
<sa-bubble variant="BubbleVariant.Outline">
    <sa-bubble-link-content href="https://www.japan.travel/" target="_blank" rel="noopener noreferrer">
        Japan entry requirements
    </sa-bubble-link-content>
</sa-bubble>
```
