# Dashboard playground Identity resources

The Users and Roles pages are ordinary Dashboard resources registered in `Program.cs`. `AddRoles<IdentityRole>()` enables `RoleManager<IdentityRole>` in the playground's existing ASP.NET Core Identity setup. The resource data sources handle listing, search, sorting, paging, and deletion. Create and edit handlers use `UserManager<IdentityUser>` and `RoleManager<IdentityRole>` for writes.

## Run the example

From this directory, run:

```bash
dotnet run
```

Open `/stellaradmin/IdentityUser` and `/stellaradmin/IdentityRole`. The existing `app.db` has the Identity schema and stores changes made through these pages. The example adds no Dashboard authorization policy.

Users and roles have create, edit, and delete examples. The playground's stock login signs in by email, so the user handlers keep `UserName` equal to `Email`. User creation has password and confirmation fields; user edit has no password field. New users are unconfirmed unless **Email confirmed** is checked. This example does not manage role membership or send confirmation email.
