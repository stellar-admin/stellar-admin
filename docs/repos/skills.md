# StellarAdmin consumer skills

Read this guide when working on `plugins/stellar-admin/` from the product repository. Shared instructions are in [AGENTS.md](../../AGENTS.md), with conventions in [docs/conventions](../conventions/). Paths below are relative to the product repository root.

These MIT consumer skills live alongside the product and teach agents how to use StellarAdmin. The separate `skills/` repo is a compatibility marketplace forwarding to this plugin; its old content is frozen, so do not regenerate into it. These are consumer skills, separate from the product repository's component-development workflows.

Read [plugin README](../../plugins/stellar-admin/README.md) for installation and packaging. Keep one copy of each skill's content; agent-specific packaging must preserve sibling reference links. The current Claude marketplace lives in `.claude-plugin/` and `plugins/*/.claude-plugin/`; those manifests are not universal plugin manifests.

## Source ownership and validation

- `plugins/stellar-admin/skills/tag-helpers/references/components/*.md` and `references/components-index.md` are generated from tag helpers in `StellarAdmin.TagHelpers` and `StellarAdmin.Dashboard`, with curated samples by `util/SkillsGenerator`.
- Component regions between `<!-- structure:begin -->` and `<!-- structure:end -->` are hand-authored and preserved by generation.
- Other reference guides and `SKILL.md` files are handwritten. Keep them consistent with the website and actual public APIs.
- Preserve the `tag-helpers`, `forms`, `layout`, and `theming` sibling layout; skills link to each other's files. Use relative file links for cross-agent discovery, not plugin command names as the only reference.

For generated-reference changes, run `dotnet run --project util/SkillsGenerator` and then `dotnet run --project util/SkillsGenerator -- --check` from the product repository. No sibling checkouts are needed for generation. Validate skill frontmatter, relative links, and referenced tag names after handwritten changes. Do not promise automatic activation solely from file extensions.
