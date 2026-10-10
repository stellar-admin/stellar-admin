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

When the user asks a question, answer it as a question. Do not treat a question as a command.

Keep actions within the operation the user requested. Do not add adjacent work, routine checks, documentation edits, cleanup, or database inspection just because they seem useful. Read-only inspection needed to answer a question or carry out an action is allowed, but a question does not authorize changes. Ask before taking a genuinely necessary step outside the requested scope; do not treat an optional step as necessary.

For a commit-only request, inspect Git status, stage the requested work, review the staged file list, commit, and report the hash. Include a modified tracked `sandbox/DashboardPlayground/app.db` in the commit unless the user explicitly excludes it. Do not run tests, query the database, change files, remove SQLite journal files, or add documentation as part of a commit-only request unless explicitly asked.

Treat entity property configuration, migration generation, and database updates as separate steps. When adding or changing entity properties, show the types, nullability, required rules, lengths, and defaults for review. Do not generate or remove migrations, edit migration snapshots, or update a database unless the user explicitly requests that specific step. Authorization to generate a migration does not authorize applying it.

Inspect Git status before changing files. For cross-repo work, inspect and report status separately for the product and website. Concurrent agents should use a separate worktree in each affected repository; coordinate generated outputs and ports. Stop only processes you started; port 5205 belongs to Jerrie.

Use the pinned SDK and checked-in package manager lockfiles. Keep package boundaries and public API names unless the current task explicitly changes them. Record substantial work and actual verification in the relevant plan; distinguish completed checks from historical results and outstanding work.

## Conventions

- [Options builders](docs/conventions/options-builders.md): consult before designing or extending public configuration APIs.
- [C# file organization](docs/conventions/csharp-file-organization.md): member ordering, braces, and logical spacing; do not reorder untouched files.
- [Unit testing](docs/conventions/unit-testing.md): required for new and migrated .NET tests; mirror the SUT's project and folders, use TUnit, and follow explicit arrange–act–assert sections.
- [XML documentation](docs/conventions/xml-documentation.md): brief consumer-facing documentation; internal types stay uncommented.
- [Markdown](docs/conventions/markdown.md): one line per prose paragraph in Markdown/MDX; do not hard-wrap prose or reflow untouched text.

## Development skills

Canonical development skills live in `.agents/skills/`; `.claude/skills/` contains relative links to them. These help build StellarAdmin. The separately packaged consumer skills in `skills/` teach applications how to use it; do not mix their audiences or discovery paths.

- [create-custom-theme](.agents/skills/create-custom-theme/SKILL.md): independent theme design, implementation, and coverage using maintained specifications.
- [prototype-component](.agents/skills/prototype-component/SKILL.md): visual exploration for new components before extraction.
- [port-shadcn-component](.agents/skills/port-shadcn-component/SKILL.md): port upstream components through library, samples, website, and consumer references.
- [embed-website-components](.agents/skills/embed-website-components/SKILL.md): shared workflow for generated inline website examples.

See [agent setup](docs/agents.md) for discovery and handoffs. Theme knobs and tokens are specified in [docs/design/tokens.md](docs/design/tokens.md).

## Separate website and release transition

Website-only work starts in the website repository and follows its own `AGENTS.md`. For component documentation/export tasks, read [website integration](docs/repos/website.md). A sibling `../website` checkout is a convenience; `STELLARADMIN_WEBSITE_DIR` selects another destination. Product-only work needs neither checkout.

Release verification now runs here through the [manual release workflow](.github/workflows/release.yml), with individual GitHub Actions steps. Opt-in publishing is limited to Core and TagHelpers and requires the configured `nuget-org` environment and publishing gate. Consumer skill source lives in `skills/`; Claude plugin packaging has been removed. See the [consumer skill consolidation plan](docs/plans/archive/consumer-skill-consolidation.md) for distribution work and the [workspace consolidation plan](docs/plans/archive/workspace-retirement.md) for historical context.
