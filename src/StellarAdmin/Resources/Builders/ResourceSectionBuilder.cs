using StellarAdmin.Resources.Options;
using StellarAdmin.UI;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures a titled form section and its fields.
/// </summary>
/// <typeparam name="TResource">The type of the resource the form edits.</typeparam>
public sealed class ResourceSectionBuilder<TResource> : ResourceFieldsBuilder<TResource>
{
    private readonly string _title;
    private FormColumnSpanDefinitions _columnSpan;
    private FormColumnStartDefinitions _columnStart;
    private string? _description;
    private FormSectionLayout? _layout;

    /// <summary>
    ///     Sets the supporting text displayed below the section title.
    /// </summary>
    /// <value>
    ///     The supporting text, or <see langword="null" /> for none. The default is
    ///     <see langword="null" />.
    /// </value>
    public string? Description
    {
        set => _description = value;
    }

    /// <summary>
    ///     Sets the layout of the section.
    /// </summary>
    /// <value>
    ///     The layout, or <see langword="null" /> to use the form's section layout. The default is
    ///     <see langword="null" />.
    /// </value>
    public FormSectionLayout? Layout
    {
        set => _layout = value;
    }

    internal ResourceSectionBuilder(string title, Action<Action<IFormScope>> configure)
        : base(configure) => _title = title;

    /// <summary>
    ///     Sets the number of columns the section spans in its parent.
    /// </summary>
    /// <param name="span">The number of columns to span, from 1 to 12.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     Without a span, the section spans one column.
    /// </remarks>
    public ResourceSectionBuilder<TResource> ColumnSpan(int span)
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
    public ResourceSectionBuilder<TResource> ColumnSpan(Action<ColumnSpanBuilder> configure)
    {
        _columnSpan = ColumnSpanBuilder.Build(configure);

        return this;
    }

    /// <summary>
    ///     Makes the section span all columns of its parent.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    public ResourceSectionBuilder<TResource> ColumnSpanFull()
    {
        _columnSpan = new(Base: ResponsiveGrid.FullSpan);

        return this;
    }

    /// <summary>
    ///     Sets the column the section starts at in its parent.
    /// </summary>
    /// <param name="start">The column to start at, from 1 to 12.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     Without a start, the section follows the item before it. The start is ignored where the
    ///     parent has one column or the section would extend past the last column.
    /// </remarks>
    public ResourceSectionBuilder<TResource> ColumnStart(int start)
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
    public ResourceSectionBuilder<TResource> ColumnStart(Action<ColumnStartBuilder> configure)
    {
        _columnStart = ColumnStartBuilder.Build(configure);

        return this;
    }

    internal FormSectionOptions Build() =>
        new(_title)
        {
            ColumnSpan = _columnSpan,
            ColumnStart = _columnStart,
            Description = _description,
            Layout = _layout,
        };
}
