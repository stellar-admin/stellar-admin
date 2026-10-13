using StellarAdmin.Dashboard.Resources.Options;
using StellarAdmin.TagHelpers;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures a titled section of a view page and its fields.
/// </summary>
public sealed class ResourceViewSectionBuilder<TModel> : ResourceViewFieldsBuilder<TModel>
{
    private readonly string _title;
    private FormColumnSpanDefinitions _columnSpan;
    private FormColumnStartDefinitions _columnStart;
    private string? _description;
    private FormSectionLayout? _layout;
    private FieldOrientation? _orientation;

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
    ///     Defaults to the page's section layout.
    /// </remarks>
    public FormSectionLayout? Layout
    {
        set => _layout = value;
    }

    /// <summary>
    ///     Whether the labels of the section's fields display above or beside their values.
    /// </summary>
    /// <remarks>
    ///     Defaults to the orientation of the parent section, then above. A field's own orientation wins.
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
    /// <remarks>
    ///     Defaults to 1.
    /// </remarks>
    public ResourceViewSectionBuilder<TModel> ColumnSpan(int span)
    {
        _columnSpan = ColumnSpanBuilder.FromSpan(span);

        return this;
    }

    /// <summary>
    ///     Sets the number of columns the section spans in its parent at each breakpoint.
    /// </summary>
    public ResourceViewSectionBuilder<TModel> ColumnSpan(Action<ColumnSpanBuilder> configure)
    {
        _columnSpan = ColumnSpanBuilder.Build(configure);

        return this;
    }

    /// <summary>
    ///     Spans all columns of the section's parent.
    /// </summary>
    public ResourceViewSectionBuilder<TModel> ColumnSpanFull()
    {
        _columnSpan = new(Base: ResponsiveGrid.FullSpan);

        return this;
    }

    /// <summary>
    ///     Sets the column the section starts at in its parent.
    /// </summary>
    /// <remarks>
    ///     Defaults to following the item before it. Ignored where the parent has one column or
    ///     the section would extend past the last column.
    /// </remarks>
    public ResourceViewSectionBuilder<TModel> ColumnStart(int start)
    {
        _columnStart = ColumnStartBuilder.FromStart(start);

        return this;
    }

    /// <summary>
    ///     Sets the column the section starts at in its parent, at each breakpoint.
    /// </summary>
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
