# Concourse theme

Status: implemented across the shipped component families. Jerrie authorized continuation after the foundation preview. The [Concourse CSS](../../../src/StellarAdmin.TagHelpers/Client/css/themes/concourse.css) is the maintained source for exact token values and component recipes. Concourse is hand-authored; ThemeGenerator must not own it.

## Authority and intent

Concourse is a compact business interface with cool grey surfaces, a single blue interaction accent, hairline borders, 4px corners, and shallow elevation. Source Sans 3 supplies UI typography; IBM Plex Mono supplies identifiers, values, and shortcuts. Interaction depth comes from colour steps, without button movement or bevels.

The original Claude Design handoff established the direction and was removed at Jerrie's request after extraction. This specification and the maintained CSS preserve the reusable decisions, exact values, and subsequent user-approved corrections. Real component markup and behavior take precedence over simulated prototype controls.

Calendar, combobox, and command-palette designs referenced an additional source file that was never supplied. Future components should compose the maintained field, menu, selection, and overlay recipes; no missing export is a prerequisite. Seek a new design decision only when those recipes cannot resolve a consequential visual choice. Use real DocsSamples with fonts loaded as the baseline for future comparisons.

## Optional fonts and native fallbacks

Decision accepted on 2026-09-14 after reviewing both showcase pages in light and dark mode: retain the original font families for their character, with the approved native stacks as fallbacks. Font loading is optional for consumers. Linking only the theme bundle requires no font downloads; an application can add the Google Fonts stylesheet in Foundations (or self-host the fonts) to restore the original typography. The sample apps and exported website demos load only the selected theme's font families with `display=swap`, showing native text while fonts load and switching when ready. A late swap can change text widths and wrapping; applications that prefer to avoid a late swap can choose `display=optional`, which may keep the native font for that page view.

The maintained CSS uses the following family order:

```css
font-family: "Source Sans 3", system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
/* Data, identifiers and shortcuts */
font-family: "IBM Plex Mono", ui-monospace, "SFMono-Regular", Menlo, Consolas, "Liberation Mono", "DejaVu Sans Mono", monospace;
```

Preserve the existing sizes, weights, tracking and line heights. Native font selection varies by operating system and installed fonts; the accepted screenshots cover Linux Chromium, not Windows or macOS. Future visual checks should exercise both loaded web fonts and native fallbacks, including small labels, dense controls and text wrapping. The bundle must not add remote font imports or require JavaScript for fallback.

## Foundations

- Palette: preserve the supplied light/dark semantic roles. `--border` divides surfaces; the stronger `--input` and private border-strong role identify controls. Dark cards are lighter than the canvas, and dark popovers are lighter than cards.
- Typography: Source Sans 3 at 400/500/600/700 and IBM Plex Mono at 400/500. UI text is 13.5px with 1.55 line height; labels 13px at 600; helper text 12px; card headings 15.5px at 700. Display/page heading sizes are available as tokens, not mandatory styling for all headings.
- Font loading: the app may load the families; the theme declares their stacks. Optionally load `https://fonts.googleapis.com/css2?family=Source+Sans+3:wght@400;500;600;700&family=IBM+Plex+Mono:wght@400;500&display=swap`, or self-host the families with their SIL Open Font License notices. Keep `font-display: swap`. Do not add CSS remote imports inside the bundle.
- Geometry: base corners 4px, inner/menu/checkbox corners 2px, dialogs 6px, circular controls remain circular. Do not use the upstream radius-offset scale to infer these values.
- Density: small/default/large controls are 30/34/40px. The source's 44px touch treatment belongs to responsive composition; a narrow viewport alone must not resize every component or introduce a theme-specific API.
- Spacing: preserve the source scale of 2/4/6/8/10/12/14/16/18/22/28/40px. Default card padding is 18px, with a 16px body gap.
- Elevation: preserve shallow action/card shadows, popover shadow, and modal shadow. No hover lift or press translation. Shadows have explicit dark-mode counterparts.
- Motion: background/border/shadow feedback is 100ms; switch movement is 150ms with the source easing. Scope reduced-motion handling to themed components.

Standard library tokens retain their public names. Additional source tokens, including status colours, type scales, focus, shadows, sizing, and interaction steps, are namespaced `--sa-concourse-*`. This does not introduce success/warning component variants, a second brand accent, or a public configuration API.

## States and composition

