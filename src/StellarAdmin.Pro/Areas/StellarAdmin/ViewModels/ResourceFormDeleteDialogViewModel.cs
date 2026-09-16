namespace StellarAdmin.Pro.Areas.StellarAdmin.ViewModels;

/// <summary>
///     The delete confirmation dialog of a form page, bound to the entity the page edits.
/// </summary>
public sealed class ResourceFormDeleteDialogViewModel
{
    /// <summary>The label of the confirmation dialog's cancel button.</summary>
    public string CancelLabel { get; }

    /// <summary>The label of the confirmation dialog's confirming button.</summary>
    public string ConfirmLabel { get; }

    /// <summary>The id of the entity the delete targets.</summary>
    public string EntityId { get; }

    /// <summary>The confirmation message.</summary>
    public string Message { get; }

    /// <summary>The title of the confirmation dialog, also used as the button label.</summary>
    public string Title { get; }

    internal ResourceFormDeleteDialogViewModel(
        string title,
        string message,
        string confirmLabel,
        string cancelLabel,
        string entityId
    )
    {
        Title = title;
        Message = message;
        ConfirmLabel = confirmLabel;
        CancelLabel = cancelLabel;
        EntityId = entityId;
    }
}
