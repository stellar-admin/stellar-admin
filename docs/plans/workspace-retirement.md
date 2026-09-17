# Workspace and skills consolidation

Status: repository consolidation, archival, and old NuGet trust policy cleanup complete. Product 0.3.0 release succeeded. Updated: 2026-09-17.

Affected repositories: workspace, stellar-admin, skills, and website (documentation and development guidance).

Consumer distribution update: the plugin packaging and compatibility approach below are historical and have been superseded by [consumer skill consolidation](consumer-skill-consolidation.md). The current consumer source is `skills/`.

## Recommendation

Make `stellar-admin/stellar-admin` the self-contained product and contributor repository. Move consumer skills, their generator, release tooling, and maintained product development guidance into it. Keep `website` independent, as required by the user. Retire the workspace as an active dependency; retain the old repositories for history and installation compatibility until the transition is proven.

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
2. Move contributor guidance, maintained designs, theme specifications, development skills, and plans into the product. Give the website a standalone contributor entry point and validate discovery without workspace/sibling checkouts. Committed as product `1c638cc`, website `c8ba7b7b`, and workspace `c8b7781`; see verification below.
3. Move release tooling and prove a complete dry run in the product repo. Completed in product `26b039d` and workspace `11288e9`; strict hosted dry run `35185318228` passed.
4. Switch skills distribution and publishing: update website installation instructions, test a compatibility marketplace, configure NuGet trust/environment settings, disable old publishing, then use the new workflow for the next authorized release.
5. Retire the workspace after successful release verification; retain history and the compatibility endpoint as needed. Validate cross-repo export with separate checkouts.

Step 4 is now authorized; see the current implementation and external-configuration status below. Commit and push its code changes only when requested. The next actual package release remains separately authorized. The website remains a separate repository throughout.

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


## Step 3 implementation — 2026-09-17

User authorized the release-tooling migration and investigation of the known test failure. Product now owns a manual `.github/workflows/release.yml`, individual orchestration steps in that workflow, the copied consumer smoke script, and the NuGet validator merged into its tool manifest alongside CSharpier. The source tooling was copied from workspace commit `c8b7781`; the workspace workflow and tool manifest remain unchanged. Its smoke script receives only the same one-line icon API correction needed to verify current product versions. Website and legacy skills are unchanged.

The product workflow deliberately exposes verification only until step 4. It checks out the dispatched product SHA without a GitHub App, uses root `global.json`, records the built SHA, and uploads verified packages with read-only repository permissions. Publishing, OIDC, tags, and releases remain in the workspace. This staged approach avoids introducing two active publishing entry points before external trust and permissions are configured.

The release workflow uses strict SemVer input validation, locked npm installs for all five solution client projects, a serial Release solution build, all five packages and symbols, package validation, both executable test suites, consumer reference drift checking, and a temporary consumer app. The smoke check copies the SDK pin and uses an isolated NuGet cache with source mapping to require the newly packed StellarAdmin packages. Release builds retain the existing `AllowMissingPrunePackageData` property.

Three issues uncovered by the release checks were fixed:

- `IconOptions.AddIconPack` now validates all semantic mappings before changing existing icons or mappings, and does not read mappings when import is disabled. The existing baseline failure now passes. Added coverage for preserving an existing icon and mapping when a later mapping is invalid, and for a pack whose mapping accessor must not be called.
- SkillsGenerator previously used `CallerFilePath` to locate its checkout; deterministic release builds rewrite that path to `/_/`, causing a runtime directory failure. It now locates the product root by walking up from its executable and checking the solution and generator manifest.
- The copied consumer smoke test still called the removed `IconOptions.Icons` property. It now uses `TryGetIcon`; the workspace copy gets the same one-line fix so it remains compatible during transition.

Strict package validation reached Source Link checks and failed with rule 119 because source URLs reference unpushed commit `1c638cc57adb7d65566801bd80d68e2f52aee781` and GitHub returns 404. An initial local verification script excluded only source URL reachability. That script and its bypass option were removed following user review; the workflow always uses strict validation. Local success is provisional until an authorized push and strict GitHub run confirm public sources, runner setup, and artifact upload.

See [release verification and cutover](../../build/README.md) for commands and the step 4 requirements: live tags/versions, product environment and NuGet policy, tag/release permissions and downstream events, serialized publishing, same-version/SHA/artifact recovery, the preserved Core/TagHelpers allowlist, and disabling the workspace publisher. External settings have not been changed or inspected during this step.


Verification before the workflow structure revision (historical evidence; the local orchestration script is no longer shipped):

