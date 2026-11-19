using System.Linq.Expressions;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures the fields of the cards of a cards data layout.
/// </summary>
/// <typeparam name="TResource">The type of the resource the index lists.</typeparam>
public sealed class ResourceCardFieldsBuilder<TResource>
{
    private readonly Action<Func<FormDisplayFieldOptions>> _add;
    private readonly Action<ResourceCardFieldLayout> _setLayout;

    /// <summary>
    ///     Sets how the fields are arranged.
    /// </summary>
    /// <value>
    ///     The data layout. The default is <see cref="ResourceCardFieldLayout.Rows" />.
    /// </value>
    public ResourceCardFieldLayout Layout
    {
        set => _setLayout(value);
    }

    internal ResourceCardFieldsBuilder(
        Action<Func<FormDisplayFieldOptions>> add,
        Action<ResourceCardFieldLayout> setLayout
    )
    {
        _add = add;
        _setLayout = setLayout;
    }

    /// <summary>
    ///     Adds a property as a field.
    /// </summary>
    /// <typeparam name="TProperty">The type of the selected property.</typeparam>
    /// <param name="field">An expression that selects the property.</param>
    /// <returns>A builder that configures the field.</returns>
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
    ///     Adds a property as a field, and configures it.
    /// </summary>
    /// <typeparam name="TProperty">The type of the selected property.</typeparam>
    /// <param name="field">An expression that selects the property.</param>
    /// <param name="configure">A delegate that configures the field.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     The value renders with the compact display its property picks, as a table cell does,
    ///     unless <see cref="ResourceCardEntryBuilder.UseDisplay{TDisplay}()" /> sets another.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="field" /> or <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceCardFieldsBuilder<TResource> Add<TProperty>(
        Expression<Func<TResource, TProperty>> field,
        Action<ResourceCardEntryBuilder> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(Add(field));

        return this;
    }
}
