# Template folders

Status: implemented, uncommitted (2026-11-17). Layout decided by Jerrie the same day.

Rename the display and editor template folders so their names say what the templates are for, not where they were first used. Raised during the [page subtitles](page-subtitles.md) design (Jerrie, 2026-11-17): the record summary in the page header will render the grid's display templates, so "grid" no longer describes them.

## Today

Four folders under `src/StellarAdmin/Areas/StellarAdmin/Views/Shared/`, each with a `Displays/` or `Editors/` subfolder:

- `DisplayTemplates/`: the full-size displays, on the view page and in row details (`DisplayFieldTagHelper` resolves `DisplayTemplates/{name}`).
- `GridDisplayTemplates/`: the compact displays, in index grid cells and the record summary (`DataGridColumnTagHelper` and `PageSummaryRenderer` resolve `GridDisplayTemplates/{template}`). Badge, Boolean, Date, Enum and Number match the full set; Email, Link and Phone add `sa-display-link`; Html shows plain text; Text keeps to one line; Icon shows alone, its name on hover; Image is a thumbnail.
- `EditorTemplates/`: form editors (`EditorTemplateFinder.FormFolder`).
- `GridEditorTemplates/`: inline-editing editors in grid cells (`EditorTemplateFinder.GridFolder`). `ListEditor` and `DictionaryEditor` render `GridDisplayTemplates/Displays/Badge`.

## Decision

Only `DisplayTemplates/` and `EditorTemplates/` at the top. The full-size templates stay where they are; the compact ones move into a `Compact/` subfolder of each:

| Today | New |
| --- | --- |
| `DisplayTemplates/` | `DisplayTemplates/` (unchanged) |
| `GridDisplayTemplates/` | `DisplayTemplates/Compact/` |
| `EditorTemplates/` | `EditorTemplates/` (unchanged) |
| `GridEditorTemplates/` | `EditorTemplates/Compact/` |

A built-in compact template moves from `GridDisplayTemplates/Displays/Badge.cshtml` to `DisplayTemplates/Compact/Displays/Badge.cshtml`. Template names, including the built-ins' `Displays/` and `Editors/` prefixes, don't change. A compact lookup doesn't fall back to the full-size template. Old paths stop working; pre-release, so no compatibility shims.

Why the full-size templates don't get a subfolder of their own (Jerrie's first proposal had `Full/` or `Form/`):

- Form editors render through MVC's `html.Editor(...)` (`FieldEditorRenderer`), whose `TemplateRenderer` always looks in `EditorTemplates/{name}`.
- Sharing templates with MVC is deliberate ([identity user forms](../design/identity-user-forms.md)): a `[UIHint("X")]` template in the app's `Views/Shared/EditorTemplates/` serves both the admin and the app's own `Html.EditorFor`.
- StellarAdmin.UI documents the same convention: `<sa-display-field template="X">` resolves `DisplayTemplates/X` like `Html.DisplayFor`, and DataGrid edit templates call `Html.EditorFor` into `EditorTemplates/`.
- MVC never looks in subfolders, so `Compact/` doesn't affect `DisplayFor` or `EditorFor`.

Accepted cost: an app's own `Views/Shared/EditorTemplates/{type}.cshtml` still applies to admin forms, as today.

## Work

