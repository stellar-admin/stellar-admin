using System.Linq.Expressions;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures the record summary shown under the title of the view and edit pages.
/// </summary>
/// <typeparam name="TModel">The type of the model the summary reads.</typeparam>
/// <remarks>
///     The summary shows its items in order, in one row that wraps. A field renders its value only,
///     without a label; add text before it to label it.
/// </remarks>
public sealed class ResourceSummaryBuilder<TModel>
{
    private readonly Action<Action<ResourceSummaryOptions>> _configure;

    /// <summary>
    ///     Sets whether a separator shows between every two items.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> to separate the items; otherwise, <see langword="false" />. The
    ///     default is <see langword="false" />.
    /// </value>
    /// <remarks>
    ///     Don't also add separators with <see cref="AddSeparator" />.
    /// </remarks>
    public bool SeparateItems
    {
        set
        {
            var source = ConfigurationSource.Capture();
            _configure(options =>
            {
                options.SeparateItems = value;
                options.SeparateItemsSource = source;
            });
        }
    }

    internal ResourceSummaryBuilder(Action<Action<ResourceSummaryOptions>> configure) =>
        _configure = configure;

    /// <summary>
    ///     Adds a field.
    /// </summary>
    /// <typeparam name="TProperty">The type of the selected property.</typeparam>
    /// <param name="field">
    ///     An expression that selects the property the field shows, or computes the value it shows.
    /// </param>
    /// <returns>A builder that configures the field.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="field" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     An empty value shows the property's <c>[DisplayFormat(NullDisplayText)]</c>, or a dash
    ///     when the property doesn't set one.
    /// </remarks>
    public ResourceSummaryItemBuilder Add<TProperty>(Expression<Func<TModel, TProperty>> field)
    {
        var fieldBuilder = ResourceDisplayFieldBuilder.Create(field, nameof(field));
        _configure(options => options.Items.Add(new SummaryFieldItem(fieldBuilder.Build())));

        return new(fieldBuilder);
    }

    /// <summary>
    ///     Adds and configures a field.
    /// </summary>
    /// <typeparam name="TProperty">The type of the selected property.</typeparam>
    /// <param name="field">
    ///     An expression that selects the property the field shows, or computes the value it shows.
    /// </param>
    /// <param name="configure">A delegate that configures the field.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="field" /> or <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceSummaryBuilder<TModel> Add<TProperty>(
        Expression<Func<TModel, TProperty>> field,
        Action<ResourceSummaryItemBuilder> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(Add(field));

        return this;
    }

    /// <summary>
    ///     Adds a separator.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     Not needed with <see cref="SeparateItems" />, which separates every two items.
    /// </remarks>
    public ResourceSummaryBuilder<TModel> AddSeparator()
    {
        var source = ConfigurationSource.Capture();
        _configure(options => options.Items.Add(new SummarySeparatorItem(source)));

        return this;
    }

    /// <summary>
    ///     Adds text.
    /// </summary>
    /// <param name="text">The text, shown as given.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="text" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Text labels the field after it, or shows the same words for every record.
    /// </remarks>
    public ResourceSummaryBuilder<TModel> AddText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        _configure(options => options.Items.Add(new SummaryTextItem(text)));

        return this;
    }

    /// <summary>
    ///     Removes the items added so far.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     A summary without items isn't shown.
    /// </remarks>
    public ResourceSummaryBuilder<TModel> Clear()
    {
        _configure(options => options.Items.Clear());

        return this;
    }
}
