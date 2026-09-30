---
component: Toaster
tags: [sa-toaster]
generated: true
---

# Toaster

The region that shows toast notifications. Place one in the layout, outside any region that AJAX requests replace.

<!-- structure:begin -->
## Composition and behavior

Put one `<sa-toaster />` in the layout, outside any region that htmx or another AJAX library swaps (including the body with `hx-boost`). `AddTagHelpers()` registers the rest; there is no setup call or middleware. Toasts come from two places:

- **Server:** inject `IToastNotifier` into a controller, page model or service and call `Success`, `Info`, `Warning` or `Error(title, description?)`, or `Add(new Toast { Title, Description, Type = ToastType.X, Duration, Action = ToastAction.Link(label, href) })`. Only link actions are possible from the server.
- **Browser:** `window.stellarAdmin.toast` (see javascript.md).

Server toasts are queued in TempData and delivered by response type: on a full page load `<sa-toaster>` renders them into the page; before a redirect they carry over to the target page; on an AJAX request (`Sec-Fetch-Mode` is not `navigate`) they go into the `SA-Toasts` response header, which the page must pass to `window.stellarAdmin.toast.fromResponse(...)`. Nothing intercepts requests automatically, so wire one listener for the app's AJAX library (javascript.md). Add toasts in actions or handlers, not in views: toasts added while a view renders miss the current header and appear on the next response. htmx's `HX-Redirect`/`HX-Location` return a 200, so a toast added there is lost when the page navigates; use a normal redirect instead.

Toasts close after 5 s by default (`TimeSpan.Zero` or `duration: 0` keeps one open); loading toasts stay open until updated. Timers pause on hover, focus and while the tab or window is in the background. At most `limit` toasts are visible at once. F6 focuses the toaster, Esc closes the focused toast, and toasts can be swiped away.
<!-- structure:end -->

## Attributes

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `duration` | `TimeSpan` | `TimeSpan.FromSeconds(5)` | — |
| `limit` | `int` | `3` | — |
| `position` | `ToasterPosition` | `BottomRight` | `TopLeft`, `TopCenter`, `TopRight`, `BottomLeft`, `BottomCenter`, `BottomRight` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

## Examples

*From `Pages/Toast/_Intro.cshtml`*

```razor
<sa-button variant="ButtonVariant.Outline" id="--toast-intro-button">Save itinerary</sa-button>
<!-- Add the toaster once, in your layout -->
<sa-toaster />
<script type="module">
    document.getElementById("--toast-intro-button").addEventListener("click", () =>
        window.stellarAdmin.toast.success("Itinerary saved", "Your Lisbon trip is ready to share."));
</script>
```

*From `Pages/Toast/_Types.cshtml`*

```razor
<div class="flex flex-wrap justify-center gap-2">
    <sa-button variant="ButtonVariant.Outline" data-toast-type="default">Default</sa-button>
    <sa-button variant="ButtonVariant.Outline" data-toast-type="success">Success</sa-button>
    <sa-button variant="ButtonVariant.Outline" data-toast-type="info">Info</sa-button>
    <sa-button variant="ButtonVariant.Outline" data-toast-type="warning">Warning</sa-button>
    <sa-button variant="ButtonVariant.Outline" data-toast-type="error">Error</sa-button>
    <sa-button variant="ButtonVariant.Outline" data-toast-type="loading">Loading</sa-button>
</div>
<sa-toaster />
<script type="module">
    const toast = window.stellarAdmin.toast;
    const types = {
        default: () => toast.add({ title: "Itinerary archived", description: "3 bookings moved to archive." }),
        success: () => toast.success("Booking confirmed", "Your trip to Lisbon departs 14 October."),
        info: () => toast.info("Fare alert", "Flights to Kyoto dropped 12% this week."),
        warning: () => toast.warning("Passport expires soon", "Renew it before your March trip to Cape Town."),
        error: () => toast.error("Payment declined", "Your card ending 4242 was declined."),
        loading: () => toast.add({ title: "Checking seat availability...", type: "loading", duration: 3000 }),
    };

    document.querySelectorAll("[data-toast-type]").forEach((button) =>
        button.addEventListener("click", () => types[button.dataset.toastType]()));
</script>
```

*From `Pages/Toast/_Action.cshtml`*

```razor
<sa-button variant="ButtonVariant.Outline" id="--toast-action-button">Book hotel</sa-button>
<sa-toaster />
<script type="module">
    document.getElementById("--toast-action-button").addEventListener("click", () =>
        window.stellarAdmin.toast.success("Hotel booked", {
            description: "Casa do Mar, Lisbon. 3 nights from 14 October.",
            action: { label: "View", href: "#booking-2048" },
        }));
</script>
```

*From `Pages/Toast/_Promise.cshtml`*

```razor
<sa-button variant="ButtonVariant.Outline" id="--toast-promise-button">Request refund</sa-button>
<sa-toaster />
<script type="module">
    function requestRefund() {
        return new Promise((resolve) => setTimeout(() => resolve({ amount: "€284.00" }), 2000));
    }

    document.getElementById("--toast-promise-button").addEventListener("click", () =>
        window.stellarAdmin.toast.promise(requestRefund(), {
            loading: "Requesting your refund...",
            success: (refund) => `Refund of ${refund.amount} requested`,
            error: "We couldn't request your refund",
        }));
</script>
```
