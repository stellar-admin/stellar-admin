namespace StellarAdmin.Areas.StellarAdmin.ViewModels;

/// <summary>
///     The delete confirmation dialog of an edit or view page, bound to the entity the page shows.
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

    /// <summary>The page the delete starts from, which a rejected delete returns to.</summary>
    public string Origin { get; }

    /// <summary>The title of the confirmation dialog, also used as the button label.</summary>
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
