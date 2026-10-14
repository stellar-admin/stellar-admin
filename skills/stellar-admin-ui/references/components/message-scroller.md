---
component: MessageScroller
tags: [sa-message-scroller, sa-message-scroller-button, sa-message-scroller-content, sa-message-scroller-item, sa-message-scroller-viewport]
generated: true
---

# MessageScroller

A scrolling frame for a conversation, holding the transcript and its scroll controls.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-message-scroller>` | A scrolling frame for a conversation, holding the transcript and its scroll controls. |
| `<sa-message-scroller-button>` | A control that scrolls a message scroller to the start or the end of its transcript. |
| `<sa-message-scroller-content>` | The transcript inside a message scroller, stacking its messages. |
| `<sa-message-scroller-item>` | A single row of a message scroller's transcript, wrapping one message. |
| `<sa-message-scroller-viewport>` | The scrolling region of a message scroller, wrapping the transcript. |

## Attributes

### `<sa-message-scroller>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `auto-scroll` | `bool` | `true` | `true`, `false` |
| `initial-position` | `MessageScrollerPosition` | `End` | `Start`, `End` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

### `<sa-message-scroller-button>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `direction` | `MessageScrollerDirection` | `End` | `Start`, `End` |
| `size` | `ButtonSize` | `IconSmall` | `Default`, `ExtraSmall`, `Small`, `Large`, `Icon`, `IconExtraSmall`, `IconSmall`, `IconLarge` |
| `variant` | `ButtonVariant` | `Secondary` | `Default`, `Destructive`, `Outline`, `Secondary`, `Ghost`, `Link` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/MessageScroller/_Intro.cshtml`*

```razor
<div class="h-[28rem] w-full">
    <sa-message-scroller>
        <sa-message-scroller-viewport aria-label="Conversation with Mika Tanaka">
            <sa-message-scroller-content class="px-1 py-2">
                <sa-message-scroller-item>
                    <sa-marker variant="MarkerVariant.Separator">
                        <sa-marker-content>Tuesday, 12 April</sa-marker-content>
                    </sa-marker>
                </sa-message-scroller-item>
                <sa-message-scroller-item>
                    <sa-message>
                        <sa-message-avatar>
                            <sa-avatar src="/avatars/avatar-2.jpg" name="Mika Tanaka"/>
                        </sa-message-avatar>
                        <sa-message-content>
                            <sa-message-header>Mika Tanaka</sa-message-header>
                            <sa-bubble variant="BubbleVariant.Secondary">
                                <sa-bubble-content>
                                    Morning! Your Kyoto itinerary is ready - 12 to 19 April.
                                </sa-bubble-content>
                            </sa-bubble>
                        </sa-message-content>
                    </sa-message>
                </sa-message-scroller-item>
                <sa-message-scroller-item>
                    <sa-message align="MessageAlign.End">
                        <sa-message-avatar>
                            <sa-avatar src="/avatars/avatar-1.jpg" name="Priya Raman"/>
                        </sa-message-avatar>
                        <sa-message-content>
                            <sa-bubble>
                                <sa-bubble-content>
                                    Wonderful. Does it still include the Arashiyama day trip?
                                </sa-bubble-content>
                            </sa-bubble>
                        </sa-message-content>
                    </sa-message>
                </sa-message-scroller-item>
                <sa-message-scroller-item>
                    <sa-message>
                        <sa-message-avatar>
                            <sa-avatar src="/avatars/avatar-2.jpg" name="Mika Tanaka"/>
                        </sa-message-avatar>
                        <sa-message-content>
                            <sa-bubble variant="BubbleVariant.Secondary">
                                <sa-bubble-content>
                                    It does - the bamboo grove first thing, then the Sagano railway at 14:00.
                                </sa-bubble-content>
                            </sa-bubble>
                        </sa-message-content>
                    </sa-message>
                </sa-message-scroller-item>
                <sa-message-scroller-item>
                    <sa-marker>
                        <sa-marker-icon>
                            <sa-icon name="ticket"/>
                        </sa-marker-icon>
                        <sa-marker-content>E-tickets issued for 2 travellers</sa-marker-content>
                    </sa-marker>
                </sa-message-scroller-item>
                <sa-message-scroller-item>
                    <sa-message align="MessageAlign.End">
                        <sa-message-avatar>
                            <sa-avatar src="/avatars/avatar-1.jpg" name="Priya Raman"/>
                        </sa-message-avatar>
                        <sa-message-content>
                            <sa-bubble>
                                <sa-bubble-content>
                                    Perfect. Could you add a vegetarian meal on both flights?
                                </sa-bubble-content>
                            </sa-bubble>
                        </sa-message-content>
                    </sa-message>
                </sa-message-scroller-item>
                <sa-message-scroller-item>
                    <sa-message>
                        <sa-message-avatar>
                            <sa-avatar src="/avatars/avatar-2.jpg" name="Mika Tanaka"/>
                        </sa-message-avatar>
                        <sa-message-content>
                            <sa-bubble variant="BubbleVariant.Secondary">
                                <sa-bubble-content>
                                    Added to both. The airline confirms special meals 48 hours before departure.
                                </sa-bubble-content>
                            </sa-bubble>
                            <sa-message-footer>16:42</sa-message-footer>
                        </sa-message-content>
                    </sa-message>
                </sa-message-scroller-item>
            </sa-message-scroller-content>
        </sa-message-scroller-viewport>
        <sa-message-scroller-button/>
    </sa-message-scroller>
