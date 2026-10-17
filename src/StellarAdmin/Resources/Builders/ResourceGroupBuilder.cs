using StellarAdmin.Resources.Options;
using StellarAdmin.UI;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures an untitled group of form fields.
/// </summary>
/// <typeparam name="TResource">The type of the resource the form edits.</typeparam>
public sealed class ResourceGroupBuilder<TResource> : ResourceFieldsBuilder<TResource>
{
    private FormColumnSpanDefinitions _columnSpan;
    private FormColumnStartDefinitions _columnStart;

    internal ResourceGroupBuilder(Action<Action<IFormScope>> configure)
        : base(configure) { }

    /// <summary>
    ///     Sets the number of columns the group spans in its parent.
    /// </summary>
    /// <param name="span">The number of columns to span, from 1 to 12.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     Without a span, the group spans one column.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     <paramref name="span" /> is less than 1 or greater than 12.
    /// </exception>
    public ResourceGroupBuilder<TResource> ColumnSpan(int span)
    {
        _columnSpan = ColumnSpanBuilder.FromSpan(span);

        return this;
    }

    /// <summary>
    ///     Sets the number of columns the group spans in its parent at each breakpoint.
    /// </summary>
    /// <param name="configure">A delegate that sets the span at each breakpoint.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceGroupBuilder<TResource> ColumnSpan(Action<ColumnSpanBuilder> configure)
    {
        _columnSpan = ColumnSpanBuilder.Build(configure);

        return this;
    }

    /// <summary>
    ///     Makes the group span all columns of its parent.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    public ResourceGroupBuilder<TResource> ColumnSpanFull()
    {
        _columnSpan = new(Base: ResponsiveGrid.FullSpan);

        return this;
    }

    /// <summary>
    ///     Sets the column the group starts at in its parent.
    /// </summary>
    /// <param name="start">The column to start at, from 1 to 12.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     Without a start, the group follows the item before it. The start is ignored where the
    ///     parent has one column or the group would extend past the last column.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     <paramref name="start" /> is less than 1 or greater than 12.
    /// </exception>
    public ResourceGroupBuilder<TResource> ColumnStart(int start)
    {
        _columnStart = ColumnStartBuilder.FromStart(start);

        return this;
    }

    /// <summary>
    ///     Sets the column the group starts at in its parent, at each breakpoint.
    /// </summary>
    /// <param name="configure">A delegate that sets the start at each breakpoint.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceGroupBuilder<TResource> ColumnStart(Action<ColumnStartBuilder> configure)
    {
        _columnStart = ColumnStartBuilder.Build(configure);

        return this;
    }

    internal FormGroupOptions Build() =>
        new() { ColumnSpan = _columnSpan, ColumnStart = _columnStart };
}
