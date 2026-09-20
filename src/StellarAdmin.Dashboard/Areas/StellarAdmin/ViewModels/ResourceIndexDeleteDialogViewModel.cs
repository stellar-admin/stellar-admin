namespace StellarAdmin.Dashboard.Areas.StellarAdmin.ViewModels;

/// <summary>
///     The delete confirmation dialog of an index page. The dialog is a shared shell:
///     the confirmation message for the targeted row arrives at open time.
/// </summary>
/// <param name="Title">The dialog title.</param>
/// <param name="Message">The confirmation message.</param>
/// <param name="ConfirmLabel">The label of the confirming button.</param>
/// <param name="CancelLabel">The label of the cancel button.</param>
public sealed record ResourceIndexDeleteDialogViewModel(
    string Title,
    string Message,
    string ConfirmLabel,
    string CancelLabel
);
