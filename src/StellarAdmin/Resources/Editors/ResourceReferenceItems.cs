namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Supplies the items of fields that refer to a resource, for lookup and choice editors.
/// </summary>
/// <remarks>
///     Integrations derive from this class to load a resource's items from their own data, and set
///     it with <see cref="Options.ResourceOptions{TResource}.UseReferenceItems" />.
/// </remarks>
public abstract class ResourceReferenceItems : LookupItems
{
    /// <summary>
    ///     Returns every item, for a choice editor.
    /// </summary>
    /// <param name="services">The request's service provider.</param>
    /// <param name="model">
    ///     The form the field is on, or <see langword="null" /> when the items load without it.
    /// </param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>The items, in the order they are shown.</returns>
    /// <remarks>
    ///     Format each item's value in the current culture.
    /// </remarks>
    public abstract Task<IReadOnlyList<ChoiceItem>> ListAsync(
        IServiceProvider services,
        object? model,
        CancellationToken cancellationToken
    );
}
