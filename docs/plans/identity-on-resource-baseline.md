# Identity example on the resource baseline

Status: DashboardPlayground example implemented on 2026-09-24. The superseded dedicated Identity package and its playground were removed on 2026-09-27.

## Decision

Use the ordinary Dashboard resource API to show how a host application can manage ASP.NET Core Identity users and roles. The example belongs in `sandbox/DashboardPlayground`, which already has an Identity database and login UI. This avoids maintaining a public Identity integration API for host-specific account and role membership policies.

The removed `StellarAdmin.Dashboard.Identity` and `IdentitySimplePlayground` were historical material. Their controllers depended on removed resource types and are not a template for this example.

## Dedicated package cleanup — 2026-09-27

Removed the dedicated Identity package and its dependent playground, including the tracked playground SQLite database, migrations, and vendored assets. Both projects had been excluded from the solution; a direct Identity package build failed with 10 compile errors against removed resource APIs. The DashboardPlayground user and role example remains the supported starting point. Current repository and build guidance was updated; the former package design documents remain with superseded notices. A serial Release solution build passed with 12 existing warnings and no errors. Direct TUnit runs passed all 393 tests across Core (66), TagHelpers (76), Dashboard unit (29), Dashboard HTTP (165), and EF Core HTTP (57). The solution-wide `dotnet test --no-build` command could not start because the sandbox denied its named-pipe socket; direct project runs succeeded. `git diff --check` passed. No browser or hosted release check was run for this removal.

## Implementation

- Register `IdentityUser` and `IdentityRole` with `AddResource<T>()`, their own data sources, keys, columns, forms, and explicitly enabled create, edit, and delete actions. The routes use the existing type-based controller names, `/stellaradmin/IdentityUser` and `/stellaradmin/IdentityRole`.
- Use `UserManager` and `RoleManager` for writes and translate `IdentityResult` errors to `ResourceOperationResult`. Create and edit form models contain only the fields this example allows. User creation includes password and confirmation. User edit excludes password.
- Keep `IdentityUser.UserName` equal to `Email`, because the playground's stock Identity login signs in by email. This was discovered in an HTTP check: an independently configured user name made newly created accounts unable to sign in.
- Use queryable Identity stores for list, search, sort, count, and paging. Keep a key tie-breaker for stable pages.
- Add a playground README with setup and the scope of account confirmation and role membership. The example does not configure Dashboard authorization or seed accounts and roles. Resource registration now adds sidebar items through the shared Dashboard provider, as recorded in [resource sidebar registration](resource-sidebar-registration.md).

## Verification

The playground built with `dotnet build sandbox/DashboardPlayground/DashboardPlayground.csproj --no-restore -m:1 -v quiet` with zero warnings on 2026-09-24 after the authorization additions were removed. CSharpier formatted the new C# files and `git diff --check` passed. The initial parallel build/restore commands exited without diagnostics in this environment, so the single-node build was used.

The earlier version was exercised against a temporary SQLite database on localhost. Its user and role create/edit/delete flows, user password validation, missing password fields on user edit, and sign-in after changing a user's email passed. The authorization and administrator-specific checks from that run applied to code subsequently removed at the user's request. After removal, a final localhost check against a temporary copy of `app.db` returned HTTP 200 without sign-in for both resource index pages and both create pages. No browser layout review or full solution test run was performed for this sample-only change.

After removing the unsolicited sidebar provider and layout links, `dotnet build sandbox/DashboardPlayground/DashboardPlayground.csproj --no-restore -m:1 -v quiet` passed with zero warnings and errors. `rg` found no remaining sidebar provider references in the playground, and `git diff --check` passed. The user's existing `app.db` and password option edits were left untouched.
