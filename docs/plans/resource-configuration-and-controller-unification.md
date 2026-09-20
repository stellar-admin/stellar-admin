# Resource configuration and controller unification

Status: revised rebuild plan agreed on 2026-09-19. Integration detachment is committed as `f936c29`; the resource reset is committed as `2f8b9d1` on `resource-redesign`. Steps 1 and 2 (resource registration and naming, then a working index page) are implemented. Step 3 (basic create) is committed as `ffb7538`. Before step 4, consolidate Dashboard tests, add a create factory callback, and introduce global label templates, in that order. This sequence supersedes the original three-phase plan; action-specific form models now come immediately after basic CRUD and before integrations.

## Objective

Build a simple standalone resource foundation in Dashboard, then make EF Core and Identity integrations use its builders and controller workflow. Resource identity comes from TResource. Labels derive from that type with optional SingularLabel and PluralLabel overrides. Integrations supply configuration and persistence/domain operations without introducing a separate form configuration system.

## Current baseline

The old resource builders, page/default options, controller base, query machinery, and related test projects have been removed. Dashboard retains its shell, Razor rendering, editors, field/layout definitions, and rendering models. These are reusable building blocks and may be simplified as the replacement API develops.

EF Core, Identity, and IdentitySimplePlayground remain detached from the solution and build pipeline. Their source references removed APIs and is retained for later adaptation. The active tests cover Core, TagHelpers, and replacement Dashboard resource configuration. The new Dashboard integration suite verifies index rendering and the basic create flow through HTTP.

## Design rules

- Keep the shared resource implementation independent of EF Core and Identity. Prove it with an in-memory Product sample.
- Use a simple builder over resource configuration. Derive labels from TResource and optional SingularLabel/PluralLabel; explicit page labels take precedence. Do not restore captured defaults, reset machinery, or compatibility shims from the rejected implementation.
- Keep field and layout configuration on the same builder path for ordinary resources and integration defaults. Seed integration definitions through that builder before applying application configuration.
- Shared controller behavior owns HTTP flow, binding, validation redisplay, rendering, and redirects. Persistence and domain operations belong to the application or integration and resolve request-scoped services through DI.
- Keep TResource as the resource identity while allowing each form action to use its own typed model. Entity-backed forms remain the default. Loading and submission must explicitly handle mapping; do not introduce a speculative generic mapping framework.
- Follow the maintained options-builder conventions. Concrete public signatures beyond the agreed resource registration and label concepts remain implementation decisions.

## Rebuild sequence

Each step is a bounded implementation increment with focused tests and a compiling sample where applicable. Record actual verification as each step lands. This plan records the intended sequence and does not itself authorize implementation, commits, publishing, or deployment.

### 1. Resource registration and naming — completed

Introduce AddResource<TResource>(...), its builder, and resource options. Derive readable singular/plural labels from the type, allow SingularLabel and PluralLabel overrides, and establish straightforward override rules. Prove configuration resolution and isolation between resources. This step only registers configuration.

### 2. Working index page — completed

Use DashboardPlayground for a Product sample backed by an in-memory data source. Add column configuration and the minimum controller behavior needed to display products. Prove the full registration-to-controller-to-rendering path without integration dependencies.

### 3. Working create form — implemented, awaiting user review

Add typed field configuration, page titles, submit labels, model binding, validation, and saving through the sample's data source. Derive default labels from the resource labels and honor explicit overrides. Prove configured writable-field binding, invalid submission redisplay, and successful submission. Apply appropriate authorization and antiforgery protection as write actions are introduced.

### Before layout: foundation follow-ups — revised 2026-09-20

1. Consolidate Dashboard tests around the public builders and HTTP behavior. Remove redundant options/controller tests, retain a small unit suite for non-HTTP contracts, and establish this approach before adding factories or templates.
2. Add an optional create factory callback for resources without a parameterless constructor. Use it for GET and POST initialization before configured-field binding, retaining parameterless construction as the default.
3. Add overridable global label templates such as `Create {SingularLabel}` and `Edit {SingularLabel}`. Explicit page titles and submit labels take precedence. Keep resolution simple, without captured defaults or synchronization machinery.

