# StellarAdmin agent skills

Agent skills that teach an AI coding agent how to build UIs with [StellarAdmin](https://www.stellaradmin.com) — the component catalog, the library's conventions, and task workflows for forms, layout, and theming.

With the skills installed, an agent knows the `<sa-*>` tag names, their attributes and enum values, and the composition rules, so it writes correct markup instead of guessing.

The product repository also contains the Claude Code **plugin marketplace**. Paths below are relative to the product repository root.

## Installation

From within Claude Code:

```text
/plugin marketplace add stellar-admin/stellar-admin
/plugin install stellar-admin@stellar-admin
/reload-plugins
```

### Existing Claude installations

The old `stellar-admin/skills` address is a compatibility marketplace pointing to this plugin with a `git-subdir` source. Keep the existing registration and run:

```text
/plugin marketplace update stellar-admin
/plugin update stellar-admin@stellar-admin
/reload-plugins
```

Forwarding is tested with Claude Code 2.1.251. If an older client rejects `git-subdir`, update Claude Code or register `stellar-admin/stellar-admin` instead. The plugin and marketplace identifiers remain unchanged. Plugin version 0.1.1 refreshes the migrated content; bump the plugin version whenever shipping skill changes. It is independent of the NuGet version.

### Codex and other agents

For Codex, copy the four sibling folders under `plugins/stellar-admin/skills/` (`tag-helpers`, `forms`, `layout`, and `theming`) into your project's `.agents/skills/` or your user-level `~/.agents/skills/`. Keep the folders together, including all `references/`, because the task skills use relative links to `tag-helpers`. Inspect existing destinations first; do not overwrite another skill with the same name. Local directory symlinks are also supported by Codex.

This installs skill folders; it does not install the Claude marketplace manifest as a Codex plugin. See [Codex skill discovery](https://learn.chatgpt.com/docs/build-skills). For an agent without native skill support, ask it to read `plugins/stellar-admin/skills/tag-helpers/SKILL.md` and the relevant companion workflow directly. Configure any automatic discovery using that agent's own settings.

Names such as `stellar-admin:forms` below are Claude plugin invocation names. The portable file is `plugins/stellar-admin/skills/forms/SKILL.md`; its declared skill name is `forms`. No agent-independent file-extension activation is guaranteed.

## What's included

The `stellar-admin` plugin covers the free, open-source [`StellarAdmin.TagHelpers`](https://www.nuget.org/packages/StellarAdmin.TagHelpers) package:

| Skill | What it does |
|-------|--------------|
| `stellar-admin:tag-helpers` | The anchor skill. Setup, the full component catalog, and the library's conventions. Use when editing StellarAdmin Razor markup; automatic selection depends on the agent. |
| `stellar-admin:forms` | Fields, validation, model binding, and the input family. |
| `stellar-admin:layout` | Page shells — app header, page header, page container, sidebar dashboards, cards. |
| `stellar-admin:theming` | Theme stylesheets, dark mode, design tokens, and menu appearance. |

All StellarAdmin components share the `stellar-admin` plugin. The integrated admin application and its Identity and EF Core integrations use the `StellarAdmin.Dashboard` package family.

## Layout

```
.claude-plugin/marketplace.json     the marketplace manifest
plugins/stellar-admin/              the OSS plugin
    .claude-plugin/plugin.json
    skills/
        tag-helpers/                anchor skill + references/
        forms/  layout/  theming/   task skills
```

## Regenerating the component reference

Everything under `plugins/stellar-admin/skills/tag-helpers/references/components/` and `components-index.md` is **generated** — do not hand-edit it. The source of truth is the tag helper C# in the `stellar-admin` repo (`[HtmlTargetElement]` attributes, bound properties, XML doc comments, enum members) plus curated example snippets from the DocsSamples app.

The generator and its source inputs live in the product repository; no sibling checkouts are required:

```bash
# From the product repository root:
dotnet run --project util/SkillsGenerator             # regenerate
dotnet run --project util/SkillsGenerator -- --check  # fail on drift
```

Generated files carry `generated: true` in their frontmatter. One escape hatch exists: a region delimited by `<!-- structure:begin -->` / `<!-- structure:end -->` in a component file is hand-authored and preserved verbatim across regeneration (used for the required-structure trees on composite components such as Sidebar and Sheet).

Every other skill file is hand-written and kept in sync with the documentation at <https://www.stellaradmin.com/docs/tag-helpers>.

## License

MIT — see [LICENSE](LICENSE).
