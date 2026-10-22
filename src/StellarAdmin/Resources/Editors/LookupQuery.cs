namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Represents a request for a page of lookup items.
/// </summary>
/// <param name="Term">The search text, or <see langword="null" /> to list all items.</param>
/// <param name="Skip">The number of items to skip.</param>
/// <param name="Take">The maximum number of items to return.</param>
public sealed record LookupQuery(string? Term, int Skip, int Take)
{
    /// <summary>
    ///     Gets the model of the form the lookup is on.
    /// </summary>
    /// <value>
    ///     The model, which holds only the values of the fields the lookup's field depends on, and
    ///     the key on the edit form; or <see langword="null" /> when the lookup isn't on a form.
    /// </value>
    public object? Model { get; init; }
}