Factory and template implementation remain pending. Prove them with focused HTTP scenarios rather than parallel options and controller unit tests. Advanced layouts remain on hold until these foundations are reviewed.

### 4. Form layout

Add sections, rows, and groups to the same form builder. Exercise them in the Product sample using the retained rendering components. Prove layout configuration, binding through nested layout containers, and rendering at desktop/mobile widths.

### 5. Edit and delete

Extend the shared controller flow with record lookup, loading edit values, updates, deletion, missing-record handling, authorization, and antiforgery protection. Prove success and failure behavior using the in-memory sample and focused request tests.

### 6. Action-specific view models

Give Product separate create and edit models, including a form-only field. Prove typed field/layout configuration, initialization/loading, binding, validation, error redisplay, and explicit mapping to the resource through the normal controller workflow. Edit identifies the resource through the route independently of posted values. Keep the action model distinct from the page presentation model.

This step must establish the core capability before adding index features or reconnecting integrations. It should expose any assumptions that every form uses TResource early enough to correct them. Decide how selecting a different form model interacts with previously configured fields and page settings. Password and confirmation fields will later use this same capability in Identity, with appropriate sensitive-value redisplay handling.

### 7. Index features

Add paging, sorting, searching, and scopes incrementally. Keep data access independent of EF and avoid assuming a provider supports EF asynchronous query methods. Verify each feature against the standalone sample before broadening the API.

### 8. Reconnect integrations

Adapt EF Core first, then Identity. Reattach each integration, its sample, and appropriate new tests only after adapting it to the proven core. EF supplies database operations and provider-specific behavior. Identity seeds fields through the normal resource builder, applies user configuration through the same builder, and performs writes through UserManager and RoleManager.

Prove Identity user creation with an action-specific model for password and confirmation through the shared controller workflow. Preserve integration domain requirements, including error translation and self-deletion prevention, and review routes, view overrides, authorization, and EF reference behavior during adaptation. Identity integration is not complete while password fields require a separate form configuration or CRUD flow.

## Verification and scope

Use integration-first TUnit coverage through public builders and HTTP, following the repository's Dashboard test strategy. Retain unit tests only for useful contracts that HTTP cannot express clearly. The shared Dashboard tests must not depend on EF or Identity. DashboardPlayground retains the host’s ASP.NET Core EF/Identity setup for future use, but its Product resource uses an independent in-memory data source. Run affected builds/tests per step and the active solution checks at delivery. Exercise rendered sample pages when adding UI behavior; leave the user's port 5205 process alone.

Update relevant maintained guidance and generated references when affected. Record commands, results, remaining decisions, and limitations here. Website work, unrelated backlog features, and new Identity workflows are outside this rebuild.

## Plan revision — 2026-09-19

Replaced the superseded implementation phases with the agreed eight-step rebuild sequence. Moved action-specific view models to step 6, immediately after edit/delete and before index features and integrations. Updated the plan index. Documentation only; no application builds or tests were rerun. Checked the documentation diff with git diff --check.

## Step 1 implementation — 2026-09-19

Added `AddResource<TResource>()` and its configuration overload, `ResourceBuilder<TResource>`, and `ResourceOptions<TResource>`. The builder is a facade over typed options registered through the standard options pattern. Humanizer.Core 2.14.1 supplies readable type names and English pluralization. A singular override affects the inferred plural. An explicit plural remains authoritative regardless of assignment order. Blank labels are rejected. Generic type arity is omitted from display labels.

Repeated resource registration composes callbacks in registration order. A no-argument call preserves configured values. Each resource type and service provider resolves a separate options instance. Defaults and application overrides can therefore share the same builder path without captured defaults or reset machinery. This step adds configuration only. Resource routes, navigation, page options, CRUD controllers, persistence, and the standalone sample remain for subsequent steps. EF and Identity remain detached.

Created a new Dashboard TUnit project through the CLI, added it to the solution, and introduced 16 tests covering default labels, compound/acronym/generic names, irregular plurals, explicit override order, updated singular fallback after reading a default, blank-label rejection, repeated registration, null callbacks, and isolation between resource types and service providers. Updated development commands and Dashboard setup notes.

