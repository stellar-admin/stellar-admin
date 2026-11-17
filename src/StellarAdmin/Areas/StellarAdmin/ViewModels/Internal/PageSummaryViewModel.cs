using StellarAdmin.Resources.Options;

namespace StellarAdmin.Areas.StellarAdmin.ViewModels.Internal;

// A view or edit page's record summary, its fields' values read when the page loaded the record, so an edit page whose
// save was rejected still summarizes the stored record
internal sealed record PageSummaryViewModel(IReadOnlyList<PageSummaryItemViewModel> Items);

internal abstract record PageSummaryItemViewModel;

internal sealed record PageSummaryFieldViewModel(
    FormDisplayFieldOptions Field,
    object? Value,
    object? DisplayData
) : PageSummaryItemViewModel;

internal sealed record PageSummaryTextViewModel(string Text) : PageSummaryItemViewModel;

internal sealed record PageSummarySeparatorViewModel : PageSummaryItemViewModel;
