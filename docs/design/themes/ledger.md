# Ledger theme

Status: foundation approved; full component coverage implemented and verified. This is the maintained design and extension specification. The [Ledger CSS](../../../src/StellarAdmin.TagHelpers/Client/css/themes/ledger.css) is the source of exact token values and implemented component recipes; it is hand-authored and must not be regenerated from shadcn. This document and the maintained source are sufficient to extend Ledger; no external design export is required.

## Design intent

Ledger is a dense business interface with warm paper surfaces in light mode, blue-tinted charcoal surfaces in dark mode, and one indigo action colour. Thin visible edges and restrained elevation make controls feel tangible. Use Lexend for UI and JetBrains Mono for identifiers, copyable values, and shortcuts. Preserve legibility and clear interaction states as the design is extended.

Use this specification for design intent and reviewed composition rules, the maintained CSS for exact light/dark values, and real DocsSamples components for semantics, keyboard operation, and state. Later user-approved corrections take precedence over early prototypes. The standalone sandbox is a historical foundation snapshot, not the authority for current component behavior. Capture the current implementation in a real browser when comparing a change.

## Optional fonts and native fallbacks

Decision accepted on 2026-09-14 after reviewing both showcase pages in light and dark mode: retain the original font families for their character, with the approved native stacks as fallbacks. Font loading is optional for consumers. Linking only the theme bundle requires no font downloads; an application can add `https://fonts.googleapis.com/css2?family=Lexend:wght@300..700&family=JetBrains+Mono:wght@400;500&display=swap` (or self-host the fonts) to restore the original typography. The sample apps and exported website demos load only the selected theme's font families with `display=swap`, showing native text while fonts load and switching when ready. A late swap can change text widths and wrapping; applications that prefer to avoid a late swap can choose `display=optional`, which may keep the native font for that page view.

The maintained CSS uses the following family order:

```css
font-family: "Lexend", "Avenir Next", Avenir, "Segoe UI", system-ui, sans-serif;
/* Data, identifiers and shortcuts */
font-family: "JetBrains Mono", ui-monospace, "SFMono-Regular", Menlo, Consolas, "Liberation Mono", "DejaVu Sans Mono", monospace;
```

Avenir Next and Avenir are optional local matches, not cross-platform guarantees. Without them, Ledger uses Segoe UI or the platform UI font. The accepted Linux comparison used Liberation Sans and did not demonstrate Avenir rendering.

Preserve the existing sizes, weights, tracking and line heights. Native font selection varies by operating system and installed fonts; the accepted screenshots cover Linux Chromium, not Windows or macOS. Future visual checks should exercise both loaded web fonts and native fallbacks, including small labels, dense controls and text wrapping. The bundle must not add remote font imports or require JavaScript for fallback.

## Foundations

- Colours: use the semantic light/dark palette in the maintained CSS, including card, popover, sidebar, primary, secondary, muted, accent, destructive, border, input, ring, and five chart colours. Exact values live in the theme CSS. Do not derive a second copy in documentation.
- Typography: body/control text 13.5px at 400; buttons 13.5px at 500; field labels and menu items 13px; helper text 12px; card headings 16px at 600 and descriptions 13.5px. Page titles are 28px at 600; page layout owns display and statistic typography.
- Geometry: 6px normal corners, 4px inner/menu corners, 3px checkbox corners, 10px large-surface corners. Preserve the exact scale rather than relying on the stock radius offsets. Circular controls remain circular.
- Density: small/default/large buttons are 32/36/42px high; default controls are 36px high. Button icon gaps are 6px. Card padding is 20px, menu surface padding 4px, menu row padding 7px vertically and 9px horizontally. Page layouts may increase touch targets to 44px; viewport width alone does not globally resize every library control.
- Surfaces: cards and fields use card colour, floating menus use popover colour, menu hover uses accent. Surfaces use a 1px border. Cards have a subtle shadow, fields an inset shadow, popovers a larger floating shadow.
- Focus: use a 3px ring at 35% ring colour, additive to existing elevation. Inputs retain their inset shadow when focused. Invalid fields use destructive borders and a 20% destructive ring. Preserve the framework's validation classes as well as ARIA states.
- Motion: background, border, and shadow transitions take 120ms; button press movement takes 80ms. Respect reduced motion. Scope motion treatment to the components; do not disable unrelated consumer animations globally.

## Buttons

