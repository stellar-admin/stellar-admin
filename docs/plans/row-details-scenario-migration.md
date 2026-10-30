# Row details integration tests as scenarios

Status: implemented, awaiting review (2026-10-30).

Move the row details integration tests to scenarios and specs, as the sidebar branding and authorization pilots did in [scenario-testing](archive/scenario-testing.md).

## Rule

A test that builds its own HTTP request, for a URL or a post the UI doesn't make, stays an integration test. Anything a user can trigger moves to a spec, which checks what the page shows and the requests it makes. The same rule applies to later migrations, such as the authorization tests that post directly to create and edit endpoints.

## New step

`expectRequests: { url, count, status? }` counts the requests the spec's pages have made since the spec started whose path and query match `url` (exact text or a `/pattern/`), and optionally checks the status of their responses. It lets specs check that a panel loads only when it's first opened, and only once, instead of comparing `hx-trigger` values. `signOut: true` ends the session by clearing the browser's cookies, for the authorization spec.

## Scenarios and specs

Scenarios in `tests/StellarAdmin.ScenarioTesting/Scenarios/RowDetails/`, specs in `tests/StellarAdmin.ScenarioTesting.Tests/Specs/RowDetails/`, one spec per scenario, on the Voyager Travel bookings with the host's `_BookingSummary`, `_BookingPricing` and `_BookingHistory` partials. The scenarios have their own `BookingHistoryHandler` and history service, duplicated from the CLI's docs `RowDetailsScenario`, not shared with it.

| Scenario | Configuration | Spec | Replaces |
| --- | --- | --- | --- |
| `row-details-inline` | `EnableRowDetails("_BookingSummary")` | Details start hidden; a row's toggle shows that booking's summary, and toggling again hides it. | `InlineDetails_RenderPartialCollapsedBelowEachRow` |
| `row-details-settings` | `RowClick`, `Single`, `Rail`, `FullWidth`, `Bleed`, `Animate = false`, `Toggle = None` | No toggles; clicking a row expands it; expanding a second row collapses the first; the emphasis, divider, inset and animate attributes. | `ConfiguredSettings_RenderOnRowDetailsElement` |
| `row-details-projection` | A projection into `BookingPricing` | No pricing request or content before expanding; two expanded rows show their own prices. | EF `ProjectedDetails_RenderLoaderForTheEntityKey`, `ProjectedDetails_RowDetailRendersProjection` |
| `row-details-handler` | `EnableRowDetails<BookingHistoryHandler>`; the handler returns `null` for one booking | Expanding shows the history without a second sidebar; the `null` booking's request gets a 404 and the row shows "This record no longer exists." | `HandlerDetails_RenderLoaderForTheRowKey`, `HandlerDetails_RowDetailRendersHandlerModel`, `HandlerDetails_MissingRecord_ReturnsNotFoundMessage` |
| `row-details-authorization` | The bookings resource requires a signed-in user | Signed in, load the grid; `signOut`; expand a row: its request gets a 302 to the scenario's sign-in page, which answers 200. With Identity's cookie, a refused request is redirected rather than a 401 (see Found). | `RequiredAuthorization_AnonymousRowDetailRequest_ReturnsUnauthorized` |
| `row-details-panels-accordion` | History (handler), Summary (inline), Pricing (projection, `Icon`, `Name = "prices"`) | Contained accordion, three titles, all closed; Summary's content present; History and Pricing each requested once, on first open, including after closing and reopening; both stay open; the icon on Pricing only. | `Panels_ByDefault_RenderClosedContainedAccordionForEachRow`, `Panels_InAccordion_LoadOnDemandPanelsWhenFirstOpened`, `PanelIcon_RendersOnAccordionTitle`, `Panels_RowDetailRendersNamedPanel`, EF `ProjectedPanel_RowDetailRendersProjection`, `ProjectedPanelWithConfigure_RendersConfiguredPanel` |
| `row-details-panels-accordion-single` | The same panels, `Accordion(Single)` | Opening History closes Pricing; opening a panel in another row leaves this row's panel open. | `Panels_InSingleAccordion_ShareNameWithinRow` |
| `row-details-panels-tabs` | The same panels, `Tabs()` | No accordion; the default tab style; History requested once when the row expands, Pricing once when its tab is first selected; the icon on the Pricing tab; selecting a tab in one row doesn't change another row's tab. | `Panels_AsTabs_RenderTabListInStyle` (default), `PanelIcon_RendersOnTab`, `Panels_AsTabs_RenderTabsForEachRow`, `Panels_AsTabs_LoadFirstPanelOnExpandAndOthersWhenShown` |
| `row-details-panels-tabs-line` | The same panels, `Tabs(Line)` | The line tab style. | `Panels_AsTabs_RenderTabListInStyle` (line) |
| `row-details-single-panel` | One panel | Neither an accordion nor tabs; the panel's content shows. | `SinglePanel_RendersWithoutTabs` |
| `row-details-problems` | One resource per misconfiguration: a handler type that isn't a handler, a handler for two models, an undefined tab style, and a handler on an in-memory custom data source without `UseKey()` | The configuration problems panel lists each message; the bookings History panel is left out and Summary and Pricing fall back to the accordion; trips and partners rows don't expand. Two problems share the bookings resource. | `HandlerDetailsWithoutKey_LogsProblem`, `HandlerDetailsWithoutModel_RowsDontExpandAndLogProblem`, `PanelHandlerForTwoModels_LeavesPanelOutAndLogsProblem`, `Panels_WithUndefinedLayout_UseAccordionAndLogProblem` |

