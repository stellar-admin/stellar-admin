# Consumer skill consolidation

Status: completed — implementation, manual installation verification and CI packaging checks complete. Updated: 2026-09-18. Affected repository: product only.

## Scope and decision

The user selected Vercel's skills installer and a clean-slate distribution model, then authorized step 1 (consolidate consumer guidance into `skills/stellar-admin/`) and subsequently steps 2–3 (remove Claude packaging, update the generator and correct guidance). Internal development skills remain in `.agents/skills/`. No publishing, commits, website work, or installation into user agent directories is authorized by these steps.

## Completed

- Moved the consumer entry point and references into `skills/stellar-admin/` with the skill name `stellar-admin`.
- Converted forms, layout, and theming from separate skills into workflow reference documents, preserving their body content except for routing terminology and paths.
- Updated internal links and entry-point routing so the skill includes its own dependencies.
- Preserved generated component references and their hand-authored structure regions without content changes.

- Removed the Claude marketplace and plugin manifests and the old plugin README; moved its MIT license into the consumer skill folder.
- Updated the generator destination and maintained repository/development-skill references to `skills/stellar-admin/references/`. Marked the former workspace plan’s distribution approach as superseded while retaining its historical record.
- Corrected Core/TagHelpers registration and icon-option ownership, removed stale Pro terminology, clarified `.cshtml` support, and removed the old README’s automatic activation claim.

## Remaining steps

5. Smoke-test installations for Claude Code and Codex in temporary projects and add CI packaging checks.

The generator now works at the new path, including the existing CI drift-check command. Installation and version guidance are documented; temporary installation tests and additional CI packaging checks remain. The retired external skills marketplace was not modified; compatibility is not a requirement for this clean-slate distribution.

## Verification

- Skill Creator's `quick_validate.py skills/stellar-admin` passed.
- Checked that exactly one `SKILL.md` exists in the consumer bundle and every relative Markdown file link resolves inside it.
- Compared all 54 component files and the component index with their original Git contents: all 55 generated files are byte-identical.
- Checked that no old sibling-skill routes or Claude command references remain in the consumer bundle.
- `git diff --check` passed.

For steps 2–3, `dotnet run --project util/SkillsGenerator` regenerated 54 component files and the index at the new path, and `dotnet run --project util/SkillsGenerator -- --check` reported no drift. All 55 generated files remain byte-identical to the original Git contents, including hand-authored structure regions. The generator reports the existing absence of curated examples for FormPage and IndexPage.

Skill validation, relative-link checks, and `git diff --check` passed again. `dotnet csharpier check util/SkillsGenerator/Program.cs` passed. No legacy plugin paths or invocation commands remain outside historical plans. Installation tests and full CI were not run; these changes do not alter product runtime code.

## Product-specific layout (2026-09-17)

The user authorized a separate skill per product before installation work. Rename the UI skill to `stellar-admin-tag-helpers`, add a consumer overview, and separate generated references by source project. Preserve Dashboard material under `skills/stellar-admin-dashboard/references/` without a `SKILL.md` until useful Dashboard workflows are ready. Each future skill keeps its own references and license; the UI skill does not require the Dashboard folder. This supersedes the single-product naming above while preserving the prior verification record.

Completed: renamed the UI skill and its frontmatter, added `skills/README.md`, moved Dashboard setup notes and both component files into their own product folder, and retained an MIT license in each product folder. Updated maintained repository guidance and the component-development skill. The generator now routes by source project and writes one index per product, retaining shared source lookup for inherited types.

Verification: generation produced 54 component files and two product indexes; `--check` reported no drift. All 54 component files are byte-identical to their original Git contents (52 UI, two Dashboard), preserving hand-authored structure regions. Both indexes exactly match their product's component files. All relative Markdown file links within each product resolve inside that product folder, and only the UI folder has a `SKILL.md`. Skill Creator validation, CSharpier checks for both changed C# files, and `git diff --check` passed. No old consumer directory references remain outside historical plan entries. Installation documentation, installation smoke tests, and additional CI packaging checks remain deferred; product runtime code was not changed.