</div>
```

*From `Pages/MessageScroller/_InitialPosition.cshtml`*

```razor
<div class="h-80 w-full">
    <sa-message-scroller initial-position="MessageScrollerPosition.Start" auto-scroll="false">
        <sa-message-scroller-viewport aria-label="Booking history">
            <sa-message-scroller-content class="px-1 py-2">
                <sa-message-scroller-item>
                    <sa-marker variant="MarkerVariant.Separator">
                        <sa-marker-content>Where this booking started</sa-marker-content>
                    </sa-marker>
                </sa-message-scroller-item>
                <sa-message-scroller-item>
                    <sa-message>
                        <sa-message-avatar>
                            <sa-avatar src="/avatars/avatar-1.jpg" name="Priya Raman"/>
                        </sa-message-avatar>
                        <sa-message-content>
                            <sa-bubble variant="BubbleVariant.Secondary">
                                <sa-bubble-content>
                                    We are two travellers looking at Kyoto in April, ideally during the blossom.
                                </sa-bubble-content>
                            </sa-bubble>
                        </sa-message-content>
                    </sa-message>
                </sa-message-scroller-item>
                <sa-message-scroller-item>
                    <sa-message align="MessageAlign.End">
                        <sa-message-avatar>
                            <sa-avatar src="/avatars/avatar-2.jpg" name="Mika Tanaka"/>
                        </sa-message-avatar>
                        <sa-message-content>
                            <sa-bubble>
                                <sa-bubble-content>
                                    The second week of April is the safest bet. Shall I hold two seats on NH212?
                                </sa-bubble-content>
                            </sa-bubble>
                        </sa-message-content>
                    </sa-message>
                </sa-message-scroller-item>
                <sa-message-scroller-item>
                    <sa-message>
                        <sa-message-avatar>
                            <sa-avatar src="/avatars/avatar-1.jpg" name="Priya Raman"/>
                        </sa-message-avatar>
                        <sa-message-content>
                            <sa-bubble variant="BubbleVariant.Secondary">
                                <sa-bubble-content>
                                    Please do. We can confirm the ryokan once the flights are held.
                                </sa-bubble-content>
                            </sa-bubble>
                        </sa-message-content>
                    </sa-message>
                </sa-message-scroller-item>
                <sa-message-scroller-item>
                    <sa-marker>
                        <sa-marker-icon>
                            <sa-icon name="plane-takeoff"/>
                        </sa-marker-icon>
                        <sa-marker-content>Seats held on NH212, London to Tokyo</sa-marker-content>
                    </sa-marker>
                </sa-message-scroller-item>
                <sa-message-scroller-item>
                    <sa-message align="MessageAlign.End">
                        <sa-message-avatar>
                            <sa-avatar src="/avatars/avatar-2.jpg" name="Mika Tanaka"/>
                        </sa-message-avatar>
                        <sa-message-content>
                            <sa-bubble>
                                <sa-bubble-content>
                                    Held until Friday. The Kanra Kyoto has a garden room free for the same nights.
                                </sa-bubble-content>
                            </sa-bubble>
                        </sa-message-content>
                    </sa-message>
                </sa-message-scroller-item>
                <sa-message-scroller-item>
                    <sa-message>
                        <sa-message-avatar>
                            <sa-avatar src="/avatars/avatar-1.jpg" name="Priya Raman"/>
                        </sa-message-avatar>
                        <sa-message-content>
                            <sa-bubble variant="BubbleVariant.Secondary">
                                <sa-bubble-content>
                                    Take it. We would rather be near the river than close to the station.
                                </sa-bubble-content>
                            </sa-bubble>
                        </sa-message-content>
                    </sa-message>
                </sa-message-scroller-item>
                <sa-message-scroller-item>
                    <sa-marker>
                        <sa-marker-icon>
                            <sa-icon name="hotel"/>
                        </sa-marker-icon>
                        <sa-marker-content>Garden room confirmed, 12 to 14 April</sa-marker-content>
                    </sa-marker>
                </sa-message-scroller-item>
                <sa-message-scroller-item>
                    <sa-message align="MessageAlign.End">
                        <sa-message-avatar>
                            <sa-avatar src="/avatars/avatar-2.jpg" name="Mika Tanaka"/>
                        </sa-message-avatar>
                        <sa-message-content>
                            <sa-bubble>
                                <sa-bubble-content>
                                    That is the whole trip booked. I will send the itinerary tomorrow morning.
                                </sa-bubble-content>
                            </sa-bubble>
                        </sa-message-content>
                    </sa-message>
                </sa-message-scroller-item>
            </sa-message-scroller-content>
        </sa-message-scroller-viewport>
        <sa-message-scroller-button/>
    </sa-message-scroller>
