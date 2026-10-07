# Custom sidebar links

Status: completed, 2026-10-07. All four phases are implemented and committed. Follow-ups below, and an integration test for a same-path override of the Dashboard layout partials, are unscheduled.

## Goal

A developer can add their own links to the Dashboard sidebar from `AddDashboard(...)`, pointing at a host Razor Page, a host MVC action, or any URL. Each link appears in the sidebar and in the command palette, where it is found by searching. Custom links share groups and ordering with resource links, so a link to a hand-written "Invoices" page can sit in the Commerce group next to Products.

## Starting point

Most of the rendering path already exists. `ISidebarItemsProvider` is public, and the sidebar view (`Areas/StellarAdmin/Views/Shared/Components/Sidebar/Default.cshtml`) and `_CommandPalette.cshtml` both render `SidebarLinkItem` (raw `Href`) and `SidebarActionLinkItem` (controller/action/area). A developer can already add links today by writing a provider, but there is no builder method, no Razor Page link type, no new-tab option, and their groups do not merge with resource groups. Because each provider returns its own `SidebarGroupItem`s, a custom "Commerce" group renders as a second Commerce heading after the resource one.

Relevant facts:

- `StellarAdminAnchorTagHelperBase` already supports `asp-page`, `asp-page-handler`, and `asp-area`, and `sa-sidebar-menu-link` marks page and action links `data-active` through `IsActiveRoute()`. `sa-command-link-item` uses the same base.
- The command palette selects an item with `.click()` on the anchor, so `target="_blank"` works there without script changes.
- The palette already matches on item text, and passes the group label as `keywords`.
- `RenderSidebarLinkItem` writes a bare `sa-sidebar-menu-item` into `sa-sidebar-content`, outside any `sa-sidebar-menu`. Ungrouped links are not used today, so this markup has not been exercised.
- Resource links hide when the user fails the resource's authorization (`AuthorizationMetadata.AuthorizeAsync`). Host pages are outside `/stellaradmin`, so the Dashboard's `RequireAuthorization()` does not protect them.

## Proposed public API

The target is essential data, so it is a parameter; everything else is optional settings, per the [options builder conventions](../../conventions/options-builders.md). `Add*` at the root of the Dashboard builder registers the link; no call means no link.

```csharp
dashboard.AddSidebarLink("Reports", SidebarLinkTarget.Page("/Reports/Index"));

dashboard.AddSidebarLink("Invoices", SidebarLinkTarget.Action("Index", "Invoices"), link =>
{
    link.Group = "Commerce";
    link.Order = 30;
    link.RequireAuthorization("Finance");
});

dashboard.AddSidebarLink("Status page", SidebarLinkTarget.Url("https://status.voyager.travel"), link =>
{
    link.Group = "Help";
    link.OpenInNewTab = true;
});
```

- `SidebarLinkTarget` is a public sealed class with three factories: `Page(string page, string? area = null)`, `Action(string action, string controller, string? area = null)`, and `Url(string url)`. `Url` accepts absolute URLs and app-relative `~/` paths, resolved with `Url.Content`. Arguments are validated as nonblank when called.
- `AddSidebarLink(string label, SidebarLinkTarget target)` and the overload with `Action<SidebarLinkBuilder> configure` return `StellarAdminDashboardBuilder`. A blank label throws.
- `SidebarLinkBuilder` has setter-only properties `Group` (`string?`, default `null`), `Order` (`int`, default `0`), and `OpenInNewTab` (`bool`, default `false`), plus the same `RequireAuthorization` overload family as `StellarAdminDashboardBuilder`. A blank, non-null `Group` throws. `RequireAuthorization` hides the link from users who fail it; it does not protect the target, which stays the host's responsibility, and the XML docs say so.
- `Group = null` makes a top-level link with no heading, for things like "Overview" or "Reports".

No route values, icons, or badges in this slice; see follow-ups.

## Ordering and grouping