Verification: Release solution build passed. The initial restore reported unavailable NuGet vulnerability metadata and existing source warnings. The final Release solution build completed with 12 warnings in unchanged source and zero errors. `dotnet test --solution StellarAdmin.slnx --no-build --configuration Release --minimum-expected-tests 1` passed all 158 tests, including 16 Dashboard cases. Tests ran outside the sandbox because local IPC was denied inside it. Solution-wide test discovery listed all 16 new cases. CSharpier and git diff --check passed. The consumer reference drift check passed. No browser checks apply to this configuration-only increment. Changes are uncommitted.

## Step 1 test approach revision — 2026-09-19

Moved the naming, override-order, fallback, and invalid-label cases from ResourceOptionsTests into StellarAdminDashboardBuilderExtensionsTests at the user's request. All resource configuration now goes through AddResource<TResource> and its builder callback. Tests resolve typed options only to observe results. The provider-isolation test also uses builder configuration and checks that providers resolve distinct instances. Removed ResourceOptionsTests. Blank-label cases now verify rejection during configuration resolution rather than directly inspecting options after a failed setter.

Verification: `dotnet build tests/StellarAdmin.Dashboard.Tests --configuration Release -m:1` passed with one existing XML documentation warning. The initial combined dotnet run/build attempt failed without a diagnostic. Running the explicit build and then `dotnet run --project tests/StellarAdmin.Dashboard.Tests --configuration Release --no-build` passed all 16 cases. Test discovery via `-- --list-tests` passed. CSharpier formatting and git diff --check passed. Production code is unchanged. The solution-wide 158-test result above predates this test-only revision.

## Step 1 language-feature review — 2026-09-19

Reviewed and preserved the user's C# `field` conversion in ResourceOptions. Simplified builder construction with target-typed `new(options)` and made the builder's single-assignment constructor expression-bodied. Kept its explicit internal constructor because a primary constructor on this public class would expose construction publicly. Registration already uses C# extension blocks. Recorded the preference for modern stable C# features in the maintained C# conventions.

Verification: the Dashboard test project Release build passed with one existing XML documentation warning and no new diagnostics. All 16 builder tests passed via `dotnet run --project tests/StellarAdmin.Dashboard.Tests --configuration Release --no-build`. CSharpier formatted the two changed implementation files and git diff --check passed.

## Step 1 builder return correction — 2026-09-19

Changed the no-callback AddResource<TResource>() overload to return ResourceBuilder<TResource> and use the requested summary, "Adds a resource." The callback overload invokes that overload, configures the returned builder immediately, and returns the Dashboard builder. Repeated calls share the registered resource options instance, matching Dashboard's registration-time configuration pattern. The builder constructor remains internal.

This supersedes the earlier deferred-callback and per-provider options behavior recorded above. Options are now shared per resource type and service collection and exposed through IOptions<ResourceOptions<TResource>>. Providers built from the same collection share those options. Separate service collections remain isolated. No copying or parallel configuration path was introduced.

Updated the tests to cover returned-builder configuration, mixed overloads, callback return values, immediate blank-label rejection, and service-collection isolation. Updated Dashboard setup notes and the options-builder conventions to state the overload return pattern and configuration lifetime.

Verification: the Dashboard test project Release build passed with one existing XML documentation warning. All 18 builder tests passed using `dotnet run --project tests/StellarAdmin.Dashboard.Tests --configuration Release --no-build`, and discovery listed all 18 cases. CSharpier formatted the changed C# files and git diff --check passed. Earlier solution-wide results were not rerun for this focused correction.

## Step 1 setter-only builder correction — 2026-09-19

ResourceBuilder now wraps OptionsBuilder<ResourceOptions<TResource>>. Its scalar properties are setter-only and register Configure actions. AddResource uses AddOptions and returns the resource builder. The callback overload invokes the builder callback and returns the Dashboard builder. Removed singleton options lookup and Options.Create registration. This supersedes the preceding registration-time shared-options implementation. Configuration values are applied when options resolve, through the standard configuration, post-configuration, and validation pipeline. Options instances are separate per provider.

