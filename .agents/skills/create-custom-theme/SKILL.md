---
name: create-custom-theme
description: Add or change a StellarAdmin theme fixture (a theme file the samples' theme pickers, the website theme builder's seeds and the theme checks offer), or add a theme variable when a design needs one no theme variable covers. Use for product-side theme work; an application's own theme file is the consumer stellar-admin-theme skill's job.
---

# Theme fixtures and theme variables

A theme is theme variable values; the library ships only the default (`stellar-admin.css` alone). Read [theme tokens](../../../docs/design/tokens.md) for the theme variables, the foundation tokens and the layers, and the product guide's [theme fixtures](../../../docs/repos/stellar-admin.md#theme-fixtures) section for where fixtures flow. An application's own theme file is not product work: the consumer [theme skill](../../../skills/stellar-admin-theme/SKILL.md) covers it.

Inspect status in each affected repository first, and keep to the requested scope: a fixture change does not authorize a new theme variable, a website export or a commit.

## Add or change a theme fixture

A fixture is one file, `util/theme-check/presets/<name>.css`; adding a theme is adding a file there. Follow the existing three (`ledger`, `ops`, `soft`):

- The header comment's first line is `Theme fixture: <Label>`, with a one-paragraph description under it. The pickers, the docs export and the theme builder read both.
- Theme variables only, on `:root` inside `@layer sa.theme`. No foundation tokens, no component rules: a look the theme variables cannot express is a missing theme variable (below), not an override.
- The font is the fixture's `--sa-font-sans`. Use one of the manifest's font choices (`src/StellarAdmin.UI/Client/css/theme-variables.json`), written exactly as its `value`; that is how the samples and the website find the family to load. Another stack works but loads no web font.
- Set dark-mode values only where the dark mode should differ (the `-dark` theme variables); the formulas derive the rest.
- Contrast is the formulas' job. Don't set `--sa-on-accent` to fix a colour; change the accent.

Review in the real samples. DocsSamples and ComponentPlayground read the folder at startup, so restart them; open DocsSamples on port 5206 (never 5205) with `?theme=<name>&mode=light|dark`, starting with `/Showcase/ThemeShowcase`. Then run the [theme checks](../../../util/theme-check/README.md) over the fixtures, light and dark: `contrast.mjs` must report no text failures, and review its faint parts and pill corners against the base theme's. Use `screenshots.mjs` for a visual sweep. Stop every server you start.

The website's theme builder sees a fixture only after `dotnet run --project docs/DocsSamplesGenerator` exports it and the website commits the output (`assets/presets/`). Run the export only when the task includes the website. The consumer theme skill's theme variable reference copies Ledger, Ops and Soft as worked examples: when one of those changes, update its copy there.

## Add a theme variable

Add a theme variable only when a design need recurs across themes and no existing theme variable, or combination of theme variables, expresses it. Theme variable names are public API once released, so propose the name, its values, its default and what it changes to Jerrie before adding it. The default must keep the default theme unchanged.

A theme variable touches:

1. `Client/css/tokens.css`: the theme variable's default, and the foundation tokens it feeds (or the component rules that read it through `var()`, for a structural switch).
2. `Client/css/theme-variables.json`: its manifest entry (group, label, meaning, type, range or choices, default; `optional` and `follows` for an optional theme variable). `npm run build:css` in `src/StellarAdmin.UI/Client` fails when the manifest and the stylesheets disagree; run it directly, since `dotnet build` can hide a failed CSS build.
3. [docs/design/tokens.md](../../../docs/design/tokens.md): its row in the theme variable tables.
4. The consumer theme skill's [theme variable reference](../../../skills/stellar-admin-theme/references/theme-variables.md), and its description-to-variables table when the theme variable answers a common description.
5. `util/theme-check/random-theme.mjs`, so the random themes exercise it.

Verify the default theme is unchanged where the theme variable is at its default, then run the contrast check over the fixtures and the random themes, light and dark, on the pages the theme variable affects. Record the work and the checks actually run in the relevant plan.

## Related skills

Porting an upstream component is the [porting skill](../port-shadcn-component/SKILL.md); designing a new component's visuals is the [prototype skill](../prototype-component/SKILL.md). Theme work alone needs neither.
