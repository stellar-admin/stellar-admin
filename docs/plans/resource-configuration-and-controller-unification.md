# Resource configuration and controller unification

Status: revised rebuild plan agreed on 2026-09-19. Integration detachment is committed as `f936c29`; the resource reset is committed as `2f8b9d1` on `resource-redesign`. Steps 1 and 2 (resource registration and naming, then a working index page) are implemented. Step 3 (basic create) is committed as `ffb7538`. Dashboard test consolidation and the create factory callback are implemented. Global delegate-based label defaults and step 4 (advanced layouts) are implemented. Step 5 was split for review. Edit is committed as `71635f8`. Delete is committed as `dd892b4`. Operation results are committed as `de44966`. Split data source contracts and custom create are implemented for review. Custom edit is next after review. This sequence supersedes the original three-phase plan; action-specific form models now come immediately after basic CRUD and before integrations.

## Objective

Build a simple standalone resource foundation in Dashboard, then make EF Core and Identity integrations use its builders and controller workflow. Resource identity comes from TResource. Labels derive from that type with optional SingularLabel and PluralLabel overrides. Integrations supply configuration and persistence/domain operations without introducing a separate form configuration system.

## Current baseline

The old resource builders, page/default options, controller base, query machinery, and related test projects have been removed. Dashboard retains its shell, Razor rendering, editors, field/layout definitions, and rendering models. These are reusable building blocks and may be simplified as the replacement API develops.

EF Core, Identity, and IdentitySimplePlayground remain detached from the solution and build pipeline. Their source references removed APIs and is retained for later adaptation. The active tests cover Core, TagHelpers, and replacement Dashboard resource configuration. The Dashboard integration suite verifies index rendering and create/edit flows through HTTP.

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

Test consolidation, the creation factory, and global delegate-based labels are implemented and reviewed. Advanced layouts are implemented below. Continue proving subsequent features through focused HTTP scenarios rather than parallel options and controller unit tests.

### 4. Form layout

Add sections, rows, and groups to the same form builder. Exercise them in the Product sample using the retained rendering components. Prove layout configuration, binding through nested layout containers, and rendering at desktop/mobile widths.

### 5. Edit and delete

Edit was implemented first and committed after review. Delete was subsequently authorized on 2026-09-20 and is now implemented for review.

Extend the shared controller flow with record lookup, loading edit values, updates, deletion, missing-record handling, authorization, and antiforgery protection. Prove success and failure behavior using the in-memory sample and focused request tests.

### 6. Action-specific view models

Give Product separate create and edit models, including a form-only field. Prove typed field/layout configuration, initialization/loading, binding, validation, error redisplay, and explicit mapping to the resource through the normal controller workflow. Edit identifies the resource through the route independently of posted values. Keep the action model distinct from the page presentation model.

This step must establish the core capability before adding index features or reconnecting integrations. It should expose any assumptions that every form uses TResource early enough to correct them. Decide how selecting a different form model interacts with previously configured fields and page settings. Password and confirmation fields will later use this same capability in Identity, with appropriate sensitive-value redisplay handling.

#### Agreed implementation checkpoints — 2026-09-20

1. Introduce shared operation results in existing data source create, update, and delete operations. Map field and general validation errors to the form, preserve submitted values, return 404 for missing records, and redisplay the index with a summary for rejected deletes. Prove persistence failures through HTTP scenarios. Keep current resource models.
2. Settle the data source contract before adding handlers. Sketch ordinary and custom registrations together and decide how consumers avoid implementing unused operations. Implemented using a listing data source, per-action handler interfaces, and the combined IResourceCrudDataSource interface.
3. Implement custom create end-to-end with `Create<TModel, THandler>()`. A custom model requires a compatible handler. Do not expose a model-only overload. Callback registration returns the resource builder, and no-callback registration returns the action builder. Preserve typed fields/layouts and factory support. Keep the shared controller responsible for HTTP binding, validation, antiforgery, and responses. Add only the internal typed connection required to execute create.
4. Demonstrate a separate create model with password and confirmation in DashboardPlayground. Verify success, model validation, and handler validation with scenario integration tests. Pause for user review before custom edit.
5. Extend the reviewed design to `Edit<TModel, THandler>()`, including loading and route-key preservation. Prove custom create with ordinary edit and ordinary create with custom edit.

