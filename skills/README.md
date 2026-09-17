# StellarAdmin consumer skills

StellarAdmin has two products, with a separate consumer skill planned for each:

| Product | Skill directory | Status |
| --- | --- | --- |
| Tag Helpers — UI library for ASP.NET Core MVC and Razor Pages | [stellar-admin-tag-helpers](stellar-admin-tag-helpers/SKILL.md) | Available: setup, components, forms, layout, and theming. |
| Dashboard — integrated admin panel | `stellar-admin-dashboard/` | Planned: existing [component references](stellar-admin-dashboard/references/components-index.md) and [setup notes](stellar-admin-dashboard/references/setup.md) are preserved here, but there is no installable skill yet. |

## Install the Tag Helpers skill

With Node.js/npm and Git available, run this from your application's project directory:

```bash
npx skills add stellar-admin/stellar-admin --skill stellar-admin-tag-helpers
```

Choose your agent in the installer. Installation is project-scoped by default. Keep `--skill stellar-admin-tag-helpers` to select the consumer skill explicitly; this repository also contains internal development skills.

To target a specific agent, append `--agent claude-code`, `--agent codex`, or another supported agent identifier. Add `--global` only if you want the skill available across projects. See [Vercel's installer documentation](https://github.com/vercel-labs/skills#readme) for supported agents and options.

This installs agent guidance and its bundled references. Install and configure the NuGet package separately using the [Tag Helpers setup guide](stellar-admin-tag-helpers/references/setup.md).

For an unpublished local checkout, use its absolute path instead of the GitHub source:

```bash
npx skills add /path/to/stellar-admin/skills/stellar-admin-tag-helpers --skill stellar-admin-tag-helpers
```

## Version compatibility

The Tag Helpers skill is maintained alongside StellarAdmin. Its references follow the repository's default branch and may include changes newer than your installed NuGet package. The skill instructs the agent to check your installed version before applying API examples; this [version guidance](stellar-admin-tag-helpers/SKILL.md#version-compatibility) travels with the installed references.

The installer does not select guidance based on your installed NuGet version or update your packages. Dashboard remains work in progress and has no installable skill yet.

## Layout and maintenance

Each skill keeps its own `SKILL.md`, license, and supporting references inside its directory. Dashboard guidance will cover admin-panel setup, resources, authentication, and customization when those workflows are ready to document. Applications customizing Dashboard UI markup can also use the Tag Helpers skill; Dashboard guidance should not duplicate the UI library reference.

These are consumer skills for applications using StellarAdmin. Repository development skills live separately in `.agents/skills/`. See the [maintenance guide](../docs/repos/skills.md) for generation and ownership.
