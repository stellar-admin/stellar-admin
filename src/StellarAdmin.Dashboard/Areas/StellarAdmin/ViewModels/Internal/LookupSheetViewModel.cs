using StellarAdmin.Dashboard.Resources;
using StellarAdmin.Dashboard.Resources.Editors;

namespace StellarAdmin.Dashboard.Areas.StellarAdmin.ViewModels.Internal;

internal sealed record LookupSheetViewModel(
    ILookupSheetEditor Editor,
    string For,
    string Title,
    string ResultsUrl,
    LookupLabelContext Labels,
    string? CreateUrl
);