Ordinary create and edit continue to use TResource and the data source. Custom handlers contain persistence and model-loading behavior, without requiring an application controller. Expected persistence rejection uses operation results. Unexpected exceptions propagate. Error field names refer to model properties without the MVC binding prefix. Handler contracts and internal adapters remain design sketches until their checkpoint. Decide how errors for unrendered fields remain visible.

### 7. Index features

Add paging, sorting, searching, and scopes incrementally. Keep data access independent of EF and avoid assuming a provider supports EF asynchronous query methods. Verify each feature against the standalone sample before broadening the API.

### 8. Reconnect integrations

Adapt EF Core first, then Identity. Reattach each integration, its sample, and appropriate new tests only after adapting it to the proven core. EF supplies database operations and provider-specific behavior. Identity seeds fields through the normal resource builder, applies user configuration through the same builder, and performs writes through UserManager and RoleManager.

Prove Identity user creation with an action-specific model for password and confirmation through the shared controller workflow. Preserve integration domain requirements, including error translation and self-deletion prevention, and review routes, view overrides, authorization, and EF reference behavior during adaptation. Identity integration is not complete while password fields require a separate form configuration or CRUD flow.

## Follow-up after the rebuild sequence

After steps 1–8, audit resource-facing text for remaining hardcoded wording, including the empty index message “No records found”. Make applicable text derive from the resource labels through the existing global delegate-based defaults and resource/page overrides. Review other empty states, action text, and confirmation messages as part of the same task. Requested on 2026-09-20 and deferred until the current list is complete. No rendering or label changes are included now.

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

## Creation factory callback — 2026-09-20

Added `create.UseFactory(Func<TResource>)` to the create builder. The nullable `ResourceCreateOptions<TResource>.Factory` is configured through the existing options pipeline. Both controller actions call `CreateInstance()`, which invokes the configured factory or falls back to `Activator.CreateInstance<TResource>()` when no factory is supplied. GET invokes the factory for initial form values; POST invokes it again before binding configured fields, then follows the existing validation and persistence flow. Invalid submissions redisplay the bound instance without invoking the factory again. The callback should return a new instance on each invocation.

DashboardPlayground demonstrates factory-provided initial pricing. Added three HTTP cases using InventoryItem, which requires a constructor argument: initial values render, a valid submission persists the fresh factory instance, and an invalid submission redisplays submitted values without persistence. Submission coverage also proves an unconfigured posted field cannot overwrite the factory's value. Existing Product cases continue exercising default parameterless construction. No new unit tests or dependencies were added.

Global label templates are next; advanced layouts remain on hold.

Verification: `dotnet build StellarAdmin.slnx --configuration Release -m:1` passed with 12 existing warnings and zero errors. After correcting a test expectation for MVC's empty-string-to-null binding, the integration project Release rebuild passed with zero warnings/errors and all 21 HTTP cases passed using `dotnet run --project tests/StellarAdmin.Dashboard.IntegrationTests --configuration Release --no-build`. Discovery listed the three new factory cases. All 10 Dashboard unit tests passed. CSharpier and `git diff --check` passed. The full solution test suite and browser checks were not run. Changes are uncommitted.

Follow-up verification after making Factory nullable and adding CreateInstance: the integration project Release build passed with the existing ViewDataKeys XML cref warning; all 21 HTTP cases passed again. CSharpier and git diff --check passed.

ResourceController now resolves options.Value once into a readonly field during construction and uses that field throughout. The integration project Release build passed with the existing ViewDataKeys XML cref warning; all 21 HTTP cases passed again. CSharpier and git diff --check passed.

## Global resource label callbacks — 2026-09-20

Implemented `dashboard.ConfigureResourceLabels(...)` with setter-only IndexTitle, CreateTitle, and CreateSubmitLabel callbacks. Each is a `Func<ResourceLabelContext, string>` receiving the resource’s effective SingularLabel and PluralLabel. ResourceLabelOptions supplies the existing default wording, and the builder uses the standard options configuration pipeline. ResourceController resolves label options once into a readonly field and invokes callbacks when constructing page models, only where an explicit resource title or submit label is absent. No parsed templates, extra dependencies, or captured defaults were introduced. Edit/delete settings remain deferred until those actions exist.

