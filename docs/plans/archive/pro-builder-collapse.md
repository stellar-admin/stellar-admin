# Collapse the pro DI surface into `AddPro()`

**Index status:** completed. **Indexed:** 2026-09-05. Historical record; completion is recorded in the phase/progress notes below. Deferred items require a separately scoped task.

Status: **COMPLETE 2026-08-16** — all three phases executed (Option B decided
2026-08-16).

The follow-up deferred from the [project reorganization](project-reorganization.md):
retire the shell as a public concept. `StellarAdmin.Pro` gets one entry point,
`AddPro()`, which owns the shell layout, the pro stylesheet/script registration, and
the application-part registration. A new `StellarAdminProBuilder` lets other pro
packages (today `StellarAdmin.Pro.Identity`) register their application parts and
controllers through it instead of each carrying its own MVC plumbing. `AddShell()`,
`AddResources()`, and ShellPlayground go away.

## Current state (what collapses)

| Today | Job | Fate |
|---|---|---|
| `AddShell()` (`StellarAdmin.Pro.Shell`) | Registers the Pro application part, calls `AddUI()`, creates `StellarAdminShellOptions` | Folded into `AddPro()` |
| `AddShell(Action<StellarAdminShellBuilder>)` | Scripts/stylesheets configuration | Folded into `AddPro(Action<StellarAdminProBuilder>)` |
| `AddResources()` (`StellarAdmin.Pro.Resources`) | Registers the same part again, adds htmx + `stellar-admin-pro.css` via `AddShell(...)` | Deleted — everything it does becomes unconditional in `AddPro()` |
| `StellarAdminShellBuilder` / `StellarAdminShellOptions` | Scripts/stylesheets list consumed by `_Layout.cshtml` | Renamed `StellarAdminProBuilder` / `StellarAdminProOptions` |
| Identity's `IdentityControllerFeatureProvider` + `IdentityControllerNameConvention` + its own `ConfigureApplicationPartManager` block | Register Identity's part and its closed generic controllers with route-friendly names | Replaced by `StellarAdminProBuilder.AddApplicationPart(...)` / `.AddController(...)` |
| `MapStellarAdmin` (`StellarAdmin.Pro.Shell`) | Endpoint mapping | Unchanged, namespace becomes `StellarAdmin.Pro` |
| ShellPlayground | Shell-only sandbox | Retired (its custom-`ISidebarItemsProvider` sample coverage disappears; IdentitySimplePlayground still renders the shell end-to-end) |

The `NullApplicationPartFactory` opt-out in both RCLs is unchanged — nothing activates
on mere package reference; `AddPro()` is now the single activation point.

## Target public surface

All in namespace `StellarAdmin.Pro` (project-root files; the `Shell/` folder dissolves,
`Shell/Sidebar/` moves to `Sidebar/` → namespace `StellarAdmin.Pro.Sidebar`).

```csharp
public static class StellarAdminBuilderExtensions
{
    extension(StellarAdminBuilder builder)
    {
        // Idempotent. Registers the StellarAdmin.Pro application part (AssemblyPart +
        // CompiledRazorAssemblyPart), calls AddUI(), and creates the StellarAdminProOptions
        // instance singleton seeded with the package's own assets:
        //   ~/_content/StellarAdmin.Pro/htmx.min.js
        //   ~/_content/StellarAdmin.Pro/stellar-admin-pro.css
        public StellarAdminProBuilder AddPro();

        // Convenience overload, same shape as AddShell/AddIdentity today.
        public StellarAdminBuilder AddPro(Action<StellarAdminProBuilder> configuration);
    }
}

public class StellarAdminProBuilder
{
    // From StellarAdminShellBuilder, unchanged semantics (dedupe by path).
    public StellarAdminProBuilder AddScript(string path);
    public StellarAdminProBuilder AddStylesheet(string path);

    // For pro packages. Registers the assembly's AssemblyPart + CompiledRazorAssemblyPart
    // with MVC unless already present (the packages opt out of automatic discovery).
    // Mirrors IMvcBuilder.AddApplicationPart in name.
    public StellarAdminProBuilder AddApplicationPart(Assembly assembly);

    // For pro packages with generic controllers: MVC skips open generics and mangles
    // closed generic names (UsersController`1). Registers the closed type via a shared
    // feature provider and assigns its route/view name via a shared convention.
    public StellarAdminProBuilder AddController(Type controllerType, string controllerName);
}