Primary, secondary, outline, and destructive actions retain a visible 1px border and raised inset bottom edge. Primary and destructive borders use their dedicated darker colour. Hover changes fill and subtly raises the outer shadow. Pressing shifts the button down 1px and flips the inset edge to the top. Keyboard focus adds its ring without flattening the control. Disabled buttons lose elevation and movement and use 50% opacity. Ghost and link variants remain flat in every state.

A focused and pressed raised button retains the focus ring while using the pressed shadow. A loading action can retain elevation but must use the component's real disabled/busy semantics; pointer-events alone is not a keyboard lock. Toggle selection is a persistent selected state, not a permanently depressed action button.

## Inferring components

1. Find the nearest existing Ledger primitives: a combobox combines a field, floating menu, menu rows, and badges; a date picker combines those surfaces with compact navigation actions and selectable cells.
2. Reuse semantic tokens and established geometry. Introduce a token only when an existing semantic role cannot express the design; prefer theme-private `--sa-ledger-*` names for Ledger-specific implementation choices.
3. Reuse the component's existing DOM, classes, slots, and real state selectors. Do not copy prototype spans that imitate interactive controls or add a separate Ledger component API.
4. Work through rest, hover, keyboard focus, pressed, selected/open, disabled, invalid, and loading where meaningful. Examine combinations such as focused + pressed and invalid + focused.
5. Compose desktop and touch layouts using the existing responsive component behaviour. A new theme does not introduce dashboard navigation or replace tables with cards globally.
6. Add a real component example, inspect light and dark mode, and record consequential inferred decisions here. User authorization permits inference; the foundational visual checkpoint has been approved.

## Inferred decisions

| Decision | Reason |
| --- | --- |
| Extra-small button: 28px high, 10px horizontal padding, 11.5px text, 14px icon. Icon sizes follow the corresponding text-button height. | Extends the 32/36/42 scale for the existing API without adding a new size. |
| Small cards use 16px padding; default cards use 20px. | Preserves the existing card size API and Ledger's compact spacing. |
| Menu keyboard focus uses the same accent fill as hover, with an inset focus ring. | Keep a visible focus affordance inside the clipping popover. |
| Success, warning, and button-specific colours remain theme-private for now. | A custom theme should not add unsupported variants to public components or decide a suite-wide token API. |
| Global anchor styling is not copied. | Action links and menu links already have component semantics; styling all anchors would leak page styling into unrelated content. |
| No imported upstream theme as a fallback. | Missing Ledger coverage must remain explicit rather than looking accidentally complete. |

## Reproducibility and future coverage

Version-controlled CSS and locked build dependencies reproduce a saved implementation; save the specification and reviewed CSS together when committing an extension. AI inference itself is not deterministic; this specification, recorded decisions, and accepted component examples constrain future work. Browser image comparisons must use the same browser, fonts, viewport, and rendering environment for base and candidate.

`util/theme-coverage/coverage.json` records 54 component families, tags, styling hooks, examples, and explicit decisions across nine themes. The build and CI run `check.mjs`; inventory changes, missing rule evidence, and pending support fail validation. Shared-only entries require rationale. The manifest is maintained after inspection, not regenerated to silence a failure. It detects omissions, not design quality.

The component-porting workflow requires custom-theme inference, real state examples, browser verification, and recorded decisions alongside upstream generation. Upstream regeneration owns only upstream-derived themes; Ledger is independently maintained. Use `npm run check:themes` and `npm run test:themes` from Client. VRT accepts `--theme ledger --mode light|dark` for repeatable component captures.

## Additional inferred decisions

- Selection controls remain flat: native radios use an indigo dot on the card surface, toggles/tabs use the accent or card surface, and navigation selection gets a thin border and subtle card shadow. They do not inherit action-button press movement.
- Small switches use a 32×18px track and 12px thumb, extending the default 38×22px track and 16px thumb. Sliders use a 6px recessed track and 18px raised thumb.
- Input groups own one border, inset shadow, and composite focus ring. Their internal fields remain transparent; validation decorates the group. OTP uses separated 40×46px cells.
- Dialogs use the card surface and modal shadow, a 480px maximum width, 20px padding, and a bordered action footer. Small alert dialogs use 360px. Sheets preserve StellarAdmin's edge placement and responsive behaviour, with the same typography and elevation.
- The mobile sidebar reuses the existing off-canvas implementation. Active navigation uses a bordered card surface; collapsed navigation retains compact 32px icon targets. Bottom navigation and table-to-card conversion belong to individual page layouts, not global theme rules.
- Attachments and items use bordered 6px surfaces. Messages use flat bordered bubbles with semantic tints, 13.5px body text, and 12px metadata. Questionnaire choices compose the field surface, native selection indicator, and keyboard focus treatment.
- Pro data grids inherit their card/table shell and existing structural rules. Pro features stay in the Pro repository; the theme does not add unsupported alert variants, chart widgets, or new component APIs.
- The success/warning colours remain private and unused by components without those variants. Full chart UI, calendar, combobox, and toast implementations are outside this theme task because those tag helpers are not currently shipped.