DashboardPlayground demonstrates an “Add Product” title and “Save” submit button. Added two HTTP cases for global labels and explicit resource precedence, and extended existing invalid-submission cases to verify callback-generated labels on redisplay. Existing scenarios continue verifying the built-in defaults. No new unit tests were added.

Verification: the initial parallel integration build stopped during restore without diagnostics. The explicit single-worker integration build with `--no-restore` passed with the existing ViewDataKeys XML cref warning. `dotnet build StellarAdmin.slnx --configuration Release --no-restore -m:1` passed with 12 existing warnings and zero errors, including DashboardPlayground. All 23 HTTP integration cases and all 10 Dashboard unit cases passed via their Release project executables with `--no-build`. CSharpier checked all eight touched C# files and `git diff --check` passed. The full solution test suite and browser checks were not run. Changes are uncommitted.

## Index create button label — 2026-09-20

Replaced the hardcoded index Create button text with a resolved view-model label. The global `IndexCreateLabel` callback defaults to “Create”; `resource.Index(index => index.CreateLabel = "...")` supplies an explicit resource override, with null falling back to the global callback. The callback receives the same effective resource labels as the index title. Extended existing HTTP scenarios to cover default text, global callback text, and resource override precedence without adding test cases.

Verification: the integration project Release build (`--no-restore -m:1`) passed with the existing ViewDataKeys XML cref warning and no errors. All 23 HTTP integration cases passed with `--no-build`. CSharpier and `git diff --check` passed. Full solution tests and browser checks were not run. Changes are uncommitted.

## Step 4: advanced form layouts — 2026-09-20

Added AddSection, AddRow, and AddGroup to ResourceFieldsBuilder, including callback overloads returning the parent and no-callback overloads returning the child builder. Sections support a title and optional Description. Nested builders share the existing typed Add field API, and Clear removes only the current scope's fields and containers. Root configuration still uses the standard options pipeline, with fresh containers and fields for each resolved options instance. No Identity or EF dependencies were introduced.

ResourceCreateOptions.Items is now the authoritative ordered layout tree. Fields is a read-only flattened projection used for the binding allowlist and field metadata. ResourceController passes the tree to the retained Razor partials, so nested fields retain the same Entity binding prefix, validation, persistence, and invalid-submission redisplay. Create.SectionLayout optionally overrides the application's form setting. No Razor, Tag Helper, or CSS changes were necessary.

DashboardPlayground demonstrates a Product details section with a description and two row columns, each containing a field group. The API remains straightforward for forms that need only fields:

```csharp
create.Fields(fields =>
{
    fields.AddSection("Product details", section =>
    {
        section.Description = "Information shown in the catalog.";
        section.AddRow(row =>
        {
            row.Add(product => product.Name);
            row.Add(product => product.Price);
        });
    });
});
```

Added three HTTP cases covering nested markup and field order, encoded section text, global/per-resource section layout selection, and clearing the entire layout. Extended the existing valid and invalid submission scenarios to use nested sections/rows/groups, including exclusion of a field removed by a nested Clear. Extended the existing provider-isolation unit case to include layout containers. Ordinary flat forms remain covered by the original rendering and factory scenarios. Dashboard now has 26 HTTP cases and 10 unit cases.

Verification: the initial integration build caught an incorrect ConfigureForms test setup, which was corrected. The full Release solution build with --no-restore -m:1 passed with 12 existing warnings and one new test nullability warning. That warning was corrected, and the subsequent integration Release build passed with zero warnings/errors. The CI command `dotnet test --solution StellarAdmin.slnx --no-build --configuration Release --minimum-expected-tests 1` passed all 178 tests across four assemblies. The solution runner ran outside the sandbox for its local IPC socket. CSharpier formatted the eight touched C# files and git diff --check passed.

Chromium checks at 1440×1000 and 390×1000 verified columns side by side on desktop and stacked on mobile, no horizontal overflow, invalid POST errors, and a successful browser submission followed by the index showing the new item. Screenshots were visually reviewed at /tmp/resource-layout-1440.png and /tmp/resource-layout-390.png. The agent-owned playground on port 5206 and browser were stopped. Port 5205 was untouched. Changes are uncommitted. Edit/delete is next, followed immediately by action-specific view models.


## Step 5a: edit — 2026-09-20

Implemented edit only. Changes remain uncommitted for review. Delete has not been started.

### Configuration and data source