Removed the builder-read test and assertions. All configuration flows through setters, with values asserted through resolved options. Added coverage for standard Configure/PostConfigure composition, options validation, and provider isolation. Updated maintained conventions and setup notes to describe the corrected pattern.

Verification: the Dashboard test project Release build passed with one existing XML documentation warning. All 20 tests passed with `dotnet run --project tests/StellarAdmin.Dashboard.Tests --configuration Release --no-build`, and discovery listed 20 cases. CSharpier formatting and git diff --check passed. No full solution rerun for this focused correction.

## Step 1 direct service-collection builder — 2026-09-19

Simplified ResourceBuilder to hold IServiceCollection directly. Setter-only label properties call Configure<ResourceOptions<TResource>>. AddResource registers options with AddOptions and returns a builder over the service collection. The internal constructor and callback overload remain unchanged in behavior. This replaces the OptionsBuilder wrapper without changing the options pipeline or adding configuration state. Updated the maintained builder convention.

Verification: the Dashboard test project Release build passed with one existing XML documentation warning. All 20 existing tests passed with `dotnet run --project tests/StellarAdmin.Dashboard.Tests --configuration Release --no-build`. CSharpier formatted the two changed implementation files and git diff --check passed. No new tests or full solution rerun were needed for this equivalent implementation.

## Historical work records

The records below describe earlier states and verification. They do not override the current baseline or rebuild sequence above.

## Planning verification — 2026-09-19

The product working tree was clean before planning. Inspected the shared builders/options, Identity and EF controllers, registration code, existing layout infrastructure, and maintained follow-up records. This change only adds planning documentation and cross-references. Application builds, tests, and browser checks were not run for the plan.

## Phase 1 rollback — 2026-09-19

The user rejected the phase 1 approach and requested removal of the implementation, including their edits based on it. Restored the affected tracked source, solution, test fixture, design guidance, development guide, and consumer setup reference to HEAD. Removed the new delete builder, configuration reference, regression tests, and Identity unit test project introduced by phase 1. The earlier planning documents remain.

The preferred direction is a simple standalone resource builder with labels derived from the resource type and optional SingularLabel and PluralLabel overrides. EF and Identity may be temporarily detached while that foundation is developed. Neither detaching integrations nor rebuilding the core was performed as part of this rollback.

Verification: checked restored tracked files byte-for-byte against HEAD and confirmed that only the three planning documents remain in Git status. `git diff --check` passed. Builds and tests were not rerun for this rollback. The previous implementation’s test results do not validate a future replacement.

## Integration detachment — 2026-09-19

Created branch `resource-redesign`. Temporarily removed EF Core, Identity, IdentitySimplePlayground, both integration test projects, and their shared Dashboard.Testing host from the solution. The Dashboard integration suite also depends on the Identity playground, so leaving it attached would still build both integrations transitively. All six project directories and their references remain intact for later adaptation and reattachment. Core, TagHelpers, and Dashboard unit tests remain active.

Release package creation, validation, and the consumer smoke script now cover Core, TagHelpers, and Dashboard. Removed the Identity playground client install from release preparation. CI and release test discovery follow the reduced solution. No resource APIs or controllers changed.

Verification: Release solution build passed with 18 warnings in unchanged source and no errors. All 149 tests across the three active unit test assemblies passed. The test runner initially failed to bind its IPC socket inside the sandbox and passed when rerun outside it. Consumer reference drift check passed. A recursive project-reference check confirmed that none of the 13 active projects pulls in a detached project. Shell syntax validation for the consumer smoke script and `git diff --check` passed. The hosted release workflow and full packaged-consumer smoke test were not run.

## Resource layer reset — 2026-09-19

Authorized deletion of the old resource orchestration and related tests before rebuilding. Deleted all shared resource builders, the resource controller base, resource/page/default options, search/scope/sort configuration, expression utilities, and index query processing. Deleted the concrete ResourceIndexPageViewModel that assembled rendering state from those options. No replacement registration, builder, or controller was added.

