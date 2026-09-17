# Dashboard setup notes

These preserved notes support the generated component references; full Dashboard guidance is still planned.

## Resource packages

The resource page helpers live in `StellarAdmin.Dashboard`, which is MIT licensed alongside the rest of StellarAdmin. These packages are currently available from the source repository; NuGet publication is a separate release step. Keep `StellarAdmin.TagHelpers` registered, reference the `StellarAdmin.Dashboard` project, add `using StellarAdmin.Dashboard;` in startup, and call `.AddDashboard()` on the `StellarAdminBuilder`. In `_ViewImports.cshtml`, add `@using StellarAdmin.Dashboard.TagHelpers` and `@addTagHelper *, StellarAdmin.Dashboard`.