`resource.UseKey(product => product.Id)` selects a direct readable property. Its value is formatted with invariant culture for URLs. `resource.Edit(edit => edit.Fields(...))` uses the same field, section, row, and group builders as Create, with independent configuration. Shared form settings now live in ResourceFormOptions, inherited by ResourceCreateOptions to retain its factory. ResourceFieldsBuilder accepts a configuration callback and no longer assumes the create action.

IResourceDataSource now includes `FindAsync(string id, CancellationToken)` and `UpdateAsync(string id, TResource resource, CancellationToken)`. Lookup returns null for a missing record. Update returns false when the target no longer exists. Lookup results must remain unpersisted until a successful update. The in-memory sample returns a copy and replaces the stored record under its lock only when saving. Keys are explicit single properties in this increment. Composite-key configuration and optimistic concurrency handling are not implemented.

### Controller and rendering

Index rows have Edit links when a key selector is configured. Edit GET loads existing values. Edit POST loads a fresh instance, binds only configured edit fields, excludes the key property, validates, persists, and redirects to Index without the route key. The route id is explicitly bound from route data, preventing query/form values from selecting a different record. Missing records return 404 on GET and POST, including a record that disappears before update. POST requires antiforgery validation. Authorization remains controlled by the host configuration. No built-in Dashboard authorization policy was introduced in this increment.

Global EditTitle, EditSubmitLabel, and IndexEditLabel callbacks default to `Edit {SingularLabel}`, `Save {SingularLabel}`, and `Edit`. Edit Title/SubmitLabel and Index EditLabel override those defaults. Applications can provide Edit.cshtml through the existing resource view lookup, falling back to ResourceEdit.cshtml and the shared form renderer.

DashboardPlayground registers Product's Id key and a separate edit section/row with Name and Price. Create retains its existing factory and layout.

### Verification

Added edit HTTP scenarios for index links, existing values, nested layout, default/global/resource labels, successful updates, unconfigured field preservation, forged route/key values, required/range/conversion validation redisplay, antiforgery rejection, malformed/missing keys, disappearance before save, and an application Edit.cshtml override with a string key. Shared antiforgery form preparation was extracted from ResourceCreateTests. Added one unit test rejecting a computed key selector. Existing create scenarios still pass.

`dotnet build StellarAdmin.slnx --configuration Release --no-restore -m:1` passed. The final incremental build reported one existing unresolved XML cref warning in ViewDataKeys.cs and no errors. The initial full build reported 12 existing warnings. A project build without `-m:1` exited unsuccessfully without diagnostics, and the serial build passed. `dotnet test --solution StellarAdmin.slnx --no-build --configuration Release --minimum-expected-tests 1` passed all 194 tests, including 41 Dashboard HTTP cases and 11 Dashboard unit cases. The solution runner required execution outside the sandbox for its local IPC sockets. An initial HTTP run had three selector assertions counting ASP.NET Core's hidden input. Restricting those assertions to Entity fields resolved them. CSharpier and `git diff --check` passed.

Chromium verified index edit links, prefilled values, desktop/mobile layout at 1440×1000 and 390×1000, no horizontal overflow, invalid submission errors, successful browser save, index redirect, and reloading persisted values. Screenshots were visually inspected at /tmp/resource-edit-1440.png and /tmp/resource-edit-390.png. The agent-owned browser and playground on port 5206 were stopped. Port 5205 was untouched.

### Edit button review correction — 2026-09-20

Restored the pre-redesign index edit button from commit 97691a8: Outline variant, IconSmall size, square-pen icon, and an untitled action column. The configurable edit label now supplies aria-label and title. The initial text-only ghost button was an unintended presentation change. Updated the existing label assertion to verify the accessible name. The Dashboard integration project Release build passed with the existing ViewDataKeys XML cref warning, all 41 HTTP tests passed, and CSharpier and git diff checks passed. No new browser check was performed for this markup-only correction.

### StellarAdmin edit tooltip — 2026-09-20

Replaced the native title tooltip with sa-tooltip, linked through interestfor with a unique ID per rendered row. The button retains aria-label. Both use the configured IndexEditLabel, including the user’s updated default of `Edit {SingularLabel}`. Updated the existing HTTP scenario to verify tooltip association, configured content, and absence of title. The final Dashboard integration Release build passed with no warnings or errors, and all 41 HTTP tests passed. No new browser check was performed for this change.

