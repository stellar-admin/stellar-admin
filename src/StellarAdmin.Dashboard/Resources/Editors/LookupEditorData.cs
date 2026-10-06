namespace StellarAdmin.Dashboard.Resources.Editors;

// What the lookup template needs from the handler: the selected item, and the controller of the resource whose
// create form adds new items, when the editor enables create and the current user may use it
internal sealed record LookupEditorData(LookupItem? Item, string? CreateController);
