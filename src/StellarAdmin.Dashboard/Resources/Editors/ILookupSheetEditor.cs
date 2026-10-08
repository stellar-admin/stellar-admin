namespace StellarAdmin.Dashboard.Resources.Editors;

// What the lookup sheet needs from a single- or multi-select lookup editor
internal interface ILookupSheetEditor
{
    LookupItems? Items { get; }

    string? MediaClassName { get; }

    LookupSheetOptions SheetOptions { get; }
}
