namespace StellarAdmin.Resources.Displays;

/// <summary>
///     Prepares the data a field display renders on the current request.
/// </summary>
public interface IFieldDisplayHandler
{
    /// <summary>
    ///     Prepares the display's data, which its template reads as <c>DisplayData</c>.
    /// </summary>
    Task<object?> PrepareAsync(FieldDisplayContext context, CancellationToken cancellationToken);
}

/// <summary>
///     Prepares the data of a specific field display.
/// </summary>
public interface IFieldDisplayHandler<TDisplay> : IFieldDisplayHandler
    where TDisplay : FieldDisplay;
