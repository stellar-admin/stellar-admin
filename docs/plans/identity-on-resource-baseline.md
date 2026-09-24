# Identity example on the resource baseline

Status: DashboardPlayground example implemented, 2026-09-24. The separate Identity package is not part of this work.

## Decision

Use the ordinary Dashboard resource API to show how a host application can manage ASP.NET Core Identity users and roles. The example belongs in `sandbox/DashboardPlayground`, which already has an Identity database and login UI. This avoids maintaining a public Identity integration API for host-specific account and role membership policies.

The detached `StellarAdmin.Dashboard.Identity` and `IdentitySimplePlayground` remain historical material. Their controllers depend on removed resource types and are not a template for this example.

## Implementation

- Register `IdentityUser` and `IdentityRole` with `AddResource<T>()`, their own data sources, keys, columns, forms, and explicitly enabled create, edit, and delete actions. The routes use the existing type-based controller names, `/stellaradmin/IdentityUser` and `/stellaradmin/IdentityRole`.
- Use `UserManager` and `RoleManager` for writes and translate `IdentityResult` errors to `ResourceOperationResult`. Create and edit form models contain only the fields this example allows. User creation includes password and confirmation. User edit excludes password.
- Keep `IdentityUser.UserName` equal to `Email`, because the playground's stock Identity login signs in by email. This was discovered in an HTTP check: an independently configured user name made newly created accounts unable to sign in.
- Use queryable Identity stores for list, search, sort, count, and paging. Keep a key tie-breaker for stable pages.
- Add a playground README with setup and the scope of account confirmation and role membership. The example does not configure Dashboard authorization, seed accounts and roles, or add navigation items.

## Verification

The playground built with `dotnet build sandbox/DashboardPlayground/DashboardPlayground.csproj --no-restore -m:1 -v quiet` with zero warnings on 2026-09-24 after the authorization additions were removed. CSharpier formatted the new C# files and `git diff --check` passed. The initial parallel build/restore commands exited without diagnostics in this environment, so the single-node build was used.

The earlier version was exercised against a temporary SQLite database on localhost. Its user and role create/edit/delete flows, user password validation, missing password fields on user edit, and sign-in after changing a user's email passed. The authorization and administrator-specific checks from that run applied to code subsequently removed at the user's request. After removal, a final localhost check against a temporary copy of `app.db` returned HTTP 200 without sign-in for both resource index pages and both create pages. No browser layout review or full solution test run was performed for this sample-only change.

After removing the unsolicited sidebar provider and layout links, `dotnet build sandbox/DashboardPlayground/DashboardPlayground.csproj --no-restore -m:1 -v quiet` passed with zero warnings and errors. `rg` found no remaining sidebar provider references in the playground, and `git diff --check` passed. The user's existing `app.db` and password option edits were left untouched.