Retained the Dashboard shell, page tag helpers, Razor views, editors, form field/section/group/row definitions, grid column definitions, and rendering view models/interface. Moved IndexPageSelection into the rendering view-model namespace because the retained index views use its selected URL values. It no longer belongs to query infrastructure. These retained pieces are building blocks, not a functioning resource CRUD pipeline.

Deleted Dashboard.Tests, Dashboard.IntegrationTests, Dashboard.EntityFrameworkCore.IntegrationTests, and Dashboard.Testing, including their project files and fixtures. Removed the last active Dashboard test project from the solution through the .NET CLI. Core and TagHelpers tests remain. EF and Identity library and playground sources remain detached and now reference deleted APIs. Their future adaptation is outstanding. No compatibility shim or replacement test host was introduced.

Verification: `dotnet build StellarAdmin.slnx --configuration Release -m:1` passed with 12 warnings in unchanged source and no errors. `dotnet test --solution StellarAdmin.slnx --no-build --configuration Release --minimum-expected-tests 1` passed all 142 remaining tests (run outside the sandbox for local IPC). Consumer reference drift check and `git diff --check` passed. No browser checks or hosted release run were performed. Passing Core/TagHelpers tests does not provide coverage for the retained Dashboard rendering.

## Step 2 implementation — 2026-09-19

Extracted the action-view lookup into a private `ResourceView(string action, object model)` helper, following the requested convention of trying the action name before `Resource{action}`. `Index` calls it with `nameof(Index)`; future actions can reuse it. Verification: Dashboard unit project Release build passed with the existing XML documentation warning, all 26 Dashboard unit tests passed, and CSharpier and `git diff --check` passed.

Follow-up: the shared resource controller now searches for `Index` through `ICompositeViewEngine.FindView` before falling back to `ResourceIndex`. Applications can provide `Areas/StellarAdmin/Views/Product/Index.cshtml`; normal shared locations and configured view-location expanders also participate. Documented this in the consumer setup reference. Two controller unit cases verify view selection with a test view engine. Verification for this follow-up: Dashboard unit and integration project Release builds passed (one existing XML documentation warning during the unit build); all 26 Dashboard unit tests and six HTTP integration cases passed, including actual rendering through the default fallback. CSharpier and `git diff --check` passed. The full solution suite and browser checks were not repeated for this follow-up.

Implemented the agreed `UseDataSource<TDataSource>()` API and `IResourceDataSource<TResource>` with its initial `ListAsync(CancellationToken)` operation. The concrete data source defaults to scoped registration and preserves any existing application registration. The controller resolves the selected source from the request scope. Write operations remain for the subsequent CRUD steps, including action-specific models before integration adaptation.

Added `Index` and typed `Columns` builders, including column `Title` and `Format`, append/clear behavior, and an optional page title. Column configuration runs through the resource options pipeline and creates separate column options for each resolved options instance. `AddResource<TResource>` registers the closed generic shared controller under the resource type name. `MapStellarAdmin` exposes `/stellaradmin/Product`, independently of display-label changes. Default page titles come directly from the current plural label. There is no captured-default state.

Simplified the retained index presentation model and shared Razor views to columns, items, title, and an empty state. The index no longer assumes create/edit/delete, sorting, search, scopes, or paging are present. These will return through the planned increments. Added the Product model and in-memory ProductDataSource to the user's DashboardPlayground, filled its existing configuration placeholder, mapped Dashboard routes, and linked Products from the host navigation. Preserved the user's EF/Identity host setup and Dashboard project reference. The detached StellarAdmin integration projects remain detached. Automatic resource sidebar entries are not part of this increment.

Added four builder/data-source registration tests and a separate Dashboard integration project with six HTTP test cases. HTTP coverage exercises real MVC routing and Razor rendering, property display metadata, column format/title overrides, encoded cell values, empty results, label/title precedence at a stable route, per-request source resolution, and an unregistered resource returning 404. Hosts and their in-memory state are private to each test. The integration suite does not reference DashboardPlayground, EF Core, or Identity.

