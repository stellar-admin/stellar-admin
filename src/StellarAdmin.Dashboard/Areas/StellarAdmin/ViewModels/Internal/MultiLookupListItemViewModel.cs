using StellarAdmin.Dashboard.Resources.Editors;

namespace StellarAdmin.Dashboard.Areas.StellarAdmin.ViewModels.Internal;

// A multi-select lookup's list row; without a remove label, the row is read-only
internal sealed record MultiLookupListItemViewModel(
    ChoiceItem Item,
    ItemMediaViewModel? Media,
    string? RemoveLabel
);
