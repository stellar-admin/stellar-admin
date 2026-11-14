# Development and verification

## Checkout and setup

This product repository is self-contained for builds, tests, and consumer reference generation. The website remains an independent repository and is needed only for website integration/export tasks. Run commands here unless another working directory is stated; inspect status separately in every repository you change.

Use the .NET SDK declared by the relevant `global.json`, restore the repo's local .NET tools, and use the checked-in package manager/lockfile for each client project. The product SDK is currently pinned to 10.0.400. Website uses pnpm; library clients use npm. Read `package.json` for available scripts. Do not silently change SDK pins or dependency versions to fit a machine.

## .NET project and dependency changes

Use the .NET CLI to create projects (`dotnet new`), add or remove solution projects (`dotnet sln ... add/remove`), manage project references (`dotnet reference add/remove --project ...`), and add, update, or remove NuGet packages (`dotnet package add/remove --project ...`). Never edit `.csproj` or `.slnx` files directly when a CLI command supports the change. Keep package versions centrally managed in `Directory.Packages.props`; package commands should update central versions as well as project references. Direct MSBuild edits are reserved for settings without a corresponding CLI command.

For TUnit, follow the [official console-project setup](https://tunit.dev/docs/getting-started/installation/): create a console project with the pinned framework, add `TUnit` through the CLI, and remove the generated `Program.cs` so TUnit supplies the entry point. Add a direct reference to the SUT project and add the test project to `StellarAdmin.slnx` through the CLI.

`global.json` selects Microsoft.Testing.Platform for the .NET 10 `dotnet test` runner, following the [official runner configuration](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test#mtp-mode-of-dotnet-test). TUnit supplies the test-project metadata used for solution discovery. New TUnit projects added to `StellarAdmin.slnx` are automatically included by both workflows. `--minimum-expected-tests 1` rejects a discovered test application that runs no tests; do not suppress that exit code.

## Public API changes

The three packaged projects use PublicApiAnalyzers. Each lists its public API in `PublicAPI.Shipped.txt` (what a released package contains) and `PublicAPI.Unshipped.txt` (changes since then). Adding, removing or changing a public member fails the build with RS0016 or RS0017 until the list is updated. Update it from the repository root, with the project whose API changed:

```bash
AllowMissingPrunePackageData=true dotnet format analyzers src/StellarAdmin/StellarAdmin.csproj --diagnostics RS0016 --severity error
```

The command adds entries for new and changed members (RS0016). It doesn't remove entries for deleted members, so delete those lines from `PublicAPI.Unshipped.txt` by hand; the RS0017 errors name them.

Review the diff of `PublicAPI.Unshipped.txt` like code: it is the public API change. During 0.x, StellarAdmin makes no API stability promise, so everything stays in Unshipped and the Shipped files stay empty. When releases start promising a stable API, move the Unshipped entries to Shipped at each release. The packaged projects also report CS1591 for a public member without an XML comment.

## File encoding

Preserve each existing file’s encoding, UTF-8 BOM, and line endings when editing it. Do not strip or add a BOM as a side effect of reading and rewriting text; this can conflict with the encoding already selected in the user’s editor.

## Commands and validation

For new .NET tests, follow the [unit testing conventions](conventions/unit-testing.md). Each test project is listed below with what it needs; add a new one to the solution and to this table. Both CI and release discover tests through the solution-wide command below; do not add individual test project steps to either workflow.

Commands below run from the product repository root unless a working directory is specified. Normal commands come first; apply the conditional environment notes below only when needed.

| Change | Validation |
| --- | --- |
| All .NET tests (unit and integration) | `dotnet test --solution StellarAdmin.slnx --configuration Release --minimum-expected-tests 1`. Use `--list-tests` instead of `--minimum-expected-tests 1` to verify discovery, here and for one project. Add `--no-build` after a Release build, as CI does. |
| Admin panel HTTP integration tests | `dotnet test --project tests/StellarAdmin.IntegrationTests --configuration Release --minimum-expected-tests 1`; uses an in-process TestServer with a private in-memory data source per test, no database or EF/Identity dependency. |
| EF Core HTTP integration tests | `dotnet test --project tests/StellarAdmin.EntityFrameworkCore.IntegrationTests --configuration Release --minimum-expected-tests 1`; uses TestServer and a private in-memory SQLite database per test, with no external database service. |
| Admin panel unit tests | `dotnet test --project tests/StellarAdmin.Tests --configuration Release --minimum-expected-tests 1`. |
| UI HTTP integration tests | `dotnet test --project tests/StellarAdmin.UI.IntegrationTests --configuration Release --minimum-expected-tests 1`; uses an in-process TestServer with MVC controllers and a Razor Page, and carries the TempData cookie between requests. No database. |
| UI unit tests (including icons) | `dotnet test --project tests/StellarAdmin.UI.Tests --configuration Release --minimum-expected-tests 1`. |
| Source generator tests | `dotnet test --project tests/StellarAdmin.Generators.Tests --configuration Release --minimum-expected-tests 1`; runs the generators on sample source and checks what they add. |
| Scenario spec tests | `dotnet test --project tests/StellarAdmin.ScenarioTesting.Tests --configuration Release --minimum-expected-tests 1`; runs the specs under its `Specs/` folder in an installed Chromium. |
| OSS C# | `dotnet build src/StellarAdmin.UI/StellarAdmin.UI.csproj`; exercise the affected DocsSamples page. |
| OSS CSS/JS | `npm run build` and `npm run check` in `src/StellarAdmin.UI/Client/`; exercise changed states in the browser. Run `build:css` directly for CSS changes because MSBuild has historically hidden client failures. |
| Admin panel | Build `src/StellarAdmin/StellarAdmin.csproj`. EF Core is included with resource listing and CRUD. AdminPlayground demonstrates Identity users and roles through ordinary resources. |
| Website app/MDX | `pnpm lint`, `pnpm types:check`, and `pnpm build` in `../website/`; inspect changed pages. |
| Consumer references | `dotnet run --project tools/StellarAdmin.Cli -- skills generate`, then `dotnet run --project tools/StellarAdmin.Cli -- skills check`. |
| Release tooling | Run the **Release verification** workflow in GitHub Actions; see [release verification](../build/README.md) for inputs and cutover status. |
| Shared conventions | Edit the single source in `docs/conventions/`; check references in the repository guides. |
| Agent docs/skills | Check imports, symlinks, frontmatter, relative links, and commands; confirm discovery in fresh sessions when the agent is available. |

For C# formatting, run `dotnet tool restore` then `dotnet csharpier format <touched-path>` from the OSS repo where the formatter manifest lives. Do not format whole repos for a localized change. Consumer-facing references under `references/components/` and `components-index.md` are generated; other guides are handwritten. The generator preserves marked `structure` regions inside component references.

## Samples, screenshots, and generated website demos

Jerrie's DocsSamples port is 5205; leave that process alone. A single agent can use 5206 when free:

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run --project docs/DocsSamples --no-launch-profile --urls http://localhost:5206
```

`--no-launch-profile` prevents launchSettings from taking port 5205. The explicit Development environment enables static web assets. Razor runtime compilation is not enabled: restart your instance after editing a partial. Stop only the processes you started. Concurrent sessions need distinct ports and isolated checkouts.

For visual checks use the repo's Chromium/CDP tools and capture relevant desktop/mobile, theme, dark-mode, and interaction states. For visual regression, capture base and head on the same machine/browser in one sitting; see [the OSS development guide](repos/stellar-admin.md) and `util/visual-regression/`.

Export samples with `dotnet run --project tools/StellarAdmin.Cli -- docs generate demos`; `docs generate screenshots` captures the admin docs screenshots and `docs generate all` does both; `docs clean demos`, `screenshots` or `all` deletes the output. Without a target, `docs generate` and `docs clean` only show help. Each `generate` deletes its own output first unless given `--keep-output`. It rewrites demos and snippets in the separate website checkout, defaulting to `../website`. Set `STELLARADMIN_WEBSITE_DIR` to use a different destination. No workspace checkout is required. Record pre-existing changes first, inspect the output, and use the development skill's `scripts/classify-generator-output.sh ../website` only as a read-only aid. Its normalization can hide genuine SVG attribute changes. Never restore files just because their normalized output equals HEAD. If clean regeneration is needed, use coordinated temporary worktrees or a pre-generation snapshot so another person's edits cannot be discarded.

## Conditional local workarounds

These came from earlier sessions on Jerrie's machine; they are not universal build requirements. Recheck the environment before using them:

- If .NET downloads hang because of the local IPv6 route, prefix the command with `DOTNET_SYSTEM_NET_DISABLEIPV6=1`.
- If a build reports NETSDK1226 about missing prune package data, retry with `-p:AllowMissingPrunePackageData=true` and report the workaround.
- Earlier workspace sessions could accidentally bypass the product SDK pin. Run commands from the product root so `global.json` selects the intended SDK; do not change the pin to accommodate a machine.
- If the formatter tool shim fails despite restoring tools, inspect the installed tool and SDK. A cached `~/.nuget/packages/.../CSharpier.dll` path is machine/version-specific and should not be the shared default.

Do not change user-wide configuration as part of routine project setup. Report skipped checks and environment failures without claiming validation passed.
