# Widgets

Status: phase 1 implemented and committed (2026-10-30); phase 2 implemented (2026-10-30), uncommitted, on the `widgets` branches of the product and the website. Phases 3 and 4 not started.

Let developers place widgets on the admin panel's pages: the home page, a resource's index page, its view page and its create and edit forms. A widget is a self-contained block such as a stat, a chart, an activity heatmap, a list of recent records, a callout or a chat box. Examples are a chart of recent sign-ups above the customers grid, or a login activity graph (like GitHub's contribution graph) at the top of a customer's view page.

This plan merges the earlier home page panels and resource page panels plans (both 2026-10-26), since they describe the same thing: blocks placed into slots, with the home page as one more page that has slots. The name follows [Filament's widgets](https://filamentphp.com/docs/5.x/widgets/overview), which Filament mostly confines to dashboards. Here they can go on any page. "Widget" also avoids a third meaning of "panel" next to the admin panel and row detail panels.

## Today

- The home page displays the setup checklist in Development and links to the resources otherwise. `ConfigureHomePage()` has one setting, `ShowSetupChecklist`. The only way to change the page is to override `Areas/StellarAdmin/Views/Home/Index.cshtml`, whose model is internal.
- Row details show a partial per expanded row, optionally with a model from an `IResourceRowDetailHandler<TModel>` (`FindAsync(string id, CancellationToken)`) or an EF Core projection. The handler gets only the row key; everything else comes from constructor injection. Widgets reuse this shape but need more input than a key (see [Parameters](#parameters-and-contexts)).

## Concepts

Prototypes in `sandbox/html/widgets-*.html` (2026-10-30), built on the AdminPlayground page shell and the real stylesheet, with a theme, mode, density and placement switcher. The candidate CSS is `sandbox/html/widgets/widgets.css`; the charts are drawn by `sandbox/html/widgets/widgets.js` with inline SVG.

- `widgets-home.html`: a dashboard grid; the setup checklist as a widget; grouped sections; the current medium width.
- `widgets-index.html`: stat cards, a chart beside stats, divided stats in one card, collapsible insights, a side column, numbers in the header, widgets below the grid.
- `widgets-view.html`: a login heatmap (12 and 6 months), a sidebar, related records and a chat below the details, record stats, widgets as a tab.
- `widgets-edit.html`: a sticky context sidebar, a callout above the form, a widget between sections, history after the form, and the create page.
- `widgets-gallery.html`: every widget kind, plus loading, empty, error, polling and stale states.

Screenshots were checked headless at 1440px (every page, one placement each; dark Ops and Soft on two) and at 390px (index). Firefox, Safari and the other placements in dark mode were not checked.

## Decisions

- **Handler and partial, like row details** (Jerrie 2026-10-30). A widget is a partial, optionally with a handler that loads its model. Developers can build anything; predefined widgets come later and are built on the same mechanism.
- **Parameters are typed** (Jerrie 2026-10-30). A parameters delegate on the widget builder receives a context that depends on the page and returns a parameters object of the developer's own type, which the handler receives.
- **The builder's parameters are signed** (Jerrie 2026-10-30) with ASP.NET Core Data Protection, so a handler can trust them even though they travel in the lazy-load URL.
- **Handler widgets load lazily; partial-only widgets render inline** (Jerrie 2026-10-30). `Inline` opts a handler widget into rendering with the page. The handler's contract is the same either way. Polling works for inline widgets too: the first render is inline, refreshes use the lazy endpoint.
- **Widget inputs bind with MVC model binding** (Jerrie 2026-10-30). Properties marked `[WidgetInput]` may take values from the widget's refresh request; the rest come only from the signed payload.
- **Widgets render their own surface** (Jerrie 2026-10-30). There is no frame: the partial renders its card (or several cards, or none), including any header actions such as a period picker or a menu. The admin panel renders only a placeholder while a lazy widget loads and an error state when it fails.
- **The setup checklist stays as it is** (Jerrie 2026-10-30). It is not a widget for now, and `ShowSetupChecklist` stays. In Development the checklist shows above the home page's widgets. Making it a seeded widget is a possible later step.
- **Create and edit have separate widgets**, since they are already separate builders (`AllowCreate`, `AllowEdit`) and create has no record.
- **Not now:** widgets between form sections, widgets as view page tabs, user-customisable dashboards (the prototype's "Customize" button), and page-level filters (a dashboard period picker driving every widget). Each is a possible later phase.

## API sketch

Proposed public API, following the [options builder conventions](../conventions/options-builders.md). Names are provisional until phase 1 is implemented.

### Registration

```csharp
admin.ConfigureHomePage(home => home.Widgets(widgets =>
{
    widgets.Add<RevenueHandler>("Revenue", "_RevenueWidget", widget =>
    {
        widget.Span = 8;
        widget.RefreshInterval = TimeSpan.FromMinutes(5);
    });
    widgets.Add("Quick actions", "_QuickActions");                      // partial only, inline
}));

admin.AddEfCoreResource<Customer>(resource =>
{
    resource.Index(index => index.Widgets(widgets =>
        widgets.Add<SignupsHandler>("Sign-ups", "_SignupsWidget", widget =>
        {
            widget.Span = 8;
            widget.Parameters(context => new SignupsParameters(context.Search, context.Scope, Days: 30));
        })));

    resource.AllowView(view => view.Widgets(widgets =>
        widgets.Add<LoginActivityHandler>("Login activity", "_LoginActivityWidget", widget =>
        {
            widget.Parameters(context => new LoginActivityParameters(context.Record.Id, Months: 12));
            widget.ShowWhen(context => context.Record.LastLoginAt is not null);
        })));

    resource.AllowEdit(edit => edit.Widgets(widgets =>
        widgets.Add<UpcomingTripHandler>("Upcoming trip", "_UpcomingTripWidget", widget =>
        {
            widget.Placement = WidgetPlacement.Aside;
            widget.Inline = true;
            widget.Parameters(context => new UpcomingTripParameters(context.Record.Id));
        })));
});
```

- `Widgets(...)` is a bare noun on `HomePageBuilder`, `ResourceIndexBuilder<TResource>`, `ResourceViewBuilder<TModel>`, `ResourceCreateBuilder<TModel>` and `ResourceEditBuilder<TModel>`. The collection always exists (empty), so it drills into structure rather than gating a feature.
- `widgets.Add(name, partialName)` and `widgets.Add<THandler>(name, partialName)`, each with a `configure` overload, mirror `panels.Add` on row details. The name identifies the widget: it must be unique on the page, appears in the lazy-load URL and in configuration problems, and is not displayed (the partial renders its own heading). The no-callback overloads return the widget builder; the callback overloads return the widgets builder.
- Widget builder properties: `Span` (1 to 12, default 12; the grid responds to its container, so spans collapse in narrow slots), `Placement` (`Top`, `Aside` or `Bottom`; default `Top`; the home page has one grid and ignores it), `Inline`, `RefreshInterval`.
- Widget builder methods: `Parameters(Func<TContext, TParameters>)`, `ShowWhen(Func<TContext, bool>)`, `RequireAuthorization()` and `RequireAuthorization(policy)` like resources.
- `TContext` is the page's context type (below), so `context.Record` is typed by the page's model. `TParameters` is inferred from the delegate's return type. Because `Add<THandler>` cannot infer the handler's parameter type at compile time, a mismatch between the delegate's type and the handler's interface is a configuration problem (like `UnusableRowDetailHandler`), and the widget is left out.

### Handlers

```csharp
public interface IWidgetHandler<TModel>
{
    Task<TModel?> LoadAsync(WidgetLoadContext context, CancellationToken cancellationToken);
}

public interface IWidgetHandler<TParameters, TModel>
{
    Task<TModel?> LoadAsync(TParameters parameters, WidgetLoadContext context, CancellationToken cancellationToken);
}

public sealed record LoginActivityParameters(int CustomerId, [property: WidgetInput] int Months);
```

- The model type comes from the interface, as with row details. A handler registered for none or several models is a configuration problem.
- The handler is registered as a scoped service unless already registered.
- `WidgetLoadContext` carries the `HttpContext`, the `User` and the widget's name. It does not carry the page's record: the handler runs in its own request when lazy, and the same contract applies inline.
- Returning `null` hides the widget. That lets a handler decide there is nothing to show.

### Parameters and contexts

The parameters delegate runs when the page renders, where the context is rich (the record is already loaded on view and edit). It returns the developer's parameters type, which is serialized, signed and put in the widget's lazy-load URL, then bound back and passed to the handler.

| Page | Context | Adds |
| --- | --- | --- |
| Every page | `WidgetContext` | `HttpContext`, `User`, the current URL, which page this is, services |
| Resource pages | `ResourceWidgetContext<TResource>` | the resource's metadata (name, labels, routes) |
| Index | `IndexWidgetContext<TResource>` | the grid's state: search, scope, sort, page |
| View, edit | `RecordWidgetContext<TModel>` | `Key` and `Record`, the page's loaded model |
| Create | `CreateWidgetContext<TModel>` | nothing beyond the resource |

A partial-only widget has no handler, so its partial gets the context as its model; a partial-only widget on the view page can show the record without a handler.

Widgets can have their own inputs, such as a 30d/90d picker. The partial renders them in an htmx form that requests the widget's URL, which the admin panel provides with the signed payload already in it (through the context or a tag helper; settle in phase 1). The endpoint binds `TParameters` with standard MVC model binding through two value providers: one over the unprotected payload, which supplies every property, and one over the query string, restricted to the properties marked `[WidgetInput]`. Binding then gives type conversion, culture handling, positional records through their constructor and validation into `ModelState`. Inline widgets make no widget request: their handler gets the delegate's parameters directly, and a changed input always goes through the lazy endpoint. Check early in phase 1 that binding through these value providers works for positional and init-only record properties.

### Rendering

- The partial renders the whole widget, card included. A widget may render one card, several (variant A on the index prototype could be one full-width widget with four stat cards, or four widgets), several stats in one divided card (variant C is one widget), or no card at all (a callout).
- Lazy widgets render a placeholder, a skeleton sized to the widget's span, and `hx-get` the widget endpoint on load (plus `every …` with `RefreshInterval`). A failed load replaces the placeholder with an error state and a retry button. A failed refresh keeps the last content; whether and how it shows that the content is stale is a phase 1 detail (the gallery prototypes a footer marker, which now belongs to the widget's own markup).
- The widget endpoint is per page: the home controller for home widgets and the resource controller for resource widgets (for example `GET {resource}/widget/{name}?p={protected parameters}`). It checks the widget's authorization again, unprotects and binds the parameters, applies `[WidgetInput]` values, runs the handler and renders the partial.
- Inline widgets run their handlers during the page request one after another, not in parallel, since they share the request's scoped services (a `DbContext` is not thread-safe).
- The widget grid (`sa-widgets`, a 12-column grid with container queries) and the aside layout are admin panel CSS.

## Phases

1. **Core and the home page.** Handler interfaces, contexts, the widget builders, signed parameters and `[WidgetInput]` binding, lazy and inline rendering, refresh, `ShowWhen`, authorization, configuration problems, the loading and error placeholders, the grid CSS, and `home.Widgets(...)`. Tests mirror the source projects (TUnit). An AdminPlayground home dashboard, the website docs page, and the consumer skills reference.
2. **Index pages.** `index.Widgets(...)` with `IndexWidgetContext` and the `Top`, `Aside` and `Bottom` placements. Decide whether widgets should refresh when the grid's search or scope changes.
3. **View, create and edit pages.** `RecordWidgetContext` and `CreateWidgetContext`, the placements on each page, and the sticky aside on forms.
4. **Predefined widgets.** Stats first (value, change, note, sparkline or goal, several stats in one card), built on the same handler mechanism. A chart widget waits for a chart component; a records list could follow [configured-row-details](configured-row-details.md).

## Phase 1 as built

Choices made while implementing. Jerrie confirmed (2026-10-30) the resource links, deferring `Placement`, documenting empty strings and having no stale marker:

- **Widgets replace the home page's resource links** when the user sees at least one widget; a user who sees none gets the links. In Development the checklist shows above the widgets, which sit in a full-width page container below the checklist's medium one.
- **Inputs go through `<sa-widget-form>`** (a tag helper in the `StellarAdmin` assembly), which renders a form with `hx-get` to the widget's URL, triggered on change and submit. The widget's URL reaches it through `ViewData`. No context property.
- **The widget's response replaces its whole cell**, which carries the new URL: after an input changes, the payload is signed again with the bound values, so a refresh keeps the picker's value.
- **`Placement` is left for phase 2**, since the home page ignores it and nothing could test it.
- **`WidgetContext` has `HttpContext`, `User` and `Services`** only; the current URL and the page kind are not added (the URL is on `HttpContext.Request`, the page is implied by the context type).
- **A widget without a handler ignores `Parameters` and `RefreshInterval`**, reported as `IgnoredWidgetSetting`; it never refreshes, since on record pages its context can't be rebuilt.
- **Responses:** an unknown widget or one the user may not see returns 404; parameters that are missing, changed, from another widget or fail binding return 400 with the error cell; a throwing handler is logged and returns 500 with the error cell (an inline one renders the error cell in the page); a handler returning `null` returns `HX-Reswap: delete`, which removes the cell.
- **An empty string binds as `null`,** so a non-nullable `string` parameter that can be empty fails MVC's implicit `[Required]` and the widget shows its error. Documented (declare it `string?`) rather than changed.
- **A failed refresh keeps the content without a stale marker.** Loaded cells that refresh carry htmx 4's `hx-status:4xx`/`hx-status:5xx="swap:none"`; no client script was needed.
- **Payload:** System.Text.Json of the handler's `TParameters`, flattened into keys for a value provider (invariant culture), protected with purposes `StellarAdmin.Widgets`, the page (`home`) and the widget's name. `AddStellarAdmin` now calls `AddDataProtection()`.
- **Configuration problems** 1100 to 1105: `BlankWidgetName`, `RepeatedWidgetName` (names compare ignoring case), `BlankWidgetPartial`, `UnusableWidgetHandler`, `MismatchedWidgetParameters`, `IgnoredWidgetSetting`; a bad `Span` or a `RefreshInterval` under one second is `InvalidValue`. The home page's widgets are checked once, when problems are listed or the page first renders, and appear under "Home page".
- **Grid CSS** is in the panel's `client.css`: 12 columns, a widget spans the row below a 34rem container, spans up to 6 take half from 34rem, and own spans from 56rem. Rows stretch so cards line up.

Not done in phase 1: the website screenshot for the new page (an open `<Screenshot>` placeholder, `home-page-widgets`), and a scenario spec for it.

## Phase 2 as built

- **`index.Widgets(...)` is on `ResourceIndexBuilderBase`**, so EF Core resources get it too. Its widgets are checked with the resource's other checks and reported under the resource.
- **`IndexWidgetContext<TResource>` derives from `WidgetContext`** and adds only `Search` (trimmed, or `null`) and `Scope` (the selected scope's id, or `null` without scopes). There's no `ResourceWidgetContext` and no sort or page: nothing needed the resource's metadata yet, and widgets don't render again when the sort or page changes, so those values would go stale.
- **Widgets render again when the search or scope changes; sorting, paging and inline saves leave them alone** (decided here; the open question). A scope tab already reloads the page. The search box's `hx-select-oob` adds the widget regions, and the index action renders widgets unless the request targets the grid (`HX-Target`) from something other than the search box (`HX-Source`). Lazy widgets come back as placeholders and load again.
- **`Placement`** (`WidgetPlacement.Top`, `Aside`, `Bottom`; default `Top`) is on the shared `WidgetBuilder`. On the home page anything but `Top` is `IgnoredWidgetSetting`, and the widget goes in the grid.
- **Regions:** `#index-page-widgets-top`, `-aside` and `-bottom` render when a widget is configured for that placement, and are `hidden` when the user sees none of them, so a search can fill them. With aside widgets, the grid and a 20rem column sit in `.sa-widgets-aside`, which becomes two columns from a 56rem container (`:has()` drops the column when it's hidden); narrower, the column goes below the grid. The grid's markup moved to `_IndexPageGrid.cshtml`, and the search input got the id `index-page-search-input`.
- **The widget list reaches `_IndexPage` through `ViewData`** (`ViewDataKeys.Widgets`), not `IResourceIndexPageViewModel`, so the public view model is unchanged and overridden index views that use `<sa-index-page>` get widgets.
- **Endpoint:** `GET {slug}/widget/{name}`, a `Widget` action on the resource controller; the payload's page purpose is `{controller}/index`, so a home payload is refused there.
- AdminPlayground: the Products index has a **Price summary** (top, follows scope and search) and **Top categories** (aside, with its picker); `_TopCategoriesWidget` moved to `Areas/StellarAdmin/Views/Shared/`.

Not done in phase 2: the website screenshot (`index-page-widgets` placeholder) and its scenario spec.

## Open questions

- Whether index widgets should load again after an inline save or a delete changes the records. They don't now.
- Where top widgets sit on the index page, and whether they should follow the search and scope at all (Jerrie 2026-10-30: unsure about widgets between the search and scopes and the grid; kept as built until everything is in place). `IndexWidgetContext` stays `Search` and `Scope` until this is settled.

## Verification

Phase 1 (2026-10-30):

- `tests/StellarAdmin.IntegrationTests/Home/HomePageWidgetTests.cs`, 17 tests: widgets replace links; links when no widget is visible; checklist above widgets; lazy placeholder then load; inline; refresh trigger; parameters reach the handler; `[WidgetInput]` changes only marked properties and survives a refresh; invalid input 400; empty and tampered payloads 400; another widget's payload 400; `null` hides (inline) and deletes (lazy); throwing handler shows the error with retry and logs; authorization hides and 404s; unusable widgets and settings reported. Positional records bind through the two value providers.
- `dotnet test StellarAdmin.slnx`: 2057 passed, 0 failed, across the six test projects.
- AdminPlayground home dashboard (`sandbox/AdminPlayground/Dashboard/`, partials in `Areas/StellarAdmin/Views/Home/`): checked headless at 1440 and 390 px on port 5206 in light mode: all four widgets load, the **Top categories** picker swaps the widget in place and keeps its value, the **Team** widget polls every 30 seconds, no console errors. A query bug in the playground's handler showed the error cell working. Dark mode, Firefox and Safari not checked.
- Consumer skill: `skills/stellar-admin/references/widgets.md`, task table and not-supported list; `build/skill-validation` check passes.
- Website: `content/docs/admin/widgets.mdx`, `home-page.mdx` updated, nav entry; `pnpm types:check` passes. Not rendered in a browser.

Phase 2 (2026-10-30):

- `tests/StellarAdmin.IntegrationTests/Resources/ResourceIndexWidgetTests.cs`, 9 tests: placements around the grid; no regions without widgets; a hidden region a search fills; the context's search and scope; a search renders widgets and a grid request doesn't, and keeps the search box's regions; a handler widget loads from the resource with the search; a home payload is refused by the resource; an unknown widget 404s; problems reported for the resource. `HomePageWidgetTests` also checks that a `Placement` on the home page is reported.
- `dotnet test StellarAdmin.slnx`: 2066 passed, 0 failed.
- AdminPlayground Products index checked headless on port 5206 at 1440 and 390 px in light mode: both widgets load; the aside column is 320px beside the grid at 1440 and below it at 390; paging kept the widget elements; a search replaced them with the filtered summary; the aside picker swapped in place; the **Under 50** scope reloaded the summary; no console errors. This check caught that the search box swapped in by a paging request had lost the widget regions, now fixed and tested. Dark mode, Firefox and Safari not checked.
- Consumer skill: `references/widgets.md` (index pages section, `Placement`), `index-pages.md` and `SKILL.md`; `build/skill-validation` passes.
- Website: new `content/docs/admin/index-page-widgets.mdx` in the Index Pages group, links from `index-pages.mdx`, `row-details.mdx` and `widgets.mdx`; `pnpm types:check` passes. Not rendered in a browser.

The prototypes are point-in-time concept pages and are not library code.