Merging happens once, after every `ISidebarItemsProvider` has returned, so any provider (built-in, a future package, or the developer's own) shares groups with the others. Providers stay independent: resources keep `ResourceSidebarItemsProvider`, and custom links get their own internal provider.

An internal `SidebarItemsMerger`, called from `SidebarViewComponent`, merges the providers' output:

- Groups with the same label (ordinal match) merge into one. A merged group sits where its label first appears, in provider order.
- Items within a merged group sort by a new `Order` on each item. Ties keep provider order, then each item's position within its provider (a stable sort).
- Ungrouped links stay where they appear. Consecutive ungrouped links sort by `Order` the same way.

The command palette receives the merged list through the view component's model.

Consequences: with equal `Order`, a resource link and a custom link in the same group sort by which provider was registered first, not by the order of the `AddResource`/`AddSidebarLink` calls; setting `Order` gives full control. An existing custom provider that returns a group with the same label as another provider's group now merges into it instead of rendering a second heading. Resource-only sidebars render exactly as before.

## Implementation phases

Each phase ends with a stop for review.

### Phase 1: registration and provider

1. Add `SidebarLinkTarget`, `SidebarLinkBuilder`, and the two `AddSidebarLink` overloads on `StellarAdminDashboardBuilder`. Store an internal `SidebarLinkOptions` (label, target, group, order, new-tab flag, authorization metadata), shaped like `ResourceSidebarItemOptions` with validation in its setters, in an internal list on `StellarAdminDashboardOptions`, the same way `AuthorizationMetadata` is held.
2. Add a public `SidebarPageLinkItem(string Label, string Page, string? Area) : SidebarLinkItemBase`, and `Order { get; init; }` (default `0`) and `OpenInNewTab { get; init; }` properties on `SidebarLinkItemBase`. All are additive; existing record constructors and the `ISidebarItemsProvider` contract do not change.
3. Add an internal `SidebarLinkItemsProvider` that filters links by authorization, groups them, and maps each target to `SidebarPageLinkItem`, `SidebarActionLinkItem`, or `SidebarLinkItem`. `AddSidebarLink` registers it once with `TryAddEnumerable`. `ResourceSidebarItemsProvider` stays as it is, except that it sets `Order` on its items.
4. Add the internal `SidebarItemsMerger` that merges all providers' output by the rules above, and have `SidebarViewComponent` pass its result to the view.
5. Unit tests in `tests/StellarAdmin.Dashboard.Tests` for rejected configuration: blank labels and groups, and blank target arguments. Following the Dashboard test strategy, merging, ordering, and authorization are covered by HTTP integration tests in `tests/StellarAdmin.Dashboard.IntegrationTests/Sidebar/SidebarLinkTests.cs`: a custom provider's group merging into a resource group, group position by first appearance, custom links sorting with resources by `Order`, authorization hiding, and ungrouped links sorting by `Order` (checked in the command palette, since the sidebar's ungrouped markup is fixed in phase 2).

Phase 1 implementation, 2026-10-07: as above. Action and page targets with no area store an empty area, because Dashboard pages render in the StellarAdmin area and link generation would otherwise keep it as an ambient value. Until phase 2, `SidebarPageLinkItem` does not render, and `OpenInNewTab` has no effect.

Phase 1 verification, 2026-10-07: the Dashboard project and the three Dashboard test projects built; the only new warning (a nullability mismatch in a new assertion) was fixed. Through the direct TUnit executables, the Dashboard unit suite passed 53 tests, the Dashboard HTTP integration suite passed 324, and the EF Core HTTP integration suite passed 67; `--list-tests` confirmed all 22 new cases were discovered. CSharpier formatted the touched files. No browser check or full solution run was performed.

### Phase 2: rendering

1. Sidebar view: add the `SidebarPageLinkItem` case (`asp-page`/`asp-area`), wrap each run of top-level links in an unlabeled `sa-sidebar-group` with an `sa-sidebar-menu`, and render `target="_blank" rel="noopener noreferrer"` when `OpenInNewTab` is set. Resolve `~/` hrefs with `Url.Content`.
2. Command palette: add the same page case and new-tab attributes. Change the copy that assumes everything is a resource: title "Search resources" becomes "Search", the description becomes "Search for a page to open.", and the empty text becomes "No results found."
3. Fix `IsActiveRoute()` in `StellarAdminAnchorTagHelperBase` to treat a missing route area and an empty `asp-area` as equal, or host links stored with an empty area are never marked active.
4. HTTP integration tests in `tests/StellarAdmin.Dashboard.IntegrationTests` (next to `Resources/ResourceSidebarTests.cs`, in a new `Sidebar` folder): rendered hrefs for page, action, absolute, and `~/` links in both the sidebar and the palette; group merging with a resource; new-tab attributes; authorization hiding; and `data-active` when a link is rendered on the page it points to.
5. A headless Chromium check that searching the palette finds a custom link and that Enter navigates to it (and opens a new tab for an external link).

Phase 2 implementation, 2026-10-07: as above, except that `data-active` is tested in `tests/StellarAdmin.TagHelpers.Tests/TagHelpers/Sidebar/SidebarMenuLinkTagHelperTests.cs` rather than over HTTP. The sidebar only renders on Dashboard pages, so a host link is never on the page it points to while the sidebar is visible; the fix matters once host pages can use the Dashboard shell (open decision 1). The Dashboard integration test host gained a `configureApp` hook, an `InvoicesController`, and a `Pages/Reports.cshtml` page so tests can map host routes. In the palette, ungrouped links still render first, in one headingless group, wherever they sit in the sidebar.

Phase 2 verification, 2026-10-07: through the direct TUnit executables, the TagHelpers unit suite passed 230 tests (the new active-area case failed with the fix reverted), TagHelpers integration 19, Dashboard unit 53, Dashboard HTTP integration 327 (3 new: rendered hrefs for page, action, absolute and `~/` targets in the sidebar and palette; new-tab attributes; ungrouped links in a headingless sidebar menu), and EF Core HTTP integration 67. Headless Chromium over CDP against DashboardPlayground on port 5207, with two links added to `Program.cs` for the check and reverted afterwards: Ctrl+K opened the palette, "privacy" found only the action link and Enter navigated to `/Home/Privacy`, "status" found the external link and Enter opened it in a new tab while the Dashboard tab stayed put, and the sidebar showed the ungrouped links in an unlabeled group. CSharpier formatted the touched C# files. No full solution run was performed.

