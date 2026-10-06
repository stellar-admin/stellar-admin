namespace StellarAdmin.Dashboard.Areas.StellarAdmin.ViewModels.Internal;

// An item's display for a lookup editor, with the value the form posts for it
internal sealed record LookupSelectionTemplateViewModel(
    string Value,
    LookupSelectionViewModel Selection
);
