# Dashboard authorization

Status: active. Phase 1 implemented and awaiting review on 2026-09-28. Phases 2 and 3 are agreed but not started.

## Decisions

Agreed with Jerrie on 2026-09-28:

- The API is `RequireAuthorization`, matching the framework's endpoint extension and the `Require*` precedent in the [options builder conventions](../conventions/options-builders.md).
- It is available on `StellarAdminDashboardBuilder` for every Dashboard page and on `ResourceBuilderBase<TResource, TBuilder>` for one resource. EF Core resources inherit it.
- Overloads mirror the framework: none (authenticated user), `AuthorizationPolicy`, `Action<AuthorizationPolicyBuilder>`, `params IAuthorizeData[]`, and `params string[]` policy names.
- Requirements accumulate. Dashboard and resource requirements, and repeated calls, must all succeed.
- The Dashboard is not secure by default. Authorization remains opt-in.
- Per-action rules, such as requiring a policy only for delete, are deferred.

## Phase 1: builder API and endpoint metadata

Implemented. Requirements are stored as the same endpoint metadata the framework's `RequireAuthorization` adds. `MapStellarAdmin` adds an endpoint convention that applies dashboard requirements to every StellarAdmin endpoint and resource requirements to that resource's `ResourceController<TResource>` endpoints. The host's authorization middleware enforces them before MVC runs.

Changed source:

- `src/StellarAdmin.Dashboard/StellarAdminDashboardBuilder.cs` and `Resources/Builders/ResourceBuilderBase.cs` add the overloads.
- `Infrastructure/Authorization/AuthorizationMetadata.cs` translates overloads into metadata.
- `StellarAdminDashboardOptions` stores dashboard requirements. The internal `ResourceAuthorizationOptions<TResource>` stores resource requirements separately, so building endpoints does not resolve and validate `ResourceOptions<TResource>` at startup.
- `Sidebar/ResourceSidebarRegistration.cs` became `Resources/ResourceRegistration.cs`, keyed by controller type, with resolvers for the sidebar item and authorization metadata. `ResourceSidebarItem` moved to its own file.
- `StellarAdminEndpointRouteBuilderExtensions.cs` adds the authorization convention.

Tests: `tests/StellarAdmin.Dashboard.IntegrationTests/Authorization/` covers anonymous 401 responses for pages and form posts, authenticated access, named policies, configured policies, `IAuthorizeData` roles, resource-only protection, combined dashboard and resource requirements, and repeated resource requirements. `Infrastructure/TestAuthenticationHandler.cs` authenticates from test headers, and `DashboardTestHost` accepts extra service configuration.

Verification run on 2026-09-28: `dotnet build StellarAdmin.slnx --configuration Release` succeeded. `dotnet test --solution StellarAdmin.slnx --no-build --configuration Release --minimum-expected-tests 1` passed 421 of 421 tests. No browser or DashboardPlayground checks were run.

## Phase 2: authorization-aware sidebar

Not started. Hide resource sidebar links the current user cannot open, evaluating the combined policy with `AuthorizationPolicy.CombineAsync` and `IAuthorizationService`. `ISidebarItemsProvider` is public and synchronous, so making it async and user-aware is a public API change that needs a decision.

## Phase 3: convention builder and playground

Not started. Return the route's convention builder from `MapStellarAdmin` instead of `void`, and demonstrate resource authorization in `sandbox/DashboardPlayground`.