## Installation documentation (step 4, 2026-09-17)

The user authorized the next step. Added the explicit consumer-skill installation command to the root README and detailed project-scoped instructions, agent selection, local-checkout installation, and version behavior to `skills/README.md`. Added compatibility guidance to the installed skill itself and maintenance guidance for future version changes. Dashboard remains unavailable as an installable skill.

The compatibility baseline is TagHelpers 0.3.0 on .NET 10. The recorded release commit is `ddc8d3de603f80e41a37e2ac92a769f90503d3ae`; a local diff confirms Core and TagHelpers source are unchanged from that commit, and both project files target `net10.0`. Live GitHub release lookup was unavailable (API connectivity failure and web cache miss), so this is based on the repository's recorded release and source comparison, not a fresh publication check. Installer syntax and default scope were checked against Vercel's official repository README. The GitHub command requires the new layout to be published; local-checkout instructions cover the current unpublished work.

Verification: Skill Creator validation and `git diff --check` passed; relative links and heading anchors in all changed documentation resolve, and the root README UTF-8 BOM is preserved. Installation smoke tests and CI packaging checks are step 5 and were not run as part of this documentation step.

## Publication handoff (2026-09-17)

The user authorized committing and pushing the completed skill migration, product split, generator changes, and installation documentation to `origin/master`. Installation smoke tests and additional CI packaging checks remain outstanding; this commit does not claim those checks are complete.

## Installation scope follow-up (2026-09-17)

The user reported a successful installation, including default-layout changes and Bootstrap removal, and requested that consumers choose whether those changes are wanted. Updated the skill entry point and setup guide to ask for basic installation or installation with layout conversion unless the user already specified the scope. Basic installation preserves the existing layout structure and Bootstrap; layout conversion includes dependency-aware Bootstrap cleanup within the agreed scope. Replaced the unconditional framework-removal recommendation with guidance to explain coexistence concerns and check actual rendering.

Verification: Skill Creator validation, relative links and heading anchors in the changed skill documents, and `git diff --check` passed. The revised prompt behavior has not been exercised in a fresh consumer session. No runtime or generated component references changed.

## Public documentation and compatibility follow-up (2026-09-17)

The user subsequently reported successful basic and full installations in both Codex and Claude Code. The website's `content/docs/tag-helpers/agent-skills.mdx` has now been updated locally to describe Vercel installation and the setup choices instead of the retired Claude plugin. The user also approved replacing the fixed 0.3.0 compatibility claim across the website, product README, consumer overview, skill, and maintenance guide: guidance follows the default branch and agents should check the application's installed package version before applying examples. This supersedes the fixed baseline policy recorded above; historical verification remains unchanged.

## CI validation and closeout — 2026-09-18

Status: completed. The successful basic and full installations in Codex and Claude Code reported on 2026-09-17 satisfy the manual installation checkpoint. The earlier audit incorrectly carried that checkpoint forward as unfinished; no repeated installation test is required for this closeout.

Added `build/skill-validation/` with pinned npm dependencies and a lockfile. The checker discovers installable consumer skills, validates YAML name/description, license and bundled references, and checks local Markdown links/images and heading anchors within each bundle. Reference-only Dashboard material is skipped; bundle symlinks are rejected. External URLs and agent execution are outside this check. CI runs installation, regression tests and bundle validation after Node setup on each PR and push to master; generated-reference drift remains a separate existing step.

Local verification: the bundle check passed for one installable skill and 62 Markdown files; all eight regression tests passed, covering valid folded YAML/reference links/duplicate headings, invalid metadata, missing license/references, broken links/anchors, path escapes, symlink cycles and empty discovery. Formatting and Git whitespace checks passed. The workflow has not been run on GitHub for this change; no product runtime tests or repeated agent installation tests were needed. No implementation work remains in this consolidation plan.
