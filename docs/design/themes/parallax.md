# Parallax

Parallax is a product-engineering theme with cool blue-grey structure, one red-orange action accent, nested corners and real elevation in both light and dark modes. The user supplied the Claude Design handoff and authorized autonomous end-to-end implementation on 2026-09-13, with visual refinements to follow.

## Authority and sources

The maintained implementation is [parallax.css](../../../src/StellarAdmin.TagHelpers/Client/css/themes/parallax.css). This specification and CSS govern extensions. The original Claude Design handoff informed the foundation and was removed after user approval on 2026-09-13. Reusable design decisions and exact values are preserved in this specification and the maintained CSS. Parallax imports no other theme and stays outside ThemeGenerator. Existing custom-theme selector/state recipes supply the mapping to real StellarAdmin markup.

Consumers link `stellar-admin.parallax.css` and use `.dark`. Optionally load Space Grotesk normal 400/500/600 for UI and headings, and JetBrains Mono normal 400/500 for explicit data and shortcuts in the application layout. URL: `https://fonts.googleapis.com/css2?family=Space+Grotesk:wght@400;500;600&family=JetBrains+Mono:wght@400;500&display=swap`. CSS does not fetch fonts. The unused Space Grotesk 700 weight is omitted.

## Optional fonts and native fallbacks

Decision accepted on 2026-09-14 after reviewing both showcase pages in light and dark mode: retain the original font families for their character, with the approved native stacks as fallbacks. Font loading is optional for consumers. Linking only the theme bundle requires no font downloads; an application can add the Google Fonts stylesheet above (or self-host the fonts) to restore the original typography. The sample apps and exported website demos load only the selected theme's font families with `display=swap`, showing native text while fonts load and switching when ready. A late swap can change text widths and wrapping; applications that prefer to avoid a late swap can choose `display=optional`, which may keep the native font for that page view.

The maintained CSS uses the following family order:

```css
font-family: "Space Grotesk", "Helvetica Neue", Helvetica, Arial, "Liberation Sans", system-ui, sans-serif;
/* Data, identifiers and shortcuts */
font-family: "JetBrains Mono", ui-monospace, "SFMono-Regular", Menlo, Consolas, "Liberation Mono", "DejaVu Sans Mono", monospace;
```

Preserve the existing sizes, weights, tracking and line heights. Native font selection varies by operating system and installed fonts; the accepted screenshots cover Linux Chromium, not Windows or macOS. Future visual checks should exercise both loaded web fonts and native fallbacks, including small labels, dense controls and text wrapping. The bundle must not add remote font imports or require JavaScript for fallback.

## Foundations

Exact tokens live in the opening `--sa-parallax-*` sections of the maintained CSS. Dark is the source design's origin: near-black blue ground, raised blue-grey cards, recessed inputs, brighter orange with near-black ink, and shadows retained at every elevation. Light uses a cool grey ground and white cards. Separate accent fill and text roles are essential in light mode. Charts compose neutrals and status colors without the action accent.

Default/small/large controls are 32/26/40px; extra-small is inferred at 22px. Table cell vertical padding is 10px, with compact applications able to override `--sa-parallax-row-pad-y` to 7px without a new theme preference API. Body and controls are 13px; headings use 600 weight and negative tracking. Mono text uses tabular figures. CSS cannot identify numeric substrings, so applications mark data with `font-mono tabular-nums`.

Owners use 6px corners; nested surfaces use 4px; progress bars use 2px. Badges, avatars, switches, sliders and radios remain round. Top/bottom sheets use 10px exposed corners; declared sheet sides remain authoritative. Motion tokens are 120/180ms where shared component contracts permit. Reduced-motion users receive suppressed transitions/animations, including overlay backdrops.

## State and composition rules

Transient hover/pressed feedback uses neutral surfaces. Persistent selection uses a tick, filled control, moved thumb, raised surface, underline or tint according to the component. Dropdown checkbox and radio items use trailing checkmarks without persistent row fill; focus adds an inset accent rail. Bold menu feedback switches marks and rails to on-primary ink. Standalone radios retain dots. Selected table rows retain their tint on hover.

Default tabs and segmented controls share a sunken track, 3px inset/gaps and a raised selected surface. Horizontal line tabs have only a 2px active underline; vertical tabs retain a side divider and marker. Input groups own the border/focus, and inner inputs stay transparent and borderless even when read-only or invalid. Focused invalid groups use a danger ring. Keep disabled checked marks visible.

Button-group text addons use the same strong control border as adjacent outline buttons and inputs. Individual controls retain their own focus indication.

Standalone toggles and spaced toggle-group items use 6px corners. Joined groups round only their outside corners in either orientation; a single-item group rounds all four. Selected toggles retain the strong control border used by unselected outline toggles, with the accent tint indicating selection. Focus and invalid borders retain their semantic colors.

