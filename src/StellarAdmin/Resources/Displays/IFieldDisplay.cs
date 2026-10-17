namespace StellarAdmin.Resources.Displays;

/// <summary>
///     Identifies the handler that prepares data for a field display.
/// </summary>
public interface IFieldDisplay
{
    /// <summary>
    ///     Gets the type of the handler.
    /// </summary>
    Type HandlerType { get; }
}

/// <summary>
///     Associates a field display with the handler that prepares its data.
/// </summary>
/// <typeparam name="THandler">The type of the handler.</typeparam>
public interface IFieldDisplay<THandler> : IFieldDisplay
    where THandler : IFieldDisplayHandler
{
    Type IFieldDisplay.HandlerType => typeof(THandler);
}
