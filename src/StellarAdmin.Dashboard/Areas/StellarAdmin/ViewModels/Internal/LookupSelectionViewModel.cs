using StellarAdmin.Dashboard.Resources.Editors;

namespace StellarAdmin.Dashboard.Areas.StellarAdmin.ViewModels.Internal;

internal sealed record LookupSelectionViewModel(
    LookupItem? Item,
    LookupEditorLayout Layout,
    bool ShowMedia,
    bool IsReadOnly
);
