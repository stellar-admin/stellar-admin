---
component: Timeline
tags: [sa-timeline, sa-timeline-item, sa-timeline-item-actions, sa-timeline-item-description, sa-timeline-item-media, sa-timeline-item-time, sa-timeline-item-title]
generated: true
---

# Timeline

Renders an ordered list of events, each with a marker joined to the next by a rail.

<!-- structure:begin -->
## Composition and behavior

Put `<sa-timeline-item>` elements directly in `<sa-timeline>`. An item takes its title, time and description from the `title`, `time` and `description` attributes, or from `<sa-timeline-item-title>`, `<sa-timeline-item-time>` and `<sa-timeline-item-description>` children; using an attribute and its own child part together throws. Use `<sa-timeline-item-time datetime="...">` (a `DateTimeOffset`) for a machine-readable `<time>`, and `<sa-timeline-item-title muted="true">` to quiet system events among comments. A timeline has no status: use a checklist for steps a user works through.

An item with no media shows the timeline's `marker` (`Dot`, `Ring` or `Number`). `<sa-timeline-item-media>` replaces the marker with your own `<sa-icon>` (drawn per the timeline's `media-style`: `Disc`, `Plain` or `Ring`; colour it with a class) or an `<sa-avatar size="AvatarSize.Small">`. Give the media a `label` (such as "Failed") when the icon adds meaning the title doesn't carry; it is read before the title.

`time-placement` puts the time at the end of the title row (`End`), under the title (`Below`), or in an aligned column before the markers (`Start`). `rail` on an item styles the line to the next item (`Solid`, `Dashed` or `None`). `current="true"` marks an item as current (`aria-current="step"`): it, the items before it and the rails between them take the accent colour. `<sa-timeline-item-actions>` holds trailing buttons on the title row. Any other child of an item (an `<sa-collapsible>` toggled by an `<sa-button commandfor>` in the actions, a comment box) lays out under the text. There is no group part: separate days are several timelines with ordinary headings.
<!-- structure:end -->

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-timeline>` | Renders an ordered list of events, each with a marker joined to the next by a rail. |
| `<sa-timeline-item>` | Renders an event in a timeline: its marker, a title, time and description, and anything else the item holds. |
| `<sa-timeline-item-actions>` | Renders the area at the end of a timeline item's title row that holds actions, such as buttons. |
| `<sa-timeline-item-description>` | Renders the description of a timeline item, shown under its title. |
| `<sa-timeline-item-media>` | Renders an icon or avatar in place of a timeline item's marker. |
| `<sa-timeline-item-time>` | Renders the time of a timeline item as a `<time>` element. |
| `<sa-timeline-item-title>` | Renders the title of a timeline item. |

## Attributes

### `<sa-timeline>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `marker` | `TimelineMarker` | `Dot` | `Dot`, `Ring`, `Number` |
| `media-style` | `TimelineMediaStyle` | `Disc` | `Disc`, `Plain`, `Ring` |
| `size` | `TimelineSize` | `Default` | `Default`, `Small` |
| `time-placement` | `TimelineTimePlacement` | `End` | `End`, `Below`, `Start` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

### `<sa-timeline-item>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `current` | `bool` | `false` | `true`, `false` |
| `description` | `string` | — | — |
| `rail` | `TimelineRail` | `Solid` | `Solid`, `Dashed`, `None` |
| `time` | `string` | — | — |
| `title` | `string` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-timeline-item-media>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `label` | `string` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-timeline-item-time>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `datetime` | `DateTimeOffset` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-timeline-item-title>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `muted` | `bool` | `false` | `true`, `false` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/Timeline/_Intro.cshtml`*

```razor
<div class="grid gap-3">
    <div class="flex items-baseline justify-between gap-2 text-sm font-semibold">
        Booking VT-2481
        <span class="text-xs font-normal text-muted-foreground">Kansai in ten days</span>
    </div>
    <sa-timeline>
        <sa-timeline-item title="Booked" description="Online, by Amara Okafor." time="2 Mar, 09:14">
            <sa-timeline-item-media><sa-icon name="plus"/></sa-timeline-item-media>
        </sa-timeline-item>
        <sa-timeline-item title="Payment received" description="R 48,200 by card ending 4417." time="2 Mar, 09:15">
            <sa-timeline-item-media><sa-icon name="credit-card"/></sa-timeline-item-media>
        </sa-timeline-item>
        <sa-timeline-item title="Tickets issued" description="Emirates, e-tickets sent to both travellers." time="3 Mar, 11:02">
            <sa-timeline-item-media><sa-icon name="ticket"/></sa-timeline-item-media>
        </sa-timeline-item>
        <sa-timeline-item title="Pre-departure pack sent" time="Today">
            <sa-timeline-item-media><sa-icon name="mail"/></sa-timeline-item-media>
        </sa-timeline-item>
        <sa-timeline-item title="Seat changed" description="Kenji Okafor: 32A → 14C." time="Today">
            <sa-timeline-item-media><sa-icon name="pencil"/></sa-timeline-item-media>
        </sa-timeline-item>
    </sa-timeline>
