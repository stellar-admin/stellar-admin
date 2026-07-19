---
component: Collapsible
tags: [sa-collapsible]
generated: true
---

# Collapsible

A container whose content can be expanded or collapsed. Toggle it with the Invoker Commands API — a trigger button carrying `commandfor` and a custom command.

## Attributes

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/Collapsible/_Intro.cshtml`*

```razor
<div class="flex w-[350px] flex-col gap-2 group/collapsible">
    <div class="flex items-center justify-between gap-4">
        <h4 class="text-sm font-semibold">
            @@ibnbattuta has 3 bookings
        </h4>
        <sa-button variant="ButtonVariant.Ghost" size="ButtonSize.Icon" commandfor="my-collapsible" command="--toggle" class="size-8">
            <sa-icon name="chevrons-up-down" class="in-aria-expanded:hidden"/>
            <sa-icon name="chevrons-down-up" class="not-in-aria-expanded:hidden"/>
            <span class="sr-only">Toggle</span>
        </sa-button>
    </div>
    <sa-item variant="ItemVariant.Outline" size="ItemSize.Small">
        <sa-item-media>
            <img
                class="object-cover size-12 rounded"
                src="https://images.unsplash.com/photo-1681785841804-4d8a976ee892?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=2670"/>
        </sa-item-media>
        <sa-item-content>
            <sa-item-title>Non-stop to NYC (JFK)</sa-item-title>
            <sa-item-description>Departure 8AM - Dec 24, 2025</sa-item-description>
        </sa-item-content>
    </sa-item>
    <sa-collapsible id="my-collapsible" hidden="" class="flex flex-col gap-2">
        <sa-item variant="ItemVariant.Outline" size="ItemSize.Small">
            <sa-item-media>
                <img
                    class="object-cover size-12 rounded"
                    src="https://images.unsplash.com/photo-1551882547-ff40c63fe5fa?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=2670"/>
            </sa-item-media>
            <sa-item-content>
                <sa-item-title>The Grand Resort & Spa</sa-item-title>
                <sa-item-description>Check-in Dec 25 | 7 Nights</sa-item-description>
            </sa-item-content>
        </sa-item>
        <sa-item variant="ItemVariant.Outline" size="ItemSize.Small">
            <sa-item-media>
                <img
                    class="object-cover size-12 rounded"
                    src="https://images.unsplash.com/photo-1557223562-6c77ef16210f?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=2670"/>
            </sa-item-media>
            <sa-item-content>
                <sa-item-title>Westminster Abbey Tour</sa-item-title>
                <sa-item-description>Dec 26, 11:00 AM Slot | 2 Adult Tickets</sa-item-description>
            </sa-item-content>
        </sa-item>
    </sa-collapsible>
</div>
```

*From `Pages/Collapsible/_Settings.cshtml`*

```razor
<sa-card class="mx-auto w-[350px]" size="CardSize.Small">
  <sa-card-header>
    <sa-card-title>Radius</sa-card-title>
    <sa-card-description>
      Set the corner radius of the element.
    </sa-card-description>
  </sa-card-header>
  <sa-card-content>
    <div class="flex items-start gap-2 group/collapsible">
      <sa-field-group class="grid w-full grid-cols-2 gap-2">
        <sa-field>
          <sa-field-label for="radius-x-1" class="sr-only">
            Radius X
          </sa-field-label>
          <sa-input id="radius-x-1" placeholder="0"/>
        </sa-field>
        <sa-field>
          <sa-field-label for="radius-y-1" class="sr-only">
            Radius Y
          </sa-field-label>
          <sa-input id="radius-y-1" placeholder="0"/>
        </sa-field>
        <sa-collapsible id="collapsible-settings" hidden="" class="col-span-full grid grid-cols-subgrid gap-2">
          <sa-field>
            <sa-field-label for="radius-x-2" class="sr-only">
              Radius X
            </sa-field-label>
            <sa-input id="radius-x-2" placeholder="0"/>
          </sa-field>
          <sa-field>
            <sa-field-label for="radius-y-2" class="sr-only">
              Radius Y
            </sa-field-label>
            <sa-input id="radius-y-2" placeholder="0"/>
          </sa-field>
        </sa-collapsible>
      </sa-field-group>
      <sa-button variant="ButtonVariant.Outline" size="ButtonSize.Icon" commandfor="collapsible-settings" command="--toggle">
        <sa-icon name="minimize" class="not-in-aria-expanded:hidden"/>
        <sa-icon name="maximize" class="in-aria-expanded:hidden"/>
      </sa-button>
    </div>
  </sa-card-content>
</sa-card>
```
