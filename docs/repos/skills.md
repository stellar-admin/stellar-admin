# StellarAdmin consumer skills

Read this guide when working on `skills/` from the product repository. Shared instructions are in [AGENTS.md](../../AGENTS.md), with conventions in [docs/conventions/](../conventions/). Paths below are relative to the product repository root.

Consumer guidance is organized by product; see the [skills overview](../../skills/README.md). It is separate from the product repository's development workflows in `.agents/skills/`. The `skills/` directory is part of this product checkout, not the retired separate skills repository.

## Product boundaries

- [stellar-admin-tag-helpers](../../skills/stellar-admin-tag-helpers/SKILL.md) teaches applications to use the UI library. Setup, forms, layout, theming, and component documentation live under its `references/` directory and are loaded as needed.
- `skills/stellar-admin-dashboard/` preserves Dashboard component references and setup notes. It has no `SKILL.md` and is not an installable skill yet. Add its entry point when useful admin-panel workflows are ready to document.
- Keep each skill self-contained with its own license and references. Do not link to sibling files as required dependencies. Dashboard consumers customizing UI markup can also use the Tag Helpers skill; avoid duplicating the UI catalog.

The Claude plugin manifests and installation README have been removed. Installation instructions and compatibility guidance live in the [consumer overview](../../skills/README.md). Installation verification and CI closeout are recorded in [consumer skill consolidation](../plans/archive/consumer-skill-consolidation.md).

## Source ownership and validation

- `skills/stellar-admin-tag-helpers/references/components/*.md` and its `components-index.md` are generated only from `StellarAdmin.TagHelpers`.
- `skills/stellar-admin-dashboard/references/components/*.md` and its `components-index.md` are generated only from `StellarAdmin.Dashboard`. These currently cover FormPage and IndexPage.
- `util/SkillsGenerator` generates both product catalogs with curated samples. It retains shared source lookup for inherited attributes and enum values, while routing output by the component's source project.
- Component regions between `<!-- structure:begin -->` and `<!-- structure:end -->` are hand-authored and preserved by generation.
- Other reference guides and `SKILL.md` files are handwritten. Keep them consistent with the website and actual public APIs. Forms, layout, and theming remain reference documents within the UI skill.

Consumer bundle validation runs in CI through `build/skill-validation/check.mjs`. Run `npm ci --prefix build/skill-validation --ignore-scripts --no-audit --no-fund`, `npm test --prefix build/skill-validation`, and `npm run check --prefix build/skill-validation` locally. It checks metadata, licenses, bundled references, relative Markdown links and heading anchors within each installable skill. Reference-only product folders are skipped. See [validator guidance](../../build/skill-validation/README.md).

Run `dotnet run --project util/SkillsGenerator` and then `dotnet run --project util/SkillsGenerator -- --check` from the product repository. No sibling checkouts are needed. Validate skill frontmatter, relative links, product ownership, and referenced tag names after changes. Do not promise automatic activation solely from file extensions.

## Compatibility maintenance

Maintain the skill alongside the library without a fixed package-version claim. Keep consumer guidance clear that default-branch references may include changes newer than an application's installed NuGet package, and instruct agents to check that version before applying API examples. The installer does not automatically match a consumer's NuGet version. Do not advertise an old release tag as a skill source unless that tag contains the product-specific skill layout.
