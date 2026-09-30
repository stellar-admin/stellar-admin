# Toast

Status: **in progress**. Phases 1 and 2 are committed; phase 3 is implemented and awaiting review. Last updated: 2026-09-30.

Port shadcn's Toast into `StellarAdmin.TagHelpers`, with a server-side API that works for full page loads, redirects and AJAX responses. Work proceeds in phases with a review checkpoint after each; approval of one phase does not authorize the next.

## Resuming

Read this file, then check the current code under the paths in [Source paths](#source-paths) before continuing. Update the phase table and the phase log when a phase finishes or stops partway.

| Phase | Scope | Status |
| --- | --- | --- |
| 1 | Server API: `IToastNotifier`, TempData storage, result filter, `SA-Toasts` header; HTTP integration tests | ☑ |
| 2 | Visual prototype: `sa-toaster`, `sel-toaster`, structural CSS, one theme, static DocsSamples demo | ☑ |
| 3 | Client API (`window.stellarAdmin.toast`), full-page rendering from TempData, htmx demo in ComponentPlayground | ◐ implemented, awaiting review |
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
  - A modal dialog makes everything outside it inert, top layer included. While a modal is open, `sel-toaster` moves the viewport into the topmost modal dialog and re-shows it, then moves it back when the dialog closes (found in the phase 2 prototype; re-showing alone left toasts visible but unclickable).
- **Animation.** Enter and exit use `@starting-style` and the `data-starting-style`/`data-ending-style` attributes.
- **Stacking.** Stacked while collapsed, expanded on hover or focus, driven by Base UI's CSS variables.
- **Accessibility:**
  - A polite live region; `Error` toasts use assertive priority.
  - F6 moves focus to the toaster landmark.
  - Timers pause on hover, on focus and while the document is hidden.
  - Every toast has a close button.
- Swipe-to-dismiss is always on. Each position swipes towards its own edges (decided in phase 2).

## Open decisions

To settle in the phase that needs them:

1. **Naming.** Settled at the phase 1 review: `IToastNotifier` (renamed from `IToasts`), the `SA-Toasts` header and `ToastType`. Settled in phase 2: `ToasterPosition` with six members (`TopLeft`, `TopCenter`, `TopRight`, `BottomLeft`, `BottomCenter`, `BottomRight`), defaulting to `BottomRight`.
2. **Header cap.** Settled in phase 1: a 4096-byte budget rather than a toast count, always sending at least one toast.
3. **Registration.** Phase 1 made it always on through `AddTagHelpers()`, which also calls `AddHttpContextAccessor()`. The filter loads TempData only for non-navigation, non-redirect results; confirm at the phase 1 review.
4. **Swipe-to-dismiss.** Settled at the phase 2 prototype review: keep it. It is about 60 lines: each position swipes towards its own edges, a 40 px threshold as in Base UI, and damped movement in other directions.
5. **Theme stubs.** Settled in phase 2: the shipped `.sa-toast` rules match upstream's current `.cn-toast` (`rounded-2xl` in nova and vega, `rounded-none` in lyra), which is the whole themed half. Everything else in upstream's toast is structural, so ThemeGenerator does not need re-running.
6. **Surface in custom themes.** (Phase 4) Upstream puts `bg-popover text-popover-foreground border shadow-lg` in the component's own classes, so phase 2 placed them in the structural `.sa-toast` rule, as Tooltip does with its colors. Structural declarations beat theme rules, so a custom theme can change the colors through its tokens but cannot give the toast its own border or shadow. Decide in phase 4 whether to move border and shadow into the theme files.

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
- `tests/StellarAdmin.TagHelpers.IntegrationTests/`: HTTP delivery tests (`Toasts/ToastDeliveryTests.cs`) and page rendering tests (`Toasts/ToasterRenderingTests.cs` with `Pages/Toaster.cshtml`).
- `tests/StellarAdmin.TagHelpers.Tests/Toasts/TempDataToastNotifierTests.cs`: validation unit tests.
- `src/StellarAdmin.TagHelpers/TagHelpers/Toaster/`: `ToasterTagHelper` (`sa-toaster`) and `ToasterPosition`.
- `src/StellarAdmin.Core/Icons/SemanticIconRole.cs` and the three icon packs: the `Success`, `Info`, `Warning` and `Error` roles.
- `src/StellarAdmin.TagHelpers/Client/js/web-components/sel-toaster.ts`: the client component, registered in `Client/js/stellar-admin-ui.ts`. The client API is `Client/js/wrappers/toast.ts`, exposed as `window.stellarAdmin.toast`.
- `src/StellarAdmin.TagHelpers/Client/css/components.css` (the `.sa-toast*` rules) and `Client/css/themes/`: styles.
- `util/theme-coverage/coverage.json`: the `Toaster` entry.
- `docs/DocsSamples/Pages/Toast/`: the demo page, listed under Overlays in `Pages/Shared/DemoNavigation.cs`.
- `sandbox/ComponentPlayground/Pages/Demo/Toast.cshtml`: the server delivery demo (htmx 4, redirects, `fetch`), with `<sa-toaster />` in the playground layout.

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
- 2026-09-30: Phase 2 prototype built, not committed. Extraction has not started.
  - `sandbox/html/toast.html`: upstream's toast classes verbatim against the real bundles, a template the JS clones, and a throwaway `proto-toaster` element that reproduces Base UI's data attributes and CSS variables. It has static strips (types, action and long content, collapsed and expanded stacks, top anchoring), a live toaster with demo buttons, and a control bar (six positions, swipe, nova/vega/lyra/maia, dark).
  - Structural additions beyond upstream: top positions (mirrored stack), a popover UA-style reset on the viewport, opacity-only transitions under reduced motion, a hidden assertive announcer for `Error` toasts, and a `ResizeObserver` that remeasures heights when fonts, themes or widths change.
  - Verified in headless Chromium 151 over CDP: the limit of 3 (older toasts marked `data-limited` and inert), hover expanding the stack and pausing the timers (a 5 s toast survived 5.7 s of hover and closed after it ended), F6 focusing the viewport and Tab reaching the toast, Escape closing the focused toast with focus restored, swipe right closing and a short or disallowed swipe snapping back, toasts over a modal dialog receiving clicks with the dialog left open, the viewport returning after the dialog closed, reduced motion removing toasts without transform transitions, 390 px mobile width, top-center stacking, dark mode, and lyra and vega.
- 2026-09-30: Phase 2 prototype reviewed and committed (05b5b93). Approved: keep swipe, the six `ToasterPosition` values with `BottomRight` as the default, and the visuals.
- 2026-09-30: Phase 2 extracted, not committed.
  - `sa-toaster` renders `<sel-toaster>` holding the toast `<template>` and the viewport. It has `position`, `limit` (at least 1) and `duration` (a `TimeSpan`, not negative) and emits `limit` and `duration` (in milliseconds) attributes. Classes set on `sa-toaster` go on the viewport, because `sel-toaster` itself has no box.
  - The template takes its type icons from four new semantic icon roles (`Success`, `Info`, `Warning`, `Error`, appended to the enum) plus the existing `Loading` and `Close`. Lucide maps them to upstream's `circle-check`, `info`, `triangle-alert` and `octagon-x`; both Tabler packs use `circle-check`, `info-circle`, `alert-triangle` and `alert-octagon`. The action link and close button use `ButtonRenderingHelper` (outline small, ghost icon-small).
  - `sel-toaster` is the prototype's element ported to a Lit component, without the prototype's static mode and swipe toggle. It reads a `<script type="application/json">` child for seeded toasts, which phase 3 fills from TempData. Global listeners are removed on disconnect, and the viewport returns from a dialog.
  - Deviation: the element methods are `addToast`, `updateToast` and `closeToast`, because `update` would override LitElement's lifecycle method. `updateToast` merges into the current options, as Base UI's `update` does. Phase 3's `window.stellarAdmin.toast` wraps them.
  - Structural CSS: `.sa-toast-viewport`, `.sa-toast` (including the top-position mirror and reduced motion) and the part rules, written as plain CSS and `@apply` in `components.css`. The text column has a new `toast-text` slot, which the `ResizeObserver` watches.
  - The CSS build runs the theme coverage gate, so `coverage.json` has a `Toaster` entry: the eight shadcn themes are `reviewed` against their `.sa-toast` rule, and the seven custom themes are `shared-only` with a rationale that names phase 4. This is provisional until phase 4.
  - DocsSamples: `Pages/Toast/Index.cshtml` with an `_Intro` partial whose buttons raise every type, an action and a loading toast that updates to success. For this phase the partial contains its own `<sa-toaster>`; where the toaster lives in the docs layout is left to phase 5.
  - Verified in headless Chromium 151 over CDP against DocsSamples on port 5206: the limit of 3 (two older toasts `data-limited`, inert, opacity 0), hover expanding the stack and pausing a 1.5 s timer until the pointer left, F6, Tab to the newest toast, Escape closing it with focus moving to the next toast and then back to the trigger, a short swipe snapping back and a right swipe closing, the assertive announcer text, loading updated to success (icon, type and description), a toast over a modal dialog closed by a real click with the dialog still open, the viewport returning afterwards, a seeded JSON toast rendering, cleanup on disconnect, top-center anchoring, reduced motion (`transition-property: opacity`), and 390 px width. Screenshots of nova (dark), lyra (0 px radius) and vega at 390 px match the prototype.
  - Verified: `dotnet test --solution StellarAdmin.slnx --configuration Release --minimum-expected-tests 1` passed with 560 tests and 0 failures. The existing per-role test covers the four new roles. `npm run build`, `node util/theme-coverage/check.mjs`, `npm run test:themes` and `oxfmt --check` also passed.
- 2026-09-30: Phase 2 extraction reviewed and committed (d99c9c4).
- 2026-09-30: Phase 3 implemented, not committed.
  - `sa-toaster` moves every toast queued in TempData into a `<script type="application/json">` child of `<sel-toaster>` (`ToastQueue.DrainAll`, with no byte budget). This covers toasts added during a full page load and toasts carried over a redirect. The serializer's default encoder escapes `<`, `>` and `&`, so the JSON cannot close the script element. AJAX responses have already drained into the header before the page renders, so the tag helper drains unconditionally.
  - `window.stellarAdmin.toast` (`Client/js/wrappers/toast.ts`): `add`, `success`/`info`/`warning`/`error` (a title plus a description string or the remaining options, mirroring the server extensions), `update`, `close` (resolves after the exit animation), `promise` (a loading toast that becomes success or error; it returns the original promise, and the settled toast goes back to the toaster's default duration), and `fromResponse`. `fromResponse` takes a `Response`, `Headers`, an `XMLHttpRequest`, the header string, or null; it returns the new ids and ignores a missing or invalid header with a console warning for invalid JSON. Every method throws if the page has no `<sa-toaster>`.
  - ComponentPlayground: `<sa-toaster />` in `_Layout.cshtml`, and `Demo/Toast` (listed under Feedback) with an htmx 4 form post that returns a fragment plus a success or error toast, a plain form post that redirects (TempData render), an htmx post that redirects to a fragment (fetch follows the redirect, so the toast arrives in the redirected response's header), `fetch` calls returning an info toast with an Undo link and three toasts in one header, and client-only `success` and `promise` buttons. The page model is `ToastPage` because `Toast` would shadow the library type. The playground's Tailwind build added `.items-end` to `wwwroot/css/site.css`.
  - The htmx 4 snippet (`htmx:after:request`, `e.detail.ctx.response.headers`) was confirmed against the playground's htmx 4 build, which fires the event for every response before its error handling.
  - Verified: five new HTTP tests in `ToasterRenderingTests` (a queued toast rendered into the page, not repeated on the next response, a Razor Pages redirect rendered on the target page, no script without toasts, HTML escaped inside the script).
  - Verified in headless Chromium 151 over CDP against ComponentPlayground on port 5206, 22 checks: the htmx post's success and error toasts plus the swapped fragment, the assertive announcer, the htmx redirect's toast and fragment, three `fetch` toasts newest first, the Undo link's href and its navigation rendering the restored toast from TempData, the seed script in the markup, a reload not repeating the toast, the plain form's redirect toast, client `success` with a description, `promise` from loading to success (auto-closing after the default 5 s) and to error, `fromResponse` with a string, `Headers`, `Response`, null, a missing header, invalid JSON and an `XMLHttpRequest`, and `add`/`update`/`close`.
  - Verified: `dotnet test` passed with 565 tests and 0 failures. `npm run build`, `check:themes` and `test:themes` passed. `oxfmt --check` passes for the changed TypeScript; it reports 17 files (the CSS files and `sel-message-scroller.ts`) that are unchanged and fail the same way on the committed tree.
