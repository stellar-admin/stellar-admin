using StellarAdmin.Resources.Options;
using StellarAdmin.UI;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures a titled section of a view page and its fields.
/// </summary>
/// <typeparam name="TModel">The type of the model the view page displays.</typeparam>
public sealed class ResourceViewSectionBuilder<TModel> : ResourceViewFieldsBuilder<TModel>
{
    private readonly string _title;
    private FormColumnSpanDefinitions _columnSpan;
    private FormColumnStartDefinitions _columnStart;
    private string? _description;
    private FormSectionLayout? _layout;
    private FieldOrientation? _orientation;

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
    ///     The layout, or <see langword="null" /> to use the page's section layout. The default is
    ///     <see langword="null" />.
    /// </value>
    public FormSectionLayout? Layout
    {
        set => _layout = value;
    }

    /// <summary>
    ///     Sets whether the labels of the section's fields are displayed above or beside their
    ///     values.
    /// </summary>
    /// <value>
    ///     The orientation, or <see langword="null" /> to use the orientation of the section that
    ///     contains this one. The default is <see langword="null" />.
    /// </value>
    /// <remarks>
    ///     Labels are displayed above their values when no section sets an orientation. A field's
    ///     own orientation overrides the section's.
    /// </remarks>
    public FieldOrientation? Orientation
    {
        set => _orientation = value;
    }

    internal ResourceViewSectionBuilder(string title, Action<Action<IFormScope>> configure)
        : base(configure) => _title = title;

    /// <summary>
    ///     Sets the number of columns the section spans in its parent.
    /// </summary>
    /// <param name="span">The number of columns to span, from 1 to 12.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     Without a span, the section spans one column.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     <paramref name="span" /> is less than 1 or greater than 12.
    /// </exception>
    public ResourceViewSectionBuilder<TModel> ColumnSpan(int span)
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
    public ResourceViewSectionBuilder<TModel> ColumnSpan(Action<ColumnSpanBuilder> configure)
    {
        _columnSpan = ColumnSpanBuilder.Build(configure);

        return this;
    }

    /// <summary>
    ///     Makes the section span all columns of its parent.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    public ResourceViewSectionBuilder<TModel> ColumnSpanFull()
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
    /// <exception cref="ArgumentOutOfRangeException">
    ///     <paramref name="start" /> is less than 1 or greater than 12.
    /// </exception>
    public ResourceViewSectionBuilder<TModel> ColumnStart(int start)
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
    public ResourceViewSectionBuilder<TModel> ColumnStart(Action<ColumnStartBuilder> configure)
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
            Orientation = _orientation,
        };
}
