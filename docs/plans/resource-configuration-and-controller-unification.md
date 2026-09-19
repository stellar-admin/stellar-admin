# Resource configuration and controller unification

Status: phase 1 implementation reverted at the user’s request on 2026-09-19. The original plan below is retained for context and needs revision before further implementation. Controller unification has not started. Action-specific form models remain deferred.

## Objective

Make Identity a thin resource integration. Shared resource builders configure its screens, and a shared resource controller workflow handles ordinary CRUD. Identity supplies defaults and operations through UserManager and RoleManager. EF Core supplies its own operations through DbContext. Dashboard must not depend on either integration package.

Deliver configuration unification first, then controller unification. Action-specific form models are a recorded third phase and are explicitly deferred.

## Current baseline

- Identity already exposes ResourceBuilder<TEntity> through ConfigureUsers and ConfigureRoles. Create/edit field builders already support sections, groups, and rows.
- FormPageDefaults only seeds flat property names and holds title/submit-label fallbacks. CreatePageBuilder and EditPageBuilder expose Title, Subtitle, and SectionLayout, but no SubmitLabel.
- IndexPageDefaults similarly holds columns, sorting, and several strings that are not exposed by IndexPageBuilder. DeleteDefaults supplies values to read-only DeleteOptions, with no Delete builder entry point.
- FormPageOptions.Items holds the layout tree. Fields flattens it for binding and other field processing. Defaults and application configuration must continue producing the same options tree.
- ResourceControllerBase<TEntity> provides binding, view-model construction, and redirect helpers, but no CRUD actions. UsersController, RolesController, and EfCoreResourceController each implement their own actions.
- EF's controller also handles key metadata, reference loading and selection validation, query execution, persistence errors, and view fallback. Identity controllers handle manager operations, Identity error mapping, and the self-deletion restriction.
- User creation binds a separate CreateUserPasswordInput and renders it through the post-form-fields slot in Users/Create.cshtml. It cannot yet use the entity-only form pipeline in full.

Relevant code is under src/StellarAdmin.Dashboard/Resources/, src/StellarAdmin.Dashboard.EntityFrameworkCore/, and src/StellarAdmin.Dashboard.Identity/. The earlier extraction is documented in [identity resource layer](archive/identity-resource-layer.md). This plan takes ownership of the shared operations work overlapping item 2 in [generic resource follow-ups](generic-resources-follow-ups.md), without reopening its unrelated backlog.

## Design rules

1. Defaults run once when a resource's options are created, before application callbacks. Repeated registration or configuration must not reseed fields or overwrite application choices.
2. Library defaults and application callbacks use the same builders and the same options instance. Scalars override earlier values, Add appends, and Clear removes all entries in that collection, including seeded containers. Existing composition rules for query transforms remain intact.
3. A resource registration owns its configuration. Request-scoped services and mutable request data never live in singleton options. Operations resolve through DI per request and must remain isolated between registered resources, including resources using different DbContexts.
4. Shared controller code owns HTTP flow, binding, validation display, view selection, and redirects. Integration operations own querying, persistence, and domain rules. No Identity or EF types appear in shared contracts.
5. Identity writes continue through UserManager and RoleManager. Consolidating controllers must not substitute direct DbContext writes.
6. Preserve existing routes, registration entry points, configured-field allow-lists, read-only behavior, authorization, antiforgery, view overrides, and successful/failed request behavior unless a change is explicitly documented and agreed.
7. Retain entity-backed forms as the default. Do not implement the deferred form-model API through an untyped dictionary, object payload, or special password parameter in the shared contract.

## Phase 1 — Unify configuration

### 1.1 Complete the shared builder surface

- Add SubmitLabel to create/edit options and builders.
- Expose the existing index create label and empty-state title, description, and icon through the shared index builder.
- Add ResourceBuilder.Delete(...) and a shared delete builder for the existing confirmation settings and display-name behavior. Follow the options-builder convention for scalar properties and behavior methods.
- Store configured values in ordinary options. Preserve useful Effective* accessors as compatibility aliases where practical rather than renaming public members unnecessarily.
- Define null/empty semantics explicitly. Existing Title = null restores a library fallback, so removing the defaults record must not silently turn that into a blank title. A small scalar fallback mechanism is acceptable if needed for compatibility. It must not become a second layout configuration API.

### 1.2 Seed integrations through builders

- Replace the active FormPageDefaults initialization path with default configuration through shared builders. Do not introduce FormPageDefaults<TEntity>.
- Migrate IndexPageDefaults and DeleteDefaults seeding as part of the same configuration unification, so Identity has one way to supply its screen configuration.
- Keep Identity default configuration beside its integration registration/options. Use typed user/role selectors. Keep EF-specific default configuration inside the EF package.
- Audit public defaults records and options constructors before removal. Prefer forwarding compatibility shims where feasible, with explicit migration notes for any breaking change. All active library registration paths must use builders even if legacy entry points are retained temporarily.
- Preserve existing default labels, field order, columns, and sorting. Demonstrate a seeded section/row in a focused fixture or sample without redesigning the shipped Identity pages as a side effect.

### Acceptance

Applications can override submit labels and existing index/delete copy through shared builders. Library-seeded layouts can contain sections, groups, and rows and can be appended to or cleared normally. Tests establish initialization order, repeated configuration behavior, option isolation, scalar fallback semantics, and unchanged ordinary defaults. Identity and EF sample registrations remain usable.

## Phase 2 — Unify controller behavior

