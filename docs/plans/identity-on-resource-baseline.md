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

## Extended user and department example — 2026-09-27

`ApplicationUser` now supplies required first and last names, optional preferred language and time zone strings, and an optional department reference. `Department` has a GUID key and required name. `ApplicationDbContext` configures the relationship and property lengths, and the CLI-generated `AddApplicationUserAndDepartments` migration adds the columns and table. Existing users receive empty first and last names when migrated because the columns are required. The playground registers `ApplicationUser` with Identity, exposes Departments as an EF Core resource, and handles the new user properties through create/edit form models and `UserManager`. A `DepartmentLookupProvider` supplies the reference editor on both user forms. The application uses plain text inputs for language and time zone.

Verification: `dotnet build sandbox/DashboardPlayground/DashboardPlayground.csproj --no-restore -m:1 -v quiet` passed with no warnings or errors. The migration applied to a temporary copy of `app.db`, leaving the tracked database unchanged. On a local playground bound to port 5218 with that copy, Department index and create returned HTTP 200, Department create redirected and persisted a generated GUID, user create rendered the new fields and department selector, user create persisted the names, department, language, and time zone, user edit rendered the selected department and persisted profile changes including clearing the department, and the user index rendered a department name alongside an existing user with no department. No browser visual review or full solution test run was performed.

The tracked `app.db` subsequently had the migration applied and was populated with 15 departments through a Chromium-driven Department create form on a separate local playground instance on port 5219. All 15 form submissions redirected to the Department index. A read-only database check found 15 unique department names and `PRAGMA integrity_check` returned `ok`. The temporary browser and playground instance were stopped without touching port 5205.

## Verification

The playground built with `dotnet build sandbox/DashboardPlayground/DashboardPlayground.csproj --no-restore -m:1 -v quiet` with zero warnings on 2026-09-24 after the authorization additions were removed. CSharpier formatted the new C# files and `git diff --check` passed. The initial parallel build/restore commands exited without diagnostics in this environment, so the single-node build was used.

The earlier version was exercised against a temporary SQLite database on localhost. Its user and role create/edit/delete flows, user password validation, missing password fields on user edit, and sign-in after changing a user's email passed. The authorization and administrator-specific checks from that run applied to code subsequently removed at the user's request. After removal, a final localhost check against a temporary copy of `app.db` returned HTTP 200 without sign-in for both resource index pages and both create pages. No browser layout review or full solution test run was performed for this sample-only change.

After removing the unsolicited sidebar provider and layout links, `dotnet build sandbox/DashboardPlayground/DashboardPlayground.csproj --no-restore -m:1 -v quiet` passed with zero warnings and errors. `rg` found no remaining sidebar provider references in the playground, and `git diff --check` passed. The user's existing `app.db` and password option edits were left untouched.
