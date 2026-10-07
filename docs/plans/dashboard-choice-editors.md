# Dashboard choice editors

Status: active, phase 1 implemented 2026-10-07 and awaiting review.

## Goal

Pick up the inline wins from the multiselect exploration (`sandbox/html/multiselect-inline.html`, committed in `e2f9f8f`) as native Dashboard editor features. The sheet and popover lookup patterns from the same exploration are out of scope and will be revisited separately.

## Decisions

- `ToggleButtonsEditor` is renamed to `SegmentedControlEditor`, matching the `sa-segmented-control` it renders. No compatibility alias: the Dashboard package is not published.
- `CheckboxGroupEditor` gets a `Variant` (`CheckboxGroupEditorVariant.List` by default, or `ChoiceCards`), column counts per breakpoint through `Columns(int)` / `Columns(Action<GridColumnsBuilder>)`, and a `Flow` (`Down` by default, or `Across`). Breakpoints are measured against the field's own width, since each form grid cell is a `field-group` container. Both flows render as CSS grid; `Down` uses `grid-auto-flow: column` with a row count the server computes per breakpoint from the item count.
- A new `ToggleGroupEditor` renders `sa-toggle-group` with a `Variant` of `Chips` (default; rounded pills with a check mark when on), `Joined` or `Buttons`. The toggle group type follows the bound property: a collection property selects multiple values, any other property selects one.

## Phases

1. Rename `ToggleButtonsEditor` to `SegmentedControlEditor`. Implemented 2026-10-07.
2. `CheckboxGroupEditor` variant, columns and flow; replace the Roles custom classes in the playground with `Columns(2)`.
3. `ToggleGroupEditor` with its three variants and single or multiple selection by property type.

Each phase covers tests, playground gallery entries and the Dashboard consumer reference, and stops for review.

## Verification

Phase 1: renamed the editor, handler, `Editors/SegmentedControl` view, `EditorClassNamesMapper.ForSegmentedControl`, the playground gallery (`SegmentedControlGallery`, slug `segmented-control`), the integration test and the Dashboard setup reference. The playground builds; `StellarAdmin.Dashboard.Tests` (67) and `StellarAdmin.Dashboard.IntegrationTests` (331) pass.