| Family | Recipe |
| --- | --- |
| Buttons | Default means filled blue; secondary uses the card surface, not the secondary token fill. Outline uses a stronger visible edge; ghost and link remain flat. Light-mode primary hover/press darken; dark hover lightens and press comes down. Use the explicit tokens. Preserve all library sizes, icons, busy and disabled semantics. |
| Focus | Actions and native selection controls use the source gap ring: a 1px background-coloured gap with an outer ring extending to 3px. Fields use a 2px wash at 30% ring colour. Focus must survive simultaneous press and persistent selection. |
| Fields | 34px height, 11px horizontal padding, card surface and strong input border. Textareas retain 88px minimum height and 9px vertical padding. Read-only fields use the muted surface. Invalid styling responds to ARIA and MVC validation classes and retains the 2px destructive wash at 22%. |
| Input groups | The outer group owns border, radius, and focus/validation. Inner controls remain transparent, borderless, and flat, including read-only and focused states. Preserve inline/block addon alignment and textarea growth. The export's text/icon/button addons have different edge treatments; verify these against the actual addon API rather than adding invented theme attributes. |
| Selection controls | Preserve real native checked/indeterminate state and model-bound hidden inputs. Checkbox/radio controls are 17px. Radio has an 8px dot. Default switch has a 38×20px track and 12px thumb; use existing thumb structure and checked sibling selectors. |
| Cards | One owner for border, corner radius, and shadow. Header title/description spacing is 3px. Preserve the existing header/action/content/footer slots and small-card variant. |
| Dialogs | Card surface, 6px corners, modal shadow; source backdrop opacity is 32% light / 55% dark. Preserve native dialog behavior, dismissal, viewport containment, scrolling, media variants, and reachable footer actions. |
| Menus and segmented selection | Dropdown checkbox/radio items show selection through their check/dot indicator, without persistent background fill. Hover/focus retains transient feedback. Segmented controls retain their separate selected-surface treatment. |
| Tabs | Selected state uses stronger weight plus a 2px blue underline. Preserve existing variants and orientation; do not silently collapse their contracts. |
| Navigation | Current item uses card fill and a 3px leading blue rule. The export's hidden mobile sidebar is a prototype gap; preserve StellarAdmin's accessible mobile drawer. |
| Tables | Muted header/footer, compact cell padding, neutral hover wash, blue selected tint, with a stronger selected-hover tint. Scrolling and caption placement remain structural. Do not globally replace narrow tables with cards. |
| Alerts | Normal card surface with a 3px semantic leading rule. Do not add source-only variants to the component API. |

## Maintained implementation decisions

The existing library conventions resolve the handoff questions about markup ownership, `.dark` triggering, modern CSS, and theme selection. The supplied 34px density is the selected direction. A second brand accent is unnecessary. Additional semantic colours remain private until a separate public API decision is warranted.

The foundation extends the size scale with a 26px extra-small action and 14px icon; small switches use a 32×18px track and 10px thumb. Small cards use 16px padding. Dialog layout currently adapts the library's 480px/360px normal/small composition with 20px padding. These extend the selected foundation to existing library variants; they are implementation inferences rather than explicit source designs.

