using StellarAdmin.Dashboard.Resources;
using StellarAdmin.Dashboard.Resources.Editors;

namespace StellarAdmin.Dashboard.Areas.StellarAdmin.ViewModels.Internal;

internal sealed record LookupResultsViewModel(
    ILookupSheetEditor Editor,
    IReadOnlyList<ChoiceItem> Items,
    string? MoreUrl,
    IReadOnlyCollection<string> Selected,
    string? Term,
    bool IsTermTooShort,
    bool IsEmpty,
    bool IsSelectedOnly,
    LookupLabelContext Labels
);
