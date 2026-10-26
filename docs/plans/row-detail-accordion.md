# Row detail panels as an accordion

Status: proposed (2026-10-26).

Add a variant that displays a row's detail panels as an accordion instead of tabs.

Today `_IndexDataGrid.cshtml` always displays several panels in `<sa-tabs>`, and `ResourceRowDetailsBuilder` has no setting to change it. StellarAdmin.UI already has an accordion (`src/StellarAdmin.UI/TagHelpers/Accordion/`). Open questions include whether more than one panel can be open at once, and how panels load their data when they're expanded (tabs load on first select).

To be filled out before implementing.