- Ran the initial shared script end to end in `/tmp/stellar-release-kf__tfmz/product`, an independent clone with the working changes overlaid, no sibling repositories, and the canonical product remote URL. Used .NET 10.0.400 and temporary Node 20.20.2, matching the workflow's Node major. Command: `build/release.sh --local 0.2.1-migration.1 /tmp/stellar-release-kf__tfmz/final-packages`. Exit 0; log: `/tmp/stellar-release-final.log`. Version is a local test identifier, not a release decision.
- Built the complete Release solution, produced five nupkgs and five snupkgs, and validated all five packages with only rule 119 excluded. Inspected package metadata and verified every internal StellarAdmin dependency uses the requested version. Existing SQLitePCLRaw 2.1.11 vulnerability warnings remain; clean compilation also reports existing XML documentation/nullability warnings. No dependency versions were changed.
- Both executable suites passed, including the original semantic-icon failure and new preservation/disabled-accessor cases. Consumer references passed after deterministic compilation and again when invoking the built generator DLL from unrelated working directory `/tmp`.
- Fresh consumer restore/build passed with zero warnings/errors, using all five local packages. Verified form/icon registration, rendered alert/button/data-grid markup, and all four expected CSS/JS assets on temporary port 5299. The script removed its consumer directory and stopped its server.
- Actionlint 1.7.12 passed for release and CI workflows; Bash syntax and Git whitespace checks passed. Exercised valid/invalid SemVer inputs, refusal to overwrite a nonempty output directory, and rejection of `--local` under both CI flags. Confirmed the standalone test's release scripts, workflow, tool manifest, and changed C# files match the working tree.
- Initial sandbox network restrictions required reviewed escalation for downloads, package validation, and fresh consumer restore. A subsequent forced restore with network access succeeded. Strict Source Link validation still requires an authorized push; no remote workflow was dispatched and artifact upload, NuGet trust, and publish permissions remain unverified.

Working trees at handoff: product contains release tooling, the three verification fixes, and updated guidance/plan; workspace contains only the smoke-script API fix plus Jerrie's pre-existing `StellarAdmin.sln.DotSettings.user` edit, which was preserved. Website and legacy skills remain clean. Nothing committed, pushed, tagged, published, or deployed. Next: review/commit when requested, then push and run the strict hosted dry run before step 4 cutover.


### Workflow structure revision

The user requested individual GitHub Actions steps and explicitly does not need local release verification. Removed `build/release.sh` and its local validation bypass. The workflow now directly owns separate version validation, restore, dependency installation, build, pack, package validation, tag-helper tests, resource integration tests, reference checks, smoke test, and artifact upload steps. The existing consumer smoke test remains a script. Updated contributor and release guidance to direct execution through GitHub Actions.

Validated the revised workflow with Actionlint and Bash syntax checks for every inline command. No builds or local release runs were repeated for this orchestration-only revision. A hosted dry run remains pending an authorized commit/push. Nothing committed or pushed.


## Step 4 implementation — 2026-09-17