## Staying as integration tests

The endpoint's answers to URLs the UI never requests:

- `DetailsNotLoadedOnDemand_RowDetailReturnsNotFound`: details inline or not enabled.
- `Panels_RowDetailWithoutOnDemandPanel_ReturnsNotFound`: no panel, an inline panel or an unknown panel.
- EF `ProjectedDetails_MissingOrFilteredEntity_ReturnsNotFound`: a missing, non-numeric or filtered-out key.

The other tests in both `ResourceRowDetailsTests.cs` files are deleted, and so is the then unused `ProductNotesAndSummaryHandler`.

## Built

- Runner: `SpecRequests` records every request in the spec's browser context; `SpecRun.Requests` holds it. `expectRequests` and `signOut` in `SpecLoader`, documented in the write-scenario-specs spec format reference.
- Scenarios in `Scenarios/RowDetails/`, with `RowDetailsBookings` (columns, the `Pricing` projection, `MissingHistoryId` = booking 2, VB-100621), `BookingHistoryHandler` and `BookingHistoryService`. The specs use VB-101459 (booking 1) and VB-100299 (booking 5, whose pricing differs).
- Each spec's first click has a 5 s timeout: with every spec running at once, the first click after a page load timed out at 1 s.
- Integration tests: both `ResourceRowDetailsTests.cs` files keep only the endpoint tests above; `ProductNotesAndSummaryHandler` is deleted.

## Found

- **Expanding a row after the session ends showed the sign-in page inside the row.** The row detail request was redirected to the sign-in page, and the row rendered it. The old integration test used a test scheme that answers 401, so it didn't show this. Fixed, see below.
- The accordion's on-demand loaders swap themselves out (`hx-swap="outerHTML"`), so removing `once` from their trigger doesn't make them load again; the tabs' loaders do, and the tabs spec fails without `once`.

## Refused htmx requests reload the page

Built (2026-10-30): `HtmxAuthorizationResultHandler` (`Infrastructure/Authorization/`) wraps the registered `IAuthorizationMiddlewareResultHandler`, which `AddStellarAdmin()` moves to a keyed service with the same lifetime. A challenged or forbidden request to an endpoint `MapStellarAdmin()` marked with `StellarAdminEndpointMetadata`, with `HX-Request` and without `HX-Boosted`, gets a 204 with `HX-Refresh: true`, so htmx reloads the page and the app's own challenge or access denied response handles it. Everything else goes to the wrapped handler. An app handler registered after `AddStellarAdmin()` replaces it; the website's authorization page says so in a warning. The bundled htmx 4.0.0 reloads on `HX-Refresh` whatever the status.

- `row-details-authorization` now checks that the page ends on `/scenario/sign-in?ReturnUrl=%2Fstellaradmin%2Fbookings` and the row detail request got a 204.
- `HtmxAuthorizationTests`: a challenged and a forbidden htmx request get the refresh; a plain and a boosted request keep the 401; an htmx request to a host endpoint keeps the 401; an app handler registered before `AddStellarAdmin()` still answers other refused requests.

## Verification

Done on 2026-10-30:

- The 11 specs pass through `scenarios run`, and `StellarAdmin.ScenarioTesting.Tests` passes 23 of 23 on three runs.
- With `once` removed from the tab trigger, `row-details-panels-tabs` fails on its Pricing request count. A wrong status and a wrong count each failed with a message naming the URL, the expected and the actual value.
- `StellarAdmin.IntegrationTests` (698) and `StellarAdmin.EntityFrameworkCore.IntegrationTests` (151) pass, 21 and 5 cases fewer.
- After the refresh handler: `StellarAdmin.Tests` (340), `StellarAdmin.IntegrationTests` (704), `StellarAdmin.EntityFrameworkCore.IntegrationTests` (151) and `StellarAdmin.ScenarioTesting.Tests` (23) pass. Without the `Decorate` call, the two refresh tests fail and the other four pass. The website wasn't built.
