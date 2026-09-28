# Dashboard authorization

Status: completed on 2026-09-28. Phase 1 was committed as `49a70c2`, phase 2 as `beb45b7`, and phase 3 with this closeout. Jerrie reviewed each phase and considers the feature done. Per-action requirements remain deferred and need separately scoped work.

## Decisions

Agreed with Jerrie on 2026-09-28:

- The API is `RequireAuthorization`, matching the framework's endpoint extension and the `Require*` precedent in the [options builder conventions](../../conventions/options-builders.md).
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

Implemented and reviewed. Jerrie approved making the public `ISidebarItemsProvider` asynchronous on 2026-09-28. It now declares `Task<SidebarItem[]> GetItemsAsync(HttpContext httpContext)`, which is a breaking change for custom providers. `SidebarViewComponent` awaits each provider in registration order.

`ResourceSidebarItemsProvider` hides a resource link when the current user fails that resource's requirements. `AuthorizationMetadata.AuthorizeAsync` combines the metadata with `AuthorizationPolicy.CombineAsync` and evaluates it with `IAuthorizationService` against `HttpContext.User`, passing the `HttpContext` as the resource like the middleware does. Dashboard-level requirements are not re-evaluated because the user has already passed them to see the page. Policies that name authentication schemes are evaluated against the current user rather than re-authenticated, because authenticating would replace the rendering page's user.

Tests: `ResourceAuthorizationTests.RequiredAuthorization_SidebarForUser_ShowsOnlyAuthorizedResources` covers users with and without the resource's role. Dashboard unit tests call the async provider API.

Verification run on 2026-09-28: `dotnet build StellarAdmin.slnx --configuration Release` succeeded. `dotnet test --solution StellarAdmin.slnx --no-build --configuration Release --minimum-expected-tests 1` passed 423 of 423 tests. No browser or DashboardPlayground checks were run.

## Phase 3: convention builder and playground

Implemented and reviewed. Both `MapStellarAdmin` overloads return the route's `ControllerActionEndpointConventionBuilder` instead of `void`, so a host can add framework conventions such as `app.MapStellarAdmin().RequireAuthorization()`. The overloads and their class now have XML documentation. `DashboardTestHost` accepts a route configuration callback, and `DashboardAuthorizationTests.RouteConvention_AnonymousRequest_ReturnsUnauthorized` covers the returned builder.

Jerrie added the DashboardPlayground demo himself, committed in `beb45b7`. The whole Dashboard requires an authenticated user, and the Users and Roles resources require the `Administrator` or `User Admin` role.

Verification run on 2026-09-28: `dotnet build StellarAdmin.slnx --configuration Release` succeeded. `dotnet test --solution StellarAdmin.slnx --no-build --configuration Release --minimum-expected-tests 1` passed 424 of 424 tests. No browser checks were run.

## Documentation

Consumer guidance is in the Authorization section of `skills/stellar-admin-dashboard/references/setup.md`, which also replaces a stale note that automatic sidebar entries were deferred. `docs/repos/stellar-admin.md` and `docs/development.md` summarize the feature and its tests. The separate website repository was not changed.

## Deferred

Per-action requirements, such as a policy only for delete, remain deferred. They also need the index and form views to hide actions the user cannot perform.
