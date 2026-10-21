# Configuration problems without startup failures

Status: in progress on the `configuration-problems` branch (2026-10-21). Phase 1, the [inventory](#inventory), is reviewed. Phases 1 to 3 are committed (6245b5a). Phase 4, request-time errors, is built and verified, and the AdminPlayground part of phase 5 is done. What remains: the rest of phase 5 (most of its integration tests already exist) and phase 6, the consumer guidance. The prototype is `sandbox/html/config-problems.html`. The presentation and the public API are settled (see [Decisions](#decisions)).

## Goal

A misconfigured admin panel must never stop the host application. Today `ValidateOnStart()` on `ResourceOptions<TResource>` and on `IconOptions` turns any configuration mistake into a startup failure of the whole app, and the admin panel is usually one part of a larger application. It also makes every new check a breaking change: configuration that worked in one release stops the app in the next.

Instead, the admin panel finds each problem, works around it, logs it, and in Development shows it to the developer with a floating indicator that opens the list.

## What fails today

- `AddResource` registers eight inline `Validate` rules and nine `IValidateOptions` validators (`src/StellarAdmin/Resources/Options/*OptionsValidator.cs`), then calls `ValidateOnStart()`. `AddStellarAdminUI` does the same for `IconOptions`.
- Removing `ValidateOnStart()` alone is not enough. `IOptions<T>.Value` still throws `OptionsValidationException`, and it throws again on every read because failures are not cached. The sidebar reads every resource's options on every admin page, so one bad resource would return 500 on the whole panel.
- The EF Core integration throws from its `PostConfigure` callbacks (`src/StellarAdmin.EntityFrameworkCore/StellarAdminBuilderExtensions.cs`): an entity not mapped in the context, an unsupported primary key, a many-to-many field without a multi-select editor, a sortable collection column. These surface through the same options read.
- Builder methods throw while `Program.cs` runs: `ArgumentException` for an invalid slug or property selector, `InvalidOperationException` for row detail panels without names, a wordmark logo with a subtitle, and similar.
- Precedent for the new model already exists: `LinkChecker` shows missing sidebar and user menu targets in Development, hides them elsewhere and logs each once. The grid editor renderer shows a missing template only in Development.

## Model

1. **Collect, don't throw.** Every check reports a problem instead of failing. Checks run where the validators run now, after all configuration and post-configuration, and they repair the options so that reading them always succeeds.
2. **Every problem has an effect.** A fallback that keeps the rest of the panel working:

   | Effect | Meaning | Examples |
   | --- | --- | --- |
   | Ignored | A setting falls back to its default. | A default sort that names no sortable column: the list uses the data source's order. Invalid scopes: the scopes bar is not shown. |
   | Adjusted | The panel corrects a setting. | A default page size missing from the page sizes is added to them. |
   | Turned off | One feature becomes unavailable. | Create without a create handler: the New button is hidden and the create route returns 404. A form field without a public setter is left out of the form. A view page without fields is turned off. |
   | Resource off | The resource is unavailable. Last resort, for problems with no narrower fallback. | An EF entity that is not mapped, or has no supported primary key. The resource leaves the sidebar and the command palette, and its routes return 404 (Development shows the problem instead). |

3. **Isolation.** A problem, or an unexpected exception while building one resource's options, affects only that resource. Other resources and the host app keep working. A guarded accessor reads resource options for the sidebar, the palette and the controller, and caches a failure as a Resource off problem instead of rethrowing it on each request.
4. **Logged once, everywhere.** A hosted service resolves every registered resource's options at startup, catching everything, and logs each problem once as a warning in the `StellarAdmin.Configuration` category with a stable event ID per kind of problem. Production operators see the problems in their logs. Nothing in startup throws.
5. **New checks never break apps.** A check added in a later release can only ignore a setting or turn a feature off, with a log entry and the Development indicator, so an upgrade cannot stop an application.
6. **Development shows them.** See [Presentation](#presentation). Other environments show nothing in the UI.
7. **Strict mode is opt-in.** With `ThrowOnProblems` set (see [API](#api)), the startup service throws on the first problem instead of logging it, for teams that want tests or CI to fail.

Each problem carries the resource, the effect, a one-sentence statement of what is wrong, what the panel did, how to fix it, and where it was configured (such as `AddResource<Booking>() → Index → DefaultSort(b => b.DepartsOn)`). These types stay internal.

## Presentation

Chosen by Jerrie on 2026-10-21 from the prototype: style 1 (Ink), bottom centre, opening the sheet.

- **Indicator.** A pill fixed at the bottom centre of the main area (centred on the inset, not on the window, so it follows the sidebar width and the collapsed sidebar). It uses the inverse colours (`--sa-color-ink` background, `--sa-color-surface` text), so it reads as a development tool rather than as part of the page in every theme. In dark mode it is light. A warning-coloured count sits on the left, followed by "Configuration problems". It lifts slightly on hover and has a visible focus ring.
- **Environment indicator.** The pill lives on the bottom edge, and the environment indicator on the top edge, so the two never overlap. With the frame indicator, the pill moves in by the frame's 3px border.
- **Sheet.** Clicking the pill opens a right-hand sheet (`sa-sheet`), titled "Configuration problems", with the description "The admin panel kept running and worked around these. Fix them before you deploy." Problems are grouped by resource, each group with a link to the resource. Each problem is a bordered item: the statement and an effect badge (Ignored, Adjusted, Turned off, Resource off), what the panel did, the fix, and the configuration location in monospace. The footer says that only Development shows this and that every environment logs each problem once as a warning from `StellarAdmin.Configuration`.
- **Rejected.** A top bar, a chip on the environment indicator, in-context alerts with sidebar dots, a header button, and seven other floating styles. A compact panel above the pill was dropped because it adds a click before the details, and the sheet already keeps the page in view.
- **Rendering.** The admin layout renders a replaceable partial, `/Areas/StellarAdmin/Views/Shared/_ConfigurationProblems.cshtml`, like `_EnvironmentIndicator.cshtml`, only in Development and only when there are problems. The pill and the sheet's item styles go in the panel's stylesheet (`src/StellarAdmin/Client/css/`), written with `sa-*` components and theme variables. The pill is a `button` with `commandfor` and `command="show-modal"`, so it needs no new JavaScript.
- Host pages that opt into the admin layout (`StellarAdminLayouts.Dashboard`) get the indicator too.

## API

The configuration API, plus a small reporting API for integrations (decision 9). Problems and the collector stay internal.

```csharp
admin.ConfigureDiagnostics(diagnostics =>
{
    diagnostics.ShowIndicator = false;   // default true; the pill only ever shows in Development
    diagnostics.ThrowOnProblems = true;  // default false; startup fails on the first problem
});
```

- `ConfigureDiagnostics` on `StellarAdminBuilder`, with a `DiagnosticsBuilder`. `Configure*` because diagnostics ship by default, and properties because both settings are scalars ([options builders](../conventions/options-builders.md)). "Diagnostics" leaves room for later developer-facing checks.
- `ShowIndicator` follows `ShowSetupChecklist` on `ConfigureHomePage`. Off hides the pill and the sheet. Problems are still logged.
- Integrations, such as StellarAdmin.EntityFrameworkCore, report problems from a post-configuration callback after repairing the options: `ResourceOptions<TResource>.ReportProblem(EventId eventId, ConfigurationProblemEffect effect, string message, string outcome)`, with the public `ConfigurationProblemEffect` enum (`Ignored`, `Adjusted`, `TurnedOff`, `ResourceOff`). Two repair methods support it: `ResourceFormOptions.RemoveField(FormFieldOptions field)` and `DataGridColumnOptions.DisableSorting()`. The EF Core integration logs its problems with event IDs 1080 to 1085.
- `ThrowOnProblems` applies in every environment. An app that wants it only in tests sets it from its own condition, such as `builder.Environment.IsEnvironment("Testing")`. It throws an `InvalidOperationException` listing every problem, from the startup service.

## Scope

1. The eight inline rules and nine validators in `AddResource`, converted to checks with fallbacks.
2. The EF Core integration's post-configuration throws, converted to Resource off or Turned off problems.
3. `IconOptions` in `StellarAdmin.UI`: drop `ValidateOnStart()` and report a bad icon registration through logging (StellarAdmin.UI has no admin layout to show it in). The panel shows it when the panel is installed.
4. `ValidateOnStart()` removed everywhere.
5. Builder-time throws. The rule: recover when we can, otherwise turn off the smallest piece, and log either way. Only contract violations keep throwing: `ArgumentNullException` for null arguments, and changing `IconOptions` after they are configured. Everything else becomes a problem with a fallback, including an invalid slug (the resource is turned off), selectors that aren't property paths, blank strings and out-of-range numbers. Option setters that throw today store the value instead, and a check repairs it. The inventory lists each throw with its outcome.
6. Request-time configuration errors, such as an editor that doesn't match its property: startup checks where model metadata allows, otherwise the field is left out when it renders and the problem is recorded, instead of returning 500.
7. Not in scope: tag helper argument errors in `StellarAdmin.UI` (they are rendering contracts in the developer's own Razor), argument checks in the APIs that handlers call at request time (`ResourceOperationResult`, `FieldBindingContext`), and errors from the developer's own handlers and data sources.

## Phases

1. **Inventory.** List every check and throw in scope with its proposed effect and fallback, for Jerrie's review. No code.
2. **Model.** The problem collector, the guarded options accessor, checks replacing the validators, the startup logging service, `ConfigureDiagnostics` with `ThrowOnProblems`, and TUnit tests per check (the problem reported and the repaired options).
3. **Presentation.** The partial, the pill and the sheet, extracted from the prototype, honouring `ShowIndicator`. Browser check in light and dark, the four environment indicator states, an expanded and a collapsed sidebar, and the theme fixtures.
4. **Request-time errors.** The checks and render-time fallbacks for [request-time configuration errors](#request-time-configuration-errors), with tests.
5. **Integration tests and samples.** An app with a broken resource starts, its other resources work, the broken one leaves the sidebar, Development renders the pill and Production does not, `ShowIndicator = false` hides it, `ThrowOnProblems` fails startup, and each problem is logged once. AdminPlayground gets a deliberately misconfigured resource behind a switch, so the indicator can be checked.
6. **Consumer guidance.** Also correct `docs/development.md`, which still says an invalid or repeated slug fails at registration. The `stellar-admin` skill reference on configuration problems and logging, and the website docs as a follow-up batch.

## Inventory

Phase 1, 2026-10-21. Every check and throw in scope, with its proposed effect and fallback. "Field" means a field of a create form, an edit form, a view page or an inline row, and "left out" means the panel drops it from that form or page only. A create form that leaves out a field saves the property's default, as it does today for any property the developer leaves out of the form; an edit form leaves the stored value unchanged.

### Resource rules in `AddResource`

| Check today | Effect | Fallback |
| --- | --- | --- |
| Paging: page sizes not positive or not distinct | Adjusted | Drop the invalid and repeated sizes. With none left, use the default paging. |
| Paging: default page size not positive or not in the page sizes | Adjusted | A non-positive default becomes the first valid size. A missing one is added to the sizes, in order. |
| Scopes: blank identifier or title | Ignored | That scope is dropped. |
| Scopes: repeated identifier | Ignored | The later scope is dropped. |
| Scopes: default scope matches no scope | Ignored | No default scope. |
| Default sort selects no sortable column | Ignored | The list uses the data source's order. |
| Create without a create handler | Turned off | No create form: the New button is hidden and the create routes return 404. |
| Edit without an edit handler | Turned off | No edit form, the same way. |
| View page without fields | Turned off | No view page. |
| View page without a view handler | Turned off | No view page. |
| Delete without a delete handler | Turned off | No delete. |

### Resource validators

| Validator: check | Effect | Fallback |
| --- | --- | --- |
| InlineRow: inline rows without a resource key | Turned off | No inline create or edit row. |
| InlineRow: inline create row misses a required property | Turned off | No inline create row. |
| InlineRow: a row field matches no index column | Turned off | That inline row is turned off (a row with a missing field could save a partial record). |
| AddAnother: `KeepForNext` names a field the create form doesn't edit | Ignored | That name is dropped. |
| ImageEditor: image field without `StoreImage` | Turned off | The field is left out. |
| ImageEditor: image or rich text field with a non-positive `MaxFileSize` or no accepted types | Turned off | The field is left out. |
| IconEditor: empty list of icons | Adjusted | Offers every registered icon, as `Icons = null` does. |
| IconEditor: inline layout without a list of icons | Adjusted | Uses the default layout. |
| IconEditor: unregistered icons in the list | Adjusted | They are dropped. With none left, the field is left out (Turned off). |
| FieldBinder: field without a public setter or a binder | Turned off | The field is left out. |
| BadgeDisplay: `UseText` reads another type than the badges show | Turned off | The column or field is left out. |
| ValueText: column or field shows a type with no text of its own | Turned off | The column or field is left out. |
| ComputedColumn: computed column without a title | Turned off | The column is left out. |
| ComputedColumn: computed column sortable without a sort expression | Ignored | The column isn't sortable. |
| ComputedColumn: two sortable columns sort by the same name | Ignored | The later column isn't sortable. |
| ComputedDisplayField: computed field without a title | Turned off | The field is left out. |

### EF Core post-configuration

| Throw today | Effect | Fallback |
| --- | --- | --- |
| Entity not mapped in the context | Resource off | |
| No single public primary key of type `int`, `long`, `Guid` or `string` | Resource off | |
| Many-to-many field without a CLR navigation or a single-key related entity | Turned off | The field is left out. |
| Many-to-many field with a single-value editor | Turned off | The field is left out. |
| Sortable column that reads a collection | Ignored | The column isn't sortable. |
| Form field that is a key, a concurrency token, generated, or has no public setter | Turned off | The field is left out. |
| Any other exception while building the resource's options, such as the context failing to build its model | Resource off | Caught by the guarded accessor. |

### Builder-time throws

| Throw today | Effect | Fallback |
| --- | --- | --- |
| `ConfigureBranding`: wordmark logo with a subtitle | Ignored | The subtitle isn't shown. |
| Row details: no panels | Ignored | No row details. |
| Row details: panel without a name | Turned off | That panel is dropped. |
| Row details: repeated panel name | Turned off | The later panel is dropped. |
| `UseEditor` / `UseDisplay`: handler type doesn't implement the matching handler interface | Turned off | The field is left out. |
| `UseDisplay` on a column with a display that has a handler (`NotSupportedException`) | Ignored | The column uses its default display. |
| Create, edit or view builder whose model no longer matches (the model was selected again) | Ignored | The stale builder's settings are skipped. |
| Two resources under the same slug | Resource off | The later resource. The first keeps the slug. |
| One resource type added again under another slug | Ignored | The later registration. |
| Invalid slug | Resource off | The resource isn't added under any URL. |
| Selector that isn't a property path or lacks public getters | Turned off or Ignored | A field or column is left out. A bad `UseKey` leaves no key, so the key rules turn off edit, view and delete. A bad default sort is ignored. |
| Blank strings, such as a label, a scope identifier or a link target | Ignored | A label falls back to its default. A scope, sidebar link or user menu item is dropped. |
| Out-of-range numbers, such as rich text `Rows = 0` | Ignored | The setting uses its default. |
| Null arguments (`ArgumentNullException`) | Keeps throwing | A contract violation at the call site. |

### `IconOptions` in StellarAdmin.UI

These throw inside `Configure` callbacks, so one bad call breaks every icon in the app.

| Throw today | Effect | Fallback |
| --- | --- | --- |
| `AddIconPack`: a semantic mapping names an icon the pack doesn't supply | Ignored | That mapping is skipped. The pack's icons are still added. |
| `MapSemanticIcon`: the icon isn't registered | Ignored | The mapping is skipped. |
| Changing the options after they are configured (frozen) | Keeps throwing | A runtime contract violation, not configuration. |

`IconOptions` keeps its problems internally and logs them through `StellarAdmin.Configuration` when it is first resolved. The panel's startup service also adds them to its list, so they appear in the pill.

### Request-time configuration errors

These throw while a page renders or a request runs, so they return 500 for one page, not the app: an editor that doesn't match its template (`FieldEditorView`, `EditorClassNamesMapper`), radio class names with no matching element, a Boolean editor on a non-Boolean property, a choice editor without items on a property that isn't an enum or a Boolean, a lookup without `UseItems`, a lookup's `EnableCreate` naming a resource without a create form, on-demand row details without a resource key, and a field path with no matching model property. Each becomes a startup check that leaves the field out (Turned off) where the property types allow, and otherwise the field is left out when it renders and the problem is recorded. Phase 4.

## Phase 2 notes

What was built, 2026-10-21:

- `ResourceOptionsFactory<TResource>` replaces the options factory for each resource. It runs every `Configure` and `PostConfigure` callback, then the checks in a fixed order (labels, handlers, columns and fields, grid layout, view fields, inline rows, add another, paging, scopes and default sort). Any exception turns the resource off. `IOptions` caches the result, so reads never throw.
- The nine validators and the eight inline rules became checks in `src/StellarAdmin/Resources/Options/*Check.cs`. Each one repairs the options and records a `ConfigurationProblem` on `ResourceOptions<TResource>.Problems`.
- Nested builders only see their column, field or form, so they report through `ResourceProblemSink`, a thread-static list that the factory opens around its callbacks. Builder calls outside any resource report to `StellarAdminOptions.Problems`. Value objects created by static factories (sidebar link targets, user menu items, logos, environment indicators and colours) carry an internal `Problem`, and the builder method that receives one reports it and leaves the value out.
- `ConfigurationProblems` collects the panel-wide and resource problems. `ConfigurationProblemsLogger`, a hosted service, logs each once at startup as a warning in `StellarAdmin.Configuration`, with the kind's stable event ID, or throws with every problem when `ThrowOnProblems` is set.
- A turned-off resource leaves the sidebar, the home page and the palette (its sidebar item resolves as hidden), and `ResourceController` returns 404 for all its actions. Showing the problem in Development instead of the 404 is left for phase 3.
- `IconOptions` skips a bad semantic mapping and keeps its problems internally. `IconOptionsLogger` builds the options at startup and logs them with event ID 1090.
- `ConfigureDiagnostics` and `DiagnosticsBuilder` are added as designed, and `ValidateOnStart()` is gone.

Changes from the inventory, decided while building:

- A problem has a message (what is wrong and how to fix it, as the validator messages already said) and an outcome. There is no separate fix or configuration location.
- An editor or display whose handler type doesn't match is ignored (the field keeps its default) rather than leaving the field out. A column display with a handler is ignored the same way.
- An image or rich text field with a non-positive `MaxFileSize` or no accepted types is adjusted to the editor's defaults rather than left out.
- Grid column counts, spans and starts outside 1 to 12 are clamped (Adjusted). A requested start of 0 is reported too, although 0 also marks an automatic start internally.
- Numeric editor and display settings (`LookupSheetOptions`, `RichTextEditor.Rows`, `TextDisplay` and `HtmlDisplay` line clamps) keep their value when set out of range, and report the problem when set while a resource is built.
- The sidebar item types in `Sidebar/Models.cs` keep throwing: custom `ISidebarItemsProvider` implementations build them at request time.

## Open questions

None.

## Phase 3 notes

Built 2026-10-21:

- `_Layout.cshtml` renders `/Areas/StellarAdmin/Views/Shared/_ConfigurationProblems.cshtml` inside the sidebar wrapper (so it reads the sidebar's width) when the environment is Development, `ShowIndicator` is on and there are problems. The partial is the ink pill and an `sa-sheet` (`#configuration-problems`) opened with `commandfor`/`command="show-modal"`, so it needs no JavaScript. The sheet groups problems by resource, with an Open link for resources that are on, and shows each problem's message, effect badge and outcome.
- The partial's model is the internal `ConfigurationProblem` list, so unlike `_EnvironmentIndicator.cshtml` an app can't usefully replace it. An app that wants its own presentation turns the indicator off and reads the log. Changed from the plan's "replaceable partial".
- The styles are in `src/StellarAdmin/Client/css/client.css` (`.sa-configuration-problems*`). The pill centres on the main area for the expanded, offcanvas-collapsed and icon-collapsed sidebar and at phone width, moves in 3px for the frame indicator, and hides while any modal dialog is open (it otherwise sat above the sheet's backdrop).
- A turned-off resource's routes render `ResourceOff.cshtml` in Development (in the admin layout, with a 404 status and the reason) and a plain 404 elsewhere.
- AdminPlayground has a deliberately misconfigured set behind `Playground:ShowConfigurationProblems` (false in `appsettings.Development.json`; set it to true or run with `Playground__ShowConfigurationProblems=true`): an Excursions resource with a computed column without a title, an unsortable default sort, a page size missing from the page sizes and create without a handler; an EF resource for an unmapped `ExcursionBooking` at `/stellaradmin/excursion-bookings`; and a sidebar link with a blank page.

## Phase 4 notes

Built 2026-10-21:

- Startup checks, in `EditorCheck` and `RowDetailsCheck`: a checkbox or toggle editor on a property that isn't a non-nullable Boolean, a choice editor without items on a property that isn't a non-flags enum or a Boolean, and a lookup without `UseItems` fall back to the property's default editor (Ignored), or leave the field out when a binder binds it. Radio class names for elements a radio group doesn't have are cleared. Row detail panels that load on demand are left out when the resource has no key.
- A lookup whose `EnableCreate` has no resource to use (untyped items, no resource for the item type, or one without a create form) is found when the lookup renders, since other resources aren't known while a resource's options are built: the lookup gets no New button, and the problem is reported then.
- The throws that can still only happen while a page renders (an editor whose template is another editor's, a field path the model lacks, a lookup value of another type than its items, and the editor checks above as a fallback) throw the internal `ConfigurationException`. Form fields and grid cells render through `FieldEditorRenderer.Guard`, which catches it, leaves the field out and reports the problem. Other exceptions, such as an app's handler failing, still fail the request.
- Problems found while a page renders are added to `ConfigurationProblems`, logged the first time and listed in the indicator from then on, including on the page that found them, since the layout renders after the body.
- Not changed: request-time argument checks in `ResourceOperationResult` and `FieldBindingContext`, errors from an app's own handlers and data sources (such as `StoreImage` returning no value), and the EF Core data errors (null parent values, null choice values, missing related entities).

## Decisions

Jerrie, 2026-10-21:

1. Presentation: the ink pill at the bottom centre, opening the sheet (see [Presentation](#presentation)).
2. Builder-time throws: null arguments keep throwing, value checks become problems, an invalid slug keeps throwing. Revised the same day: see item 7.
3. Strict mode: yes, opt-in, off by default.
4. AdminPlayground: yes, a deliberately misconfigured resource behind a switch.
5. Applications can turn the indicator off.
6. Strict mode and the indicator switch share one builder: `ConfigureDiagnostics` with `ShowIndicator` and `ThrowOnProblems` (see [API](#api)).
7. After the inventory: recover when we can, otherwise turn off the smallest piece, and log either way. Only null arguments and changes to frozen `IconOptions` keep throwing. An invalid slug turns its resource off, and argument-shape checks become problems (see [Scope](#scope) item 5).
8. Request-time configuration errors are in this work, as phase 4.
9. Integrations report problems through a public API rather than `InternalsVisibleTo` (see [API](#api)).
10. The indicator lists icon-registration problems too, through the public `IconOptions.Problems` (StellarAdmin.UI logs them; the panel only shows them).

## Verification

Phase 2, 2026-10-21, on the branch: the solution builds with no new warnings. Admin panel unit tests 282/282, admin panel integration tests 620/620 (including the new `Diagnostics/ConfigurationProblemsTests`: a broken resource returns 404 and leaves the sidebar while the others work, a problem is logged once with its event ID, `ThrowOnProblems` fails startup listing every problem, and a valid configuration logs nothing), UI unit tests 732/732. EF Core integration tests 121/121 after the EF conversion: an unmapped entity or unsupported key turns the resource off, and unsupported form fields, many-to-many fields with a single-value editor and sortable collection columns are left out or unsorted, each logged. The solution-wide `dotnet test --solution StellarAdmin.slnx --configuration Release --minimum-expected-tests 1` passed 1774/1774. Tests that expected exceptions now check the repaired options and the reported problem.

Phase 3, 2026-10-21: the solution-wide test command passed 1780/1780, including integration tests that the indicator renders only in Development with `ShowIndicator` on, lists problems by resource with Open links, renders nothing for a valid configuration, and that a turned-off resource shows its reason in Development. Headless Chromium screenshots of AdminPlayground with the misconfigured set (port 5208, stopped afterwards): light and dark, the sheet open in both, the frame and banner indicators, the collapsed sidebar, 390px phone width, the ops, soft and ledger theme fixtures, and the turned-off resource page. One fix from them: the pill showed above the sheet's backdrop, so it now hides while a modal is open.

Phase 4, 2026-10-21: the solution-wide test command passed 1784/1784. Tests that expected a 500 for these mistakes now expect the page and a logged problem; new tests cover the editor check and an editor rendered with another editor's template (left out of the form, reported once, listed in the indicator).

Earlier: The prototype was checked in headless Chromium: all eight floating styles in light mode, the ink pill in dark mode with the frame indicator at bottom centre, the bubble expanded, and the panel and sheet open.
