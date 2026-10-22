namespace StellarAdmin.Resources.Editors;

// What the multi-select lookup template needs from the handler: the selected items, in the order of the values, and
// whether the items have media
internal sealed record MultiLookupSheetEditorData(IReadOnlyList<ChoiceItem> Items, bool HasMedia);
