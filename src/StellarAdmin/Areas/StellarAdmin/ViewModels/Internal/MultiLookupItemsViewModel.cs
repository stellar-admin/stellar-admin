using StellarAdmin.Resources;
using StellarAdmin.Resources.Editors;

namespace StellarAdmin.Areas.StellarAdmin.ViewModels.Internal;

internal sealed record MultiLookupItemsViewModel(
    MultiLookupSheetEditor Editor,
    IReadOnlyList<ChoiceItem> Items,
    string Label
);