</div>
```

*From `Pages/Timeline/_Avatars.cshtml`*

```razor
<sa-timeline>
    <sa-timeline-item title="Amara Okafor added a traveller" description="Kenji Okafor, adult.">
        <sa-timeline-item-media><sa-avatar name="Amara Okafor" size="AvatarSize.Small"/></sa-timeline-item-media>
        <sa-timeline-item-time datetime="new DateTimeOffset(2026, 10, 20, 9, 40, 0, TimeSpan.Zero)">5 min ago</sa-timeline-item-time>
    </sa-timeline-item>
    <sa-timeline-item title="Lindiwe Mokoena changed the hotel" description="Gion Garden Hotel → Ryokan Hanaya.">
        <sa-timeline-item-media><sa-avatar src="/avatars/avatar-2.jpg" size="AvatarSize.Small"/></sa-timeline-item-media>
        <sa-timeline-item-time datetime="new DateTimeOffset(2026, 10, 20, 8, 45, 0, TimeSpan.Zero)">1 h ago</sa-timeline-item-time>
    </sa-timeline-item>
    <sa-timeline-item title="Voyager sent the payment receipt">
        <sa-timeline-item-media><sa-avatar initials="VT" size="AvatarSize.Small"/></sa-timeline-item-media>
        <sa-timeline-item-time datetime="new DateTimeOffset(2026, 3, 2, 9, 15, 0, TimeSpan.Zero)">2 Mar</sa-timeline-item-time>
    </sa-timeline-item>
    <sa-timeline-item title="Amara Okafor created the booking">
        <sa-timeline-item-media><sa-avatar name="Amara Okafor" size="AvatarSize.Small"/></sa-timeline-item-media>
        <sa-timeline-item-time datetime="new DateTimeOffset(2026, 3, 2, 9, 14, 0, TimeSpan.Zero)">2 Mar</sa-timeline-item-time>
    </sa-timeline-item>
</sa-timeline>
```

*From `Pages/Timeline/_Expandable.cshtml`*

```razor
<sa-timeline>
    <sa-timeline-item title="Price changed" description="By Lindiwe Mokoena." time="Today 10:02">
        <sa-timeline-item-media><sa-icon name="pencil"/></sa-timeline-item-media>
        <sa-timeline-item-actions>
            <sa-button variant="ButtonVariant.Ghost" size="ButtonSize.Small" commandfor="change-81" command="--toggle">
                Changes
                <sa-icon name="chevron-down" class="transition-transform in-aria-expanded:rotate-180"/>
            </sa-button>
        </sa-timeline-item-actions>
        <sa-collapsible id="change-81">
            <dl class="grid grid-cols-[max-content_1fr] gap-x-3 gap-y-1 rounded-md bg-muted p-3 text-xs">
                <dt class="text-muted-foreground">Price per person</dt>
                <dd><del class="text-destructive">¥178,000</del> → ¥186,000</dd>
                <dt class="text-muted-foreground">Single supplement</dt>
                <dd><del class="text-destructive">¥24,000</del> → ¥28,000</dd>
            </dl>
        </sa-collapsible>
    </sa-timeline-item>
    <sa-timeline-item title="Day added" description="By Amara Okafor." time="Yesterday">
        <sa-timeline-item-media><sa-icon name="plus"/></sa-timeline-item-media>
        <sa-timeline-item-actions>
            <sa-button variant="ButtonVariant.Ghost" size="ButtonSize.Small" commandfor="change-80" command="--toggle">
                Changes
                <sa-icon name="chevron-down" class="transition-transform in-aria-expanded:rotate-180"/>
            </sa-button>
        </sa-timeline-item-actions>
        <sa-collapsible id="change-80" hidden="">
            <dl class="grid grid-cols-[max-content_1fr] gap-x-3 gap-y-1 rounded-md bg-muted p-3 text-xs">
                <dt class="text-muted-foreground">Day 6</dt>
                <dd>Nishiki Market food tour</dd>
            </dl>
        </sa-collapsible>
    </sa-timeline-item>
    <sa-timeline-item title="Photo removed" description="By Amara Okafor." time="3 Mar">
        <sa-timeline-item-media><sa-icon name="trash"/></sa-timeline-item-media>
    </sa-timeline-item>
    <sa-timeline-item title="Tour created" description="By Amara Okafor." time="2 Mar">
        <sa-timeline-item-media><sa-icon name="plus"/></sa-timeline-item-media>
    </sa-timeline-item>
