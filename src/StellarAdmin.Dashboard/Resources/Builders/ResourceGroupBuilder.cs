using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures an untitled group of form fields.
/// </summary>
public sealed class ResourceGroupBuilder<TResource> : ResourceFieldsBuilder<TResource>
{
    private FormColumnSpanDefinitions _columnSpan;

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
        _columnSpan = new(Default: FormColumnSpanDefinitions.Full);

        return this;
    }

    internal FormGroupOptions Build() => new() { ColumnSpan = _columnSpan };
}
