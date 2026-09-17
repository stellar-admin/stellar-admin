# Aurora

Aurora is a dense operations theme with cool grey-green surfaces, a teal signal color, square containers, strong rules and no resting elevation. The user supplied the Claude Design handoff and authorized autonomous end-to-end implementation on 2026-09-12, explicitly selecting square sliders and rounded switches.

## Authority and maintained sources

The maintained implementation is [aurora.css](../../../src/StellarAdmin.TagHelpers/Client/css/themes/aurora.css). The user accepted the reviewed appearance on 2026-09-12. This specification and CSS govern extensions, including the card-divider and empty-border corrections below. The supplied runnable source informed the design; its scaled screenshots were illustrative. The handoff was removed at the user’s request on 2026-09-12 after preserving its reusable decisions in this specification and the maintained CSS. Aurora imports no other theme and stays outside ThemeGenerator; existing component selector/state recipes inform the mapping to real StellarAdmin markup.

Consumers link `stellar-admin.aurora.css` and use `.dark` normally. Optionally load Archivo normal 400/500/600 for headings and UI and IBM Plex Mono normal 400/500 for explicit data and shortcuts from the application layout. Font URL: `https://fonts.googleapis.com/css2?family=Archivo:wght@400;500;600&family=IBM+Plex+Mono:wght@400;500&display=swap`. The CSS does not fetch fonts. No font binaries were supplied; unused Archivo 700 is omitted.

## Optional fonts and native fallbacks

Decision accepted on 2026-09-14 after reviewing both showcase pages in light and dark mode: retain the original font families for their character, with the approved native stacks as fallbacks. Font loading is optional for consumers. Linking only the theme bundle requires no font downloads; an application can add the Google Fonts stylesheet above (or self-host the fonts) to restore the original typography. The sample apps and exported website demos load only the selected theme's font families with `display=swap`, showing native text while fonts load and switching when ready. A late swap can change text widths and wrapping; applications that prefer to avoid a late swap can choose `display=optional`, which may keep the native font for that page view.

The maintained CSS uses the following family order:

```css
font-family: "Archivo", "Helvetica Neue", Helvetica, Arial, "Liberation Sans", system-ui, sans-serif;
/* Data, identifiers and shortcuts */
font-family: "IBM Plex Mono", ui-monospace, "SFMono-Regular", Menlo, Consolas, "Liberation Mono", "DejaVu Sans Mono", monospace;
```

Preserve the existing sizes, weights, tracking and line heights. Native font selection varies by operating system and installed fonts; the accepted screenshots cover Linux Chromium, not Windows or macOS. Future visual checks should exercise both loaded web fonts and native fallbacks, including small labels, dense controls and text wrapping. The bundle must not add remote font imports or require JavaScript for fallback.

## Foundations

Exact palette, type, spacing and motion values live in the opening `--sa-aurora-*` sections of the maintained CSS, mapped to public semantic variables. Cards are white on cool grey-green in light mode, and near-black green in dark mode. Dark inputs recess below the card; overlays rise one surface. Deep teal becomes luminous teal in dark mode, with near-black text on filled controls. Shadows exist only on floating layers; cards and default selected tab surfaces remain flat.

Default controls are 30px, small 24px and large 38px; extra-small is inferred at 22px. Table cell vertical padding is 9px. Applications can override the private row-padding token for compact views without a new density preference API. Archivo sets body and controls at 13px, labels at 12px, helper text at 11px, titles at 14–17px/600. IBM Plex Mono sets 9px table headings and explicitly marked values, shortcuts and OTP. CSS cannot identify numeric substrings automatically; applications use `font-mono tabular-nums` where appropriate.

Containers, badges, sliders, progress tracks, tabs and overlays are square. Radios, switch tracks/thumbs, avatars, status dots, carousel position dots and loading spinners retain semantic circular geometry. Use 120/180ms motion where component contracts permit, and suppress motion for reduced-motion users. Focus is a hard 2px decoration with no blur.

## State and composition rules

Neutral hover/pressed surfaces are transient. Persistent selection uses a check, filled control, moved thumb, underline or teal tint as appropriate. Both menu checkbox and radio rows use trailing checkmarks without a resting selected fill; focus adds an inset accent rail. Keep unchecked indicators hidden and preserve explicit inverted/translucent/bold menu variants. In bold menus, use on-primary ink for the hovered/focused selection mark and keyboard rail so they remain visible on the filled row. Standalone radios retain a dot. Selected table rows keep their tint during hover.

Default tabs and segmented controls share a sunken track, 3px padding/gaps and a flat selected card surface. Horizontal line tabs have no full-width bottom border; only the active tab has a 2px accent underline. Vertical line tabs retain a side rule and active side marker. Grouped controls own the outer edge/focus while inner fields remain transparent and borderless; text/button addons supply internal separators. Focused invalid fields and groups use danger decoration; model-bound validation must work as well as explicit ARIA states.

Cards own borders, surface and section insets: 12px/16px headers, 16px bodies, 10px/16px footers. Small cards use 12px insets. When a header immediately precedes a footer, the header owns the single 1px divider and the footer suppresses its top border. With intervening content, both separate section rules remain. Preserve headerless cards and explicit utility overrides. Tables retain their wrapper and horizontal scrolling. Menus and overlays use stronger borders and floating shadows. Dialogs use a 440px maximum and 16px insets; small/media variants preserve their layout contracts. Sheets retain the selected side, scrollable body and reachable footer. Sidebar variants and collapse behavior stay within the existing component contract.

## Inferences and source corrections

