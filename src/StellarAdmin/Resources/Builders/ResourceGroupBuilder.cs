using StellarAdmin.Resources.Options;
using StellarAdmin.UI;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures an untitled group of form fields.
/// </summary>
public sealed class ResourceGroupBuilder<TResource> : ResourceFieldsBuilder<TResource>
{
    private FormColumnSpanDefinitions _columnSpan;
    private FormColumnStartDefinitions _columnStart;

    internal ResourceGroupBuilder(Action<Action<IFormScope>> configure)
        : base(configure) { }

    /// <summary>
    ///     Sets the number of columns the group spans in its parent.
    /// </summary>
    /// <remarks>
    ///     Defaults to 1.
    /// </remarks>
    public ResourceGroupBuilder<TResource> ColumnSpan(int span)
    {
        _columnSpan = ColumnSpanBuilder.FromSpan(span);

        return this;
    }

    /// <summary>
    ///     Sets the number of columns the group spans in its parent at each breakpoint.
    /// </summary>
    public ResourceGroupBuilder<TResource> ColumnSpan(Action<ColumnSpanBuilder> configure)
    {
        _columnSpan = ColumnSpanBuilder.Build(configure);

        return this;
    }

    /// <summary>
    ///     Spans all columns of the group's parent.
    /// </summary>
    public ResourceGroupBuilder<TResource> ColumnSpanFull()
    {
        _columnSpan = new(Base: ResponsiveGrid.FullSpan);

        return this;
    }

    /// <summary>
    ///     Sets the column the group starts at in its parent.
    /// </summary>
    /// <remarks>
    ///     Defaults to following the item before it. Ignored where the parent has one column or
    ///     the group would extend past the last column.
    /// </remarks>
    public ResourceGroupBuilder<TResource> ColumnStart(int start)
    {
        _columnStart = ColumnStartBuilder.FromStart(start);

        return this;
    }

    /// <summary>
    ///     Sets the column the group starts at in its parent, at each breakpoint.
    /// </summary>
    public ResourceGroupBuilder<TResource> ColumnStart(Action<ColumnStartBuilder> configure)
    {
        _columnStart = ColumnStartBuilder.Build(configure);

        return this;
    }

    internal FormGroupOptions Build() =>
        new() { ColumnSpan = _columnSpan, ColumnStart = _columnStart };
}