The tooltip ID now uses `--resource-edit-{rowId}` instead of a random GUID, as requested. The row key is resolved once and reused for the edit route and tooltip ID. The Dashboard integration Release build and all 41 HTTP tests passed after this correction.

## Step 5b: delete implementation — 2026-09-20

Added `IResourceDataSource<TResource>.DeleteAsync(id, cancellationToken)`, returning false for a missing resource. The shared controller accepts an antiforgery-protected POST, takes the key only from the route, returns 404 for missing resources or unconfigured keys, and redirects to the index with the route id cleared. Host authorization remains the application's responsibility, as with the existing actions.

The index now offers an outline trash icon button beside edit, with a StellarAdmin tooltip identified by the resource key. It reuses the existing shared StellarAdmin alert dialog for confirmation. Cancelling leaves the resource untouched. Confirming submits the row's normal POST form. No HTMX dependency was introduced. This increment exposes deletion from the index. The retained form-page delete affordance is not wired into edit.

Global delegate defaults cover `DeleteTitle`, `DeleteMessage`, `DeleteConfirmLabel`, `DeleteCancelLabel`, and `IndexDeleteLabel`. Resource overrides are available through `resource.Delete(...)` and `resource.Index(index => index.DeleteLabel = ...)`. The existing hardcoded-text audit remains deferred until after the rebuild sequence.

Updated active sample and test data sources. The sample now handles creating a resource after deleting every existing item. Added ten integration cases covering label defaults and overrides, rendered confirmation, route-key protection, successful deletion and redirect, missing records including disappearance after rendering, GET rejection, missing antiforgery tokens, and resources without keys. Existing create and edit scenarios remain intact.

Verification: the final Release solution build with `--no-restore -m:1` passed with one existing XML documentation warning in `ViewDataKeys`. The solution test command `dotnet test --solution StellarAdmin.slnx --no-build --configuration Release --minimum-expected-tests 1` passed all 204 cases, including 51 Dashboard HTTP cases. CSharpier formatted the touched C# files and `git diff --check` passed. An isolated Chromium session exercised the sample at desktop and mobile widths, checked confirmation and cancellation, deleted all three sample records, and successfully created a resource afterward. The agent-owned sample server on port 5206 and browser were stopped. Changes are uncommitted. Action-specific view models are next after review.


## Step 6a: operation results — 2026-09-20

Implemented the first agreed checkpoint without introducing action-specific models or handlers. CreateAsync, UpdateAsync, and DeleteAsync now return ResourceOperationResult with Success(), NotFound(), or ValidationFailed(...) outcomes. ResourceValidationError carries a model property name and message, with null representing an operation-level error. Result construction snapshots errors and rejects empty error collections or blank messages. Expected failures must not persist changes. Unexpected exceptions remain exceptions.

The shared controller maps configured field errors to the Entity binding prefix, redisplays create/edit with attempted values, and puts general errors and errors for unrendered fields into the summary. Delete validation failures reload the index in the same response and show all messages in its summary. This preserves ModelState without TempData. Refreshing that POST can resubmit the delete. Missing-resource results return 404, while successful operations retain their existing redirects. Applications overriding the full index view can render the errors through the standard MVC validation summary.

Updated all active sample and fixture data sources to the new return contract. Added three HTTP scenarios covering create/edit value preservation, field and general errors, unrendered-field fallback, unchanged persistence after rejection, and rejected deletion with encoded messages and the record still visible. Existing success, not-found, model-validation, antiforgery, and view-override scenarios remain passing. No duplicate unit suite was added.

Verification: the final Release solution build (`dotnet build StellarAdmin.slnx --configuration Release --no-restore -m:1`) passed with one existing unresolved XML cref warning in ViewDataKeys. The initial full build reported 12 existing warnings. `dotnet test --solution StellarAdmin.slnx --no-build --configuration Release --minimum-expected-tests 1` passed all 207 tests, including 54 Dashboard HTTP scenarios. The runner initially failed under the sandbox's IPC socket restrictions and passed with escalated execution. CSharpier and `git diff --check` passed. No browser check was performed. Changes remain uncommitted.

Next: review the data source contract for unused operations before implementing custom create. Custom model and handler pairing is agreed, but interface composition and internal adapter details remain proposals.

## Step 6b–d: split contracts and custom create — 2026-09-20

