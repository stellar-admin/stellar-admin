# Repository consolidation

Status: completed; user spot testing accepted and clean import committed. Updated: 2026-09-16.

Scope: keep `stellar-admin`, `website`, `skills`, and workspace; preserve `.Pro*` APIs and package names. User authorized a clean import and local commits. Push, publication, and repository deletion remain separate steps.

- Imported tracked Pro libraries, EF tests, sandboxes, and docs generator from `stellar-admin-pro` commit `dff2472b235d17334b8c26faabe0d26d5d3f83f7`. User chose a clean import; original checkout and history remain untouched.
- Merged DataGrid and its status template into DocsSamples; duplicate DocsSamplesPro was not imported. Identity design documents now live in workspace `docs/design/`.
- Unified MIT package metadata, references, solutions, CI test execution, release packing, website demos, and consumer references. Release publication still selects only TagHelpers until a separate release decision.
- Standalone source snapshot builds without sibling repos; both executable suites pass (108 EF integration assertions). Four packages pack, restore in a fresh consumer app, render grid/tag helpers, and serve assets. Package licenses, repository URLs, XML docs, and dependency versions verified.
- Observatory checks pass in light/dark at desktop/mobile widths; DataGrid selection, sorting, paging, and display templates pass. Website lint, types, and production build pass after final regeneration. All 415 demos export and 54 skill references pass the drift check; inline exports refresh without changes.
- Local verification uses installed SDK 10.0.400 from workspace instead of the unavailable pinned 10.0.100, with `AllowMissingPrunePackageData=true`. Existing nullable/XML warnings and the SQLitePCLRaw 2.1.11 NU1903 advisory remain. Browser and prerender checks require local-listener permission outside the sandbox.

CI-style packages pass the NuGet validator with network-dependent rules 52 and 119 excluded locally. Full validation cannot access the GitHub URLs from this sandbox; release CI retains all checks. The initial non-Git snapshot packages also lack source-link metadata, so validation was repeated against CI-style builds in the product checkout.

The migration is committed across the product, website, skills, and workspace repositories; stellar-admin-pro remains unchanged. All retained tracked Pro files are accounted for. Generated HTML changes include regenerated IDs and anti-forgery tokens; files proven identical except SVG attribute order were restored. No push or publication performed. Temporary sample server stopped.

Remaining: push and confirm remote CI when authorized; archive the old Pro repository and remove its local checkout when authorized. Package renaming and publication remain separate work.

Commits (workspace changes accompany this record):

- `stellar-admin`: `eb1382d`
- `website`: `caa6e398`
- `skills`: `c365861`

## Data grid follow-up — 2026-09-16

Moved the 18 data grid source files into `StellarAdmin.TagHelpers`, including public grid types and the sort enum, and updated resource/EF consumers. DocsSamples and DataGridSpike now reference only TagHelpers and use `AddTagHelpers()`; grid markup and existing CSS/JS are unchanged. Updated the theme coverage manifest, website setup instructions, and generated consumer references. This is a public namespace/assembly change from `StellarAdmin.Pro.TagHelpers` to `StellarAdmin.TagHelpers`.

Verification: full product solution build and both console test suites passed; the grid passed browser checks for selection, server sorting/paging, and display templates without Pro dependencies. All 415 docs demos exported to a temporary directory; the generated grid snippet remained identical. Website lint, type checking, and production build passed. Used installed SDK 10.0.400 from the workspace with `AllowMissingPrunePackageData=true`; the pinned SDK remains unchanged. Committed with product `01e8df2`, website `a59c9d07`, and skills `5340253`; the workspace tooling and this record are committed alongside them. Publication remains separate.

## Dashboard rename follow-up — 2026-09-16

Renamed the former Pro package family to `StellarAdmin.Dashboard`, `.Dashboard.Identity`, and `.Dashboard.EntityFrameworkCore`, with `AddDashboard()`, `StellarAdminDashboardBuilder`, and `StellarAdminDashboardOptions`. Updated project paths, namespaces, Razor imports, static assets, sample consumers, CI/release tooling, maintained documentation, and generated consumer references. No compatibility aliases. Historical records and the retired Pro checkout remain intact; website content required no changes.

Verification: workspace solution build, TagHelpers executable checks, all 108 EF resource integration assertions, CSharpier, and skills regeneration/drift check passed. All four NuGet packages were packed and inspected for renamed dependencies and assets; the fresh consumer smoke test passed, including rendered grid/tag helpers and Dashboard CSS/htmx asset URLs. Used SDK 10.0.400 and `AllowMissingPrunePackageData=true`; existing warnings remain. Committed with product `9b5ce43` and skills `4e5e761`; workspace tooling and this record are committed alongside them. Website is unchanged. Nothing pushed or published.

## Core extraction follow-up — 2026-09-16

