---
component: Message
tags: [sa-message, sa-message-avatar, sa-message-content, sa-message-footer, sa-message-group, sa-message-header]
generated: true
---

# Message

A single message in a conversation, laying out its avatar, content, header and footer.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-message>` | A single message in a conversation, laying out its avatar, content, header and footer. |
| `<sa-message-avatar>` | The avatar alongside a message, aligned with the bottom of the message. |
| `<sa-message-content>` | The column holding a message's header, message surface and footer. |
| `<sa-message-footer>` | The line below a message, typically holding a timestamp, delivery status or message actions. |
| `<sa-message-group>` | A container that stacks consecutive messages from the same sender. |
| `<sa-message-header>` | The line above a message, typically naming the sender. |

## Attributes

### `<sa-message>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `align` | `MessageAlign` | `Start` | `Start`, `End` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

## Examples

*From `Pages/Message/_Intro.cshtml`*

```razor
<div class="flex w-full flex-col gap-6">
    <sa-message>
        <sa-message-avatar>
            <sa-avatar src="/avatars/avatar-2.jpg" name="Mika Tanaka"/>
        </sa-message-avatar>
        <sa-message-content>
            <sa-bubble variant="BubbleVariant.Secondary">
                <sa-bubble-content>
                    Morning! Your Kyoto itinerary is ready — 12 to 19 April.
                </sa-bubble-content>
            </sa-bubble>
        </sa-message-content>
    </sa-message>
    <sa-message align="MessageAlign.End">
        <sa-message-avatar>
            <sa-avatar src="/avatars/avatar-1.jpg" name="Priya Raman"/>
        </sa-message-avatar>
        <sa-message-content>
            <sa-bubble>
                <sa-bubble-content>
                    That's great. Does it still include the Arashiyama day trip?
                </sa-bubble-content>
            </sa-bubble>
        </sa-message-content>
    </sa-message>
    <sa-message>
        <sa-message-avatar>
            <sa-avatar src="/avatars/avatar-2.jpg" name="Mika Tanaka"/>
        </sa-message-avatar>
        <sa-message-content>
            <sa-bubble variant="BubbleVariant.Secondary">
                <sa-bubble-content>
                    It does — the bamboo grove in the morning, then the Sagano railway at 14:00.
                </sa-bubble-content>
            </sa-bubble>
        </sa-message-content>
    </sa-message>
</div>
```

*From `Pages/Message/_Group.cshtml`*

```razor
<div class="flex w-full flex-col gap-6">
    <sa-message-group>
        <sa-message>
            <sa-message-avatar></sa-message-avatar>
            <sa-message-content>
                <sa-bubble variant="BubbleVariant.Secondary">
                    <sa-bubble-content>Your e-tickets are attached to the itinerary.</sa-bubble-content>
                </sa-bubble>
            </sa-message-content>
        </sa-message>
        <sa-message>
            <sa-message-avatar></sa-message-avatar>
            <sa-message-content>
                <sa-bubble variant="BubbleVariant.Secondary">
                    <sa-bubble-content>Check-in opens 24 hours before departure.</sa-bubble-content>
                </sa-bubble>
            </sa-message-content>
        </sa-message>
        <sa-message>
            <sa-message-avatar>
                <sa-avatar src="/avatars/avatar-2.jpg" name="Mika Tanaka"/>
            </sa-message-avatar>
            <sa-message-content>
                <sa-bubble variant="BubbleVariant.Secondary">
                    <sa-bubble-content>Let me know if you'd like seats together.</sa-bubble-content>
                </sa-bubble>
            </sa-message-content>
        </sa-message>
    </sa-message-group>
    <sa-message-group>
        <sa-message align="MessageAlign.End">
            <sa-message-avatar></sa-message-avatar>
            <sa-message-content>
                <sa-bubble>
                    <sa-bubble-content>Yes please — two seats by the window.</sa-bubble-content>
                </sa-bubble>
            </sa-message-content>
        </sa-message>
        <sa-message align="MessageAlign.End">
            <sa-message-avatar>
                <sa-avatar src="/avatars/avatar-1.jpg" name="Priya Raman"/>
            </sa-message-avatar>
            <sa-message-content>
                <sa-bubble>
                    <sa-bubble-content>And a vegetarian meal for both of us.</sa-bubble-content>
                </sa-bubble>
            </sa-message-content>
        </sa-message>
    </sa-message-group>
</div>
```

*From `Pages/Message/_HeaderFooter.cshtml`*

```razor
<div class="flex w-full flex-col gap-6">
    <sa-message>
        <sa-message-avatar>
            <sa-avatar src="/avatars/avatar-2.jpg" name="Mika Tanaka"/>
        </sa-message-avatar>
        <sa-message-content>
            <sa-message-header>Mika Tanaka</sa-message-header>
            <sa-bubble variant="BubbleVariant.Secondary">
                <sa-bubble-content>
                    I've moved the airport transfer to 09:15 so you can make the earlier train.
                </sa-bubble-content>
            </sa-bubble>
            <sa-message-footer>Yesterday at 16:42</sa-message-footer>
        </sa-message-content>
    </sa-message>
    <sa-message align="MessageAlign.End">
        <sa-message-avatar>
            <sa-avatar src="/avatars/avatar-1.jpg" name="Priya Raman"/>
        </sa-message-avatar>
        <sa-message-content>
            <sa-message-header>Priya Raman</sa-message-header>
            <sa-bubble>
                <sa-bubble-content>
                    Perfect. Please send the driver's details the night before.
                </sa-bubble-content>
            </sa-bubble>
            <sa-message-footer>Read</sa-message-footer>
        </sa-message-content>
    </sa-message>
</div>
```

*From `Pages/Message/_Actions.cshtml`*

```razor
<div class="flex w-full flex-col gap-6">
    <sa-message>
        <sa-message-avatar>
            <sa-avatar src="/avatars/avatar-2.jpg" name="Mika Tanaka"/>
        </sa-message-avatar>
        <sa-message-content>
            <sa-bubble variant="BubbleVariant.Secondary">
                <sa-bubble-content>
                    Booking reference VYG-4471-KX. Quote it at the ryokan front desk.
                </sa-bubble-content>
            </sa-bubble>
            <sa-message-footer class="gap-1">
                <sa-button size="ButtonSize.IconExtraSmall" variant="ButtonVariant.Ghost" aria-label="Copy booking reference">
                    <sa-icon name="copy"/>
                </sa-button>
                <sa-button size="ButtonSize.IconExtraSmall" variant="ButtonVariant.Ghost" aria-label="Pin message">
                    <sa-icon name="pin"/>
                </sa-button>
            </sa-message-footer>
        </sa-message-content>
    </sa-message>
    <sa-message align="MessageAlign.End">
        <sa-message-avatar>
            <sa-avatar src="/avatars/avatar-1.jpg" name="Priya Raman"/>
        </sa-message-avatar>
        <sa-message-content>
            <sa-bubble variant="BubbleVariant.Destructive">
                <sa-bubble-content>
                    Can you add the airport lounge pass to the booking?
                </sa-bubble-content>
            </sa-bubble>
            <sa-message-footer class="gap-1">
                <span>Failed to send</span>
                <sa-button size="ButtonSize.IconExtraSmall" variant="ButtonVariant.Ghost" aria-label="Retry sending message">
                    <sa-icon name="refresh-cw"/>
                </sa-button>
            </sa-message-footer>
        </sa-message-content>
    </sa-message>
</div>
```
