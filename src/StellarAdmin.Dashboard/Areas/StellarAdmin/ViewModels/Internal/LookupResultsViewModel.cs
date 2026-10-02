using StellarAdmin.Dashboard.Resources.Editors;

namespace StellarAdmin.Dashboard.Areas.StellarAdmin.ViewModels.Internal;

internal sealed record LookupResultsViewModel(
    IReadOnlyList<LookupResult> Items,
    string? MoreUrl,
    string? Message
);