The user confirmed the publish allowlist remains Core and TagHelpers and authorized step 4. Verified the strict hosted dry run [35185318228](https://github.com/stellar-admin/stellar-admin/actions/runs/35185318228): success at product SHA `26b039d5ab5d664a0a6640762f078f0750ae208f`, version `0.2.1-preview.1`. The earlier run with a trailing dot in its version failed as expected. Product and workspace tags match through `v0.2.0`; NuGet lists TagHelpers through 0.2.0 and Core's version index returns 404, so the new trust policy must allow first publication of Core as well as new versions.

### Release implementation

The product workflow retains individual GitHub Actions steps and adds an opt-in `publish` input. Dry runs still build/validate all five packages with read-only permissions and no writes. The publish job uses only the built SHA and immutable artifact ID, root SDK pin, product `GITHUB_TOKEN`, and `nuget-org` OIDC authentication. Its only published packages and release assets are Core and TagHelpers plus their symbols. No standalone release orchestration script or local validation bypass was reintroduced.

Whole publishing runs are serialized without canceling an active run. The preflight compares SemVer precedence against tags and both NuGet version indexes, reserves a draft release, and binds it to the workflow run ID, source SHA, and artifact ID. It tags only the built product commit. Recovery uses **Re-run failed jobs** in the same run, skipping already accepted nupkgs and snupkgs independently and completing asset upload/release publication. Another run/artifact, conflicting tag, or a later published version blocks recovery. The original 30-day artifact must remain available; there is no automatic rebuild fallback.

The workspace workflow is replaced locally with a retirement notice. On GitHub, workflow ID `337429987` was disabled after confirming no unfinished release runs; API state is `disabled_manually`. This prevents the old publisher from running before the new code is pushed. Its old App secrets and NuGet policy remain intact for later retirement; no credentials were deleted.

### Live configuration

- Product had no environment, repository secrets, branch protection, or rulesets. Its default workflow token permissions allow writes, so the explicit publish-job permissions can use `GITHUB_TOKEN`. Inspected product workflows have no tag/release-dependent downstream automation; the VRT comment workflow only follows VRT runs.
- Created product `nuget-org` environment (ID `22117682589`) with the same existing workspace protections: no reviewers, timer, or branch restrictions.
- Created repository variable `RELEASE_PUBLISHING_ENABLED=false`. The workflow refuses publication until the variable is true and `NUGET_USER` is populated.
- Requested the NuGet username and policy configuration from the user. The old username is an environment secret whose value GitHub cannot disclose, and no authenticated NuGet management connection is available. Required trust: owner `stellar-admin`, repository `stellar-admin`, workflow `release.yml`, environment `nuget-org`, package scope limited to Core and TagHelpers with permission for new package creation and new versions.
- After the user supplies the username and confirms trust, set the product environment secret, recheck the workspace workflow is disabled, then enable the repository publishing gate. OIDC exchange and package publication remain untested until the next separately authorized release.

### Skills distribution

Product plugin version advances from 0.1.0 to 0.1.1 to refresh migrated content. Marketplace and plugin names remain `stellar-admin`. Product README, plugin README, and website installation instructions use `stellar-admin/stellar-admin`. Existing installations can update their existing marketplace and plugin; manually copied skills/symlinks need the product path. Refresh instructions use `/reload-plugins`; website wording no longer promises unconditional file-extension activation.

The old skills marketplace now points at `https://github.com/stellar-admin/stellar-admin.git`, branch `master`, subdirectory `plugins/stellar-admin`, using `git-subdir`. Its old `plugins/` tree is left frozen for legacy local paths and older clients; it is no longer referenced by the marketplace or regenerated. The old README directs new work and installations to the product. Deploy these source changes by pushing the product before the compatibility marketplace and website updates.

### Verification and remaining work

- Actionlint 1.7.12 passed for product release/CI and the workspace retirement workflow; all inline Bash steps passed syntax checks. Sixteen mocked preflight scenarios passed, including SemVer ordering, first publication, same-artifact recovery, conflicting artifacts/tags, invalid versions, later versions, and API permission errors. These checks execute the actual inline preflight code with mocked APIs; no release or tag was created.
- Claude Code 2.1.251 validated both marketplace manifests and the plugin manifest. Fresh isolated installs from the public product marketplace and the compatibility manifest both succeeded. A separate local Git fixture proved update from the original 0.1.0 marketplace to forwarded 0.1.1 content; that fixture substituted a local source URL for the not-yet-pushed plugin version. Both versions retained all four sibling skills and 60 valid relative links. Temporary `CLAUDE_CONFIG_DIR` directories isolated every test from the user's plugin settings.
- The public product marketplace checkout measured 33 MB; the installed plugin was 528 KB. This is the anticipated checkout-size tradeoff, with no extra distribution service introduced.
- Website lint, type checking, and production build passed; built route output contains the updated installation section. Initial sandbox checks could not open package-manager state, so approved escalation was used. No website application or generated demos changed.
- No package build was repeated for step 4; the hosted dry-run result is the build evidence. No hosted publish job, OIDC login, package push, release, or deployment was run.

The user authorized committing and pushing all four repositories with NuGet configuration still pending. Push the product before the compatibility marketplace and website so their new source is available. GitHub settings remain as listed above: the workspace publisher is disabled and the product publishing gate is false. After the push, configure the NuGet policy and product environment secret, then enable the gate. The first actual publication remains a separate release action; workspace retirement remains step 5.


## Step 5 retirement — 2026-09-17

Verified successful product release [35188566992](https://github.com/stellar-admin/stellar-admin/actions/runs/35188566992) at `ddc8d3de603f80e41a37e2ac92a769f90503d3ae`. Both build and publish jobs passed, including NuGet OIDC login and package/symbol pushes. The public [v0.3.0 release](https://github.com/stellar-admin/stellar-admin/releases/tag/v0.3.0) contains exactly the Core and TagHelpers nupkgs and snupkgs. This completes the hosted publishing checkpoint.

The user authorized retirement. Searches of active product, website, and compatibility repository content found no remaining Pro/workspace remote dependencies or release App references; historical plans/design records were excluded. `stellar-admin-pro` was already archived and remains private. Archived `workspace` and `skills`; skills remains public with its committed product marketplace forwarding intact. The website remains separate and active. Local checkouts were retained.

Removed workspace repository secrets `NUGET_USER`, `RELEASE_APP_ID`, and `RELEASE_APP_PRIVATE_KEY`; verified zero remaining repository secrets. Its `nuget-org` environment had no secrets, and the retired release workflow was disabled. These removals delete workspace-held credentials, not the GitHub App itself or any globally valid private key.

The user confirmed deletion of the old workspace NuGet trusted publishing policy on 2026-09-17. This completes the NuGet account cleanup; confirmation is user-reported because no authenticated NuGet management access is available here. The consolidation is complete. The old release GitHub App remains intact; review its other consumers before any separate key revocation or App removal.
