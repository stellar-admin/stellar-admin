# Workspace and skills consolidation

Status: active — step 1 committed; step 2 (contributor guidance) implemented and verified, with local commits authorized. Steps 3–5, publication, and retirement remain separate. Updated: 2026-09-17.

Affected repositories: workspace, stellar-admin, skills, and website (documentation and development guidance).

## Recommendation

Make `stellar-admin/stellar-admin` the self-contained product and contributor repository. Move consumer skills, their generator, release tooling, and maintained product development guidance into it. Keep `website` independent initially. Retire the workspace as an active dependency; retain the old repositories for history and installation compatibility until the transition is proven.

The OSS/Pro split no longer justifies a release orchestration repository. The current release workflow already builds all five packages from a single product checkout. The workspace solution largely repeats the product solution, adding only SkillsGenerator. Consumer references are tightly coupled to the library and samples, so colocating them permits one PR and a meaningful drift check.

The remaining benefits of a separate skills repo are its small download size and stable installation address, not independent source ownership. A compatibility marketplace can preserve the address without maintaining duplicate skill content.

## Proposed destinations

Paths in the destination column are relative to the product repository.

| Current workspace path | Destination / treatment |
| --- | --- |
| `skills/plugins/stellar-admin/` | `plugins/stellar-admin/`; preserve all four sibling skills and references |
| `skills/.claude-plugin/marketplace.json` | `.claude-plugin/marketplace.json`; preserve marketplace and plugin names |
| `skills/README.md` | `plugins/stellar-admin/README.md`, with new paths and install instructions |
| `src/SkillsGenerator/` | `util/SkillsGenerator/`, added to the existing product solution |
| `.github/workflows/release.yml` | `.github/workflows/release.yml`, simplified to one checkout and one tag |
| `build/smoke-test.sh` | `build/smoke-test.sh` |
| `.config/dotnet-tools.json` | Merge the NuGet validator into the product tool manifest; retain CSharpier |
| `Directory.Packages.props` | Merge only the generator's needed dependency configuration |
| `docs/conventions/`, maintained product `docs/design/`, useful plans | Corresponding product docs; retain historical status and attribution |
| `AGENTS.md`, `CLAUDE.md`, `.agents/skills/`, `.claude/skills/` | Product contributor setup with rewritten paths and relative symlinks |
| Website-specific contributor guidance and workflow | Website-owned instructions, or explicit links to product-owned shared workflows |
| Workspace solution, `.idea/`, personal `.DotSettings.user`, local permission settings | Do not copy wholesale; keep the existing product solution and selectively retain shared settings |

Development skills remain in `.agents/skills/`; shipped consumer skills remain under `plugins/stellar-admin/skills/`. They have different audiences despite sharing a repository. The existing instruction to keep development setup out of child repos must be deliberately replaced as part of retirement.

## Skills migration

Update `SkillsGenerator/Program.cs` to discover the product root from `util/SkillsGenerator`, read `src/` and `docs/DocsSamples/Pages/`, and write `plugins/stellar-admin/skills/tag-helpers/references/`. Update the manifest location, diagnostics, commands, and development workflows. Preserve the handwritten `structure` regions inside generated component files. Curated example paths are relative to sample pages and should not need a wholesale rewrite.

Add `dotnet run --project util/SkillsGenerator -- --check` to product CI, with handwritten frontmatter/link validation as appropriate. The current check compares expected outputs but does not detect obsolete component files, and missing curated examples are silently skipped. Address those gaps before treating it as a complete reference-integrity gate.

There is a dependency collision: workspace Roslyn is `Microsoft.CodeAnalysis.CSharp` 5.0.0, while the product pins 4.14.0. Preserve 5.0 for SkillsGenerator initially with a scoped central-package declaration or supported project override, then validate; do not silently downgrade the parser or upgrade the product source generator as a side effect. Spectre.Console already matches at 0.57.1.

