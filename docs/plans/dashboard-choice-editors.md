# Dashboard choice editors

Status: active, phases 1 and 2 implemented 2026-10-07; phase 2 awaiting review.

## Goal

Pick up the inline wins from the multiselect exploration (`sandbox/html/multiselect-inline.html`, committed in `e2f9f8f`) as native Dashboard editor features. The sheet and popover lookup patterns from the same exploration are out of scope and will be revisited separately.

## Decisions

- `ToggleButtonsEditor` is renamed to `SegmentedControlEditor`, matching the `sa-segmented-control` it renders. No compatibility alias: the Dashboard package is not published.
- `CheckboxGroupEditor` gets an `Appearance` (`CheckboxGroupAppearance.Default` or `Cards`, following `RadioGroupEditor.Appearance`), column counts per breakpoint through `Columns(int)` / `Columns(Action<GridColumnsBuilder>)`, and a `Flow` (`CheckboxGroupFlow.Down` by default, or `Across`). Breakpoints are the form's (30, 40 and 56rem) measured against the field's own width, since each form grid cell is a `field-group` container; a one-column form's field cell is about 47rem wide, a third-width cell about 22rem. Both flows render as CSS grid reusing the non-inheriting `--sa-cols*` variables; `Down` uses `grid-auto-flow: column` with `--sa-rows*` counts the server computes per breakpoint from the item count. The template marks the grid with `sa-choice-columns` / `sa-choice-columns-down` classes because Razor renders a null `data-*` attribute on a tag helper as empty.
- A new `ToggleGroupEditor` renders `sa-toggle-group` with an `Appearance` of `Chips` (default; rounded pills with a check mark when on), `Joined` or `Buttons`. The toggle group type follows the bound property: a collection property selects multiple values, any other property selects one.

## Phases

1. Rename `ToggleButtonsEditor` to `SegmentedControlEditor`. Implemented 2026-10-07.
2. `CheckboxGroupEditor` appearance, columns and flow; replace the Roles custom classes in the playground with `Columns(2)`. Implemented 2026-10-07.
3. `ToggleGroupEditor` with its three appearances and single or multiple selection by property type.

Each phase covers tests, playground gallery entries and the Dashboard consumer reference, and stops for review.

## Verification

Phase 1: renamed the editor, handler, `Editors/SegmentedControl` view, `EditorClassNamesMapper.ForSegmentedControl`, the playground gallery (`SegmentedControlGallery`, slug `segmented-control`), the integration test and the Dashboard setup reference. The playground builds; `StellarAdmin.Dashboard.Tests` (67) and `StellarAdmin.Dashboard.IntegrationTests` (331) pass.

Phase 2: added `CheckboxGroupAppearance`, `CheckboxGroupFlow`, `CheckboxGroupEditor.Columns`, the internal `ChoiceColumnsStyle`, the `--sa-rows*` properties and choice column rules in the Dashboard `client.css`, and generalized the `GridColumnsBuilder` docs to "the container". The playground Roles fields use `Columns(2)`, and the checkbox group gallery has "Appearance and columns" and "Columns in narrow fields" sections. Tests: `ChoiceColumnsStyleTests` (4) and four `ChoiceEditorTests` cases; `StellarAdmin.Dashboard.Tests` (71), `StellarAdmin.Dashboard.IntegrationTests` (335) and `StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests` (67) pass. Headless Chromium screenshots of the gallery at 1400px (light and dark) and 420px confirmed the down and across flows, cards in columns, equal card heights per row, and one column in half-width fields and on mobile. The Users resource requires sign-in and was not checked in the browser. `client.css` already failed `oxfmt --check` at HEAD; it was not reformatted.
