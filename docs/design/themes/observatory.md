# Observatory

Observatory is a compact operations theme: cool neutral surfaces, a single blue signal that becomes cyan in dark mode, 1px hairlines, 4px corners, IBM Plex Sans UI text and IBM Plex Mono data. Jerrie selected the Claude Design handoff and compact density, and authorized autonomous implementation and verification on 2026-09-11.

## Authority and maintained sources

The maintained implementation is [observatory.css](../../../src/StellarAdmin.TagHelpers/Client/css/themes/observatory.css). Its foundation tokens preserve the exact supplied values. This specification and the maintained CSS are the authority for future controls, including the review decisions below. The original Claude Design handoff was removed at Jerrie's request on 2026-09-12 after extracting its reusable guidance; its source HTML/CSS informed the implementation rather than its downscaled screenshots or conflicting prose. The theme is independent of ThemeGenerator and imports no other theme.

Consumers link `stellar-admin.observatory.css`, use the existing `.dark` class, and optionally load IBM Plex Sans (normal 400/500/600) and IBM Plex Mono (normal 400/500) from their app layout. The library stylesheet makes no remote font request. Samples load these families and support `?theme=observatory&mode=light|dark`.

Font loading URL: `https://fonts.googleapis.com/css2?family=IBM+Plex+Sans:wght@400;500;600&family=IBM+Plex+Mono:wght@400;500&display=swap`. The handoff identifies the fonts as IBM releases under SIL OFL 1.1; no font binaries were supplied. Self-hosted applications must retain the applicable font license. The prototype's icon sprite, topology illustrations, simulated state classes and JavaScript were example assets, not runtime dependencies; use StellarAdmin's existing icons and behavior.

## Optional fonts and native fallbacks

Decision accepted on 2026-09-14 after reviewing both showcase pages in light and dark mode: retain the original font families for their character, with the approved native stacks as fallbacks. Font loading is optional for consumers. Linking only the theme bundle requires no font downloads; an application can add the Google Fonts stylesheet above (or self-host the fonts) to restore the original typography. The sample apps and exported website demos load only the selected theme's font families with `display=swap`, showing native text while fonts load and switching when ready. A late swap can change text widths and wrapping; applications that prefer to avoid a late swap can choose `display=optional`, which may keep the native font for that page view.

The maintained CSS uses the following family order:

```css
font-family: "IBM Plex Sans", system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
/* Data, identifiers and shortcuts */
font-family: "IBM Plex Mono", ui-monospace, "SFMono-Regular", Menlo, Consolas, "Liberation Mono", "DejaVu Sans Mono", monospace;
```

Preserve the existing sizes, weights, tracking and line heights. Native font selection varies by operating system and installed fonts; the accepted screenshots cover Linux Chromium, not Windows or macOS. Future visual checks should exercise both loaded web fonts and native fallbacks, including small labels, dense controls and text wrapping. The bundle must not add remote font imports or require JavaScript for fallback.

## Foundations

Exact palette, spacing, typography, radii, focus and motion values live in the opening foundation token sections of the maintained CSS. The source's semantic tokens use the private `--sa-observatory-*` prefix; public StellarAdmin palette and font variables map to them in both root and dark scopes, including inverted menus.

Compact is built in: 7px vertical table-cell padding. Default controls remain 32px high; small controls are 26px and large controls 40px. The library's extra-small variant, absent from the export, is inferred at 22px. App composition can choose larger controls for touch use. Body/control text is 13px; section headings 14px; dialog headings 15px; page titles 17px. Mono uppercase micro-labels are 9px. Default corners are 4px, nested controls 3px, badges and avatars circular/pill-shaped. Use `font-mono tabular-nums` on application identifiers, numbers and timestamps; CSS cannot identify numerical substrings in arbitrary text.

Native selects use 12.5px text as a rendering workaround: Chromium on Wayland at 1.6× display scaling can clip the options popup's bottom border with the original 13px metrics, depending on its screen position. Preserve the native select, control height and focus treatment. Review the actual desktop popup with `verify-native-select-popup.mjs`; headless screenshots did not reproduce this clipping. This is verified at the reported scale, not a guarantee against every browser/platform rounding defect.

