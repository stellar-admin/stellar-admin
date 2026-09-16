using System.Globalization;

namespace StellarAdmin.Pro.Resources.Options;

/// <summary>
///     The configured options for deleting an entity.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class DeleteOptions<TEntity>
    where TEntity : class
{
    /// <summary>The label of the confirmation dialog's cancel button.</summary>
    public string CancelLabel { get; }

    /// <summary>The label of the confirmation dialog's confirming button.</summary>
    public string ConfirmLabel { get; }

    /// <summary>Returns the display name of an entity, for the confirmation message.</summary>
    public Func<TEntity, string?> DisplayName { get; }

    /// <summary>
    ///     The composite format of the confirmation message; <c>{0}</c> is the entity's
    ///     display name.
    /// </summary>
    public string MessageFormat { get; }

    /// <summary>The title of the confirmation dialog.</summary>
    public string Title { get; }

    public DeleteOptions(DeleteDefaults<TEntity> defaults)
    {
        Title = defaults.Title;
        MessageFormat = defaults.MessageFormat;
        ConfirmLabel = defaults.ConfirmLabel;
        CancelLabel = defaults.CancelLabel;
        DisplayName = defaults.DisplayName;
    }

    /// <summary>Returns the confirmation message for an entity.</summary>
    public string FormatMessage(TEntity entity)
    {
        return string.Format(CultureInfo.CurrentCulture, MessageFormat, DisplayName(entity));
    }
}
