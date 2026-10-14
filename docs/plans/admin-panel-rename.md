# Package rename: StellarAdmin and StellarAdmin.UI

Status: proposed (2026-10-14).

"Dashboard" suggests observability, while the package is a CRUD admin panel. Instead of renaming it to AdminPanel (the first draft of this plan), the panel takes the brand name: the `StellarAdmin` package is the admin panel, and the component library becomes `StellarAdmin.UI`, absorbing `StellarAdmin.Core`. Applications register the panel with a single `AddStellarAdmin` call, and component-only applications call `AddStellarAdminUI`.

## Settled

1. The panel package takes the bare `StellarAdmin` name.
2. The component library is `StellarAdmin.UI`.
3. `StellarAdmin.Core` (icons and the shared builder) merges into `StellarAdmin.UI`.
4. The `sa-` tag prefix and the `stellar-admin.css`/`stellar-admin.js` asset names stay.
5. UI namespaces: everything in `StellarAdmin.UI`, with icons in `StellarAdmin.UI.Icons`.
6. UI builder and options: `StellarAdminUIBuilder`, `StellarAdminUIOptions` and `StellarAdminUIMenuOptions`; `StellarAdminFormsBuilder` and `StellarAdminFormsOptions` keep their names.
7. Layout constant: `StellarAdminLayouts.Default`.
8. Panel assets: `stellar-admin-panel.{css,js}`.
9. Panel custom element prefix: `admin-*`.
10. Lambda parameter in examples: `admin`.
11. Playground: `AdminPlayground`.
12. Change the namespace line in each playground migration and the snapshot to `AdminPlayground.Data.Migrations` (approved 2026-10-14). `app.db` is unaffected.
13. `StellarAdmin` and `StellarAdmin.EntityFrameworkCore` on NuGet are Jerrie's and continue as this product's package IDs.
14. The website phase runs in a separate worktree of the website repository after the product phases pass.

## Target API

```csharp
// Admin panel
builder.Services.AddStellarAdmin(admin =>
{
    admin.AddResource<Product>(...);
    admin.AddSidebarLink(...);
    admin.ConfigureUI(ui => ui.AddIconPack<TablerOutlineIconPack>());
});
app.MapStellarAdmin();

// Components only
builder.Services.AddStellarAdminUI(ui =>
{
    ui.AddIconPack<TablerOutlineIconPack>();
    ui.ConfigureMenu(...);
    ui.ConfigureForms(...);
});
```

`AddStellarAdmin` calls `AddStellarAdminUI`, so panel applications never register the UI separately. Both entry points are extensions on `IServiceCollection`, with a parameterless overload that returns the builder.

## Mapping

| Kind | Current | New |
| --- | --- | --- |
| Packages | `StellarAdmin.Core` + `StellarAdmin.TagHelpers` | `StellarAdmin.UI` |
| | `StellarAdmin.Dashboard` | `StellarAdmin` |
| | `StellarAdmin.Dashboard.EntityFrameworkCore` | `StellarAdmin.EntityFrameworkCore` |
| UI namespaces | `StellarAdmin.TagHelpers`, `StellarAdmin.Icons`, `StellarAdmin` (forms) | `StellarAdmin.UI`, `StellarAdmin.UI.Icons` |
| UI entry point | `AddStellarAdmin().AddTagHelpers()`, `ConfigureForms` on the Core builder | `AddStellarAdminUI()` |
| UI builder | `StellarAdminBuilder` (Core: icons) + `StellarAdminTagHelpersBuilder` (menu) | `StellarAdminUIBuilder` (icons, menu, forms) |
| UI options | `StellarAdminTagHelpersOptions`, `StellarAdminTagHelpersMenuOptions` | `StellarAdminUIOptions`, `StellarAdminUIMenuOptions` |
| UI `@addTagHelper` | `*, StellarAdmin.TagHelpers` | `*, StellarAdmin.UI` |
| UI asset path | `_content/StellarAdmin.TagHelpers/stellar-admin.{css,js}` | `_content/StellarAdmin.UI/stellar-admin.{css,js}` |
| Panel namespaces | `StellarAdmin.Dashboard.*` | `StellarAdmin.*` (e.g. `StellarAdmin.Resources.Editors`) |
| Panel entry point | `AddStellarAdmin().AddDashboard(configureDashboard)` | `AddStellarAdmin(configure)` |
| Panel builder and options | `StellarAdminDashboardBuilder`, `StellarAdminDashboardOptions` | `StellarAdminBuilder`, `StellarAdminOptions` |
| Panel builder extensions | `StellarAdminDashboardBuilderExtensions` (both packages) | `StellarAdminBuilderExtensions` |
| Layout constant | `StellarAdminLayouts.Dashboard` | `StellarAdminLayouts.Default` |
| Panel assets | `_content/StellarAdmin.Dashboard/stellar-admin-dashboard.{css,js}` | `_content/StellarAdmin/stellar-admin-panel.{css,js}` |
| Panel npm package | `stellar-admin-dashboard` | `stellar-admin-panel` |
| Panel custom elements, classes, events, JS classes | `dashboard-remote-sheet`, `DashboardLookupPicker`, … | `admin-remote-sheet`, `AdminLookupPicker`, … |
| Lambda in samples, docs and tests | `dashboard =>` | `admin =>` |
| Tests | `StellarAdmin.Core.Tests` + `StellarAdmin.TagHelpers.Tests`, `StellarAdmin.TagHelpers.IntegrationTests` | `StellarAdmin.UI.Tests`, `StellarAdmin.UI.IntegrationTests` |
| | `StellarAdmin.Dashboard.*Tests`, `StellarAdmin.Dashboard.EntityFrameworkCore.*Tests` | `StellarAdmin.*Tests`, `StellarAdmin.EntityFrameworkCore.*Tests` |
| Test host | `DashboardTestHost` | `StellarAdminTestHost` |
| Sandbox | `sandbox/DashboardPlayground` (including the migrations namespace) | `sandbox/AdminPlayground` |
| Consumer skills | `stellar-admin-dashboard`, `stellar-admin-tag-helpers` | `stellar-admin`, `stellar-admin-ui` (`stellar-admin-theme` unchanged) |
| Prose | "the Dashboard", "the tag helpers library" | "StellarAdmin" / "the admin panel", "StellarAdmin.UI" |