1. Library
   - `git mv` the built-in templates: `GridDisplayTemplates/Displays/*` (12) to `DisplayTemplates/Compact/Displays/`, `GridEditorTemplates/Editors/*` (13) to `EditorTemplates/Compact/Editors/`.
   - Lookups: `DataGridColumnTagHelper.ResolveDisplayTemplate` (StellarAdmin.UI), `PageSummaryRenderer.RenderTemplateAsync`, `EditorTemplateFinder.GridFolder` (becomes `EditorTemplates/Compact`), the Badge paths in `ListEditor` and `DictionaryEditor`.
   - Shared display lookup (Jerrie, 2026-11-17): the same find-the-view code (`GetView` then `FindView`, a `~/` or `/` path used as-is, an exception listing the searched locations) is copied in `DisplayFieldTagHelper.RenderTemplateAsync`, `DataGridColumnTagHelper.ResolveDisplayTemplate` and `PageSummaryRenderer.RenderTemplateAsync`. Fold the two StellarAdmin.UI copies into one internal helper that takes the folder (`DisplayTemplates` or `DisplayTemplates/Compact`); the grid keeps its per-grid cache on top. `PageSummaryRenderer` is in StellarAdmin, which can't see StellarAdmin.UI internals (no `InternalsVisibleTo`), so it uses `EditorTemplateFinder.FindView` (the same `GetView`/`FindView` lookup, already in StellarAdmin; it gains the `~/` and `/` rule and is renamed to a general template finder) and throws its own error when that returns null. That leaves one lookup helper per package (Jerrie, 2026-11-17); a single helper would need a public StellarAdmin.UI API or `InternalsVisibleTo`, both rejected.
   - Wording: "grid display template" errors become "compact display template"; rename the internal `DataGridContext.GridDisplayTemplates` cache.
   - Public XML docs: `DataGridColumnTagHelper.Template`, `FieldDisplay.TemplateName`, `FieldEditor.TemplateName`, `GridEditorHtmlHelperExtensions`; the comments in `EditorTemplateFinder` and `GridEditorRenderer`.
2. Apps and tests
   - Move app-side templates: `tests/StellarAdmin.IntegrationTests/Views/Shared/Grid*Templates/` (Price, ProductCode, SeatMap), `tests/StellarAdmin.ScenarioTesting.Host/Areas/StellarAdmin/Views/Shared/GridDisplayTemplates/` and the controller-local `Views/DisplayTemplatesBoolean/GridDisplayTemplates/`, `docs/DocsSamples/Pages/Shared/GridDisplayTemplates/`, `sandbox/AdminPlayground/Views/Shared/GridDisplayTemplates/`.
   - Paths in `GridEditorTests`, `AirportDisplay`, `DisplayFieldTagHelperTests` (and `.Templates`).
   - Build and run the tests.
3. Guidance
   - Consumer skills: `skills/stellar-admin/SKILL.md` and `references/` (`fields.md`, `view-pages.md`, `columns.md`, `editors.md`, `inline-editing.md`, `customization.md`); the hand-written regions of `skills/stellar-admin-ui/references/components/data-grid.md` and `details.md`; then `skills generate` and `skills check`.
   - `docs/design/identity-configuration.md`; the expected answer in `build/agent-evals/admin-panel/tasks.md`.
4. Website (`content/docs/admin/`): `display-templates.mdx`, `template-overrides.mdx`, `custom-displays.mdx`, `custom-editors.mdx`, `inline-editing.mdx`.

## As built

- StellarAdmin.UI: `DisplayTemplateFinder` (internal, `TagHelpers/DisplayTemplateFinder.cs`) holds the lookup for `DisplayFieldTagHelper` and `DataGridColumnTagHelper`, with `Folder` and `CompactFolder`. Its error reads "The display template '{view name}' was not found", the view name showing which folder was searched.
- StellarAdmin: a new internal `TemplateFinder` (`Areas/StellarAdmin/TemplateFinder.cs`) does the `GetView`/`FindView` lookup and keeps the searched locations, and holds `CompactDisplayFolder`. `EditorTemplateFinder.FindView` and `PageSummaryRenderer` call it; the `~/` and `/` rule stays in `PageSummaryRenderer`, so editor lookups are unchanged. `EditorTemplateFinder.GridFolder` became `CompactFolder`.
- The missing-cell-editor message reads "has no compact editor template".
- Left as is: prose that says "grid template" for a template used in a grid (`FieldEditorView.ErrorId` docs, a `GridEditorTests` comment, the Boolean screenshot scenario's description).

## Verification

Done 2026-11-17:

- `dotnet build StellarAdmin.slnx --configuration Release`: no errors; the one warning in a touched file (`GridEditorRenderer.cs:44`, CS8604) is on an untouched line.
- `dotnet test --solution StellarAdmin.slnx --configuration Release --no-build --minimum-expected-tests 1`: 2168 passed, 0 failed (unit, integration, EF Core, UI and scenario specs).
- `stellar skills generate` and `skills check`: 67 references up to date; the UI skill references needed no changes.
- Website: the five pages updated in `../website` (on `master`, uncommitted); not built.

Not done: no browser check of the playground or DocsSamples beyond the scenario specs.
