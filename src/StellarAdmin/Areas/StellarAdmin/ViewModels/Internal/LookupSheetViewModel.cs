using StellarAdmin.Resources;
using StellarAdmin.Resources.Editors;

namespace StellarAdmin.Areas.StellarAdmin.ViewModels.Internal;

internal sealed record LookupSheetViewModel(
    ILookupSheetEditor Editor,
    string For,
    string Title,
    string ResultsUrl,
    LookupLabelContext Labels,
    string? CreateUrl
);
