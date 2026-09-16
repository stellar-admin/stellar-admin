# EF resource integration checks

Run from the workspace root:

```bash
dotnet build stellar-admin/tests/StellarAdmin.Dashboard.EntityFrameworkCore.Tests -m:1
dotnet run --project stellar-admin/tests/StellarAdmin.Dashboard.EntityFrameworkCore.Tests --no-build
```

This executable suite starts IdentitySimplePlayground through WebApplicationFactory, migrates a temporary SQLite database, and checks resource HTTP behavior and persisted data. It exits unsuccessfully on any failed assertion and never uses the playground's app.db. Extra resources are registered only inside the test host to exercise independent configuration, read-only binding, and authorization.
