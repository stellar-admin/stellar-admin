# Global labels rework

Status: proposed (2026-10-26).

Rework the global labels (`ConfigureResourceLabels()` and its builders in `src/StellarAdmin/Resources/Builders/`) so they are better organized, and make sure they cover all the admin panel's text.

- Organization: the current grouping (index, create, edit, view, delete, lookup, list editor, dictionary editor, sheet) mixes pages, editors and the sheet, and some groups take functions while `Sheet()` takes strings.
- Coverage: some text is hard-coded in the views, such as "No records found", "Nothing here yet", "Back" and "Cancel" (`_IndexDataGrid.cshtml`, `_FormPage.cshtml`, `_CreateSheet.cshtml`, `_ViewPage.cshtml`, `_HomeResources.cshtml`). Audit all of it.
- Update the website page `content/docs/admin/global-labels.mdx` to match.

To be filled out before implementing.
