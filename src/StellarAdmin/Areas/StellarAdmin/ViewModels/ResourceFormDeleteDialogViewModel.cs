namespace StellarAdmin.Areas.StellarAdmin.ViewModels;

/// <summary>
///     Represents the delete confirmation dialog of an edit or view page.
/// </summary>
public sealed class ResourceFormDeleteDialogViewModel
{
    /// <summary>
    ///     Gets the label of the dialog's cancel button.
    /// </summary>
    public string CancelLabel { get; }

    /// <summary>
    ///     Gets the label of the dialog's confirm button.
    /// </summary>
    public string ConfirmLabel { get; }

    /// <summary>
    ///     Gets the key of the resource to delete.
    /// </summary>
    public string EntityId { get; }

    /// <summary>
    ///     Gets the confirmation message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    ///     Gets the page that the deletion starts from, <c>edit</c> or <c>view</c>.
    /// </summary>
    /// <remarks>
    ///     A rejected deletion returns to this page.
    /// </remarks>
    public string Origin { get; }

    /// <summary>
    ///     Gets the title of the dialog.
    /// </summary>
    /// <remarks>
    ///     The title is also the label of the button that opens the dialog.
    /// </remarks>
    public string Title { get; }

    internal ResourceFormDeleteDialogViewModel(
        string title,
        string message,
        string confirmLabel,
        string cancelLabel,
        string entityId,
        string origin = "edit"
    )
    {
        Title = title;
        Message = message;
        ConfirmLabel = confirmLabel;
        CancelLabel = cancelLabel;
        EntityId = entityId;
        Origin = origin;
    }
}
