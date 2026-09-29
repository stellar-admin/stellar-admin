# Toast

Status: **in progress**. Phase 1 is complete and awaiting review. Last updated: 2026-09-30.

Port shadcn's Toast into `StellarAdmin.TagHelpers`, with a server-side API that works for full page loads, redirects and AJAX responses. Work proceeds in phases with a review checkpoint after each; approval of one phase does not authorize the next.

## Resuming

Read this file, then check the current code under the paths in [Source paths](#source-paths) before continuing. Update the phase table and the phase log when a phase finishes or stops partway.

| Phase | Scope | Status |
| --- | --- | --- |
| 1 | Server API: `IToastNotifier`, TempData storage, result filter, `SA-Toasts` header; HTTP integration tests | ☑ |
| 2 | Visual prototype: `sa-toaster`, `sel-toaster`, structural CSS, one theme, static DocsSamples demo | ☐ |
| 3 | Client API (`window.stellarAdmin.toast`), full-page rendering from TempData, htmx demo in ComponentPlayground | ☐ |
| 4 | Theme coverage across all shipped themes and the custom-theme specifications | ☐ |
| 5 | Demos, website docs, consumer skills reference, per-library AJAX guidance, tests, handover | ☐ |

## Research summary

- The current shadcn Toast (July 2026) is built on Base UI's Toast primitives, not Sonner. Sonner is React-only, so neither can be used directly.
- Base UI defaults to a limit of 3 visible toasts and a 5000 ms timeout. It exposes state through `data-*` attributes (`data-type`, `data-expanded`, `data-behind`, `data-limited`, `data-swiping`, `data-starting-style`, `data-ending-style`) and CSS variables (`--toast-index`, `--toast-height`, `--toast-offset-y`, `--toast-frontmost-height`). We reproduce this contract so shadcn's toast styles port with little change.
- Vanilla libraries were evaluated (toastkit, andreruffert's toast-queue, Toastify, Notyf). They are either immature or ship their own look. We build our own `sel-toaster` with no new dependency.
- Prior art for server-side toasts: Django messages moved into `HX-Trigger` by middleware, Rails `turbo_flash` via Turbo Streams, Filament's fluent `Notification::make()`, and NToastNotify's TempData plus AJAX interception. We take the TempData queue and header delivery, but not a library-specific header or automatic interception.

## Approved design

### Decisions

- **No new JS dependency.** `sel-toaster` is a Lit component like the other `sel-*` components.
- **The server API is an injected service on top of TempData.** The storage can be swapped later without changing callers.
- **One server API for every response type.** The library decides whether a queued toast is rendered into the page, carried across a redirect, or sent in the `SA-Toasts` response header.
- **No AJAX-library dependency and no interception.** The client exposes `window.stellarAdmin.toast.fromResponse(...)`. The docs give a short snippet for each supported library. There is no global `fetch`/XHR wrapper.
- **Supported in v1:** MVC and Razor Pages; htmx 2, htmx 4 and plain `fetch` on the client.
- **Out of scope for v1:** minimal APIs, Datastar, and server-side actions that post back (only link actions are supported from the server).

### Setup

`AddTagHelpers()` registers the service and the global result filter, so there is no separate call and no middleware.

```csharp
builder.Services.AddStellarAdmin().AddTagHelpers();
```

```html
<!-- _Layout.cshtml, outside any region that AJAX swaps -->
<sa-toaster position="BottomRight" />
```

### Server API

```csharp
public interface IToastNotifier
{
    void Add(Toast toast);
}

// Extensions: Success, Info, Warning, Error (title, optional description)
notifier.Success("Booking saved", "Your trip to Lisbon is confirmed.");

notifier.Add(new Toast
{
    Title = "Itinerary archived",
    Description = "3 bookings moved to archive.",
    Type = ToastType.Info,
    Duration = TimeSpan.FromSeconds(8),
    Action = ToastAction.Link("Undo", Url.Action("Restore", new { id })),
});
```

- `Toast`:
  - `Title`: required.
  - `Description`: optional.
  - `Type`: a dedicated `ToastType` enum (`Default`, `Success`, `Info`, `Warning`, `Error`). The `loading` type is JS-only, because a server-sent toast can't be updated later.
  - `Duration`: `TimeSpan?`, where null means the toaster default.
  - `Action`: optional, link only in v1.
- The default implementation is scoped and uses `ITempDataDictionaryFactory` with `IHttpContextAccessor`. Toasts are stored as a single JSON string under one TempData key, because TempData only holds simple types. The service writes through to TempData as soon as a toast is added.
- Draining the queue is internal. The filter and `sa-toaster` use it; app code never calls it.

### How a toast is delivered

The global filter implements `IAlwaysRunResultFilter`, and all of its work happens in `OnResultExecuting`. At that point the response hasn't started and MVC's `SaveTempDataFilter` hasn't saved TempData, so both destinations are still open.

```
if result is a redirect (RedirectResult, RedirectToActionResult, RedirectToRouteResult, RedirectToPageResult, LocalRedirectResult)
    → leave in TempData; the next response decides again
else if Sec-Fetch-Mode is "navigate", or the header is missing
    → leave in TempData; <sa-toaster> renders it on this page
else
    → drain into the SA-Toasts header
```

| What happened | Where the toast shows |
| --- | --- |
| Action returns a redirect | The next response, whether a full page or AJAX, via TempData |
| Full page load | Rendered by `<sa-toaster>` from TempData |
| `fetch`/XHR response | `SA-Toasts` header, read by the app's snippet |
| Page without `<sa-toaster>` (a download, an error page) | Stays in TempData until a page with the toaster renders |
| Raised in the browser | `window.stellarAdmin.toast.*` |

- `Sec-Fetch-Mode` is set by the browser and can't be set by script. A missing header is treated as a navigation, so a toast arrives late rather than getting lost.
- Header format: a JSON array in camelCase, serialized with System.Text.Json's default escaping, which keeps the value ASCII. Toasts go into one header until it would pass 4096 bytes, to stay well under the ~8 KB proxy limit. The first toast is always sent, so an oversized toast can't block the queue. Any overflow stays queued for the next response.
- Toasts added after the filter runs are not sent in the current response's header. That includes toasts added while a view renders. They stay in TempData for the next response. The docs say to add toasts in actions or handlers.

### Client API

```js
window.stellarAdmin.toast.success("Copied to clipboard");

const id = window.stellarAdmin.toast.add({ title: "Uploading…", type: "loading", duration: 0 });
window.stellarAdmin.toast.update(id, { title: "Uploaded", type: "success", duration: 5000 });
window.stellarAdmin.toast.close(id);

window.stellarAdmin.toast.promise(upload(), {
  loading: "Uploading…",
  success: "Photos uploaded",
  error: (e) => `Upload failed: ${e.message}`,
});

// Takes a Response, Headers, an XMLHttpRequest, or the raw header string
window.stellarAdmin.toast.fromResponse(res);
```

Per-library guidance for the docs. The htmx 4 event was checked against the Dashboard's `4.0.0-beta6`.

```js
// htmx 4
document.addEventListener("htmx:after:request", (e) =>
  window.stellarAdmin.toast.fromResponse(e.detail.ctx.response.headers));

// htmx 2
document.addEventListener("htmx:afterRequest", (e) =>
  window.stellarAdmin.toast.fromResponse(e.detail.xhr));

// plain fetch, usually inside the app's own fetch helper
const res = await fetch(url, options);
window.stellarAdmin.toast.fromResponse(res);
```

### `sa-toaster` and `sel-toaster`

- **Server markup.** `sa-toaster` renders:
  - `<sel-toaster>` containing the viewport.
  - A `<template>` for a toast, so the markup and classes stay server-owned and themeable.
  - Toasts drained from TempData, as JSON in a `<script type="application/json">`.

  There is one rendering path: JS clones the template for both server and browser toasts.
- **Attributes:**
  - `position`: a dedicated `ToasterPosition` enum; the default is `BottomRight`.
  - `limit`: defaults to 3.
  - `duration`: the default timeout, 5000 ms.
- **Top layer.** The viewport uses `popover="manual"`, so toasts appear above page content and dialogs.
  - A modal dialog opened *after* the toaster sits above it in the top layer, so `sel-toaster` re-shows its popover when a toast is added.
- **Animation.** Enter and exit use `@starting-style` and the `data-starting-style`/`data-ending-style` attributes.
- **Stacking.** Stacked while collapsed, expanded on hover or focus, driven by Base UI's CSS variables.
- **Accessibility:**
  - A polite live region; `Error` toasts use assertive priority.
  - F6 moves focus to the toaster landmark.
  - Timers pause on hover, on focus and while the document is hidden.
  - Every toast has a close button.
- Swipe-to-dismiss is decided in phase 2, based on how much it costs.

## Open decisions

To settle in the phase that needs them:

1. **Naming.** Settled at the phase 1 review: `IToastNotifier` (renamed from `IToasts`), the `SA-Toasts` header and `ToastType`. `ToasterPosition` is decided in phase 2.
2. **Header cap.** Settled in phase 1: a 4096-byte budget rather than a toast count, always sending at least one toast.
3. **Registration.** Phase 1 made it always on through `AddTagHelpers()`, which also calls `AddHttpContextAccessor()`. The filter loads TempData only for non-navigation, non-redirect results; confirm at the phase 1 review.
4. **Swipe-to-dismiss.** Whether to include it. (Phase 2)
5. **Theme stubs.** The shadcn theme files contain a leftover `.sa-toast { @apply rounded-2xl; }` from the Sonner era. Decide whether the ThemeGenerator needs re-running to pick up the Base UI toast rules. (Phase 4)

## Known limitations to document

- Client-side redirects sent as response headers (htmx `HX-Redirect`/`HX-Location`) return a 200. The toast goes into the header, and then the page navigates away. Use a normal redirect when a toast must survive navigation.
- With `hx-boost` or any other body swap, `<sa-toaster>` must sit outside the swapped region.

## Phase plan

### Phase 1: server API

- `Toast`, `ToastType`, `ToastAction`, `IToastNotifier` and its extensions, the TempData-backed implementation, and the result filter, all registered from `AddTagHelpers()`.
- HTTP integration tests (TUnit, following the [unit testing convention](../../conventions/unit-testing.md)) that cover every row of the delivery table. The most important test proves that the filter runs before `SaveTempDataFilter`: a toast drained into the header must not also come back on the next request.
- Unit tests for serialization: ASCII escaping and the cap with overflow.
- No UI in this phase.

### Phase 2: visual prototype

- Follow the [prototype-component](../../../.agents/skills/prototype-component/SKILL.md) workflow.
- `sa-toaster`, the toast template, the `sel-toaster` rendering path from JSON, stacking and animation, structural CSS in `Client/css/components.css`, and one theme.
- A static DocsSamples demo that raises toasts from buttons.
- Verify in headless Chromium on port 5206:
  - Stacking, expanding and the limit.
  - Pause and resume.
  - F6 focus.
  - Toasts appearing over a modal dialog.
  - Reduced motion.

### Phase 3: client API and wiring

- The `window.stellarAdmin.toast` API, including `fromResponse`.
- `sa-toaster` renders the TempData queue on full page loads.
- A ComponentPlayground demo, which already loads htmx:
  - An htmx 4 form post that returns a partial with a toast.
  - A post that redirects.
  - A plain `fetch` call.

### Phase 4: themes

- Themed rules in every shipped theme and in the custom-theme specifications under `docs/design/themes/`.

### Phase 5: documentation and handover

- Follow the [port-shadcn-component](../../../.agents/skills/port-shadcn-component/SKILL.md) workflow for the remaining pieces:
  - DocsSamples demos, re-themed to the Voyager Travel content.
  - Website docs and exports.
  - The consumer skills reference.
  - Per-library AJAX guidance for htmx 2, htmx 4 and `fetch`.
  - Remaining tests.
  - Updating the [component parity](component-parity.md) row.
- Using toasts in the Dashboard (for example after a resource is saved) is a separate task after phase 5.

## Source paths

Phase 1 paths are confirmed; later paths are expected locations.

- `src/StellarAdmin.TagHelpers/StellarAdminTagHelpersExtensions.cs`: service and filter registration.
- `src/StellarAdmin.TagHelpers/Toasts/`: `Toast`, `ToastType`, `ToastAction`, `IToastNotifier`, `ToastNotifierExtensions`, `TempDataToastNotifier`, `ToastResultFilter` and `ToastQueue` (TempData storage and header serialization). All types use the `StellarAdmin.TagHelpers` namespace.
- `tests/StellarAdmin.TagHelpers.IntegrationTests/`: HTTP delivery tests (`Toasts/ToastDeliveryTests.cs`).
- `tests/StellarAdmin.TagHelpers.Tests/Toasts/TempDataToastNotifierTests.cs`: validation unit tests.
- `src/StellarAdmin.TagHelpers/TagHelpers/`: `sa-toaster`.
- `src/StellarAdmin.TagHelpers/Client/js/web-components/sel-toaster.ts` and `Client/js/wrappers/toast.ts`: the client component and API, registered in `Client/js/stellar-admin-ui.ts`.
- `src/StellarAdmin.TagHelpers/Client/css/components.css` and `Client/css/themes/`: styles.

## Phase log

- 2026-09-30: Research and design agreed in conversation. This plan was written, and no code has changed.
- 2026-09-30: Phase 1 implemented, not committed.
  - Added the public `Toast`, `ToastType`, `ToastAction` (`Link` only), `IToastNotifier` and the `Success`/`Info`/`Warning`/`Error` extensions. `TempDataToastNotifier` rejects a blank title, a negative duration, and use outside a request.
  - `AddTagHelpers()` now registers `AddHttpContextAccessor()`, a scoped `IToastNotifier`, and `ToastResultFilter` once in `MvcOptions.Filters`.
  - Redirects are detected through MVC's `IKeepTempDataResult`, which all five redirect results implement.
  - Header JSON: camelCase, nulls omitted, `duration` in milliseconds, `action` as `{label, href}`.
  - Deviation: the plan called for serialization unit tests, but the TagHelpers project has no `InternalsVisibleTo`. Escaping, the byte budget and overflow are tested over HTTP instead.
  - Created `tests/StellarAdmin.TagHelpers.IntegrationTests` (Razor SDK, TUnit, TestServer) through the .NET CLI and added it to `StellarAdmin.slnx`. The Razor SDK and `AddRazorSupportForMvc` were set by hand because the CLI has no command for them.
  - 14 HTTP tests cover each row of the delivery table: fetch modes, redirect, navigation, a missing `Sec-Fetch-Mode`, detailed serialization, ASCII escaping, overflow, an oversized toast, and a Razor Pages handler and redirect.
  - Verified: `FetchResponse_DoesNotKeepHeaderToastsForNextResponse` failed when draining was changed to leave toasts in TempData, and passed again after the change was reverted. This confirms the filter runs before TempData is saved.
  - Verified: `dotnet test --solution StellarAdmin.slnx --configuration Release --minimum-expected-tests 1` passed with 556 tests and 0 failures.