## Token and recipe selection

Read the `:root` and `.dark` blocks in the maintained CSS together before extending a component. Reuse variables rather than copying literal colours or shadow definitions into new rules. The semantic palette and private dark-mode elevation overrides are already complete in that file.

| Role | Tokens and treatment |
| --- | --- |
| Page canvas and reading text | `--background`, `--foreground`; supporting text uses `--muted-foreground`. |
| Cards, fields, dialogs, active default tabs | `--card`, `--card-foreground`; active tabs use `--sa-tabs-active-background`. |
| Floating menus and pickers | `--popover`, `--popover-foreground`, `--sa-ledger-shadow-pop`. |
| Subdued regions and transient interaction | `--muted` for headers/tracks, `--accent` and `--accent-foreground` for hover/focus where the component recipe calls for it. Checked state alone does not imply accent fill. |
| Actions | `--primary` or `--secondary` and their foregrounds; raised primary actions also use `--sa-ledger-primary-hover` and `--sa-ledger-primary-border`. Destructive actions have corresponding private hover, border, and foreground tokens. |
| Boundaries and validation | `--border` for surfaces, `--input` for fields, `--ring` for focus, `--destructive` for invalid state. |
| Elevation | `--sa-ledger-shadow-card`, `--sa-ledger-shadow-inset`, `--sa-ledger-shadow-pop`, `--sa-ledger-shadow-modal`; raised actions compose the `btn`, `btn-hover`, and `btn-active` shadows with `--sa-ledger-ring-shadow`. |
| Navigation and charts | Use the `--sidebar-*` roles inside navigation and `--chart-1` through `--chart-5` for series. Do not substitute chart colours for interaction-state colours. |

Spacing steps are 2, 4, 6, 8, 10, 12, 14, 16, 20, 24, 32, and 40px (`--sa-ledger-space-1` through `-12`). Choose the nearest existing component recipe before selecting a new spacing. A composite should not accumulate full padding from each nested primitive. Account for borders and font metrics when judging rendered alignment; equal CSS padding does not guarantee equal visible text spacing.

The display typography scale is 38/28/22/17px, weight 600, with line heights 1.1/1.2/1.25/1.3 and tracking −0.025/−0.02/−0.015/−0.01em. This is available to page composition; component headings retain their own compact recipes. UI base line height is 1.55, labels 1.2, helper text 1.45. Small uppercase section labels use 11.5px, weight 600, and 0.07em tracking. Optionally load Lexend and JetBrains Mono through the app layout; the CSS declares families but does not fetch fonts.

## Reviewed composition rules

These rules preserve the corrections made during component review and should guide analogous future components.