public class StellarAdminProOptions   // rename of StellarAdminShellOptions
{
    public IList<string> Scripts { get; }
    public IList<string> Stylesheets { get; }
}
```

Notes:

- `StellarAdminProBuilder` needs `IServiceCollection` (for part/controller registration),
  so unlike the shell builder it wraps both the services and the options.
- The htmx/css seeding is unconditional. For data-grid-only hosts (DocsSamples,
  DataGridSpike) the entries are inert — only the shell `_Layout.cshtml` reads them.
  One code path, no "resources mode" flag.
- `AddController` subsumes both of Identity's `Infrastructure/Mvc` classes: a single
  internal feature provider + name convention pair lives in
  `StellarAdmin.Pro/Infrastructure/Mvc/`, and Identity deletes its copies. The name is
  a required argument per the options-builder conventions (essential data on the verb).
- `GetOrCreateOptions` instance-singleton pattern carries over unchanged, so repeat
  `AddPro()` calls (consumer + Identity internally) configure the same instance.
- `_Layout.cshtml` injects `StellarAdminProOptions`; `_ViewImports.cshtml` drops
  `@using StellarAdmin.Pro.Shell`.

## Decision: where does `AddIdentity` attach?

**Decided (Jerrie, 2026-08-16): Option B** — `AddIdentity` moves to
`StellarAdminProBuilder`. Rationale: Pro will potentially grow other specialized
packages, and they should all hang off the pro builder the same way — the
`StellarAdmin.Pro.*` package hierarchy is visible in the registration code.
(Option A — keeping `AddIdentity` on `StellarAdminBuilder` with an internal
`AddPro()` call — was rejected: it hides the tiering and gives future specialized
packages no consistent home.)

Consumer code in every Identity host changes to:

```csharp
// Program.cs (IdentitySimplePlayground)
builder.Services.AddStellarAdmin().AddPro(pro =>
{
    pro.AddIdentity<ApplicationUser, ApplicationRole>(identity =>
    {
        identity.ConfigureUsers(users => users.Index(index => { /* ... */ }));
    });
});
app.MapStellarAdmin("/admin");

// Chained form (no lambda) works too:
builder.Services.AddStellarAdmin().AddPro().AddIdentity<ApplicationUser, ApplicationRole>();
```

Library side — Identity's extensions retarget `StellarAdminProBuilder`, and the
configure overloads return it for chaining (parallel to today's overloads returning
`StellarAdminBuilder`):

```csharp
public static class StellarAdminProBuilderExtensions
{
    extension(StellarAdminProBuilder pro)
    {
        public StellarAdminIdentityBuilder<TUser, TRole, string> AddIdentity<TUser, TRole>() ...
        public StellarAdminProBuilder AddIdentity<TUser, TRole>(
            Action<StellarAdminIdentityBuilder<TUser, TRole, string>> configuration) ...
        // + the TKey overload pair, as today
    }
}