Implemented the reviewed interface composition: IResourceDataSource supplies ListAsync, IResourceCreateHandler supplies CreateAsync, IResourceEditHandler supplies FindAsync and UpdateAsync, and IResourceDeleteHandler supplies DeleteAsync. IResourceCrudDataSource combines them for ordinary CRUD. Migrated active sample and fixture sources. Detached EF/Identity projects remain untouched.

Added Create<TModel, THandler>() and its callback overload. The model and compatible handler are required together. The shared controller binds the selected model, applies its data annotations, maps operation-result errors, and retains antiforgery and view-override behavior. A typed delegate resolves the handler from request services and dispatches the model. No separate controller or general dispatch framework was added. Explicit handlers take precedence over source capabilities regardless of data source registration order. Concrete handler registrations preserve application-selected lifetimes and otherwise default to scoped.

The initial implementation below was superseded by the explicit action registration follow-up on 2026-09-21. Create options have a common form base and a typed factory implementation. Repeated configuration for one model composes. Selecting a different model clears fields and factory while retaining page labels and section layout. Ordinary Create selects TResource and removes the explicit handler. Retained builders reject configuration if their model no longer matches the selected model. Unsupported actions are hidden on the index and reject direct requests. Final options validation catches configured create/edit/delete actions without the required implementation.

DashboardPlayground retains Product as ordinary CRUD and adds Customer with CreateCustomerModel and CreateCustomerHandler, while editing Customer through the data source. Password and confirmation demonstrate model-only fields and validation. They are not persisted and this sample does not create authentication accounts. Duplicate emails demonstrate handler-level field errors. CustomerDataSource implements listing, edit, and delete without an unused CreateAsync method.

Added HTTP scenarios covering typed fields/layouts, factory initialization without a parameterless constructor, repeated custom configuration, handler precedence, registration order, allowlisted binding, model and handler validation, password-free redisplay, ordinary editing with custom create, antiforgery, and unsupported actions. Added only three focused unit cases for invalid action configuration. Existing scenarios continue to cover ordinary CRUD and overrides.

Review checkpoint: custom create is ready for review. Custom edit models remain unimplemented until this approach is reviewed. Next is Create's reviewed counterpart, Edit<TModel, THandler>(), including typed loading and route-key protection.

Verification: Release solution build passed with one existing unresolved XML cref warning in ViewDataKeys. The final `dotnet test --solution StellarAdmin.slnx --configuration Release --minimum-expected-tests 1` passed all 221 tests, including 65 Dashboard HTTP scenarios. Tests required execution outside the sandbox for local IPC. CSharpier formatted changed C# and `git diff --check` passed. An isolated Chromium session verified desktop/mobile layout, confirmation validation, duplicate-email rejection, empty password values on redisplay, successful creation, and ordinary editing. Desktop and mobile screenshots were inspected. The agent-owned browser and playground on port 5206 were stopped. Changes remain uncommitted.


## Explicit action registration — 2026-09-21

Following review, create, edit, and delete now require explicit registration. Their options remain null until the corresponding builder method is called. Removed CreateConfigured, EditConfigured, DeleteConfigured, and SelectCreateModel. Each action registration creates fresh options when the options pipeline executes; repeated registrations discard earlier action configuration, including labels, layout, fields, factory, and custom create handler. Setters on the returned action builder still compose. Added no-callback Edit() and Delete() overloads, matching Create(). No model-switch preservation remains.

Controller UI and request guards require the action's options. Handler implementation alone no longer exposes write actions. Existing options validation still rejects registered actions without a matching implementation. Playground resources explicitly register deletion. Updated scenarios to configure a form through one action builder, and extended HTTP coverage to verify unregistered actions with both read-only and full CRUD sources, as well as repeated ordinary/custom create registration replacing the prior form. Custom edit remains deferred pending review.

Verification: Release solution build passed with the existing unresolved XML cref warning in ViewDataKeys. The final `dotnet test --solution StellarAdmin.slnx --configuration Release --minimum-expected-tests 1` passed all 227 tests, including 71 Dashboard HTTP scenarios. The first test run identified two view-override fixtures relying on implicitly enabled actions; both now explicitly register their forms. Tests required local IPC access outside the sandbox. CSharpier and `git diff --check` passed. No new browser check was performed for this configuration follow-up. Changes remain uncommitted.