Verification: `dotnet build StellarAdmin.slnx --configuration Release -m:1` passed with 12 existing warnings and zero errors. The final unit assertion refinement was then rebuilt with zero warnings/errors. `dotnet test --solution StellarAdmin.slnx --no-build --configuration Release --minimum-expected-tests 1` passed all 172 tests across four assemblies, including 24 Dashboard unit tests and six Dashboard integration cases. Solution-wide `--list-tests` discovered all 172. The solution runner used escalation for its local IPC sockets. Focused Dashboard unit and integration executables also passed in the sandbox. CSharpier and `git diff --check` passed. Consumer reference generation and the subsequent `--check` reported no drift.

Ran DashboardPlayground on an agent-owned port 5206 instance and verified the Product page in Chromium at 1440×1000 and 390×844. Both sizes showed all three seeded products, configured headers and decimal formatting, loaded stylesheets, and no horizontal page overflow. Captured screenshots at `/tmp/product-index-desktop.png` and `/tmp/product-index-mobile.png`. Used decimal sample formatting to avoid the invariant culture's generic currency symbol. Port 5205 was untouched. Changes are uncommitted. Step 3 (create form) is next.

## Step 3 implementation — 2026-09-20

Implemented the agreed basic create API: `resource.Create(...)`, setter-only Title and SubmitLabel, and explicit typed fields with Add, Clear, and field Title overrides. Configuration uses the standard options pipeline and creates independent field definitions per resolved options instance. Selectors reject methods, nested members, and properties without public getters/setters. Retained form field options and editor partials provide rendering. Advanced layout builders remain deferred until the user reviews and accepts the basic flow.

Added `CreateAsync(TResource, CancellationToken)` to the data source. The shared controller constructs a resource through its public parameterless constructor, binds only configured fields under the Entity prefix, validates through MVC, redisplays invalid submissions, and persists valid submissions before redirecting to Index. POST validates antiforgery tokens. Create uses the existing ResourceView helper and falls back to ResourceCreate. The index now links to Create. Both default form title and submit label derive from the current singular resource label.

DashboardPlayground uses a mutable Product with data annotations and explicitly configures Name and Price. Its synchronized singleton in-memory data source preserves created records across requests and assigns IDs. Restarting resets the data. The existing host EF/Identity setup is unchanged. Action-specific models, business validation results, edit/delete, and integration adaptation remain later work.

Added builder tests for replacement, options isolation, and rejected selectors. HTTP tests cover default and overridden labels, configured fields, successful persistence and index redirect, exclusion of forged IDs, required/range/conversion failures with value redisplay, and missing antiforgery tokens. The existing index tests and data source fakes were adapted to the new operation.

Verification: Dashboard unit and HTTP integration Release builds passed, with the existing unresolved FormFieldOptions XML cref warning on the integration build. All 31 unit and 13 HTTP tests passed through their project executables. DashboardPlayground Release build passed. Chromium checks at 1440×1000 and 390×1000 verified form fields and no horizontal overflow, invalid POST errors, and a valid browser submission followed by the index showing the created product. Screenshots are `/tmp/resource-create-1440.png` and `/tmp/resource-create-390.png`. The agent-owned playground on port 5206 and browser were stopped. Port 5205 was untouched.

Final verification: the full Release solution build passed with 11 existing warnings and no errors. Consumer reference drift check passed. CSharpier formatted the touched C# files and `git diff --check` passed. The full solution test suite was not rerun. Changes are uncommitted.

## Integration-first test consolidation — 2026-09-20

Completed the testing follow-up first at the user's request, before factory callbacks and global label templates. Recorded the durable Dashboard strategy in `docs/conventions/unit-testing.md` and updated development guidance and the plan index. Production behavior is unchanged.

Reduced Dashboard coverage from 31 unit plus 13 HTTP cases to 10 unit plus 18 HTTP cases. The retained unit cases cover blank labels (four), unsupported field selectors (three), DI lifetime preservation and replacement (two), and consolidated resource/column/field/editor isolation across providers (one).

Coverage mapping and intentional removals:

