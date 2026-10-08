using StellarAdmin.Dashboard.Resources;
using StellarAdmin.Dashboard.Resources.Editors;

namespace StellarAdmin.Dashboard.Areas.StellarAdmin.ViewModels.Internal;

internal sealed record MultiLookupItemsViewModel(
    MultiLookupSheetEditor Editor,
    IReadOnlyList<ChoiceItem> Items,
    string Label
);
