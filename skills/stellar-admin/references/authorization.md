# Authorization

The admin panel is open to everyone until you require authorization. Requirements can be set on the whole panel, on a resource and on a sidebar link, and they accumulate: a user must satisfy every requirement that applies.

```csharp
builder.Services.AddStellarAdmin(admin =>
{
    admin.RequireAuthorization();                                    // every panel page: signed-in users

    admin.AddEfCoreResource<Customer>(resource => { /* any signed-in user */ });

    admin.AddEfCoreResource<Booking>(resource =>
    {
        resource.RequireAuthorization(policy => policy.RequireRole("Sales"));   // and the Sales role
    });

    admin.AddSidebarLink("Departures this week", SidebarLinkTarget.Page("/Admin/Departures"), link =>
        link.RequireAuthorization(policy => policy.RequireRole("Sales")));      // hides the link only
});
```

## Where requirements apply

| Call | Protects | Sidebar |
| --- | --- | --- |
| `admin.RequireAuthorization(...)` | Every admin panel route | — |
| `app.MapStellarAdmin().RequireAuthorization(...)` | Every admin panel route (endpoint convention) | — |
| `resource.RequireAuthorization(...)` | All of the resource's pages and posts: index, create, edit, view, delete, and its lookup create sheet | Hides the resource's item |
| `link.RequireAuthorization(...)` | **Nothing**: the destination is outside the panel | Hides the link |
| `[Authorize]` on a host page or controller | That page | — |

All `RequireAuthorization` methods take the same arguments as endpoint `RequireAuthorization`: none (any authenticated user), policy names (`"Sales"`), an `AuthorizationPolicy`, `IAuthorizeData` (`new AuthorizeAttribute { Roles = "Admin" }`) or a policy-builder callback (`policy => policy.RequireRole("Sales")`). A policy name must be registered with `AddAuthorization(options => options.AddPolicy(...))`; a role check needs no registered policy when written as a callback.

## Requirements

- Requirements are endpoint metadata applied by `MapStellarAdmin`, so the app needs authorization middleware (`UseAuthorization`, after `UseRouting`). `WebApplication` adds the authentication middleware itself when authentication is registered; only an app that orders its middleware explicitly, for example with `UseCors`, calls `UseAuthentication` before `UseAuthorization`.
- Anonymous users get the authentication challenge; signed-in users who fail get the forbidden response. With cookie authentication both are redirects, to `LoginPath` and `AccessDeniedPath` (`/Account/Login`, `/Account/AccessDenied` by default); make sure those pages exist.
- `resource.SidebarItem(item => item.Visible = false)` hides an item but leaves its pages reachable.
- There are no per-action rules (for example, everyone can view but only managers can delete). To restrict an action, leave it disabled for that resource, or enforce the rule in a custom handler or data source and return `ValidationFailed`.
- Scopes are not access control. Restrict which records a user can see with EF global query filters or a custom data source.

## Signing out

The sidebar's user menu shows the signed-in user from their claims and ends with a log out item, which posts to the panel's logout endpoint (`/stellaradmin/logout`, protected by an antiforgery token and the panel's own requirements). Without configured schemes it signs out of the default authenticate scheme. An app that signs in through several schemes lists them in sign-out order, local cookies first and OpenID Connect last; see [log out](sidebar.md#log-out) in the sidebar reference, which also covers the claims the menu reads.

## What correct protection looks like

A user without the role is refused `/stellaradmin/{slug}` and the host page when requesting them directly (403, or a redirect to the access-denied path with cookie authentication), and the sidebar doesn't show them. A user with the role gets 200.