Light cards have the source's small shadow; dark cards are shadowless. Dark menus and dialogs use the raised surface and retain the source's overlay shadow. Both modes have a 3px accent-tint focus halo, and focused invalid fields use the corresponding danger tint. State transitions use the source 120/180ms timings where existing structural hooks permit. Reduced motion suppresses library transitions and animations while leaving consumer elements alone.

## State rules

Neutral hover/pressed fills mean transient feedback. Persistent state uses a check/dot, selected surface, accent tint, or underline as appropriate. Dropdown checkbox/radio rows both use trailing checkmarks with no resting selected fill; hover stays neutral and keyboard focus adds an inset 2px accent rail. Explicit bold/inverted/translucent menu variants retain their existing public meaning.

Selected table rows retain their accent tint while hovered. Sidebar selection uses accent tint and accent text, without the Concourse inset bar. Selected default tabs and segmented controls share a white/dark-card thumb over a sunken track. Line tabs use accent text and the source's actual 2px underline, with room inside the scroll container. Checkbox checked + hover changes fill while retaining its indicator. Grouped invalid/focused fields decorate the outer group, with transparent inner controls. Model-bound validation classes and `aria-invalid` must both work.

## Composition recipes

- Empty states are borderless by default. Suppress only border width so an explicit `border` utility retains the theme's border color.
- Fields own vertical rhythm; standalone inputs own edges and focus. Input groups own their outside edge and focus ring, while addons own only internal separators and padding. Preserve block addons, textareas, nested controls, and existing button sizes. Keep inner focus reachable; do not hide overflow merely to conceal geometry defects.
- Cards own the border/radius/shadow, with a 12px/16px header, padded content, and a hairline-separated footer. Preserve headerless cards and small variants. Cards are bounded to the available width and Pro grid footers wrap on narrow screens. Tables contribute internal row borders and their own horizontal scrolling; do not force every table into a new wrapper or make headers universally sticky.
- Menus/popovers own their panel border and elevation. Keep the library's popup positioning, maximum viewport containment and scrolling. Existing icons remain in use rather than introducing the export's icon sprite.
- Dropdown checkbox and radio rows reserve a trailing 16px indicator slot, 12px from the right, with 36px right padding and 12px left padding. Reuse the existing SVG checkmark for both; unchecked indicators stay hidden. This approved treatment replaces the handoff's leading checkbox check and radio dot. Standalone radio controls retain their radio appearance.
- Dialogs use 440px default maximum width, existing small alert-dialog semantics, 16px content insets, and tinted footer surfaces. Sheets retain their explicit side and existing scrollable body/pinned footer. The example's automatic right-to-bottom switch is a page choice, not a theme-wide change to `side`.
- Default tabs and segmented controls use matching 3px track padding, 3px inner radii, 12px labels and 4px/12px item padding. Line-tab orientation and page-header scrolling remain functional.
- Sidebar floating/inset and collapse behavior preserve the library's DOM and breakpoints. Hide the navigation scrollbar while preserving wheel, touch and keyboard scrolling, as requested during review. The export's use of “inset” differs from StellarAdmin's inset-content contract; style the real variant rather than remapping its API.

## Inference and special review

1. **Source contrast claims:** the export claims 3:1 control borders and 4.5:1 text throughout, but its actual palette does not consistently meet those claims. For example, light control borders are about 1.54:1 on white; dark borders about 1.47:1 on the input surface; light warning text about 3.67:1 on its tint. Preserve its selected appearance for now; do not describe the theme as accessibility-certified. Review subtle borders, micro-labels, and light status text before adopting a stricter contrast target.
2. **Dark input appearance:** use `#121a24` over cards at `#0d1117`, following the authoritative CSS. It is lighter, despite the README calling it darker/recessed.
3. **Line-tab underline:** the CSS draws 2px even though prose mentions 3px. Follow the rendered source.
4. **Mobile behavior:** preserve library sheet sides, sidebar breakpoints and scrollable tables. The export's automatic bottom sheets and table-to-card replacement belong to app layout and would require changing component behavior.
5. **Unspecified families:** OTP composes field geometry and mono text; questionnaire composes fields/selection; attachments, bubbles and messages compose cards, badges and muted surfaces; empty states use quiet text; sliders preserve range/vertical semantics. These are best-effort extensions, not designs explicitly shown in the export.
6. **Existing sample issue:** shorthand alerts render empty title/description text in both Observatory and unchanged Nova. Explicit alert child tags render correctly; this is a separate Tag Helper issue, not an Observatory CSS defect.
7. **Density and extra-small sizing:** ship compact alone; no density preference UI. The 22px extra-small action is inferred so all library sizes remain distinct.

