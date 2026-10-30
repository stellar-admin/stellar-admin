namespace StellarAdmin.Widgets;

/// <summary>
///     Loads the model of a widget.
/// </summary>
/// <typeparam name="TModel">The type of the widget's model.</typeparam>
public interface IWidgetHandler<TModel>
{
    /// <summary>
    ///     Loads the widget's model.
    /// </summary>
    /// <param name="context">The request the widget loads in.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>
    ///     A task that returns the model, or <see langword="null" /> to hide the widget.
    /// </returns>
    Task<TModel?> LoadAsync(WidgetLoadContext context, CancellationToken cancellationToken);
}

/// <summary>
///     Loads the model of a widget from the widget's parameters.
/// </summary>
/// <typeparam name="TParameters">The type of the widget's parameters.</typeparam>
/// <typeparam name="TModel">The type of the widget's model.</typeparam>
public interface IWidgetHandler<TParameters, TModel>
{
    /// <summary>
    ///     Loads the widget's model.
    /// </summary>
    /// <param name="parameters">
    ///     The parameters that the widget's <c>Parameters</c> delegate returned, with any
    ///     <see cref="WidgetInputAttribute" /> values from the widget's request.
    /// </param>
    /// <param name="context">The request the widget loads in.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>
    ///     A task that returns the model, or <see langword="null" /> to hide the widget.
    /// </returns>
    Task<TModel?> LoadAsync(
        TParameters parameters,
        WidgetLoadContext context,
        CancellationToken cancellationToken
    );
}
