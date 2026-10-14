using StellarAdmin.Resources.Editors;

namespace StellarAdmin.Areas.StellarAdmin.ViewModels.Internal;

internal sealed record LookupSelectionViewModel(
    ChoiceItem? Item,
    LookupSheetEditorLayout Layout,
    bool ShowMedia,
    bool IsReadOnly,
    string? MediaClass
);
