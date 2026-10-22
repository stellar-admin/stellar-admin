using StellarAdmin.Resources;
using StellarAdmin.Resources.Editors;

namespace StellarAdmin.Areas.StellarAdmin.ViewModels.Internal;

internal sealed record LookupResultsViewModel(
    ILookupSheetEditor Editor,
    LookupItems ItemSource,
    IReadOnlyList<ChoiceItem> Items,
    string? MoreUrl,
    IReadOnlyCollection<string> Selected,
    string? Term,
    bool IsTermTooShort,
    bool IsEmpty,
    bool IsSelectedOnly,
    LookupLabelContext Labels,
    string? CreateUrl = null
);
