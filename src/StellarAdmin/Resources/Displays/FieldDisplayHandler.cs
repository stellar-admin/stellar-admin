namespace StellarAdmin.Resources.Displays;

/// <summary>
///     Provides the base class for field display handlers.
/// </summary>
/// <typeparam name="TDisplay">
///     The type of the display that the handler prepares data for.
/// </typeparam>
/// <param name="display">The display settings of the field.</param>
public abstract class FieldDisplayHandler<TDisplay>(TDisplay display)
    : IFieldDisplayHandler<TDisplay>
    where TDisplay : FieldDisplay
{
    /// <summary>
    ///     Gets the display settings of the field.
    /// </summary>
    protected TDisplay Display { get; } = display;

    /// <inheritdoc />
    public abstract Task<object?> PrepareAsync(
        FieldDisplayContext context,
        CancellationToken cancellationToken
    );
}
