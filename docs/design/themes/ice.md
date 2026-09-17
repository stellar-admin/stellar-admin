# Ice theme

## Authority and intent

Ice is an independent high-density theme for operational business screens: square geometry, hairline separation, no elevation shadows, compact controls, and one blue interaction accent. The approved Claude Design handoff supplies the foundation; its `theme.css` takes precedence over original inline prototypes and screenshots. Subsequent user-approved refinements recorded here and in the maintained CSS supersede the handoff. Jerrie authorized implementation and best-effort decisions without intermediate review on 2026-09-11.

The maintained implementation is [ice.css](../../../src/StellarAdmin.TagHelpers/Client/css/themes/ice.css). Exact source values are preserved in its `--sa-ice-*` foundation tokens; this document describes their meaning rather than maintaining another palette. The temporary handoff was removed at Jerrie’s request after extraction; this specification, maintained CSS, and real samples are sufficient for future component work. Do not import another theme or add Ice to ThemeGenerator.

## Optional fonts and native fallbacks

Decision accepted on 2026-09-14 after reviewing both showcase pages in light and dark mode: retain the original font families for their character, with the approved native stacks as fallbacks. Font loading is optional for consumers. Linking only the theme bundle requires no font downloads; an application can add the Google Fonts stylesheet in Foundations (or self-host the fonts) to restore the original typography. The sample apps and exported website demos load only the selected theme's font families with `display=swap`, showing native text while fonts load and switching when ready. A late swap can change text widths and wrapping; applications that prefer to avoid a late swap can choose `display=optional`, which may keep the native font for that page view.

The maintained CSS uses the following family order:

```css
font-family: "IBM Plex Sans", system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
/* Data, identifiers and shortcuts */
font-family: "JetBrains Mono", ui-monospace, "SFMono-Regular", Menlo, Consolas, "Liberation Mono", "DejaVu Sans Mono", monospace;
```

Preserve the existing sizes, weights, tracking and line heights. Native font selection varies by operating system and installed fonts; the accepted screenshots cover Linux Chromium, not Windows or macOS. Future visual checks should exercise both loaded web fonts and native fallbacks, including small labels, dense controls and text wrapping. The bundle must not add remote font imports or require JavaScript for fallback.

## Foundations

Select the single `stellar-admin.ice.css` bundle and use the normal `.dark` class for dark mode. Geometry is identical in both modes. Dark surfaces become lighter than the canvas; row separators become darker than structural borders. The readable accent and bright marker/fill accent are separate in light mode, and converge in dark mode. Solid primary actions use white ink in light mode and dark ink in dark mode. Never replace the readable accent with the bright marker tint for small light-mode text.

Optionally load IBM Plex Sans at weights 400/500/600 and JetBrains Mono at 400/500/700 from the app layout, either self-hosted or through a font provider. The bundle specifies font families without fetching fonts. The provider URL is `https://fonts.googleapis.com/css2?family=IBM+Plex+Sans:wght@400;500;600&family=JetBrains+Mono:wght@400;500;700&display=swap`; preserve the families’ SIL Open Font License notices when redistributing font binaries. UI text uses Plex Sans; numeric data, identifiers, badges, keyboard hints, and navigation micro-labels use Mono. Consumer-authored numeric content should use `font-mono tabular-nums`; the theme cannot infer which arbitrary text is an identifier.

Default controls are 28px with 12.5px UI text. Small/extra-small/large actions use the handoff's 24/22/34px scale. Native checkbox/radio geometry is 14px; switches are square. Radii are zero except native radio circles and spinner geometry. Every visible structural/control border is one pixel; selection markers are two pixels. Surfaces use hairlines rather than elevation. Keep existing component responsiveness; do not turn the handoff's 390px phone frame or desktop three-column composition into global layout requirements.

Colour transitions use 90ms and switch movement uses 140ms. Reduced-motion handling is scoped to library elements. Existing library show/hide mechanics and positioning remain functional; motion hooks must preserve those contracts.

## State rules

Hover and pressed feedback use neutral washes; selection uses a tint plus an inset blue marker or a blue border. Primary actions use the dedicated hover/pressed accent values. Destructive actions are outlined; destructive confirmation buttons within alert-dialog footers are filled. Validation uses a danger border at rest and a danger focus wash while focused. A field group owns its focus/validation ring; its inner input remains transparent, including read-only, disabled, and invalid combinations.

Dropdown checkbox/radio selection uses trailing checkmarks and medium label weight. There is no resting selected row fill. Keyboard menu focus uses a neutral background and inset blue marker; pointer hover uses a neutral wash. Keep existing menu ARIA state, roving keyboard navigation, disabled handling, and submenu behavior.

Default tabs and segmented controls share the approved quiet treatment: square corners, a three-pixel inset well and gap, and a one-pixel input-colour outer border. Unselected options have transparent surfaces and borders with muted labels. Only the selected option has a white light-mode surface or raised dark-mode surface, neutral hairline, and blue label; keyboard focus retains its blue outline. Line tabs use a blue underline inside their own box; header navigation must not acquire a vertical scrollbar. Long navigation remains horizontally scrollable. Preserve both tab orientations and native segmented-control radio binding.

