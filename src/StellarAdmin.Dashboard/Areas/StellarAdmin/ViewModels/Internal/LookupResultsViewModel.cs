using StellarAdmin.Dashboard.Resources;
using StellarAdmin.Dashboard.Resources.Editors;

namespace StellarAdmin.Dashboard.Areas.StellarAdmin.ViewModels.Internal;

internal sealed record LookupResultsViewModel(
    LookupEditor Editor,
    IReadOnlyList<LookupResult> Items,
    string? MoreUrl,
    string? Selected,
    string? Term,
    bool IsTermTooShort,
    bool IsEmpty,
    LookupLabelContext Labels
);
