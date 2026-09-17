# Development and verification

## Checkout and setup

This product repository is self-contained for builds, tests, and consumer reference generation. The website remains an independent repository and is needed only for website integration/export tasks. Run commands here unless another working directory is stated; inspect status separately in every repository you change.

Use the .NET SDK declared by the relevant `global.json`, restore the repo's local .NET tools, and use the checked-in package manager/lockfile for each client project. The product SDK is currently pinned to 10.0.400. Website uses pnpm; library clients use npm. Read `package.json` for available scripts. Do not silently change SDK pins or dependency versions to fit a machine.

## File encoding

Preserve each existing file’s encoding, UTF-8 BOM, and line endings when editing it. Do not strip or add a BOM as a side effect of reading and rewriting text; this can conflict with the encoding already selected in the user’s editor.

## Commands and validation

Commands below run from the product repository root unless a working directory is specified. Normal commands come first; apply the conditional environment notes below only when needed.

| Change | Validation |
| --- | --- |
| OSS C# | `dotnet build src/StellarAdmin.TagHelpers/StellarAdmin.TagHelpers.csproj`; exercise the affected DocsSamples page. |
| OSS CSS/JS | `npm run build` in `src/StellarAdmin.TagHelpers/Client/`; inspect the compiled bundle and exercise changed states. Run `build:css` directly for CSS changes because MSBuild has historically hidden client failures. |
| Resources / Identity / EF Core | Build the affected project under `src/`; exercise DocsSamples or the Identity playground, including binding and view overrides. |
| Website app/MDX | `pnpm lint`, `pnpm types:check`, and `pnpm build` in `../website/`; inspect changed pages. |
| Consumer references | `dotnet run --project util/SkillsGenerator`, then `dotnet run --project util/SkillsGenerator -- --check`. |
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

Export samples with `dotnet run --project docs/DocsSamplesGenerator`. It rewrites demos and snippets in the separate website checkout, defaulting to `../website`. Set `STELLARADMIN_WEBSITE_DIR` to use a different destination. No workspace checkout is required. Record pre-existing changes first, inspect the output, and use the development skill's `scripts/classify-generator-output.sh ../website` only as a read-only aid. Its normalization can hide genuine SVG attribute changes. Never restore files just because their normalized output equals HEAD. If clean regeneration is needed, use coordinated temporary worktrees or a pre-generation snapshot so another person's edits cannot be discarded.

## Conditional local workarounds

These came from earlier sessions on Jerrie's machine; they are not universal build requirements. Recheck the environment before using them:

- If .NET downloads hang because of the local IPv6 route, prefix the command with `DOTNET_SYSTEM_NET_DISABLEIPV6=1`.
- If a build reports NETSDK1226 about missing prune package data, retry with `-p:AllowMissingPrunePackageData=true` and report the workaround.
- Earlier workspace sessions could accidentally bypass the product SDK pin. Run commands from the product root so `global.json` selects the intended SDK; do not change the pin to accommodate a machine.
- If the formatter tool shim fails despite restoring tools, inspect the installed tool and SDK. A cached `~/.nuget/packages/.../CSharpier.dll` path is machine/version-specific and should not be the shared default.

Do not change user-wide configuration as part of routine project setup. Report skipped checks and environment failures without claiming validation passed.