Extracted `StellarAdminBuilder` and `AddStellarAdmin()` into `StellarAdmin.Core`, retaining the `StellarAdmin` namespace. Core depends only on `Microsoft.Extensions.DependencyInjection.Abstractions`; TagHelpers and Dashboard reference it. `ConfigureForms()` is now an extension supplied by TagHelpers with unchanged call syntax. Form types and icons remain in TagHelpers. Updated both solutions, maintained ownership documentation, and release tooling; Core must be published alongside future TagHelpers releases as its dependency.

Verification: workspace solution build, both executable test suites, formatting, all five package builds and dependency inspection, and the fresh package consumer smoke test passed. The smoke test resolves Core transitively and verifies form configuration, rendered tag helpers/grid, and static assets. Used installed SDK 10.0.400 with `AllowMissingPrunePackageData=true`; existing warnings and unavailable vulnerability-feed warnings remain. Product changes, including the explicit `StellarAdmin` root namespace, are committed as `3b5220e`; workspace tooling and this record are committed alongside them. Website and skills are unchanged. Nothing pushed or published.

## Core icons follow-up — 2026-09-16

Moved icon contracts, definitions, packs, and the manager into Core under `StellarAdmin.Icons`. `AddStellarAdmin()` now registers the manager and Lucide defaults; `AddIcon()` and `AddIconPack<T>()` belong to `StellarAdminBuilder`. TagHelpers retains SVG rendering. Renamed the build-only generator to `StellarAdmin.Generators`, updated its generated namespace/debug target, and transferred the analyzer reference and JSON inputs to Core. Updated both solutions, samples, website, consumer guidance, and maintained workspace documentation.

Per the user's instruction, the mutable static `DefaultIconManager.Instance` and all manager behavior are unchanged. Research its original rationale and cross-host/registration-order effects separately before changing its lifetime; repeated `AddStellarAdmin()` calls now apply Lucide defaults at that entry point. Disabled Tabler sources remain disabled.

Verification: workspace solution build, Core icon registration/override checks, existing TagHelpers rendering checks, EF integration suite, all five product packages, and fresh NuGet consumer smoke test passed. Package inspection confirms Core retains only the DI abstractions dependency and no generator/runtime analyzer dependency or raw definitions ship. All moved icon sources and JSON are byte-identical except namespaces. Used SDK 10.0.400 with `AllowMissingPrunePackageData=true`; existing warnings remain. Website lint, type checks, and production build passed using the installed package scripts directly because the pnpm launcher could not open its database in the sandbox; prerendering and the NuGet smoke test required local-listener access outside the sandbox. Changes span product, website, skills, and workspace and are uncommitted. Nothing pushed or published.

## Icon options follow-up — 2026-09-16

User authorized replacing the global mutable manager with the options pattern after reviewing its effects. Added public `StellarAdmin.Icons.IconOptions` with a case-insensitive dictionary seeded with Lucide defaults. Existing builder methods configure these options; the default manager is a per-provider DI singleton with a frozen lookup snapshot. `AddStellarAdmin()` uses `TryAddSingleton`, so repeated registration preserves custom managers and icon overrides. Core adds `Microsoft.Extensions.Options` 10.0.10.

Pack creation and duplicate-name exceptions now happen on options resolution. Later registrations do not affect existing providers. Dictionary changes after manager creation do not update its snapshot. Icon definitions still contain mutable collections; this change does not provide deep immutability. Updated website, consumer guidance, and workspace conventions.

Verification: solution build, icon options/isolation/ordering/duplicate-name tests, existing rendering tests, EF integration checks, package builds and Core dependency inspection passed. Website lint, type checks, production build, and fresh package consumer smoke verification passed. Used installed website scripts directly; production prerendering and the NuGet smoke test used approved temporary local listeners outside the sandbox. Used installed SDK 10.0.400 and `AllowMissingPrunePackageData=true`; existing warnings remain. The final direct-options implementation is committed as recorded below. No push or publication.

## Direct icon options follow-up — 2026-09-16

User chose direct options consumption after reviewing framework and third-party precedents. Removed `IIconManager`, `DefaultIconManager`, and manager registration. Tag helper constructors inject `IOptions<IconOptions>` and immediately retain `.Value`, as requested. Internal composition accepts resolved `IconOptions` through internal constructors for Icon, Input, and PaginationEllipsis helpers and parameters for shared rendering helpers. Updated the icon browser, package smoke check, tests, website, and consumer guidance.

This supersedes the frozen manager snapshot described above. Consumers read the options dictionary directly; configure it at startup and treat it and its definitions as read-only while serving requests. Registration API, Lucide defaults, case-insensitive lookup, overrides, and provider isolation remain intact.

Verification: full solution build, both executable test suites, constructor-time resolution and SVG/fallback assertions, website lint/type checks/production build passed. All five product packages packed, and the fresh NuGet consumer smoke test passed, including tag helper/grid rendering and static assets. Used SDK 10.0.400 with `AllowMissingPrunePackageData=true`; existing warnings remain. The options conversion and manager removal are committed as product `c015237`, website `01bbbc11`, and skills `c8f4f7b`; workspace changes and this record are committed alongside them. Nothing pushed or published.