</div>
```

*From `Pages/MessageScroller/_Directions.cshtml`*

```razor
<div class="h-80 w-full">
    <sa-message-scroller>
        <sa-message-scroller-viewport aria-label="Trip notes">
            <sa-message-scroller-content class="px-1 py-2">
                <sa-message-scroller-item>
                    <sa-marker variant="MarkerVariant.Separator">
                        <sa-marker-content>Trip notes</sa-marker-content>
                    </sa-marker>
                </sa-message-scroller-item>
                <sa-message-scroller-item>
                    <sa-marker>
                        <sa-marker-icon>
                            <sa-icon name="hotel"/>
                        </sa-marker-icon>
                        <sa-marker-content>Two nights at the Kanra Kyoto, 12 to 14 April</sa-marker-content>
                    </sa-marker>
                </sa-message-scroller-item>
                <sa-message-scroller-item>
                    <sa-marker>
                        <sa-marker-icon>
                            <sa-icon name="map-pin"/>
                        </sa-marker-icon>
                        <sa-marker-content>Arashiyama bamboo grove, early morning on the 13th</sa-marker-content>
                    </sa-marker>
                </sa-message-scroller-item>
                <sa-message-scroller-item>
                    <sa-marker>
                        <sa-marker-icon>
                            <sa-icon name="ticket"/>
                        </sa-marker-icon>
                        <sa-marker-content>Sagano railway, 14:00 departure, carriage 3</sa-marker-content>
                    </sa-marker>
                </sa-message-scroller-item>
                <sa-message-scroller-item>
                    <sa-marker>
                        <sa-marker-icon>
                            <sa-icon name="credit-card"/>
                        </sa-marker-icon>
                        <sa-marker-content>Balance of EUR 1,240 due by 28 March</sa-marker-content>
                    </sa-marker>
                </sa-message-scroller-item>
                <sa-message-scroller-item>
                    <sa-marker>
                        <sa-marker-icon>
                            <sa-icon name="plane-takeoff"/>
                        </sa-marker-icon>
                        <sa-marker-content>Return NH211 departs Haneda 09:55 on the 19th</sa-marker-content>
                    </sa-marker>
                </sa-message-scroller-item>
                <sa-message-scroller-item>
                    <sa-marker>
                        <sa-marker-icon>
                            <sa-icon name="file-text"/>
                        </sa-marker-icon>
                        <sa-marker-content>Travel insurance certificate attached to the booking</sa-marker-content>
                    </sa-marker>
                </sa-message-scroller-item>
                <sa-message-scroller-item>
                    <sa-marker>
                        <sa-marker-icon>
                            <sa-icon name="map-pin"/>
                        </sa-marker-icon>
                        <sa-marker-content>Fushimi Inari at first light on the 15th, guide booked</sa-marker-content>
                    </sa-marker>
                </sa-message-scroller-item>
                <sa-message-scroller-item>
                    <sa-marker>
                        <sa-marker-icon>
                            <sa-icon name="user"/>
                        </sa-marker-icon>
                        <sa-marker-content>Vegetarian meals requested for both travellers</sa-marker-content>
                    </sa-marker>
                </sa-message-scroller-item>
                <sa-message-scroller-item>
                    <sa-marker>
                        <sa-marker-icon>
                            <sa-icon name="ticket"/>
                        </sa-marker-icon>
                        <sa-marker-content>Airport transfer at 09:15, driver details to follow</sa-marker-content>
                    </sa-marker>
                </sa-message-scroller-item>
                <sa-message-scroller-item>
                    <sa-marker variant="MarkerVariant.Separator">
                        <sa-marker-content>End of notes</sa-marker-content>
                    </sa-marker>
                </sa-message-scroller-item>
            </sa-message-scroller-content>
        </sa-message-scroller-viewport>
        <sa-message-scroller-button direction="MessageScrollerDirection.Start"/>
        <sa-message-scroller-button direction="MessageScrollerDirection.End"/>
    </sa-message-scroller>
