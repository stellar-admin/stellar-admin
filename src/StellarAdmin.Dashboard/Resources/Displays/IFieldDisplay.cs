namespace StellarAdmin.Dashboard.Resources.Displays;

/// <summary>
///     Identifies the handler that prepares a field display.
/// </summary>
public interface IFieldDisplay
{
    /// <summary>
    ///     The handler type.
    /// </summary>
    Type HandlerType { get; }
}

/// <summary>
///     Associates a field display with its handler.
/// </summary>
public interface IFieldDisplay<THandler> : IFieldDisplay
    where THandler : IFieldDisplayHandler
{
    Type IFieldDisplay.HandlerType => typeof(THandler);
}
