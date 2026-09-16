namespace StellarAdmin.Pro.Resources.Options;

/// <summary>
///     The default values a resource seeds its delete confirmation with.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <param name="Title">The title of the confirmation dialog.</param>
/// <param name="MessageFormat">
///     The composite format of the confirmation message; <c>{0}</c> is the entity's
///     display name.
/// </param>
/// <param name="ConfirmLabel">The label of the confirming button.</param>
/// <param name="CancelLabel">The label of the cancel button.</param>
/// <param name="DisplayName">Returns the display name of an entity.</param>
public sealed record DeleteDefaults<TEntity>(
    string Title,
    string MessageFormat,
    string ConfirmLabel,
    string CancelLabel,
    Func<TEntity, string?> DisplayName
)
    where TEntity : class;