### Phase 3: sample and guidance

1. DashboardPlayground: add a Razor Page (`Pages/Reports.cshtml`), point a link at the existing `HomeController.Privacy` action, and add an external link opened in a new tab, with one of them placed in the Commerce group.
2. Update the consumer reference in `skills/stellar-admin-dashboard/references/setup.md`, next to the existing `ISidebarItemsProvider` note, and the Dashboard section of `docs/development.md`. Check whether the website documents the Dashboard sidebar and, if it does, raise that as a separate website task.
3. Record the checks actually run here.

Phase 3 implementation, 2026-10-07: DashboardPlayground adds `Pages/Reports.cshtml` (with a `Pages/_ViewStart.cshtml` using the host layout) as "Sales report" in the Commerce group at `Order = 30`, "Privacy policy" to `HomeController.Privacy`, and "ASP.NET Core docs" to learn.microsoft.com opened in a new tab. `skills/stellar-admin-dashboard/references/setup.md` gained a "Custom sidebar links" section, and the Dashboard section of `docs/development.md` describes the provider, the merger, and the sample. The website does not document the Dashboard sidebar, so no website task is needed.

Phase 3 verification, 2026-10-07: DashboardPlayground built and ran on port 5207; the sidebar showed "Sales report" (`/Reports`) after the Commerce resources, and "Privacy policy" (`/Home/Privacy`) and "ASP.NET Core docs" (`target="_blank"`) at the top level after the groups; `/Reports` returned 200 with the host layout. `util/SkillsGenerator -- --check` reported no drift. The website checkout was searched read-only and was clean. No tests were rerun, since phase 3 changed only the sample and documentation.

### Phase 4: host pages in the Dashboard layout

Added 2026-10-07 after decision 1. A host page opts into the Dashboard shell by setting its layout; it keeps its own URL and authorization, and needs no new registration API beyond `AddSidebarLink`.

1. The layout, the sidebar view component, and `_RemoteSheet` reference their views and partials (`Components/Sidebar/Default`, `_CommandPalette`, `_RemoteSheet`, `_SheetLoading`) by full path, since short names only resolve inside the StellarAdmin area.
2. Public `StellarAdminLayouts.Dashboard` holds the layout path.
3. The layout's `<title>` uses `ViewData["Title"]` as `{title} - StellarAdmin`, falling back to `StellarAdmin`. This also applies to resource pages, which already set it. The header text stays "StellarAdmin" on every page.
4. DashboardPlayground's `Pages/_ViewStart.cshtml` uses the Dashboard layout, a new `Pages/_ViewImports.cshtml` adds the TagHelpers, and `Pages/Reports.cshtml` uses `sa-page-container` and `sa-page-header`.

Phase 4 verification, 2026-10-07: new `tests/StellarAdmin.Dashboard.IntegrationTests/Layout/HostPageLayoutTests.cs` (with `Pages/SalesReport.cshtml`) requests a host Razor Page using the layout and checks the title, body, command palette, both sheets, and the active sidebar link; before the `_SheetLoading` path fix it failed with a 500. Dashboard integration 328, Dashboard unit 53 and EF Core integration 67 tests passed. DashboardPlayground ran in Development on port 5207; Chromium screenshots of `/Reports` and `/stellaradmin/products` showed the same shell, with "Sales report" active.

## Open decisions

1. ~~Do host pages render inside the Dashboard shell?~~ Decided 2026-10-07: yes, by setting `Layout = StellarAdminLayouts.Dashboard`; host pages keep their own URLs and authorization (phase 4). Original question: A link to a host Razor Page or action leaves the shell and shows the host's own layout. If custom pages should look like part of the Dashboard (sidebar, header, theme), that needs a supported way for host views to use the Dashboard layout. This plan does not cover it; I recommend a separate plan.
2. ~~Target as a parameter or named methods~~ Decided 2026-10-07: keep the parameter. Named methods could be added later as wrappers. Original question: target as a parameter (`SidebarLinkTarget.Page(...)`, recommended) or three named methods (`AddSidebarPageLink`, `AddSidebarActionLink`, `AddSidebarUrlLink`). The parameter keeps one builder and one method; named methods are more discoverable in IntelliSense.
3. Should `Keywords` be settable on a link, so a "Reports" link is also found by searching "analytics"? It is cheap, since the palette already supports `keywords`; it is left out until requested.
4. ~~The palette copy change in phase 2 step 2 alters existing visible text.~~ Approved 2026-10-07.

## Follow-ups (not scheduled)

- Icons on sidebar links, for both resources and custom links.
- Route values on page and action targets.
- Nested (collapsible) sub-menus.
- An integration test proving an app view at the same path (for example `Areas/StellarAdmin/Views/Shared/_CommandPalette.cshtml`) overrides the Dashboard's partial on Dashboard and host pages. Full view paths drop per-controller and view-location-expander overrides of the layout partials; same-path overrides still apply.
