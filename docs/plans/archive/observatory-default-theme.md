# Observatory as the recommended theme and website palette

## Code audit — 2026-09-18

Current status: **completed**. DocsSamples defaults to Observatory; website `src/lib/demo-theme.ts` and the consumer theming guide retain the recommendation. Website palette source was inspected alongside the theme source.

This assessment uses the current checkout; earlier status, paths, permissions and verification notes below describe historical sessions. Runtime/browser and hosted release checks were not rerun for this documentation audit.

## Historical record

Status: completed — implemented and verified locally on 2026-09-14.

Last updated: 2026-09-14.

Affected repos: workspace (this plan and maintained guidance), website, stellar-admin, stellar-admin-pro (sample layout and export generator), skills (handwritten consumer guidance).

## Intent and scope

Make Observatory the main suggested StellarAdmin theme and align the public website, documentation shell, and React shadcn components with its overall colours. The user authorized implementation and explicitly allows retaining the existing shadcn component styles. Selector labels and ordering must remain unchanged; Observatory is selected only as the fallback. The maintained [Observatory specification](../../design/themes/observatory.md) and [theme CSS](../../../stellar-admin/src/StellarAdmin.TagHelpers/Client/css/themes/observatory.css) remain the design authority.

Recommended interpretation: Observatory becomes the recommendation for new applications and the default for visitors without a valid saved choice. Existing applications still select their stylesheet explicitly. All other themes remain available, and valid saved demo preferences remain respected. The website always uses the Observatory palette; selecting another demo theme changes the examples only.

Match colours first. Keep website component geometry, documentation font sizes, layout, and interaction behavior. Optional follow-up: adopt IBM Plex Sans and IBM Plex Mono on the website, retaining readable prose sizing and native fallbacks. Do not copy Observatory's 13px body sizing, 32px controls, or 4px radius across the documentation shell as part of the palette work. Actual StellarAdmin demos continue to use the complete Observatory bundle and its established typography and geometry.

## Findings

| Surface | Current implementation | Planned change |
| --- | --- | --- |
| Website colours | `website/src/styles/app.css` defines neutral light/dark shadcn variables | Map colour variables to Observatory |
| Fumadocs | Already imports `fumadocs-ui/css/shadcn.css`; installed adapter maps `--color-fd-*` to shadcn variables and uses sidebar variables | Keep adapter; verify sidebar, search, prose, and overlays inherit correctly |
| React components | `website/src/components/ui/` contains Button, Badge, Card, Alert, Select using semantic colours | Inherit global palette without rewriting component recipes |
| Website demos | `website/src/lib/demo-theme.ts` defaults to `shadcn.nova` | Default to `observatory` |
| Demo selector | `website/src/components/demo-theme-select.tsx` sorts custom themes alphabetically | Keep labels and ordering unchanged; use the new fallback |
| Exported demos | `stellar-admin-pro/docs/DocsSamplesGenerator/Generator.cs` injects a separate Nova fallback; initial stylesheet comes from sample HTML | Change fallback and regenerate so first paint, iframe runtime, and full-preview tabs agree |
| OSS samples | `_Layout.cshtml`, `wwwroot/js/appearance.js`, and `_NavigationLayout.cshtml` use Ledger defaults | Align server, browser, initial label, and selected menu item with Observatory |
| Pro samples | `docs/DocsSamplesPro/Pages/Shared/_Layout.cshtml` falls back to Nova | Default to Observatory; preserve explicit theme queries |
| Getting started | Website installation/theming examples and consumer setup/theming skills begin with Nova | Recommend Observatory and use its bundle in general setup examples |