</sa-timeline>
```

*From `Pages/Timeline/_Failure.cshtml`*

```razor
<div class="grid gap-3">
    <div class="flex items-baseline justify-between gap-2 text-sm font-semibold">
        Refund RF-118
        <span class="text-xs font-normal text-muted-foreground">R 12,400</span>
    </div>
    <sa-timeline>
        <sa-timeline-item title="Refund requested" description="Flight cancelled by the airline." time="9 Mar">
            <sa-timeline-item-media><sa-icon name="undo-2"/></sa-timeline-item-media>
        </sa-timeline-item>
        <sa-timeline-item title="Approved" description="By Lindiwe Mokoena." time="10 Mar">
            <sa-timeline-item-media label="Approved">
                <sa-icon name="check" class="text-[var(--sa-color-success)]"/>
            </sa-timeline-item-media>
        </sa-timeline-item>
        <sa-timeline-item title="Payout to the traveller" description="The card ending 4417 has expired." time="11 Mar">
            <sa-timeline-item-media label="Failed">
                <sa-icon name="circle-x" class="text-destructive"/>
            </sa-timeline-item-media>
            <sa-timeline-item-actions>
                <sa-button size="ButtonSize.Small">Retry</sa-button>
            </sa-timeline-item-actions>
        </sa-timeline-item>
    </sa-timeline>
</div>
```

*From `Pages/Timeline/_Progress.cshtml`*

```razor
<div class="grid gap-3">
    <div class="flex items-baseline justify-between gap-2 text-sm font-semibold">
        Kansai in ten days
        <span class="text-xs font-normal text-muted-foreground">Day 2 of 10</span>
    </div>
    <sa-timeline time-placement="TimelineTimePlacement.Start">
        <sa-timeline-item title="Arrive at Kansai International" time="Day 1">
            <sa-timeline-item-media><sa-icon name="plane"/></sa-timeline-item-media>
        </sa-timeline-item>
        <sa-timeline-item title="Haruka express to Kyoto" time="Day 1">
            <sa-timeline-item-media><sa-icon name="train-front"/></sa-timeline-item-media>
        </sa-timeline-item>
        <sa-timeline-item title="Ryokan Hanaya, Gion" description="You are here. Check-out is at 10:00 on Day 4." time="Day 1–3" current="true">
            <sa-timeline-item-media><sa-icon name="bed"/></sa-timeline-item-media>
        </sa-timeline-item>
        <sa-timeline-item title="Kumano Kodo day hike" description="Optional; book by Day 3." time="Day 5" rail="TimelineRail.Dashed">
            <sa-timeline-item-media><sa-icon name="mountain"/></sa-timeline-item-media>
        </sa-timeline-item>
        <sa-timeline-item title="Nishiki Market food tour" time="Day 6" rail="TimelineRail.None">
            <sa-timeline-item-media><sa-icon name="utensils"/></sa-timeline-item-media>
        </sa-timeline-item>
        <sa-timeline-item title="Fly home" description="EK 317, 23:45." time="Day 10">
            <sa-timeline-item-media><sa-icon name="plane"/></sa-timeline-item-media>
        </sa-timeline-item>
    </sa-timeline>
</div>
```
