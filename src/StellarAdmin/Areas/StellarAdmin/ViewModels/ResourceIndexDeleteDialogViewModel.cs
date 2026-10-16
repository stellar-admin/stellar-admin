namespace StellarAdmin.Areas.StellarAdmin.ViewModels;

/// <summary>
///     The delete confirmation dialog of an index page.
///     One dialog serves every row. The message for the chosen row is filled in when the dialog opens.
/// </summary>
public sealed class ResourceIndexDeleteDialogViewModel
{
    /// <summary>
    ///     The label of the cancel button.
    /// </summary>
    public string CancelLabel { get; }

    /// <summary>
    ///     The label of the confirming button.
    /// </summary>
    public string ConfirmLabel { get; }

    /// <summary>
    ///     The confirmation message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    ///     The dialog title.
    /// </summary>
    public string Title { get; }

    internal ResourceIndexDeleteDialogViewModel(
        string title,
        string message,
        string confirmLabel,
        string cancelLabel
    )
    {
        Title = title;
        Message = message;
        ConfirmLabel = confirmLabel;
        CancelLabel = cancelLabel;
    }
}
