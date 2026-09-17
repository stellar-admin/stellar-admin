# StellarAdmin contributor guidance

This repository contains the complete MIT-licensed product, samples, tests, generators, consumer skills, and product development guidance. Start product work here; the workspace repository is not required to build, test, or regenerate consumer references. The website remains a separate repository.

## Read before working

- [Product development guide](docs/repos/stellar-admin.md): library architecture, component conventions, client assets, samples, and tests.
- [Development and verification](docs/development.md): setup, commands, file encoding, and local environment notes.
- [Consumer skills guide](docs/repos/skills.md): ownership and regeneration of the shipped agent guidance.
- [Plan index](docs/plans/README.md): maintained work and historical context. Old plans do not authorize new work.

Paths and commands in maintained guidance are relative to this repository root unless another working directory is stated.

## Working agreement

Follow the user's current scope and authorization. Preserve existing edits. Do not commit, push, publish, or deploy unless authorized in the conversation. Skills and historical plans do not expand task scope or override current instructions.

Inspect Git status before changing files. For cross-repo work, inspect and report status separately for the product and website. Concurrent agents should use a separate worktree in each affected repository; coordinate generated outputs and ports. Stop only processes you started; port 5205 belongs to Jerrie.

Use the pinned SDK and checked-in package manager lockfiles. Keep package boundaries and public API names unless the current task explicitly changes them. Record substantial work and actual verification in the relevant plan; distinguish completed checks from historical results and outstanding work.

## Conventions

- [Options builders](docs/conventions/options-builders.md): consult before designing or extending public configuration APIs.
- [C# file organization](docs/conventions/csharp-file-organization.md): member ordering, braces, and logical spacing; do not reorder untouched files.
- [XML documentation](docs/conventions/xml-documentation.md): brief consumer-facing documentation; internal types stay uncommented.
- [Markdown](docs/conventions/markdown.md): one line per prose paragraph in Markdown/MDX; do not hard-wrap prose or reflow untouched text.

## Development skills

Canonical development skills live in `.agents/skills/`; `.claude/skills/` contains relative links to them. These help build StellarAdmin. The separately packaged consumer skills in `plugins/stellar-admin/skills/` teach applications how to use it; do not mix their audiences or discovery paths.

- [create-custom-theme](.agents/skills/create-custom-theme/SKILL.md): independent theme design, implementation, and coverage using maintained specifications.
- [prototype-component](.agents/skills/prototype-component/SKILL.md): visual exploration for new components before extraction.
- [port-shadcn-component](.agents/skills/port-shadcn-component/SKILL.md): port upstream components through library, samples, website, and consumer references.
- [embed-website-components](.agents/skills/embed-website-components/SKILL.md): shared workflow for generated inline website examples.

See [agent setup](docs/agents.md) for discovery and handoffs. Maintained theme specifications live in [docs/design/themes](docs/design/themes/).

## Separate website and release transition

Website-only work starts in the website repository and follows its own `AGENTS.md`. For component documentation/export tasks, read [website integration](docs/repos/website.md). A sibling `../website` checkout is a convenience; `STELLARADMIN_WEBSITE_DIR` selects another destination. Product-only work needs neither checkout.

Release verification now runs here through the [manual release workflow](.github/workflows/release.yml), with individual GitHub Actions steps. Publishing remains in `stellar-admin/workspace` until the separate cutover. Consumer skill source lives here, but the old skills marketplace remains the distribution endpoint until its separate cutover. See the [consolidation plan](docs/plans/workspace-retirement.md); do not advance either cutover as part of unrelated work.
