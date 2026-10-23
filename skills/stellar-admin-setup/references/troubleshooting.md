# Troubleshooting the admin panel installation

Match the symptom from a real request (not just a successful build) to its cause.

| Symptom | Cause |
| --- | --- |
| 404 at `/stellaradmin` | `app.MapStellarAdmin()` missing, or a different path was passed to it. |
| Unstyled page | Static assets not mapped (`MapStaticAssets` or `UseStaticFiles`). |
| Literal `<sa-index-page>` or other `sa-` tags | An app view under `Areas/StellarAdmin/Views/` has no `_ViewImports.cshtml` registering the tag helpers. |
| Every page is a 500 | Invalid resource configuration; read the exception message in the log. |
| Anonymous users see the panel | No `RequireAuthorization`, or authentication and authorization middleware missing. |
| Foreign key shows a number box | Register the related type as a resource; an EF Core entity form then gives the field a lookup sheet. A form model's field needs an editor with `UseResource<T>()`, and a related type that isn't a resource needs `UseItems`. |
| `/stellaradmin/{slug}` 404, and the log says no registered DbContext maps the entity, or more than one does | Register the context that maps it with `AddDbContext`, or pick one with `resource.UseDbContext<TContext>()`. |
| `/stellaradmin/{slug}` 404 for one resource | The slug differs from the guess: it is the kebab-case plural of the type name (`OrderItem` → `order-items`) unless one was passed to `AddEfCoreResource` or `AddResource`. |
| Every page is a 500 mentioning page sizes | `paging.PageSize` isn't one of `paging.PageSizes` (default 10, 25, 50, 100). |
| Signed-in users without access land on a 404 | Cookie authentication redirects them to `/Account/AccessDenied`; add that page or set `AccessDeniedPath`. |
| A sidebar or user menu link shows red with "No page …" or "No action …" (Development), or is missing in production with a warning in the log | The `Page` or `Action` target doesn't exist: check the page name, the controller and action, and the area (targets resolve outside any area unless one is given). See the stellar-admin skill's sidebar reference. |
| Create or Edit button missing | `AllowCreate` or `AllowEdit` not called for that resource. |
| Date fields show 0001-01-01 on create | Set defaults in `create.UseFactory(() => new Booking { ... })`. |

## Reading a 500

Open the app's console log. Options validation failures name the resource type and the setting, for example a view page without fields or a default sort on a column that isn't sortable. Fix the configuration and request the page again; there is no separate startup check.

## Checking without a browser

```bash
curl -s -o /dev/null -w '%{http_code}\n' http://localhost:5000/stellaradmin/bookings
curl -s http://localhost:5000/stellaradmin/bookings | grep -c '<sa-'   # should print 0
```

With authentication, sign in first and pass the cookie (`-b cookies.txt`). An unauthenticated request to a protected panel returns a redirect to the sign-in page (302) or 401, depending on the scheme.