- Optional empty-state borders use the standard `--border` surface rule, matching bordered tables; the stronger control border is reserved for inputs and other interactive controls. Default empty states remain borderless.
- Keep library breakpoints, table scrolling and declared sheet sides. Automatic table-to-card conversion, mobile bottom navigation and forced bottom sheets are application behavior, not theme CSS.
- Use established trailing checkmarks and underline-only line tabs despite the source's leading indicators and line-tab baseline. Match enclosed tabs and segmented geometry despite the source's 2px/3px gap discrepancy.
- Square slider tracks and thumbs follow the user's explicit decision. The source's rounded slider and 1px sparkline corners do not override it.
- Strengthen control borders from light `#b9c4c0` to `#85968e` and dark `#2e3836` to `#657973`; strengthen hover companions accordingly. Dark fields remain `#080d0c`. Light subtle text changes from `#6b7772` to `#606d67`. The original control borders measured only 1.79:1 and 1.62:1 against their input surfaces. Verify the corrected ratios rather than repeating source claims.
- Preserve the original accent, surfaces and status families. Explicitly measure relevant foreground/background pairs during verification; this is not an exhaustive accessibility certification.
- Infer unsupported designs from maintained primitives: OTP and questionnaire fields from inputs; attachments/messages/bubbles from cards and badges; future date pickers from square accent endpoints and tinted ranges. Do not introduce new components merely because the export mentions them.
- Preserve existing Lucide icons and runtime behavior. Prototype scripts, simulated state classes, icon sprite and topology diagram are reference material, not production dependencies.

## Adding controls

Choose the closest maintained Aurora recipe after inspecting the real Tag Helper, shared CSS and web component. These are starting points for future work, not claims that the controls already exist:

| New control or part | Aurora starting point |
| --- | --- |
| Searchable or multi-value picker | Input-group border and focus ownership, square secondary badges for values, menu panel with trailing checked-only indicators. |
| Calendar or date range | Square compact cells, teal-filled endpoints, quiet accent range fill and visible keyboard focus. Preserve date and range semantics. |
| Tree or step navigation | Sidebar disclosure/selection treatment; distinguish the current item from transient hover and preserve keyboard navigation. |
| Upload area or editor | Field/group focus and validation, quiet surface border for the container, card insets and attachment rows. |
| New overlay | Floating elevation, square corners, Archivo heading, constrained scrolling and reachable actions. |
| Data display | Table spacing, quiet row dividers, semantic status badges and explicit mono/tabular values. |

Use `--border` for passive surface edges and dividers, including optional Empty borders; use the stronger input/control recipe where an interactive boundary must be visible. Match Empty to a bordered table in the same mode rather than assuming any non-text border should use the strongest token. Preserve rounded semantic controls such as switches and radios while keeping sliders square.

Exercise supported sizes, orientations and combined states, including selected or checked plus hover/focus and invalid plus focus. In filled menu variants, indicators and keyboard rails need foreground ink that contrasts with the actual row fill. Check the compiled focus decoration: Tailwind arbitrary properties can compile successfully with the wrong inferred type.

## Extension and verification

Read the Tag Helper, shared CSS and web component before extending rules under `@layer components.theme`. Keep appearance in Aurora, preserve utility overrides and update explicit theme coverage after reviewing actual rules and rendered states. Shared tabs, segmented controls and toggles expose optional focus-width/color/outline hooks for Aurora’s 2px ring; fallbacks retain the original 3px ring and 1px outline. The ring-width utility uses Tailwind’s explicit `length:` hint so it compiles as a width rather than a color. No shared default should change without an explanation and compatibility check.

Use ordinary DocsSamples with `?theme=aurora&mode=light|dark` at 1280px and 390px, with fonts loaded and native scrollbars visible. Compare default tabs with segmented controls, both menu indicator types, grouped validation/focus, progress/header overflow, sliders, switch states, small/media dialogs, all sheet sides and sidebar variants. Run `util/visual-regression/verify-aurora.mjs` against the sample app; optional `AURORA_FONT_CSS` supplies local font CSS and `AURORA_PRO_URL` enables Pro checks. For card changes, run [verify-card-dividers.mjs](../../../util/visual-regression/verify-card-dividers.mjs) against the same sample app; it compares adjacent header/footer and header/content/footer compositions across Aurora, Meridian and Observatory. For optional Empty borders, compare the real `Empty/_WithBorder` and `Table/_Border` samples in both modes and confirm removing the `border` utility restores a borderless Empty. Outcomes and outstanding limits live in the [implementation plan](../../plans/archive/aurora-theme.md).

## Verified limits

The audit rendered all 53 full sample routes in light/dark at 1280px/390px (212 renders), alongside focused state and overlay checks. Full-page AlertDialog, Badge, Dialog, Item, Popover, Separator, Sheet, Table and Tooltip demos still overflow narrow viewports because of existing fixed-width/nowrap sample compositions; isolated Aurora dialogs/sheets and the Pro grid stay contained. Consumer `rounded-full` utilities and class-name customization examples intentionally override Aurora geometry. Carousel position dots remain round navigation indicators.

Control borders measure 3.11:1 against white fields and 4.23:1 against recessed dark fields. Corrected light subtle text measures 4.76:1 on the page and 4.56:1 on sunken surfaces; positive/warning/danger text on their light tints measures 4.56:1/4.84:1/5.55:1. These checks cover the supplied roles, not every consumer composition or accessibility requirement. RTL, forced-colors mode, exhaustive assistive-technology behavior and native select popup painting at fractional desktop scaling remain unverified.

Refresh `../website/public/demo/tag-helpers/assets/stellar-admin.aurora.css` after CSS-only refinements. Changes to shared focus hooks require refreshing the other bundles as well. Regenerate HTML only when sample/export markup changes. Use this specification and the maintained CSS for future work; the temporary handoff is no longer required.
