# StellarAdmin consumer skills

Read this guide when working on `skills/` from the product repository. Shared instructions are in [AGENTS.md](../../AGENTS.md), with conventions in [docs/conventions/](../conventions/). Paths below are relative to the product repository root.

Consumer guidance is organized by product; see the [skills overview](../../skills/README.md). It is separate from the product repository's development workflows in `.agents/skills/`. The `skills/` directory is part of this product checkout, not the retired separate skills repository.

## Product boundaries

- [stellar-admin-ui](../../skills/stellar-admin-ui/SKILL.md) teaches applications to use the UI library. Setup, forms, layout, theming, and component documentation live under its `references/` directory and are loaded as needed.
- [stellar-admin-theme](../../skills/stellar-admin-theme/SKILL.md) creates and changes theme files of CSS variables for either product: the workflow in `SKILL.md`, and `references/theme-variables.md` (the theme variable reference, description-to-theme variables guidance, Ledger, Ops and Soft as worked examples, the theme builder link format). It is handwritten: keep it in step with `src/StellarAdmin.UI/Client/css/theme-variables.json` and the theme fixtures in `util/theme-check/presets/`, and the link format with the website's theme builder. The UI skill's theming guide covers linking a theme, fonts, dark mode and tokens, and names this skill for creating one.
- [stellar-admin-setup](../../skills/stellar-admin-setup/SKILL.md) installs the admin panel: packages, registration, routes and middleware order, protection, theme and verification, with `references/troubleshooting.md`.
- [stellar-admin](../../skills/stellar-admin/SKILL.md) teaches day-to-day admin panel work: a task table, one complete EF Core resource, a checklist, namespaces, common mistakes and unsupported features in `SKILL.md`, and one reference per topic under `references/` (resources, index pages, columns, forms, inline editing, fields, editors, relationships, view pages, sidebar, authorization, customization). It is handwritten from the public API in `src/StellarAdmin` and `src/StellarAdmin.EntityFrameworkCore`; update it with API changes. Its common mistakes and unsupported features come from the evaluation tasks in `build/skill-evals/admin-panel/` and the [admin panel backlog](../plans/admin-panel-backlog.md); rerun the tasks after substantial changes.
- Keep each skill self-contained with its own license and references. Do not link to sibling files as required dependencies. Admin panel consumers customizing UI markup can also use the UI skill; avoid duplicating the UI catalog.

The Claude plugin manifests and installation README have been removed. Installation instructions and compatibility guidance live in the [consumer overview](../../skills/README.md). Installation verification and CI closeout are recorded in [consumer skill consolidation](../plans/archive/consumer-skill-consolidation.md).

## Source ownership and validation

- `skills/stellar-admin-ui/references/components/*.md` and its `components-index.md` are generated only from `StellarAdmin.UI`.
- `util/SkillsGenerator` generates the UI catalog with curated samples. The admin panel's page tag helpers (`sa-index-page`, `sa-form-page`, `sa-view-page`) are documented by hand in the stellar-admin skill's `references/customization.md`, not generated.
- Component regions between `<!-- structure:begin -->` and `<!-- structure:end -->` are hand-authored and preserved by generation.
- Other reference guides and `SKILL.md` files are handwritten. Keep them consistent with the website and actual public APIs. Forms, layout, and theming remain reference documents within the UI skill.

Consumer bundle validation runs in CI through `build/skill-validation/check.mjs`. Run `npm ci --prefix build/skill-validation --ignore-scripts --no-audit --no-fund`, `npm test --prefix build/skill-validation`, and `npm run check --prefix build/skill-validation` locally. It checks metadata, licenses, bundled references, relative Markdown links and heading anchors within each installable skill. Reference-only product folders are skipped. See [validator guidance](../../build/skill-validation/README.md).

Run `dotnet run --project util/SkillsGenerator` and then `dotnet run --project util/SkillsGenerator -- --check` from the product repository. No sibling checkouts are needed. Validate skill frontmatter, relative links, product ownership, and referenced tag names after changes. Do not promise automatic activation solely from file extensions.

## Compatibility maintenance

Maintain the skill alongside the library without a fixed package-version claim. Keep consumer guidance clear that default-branch references may include changes newer than an application's installed NuGet package, and instruct agents to check that version before applying API examples. The installer does not automatically match a consumer's NuGet version. Do not advertise an old release tag as a skill source unless that tag contains the product-specific skill layout.
