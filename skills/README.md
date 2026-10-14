# StellarAdmin consumer skills

StellarAdmin has two products, each with its own consumer skills, plus a theme skill shared by both:

| Product | Skill directory | Status |
| --- | --- | --- |
| StellarAdmin.UI — UI library for ASP.NET Core MVC and Razor Pages | [stellar-admin-ui](stellar-admin-ui/SKILL.md) | Available: setup, components, forms, layout, and theming. |
| Themes — theme files of CSS variables, for either product | [stellar-admin-theme](stellar-admin-theme/SKILL.md) | Available: writing a theme from a description or brand colours, the theme variable reference, worked examples, and theme builder links. |
| StellarAdmin — integrated admin panel | [stellar-admin-setup](stellar-admin-setup/SKILL.md) | Available: installing the admin panel (packages, registration, routes, protection, theme) and checking that it works. |
| StellarAdmin — integrated admin panel | [stellar-admin](stellar-admin/SKILL.md) | Available: resources, index pages, columns, forms, fields, editors, relationships, view pages, sidebar, authorization and customization. |

## Install the UI skill

With Node.js/npm and Git available, run this from your application's project directory:

```bash
npx skills add stellar-admin/stellar-admin --skill stellar-admin-ui
```

Choose your agent in the installer. Installation is project-scoped by default. Keep `--skill stellar-admin-ui` to select the consumer skill explicitly; this repository also contains internal development skills.

To target a specific agent, append `--agent claude-code`, `--agent codex`, or another supported agent identifier. Add `--global` only if you want the skill available across projects. See [Vercel's installer documentation](https://github.com/vercel-labs/skills#readme) for supported agents and options.

This installs agent guidance and its bundled references. Install and configure the NuGet package separately using the [UI setup guide](stellar-admin-ui/references/setup.md).

## Install the theme skill

The theme skill creates and changes themes: ask for a look ("warm, editorial, dense, green accent") or invoke it by name. Install it the same way, alongside the UI skill or on its own:

```bash
npx skills add stellar-admin/stellar-admin --skill stellar-admin-theme
```

For an unpublished local checkout, use its absolute path instead of the GitHub source:

```bash
npx skills add /path/to/stellar-admin/skills/stellar-admin-ui --skill stellar-admin-ui
```

## Install the admin panel skills

Install both admin panel skills to add StellarAdmin to an application and build with it:

```bash
npx skills add stellar-admin/stellar-admin --skill stellar-admin-setup
npx skills add stellar-admin/stellar-admin --skill stellar-admin
```

`stellar-admin-setup` is needed only while installing; `stellar-admin` covers day-to-day work. Pages that use `sa-*` components directly, such as host pages and view overrides, also benefit from the UI skill.

## Version compatibility

The skills are maintained alongside StellarAdmin. Their references follow the repository's default branch and may include changes newer than your installed NuGet package. The skill instructs the agent to check your installed version before applying API examples; this [version guidance](stellar-admin-ui/SKILL.md#version-compatibility) travels with the installed references.

The installer does not select guidance based on your installed NuGet version or update your packages.

## Layout and maintenance

Each skill keeps its own `SKILL.md`, license, and supporting references inside its directory. Applications customizing admin panel markup can also use the UI skill; admin panel guidance does not duplicate the UI library reference.

These are consumer skills for applications using StellarAdmin. Repository development skills live separately in `.agents/skills/`. See the [maintenance guide](../docs/repos/skills.md) for generation and ownership.