### 2.1 Define the operations contract and migration boundary

Write down concrete signatures and DI registration before migrating actions. Names remain an implementation design decision, but the contract must cover index execution, record lookup and key formatting, create/update/delete operations, and structured field/form errors. It must distinguish missing records, validation/domain failures, and success without returning MVC IActionResult from integration operations.

Keep provider-specific query execution in the integrations. EF retains asynchronous database execution and cancellation. Shared contracts must not require an EF-backed IQueryable or assume every provider supports EF async methods. If operation-level record constraints are exposed, they must apply consistently to index and direct record lookup. Existing index TransformQuery remains presentation/query configuration, not an authorization boundary.

Use scoped operations with constructor injection. If consumer replacement is exposed in this phase, it must allow overriding one operation while retaining or invoking the integration defaults. Do not force applications to reimplement CRUD. The earlier UseOperations<T>() sketch is not an agreed public name.

### 2.2 Extract and migrate EF operations

- Move EF key conversion/validation, querying, reference includes, persistence, and error translation out of the HTTP controller.
- Define a bounded form-preparation/validation extension for loading reference choices, validating submitted selections, and rebuilding choices on failed posts. Shared code coordinates it without knowing EF metadata.
- Preserve current reference behavior, including disabled assigned values, required/optional selections, label projection, sorting, query counts, and read-only protection.
- Move ordinary actions into the shared controller workflow. A thin closed controller or registration adapter may remain to preserve routing and integration-specific registration identity, but it must not duplicate the CRUD action bodies.
- Preserve resource-specific view lookup before the existing fallback and preserve per-resource authorization policies.

### 2.3 Migrate Identity operations

- Implement role operations through RoleManager and user operations through UserManager.
- Move IdentityResult translation into the integration, returning errors that the shared workflow attaches to the appropriate form field or validation summary.
- Preserve self-deletion prevention using the current request's principal through an explicit context/service boundary.
- Migrate role index/create/edit/delete and user index/edit/delete to the shared workflow. Keep existing routes and override locations even if controller implementation types change.
- Keep user creation's password input, validation, manager call, and Razor slot as a documented temporary specialized path. Reuse shared helpers where possible without inventing the deferred form-model API. This exception is removed in phase 3, not hidden behind a password-aware common controller.

### 2.4 Preserve and document observable differences

Identity currently treats deletion of an already missing record as a redirect, while EF returns NotFound. Preserve these outcomes through an explicit operation result or resource policy. Preserve error messages, validation redisplay, post-success redirects, and htmx return behavior. Extracting common code is not authorization to normalize these behaviors silently.

### Acceptance

EF and Identity roles execute ordinary CRUD through the shared action workflow. Identity users share index/edit/delete. Integrations contain persistence/domain behavior and configuration rather than duplicated HTTP action flow. User creation is the only explicitly retained Identity CRUD exception for its extra inputs. Routes, view customization, authorization, binding protection, and integration-specific behavior pass regression coverage.

## Phase 3 — Deferred: action-specific form models

The user wants to select a form model for an individual CRUD action, with TEntity remaining the default when no model is selected. Create and edit may use different types. All field expressions and layout builders for that action must target the selected model. Password and password confirmation become ordinary typed fields that can be placed anywhere in the form layout.

The eventual API needs typed loading/initialization and submission operations. Selecting a type alone does not define entity mapping. The shared workflow must bind configured writable fields, validate the model, preserve errors on redisplay, and submit the valid model to the action operation. Edit identifies the resource independently through the route, rather than trusting a posted identifier. Passwords require appropriate redisplay handling and must never be copied into persisted entity properties by generic mapping.

Keep the action's form model distinct from ResourceFormPageViewModel, which carries page presentation. Decide how changing the form-model type replaces incompatible seeded field expressions while preserving applicable page settings. A sketch such as Create<TForm>(...) is illustrative, not a committed signature.

Do not implement this phase during phases 1 or 2. Phase 2's contract design should leave room for it without building a speculative general mapping framework.

## Verification and delivery

- Add focused TUnit tests under the owning projects following [unit testing conventions](../conventions/unit-testing.md). Use Dashboard unit tests for shared builder/options behavior and shared workflow behavior that can be tested independently.
- Extend Dashboard and EF integration coverage using the existing isolated host infrastructure. Include Identity user/role create/edit/delete behavior, mapped errors, password validation, self-deletion rejection, configured-field binding, view overrides, authorization/antiforgery, multiple resource registrations, and EF reference regressions. Keep integration-specific dependencies out of the Dashboard unit test project.
- Run affected project builds and focused test projects during each phase. At completion run `dotnet test --solution StellarAdmin.slnx --configuration Release --minimum-expected-tests 1` with the pinned SDK.
- Exercise representative Identity and EF screens in the playground. Verify the configured section/row example at desktop/mobile widths if markup or shipped layouts change. Use an agent-owned process and leave port 5205 alone.
- Update maintained resource/Identity guidance and affected consumer references. Regenerate component references only if their source/API changes require it. Website work is not part of this plan.
- Update this record with actual commands, outcomes, compatibility decisions, and outstanding work. Do not treat historical test results as new verification.

## Explicit exclusions

No action-specific form-model implementation, targeted editing of existing sections by identifier, new Identity workflows, details pages, sidebar unification, new reference editors/sources, concurrency-token round-tripping, or unrelated backlog cleanup. No commit, push, publish, or deployment is authorized by this plan.

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