Unchanged: the `sa-`/`sel-` prefixes, `stellar-admin.css`/`.js`, the UI npm package (`stellar-admin-ui`), `MapStellarAdmin`, the `/stellaradmin` route, the `StellarAdmin` area, `ComponentPlayground`, `DataGridSpike`, generic "dashboard" icon names and wording, vendored libraries, and archived plans (kept as history).

## NuGet

Checked 2026-10-14:

- `StellarAdmin` and `StellarAdmin.EntityFrameworkCore` (Jerrie's earlier work) exist with versions 0.9.0–0.30.0 and are unlisted and deprecated. Releasing the panel means undeprecating and relisting them and publishing above 0.30.0.
- `StellarAdmin.Core` (0.3.0, 0.4.0) and `StellarAdmin.TagHelpers` (up to 0.4.0) are listed under `jerriep`. They get deprecated in favor of `StellarAdmin.UI` after its first release.
- `StellarAdmin.UI` and `StellarAdmin.Dashboard` are unused.

The release workflow switches its publishable set from Core and TagHelpers to `StellarAdmin.UI`. Enabling publishing for `StellarAdmin` and `StellarAdmin.EntityFrameworkCore`, version numbers and the NuGet deprecations are release steps outside this rename.

## Phases

1. **Move and merge projects.** `git mv` the source, test, sandbox and skill directories; move Core's `Icons` and builder into `StellarAdmin.UI`; merge `StellarAdmin.Core.Tests` into `StellarAdmin.UI.Tests`. Update `StellarAdmin.slnx`, project references, `InternalsVisibleTo`, `.DotSettings`, `.claude/skills` links and the panel and UI client build scripts. Delete stale `bin`/`obj` under the old paths.
2. **UI identifiers.** Namespaces, the merged `StellarAdminUIBuilder`, `AddStellarAdminUI`, options, `@addTagHelper` and `_content/StellarAdmin.UI` paths across the library, samples, DocsSamples and its generator, sandbox HTML prototypes and the `util` tools.
3. **Panel identifiers.** Namespaces, `AddStellarAdmin`, `StellarAdminBuilder`, `StellarAdminOptions`, `ConfigureUI`, the layout constant, assets, client elements and classes, test host and playground.
4. **Build and release scripts.** `build/smoke-test.sh`, `.github/workflows/release.yml`, `util/SkillsGenerator`.
5. **Prose and generated references.** XML documentation, maintained docs (`readme.md`, `AGENTS.md`, `docs/development.md`, `docs/agents.md`, `docs/repos/`, `docs/conventions/`, `docs/design/`, `skills/README.md`), development skills in `.agents/skills/`, the still-relevant plans and the plan index. Regenerate the consumer skills.
6. **Verify the product.** Build both client bundles and the solution, run all tests, check the regenerated skills, run `build/smoke-test.sh`, quick-check the playground and ComponentPlayground on port 5206 and stop them, and grep for leftover `Dashboard`, `StellarAdmin.Core` and `StellarAdmin.TagHelpers` outside the unchanged list.
7. **Website** (separate worktree of the website repository, decision 14). Update package names, `@addTagHelper`, `_content` paths and install snippets, re-export docs demos and inline examples, and build the site.

Stop for review after phase 6, and again after phase 7. No commits unless requested.

## Progress

**Phase 1 (done 2026-10-14).** Moved with `git mv`: `src/StellarAdmin.TagHelpers` → `src/StellarAdmin.UI` (Core's `Icons/`, `StellarAdminBuilder.cs` and `StellarAdminExtensions.cs` moved in, the generator reference and icon `AdditionalFiles` moved into `StellarAdmin.UI.csproj`, Core's project deleted), `src/StellarAdmin.Dashboard` → `src/StellarAdmin`, `src/StellarAdmin.Dashboard.EntityFrameworkCore` → `src/StellarAdmin.EntityFrameworkCore`, the five test projects (Core tests merged into `StellarAdmin.UI.Tests`), `sandbox/DashboardPlayground` → `sandbox/AdminPlayground`, and the skills to `skills/stellar-admin` and `skills/stellar-admin-ui`. Files renamed ahead of their types: `StellarAdminUIBuilder.cs`, `StellarAdminUIExtensions.cs`, `StellarAdminUIOptions.cs`, `StellarAdminUIMenuOptions.cs`; in the panel `StellarAdminServiceCollectionExtensions.cs` (was `StellarAdminBuilderExtensions.cs`, the `AddDashboard` entry point), `StellarAdminBuilder.cs`, `StellarAdminOptions.cs`, `StellarAdminBuilderExtensions.cs`; the EF Core `StellarAdminBuilderExtensions.cs`; tests `StellarAdminUIExtensionsTests*.cs`, `StellarAdminUIBuilderTests.Icons.cs`, `StellarAdminBuilderTests.cs`, `StellarAdminBuilderExtensionsTests.cs`, `StellarAdminTestHost.cs`, `StellarAdminAuthorizationTests.cs`, `StellarAdminSheetTests.cs`, `StellarAdminThemeTests.cs`; panel client `stellar-admin-panel.js`; playground stylesheet `playground.css`. Updated `StellarAdmin.slnx`, all project references, `InternalsVisibleTo`, the client `package.json`/lockfile names, `copy-js.mjs`, the Tailwind adapter imports (panel, playground, DocsSamples, ComponentPlayground) and the generator's launch settings. The playground keeps its `UserSecretsId` so local secrets still resolve. `.claude/skills` links point at `.agents/skills` and needed no change. Stale `bin`/`obj` and the untracked `tests/StellarAdmin.Dashboard.EntityFrameworkCore.Tests` leftovers were deleted.

Verification: `dotnet build StellarAdmin.slnx` builds all libraries, tests, AdminPlayground, DataGridSpike and the generators, including both client bundles. DocsSamples and ComponentPlayground fail with Razor errors because `@addTagHelper *, StellarAdmin.TagHelpers` no longer resolves; the panel views compile but their tag helpers are unbound for the same reason. Both are phase 2 work. Tests were not run.

**Phase 2 (done 2026-10-14).** Namespaces `StellarAdmin.TagHelpers` and `StellarAdmin` (forms) became `StellarAdmin.UI`, and `StellarAdmin.Icons` became `StellarAdmin.UI.Icons` (including the icon generator's output), across the library, tests, DocsSamples and its generator, ComponentPlayground, DataGridSpike, the sandbox HTML prototypes, `util/visual-regression` and the UI references inside the panel. The same pass renamed `@addTagHelper *, StellarAdmin.UI`, `_content/StellarAdmin.UI/` paths and the test namespaces (`StellarAdmin.UI.Tests`, `StellarAdmin.UI.IntegrationTests`). Core's `StellarAdminBuilder` (icons), the tag helpers builder (menu) and `ConfigureForms` merged into `StellarAdminUIBuilder`; Core's `AddStellarAdmin` and `AddTagHelpers` merged into `AddStellarAdminUI()` and `AddStellarAdminUI(Action<StellarAdminUIBuilder>)` in `StellarAdminUIExtensions`. Options are `StellarAdminUIOptions` and `StellarAdminUIMenuOptions`. Test classes and methods follow (`StellarAdminUIBuilderTests`, `StellarAdminUIExtensionsTests`, `AddStellarAdminUI_*`). CSharpier was run on the changed C# files; its reformatting of files that were never formatted (playground `HomeController`, the four playground migrations, ComponentPlayground `TableRowDetails`, DataGridSpike `SpikeItemTemplateTagHelper`) was reverted, keeping only reflows of lines the rename shortened.

Verification: StellarAdmin.UI, both UI test projects, DocsSamples, DocsSamplesGenerator, ComponentPlayground and DataGridSpike build. `dotnet test` passes 513 tests in `StellarAdmin.UI.Tests` and 19 in `StellarAdmin.UI.IntegrationTests`. The panel fails to compile only where its `AddDashboard` extends the removed Core `StellarAdminBuilder` (three errors in `StellarAdminServiceCollectionExtensions.cs`), which phase 3 replaces; the panel's tests, EF Core package and AdminPlayground depend on it and were not built.

**Phase 3 (done 2026-10-14).** Panel namespaces `StellarAdmin.Dashboard.*` and `StellarAdmin.Dashboard.EntityFrameworkCore` became `StellarAdmin.*` and `StellarAdmin.EntityFrameworkCore`, with the test namespaces following (`StellarAdmin.Tests`, `StellarAdmin.IntegrationTests`, `StellarAdmin.EntityFrameworkCore.IntegrationTests`). `AddStellarAdmin()` and `AddStellarAdmin(Action<StellarAdminBuilder>)` on `IServiceCollection` (`StellarAdminServiceCollectionExtensions`) replace `AddStellarAdmin().AddDashboard(...)` and call `AddStellarAdminUI()`; `StellarAdminBuilder.ConfigureUI(Action<StellarAdminUIBuilder>)` configures the UI from the panel. Types: `StellarAdminBuilder`, `StellarAdminOptions`, `StellarAdminBuilderExtensions` (both packages), `StellarAdminLayouts.Default`. Assets are `_content/StellarAdmin/stellar-admin-panel.{css,js}`; custom elements, events and JS classes use `admin-*`/`Admin*`; the playground stylesheet link is `~/css/playground.css`. Samples, tests and the playground use the `admin` lambda and parameter names, `StellarAdminTestHost` and `configureAdmin`; test methods `PanelAndResourceRequirements_*` and `HostPage_RendersInDefaultLayout`. The playground namespace is `AdminPlayground`, including the namespace and `using` lines of its migrations and snapshot (decision 12); the entity type-name strings inside the migration designers and snapshot still read `DashboardPlayground.Data.*` (see open question below). XML docs, comments and package descriptions are phase 5. CSharpier was run on the touched panel files that were formatted at HEAD; the playground `HomeController` was left unformatted.

Verification: `dotnet build StellarAdmin.slnx` succeeds, including both client bundles. Tests pass: `StellarAdmin.Tests` 154, `StellarAdmin.IntegrationTests` 437, `StellarAdmin.EntityFrameworkCore.IntegrationTests` 86, `StellarAdmin.UI.Tests` 513, `StellarAdmin.UI.IntegrationTests` 19. The playground was not run (phase 6).

Snapshot strings (decision 15, 2026-10-14): the migration designers and the snapshot still name entity types `DashboardPlayground.Data.*` and stay that way. The app and the schema are unaffected, since EF Core diffs tables, and the next `migrations add` rewrites the snapshot strings itself.

**Phase 4 (done 2026-10-14).** `util/SkillsGenerator` reads `src/StellarAdmin.UI` into `skills/stellar-admin-ui` (catalog title "StellarAdmin.UI") and `src/StellarAdmin` into `skills/stellar-admin` (title "StellarAdmin", component files name the `StellarAdmin` package). `build/smoke-test.sh` installs `StellarAdmin.UI` and `StellarAdmin`, maps both `StellarAdmin` and `StellarAdmin.*` to the folder feed, registers with `AddStellarAdmin(admin => admin.ConfigureUI(ui => ui.ConfigureForms(...)))`, checks that icons come from the `StellarAdmin.UI` assembly, and asserts the `_content/StellarAdmin.UI/` and `_content/StellarAdmin/` assets (`stellar-admin-panel.css`, `htmx.min.js`). The release workflow installs the `src/StellarAdmin.UI/Client` and `src/StellarAdmin/Client` dependencies, packs and validates `StellarAdmin.UI`, `StellarAdmin` and `StellarAdmin.EntityFrameworkCore`, and publishes and attaches only `StellarAdmin.UI`.

Verification: `bash -n` passes on the smoke test and the workflow parses as YAML. SkillsGenerator builds and `--check` reports the expected drift in five files (both catalog titles and the `Package:` line of the three panel components), which phase 5 regenerates. The smoke test and the workflow were not run (phase 6 runs the smoke test).

**Phase 5 (done 2026-10-14).** Prose now says StellarAdmin.UI, StellarAdmin or "the admin panel" in the panel and EF Core XML docs, comments and package metadata (the panel's `dashboard` package tag became `admin-panel`), the client CSS and script comments, `readme.md`, `AGENTS.md`, `build/README.md`, the development skills in `.agents/skills/`, the maintained docs (`docs/development.md`, `docs/conventions/`, `docs/design/tokens.md`, `docs/repos/`), the still-open plans and the plan index, and the playground's stylesheet comment and Reports page. The UI test-strategy heading became "Admin panel test strategy" (anchor `#admin-panel-test-strategy`). Consumer skills: `stellar-admin-ui` (skill name, description, setup with `AddStellarAdminUI()`, icons and menu examples on the UI builder, `_content/StellarAdmin.UI/` paths, `StellarAdmin.UI.Icons`), `stellar-admin` (setup notes with `AddStellarAdmin(admin => ...)`, `StellarAdminLayouts.Default`, `StellarAdmin.*` namespaces, AdminPlayground), `stellar-admin-theme` and `skills/README.md` ("Install the UI skill"); the references were regenerated. Left as history: archived plans and their index entries, the superseded Identity designs, `docs/plans/oss/archive`, the `sandbox/html` prototypes, and website URLs and paths (`docs/tag-helpers`, phase 7).

Verification: SkillsGenerator regenerated 61 component files and both indexes, and `--check` reports no drift. `dotnet build StellarAdmin.slnx` succeeds. CSharpier passes on the edited C# files. Tests were not run (phase 6).

Panel page tag helpers (decision 16, 2026-10-14): `FormPageTagHelper`, `IndexPageTagHelper`, `ViewPageTagHelper` and `FormSectionLayoutExtensions` moved from `StellarAdmin.TagHelpers` (what phase 3's rule produced, identical to the old UI namespace) to the root `StellarAdmin` namespace; the folder stays `TagHelpers/`. The panel's `_ViewImports.cshtml` dropped its `@using StellarAdmin.TagHelpers`, and the `stellar-admin` setup notes say `@using StellarAdmin`. The solution builds, `--check` reports no drift, and the panel suites pass (154, 437, 86).

**Phase 6 (done 2026-10-14).** Verified the product with the phase 4, phase 5 and decision 16 changes in the working tree; nothing was changed.

Verification: `npm run build` succeeds in `src/StellarAdmin.UI/Client` and `src/StellarAdmin/Client` and leaves the tracked bundles unchanged. A `--no-incremental` build of `StellarAdmin.slnx` first failed with Razor offset errors in `_IndexDataGrid.cshtml` from stale compiler-server state; after `dotnet build-server shutdown` it succeeds with 0 errors and 38 warnings (test nullability, DocsSamples page models and an unresolved `Options.FormFieldOptions` cref in `ViewDataKeys.cs`, written the same way before the rename). Tests pass: `StellarAdmin.UI.Tests` 513, `StellarAdmin.UI.IntegrationTests` 19, `StellarAdmin.Tests` 154, `StellarAdmin.IntegrationTests` 437, `StellarAdmin.EntityFrameworkCore.IntegrationTests` 86. SkillsGenerator `--check` reports no drift. `dotnet pack -c Release` of the three packages at `0.0.0-rename.1` produces `StellarAdmin.UI`, `StellarAdmin` (depends on `StellarAdmin.UI`) and `StellarAdmin.EntityFrameworkCore` (depends on `StellarAdmin`), and `build/smoke-test.sh` passes against them. AdminPlayground on port 5206 returns 200 for the home page, `/stellaradmin`, all 27 sidebar links, create, edit, view and paging, redirects users and roles to login, and serves the `_content/StellarAdmin.UI/` and `_content/StellarAdmin/` assets; `app.db` was unchanged. ComponentPlayground on port 5206 returns 200 for all 39 linked pages and its `_content/StellarAdmin.UI/` assets. Both were stopped. Outside the accepted history (this plan, the plan index, archives, the Identity designs, history notes in `options-builders.md` and `xml-documentation.md`, migrations, `sandbox/html`), the only old names are the playground's kept `UserSecretsId`, the theme skill's note on the earlier package name, and generic "Dashboard" nav labels. The release workflow itself was not run.

EF Core package tags (decision 17, 2026-10-14): `StellarAdmin.EntityFrameworkCore` inherited the UI package's `tag helper shadcn crud` tags and now sets its own `admin;admin-panel;crud;entity-framework-core`. A Release pack shows the new tags in the nuspec.