Checked and indeterminate checkboxes use the primary accent background, matching the radio and switch accent, with a white check or dash in both modes. This replaces the handoff’s bright fill and dark mark.

## Composition recipes

| Family | Recipe and ownership |
| --- | --- |
| Fields and input groups | Wrapper owns border and ring; text addons own a dividing hairline and muted surface, button addons align to the inner height. Inner controls never duplicate the ring. Block addons retain natural multiline height. |
| Cards and form sections | Surface and one hairline; section children own their padding. Use the shared gap vocabulary with Ice values. |
| Menus and popovers | Surface, control border, square corners, no elevation. Checkbox/radio menu items reserve a trailing indicator gutter; supplied icons use StellarAdmin's existing icon pack. |
| Dialogs and sheets | Control border, modal scrim, compact title/body/footer spacing. Dialogs retain viewport containment and legitimate content scrolling; sheets preserve all four sides and reachable footer actions. Existing animation variables disable sheet sliding, and backdrop blur is zero so the scrim stays flat. |
| Tables | Data cells have a 36px minimum height with 6px vertical and 10px horizontal padding, giving rows more breathing room. Headers retain their compact mono micro-headings and 7px vertical padding. Keep quiet row separators; selected rows pair tint with an inset marker. In-cell badges remove their border and tighten padding. Cards stay within the parent width, table containers own horizontal scrolling, and card footers wrap controls when necessary. Content can grow rows when required. |
| Navigation | Neutral resting rows, blue current label and inset marker, mono micro-labels; collapsed, inset, floating, and mobile layouts follow real sidebar structure. Floating sidebars use a square bordered panel with an 8px outer gutter; the main header aligns to that gutter and has its own hairline border. Inset layouts place a square bordered card surface on the neutral canvas, with the sidebar blending into that canvas. These desktop treatments use no shadows; mobile retains the overlay sidebar. Sidebar content hides its scrollbar while remaining scrollable. |
| Other families | Compose square surfaces, compact controls, semantic palette and existing behavior. Chat bubbles, attachments, questionnaires, sliders and OTP have explicit theme rules. Structural-only tags use the shared palette/gaps and their themed children. |

DocsSamples retains its inset sidebar variant. Its navigation content and example wrappers remain transparent so the enclosing theme surface shows through. Do not add blanket `bg-background` utilities to these wrappers or switch sidebar variants to conceal a surface mismatch.

## Inferences and intentional limits

The existing Lucide icon system is retained in place of Unicode placeholders. The roomier table rows supersede the handoff's 28px treatment; no new density API is introduced. Five chart tokens use a tonal blue scale to avoid introducing decorative outcome colours; differentiation for complex multi-series charts remains an app-level design decision. Validation summaries can compose the existing destructive alert and field-error treatments. Forced-colors fallbacks provide explicit focus/selection outlines; this is not a claim of a full accessibility audit. The handoff did not design RTL, and directional sidebar/menu selection markers retain the current library convention.

No new component API is introduced for prototype-only comboboxes, calendars, or toast behavior. Preserve the public components that actually ship. Shared CSS has optional active-label/border and underline-position hooks for tabs and segmented controls, plus separate checkbox fill/ink hooks; their fallbacks preserve other themes. Any future shared changes must explain the blocked customization and verify compatibility.

For future components, compose existing recipes: steppers use grouped selection with the current step styled like an active tab; trees use navigation rows with 14px indentation per level; upload dropzones use a dashed control border, changing to an accent border and selected tint during drag; tag inputs use chips within a field whose wrapper owns focus; popovers use the menu shell with 16px panel padding. These are inheritance starting points, subject to actual component semantics and the approved refinements above. More complex drag/editing interactions, print layouts, and new empty/loading states require component-specific verification rather than assumed prototype coverage.

Horizontal line tab lists have no full-width bottom border; only the active tab draws its accent underline. This review preference applies to standalone tabs and page-header navigation.

## Extension and verification

Before extending Ice, inspect the component's actual `.sa-*` hooks, DOM, slots, state attributes and client behavior. Derive appearance from the recipes above, implement all supported sizes/variants/orientations, then update `util/theme-coverage/coverage.json` with real rule evidence or a structural-only rationale. Coverage detects inventory omissions, not visual fidelity.

Review ordinary DocsSamples with `?theme=ice&mode=light` or `?theme=ice&mode=dark`. Useful routes include `/Button`, `/InputGroup`, `/Tabs`, `/SegmentedControl`, `/DropdownMenu`, `/Table`, `/AlertDialog`, `/Sheet`, `/Sidebar`, `/PageHeader`, and `/Progress`. Use native-scrollbar-visible captures at desktop/mobile widths, inspect computed geometry and screenshots, and test keyboard focus separately from selected state. Build CSS explicitly and confirm nonempty output before browser review. Run `node util/visual-regression/verify-ice.mjs http://localhost:5206` in OSS for focus/validation, menu selection, checkbox fill/ink consistency, keyboard behavior, reduced motion and forced-colors focus. The matching Pro script uses port 5207 and checks mobile grid containment, table scrolling, query preservation and selection. Implementation status and actual verification are recorded in [the Ice plan](../../plans/archive/ice-theme.md).
