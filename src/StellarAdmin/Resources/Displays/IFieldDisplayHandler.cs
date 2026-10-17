namespace StellarAdmin.Resources.Displays;

/// <summary>
///     Prepares the data that a field display renders on the current request.
/// </summary>
public interface IFieldDisplayHandler
{
    /// <summary>
    ///     Prepares the display's data.
    /// </summary>
    /// <param name="context">The field to prepare data for.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>A task that returns the data, or <see langword="null" /> for none.</returns>
    /// <remarks>
    ///     The display template reads the data from <c>DisplayData</c>.
    /// </remarks>
    Task<object?> PrepareAsync(FieldDisplayContext context, CancellationToken cancellationToken);
}

/// <summary>
///     Prepares the data for a specific type of field display.
/// </summary>
/// <typeparam name="TDisplay">The type of the display.</typeparam>
public interface IFieldDisplayHandler<TDisplay> : IFieldDisplayHandler
    where TDisplay : FieldDisplay;
