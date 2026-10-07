using StellarAdmin.Dashboard.Resources.Options;
using StellarAdmin.TagHelpers;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures a titled form section and its fields.
/// </summary>
public sealed class ResourceSectionBuilder<TResource> : ResourceFieldsBuilder<TResource>
{
    private readonly string _title;
    private FormColumnSpanDefinitions _columnSpan;
    private string? _description;
    private FormSectionLayout? _layout;

    /// <summary>
    ///     Supporting text displayed below the section title.
    /// </summary>
    public string? Description
    {
        set => _description = value;
    }

    /// <summary>
    ///     The layout of the section.
    /// </summary>
    /// <remarks>
    ///     Defaults to the form's section layout.
    /// </remarks>
    public FormSectionLayout? Layout
    {
        set => _layout = value;
    }

    internal ResourceSectionBuilder(string title, Action<Action<IFormScope>> configure)
        : base(configure) => _title = title;

    /// <summary>
    ///     Sets the number of columns the section spans in its parent.
    /// </summary>
    /// <remarks>
    ///     Defaults to 1.
    /// </remarks>
    public ResourceSectionBuilder<TResource> ColumnSpan(int span)
    {
        _columnSpan = ColumnSpanBuilder.FromSpan(span);

        return this;
    }

    /// <summary>
    ///     Sets the number of columns the section spans in its parent at each breakpoint.
    /// </summary>
    public ResourceSectionBuilder<TResource> ColumnSpan(Action<ColumnSpanBuilder> configure)
    {
        _columnSpan = ColumnSpanBuilder.Build(configure);

        return this;
    }

    /// <summary>
    ///     Spans all columns of the section's parent.
    /// </summary>
    public ResourceSectionBuilder<TResource> ColumnSpanFull()
    {
        _columnSpan = new(Default: FormColumnSpanDefinitions.Full);

        return this;
    }

    internal FormSectionOptions Build() =>
        new(_title)
        {
            ColumnSpan = _columnSpan,
            Description = _description,
            Layout = _layout,
        };
}