For new installations, the marketplace address becomes `stellar-admin/stellar-admin`; `/plugin install stellar-admin@stellar-admin` can retain its identity. Update the plugin's repository URL and `website/content/docs/tag-helpers/agent-skills.mdx`. Keep a deliberate plugin version policy: its current manifest says 0.1.0, and repository consolidation does not by itself ensure installed clients refresh their cached content.

For existing installations, keep `stellar-admin/skills` temporarily as a compatibility marketplace, pointing at `plugins/stellar-admin` in the product repo using a `git-subdir` source. This is an explicitly supported source type, but installation/update behavior needs testing with supported client versions. Avoid an ongoing generated-content mirror. If older clients cannot follow the new source, retain a frozen last-compatible version and publish migration instructions. Do not assume users' existing marketplace URLs or local symlinks update automatically. [Claude marketplace documentation](https://code.claude.com/docs/en/plugin-marketplaces)

Keeping the marketplace in the product repo increases marketplace checkout size. Subdirectory plugin sources can reduce plugin-fetch bandwidth; they do not automatically eliminate fetching the marketplace repository itself. Assess this during installation validation, rather than introducing separate distribution infrastructure immediately.

## Release migration

The current workspace workflow builds, packs, validates, tests, and smoke-tests Core, TagHelpers, Dashboard, Dashboard.Identity, and Dashboard.EntityFrameworkCore. It publishes only Core and TagHelpers. Preserve this allowlist: moving the workflow is not a decision to publish the other three packages.

Use the dispatched product ref as the single source revision, record its SHA, and tag precisely that commit. Remove the extra checkout, `oss_ref` indirection, workspace SHA/tag, and cross-repository GitHub App authentication if repository rules allow the normal token. Keep dry runs, package artifacts, package validation, both test suites, and the consumer smoke test. Set SDK selection from root `global.json`, run tools from the root manifest, and remove the old `stellar-admin/` path prefixes. Keep writes and OIDC permission confined to the publish job.

Recreate or verify the `nuget-org` environment, its protection rules, `NUGET_USER` availability, and repository release/tag permissions in the product repository. Configure NuGet trusted publishing for owner `stellar-admin`, repository `stellar-admin`, workflow filename `release.yml`, and the intended environment. These are external settings; copying YAML cannot migrate them. Their live configuration was not inspected during this analysis. [NuGet trusted publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing)

