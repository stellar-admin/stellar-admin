# Row detail panels as an accordion

Status: implemented, awaiting review (2026-10-30).

Show a row's detail panels as a contained accordion, and make that the default for two or more panels. Tabs stay available, in either tab style.

## Decisions

- The look is the contained accordion ("D" in `sandbox/html/row-details-accordion.html`): one bordered box, a muted title band on each item, no highlight on the open item. `sandbox/html/row-details-playground.html` shows it against both tab styles with every row details setting.
- `PanelLayout` on `ResourceRowDetailsBuilder` takes a closed class hierarchy, like `BrandingLogo`: `ResourceRowDetailPanelLayout.Accordion()` or `Accordion(ResourceRowDetailAccordionExpandMode)`, and `Tabs()` or `Tabs(ResourceRowDetailTabStyle)`. The factories return `AccordionResourceRowDetailPanelLayout` (`ExpandMode`) and `TabsResourceRowDetailPanelLayout` (`Style`).
- Dedicated enums: `ResourceRowDetailAccordionExpandMode { Single, Multiple }` and `ResourceRowDetailTabStyle { Default, Line }`. An undefined value is a configuration problem, and the default layout is used.
- Unset, the layout is an accordion with several panels open at once. `Accordion()` without an argument is the same. `Tabs()` without an argument uses the `Default` tab style.
- **This changes existing apps:** resources with two or more panels switch from line tabs to the accordion.
- Accordion panels all start collapsed; there's no setting for it. Panels that load on demand load when first opened. `Single` uses a shared `name` on the items' `<details>`.
- `ResourceRowDetailPanelBuilder.Icon` (`string?`) puts an icon on the panel's accordion title or tab. There's no global switch.
- All three `Add()` methods have an overload with a trailing `Action<ResourceRowDetailPanelBuilder> configure`, which returns the panels builder: `panels.Add("Overview", "_ProductOverview", panel => panel.Icon = "package")`.
- StellarAdmin.UI gets public API for it: `AccordionVariant { Default, Contained }` on `<sa-accordion variant>`, and `icon` on `<sa-accordion-item-title>`, shown in a tile before the title, with the content indented to line up under the title.
- Counts on the titles are out of scope.

## Work

- StellarAdmin.UI: `AccordionVariant`, `AccordionTagHelper.Variant`, `AccordionItemTitleTagHelper.Icon`, contained and icon CSS in `accordion.css`, tests, a DocsSamples demo.
- StellarAdmin: the layout hierarchy and enums, `PanelLayout` and `Icon` on the builders, the options, the check, `_IndexDataGrid.cshtml`, tests.
- AdminPlayground and docs: the samples' row details, the `EnableRowDetails` remarks, the consumer skill reference and the website docs.

## Verification

Done on 2026-10-30:

- `npm run build:css`, and the build of StellarAdmin.EntityFrameworkCore with the public API files updated.
- `StellarAdmin.UI.Tests` (803), `StellarAdmin.Tests` (338), `StellarAdmin.IntegrationTests` (719) and `StellarAdmin.EntityFrameworkCore.IntegrationTests` (155) pass. New tests cover the accordion variant and title icon, the layout factories, `Icon`, the default accordion, on-demand loading on first open, `Single`'s shared name, both tab styles, panel icons in both layouts and the undefined-layout problem.
- AdminPlayground Products in headless Chromium: expanding a row leaves the three panels closed, and Pricing and Related load when first opened. The page matches the approved contained look, with icons. No new configuration problems.
- `skills generate` and `skills check`; `docs generate demos` with the new `Accordion/_Contained` demo, checked in dark mode; website `pnpm build`.

- The `configure` overloads: `StellarAdmin.Tests` (340), `StellarAdmin.IntegrationTests` (719) and `StellarAdmin.EntityFrameworkCore.IntegrationTests` (156) pass, and the AdminPlayground's Products panels, which use all three on the EF Core builder, build.

Outstanding:

- The website's `row-details-accordion` screenshot placeholder needs a scenario and spec. The old `row-details-tabs` images in the website are now unused.
- Browser checks of the tabs layouts and `Single` in the real admin panel are covered by the integration tests' markup, not yet clicked through.
