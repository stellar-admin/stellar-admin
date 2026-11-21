using StellarAdmin.Resources.Displays;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures the badge on the cards of a cards data layout.
/// </summary>
public sealed class ResourceCardBadgeBuilder
{
    private readonly ResourceCardEntryBuilder _entry;

    internal ResourceCardBadgeBuilder(ResourceCardEntryBuilder entry) => _entry = entry;

    /// <summary>
    ///     Sets and configures the display that renders the badge.
    /// </summary>
    /// <typeparam name="TDisplay">The type of the display.</typeparam>
    /// <param name="configure">A delegate that configures the display.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     A display with a handler is a configuration problem, and the badge keeps
    ///     <see cref="BadgeDisplay" />.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceCardBadgeBuilder UseDisplay<TDisplay>(Action<TDisplay> configure)
        where TDisplay : FieldDisplay, new()
    {
        _entry.UseDisplay(configure);

        return this;
    }

    /// <summary>
    ///     Sets the display that renders the badge.
    /// </summary>
    /// <typeparam name="TDisplay">The type of the display.</typeparam>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     A display with a handler is a configuration problem, and the badge keeps
    ///     <see cref="BadgeDisplay" />.
    /// </remarks>
    public ResourceCardBadgeBuilder UseDisplay<TDisplay>()
        where TDisplay : FieldDisplay, new() => UseDisplay<TDisplay>(_ => { });

    internal FormDisplayFieldOptions Build() => _entry.Build();
}
