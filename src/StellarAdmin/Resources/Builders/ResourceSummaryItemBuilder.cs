using StellarAdmin.Resources.Displays;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures a field of a record summary.
/// </summary>
public sealed class ResourceSummaryItemBuilder
{
    private readonly ResourceDisplayFieldBuilder _field;

    internal ResourceSummaryItemBuilder(ResourceDisplayFieldBuilder field) => _field = field;

    /// <summary>
    ///     Renders the field with a display and configures it.
    /// </summary>
    /// <typeparam name="TDisplay">The type of the display.</typeparam>
    /// <param name="configure">A delegate that configures the display.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     The summary renders the display's compact template, as index grid cells do. Without a
    ///     display, the field renders the template its property's <c>[UIHint]</c> names, or the
    ///     default display for its type.
    /// </remarks>
    public ResourceSummaryItemBuilder UseDisplay<TDisplay>(Action<TDisplay> configure)
        where TDisplay : FieldDisplay, new()
    {
        _field.UseDisplay(configure);

        return this;
    }

    /// <summary>
    ///     Renders the field with a display.
    /// </summary>
    /// <typeparam name="TDisplay">The type of the display.</typeparam>
    /// <returns>The builder, so that calls can be chained.</returns>
    public ResourceSummaryItemBuilder UseDisplay<TDisplay>()
        where TDisplay : FieldDisplay, new() => UseDisplay<TDisplay>(_ => { });

    /// <summary>
    ///     Renders the value from the property's metadata instead of a display.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     The value is formatted with the property's <c>[DisplayFormat]</c>.
    /// </remarks>
    public ResourceSummaryItemBuilder UseBuiltInRendering()
    {
        _field.UseBuiltInRendering();

        return this;
    }
}
