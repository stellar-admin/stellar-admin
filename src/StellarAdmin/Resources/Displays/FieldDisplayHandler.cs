namespace StellarAdmin.Resources.Displays;

/// <summary>
///     Base class for field display handlers.
/// </summary>
public abstract class FieldDisplayHandler<TDisplay>(TDisplay display)
    : IFieldDisplayHandler<TDisplay>
    where TDisplay : FieldDisplay
{
    /// <summary>
    ///     The display's configuration.
    /// </summary>
    protected TDisplay Display { get; } = display;

    /// <inheritdoc />
    public abstract Task<object?> PrepareAsync(
        FieldDisplayContext context,
        CancellationToken cancellationToken
    );
}
