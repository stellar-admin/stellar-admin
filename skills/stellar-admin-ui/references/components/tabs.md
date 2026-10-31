---
component: Tabs
tags: [sa-tab, sa-tab-link, sa-tab-list, sa-tab-panel, sa-tabs]
generated: true
---

# Tabs

Renders a tab that shows its panel when selected.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-tab>` | Renders a tab that shows its panel when selected. |
| `<sa-tab-link>` | Renders a tab in a `<sa-tab-list>` as a link to another page. |
| `<sa-tab-list>` | Renders a list of tabs. |
| `<sa-tab-panel>` | Renders the panel that a tab shows. |
| `<sa-tabs>` | Renders tabs that switch between panels on the page. |

## Attributes

### `<sa-tab>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `for` | `string` | — | — |
| `disabled` | `bool` | `false` | `true`, `false` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

### `<sa-tab-link>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `disabled` | `bool` | `false` | `true`, `false` |
| `active` | `bool` | — | `true`, `false` |
| `asp-action` | `string` | — | — |
| `asp-area` | `string` | — | — |
| `asp-controller` | `string` | — | — |
| `asp-fragment` | `string` | — | — |
| `asp-host` | `string` | — | — |
| `asp-page` | `string` | — | — |
| `asp-page-handler` | `string` | — | — |
| `asp-protocol` | `string` | — | — |
| `asp-route` | `string` | — | — |
| `asp-all-route-data` | `IDictionary<string, string?>` | — | — |
| `asp-route-*` | `IDictionary<string, string?>` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-tab-list>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `more-label` | `string` | — | — |
| `orientation` | `TabListOrientation` | `Horizontal` | `Horizontal`, `Vertical` |
| `overflow` | `TabListOverflow` | `Menu` | `Menu`, `None` |
| `variant` | `TabListVariant` | `Default` | `Default`, `Line` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-tabs>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `value` | `string` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/Tabs/_Intro.cshtml`*

```razor
<sa-tab-list>
    <sa-tab-link href="#">Flights</sa-tab-link>
    <sa-tab-link href="#">Accommodation</sa-tab-link>
    <sa-tab-link href="#">Car Rental</sa-tab-link>
</sa-tab-list>
```

*From `Pages/Tabs/_Icons.cshtml`*

```razor
<sa-tab-list>
    <sa-tab-link href="#">
        <sa-icon name="plane"/>
        Flights
    </sa-tab-link>
    <sa-tab-link href="#">
        <sa-icon name="bed-double"/>
        Accommodation
    </sa-tab-link>
    <sa-tab-link href="#">
        <sa-icon name="car-front"/>
        Car Rental
    </sa-tab-link>
</sa-tab-list>
```

*From `Pages/Tabs/_Url.cshtml`*

```razor
<sa-tab-list>
    <sa-tab-link asp-controller="Search" asp-route-category="flights">
        Flights
    </sa-tab-link>
    <sa-tab-link asp-controller="Search" asp-route-category="accommodation">
        Accommodation
    </sa-tab-link>
    <sa-tab-link asp-controller="Search" asp-route-category="carrental">
        Car Rental
    </sa-tab-link>
</sa-tab-list>
```

*From `Pages/Tabs/_Panels.cshtml`*

```razor
<sa-tabs class="w-full max-w-md">
    <sa-tab-list>
        <sa-tab for="tabs-panels-flights">Flights</sa-tab>
        <sa-tab for="tabs-panels-accommodation">Accommodation</sa-tab>
        <sa-tab for="tabs-panels-car-rental" disabled="true">Car Rental</sa-tab>
    </sa-tab-list>
    <sa-tab-panel id="tabs-panels-flights">
        <p class="font-medium">Lisbon (LIS) to Reykjavík (KEF)</p>
        <p class="text-muted-foreground text-sm">Departs 14 March at 07:40, arrives 10:15. Economy, 1 checked bag.</p>
    </sa-tab-panel>
    <sa-tab-panel id="tabs-panels-accommodation">
        <p class="font-medium">Hotel Borg, Reykjavík</p>
        <p class="text-muted-foreground text-sm">3 nights from 14 March. Deluxe double room, breakfast included.</p>
    </sa-tab-panel>
    <sa-tab-panel id="tabs-panels-car-rental">
        <p class="font-medium">No car booked</p>
    </sa-tab-panel>
</sa-tabs>
```

*From `Pages/Tabs/_PanelsLoadOnShow.cshtml`*

```razor
<sa-tabs class="w-full max-w-md">
    <sa-tab-list>
        <sa-tab for="tabs-load-summary">Summary</sa-tab>
        <sa-tab for="tabs-load-documents">Documents</sa-tab>
    </sa-tab-list>
    <sa-tab-panel id="tabs-load-summary">
        <p class="font-medium">Booking VYG-4471-KX</p>
        <p class="text-muted-foreground text-sm">Reykjavík, 14 to 17 March. Paid in full.</p>
    </sa-tab-panel>
    <sa-tab-panel id="tabs-load-documents">
        <div class="flex flex-col gap-2">
            <sa-skeleton class="h-4 w-48"/>
            <sa-skeleton class="h-4 w-40"/>
        </div>
    </sa-tab-panel>
</sa-tabs>

<script>
    (() => {
        // With htmx, hx-get="…" hx-trigger="tabshow once" on the panel does the same.
        const panel = document.getElementById("tabs-load-documents");
        panel.addEventListener("tabshow", async () => {
            await new Promise((resolve) => setTimeout(resolve, 800));
            panel.innerHTML = `
                <ul class="text-muted-foreground list-disc space-y-1 ps-5 text-sm">
                    <li>E-ticket LIS to KEF</li>
                    <li>Hotel Borg confirmation</li>
                </ul>`;
        }, { once: true });
    })();
</script>
```