Cards own their border, radius and shadow. Headers use 12px/16px insets, bodies 16px, footers 10px/16px; small cards use 12px horizontal insets. Touching header/footer sections share one divider. Tables keep their wrapper and horizontal scrolling. Menus and overlays use elevated surfaces and stronger borders. Dialogs use 440px maximum width and 16px insets. Sheets retain scrolling and reachable footer actions; sidebar variants keep the existing responsive behavior. Optional Empty borders use the ordinary surface border, and the default remains borderless.

## Inferences and source corrections

- Preserve trailing menu checks and underline-only horizontal tabs from the established workspace preference. Reconcile enclosed tabs and segmented controls to the same 3px gap.
- Light primary fill changes from `#d94d13` to `#cf4811` while preserving the red-orange hue; accent text stays `#a83a0e`. This addresses the original 4.18:1 white-label contrast. Light control borders change from `#c4ccd7` to `#8793a3`; dark control borders change from `#333c49` to `#64748b`. Hover companions change to `#718096` and `#8290a3`. Light warning text changes from `#a06a00` to `#925f00`, positive text from `#0d8055` to `#0c7b51`, and subtle text from `#6c7683` to `#636d7a` for their tinted/sunken surfaces. Verify actual pairings; do not repeat the export's inaccurate contrast claims.
- Preserve component behavior, Lucide icons, utility overrides and public APIs. Prototype scripts, state-simulation classes, icon sprites and topology artwork are not production dependencies.
- Automatic table-to-card conversion, bottom navigation, forced mobile bottom sheets and example-specific sidebar breakpoints are application behavior, not theme requirements.
- Use real checkbox indicator markup to avoid the prototype's disabled tick loss. Suppress reduced-motion animations instead of stretching all overlay entrances to two seconds.
- Infer OTP/questionnaire fields from inputs; attachments, messages and bubbles from cards/badges; future calendars from accent endpoints and tinted ranges; future pickers from input groups and menu panels. Do not add new components merely because the export mentions them.

## Mapping future controls

Use these accepted primitives when extending Parallax; exact tokens and state selectors remain in the maintained CSS.

| New control or part | Starting recipe | Composition checks |
| --- | --- | --- |
| Text entry, picker trigger or editable value | Input/input-group surfaces, 32/26/40px sizes, strong control border and 6px owner corners | One border/focus owner for a combined field; transparent inner inputs, read-only and invalid-plus-focused states. |
| Toggle-like choice or toolbar option | Toggle tint and strong selected border; 6px standalone corners | Spaced items round independently; joined items round only exposed corners in both orientations, including a single-item group. |
| Static addon beside an action or field | Button-group text addon with `--sa-parallax-line-strong` | Match adjacent resting control borders; preserve individual focus and square joined seams. |
| Exclusive view navigation | Tabs/segmented control with sunken track and raised selection | Match 3px inset/gaps; horizontal line variant has only the active underline. |
| Selectable menu option | Existing dropdown checkbox/radio item | Trailing check, no resting selected fill, keyboard focus rail and contrasting marks on bold rows. |
| Attachment, message or information panel | Card/badge surfaces and nested 4px corners | Quiet surface borders, one divider per touching section, elevation in both modes. |
| Calendar or richer picker | Accent endpoints/tinted ranges; input-group trigger and menu panel | Separate persistent selection from hover/focus, preserve popup semantics and reachable content. |

## Extension and verification

Inspect the Tag Helper, shared CSS, web component and state attributes before adding theme rules under `@layer components.theme`. Keep visual values here and in the CSS; preserve structural contracts. Maintain explicit component coverage and compare related controls, not only the inventory.

Use ordinary DocsSamples with `?theme=parallax&mode=light|dark` at 1280px/390px, native scrollbars visible and fonts loaded. Run `node util/visual-regression/verify-parallax.mjs http://localhost:5206`; `PARALLAX_FONT_CSS` supplies local font CSS and `PARALLAX_PRO_URL` enables the Pro grid checks. Check grouped focus/validation, checked and focused menus, default tabs/segments, header overflow, progress, small/media dialogs, all sheet sides and sidebar variants. Record actual outcomes and remaining limits in the [implementation plan](../../plans/archive/parallax-theme.md).

For corner and border refinements, compare `/Toggle`, `/ToggleGroup` and `/ButtonGroup`, including the isolated `ButtonGroup/_WithText` sample. Check resting borders after transitions settle and use keyboard navigation for focus-visible checks. The Part Classes demos deliberately override theme styling and should be assessed as consumer composition. Rebuild CSS and refresh the exported Parallax stylesheet after CSS changes; update coverage and follow the component-porting workflow when adding a component.