For additional families, use these retained handoff suggestions as inference starting points, not claims of implemented or approved new controls:

| Future control | Starting recipe |
| --- | --- |
| Calendar/date range | Grid of small-radius cells; accent-filled endpoints and accent-soft range span. Preserve keyboard and range semantics. |
| Combobox/multi-select | Input-group outer ownership, secondary badge chips, and menu/popover geometry; apply the approved trailing selection marks. |
| Tree/nested navigation | Sidebar/navigation treatment with 16px indentation per level and existing disclosure icons. |
| Charts | Line token for axes/grid, accent for the primary series and semantic status colors only where meaningful. |
| Upload/editor/grid editing | Existing field geometry and grouped-focus ownership; compose attachments/cards where appropriate. |
| Tooltip/toast behavior | Reuse maintained tooltip/toast styles; positioning, queues, dismissal and focus belong to existing component behavior. |

Command-palette active-descendant highlighting can use accent tint because it represents the active search result; do not copy that fill into resting checked dropdown rows. The original handoff did not address forced-colors or RTL, and current verification does not establish support for them. Review direction-sensitive rails, indicator positioning and disclosure icons when adding RTL support.

Horizontal line tab lists have no full-width bottom border; only the active tab draws its accent underline. This review preference applies to standalone tabs and page-header navigation.

## Extension and verification

Add future visual rules only to the maintained theme CSS under `@layer components.theme`, preserving shared structure. Consult the component Tag Helper, web component and shared CSS first. Existing hooks expose page title sizes, tab surfaces/indicator position, textarea minimum height and button transition timing. Keep explicit support in `util/theme-coverage/coverage.json`; its inventory validation alone does not establish visual fidelity.

Use normal DocsSamples routes and `DocsStatic` partials with the Observatory query parameter. Verify both modes at 1280px and 390px, with fonts loaded and native scrollbars visible. Review related pairs together: tabs/segmented controls, checkbox/radio menu items, input/grouped input, standalone/page-header line tabs. Exercise pressed focus, selected hover, grouped invalid focus, overlay containment, all sheet sides, sidebar collapse and reduced motion. Record actual outcomes and any remaining limitations in the [implementation plan](../../plans/archive/observatory-theme.md).

Run [verify-observatory.mjs](../../../util/visual-regression/verify-observatory.mjs) from the OSS repo with the sample base URL, for example `node util/visual-regression/verify-observatory.mjs http://localhost:5206`. `OBSERVATORY_FONT_CSS` can supply embedded font CSS; `OBSERVATORY_PRO_URL` enables the Pro grid check. Its Empty bordered/default cases guard semantic border composition, and its dropdown cases guard trailing visible checkmarks and hidden unchecked indicators. Use the [native popup check](../../../util/visual-regression/verify-native-select-popup.mjs) separately for the platform-specific select workaround, following the [visual verification README](../../../util/visual-regression/README.md).

After adding a control, update the coverage manifest and this specification for any new recipe, build CSS explicitly, and inspect the real component's states in both modes and widths. For CSS-only corrections, refresh the exported Observatory stylesheet without regenerating unrelated demo HTML. Consult the [custom-theme workflow](../custom-theme-workflow.md) for utility composition, native popup capture limits and the full review matrix.

Card headers immediately followed by footers share one 1px divider owned by the header; suppress the adjacent footer’s top border. Cards with intervening content retain both separate section rules. Verify all three themes with `node util/visual-regression/verify-card-dividers.mjs http://localhost:5206` from the product repository root. The regression reproduced the doubled borders before the fix and passed all six card compositions in light/dark at 1280px/390px after rebuilding; the website bundle was refreshed.
