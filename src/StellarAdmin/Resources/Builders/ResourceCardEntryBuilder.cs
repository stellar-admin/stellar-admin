using StellarAdmin.Diagnostics;
using StellarAdmin.Resources.Displays;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures a stat or field of the cards of a cards layout.
/// </summary>
public sealed class ResourceCardEntryBuilder
{
    private readonly ResourceDisplayFieldBuilder _field;
    private readonly List<Action<FormDisplayFieldOptions>> _problems = [];

    /// <summary>
    ///     Sets the text shown when the value is empty.
    /// </summary>
    /// <value>
    ///     The text, or <see langword="null" /> to use the property's
    ///     <c>[DisplayFormat(NullDisplayText)]</c>, or else a dash. The default is
    ///     <see langword="null" />.
    /// </value>
    public string? EmptyText
    {
        set => _field.EmptyText = value;
    }

    /// <summary>
    ///     Sets the label.
    /// </summary>
    /// <value>
    ///     The label, or <see langword="null" /> to use the property's display name. The default
    ///     is <see langword="null" />.
    /// </value>
    public string? Title
    {
        set => _field.Title = value;
    }

    internal ResourceCardEntryBuilder(ResourceDisplayFieldBuilder field) => _field = field;

    /// <summary>
    ///     Renders the value with a display, and configures it.
    /// </summary>
    /// <typeparam name="TDisplay">The type of the display.</typeparam>
    /// <param name="configure">A delegate that configures the display.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     A display with a handler is a configuration problem, and the value keeps its default
    ///     display.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceCardEntryBuilder UseDisplay<TDisplay>(Action<TDisplay> configure)
        where TDisplay : FieldDisplay, new()
    {
        ArgumentNullException.ThrowIfNull(configure);

        // A handler would prepare its data once for every card, so cards leave them out, as columns do
        if (typeof(IFieldDisplay).IsAssignableFrom(typeof(TDisplay)))
        {
            _problems.Add(options =>
                ResourceProblemSink.Report(
                    ConfigurationProblemKind.CardDisplayWithHandler,
                    $"The `{options.FieldName ?? options.Title}` card value doesn't use `{typeof(TDisplay).Name}` because it has a handler, which cards don't support. It uses its default display.",
                    "Use a display without a handler on the card.",
                    options.Source
                )
            );

            return this;
        }

        _field.UseDisplay(configure);

        return this;
    }

    /// <summary>
    ///     Renders the value with a display.
    /// </summary>
    /// <typeparam name="TDisplay">The type of the display.</typeparam>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     A display with a handler is a configuration problem, and the value keeps its default
    ///     display.
    /// </remarks>
    public ResourceCardEntryBuilder UseDisplay<TDisplay>()
        where TDisplay : FieldDisplay, new() => UseDisplay<TDisplay>(_ => { });

    internal FormDisplayFieldOptions Build()
    {
        var options = _field.Build();
        foreach (var report in _problems)
        {
            report(options);
        }

        return options;
    }
}