The [Fumadocs theme documentation](https://www.fumadocs.dev/docs/ui/theme) confirms its shadcn preset adopts shadcn colours. The [shadcn theming documentation](https://ui.shadcn.com/docs/theming) supports changing semantic variables without rewriting component classes. The installed Fumadocs adapter was also inspected, so this approach is grounded in the checked-out website dependencies.

## 1. Apply the website palette

Add a small website-owned palette file, for example `website/src/styles/observatory.css`, imported by `app.css`. Move colour values into that file, using Observatory's public colour mappings and resolved light/dark values. Keep Tailwind vocabulary and general layout rules in `app.css`. Avoid loading the full StellarAdmin bundle in the website shell: it includes its own base styles and component rules. Keep the website independently buildable without a sibling checkout; document the source of the copied palette and the need to update it when the authoritative theme changes. A new shared package or token generator is unnecessary for this initial adoption.

| Semantic role | Light | Dark |
| --- | --- | --- |
| Page background | `#f4f6f9` | `#07090d` |
| Main text | `#0d1620` | `#e4ecf5` |
| Card | `#ffffff` | `#0d1117` |
| Popover | `#ffffff` | `#121a24` |
| Secondary / muted surface | `#eef2f8` | `#18222e` |
| Muted text | `#5a6b7d` | `#7d8fa3` |
| Primary / focus ring | `#0a6ed1` | `#4cc2ff` |
| Text on primary | `#ffffff` | `#04121b` |
| Accent tint | `#e8f1fb` | `#12293a` |
| Border | `#dde3ec` | `#1e2833` |
| Input border | `#c7d1e0` | `#2b3846` |
| Sidebar surface | `#f7f9fc` | `#121a24` |

Also map foreground companions, sidebar selection, destructive colour, and chart colours using the maintained CSS. Distinguish `--primary` (the blue/cyan signal) from `--accent` (its soft tint). Preserve the dark primary foreground: white text on the cyan primary is not the theme's intended pairing.

Review the home page, blog, docs sidebar and table of contents, search dialog, navigation, demo toolbar, and full-preview controls. Audit callouts and code blocks separately: semantic warning/error colours and syntax highlighting need to remain distinguishable. Use targeted Fumadocs colour variables only where the existing adapter leaves a visible mismatch. Keep the palette change scoped to colour; record any proposed structural change separately.

Deliverable: representative light/dark desktop/mobile captures of the home page and one component documentation page for visual review before broad export work.

## 2. Align defaults and theme discovery

Change all default locations in the findings table together. Preserve existing storage keys (`demo-theme`, `docsSamples.theme`, and the separate light/dark preferences). Missing or invalid theme values fall back to Observatory; valid values are retained. Preserve the OSS sample query-over-storage precedence and explicit sample URLs. Do not reset storage to force existing visitors onto Observatory.

Keep both selector groups, their current ordering, and their plain theme labels unchanged, as requested on 2026-09-14. Observatory gets no badge, special label, or pinned position. Align only the initial selection and sample menu label with the default.

Verify the website toolbar, initial static demo stylesheet, injected theme initializer, runtime swaps, and selected font loading agree. Exercise inaccessible localStorage as well as fresh and returning visits; record or fix any inconsistency within this theme-selection path, without turning this into a general preferences rewrite.

## 3. Update recommendations and exports

Update `website/content/docs/tag-helpers/installation.mdx` and the opening guidance/example in `theming.mdx` to lead with Observatory. Explain in the opening guidance that it is the recommended starting point, while retaining the existing theme catalog and documentation for every other theme. Keep examples explicitly demonstrating another theme, namespace migrations, or intentional custom palette overrides where they still serve their purpose; do not globally replace Nova/Vega/Ledger strings.

Synchronize the handwritten `skills/plugins/stellar-admin/skills/theming/SKILL.md` and `skills/plugins/stellar-admin/skills/tag-helpers/references/setup.md`. Check current package readmes and live workspace sample-default guidance for introductory recommendations that need updating. Leave historical plans and deliberately varied compatibility sandboxes alone. No consumer registration API or package default is needed: a theme remains a linked stylesheet.

After source changes, build the sample apps and run `dotnet run --project stellar-admin-pro/docs/DocsSamplesGenerator` from the workspace. Generated demo HTML requires regeneration because the old fallback is embedded in each page. Review exported bundles, font loader, initial links, and snippets; change sources rather than hand-editing `website/public/demo/`. Inspect Git status before and after generation and preserve unrelated changes. Run SkillsGenerator only if curated samples or generated references actually change; handwritten guidance alone does not require regeneration.

## 4. Validate and finish

- Run `pnpm lint`, `pnpm types:check`, and `pnpm build` in `website/`. Build affected sample/generator projects. Run the library CSS build only if theme assets need rebuilding or CSS sources change.
- Inspect home, blog, installation, theming/showcase, Button/Select, and a Pro grid example in light/dark at approximately 1280px and 390px. Include long prose, code blocks, sidebar selection, open search, dropdown hover, and keyboard focus.
- Verify fresh storage, saved Observatory, a saved alternative, invalid values, blocked storage, direct full-preview navigation, and system/light/dark modes. Confirm switching demos leaves website colours fixed and that initial loading does not visibly flash Nova/Ledger.
- Inspect text, primary actions, focus indicators, and selected states for contrast on their actual website surfaces. The Observatory specification records known subtle-border and status-colour limitations; do not assume the palette certifies every shadcn/Fumadocs composition. Record any website-specific colour adjustment and its reason.
- Check exported CSS/font requests for failures and validate consumer-guide links. Capture before/after in the same browser environment. Add a focused default-selection regression check if the existing tooling supports it; do not add tests that merely repeat every colour declaration.

Acceptance: new visitors see Observatory demos, new-user guidance recommends its stylesheet, and website surfaces and controls share its light/dark palette. Alternative themes and saved choices continue to work. Relevant builds/checks pass and the representative visual review is complete.

## Implementation and verification

Implemented on 2026-09-14. The website owns a colour-only `src/styles/observatory.css`, with the maintained theme's resolved public palette in both modes. Website component recipes, typography, radii, selector labels, and ordering remain unchanged. Observatory is the fallback in website state, generated demo initialization, OSS browser/server defaults and initial appearance menu, and the Pro sample layout. Installation examples, the OSS readme, consumer setup/theming guidance, and the website customization example now match the documentation default. Theme-specific Nova examples remain intentional.

Validation completed:

- Website lint, type checking, and production build passed; the final production build was repeated after exports and the last MDX adjustment.
- DocsSamplesGenerator built and exported all 414 demo partials. Pro samples built with zero errors. Existing nullable warnings and NU1900 vulnerability-feed warnings were reported; no SDK pin or package version was changed. Commands ran from the workspace, as documented for cross-repo tooling.
- Chromium checked home, Button docs, installation, theming, and blog pages in light/dark at 1280px and 390px: expected body palette, no page-wide horizontal overflow, Observatory selector defaults, and Observatory iframe stylesheets. Home and Button screenshots were captured; representative desktop light and mobile dark docs and desktop dark home captures were visually inspected.
- Saved Nova and Ledger choices persisted; invalid values fell back to Observatory. Website colours remained fixed when the demo choice changed. A direct exported demo with storage access blocked loaded Observatory.
- Selector checks confirmed all fifteen options, unchanged alphabetical order, and no recommendation labels. Keyboard navigation and the search overlay passed in light and dark modes.
- Generated diffs were inspected: the theme fallback and initial stylesheet changed across all demo HTML, alongside generator variability such as SVG attribute ordering, generated element IDs, and antiforgery values. Generated snippets and theme bundles did not change. No generated output was restored or hand-edited.

Limitations: this was a palette/default rollout, not a full component or accessibility audit. Standalone sample runtime and Pro grid visual checks were not rerun; their layouts were compiled. Font families remain unchanged on the website. Browser captures and temporary verification scripts are under `/tmp/observatory-*`; they are local review artifacts, not committed fixtures.

Git handoff: workspace contains this record, index, and updated OSS sample-default guidance; OSS contains layout/menu/browser default and readme edits; Pro contains sample-layout and generator-fallback edits; website contains palette/default/docs changes and regenerated HTML; skills contains two handwritten guidance edits. Changes were subsequently committed at the user’s request; see the commit record below. No publishing was performed. No required implementation work remains.

## Website corner-radius follow-up

The user reviewed the palette rollout and requested Observatory's sharper corners as well, authorizing implementation on 2026-09-14. This supersedes the original decision to preserve website radii; readable typography and spacing remain unchanged.

The follow-up changes only `website/src/styles/app.css` and `website/src/components/hero.tsx`. The shared scale uses 2px details, 3px nested items, and 4px for medium and larger radii. Larger Tailwind radius names intentionally map to the same 4px panel corner, so existing shadcn/Fumadocs card, code-block, demo-frame, and dialog recipes inherit the look without editing their internals. Hero actions no longer force `rounded-full`. Targeted `[data-search-full]` and `[data-theme-toggle]` rules override Fumadocs' hard-coded pills, with 3px corners on the nested theme choices. Actual badges, circular indicators, and scrollbar thumbs retain their deliberate shapes.

Website lint, type checking, production build, and diff whitespace checks passed. Browser review and captures use `/tmp/observatory-radius-*`. The earlier exported demos and unrelated uncommitted edits were preserved; no regeneration was needed for this website-only refinement.

Radius checks passed in light and dark modes at 1280px and 390px: 4px hero/search/theme-toggle/control/panel corners, 3px selector items, retained pill badge, no page-wide overflow, and contained search dialogs. Representative home, docs, and mobile search captures were visually inspected. The user subsequently authorized commits across all repositories.


## Commit record

Committed at the user's request on 2026-09-14. Child repositories are clean after committing; nothing was pushed.

| Repository | Commit |
| --- | --- |
| stellar-admin | `5500cfb` — default sample and setup theme |
| stellar-admin-pro | `dff2472` — sample and export defaults |
| website | `4f7c9c79` — palette, corners, guidance, and regenerated demos |
| skills | `5cf6b28` — setup and theming guidance |

The workspace record and sample-default guide are committed together with this record.
