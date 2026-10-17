namespace StellarAdmin.Areas.StellarAdmin.ViewModels;

/// <summary>
///     Represents the delete confirmation dialog of an index page.
/// </summary>
/// <remarks>
///     One dialog serves every row.
/// </remarks>
public sealed class ResourceIndexDeleteDialogViewModel
{
    /// <summary>
    ///     Gets the label of the cancel button.
    /// </summary>
    public string CancelLabel { get; }

    /// <summary>
    ///     Gets the label of the confirm button.
    /// </summary>
    public string ConfirmLabel { get; }

    /// <summary>
    ///     Gets the confirmation message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    ///     Gets the title of the dialog.
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
