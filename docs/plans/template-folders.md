# Template folders

Status: open (2026-11-17).

Rename the display and editor template folders so their names say what the templates are for, not where they were first used. Raised during the [page subtitles](page-subtitles.md) design (Jerrie, 2026-11-17): the record summary in the page header will render the grid's display templates, so "grid" no longer describes them.

## Today

Four folders under `src/StellarAdmin/Areas/StellarAdmin/Views/Shared/`, each with a `Displays/` or `Editors/` subfolder:

- `DisplayTemplates/`: the full-size displays, on the view page and in row details (`DisplayFieldTagHelper` resolves `DisplayTemplates/{name}`).
- `GridDisplayTemplates/`: the compact displays, in index grid cells (`DataGridColumnTagHelper` resolves `GridDisplayTemplates/{template}`). Badge, Boolean, Date, Enum and Number match the full set; Email, Link and Phone add `sa-display-link`; Html shows plain text; Text keeps to one line; Icon shows alone, its name on hover; Image is a thumbnail.
- `EditorTemplates/`: form editors (`EditorTemplateFinder.FormFolder`).
- `GridEditorTemplates/`: inline-editing editors in grid cells (`EditorTemplateFinder.GridFolder`). `ListEditor` and `DictionaryEditor` render `GridDisplayTemplates/Displays/Badge`.

## Proposal

Jerrie's proposal, open for the implementing agent to refine: only `DisplayTemplates/` and `EditorTemplates/` at the top, each with a `Compact/` folder for the compact templates and a second folder, such as `Form/` or `Full/`, for the full-size ones.

## To decide

- The name of the full-size folder.
- `DisplayTemplates/` and `EditorTemplates/` are also MVC's own conventions: `Html.DisplayFor`, `Html.EditorFor` and `[UIHint("X")]` look up `DisplayTemplates/X` and `EditorTemplates/X`, and applications override built-in templates by adding a file at the same path. Moving the full-size templates into a subfolder changes the paths applications override and may change what `[UIHint]` finds; check how the current lookups, the data-type templates in `EditorTemplates/` and application overrides resolve before choosing.
- Whether an application's own templates keep working at the old paths, or the docs only describe the new ones (pre-release, so no compatibility shims are required).

## Affected

- Code: `DataGridColumnTagHelper`, `DataGridContext`, `DisplayFieldTagHelper` (StellarAdmin.UI); `EditorTemplateFinder`, `GridEditorRenderer`, `GridEditorHtmlHelperExtensions`, the `FieldDisplay.TemplateName` and `FieldEditor` XML docs, and the views that render templates by path (StellarAdmin).
- Docs: `docs/design/identity-configuration.md`.
- Consumer skills: `skills/stellar-admin/references/` (`fields.md`, `view-pages.md`, `columns.md`, `editors.md`, `inline-editing.md`, `customization.md`).
- Website: `content/docs/admin/` pages `template-overrides.mdx`, `display-templates.mdx`, `inline-editing.mdx` and `custom-editors.mdx`.
