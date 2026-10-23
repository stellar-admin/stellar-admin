namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Loads the choices of a choice editor for each request.
/// </summary>
/// <remarks>
///     Integration packages derive from this class to supply choices, such as rows of a database
///     table.
/// </remarks>
public abstract class ChoiceItems
{
    /// <summary>
    ///     Gets the type of the choices' values.
    /// </summary>
    /// <value>
    ///     The type of the values, or <see langword="null" /> when it isn't known. A field whose
    ///     property doesn't hold this type is a configuration problem, and keeps its default editor.
    /// </value>
    public virtual Type? ValueType => null;

    /// <summary>
    ///     Loads the choices.
    /// </summary>
    /// <param name="model">The form's model.</param>
    /// <param name="services">The request's services.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The choices, in display order.</returns>
    /// <remarks>
    ///     When the field depends on other fields, the model holds only their values, and the key
    ///     on the edit form.
    /// </remarks>
    public abstract Task<IReadOnlyList<ChoiceItem>> LoadAsync(
        object model,
        IServiceProvider services,
        CancellationToken cancellationToken
    );
}
