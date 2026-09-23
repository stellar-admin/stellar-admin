# Development and verification

## Checkout and setup

This product repository is self-contained for builds, tests, and consumer reference generation. The website remains an independent repository and is needed only for website integration/export tasks. Run commands here unless another working directory is stated; inspect status separately in every repository you change.

Use the .NET SDK declared by the relevant `global.json`, restore the repo's local .NET tools, and use the checked-in package manager/lockfile for each client project. The product SDK is currently pinned to 10.0.400. Website uses pnpm; library clients use npm. Read `package.json` for available scripts. Do not silently change SDK pins or dependency versions to fit a machine.

## .NET project and dependency changes

Use the .NET CLI to create projects (`dotnet new`), add or remove solution projects (`dotnet sln ... add/remove`), manage project references (`dotnet reference add/remove --project ...`), and add, update, or remove NuGet packages (`dotnet package add/remove --project ...`). Never edit `.csproj` or `.slnx` files directly when a CLI command supports the change. Keep package versions centrally managed in `Directory.Packages.props`; package commands should update central versions as well as project references. Direct MSBuild edits are reserved for settings without a corresponding CLI command.

For TUnit, follow the [official console-project setup](https://tunit.dev/docs/getting-started/installation/): create a console project with the pinned framework, add `TUnit` through the CLI, and remove the generated `Program.cs` so TUnit supplies the entry point. Add a direct reference to the SUT project and add the test project to `StellarAdmin.slnx` through the CLI.

`global.json` selects Microsoft.Testing.Platform for the .NET 10 `dotnet test` runner, following the [official runner configuration](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test#mtp-mode-of-dotnet-test). TUnit supplies the test-project metadata used for solution discovery. New TUnit projects added to `StellarAdmin.slnx` are automatically included by both workflows. `--minimum-expected-tests 1` rejects a discovered test application that runs no tests; do not suppress that exit code. Unit and integration test projects use the same runner and discovery command; no legacy test executables or individual test-project workflow steps remain.

## File encoding

Preserve each existing file’s encoding, UTF-8 BOM, and line endings when editing it. Do not strip or add a BOM as a side effect of reading and rewriting text; this can conflict with the encoding already selected in the user’s editor.

## Commands and validation

For new and migrated .NET tests, follow the [unit testing conventions](conventions/unit-testing.md). The active solution contains TUnit unit suites; project ownership and environment requirements are listed below. Each migration must document its verified TUnit command here and add the project to the solution. Both CI and release discover tests through the solution-wide command below; do not add individual unit-test project steps to either workflow.

Commands below run from the product repository root unless a working directory is specified. Normal commands come first; apply the conditional environment notes below only when needed.

| Change | Validation |
| --- | --- |
| All .NET tests (unit and integration) | `dotnet test --solution StellarAdmin.slnx --configuration Release --minimum-expected-tests 1`; use `--list-tests` instead of `--minimum-expected-tests 1` to verify discovery. Add `--no-build` after a Release build, as CI does. |
| Core unit tests | `dotnet run --project tests/StellarAdmin.Core.Tests --configuration Release`; append `-- --list-tests` to verify discovery. |
| Dashboard HTTP integration tests | `dotnet run --project tests/StellarAdmin.Dashboard.IntegrationTests --configuration Release`; uses an in-process TestServer with a private in-memory data source per test, no database or EF/Identity dependency. |
| EF Core HTTP integration tests | `dotnet test --project tests/StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests --minimum-expected-tests 1`; uses TestServer and a private in-memory SQLite database per test, with no external database service. |
| Dashboard unit tests | `dotnet run --project tests/StellarAdmin.Dashboard.Tests --configuration Release`; append `-- --list-tests` to verify discovery. |
| TagHelpers unit tests | `dotnet run --project tests/StellarAdmin.TagHelpers.Tests --configuration Release`; append `-- --list-tests` to verify discovery. |
| OSS C# | `dotnet build src/StellarAdmin.TagHelpers/StellarAdmin.TagHelpers.csproj`; exercise the affected DocsSamples page. |
| OSS CSS/JS | `npm run build` in `src/StellarAdmin.TagHelpers/Client/`; inspect the compiled bundle and exercise changed states. Run `build:css` directly for CSS changes because MSBuild has historically hidden client failures. |
| Dashboard | Build `src/StellarAdmin.Dashboard/StellarAdmin.Dashboard.csproj`. EF Core is included again with resource listing and CRUD. Identity and its playground remain excluded during the resource redesign. |
| Website app/MDX | `pnpm lint`, `pnpm types:check`, and `pnpm build` in `../website/`; inspect changed pages. |
| Consumer references | `dotnet run --project util/SkillsGenerator`, then `dotnet run --project util/SkillsGenerator -- --check`. |
| Release tooling | Run the **Release verification** workflow in GitHub Actions; see [release verification](../build/README.md) for inputs and cutover status. |
| Shared conventions | Edit the single source in `docs/conventions/`; check references in the repository guides. |
| Agent docs/skills | Check imports, symlinks, frontmatter, relative links, and commands; confirm discovery in fresh sessions when the agent is available. |

For C# formatting, run `dotnet tool restore` then `dotnet csharpier format <touched-path>` from the OSS repo where the formatter manifest lives. Do not format whole repos for a localized change. Consumer-facing references under `references/components/` and `components-index.md` are generated; other guides are handwritten. The generator preserves marked `structure` regions inside component references.

## Resource redesign baseline

The old resource builders, page/default options, controller base, and query infrastructure have been removed. Dashboard retains its shell, page tag helpers, Razor views, editors, and rendering data definitions. The replacement `AddResource<TResource>()` registration configures typed labels, index columns, and an `IResourceDataSource<TResource>`. The shared resource controller renders index, create, and edit pages. Create forms now support explicit fields, nested sections/rows/groups, validation, and persistence. Edit supports explicit keys, record lookup, configured-field updates, and missing-record handling. Delete uses a shared index confirmation dialog and an antiforgery-protected POST through the data source. Data source write operations return ResourceOperationResult for success, not-found, or validation rejection. Rejected create/edit submissions preserve entered values and show field or summary errors. Rejected deletes redisplay the index with a validation summary. `IResourceDataSource<TResource>` provides listing through `ListAsync(ResourceListRequest, CancellationToken)`, returning `ResourceListResult<TResource>` with items and their total count before paging. Index paging is opt-in through `EnablePaging`. Sources apply stable ordering and paging, and null paging requests all matching resources. Sources opt into create, edit, and delete handler interfaces or implement the combined `IResourceCrudDataSource<TResource>`. `AllowCreate<TModel, THandler>()` pairs a separate create model with a handler, `AllowEdit<TModel, THandler>()` does the same for loading and updating a separate edit model. Ordinary create and edit continue using TResource. Unsupported actions are hidden and reject direct requests. Configured actions without implementations fail options validation. `sandbox/DashboardPlayground` demonstrates Customer with separate custom create and edit models using its in-memory source. Product now uses a separate in-memory SQLite database through ProductDbContext, resets to 37 seeded products on startup, and exposes create, edit, and delete through the shared resource controller. Its two-decimal prices are stored as integer cents for SQLite sorting. Create, edit, and delete must be explicitly registered through their builder methods; a handler implementation alone does not enable an action. Repeated action registration replaces its previous configuration. The Customer password fields demonstrate validation only and do not create authentication accounts. Its ASP.NET Core EF/Identity host setup is retained for future work.

EF Core and its SQLite integration tests are back in `StellarAdmin.slnx` and build/release verification. `AddEfCoreResource<TContext, TEntity>` supplies key discovery, listing, and opt-in CRUD through the shared controller. EF search and scope predicates filter before counting and paging through the shared search UI and scope tabs. Column sort expressions override database ordering while preserving the shared sorting UI. Initialized EF-owned objects can be configured through ordinary column and form selectors such as `product => product.Details.Sku`; the Product playground demonstrates rendering, sorting, creation, and editing of that property. `UseEditor<ReferenceLookupEditorOptions>` and an application lookup provider render reference selects on create/edit forms, including custom models. The playground's Product–Category index column displays the CategoryId value directly. Query transformations and data-source event callbacks remain deferred for a broader API design. Identity and `sandbox/IdentitySimplePlayground` remain detached and still reference removed APIs.

The old Dashboard unit tests, Dashboard integration tests, EF resource integration tests, and shared Dashboard.Testing host have been deleted. The active solution command runs Core, TagHelpers, Dashboard unit tests, Dashboard HTTP integration tests, and EF Core SQLite HTTP integration tests. Dashboard resource work follows the [integration-first test strategy](conventions/unit-testing.md#dashboard-test-strategy). Unit tests are limited to invalid configuration, DI registration contracts, and provider isolation. Integration scenarios live under `tests/StellarAdmin.Dashboard.IntegrationTests/Resources`, with shared host setup under `Infrastructure`, sample types under `Fixtures`, and real Razor overrides under `Areas/StellarAdmin/Views`. The HTTP tests cover real application Razor overrides, repeated builder configuration, index routes, label overrides, columns, encoded cell values, empty results, request-scoped data sources, and create/edit form rendering, validation, binding, persistence, key protection, missing records, deletion, and antiforgery protection using the shared Razor views. The remaining legacy rendering coverage has not been restored.

## Samples, screenshots, and generated website demos

Jerrie's DocsSamples port is 5205; leave that process alone. A single agent can use 5206 when free:

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run --project docs/DocsSamples --no-launch-profile --urls http://localhost:5206
```

`--no-launch-profile` prevents launchSettings from taking port 5205. The explicit Development environment enables static web assets. Razor runtime compilation is not enabled: restart your instance after editing a partial. Stop only the processes you started. Concurrent sessions need distinct ports and isolated checkouts.

For visual checks use the repo's Chromium/CDP tools and capture relevant desktop/mobile, theme, dark-mode, and interaction states. For visual regression, capture base and head on the same machine/browser in one sitting; see [the OSS development guide](repos/stellar-admin.md) and `util/visual-regression/`.

Export samples with `dotnet run --project docs/DocsSamplesGenerator`. It rewrites demos and snippets in the separate website checkout, defaulting to `../website`. Set `STELLARADMIN_WEBSITE_DIR` to use a different destination. No workspace checkout is required. Record pre-existing changes first, inspect the output, and use the development skill's `scripts/classify-generator-output.sh ../website` only as a read-only aid. Its normalization can hide genuine SVG attribute changes. Never restore files just because their normalized output equals HEAD. If clean regeneration is needed, use coordinated temporary worktrees or a pre-generation snapshot so another person's edits cannot be discarded.

## Conditional local workarounds

These came from earlier sessions on Jerrie's machine; they are not universal build requirements. Recheck the environment before using them:

- If .NET downloads hang because of the local IPv6 route, prefix the command with `DOTNET_SYSTEM_NET_DISABLEIPV6=1`.
- If a build reports NETSDK1226 about missing prune package data, retry with `-p:AllowMissingPrunePackageData=true` and report the workaround.
- Earlier workspace sessions could accidentally bypass the product SDK pin. Run commands from the product root so `global.json` selects the intended SDK; do not change the pin to accommodate a machine.
- If the formatter tool shim fails despite restoring tools, inspect the installed tool and SDK. A cached `~/.nuget/packages/.../CSharpier.dll` path is machine/version-specific and should not be the shared default.

Do not change user-wide configuration as part of routine project setup. Report skipped checks and environment failures without claiming validation passed.
