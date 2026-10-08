namespace StellarAdmin.Dashboard.Resources.Editors;

// What the multi-select lookup template needs from the handler: the selected items, in the order of the values
internal sealed record MultiLookupSheetEditorData(IReadOnlyList<ChoiceItem> Items);
