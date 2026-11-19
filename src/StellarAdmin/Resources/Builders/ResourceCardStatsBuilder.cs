using System.Linq.Expressions;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Adds the stats of the cards of a cards data layout.
/// </summary>
/// <typeparam name="TResource">The type of the resource the index lists.</typeparam>
public sealed class ResourceCardStatsBuilder<TResource>
{
    private readonly Action<Func<FormDisplayFieldOptions>> _add;

    internal ResourceCardStatsBuilder(Action<Func<FormDisplayFieldOptions>> add) => _add = add;

    /// <summary>
    ///     Adds a property as a stat.
    /// </summary>
    /// <typeparam name="TProperty">The type of the selected property.</typeparam>
    /// <param name="field">An expression that selects the property.</param>
    /// <returns>A builder that configures the stat.</returns>
    /// <remarks>
    ///     The value renders with the compact display its property picks, as a table cell does,
    ///     unless <see cref="ResourceCardEntryBuilder.UseDisplay{TDisplay}()" /> sets another.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="field" /> is <see langword="null" />.
    /// </exception>
    public ResourceCardEntryBuilder Add<TProperty>(Expression<Func<TResource, TProperty>> field)
    {
        var entry = new ResourceCardEntryBuilder(
            ResourceDisplayFieldBuilder.Create(field, nameof(field))
        );
        _add(entry.Build);

        return entry;
    }

    /// <summary>
    ///     Adds a property as a stat, and configures it.
    /// </summary>
    /// <typeparam name="TProperty">The type of the selected property.</typeparam>
    /// <param name="field">An expression that selects the property.</param>
    /// <param name="configure">A delegate that configures the stat.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     The value renders with the compact display its property picks, as a table cell does,
    ///     unless <see cref="ResourceCardEntryBuilder.UseDisplay{TDisplay}()" /> sets another.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="field" /> or <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceCardStatsBuilder<TResource> Add<TProperty>(
        Expression<Func<TResource, TProperty>> field,
        Action<ResourceCardEntryBuilder> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(Add(field));

        return this;
    }
}