Native disabled opacity remains controlled by shared CSS where it outranks the theme (50%, versus the source's 45%). Indeterminate checkbox border treatment is also owned partly by shared structure. Do not modify shared defaults merely to copy a prototype value. Explain any necessary shared variable hook and verify other themes before applying it.

Native selects use 14px text instead of the 13.5px body size. This works around a missing options-popup bottom border reproduced with Source Sans 3 in Chromium on Wayland at 1.6× display scaling. Keep the existing native control, height and focus treatment. Verify using `verify-native-select-popup.mjs` on the actual desktop; headless captures did not reveal the clipping. This workaround is verified at the reported scale, not every browser/platform combination.

Input groups retain a single outer border and focus/validation decoration. Text-only inline addons use a muted fill and a separating edge; icon/button addons remain plain. Inline button addons use 3px vertical padding so a 26px action fits a 34px group. Block addons have 9px vertical padding, and internal fields retain 11px edge insets. Focus and read-only styling never add an inner surface. User-supplied utility classes still override component styling.

Default tabs and segmented controls share the user-approved Ledger-like selected-item treatment: 2px track padding, 3px gaps, a card-coloured selected surface with its own border and shallow shadow, and rounded inner items without divider borders. Concourse retains its own palette, typography, and radius tokens. Both components use 6px vertical and 10px horizontal item padding. Native checked inputs drive segmented selection; focus and disabled behavior remain structural. Line tabs use 3px bottom padding (reduced from 10px) to bring the underline closer to the label. The tabs/segmented appearance-equality check must pass for Concourse along with the other themes. A shared `--sa-tabs-indicator-background` hook keeps the foreground fallback for other themes. Checked toggles use `--sa-toggle-hover-background` so shared hover styling cannot replace persistent accent selection; other themes retain the muted fallback.

Dropdown checkbox and radio items use only their existing check/dot indicator for persistent selection, following the user correction shared with Ledger. Checked and unchecked rows receive identical hover/focus backgrounds; keyboard focus remains visible. Do not copy tab/toggle selected fills into menu items. Bold menu accents and inverted/translucent surfaces retain their existing hover/focus and menu APIs.

Sidebar selection uses a card surface and 3px leading rule; its inset/floating layouts reuse the existing responsive structure, scrolling, and mobile drawer. Attachments, items, messages, and questionnaire choices compose Concourse's surfaces and typography while retaining their native structure. Sliders use flat circular thumbs with the action focus ring. OTP uses 34×40px cells and the monospaced family. Progress tracks are 6px with a hairline edge.

Form rows, form sections, page headers, stack/group layouts, icons, slots, collapsibles, and spinners use shared structure and semantic tokens where they need no independent visual recipe. Form sections retain their shared radius/typography scale. Page headers receive Concourse's title size and line-height hooks. Pagination wraps when its controls cannot fit one row; it does not hide available navigation actions.

Progress indicators cap their inherited height at the track's content height. The 6px bordered track has a 4px interior; an uncapped inherited height creates vertical overflow and native scrollbar arrows at the right edge. Verify with scrollbars visible using `util/visual-regression/verify-concourse-progress.mjs`, which covers all four real progress samples in both modes and viewport sizes.

Reduced-motion handling is scoped to library elements and their pseudo-elements, including the structural animations that outrank the theme layer. The supported theme selection, component markup, keyboard operation, and binding APIs do not change.

Sidebar default and selected items have no border, matching the handoff. A transparent border would expose the card background outside the inset blue selection rule. The outline variant retains its border while unselected; selected items use the same uninterrupted leading rule across variants.

Line tabs inside page-header navigation leave 2px of extra space below the list so the shared underline, positioned 5px below each trigger, fits within the scroll container. Check native scrollbars: ordinary navigation must not overflow vertically, while long labels must remain horizontally scrollable. This spacing belongs only to the header composition; standalone tabs retain their existing geometry.

## Implementation and verification

Consumers link one `_content/StellarAdmin.TagHelpers/stellar-admin.concourse.css` stylesheet and optionally load Source Sans 3 and IBM Plex Mono in the layout. DocsSamples support `?theme=concourse&mode=light|dark`; DocsSamples also exposes Concourse in its appearance picker. Website demo exports include the bundle, fonts, and theme-switcher entry.

Keep all visual component declarations in `@layer components.theme`, with body typography in the base layer. Do not import another theme as a fallback. Inspect the component's real DOM/state hooks before extending a recipe. If shared structure prevents a needed visual treatment, explain a narrow hook and compare other themes before changing it.

`util/theme-coverage/coverage.json` records explicit Concourse coverage for all 55 current component families, using actual rule evidence or a shared/composed rationale. The inventory check does not prove visual fidelity. New components require a coverage decision plus real rendered verification; adding a component not currently shipped remains a separate task.

Run `npm run build:css` in the library Client directory and verify nonempty output. Run `node util/theme-coverage/check.mjs` and `node --test util/theme-coverage/check.test.mjs` from the OSS repo. With DocsSamples running, use `node util/visual-regression/verify-concourse.mjs http://localhost:5206` for focus/selection/native-input/overlay checks, and `node util/segmented-control/check.mjs http://localhost:5206` for the shared tab appearance and form-binding contract across themes. The browser scripts need local socket/process access; a bounded font wait reports provider timeouts rather than hanging functional checks.

Verification for the initial implementation included 69 representative real partials in both modes at 1280px and 390px (276 captures), focused reruns after corrections, and same-browser source comparisons of primary action rest/hover/press geometry, typography, colours, shadows, and movement. All final reviewed sample compositions fit the viewport; focus, selection, grouped validation, native input, sheet containment/Escape, and reduced-motion checks passed. This is representative coverage, not an exhaustive accessibility audit or pixel comparison of every component against the handoff. See the [implementation record](../../plans/archive/concourse-theme.md) for precise results and environment notes.