private static (...) AddIdentityCore<TUser, TKey>(StellarAdminProBuilder pro)
{
    // replaces builder.AddResources() + the ConfigureApplicationPartManager plumbing:
    pro.AddApplicationPart(typeof(StellarAdminProBuilderExtensions).Assembly);
    pro.AddController(
        typeof(UsersController<,>).MakeGenericType(typeof(TUser), typeof(TKey)),
        "Users");
    // ... rest unchanged (sidebar provider, options, resource registrations)
}
```

Convention consequence (update `docs/conventions/options-builders.md` in Phase 3):
`Add*` on `StellarAdminBuilder` registers a top-level product (`AddUI`, `AddPro`);
specialized packages of a product register with `Add*` on that product's builder
(`pro.AddIdentity<...>`). The verb table's "`Add*` (on `StellarAdminBuilder`)" row
generalizes to "`Add*` (on a product-entry builder)".

## Phases

Each phase ends with the solution building (from the workspace root) and the smoke
tests below; stop for review between phases.

### Phase 1 — new surface alongside the old — DONE 2026-08-16

Execution notes:
- `AddPro()` preserves `AddShell()`'s `Services.AddMvc()` call (the builder methods
  themselves use `AddMvcCore`), so hosts that only call `AddRazorPages` keep working.
- The controller plumbing landed as the single internal
  `StellarAdminControllerFeatureProvider` (feature provider + name convention in one,
  hooked into `MvcOptions.Conventions` once on first creation).
- `StellarAdminShellBuilder` now forwards to `StellarAdminProBuilder` (its ctor takes
  the pro builder — acceptable break, the type dies in Phase 2).
- Behavior note: ShellPlayground's shell now links htmx + `stellar-admin-pro.css`
  (AddPro seeds unconditionally; `AddShell()` never did). Harmless; project retires
  in Phase 3.
- Verified: solution build clean; ShellPlayground `/admin` 200, DocsSamples
  `/DataGrid` 200 with 0 unprocessed tags, IdentitySimplePlayground `/admin/Users` +
  `/admin/Roles` 200, both `_content/StellarAdmin.Pro` assets 200.

In `StellarAdmin.Pro`:
- Add root-level `StellarAdminBuilderExtensions.cs` (`AddPro` + overload),
  `StellarAdminProBuilder.cs`; rename `StellarAdminShellOptions` →
  `StellarAdminProOptions` (root file, ns `StellarAdmin.Pro`).
- Add `Infrastructure/Mvc/` controller registration plumbing (generalized from
  Identity's, internal) — one class can serve as both the controller feature
  provider and the name convention, since both read the same registrations.
- Rewrite `AddShell()`/`AddShell(...)`/`AddResources()` as thin delegators to
  `AddPro()` so everything still compiles.
- `_Layout.cshtml` injects `StellarAdminProOptions`.

### Phase 2 — migrate callers, delete the old surface — DONE 2026-08-16

Execution notes:
- `StellarAdminProBuilder` gained a public `Services` property
  (`[EditorBrowsable(Never)]`, mirroring `StellarAdminBuilder`) — Identity's
  registrations need the services collection.
- Identity's extensions file renamed `StellarAdminProBuilderExtensions.cs` (git mv);
  `AddIdentityCore` collapsed to `AddApplicationPart(...).AddController(closed,
  "Users"/"Roles")`; both `Infrastructure/Mvc` classes deleted.
- Verified: grep-clean of `Pro.Shell`/`AddShell`/`AddResources`; solution builds;
  ShellPlayground `/admin`, DocsSamples `/DataGrid`, DataGridSpike `/admin/`,
  IdentitySimplePlayground Users/Roles + `_content` assets all 200, 0 unprocessed
  data-grid tags.

- Identity: retarget the `AddIdentity` extensions to `StellarAdminProBuilder` and
  route the MVC wiring through it; delete `IdentityControllerFeatureProvider`,
  `IdentityControllerNameConvention`, and the
  `ConfigureApplicationPartManager`/`AddMvcOptions` plumbing they required.
- Consumers: DocsSamples, DataGridSpike → `AddPro()`; IdentitySimplePlayground →
  `AddPro(pro => pro.AddIdentity<...>(...))`, and `using StellarAdmin.Pro.Shell` →
  `StellarAdmin.Pro` for `MapStellarAdmin`.
- Delete `Shell/StellarAdminBuilderExtensions.cs` (AddShell),
  `Resources/StellarAdminBuilderExtensions.cs` (AddResources),
  `StellarAdminShellBuilder`.
- Dissolve `Shell/`: `StellarAdminEndpointRouteBuilderExtensions.cs` → project root
  (ns `StellarAdmin.Pro`); `Shell/Sidebar/` → `Sidebar/` (ns
  `StellarAdmin.Pro.Sidebar`) with the using updates in Pro views/view components,
  Identity, and sandboxes.
- Update the `AssemblyInfo.cs` comments in both RCLs (they name `AddShell()`).

### Phase 3 — retire ShellPlayground + sweep — DONE 2026-08-16

Execution notes:
- ShellPlayground deleted; workspace `StellarAdmin.slnx` updated (the pro repo has
  no slnx of its own).
- Live docs were already clean of `AddShell`/`AddResources` mentions; the two design
  docs' Target-usage examples and the conventions doc (intro, verb table, canonical
  example) now show the `AddPro(pro => pro.AddIdentity<...>(...))` shape, plus a new
  supporting rule: "Specialized packages extend their product's builder."
- Memories updated: reorganization plan (collapse EXECUTED), pro-taghelpers,
  project scope, shell-followups (`StellarAdminProOptions`), MEMORY.md index.
- Verified: solution builds without ShellPlayground; IdentitySimplePlayground
  Users/Roles 200.

- Delete `stellar-admin-pro/sandbox/ShellPlayground/`; remove from the workspace
  `StellarAdmin.slnx`.
- Docs sweep: `stellar-admin-pro/docs/design/*.md` mentions of AddShell/AddResources
  and the `AddStellarAdmin().AddIdentity<...>` registration examples (live-doc
  phrasing only), pro repo README/CLAUDE.md if any, workspace CLAUDE.md if it names
  the entry points, memory files.
- Update `docs/conventions/options-builders.md` for the Option B convention change
  (specialized packages register on their product's builder; generalize the verb
  table's `Add*` row and the canonical example's registration line).

### Smoke tests (per phase)

From the workspace root, `DOTNET_SYSTEM_NET_DISABLEIPV6=1 dotnet build StellarAdmin.slnx -p:AllowMissingPrunePackageData=true`, then on port 5206:
- DocsSamples `/DataGrid` — no unprocessed `<sa-data-grid>` tags.
- IdentitySimplePlayground `/admin/Users`, `/admin/Roles` — 200, sidebar renders,
  `_content/StellarAdmin.Pro/stellar-admin-pro.css` + `htmx.min.js` 200 (GETs only;
  committed app.db untouched).
- ShellPlayground `/admin` — 200 (phases 1–2 only).

## Out of scope

- Any change to the resource layer's own API (`ResourceControllerBase`, options,
  builders) — `Resources/` only loses its `StellarAdminBuilderExtensions.cs`.
- Sidebar item registration API rework (the hardcoded Voyager branding follow-up).
- Packaging/NuGet metadata.