Using `GITHUB_TOKEN` for tags/releases avoids the cross-repo app, but its generated events generally do not trigger other workflows. Check actual downstream release automation and rulesets before removing the app. [GitHub workflow triggering](https://docs.github.com/en/actions/how-tos/write-workflows/choose-when-workflows-run/trigger-a-workflow)

The local workspace and product tag-name lists currently match, through v0.2.0; verify remote tags and published versions before cutover. Keep historical tags unchanged. Audit the existing version guard while moving it: it uses a limited regex and `sort -V`, and a release-tag guard can prevent recovery after packages/tags were published but release assets failed. Add serialized publication and an explicit recovery path for the same version/SHA. Never cancel an in-progress package publication merely because a second release was requested.

Disable the old publishing entry point at cutover so only one repository can initiate normal releases. Retire old app credentials/trusted policy only after confirming no remaining use. A successful dry run proves packaging, not NuGet OIDC policy or publish permissions; the first explicitly authorized release completes that verification.

## Workspace and website

The workspace has maintained theme specifications, Identity designs, conventions, and component-development workflows that are still valuable. Removing the workspace without migrating these would make the product less self-contained. Import selected current files and useful records, with source commit provenance; retain the old repo for original history. Do not merge all workspace history or machine-specific configuration by default. Review imported historical material for obsolete commercial assumptions and local references.

The website can continue as a sibling checkout under an ordinary, untracked parent directory. `docs/DocsSamplesGenerator/Generator.cs` already defaults to `../website` and accepts `STELLARADMIN_WEBSITE_DIR`; no workspace Git repository is needed. Update website guidance that currently tells contributors to start in the workspace. Ordinary product builds, tests, release packaging, and skills checks must work without a website checkout.

Keeping the website separate retains its independent Node/TanStack deployment lifecycle. The tradeoff is that component documentation/export changes still span two repositories. Moving it into the product would remove that last boundary, but would also require reviewing deployment root settings, CI paths, and the larger checkout. Treat that as a separate decision rather than a prerequisite.

## Agreed implementation sequence and acceptance

The user confirmed that the website must remain separate and accepted this five-step order, superseding the original analysis ordering:

1. Move consumer skills and SkillsGenerator into the product repo; verify generation, preservation, and CI drift checking. Completed and committed as product `51bcfb7` and workspace `0c80b64`.
2. Move contributor guidance, maintained designs, theme specifications, development skills, and plans into the product. Give the website a standalone contributor entry point and validate discovery without workspace/sibling checkouts. Implemented locally; see verification below.
3. Move release tooling and prove a complete dry run in the product repo. Keep the workspace publishing entry point until this passes.
4. Switch skills distribution and publishing: update website installation instructions, test a compatibility marketplace, configure NuGet trust/environment settings, disable old publishing, then use the new workflow for the next authorized release.
5. Retire the workspace after successful release verification; retain history and the compatibility endpoint as needed. Validate cross-repo export with separate checkouts.

Next concrete step after review of step 2: commit if requested, then begin the release-tooling move only when authorized. The website remains a separate repository throughout.

## Initial analysis verification and working tree state

Inspected local workflow YAML, smoke script, solutions, central package files, tool manifests, skills manifests/content layout, generator source, contributor guides, website references, and local release tags. Checked current official NuGet, GitHub Actions, and Claude marketplace documentation. No build, generator, install, release, or remote repository-settings validation was performed; this is an analysis, not a tested migration.

At inspection, product, skills, and website working trees were clean. Workspace had a pre-existing modification to `StellarAdmin.sln.DotSettings.user`, left untouched. Analysis adds this plan and its index entry only.


## Step 1 implementation — 2026-09-17

User authorized step 1 only. Imported the consumer plugin and marketplace from skills commit `524fb2c30d5e6c4c5930ddcf196f8facda68e5ab` into the product. Moved SkillsGenerator from workspace commit `3cf57baa0b51a8cfce5c813554e59880361f0655` to product `util/SkillsGenerator`; unchanged support files retain their original bytes. Updated both solutions, product CI, and currently used workspace commands/ownership guidance. The product baseline was `97691a8c3ce5f86811d767ec6c4230520fb29599`. Product changes committed as `51bcfb7` after the user authorized committing step 1; workspace migration and handoff changes accompany this record.

SkillsGenerator uses a project-local Roslyn `VersionOverride="5.0.0"`; resolved assets confirm the product source generator still uses 4.14.0. Added the plugin README and retained its MIT license. The legacy skills repository and website are unchanged; distribution cutover and compatibility forwarding remain step 4. Development guidance continues to live in the workspace pending step 2.

Verification completed:

- Regenerated 54 component references and the index, then passed `--check`, including the exact Release/no-build command added to CI. Only three existing sample icon names changed (Card and Dropdown Menu: `trash-2` → `trash`; Toggle Group: `waves` → `waves-horizontal`).
- Compared all imported skill files: all five handwritten structure regions and all other skill content were preserved byte-for-byte. Validated four skill frontmatter blocks, 60 relative links, and marketplace/plugin JSON.
- Copied generator inputs/configuration and skills into a temporary standalone directory without workspace, skills, or website sibling checkouts. Its check passed; intentionally changing a generated reference caused the check to exit 1 as expected.
- Full product Release solution build passed with 23 existing warnings (including XML documentation warnings and the SQLitePCLRaw package advisory). Initial sandbox NuGet audit access warned; the subsequent full restore/build completed.
- EF resource integration suite passed. Tag-helper suite failed at `SemanticIconTests.cs:102`: “An invalid mapping must not partially register a pack.” Reproduced the identical failure in a fresh `git archive HEAD` baseline, proving it predates this migration. No unrelated product fix was made.
- Formatted the edited generator entry point with CSharpier and passed Git whitespace checks. Existing generator limitations concerning obsolete files and missing examples remain unchanged; this step adds the existing drift check rather than broadening its behavior.

Working trees: product has the imported plugin/generator and CI/solution changes; workspace has generator deletions, solution/guidance updates, and this plan/index. The pre-existing `StellarAdmin.sln.DotSettings.user` change remains untouched. Skills and website remain clean. No publish, release, install cutover, or remote CI run was performed.

Step 1 is complete and committed locally. User requested a pause after committing. Next proposed work is step 2, moving contributor guidance; release tooling and website repository boundaries remain unchanged. Nothing has been pushed.


## Step 2 implementation — 2026-09-17

User authorized contributor-guidance migration. Product now owns `AGENTS.md`, `CLAUDE.md`, `docs/agents.md`, `docs/development.md`, repo guides, conventions, maintained designs/theme specifications, plans, and the four canonical `.agents/skills/` workflows. The import source is workspace commit `0c80b64`. Product `.claude/skills/` links to its canonical folders. Workspace `.agents/skills` forwards to that same directory for transitional sessions; existing workspace Claude links continue to resolve. No personal tool permissions or IDE settings were imported.

Workspace `AGENTS.md` and `docs/README.md` are routing/transition guides. Its release workflow, smoke script, tool manifest, solution, and settings stay in place. Website has local `AGENTS.md` and `CLAUDE.md`, a README contributor link, and a corrected exporter location. Its application, generated demos, public skill-installation instructions, and deployment configuration remain unchanged. Shared embedding/component workflows stay product-owned and are linked from the website for cross-repo work.

Maintained product instructions now use product-root-relative commands and treat `../website` as an optional sibling, with `STELLARADMIN_WEBSITE_DIR` available for other export destinations. Corrected the stale SDK reference to the actual 10.0.400 pin and replaced the obsolete claim that no test projects exist. Historical plans retain their original wording/commands, with import context and current-execution guidance in the plan index. All 52 unchanged convention and historical-plan files compared byte-for-byte with their workspace source.

Verification:

- Checked all local Markdown file links in maintained entry points, repo guides, design specifications, conventions, skills, and the plan index; all resolved. Checked root Claude imports and all product/workspace skill symlinks.
- All four development skills passed the skill-creator frontmatter validator. The relocated read-only export classifier ran successfully against the separate website checkout and found no changed generated exports.
- Fresh Codex sessions ran in isolated temporary copies of the product and website, outside the parent workspace and without sibling checkouts. Product startup injected its own AGENTS instructions and all four local development skills; website startup injected its own instructions and no product development skills. Both reported the expected verification commands and independent ordinary-development requirements. Initial sandbox client initialization failed on read-only local client state; retries with reviewed escalation succeeded, while the sessions themselves used read-only access.
- Claude imports and symlinks were checked structurally; no fresh Claude model session was run. Official Codex discovery references are maintained in `docs/agents.md`.
- Git whitespace checks passed for all three affected repositories. No application or build-input code changed, so product tests and website build were not rerun for this documentation relocation. Step 1 build/test results, including the reproduced baseline semantic-icon failure, remain recorded above.

Working trees at handoff: workspace has moved-file deletions, routing guidance, and the forwarding skill link; product has the imported contributor setup/docs and README link; website has its entry points and README update. Legacy skills repo remains clean. The pre-existing workspace `StellarAdmin.sln.DotSettings.user` change remains untouched. User authorized committing step 2 in the product, website, and workspace repositories. These changes are included in their local contributor-guidance migration commits; nothing pushed or published. Release migration has not started.
