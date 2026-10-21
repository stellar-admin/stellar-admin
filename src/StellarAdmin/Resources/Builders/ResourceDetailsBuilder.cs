using System.Linq.Expressions;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources.Options;
using StellarAdmin.UI;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures a titled form section that lists read-only fields.
/// </summary>
/// <typeparam name="TResource">The type of the resource the form shows.</typeparam>
public sealed class ResourceDetailsBuilder<TResource>
{
    private readonly List<ResourceDisplayFieldBuilder> _fields = [];
    private readonly ConfigurationSource? _source = ConfigurationSource.Capture();
    private readonly string _title;
    private FormGridColumnDefinitions _columns;
    private FormColumnSpanDefinitions _columnSpan;
    private FormColumnStartDefinitions _columnStart;
    private string? _description;
    private FormSectionLayout? _layout;
    private DetailsOrientation? _orientation;
    private DetailsVariant? _variant;

    /// <summary>
    ///     Sets the supporting text displayed below the section title.
    /// </summary>
    public string? Description
    {
        set => _description = value;
    }

    /// <summary>
    ///     Sets the layout of the section.
    /// </summary>
    /// <value>
    ///     The default is the form's section layout.
    /// </value>
    public FormSectionLayout? Layout
    {
        set => _layout = value;
    }

    /// <summary>
    ///     Sets whether labels display above or beside their values.
    /// </summary>
    /// <value>
    ///     The default is <see cref="DetailsOrientation.Vertical" />, which displays labels above
    ///     their values.
    /// </value>
    public DetailsOrientation? Orientation
    {
        set => _orientation = value;
    }

    /// <summary>
    ///     Sets the visual variant of the list.
    /// </summary>
    /// <value>
    ///     The default is <see cref="DetailsVariant.Default" />.
    /// </value>
    public DetailsVariant? Variant
    {
        set => _variant = value;
    }

    internal ResourceDetailsBuilder(string title) => _title = title;

    /// <summary>
    ///     Adds a read-only field.
    /// </summary>
    /// <typeparam name="TProperty">The type of the field's property.</typeparam>
    /// <param name="field">
    ///     An expression that selects the property the field shows, or computes the value it shows,
    ///     such as <c>tour =&gt; tour.Nights * tour.NightlyRate</c>.
    /// </param>
    /// <returns>A builder that configures the field.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="field" /> is <see langword="null" />.
    /// </exception>
    public ResourceDisplayFieldBuilder Add<TProperty>(Expression<Func<TResource, TProperty>> field)
    {
        var fieldBuilder = ResourceDisplayFieldBuilder.Create(field, nameof(field));
        _fields.Add(fieldBuilder);

        return fieldBuilder;
    }

    /// <summary>
    ///     Adds and configures a read-only field.
    /// </summary>
    /// <typeparam name="TProperty">The type of the field's property.</typeparam>
    /// <param name="field">
    ///     An expression that selects the property the field shows, or computes the value it shows,
    ///     such as <c>tour =&gt; tour.Nights * tour.NightlyRate</c>.
    /// </param>
    /// <param name="configure">A delegate that configures the field.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="field" /> or <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceDetailsBuilder<TResource> Add<TProperty>(
        Expression<Func<TResource, TProperty>> field,
        Action<ResourceDisplayFieldBuilder> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(Add(field));

        return this;
    }

    /// <summary>
    ///     Sets the number of columns the section spans in its parent at every width.
    /// </summary>
    /// <param name="span">The number of columns, from 1 to 12.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     The default is 1.
    /// </remarks>
    public ResourceDetailsBuilder<TResource> ColumnSpan(int span)
    {
        _columnSpan = ColumnSpanBuilder.FromSpan(span);

        return this;
    }

    /// <summary>
    ///     Sets the number of columns the section spans in its parent at each breakpoint.
    /// </summary>
    /// <param name="configure">A delegate that sets the span at each breakpoint.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceDetailsBuilder<TResource> ColumnSpan(Action<ColumnSpanBuilder> configure)
    {
        _columnSpan = ColumnSpanBuilder.Build(configure);

        return this;
    }

    /// <summary>
    ///     Makes the section span all columns of its parent.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    public ResourceDetailsBuilder<TResource> ColumnSpanFull()
    {
        _columnSpan = new(Base: ResponsiveGrid.FullSpan);

        return this;
    }

    /// <summary>
    ///     Sets the column the section starts at in its parent, at every width.
    /// </summary>
    /// <param name="start">The column number, from 1 to 12.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     By default, the section follows the item before it. The start is ignored where the
    ///     parent has one column or the section would extend past the last column.
    /// </remarks>
    public ResourceDetailsBuilder<TResource> ColumnStart(int start)
    {
        _columnStart = ColumnStartBuilder.FromStart(start);

        return this;
    }

    /// <summary>
    ///     Sets the column the section starts at in its parent, at each breakpoint.
    /// </summary>
    /// <param name="configure">A delegate that sets the start at each breakpoint.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceDetailsBuilder<TResource> ColumnStart(Action<ColumnStartBuilder> configure)
    {
        _columnStart = ColumnStartBuilder.Build(configure);

        return this;
    }

    /// <summary>
    ///     Sets the number of columns when the section is at least 40rem wide.
    /// </summary>
    /// <param name="count">The number of columns, from 1 to 12.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     The default is 1. A narrower section uses one column.
    /// </remarks>
    public ResourceDetailsBuilder<TResource> Columns(int count)
    {
        _columns = FormGridColumnDefinitions.FromCount(count);

        return this;
    }

    /// <summary>
    ///     Sets the number of columns at each breakpoint.
    /// </summary>
    /// <param name="configure">
    ///     A delegate that sets the number of columns at each breakpoint.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Breakpoints are measured against the section's width.
    /// </remarks>
    public ResourceDetailsBuilder<TResource> Columns(Action<GridColumnsBuilder> configure)
    {
        _columns = GridColumnsBuilder.Build(configure);

        return this;
    }

    internal FormDetailsOptions Build()
    {
        var options = new FormDetailsOptions(_title)
        {
            Columns = _columns,
            ColumnSpan = _columnSpan,
            ColumnStart = _columnStart,
            Description = _description,
            Layout = _layout,
            Orientation = _orientation,
            Source = _source,
            Variant = _variant,
        };
        options.MutableFields.AddRange(_fields.Select(field => field.Build()));

        return options;
    }
}
