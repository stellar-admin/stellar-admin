using StellarAdmin.Dashboard.Resources.Options;
using StellarAdmin.TagHelpers;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures an untitled group of a view page's fields.
/// </summary>
public sealed class ResourceViewGroupBuilder<TModel> : ResourceViewFieldsBuilder<TModel>
{
    private FormColumnSpanDefinitions _columnSpan;
    private FormColumnStartDefinitions _columnStart;

    internal ResourceViewGroupBuilder(Action<Action<IFormScope>> configure)
        : base(configure) { }

    /// <summary>
    ///     Sets the number of columns the group spans in its parent.
    /// </summary>
    /// <remarks>
    ///     Defaults to 1.
    /// </remarks>
    public ResourceViewGroupBuilder<TModel> ColumnSpan(int span)
    {
        _columnSpan = ColumnSpanBuilder.FromSpan(span);

        return this;
    }

    /// <summary>
    ///     Sets the number of columns the group spans in its parent at each breakpoint.
    /// </summary>
    public ResourceViewGroupBuilder<TModel> ColumnSpan(Action<ColumnSpanBuilder> configure)
    {
        _columnSpan = ColumnSpanBuilder.Build(configure);

        return this;
    }

    /// <summary>
    ///     Spans all columns of the group's parent.
    /// </summary>
    public ResourceViewGroupBuilder<TModel> ColumnSpanFull()
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
    public ResourceViewGroupBuilder<TModel> ColumnStart(int start)
    {
        _columnStart = ColumnStartBuilder.FromStart(start);

        return this;
    }

    /// <summary>
    ///     Sets the column the group starts at in its parent, at each breakpoint.
    /// </summary>
    public ResourceViewGroupBuilder<TModel> ColumnStart(Action<ColumnStartBuilder> configure)
    {
        _columnStart = ColumnStartBuilder.Build(configure);

        return this;
    }

    internal FormGroupOptions Build() =>
        new() { ColumnSpan = _columnSpan, ColumnStart = _columnStart };
}