- Default and explicit labels and ordinary builder configuration remain covered by existing rendered index/create tests. Strengthened create assertions to reject duplicate fields after clearing and rebuilding configuration.
- Moved column replacement to an HTTP test that checks the actual headers and cells. Added repeated registration cases using both builder overloads to verify singular-derived and explicit plural labels at the stable route.
- Replaced mocked controller/view-engine tests with compiled application Razor overrides for Index and Create on a second resource. Existing Product tests continue to verify shared fallback views. Enabled MVC Razor compilation in the integration project for these fixtures.
- Consolidated three provider-isolation tests into one. Removed separate service-collection isolation coverage because every HTTP scenario uses a private host.
- Removed low-value assertions about builder return references, null callback guards, standard options Configure/PostConfigure/Validate plumbing, and third-party acronym/generic naming details. These are deliberate coverage reductions, not claimed HTTP replacements. Retained irregular plural and override composition checks through rendering.

Verification: both Dashboard Release builds passed without warnings after correcting a namespace in the new Razor fixture. All 18 HTTP cases and 10 unit cases passed. The CI/release solution command `dotnet test --solution StellarAdmin.slnx --no-build --configuration Release --minimum-expected-tests 1` passed all 170 tests across four assemblies. Solution-wide discovery listed 170 tests. After moving the consolidated isolation case to ResourceBuilderTests, its explicit Release build and all 10 unit cases passed again. The combined run/build command had failed without diagnostics, so verification used separate build and no-build run commands. The solution runner required execution outside the sandbox because its local IPC socket was denied inside it. CSharpier and git diff --check passed. No browser checks or consumer-reference regeneration were needed for this test/documentation-only change. Changes are uncommitted.

## Scenario-oriented integration test structure — 2026-09-20

Reorganized the 18 Dashboard HTTP cases into `Resources/ResourceIndexTests.cs` (three), `ResourceCreateTests.cs` (seven), `ResourceConfigurationTests.cs` (six), and `ResourceViewOverrideTests.cs` (two). Renamed methods to `Scenario_ExpectedOutcome` without controller action prefixes. Extracted the existing host setup into `Infrastructure/DashboardTestHost.cs` and sample models, data sources, and per-test state into `Fixtures/`. The create form helper remains private to ResourceCreateTests. Real Razor overrides remain under `Areas/StellarAdmin/Views/CustomProduct`. Removed the old controller test files and their empty directory.

This is an organizational change. No scenarios or assertions were added or removed. Each test still creates and disposes its own host and state. Updated the testing conventions to distinguish production-type-based unit tests from feature/scenario integration tests, and documented the folders in development guidance.

Verification: `dotnet build tests/StellarAdmin.Dashboard.IntegrationTests --configuration Release -m:1` passed with zero warnings and errors. `dotnet run --project tests/StellarAdmin.Dashboard.IntegrationTests --configuration Release --no-build` passed all 18 cases. The same command with `-- --list-tests` discovered all 18 renamed cases. CSharpier and git diff --check passed. The earlier 170-test solution result predates this reorganization. No production changes, browser checks, or commits were made.

## HTML test helper cleanup — 2026-09-20

Added four internal extension helpers in `tests/StellarAdmin.Dashboard.IntegrationTests/Infrastructure/HtmlTestExtensions.cs`: GetDocumentAsync fetches a successful response, parses it, and disposes the response. ReadDocumentAsync parses a caller-owned response without asserting its status or disposing it. RequiredElement returns the first matching element or throws a descriptive exception containing the selector. TextContents returns trimmed text for all matching elements in document order.

Updated the four resource scenario classes to use these helpers for repeated parsing and HTML reads. Requests, response status assertions, binding names, and scenario expectations remain visible in each test. RequiredElement prevents a missing input from passing an empty-value assertion. Explicit absence and element-count assertions retain direct queries. PrepareForm stays private to ResourceCreateTests and now reuses response parsing and required-element lookup. No dependencies or additional test cases were introduced.

Verification: the integration project Release build passed with one existing unresolved XML cref warning in ViewDataKeys.cs and zero errors. `dotnet run --project tests/StellarAdmin.Dashboard.IntegrationTests --configuration Release --no-build` passed all 18 cases. CSharpier and git diff --check passed. The full solution suite was not rerun for this test-helper cleanup. Production code is unchanged. Changes are uncommitted.