| Family | Rule and existing recipe to inspect |
| --- | --- |
| Fields and input groups | Inspect `.sa-input-group` and its input/addon rules. The group owns the border, inset shadow, and focus treatment. Inner fields, including read-only ones, remain transparent and flat. Current inline spacing uses 6px edge padding and a 6px internal gap; start/end addon rules avoid duplicate boundary padding. Block addons need explicit vertical layout and enough input height for labels and helper text. |
| Joined actions | Inspect `.sa-button-group`. Controls inside one subgroup join with zero gap and only exposed end corners rounded. Nested groups are separated by 8px; the outer grouping container adds no shadow. Preserve horizontal and vertical variants and consumer radius overrides. |
| Selection and menus | Inspect dropdown checkbox/radio items separately from toggles and tabs. Checked menu items show an indicator without a persistent background; hover, keyboard focus, and open submenus retain feedback. Segmented controls match default tabs: 2px list padding, 3px gap, 6px list radius, 4px item radius, 6px 10px item padding, 13px type, and a card-surface checked item with a thin border and card shadow; selection and focus follow the native radio. Default tab lists have 2px padding around their triggers; line tabs retain their separate underline treatment. |
| Switches and avatars | Reuse the existing thumb positioning and checked selectors, including model-bound hidden inputs. Do not add a second vertical translation to an already centered thumb. Circular avatar borders, including pseudo-elements, inherit the avatar radius. |
| Tables | Table and scroll container fill the available width; narrow containers scroll horizontally. Captions sit below. Muted headers/footers and row separators supply structure. An optional rounded outer border wrapper owns clipping; do not add a competing inner radius, border, or shadow. |
| Dialogs | Alert headers use a grid with a 6px title/description gap. Small alert dialogs center media and text; media adds another 8px below it. Normal dialogs retain their normal alignment. Keep content and bordered action footer separate. |
| Sheets | Panels use fixed positioning and the existing side/size contract. The close button is absolute, 12px from the top/right, so it adds no flow height above a full-height body. Validate all four sides and ensure short content does not create forced scrolling or hide footer actions. |
| Accordions | Current trigger padding is 10px 16px 7px; content is 0 17px 10px. The extra horizontal pixel accounts for the trigger border. With Lexend, heading-to-content visible spacing is about 13px. Check wrapped content and multiple open items, not just CSS box gaps. |
| Sidebar and app header | Preserve scrolling while hiding the sidebar's native scrollbar. Desktop inset content has 8px outer gutters, no expanded left gutter, an 8px collapsed left gutter, Ledger radius/shadow, and clipped corners. Mobile is edge-to-edge. Floating-sidebar main headers currently keep their contrasting card background with rounded lower corners and a subtle shadow at desktop widths. |

## Implementing support for a new component

The reusable [custom-theme workflow](../custom-theme-workflow.md) covers exploration, handoff extraction, implementation, and cleanup for all independent themes. Use the Ledger-specific rules here when applying it to Ledger.

1. Inspect the new component's Tag Helpers, shared CSS, slots, state selectors, and real sample markup. Upstream examples establish functionality and variant layout; existing Ledger recipes establish appearance. Map each part to a surface, action, field, selection indicator, typography role, or layout container using the tables above. For example, a future combobox should compose field and popover/menu recipes; a future toast should compose a floating surface, message typography, and flat dismiss action while preserving its own announcement semantics.
2. Add the required `.sa-*` rules to the maintained Ledger CSS in `@layer components.theme`. Do not import an upstream theme as a fallback, copy an entire upstream visual stylesheet, or introduce Ledger-specific public component markup. Read the existing structural CSS before adding positioning, transforms, overflow, sizing, or pseudo-elements: those properties have caused several of the reviewed regressions above.
3. Account for layer precedence: rules directly in `@layer components` outrank the nested theme layer even when the theme selector is more specific. Keep Ledger values in Ledger CSS. Where a base visual property genuinely blocks customization, use a narrowly scoped shared variable hook with the existing behavior as its fallback, and verify the other themes. Existing hooks cover button transition properties/duration, textarea minimum height, page-header typography, and active-tab background. A Ledger-only adjustment is not permission to change shared defaults.
4. Cover all supported variants, sizes, orientations, responsive states, and meaningful state combinations. Distinguish transient hover/focus/press from persistent checked/selected/open. Preserve keyboard operation, focus visibility, disabled/busy semantics, validation, reduced motion, and user-supplied classes. Inspect both light and dark token resolutions.
5. Extend ordinary DocsSamples examples and open them with `?theme=ledger` or `?theme=ledger&mode=dark`; there are no dedicated Ledger sample pages. Update the [coverage manifest](../../../util/theme-coverage/coverage.json) after inspection: record actual rules for reviewed support, or explain composed/shared-only support. Never add placeholder rules merely to pass coverage. Consult the [coverage workflow](../../../util/theme-coverage/README.md) for its schema and checks.
6. Run `npm run check:themes`, `npm run test:themes`, and `npm run build:css` from `src/StellarAdmin.TagHelpers/Client`. Exercise actual rendered examples at desktop/mobile widths in both modes. Use `node util/visual-regression/verify-ledger.mjs <sample-base-url>` from the OSS repo for existing focused checks, and add meaningful regression coverage for defects found during review. Capture base/candidate with the same browser, fonts, viewport, and mode; inspect spacing, clipping, overflow, focus, and interaction rather than relying on the coverage manifest as visual proof. Shared changes require comparison against the other themes as well.
7. Follow the product component-porting workflow for website exports and consumer references when adding a component. Record new reusable design decisions here and task-specific verification in the plan. Keep this specification and the maintained CSS aligned so the next extension does not depend on conversation history or temporary screenshots.
