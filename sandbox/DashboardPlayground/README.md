# Dashboard playground Identity resources

The Users, Roles, and Departments pages are ordinary Dashboard resources registered in `Program.cs`. `ApplicationUser` extends Identity with required first and last names, an optional department, preferred language, and time zone. `AddRoles<IdentityRole>()` enables `RoleManager<IdentityRole>` in the playground's existing ASP.NET Core Identity setup. The user and role data sources handle listing, search, sorting, paging, and deletion. User and role create/edit handlers use `UserManager<ApplicationUser>` and `RoleManager<IdentityRole>` for writes. The Department resource uses the EF Core data source.

## Run the example

From this directory, run:

```bash
dotnet ef database update
dotnet run
```

Open `/stellaradmin/ApplicationUser`, `/stellaradmin/IdentityRole`, and `/stellaradmin/Department`. The migration adds the new user properties and Departments table to the existing `app.db`. Existing accounts receive empty first and last names and can be completed through the user edit form. The example adds no Dashboard authorization policy.

Resource registration also adds sidebar links. Users, Roles, and Departments appear in the Identity group. Products and Customers appear in the Commerce group, with Products first. Their labels default to each resource's plural label unless `SidebarItem` overrides them.

Users and roles have create, edit, and delete examples. Departments have create and edit examples. User create/edit forms use a department lookup and plain text fields for preferred language and time zone. The playground's stock login signs in by email, so the user handlers keep `UserName` equal to `Email`. User creation has password and confirmation fields; user edit has no password field. New users are unconfirmed unless **Email confirmed** is checked. The stock Identity registration page does not collect the required name fields; create users through the Dashboard resource. This example does not manage role membership or send confirmation email.