</div>
```

*From `Pages/MessageScroller/_NewMessages.cshtml`*

```razor
<div class="flex h-[26rem] w-full flex-col gap-3">
    <div class="min-h-0 flex-1">
        <sa-message-scroller>
            <sa-message-scroller-viewport aria-label="Conversation with Mika Tanaka">
                <sa-message-scroller-content id="--message-scroller-transcript" class="px-1 py-2">
                    <sa-message-scroller-item>
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
                    </sa-message-scroller-item>
                    <sa-message-scroller-item>
                        <sa-message>
                            <sa-message-avatar>
                                <sa-avatar src="/avatars/avatar-2.jpg" name="Mika Tanaka"/>
                            </sa-message-avatar>
                            <sa-message-content>
                                <sa-bubble variant="BubbleVariant.Secondary">
                                    <sa-bubble-content>
                                        Checking with the operator now - one moment.
                                    </sa-bubble-content>
                                </sa-bubble>
                            </sa-message-content>
                        </sa-message>
                    </sa-message-scroller-item>
                </sa-message-scroller-content>
            </sa-message-scroller-viewport>
            <sa-message-scroller-button/>
        </sa-message-scroller>
    </div>
    <div class="flex justify-center">
        <sa-button id="--message-scroller-receive" variant="ButtonVariant.Outline" size="ButtonSize.Small">
            <sa-icon name="message-circle"/>
            Receive a message
        </sa-button>
    </div>
</div>

<template id="--message-scroller-template">
    <sa-message-scroller-item>
        <sa-message>
            <sa-message-avatar>
                <sa-avatar src="/avatars/avatar-2.jpg" name="Mika Tanaka"/>
            </sa-message-avatar>
            <sa-message-content>
                <sa-bubble variant="BubbleVariant.Secondary">
                    <sa-bubble-content></sa-bubble-content>
                </sa-bubble>
            </sa-message-content>
        </sa-message>
    </sa-message-scroller-item>
</template>

<script>
    (() => {
        const transcript = document.getElementById("--message-scroller-transcript");
        const template = document.getElementById("--message-scroller-template");
        const replies = [
            "Two seats are still open on the 14:00 - carriage 3.",
            "Held them under booking VYG-4471-KX for the next hour.",
            "E-tickets are on their way to your inbox now.",
            "Anything else you would like me to sort out before you fly?",
        ];
        let next = 0;

        // Appending to the transcript is all the scroller needs: it watches the DOM, so the
        // same code works whether the markup comes from a fetch, an htmx swap or a template.
        document.getElementById("--message-scroller-receive").addEventListener("click", () => {
            const item = template.content.cloneNode(true);
            item.querySelector('[data-slot="bubble-content"]').textContent = replies[next % replies.length];
            transcript.append(item);
            next++;
        });
    })();
</script>
```
