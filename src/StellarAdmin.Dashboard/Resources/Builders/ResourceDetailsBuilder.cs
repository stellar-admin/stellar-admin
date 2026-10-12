using System.Linq.Expressions;
using StellarAdmin.Dashboard.Resources.Options;
using StellarAdmin.TagHelpers;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures a titled form section that lists read-only fields.
/// </summary>
public sealed class ResourceDetailsBuilder<TResource>
{
    private readonly List<ResourceDisplayFieldBuilder> _fields = [];
    private readonly string _title;
    private FormGridColumnDefinitions _columns;
    private FormColumnSpanDefinitions _columnSpan;
    private FormColumnStartDefinitions _columnStart;
    private string? _description;
    private FormSectionLayout? _layout;
    private FieldOrientation? _orientation;
    private DetailsVariant? _variant;

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

    /// <summary>
    ///     Whether labels display above or beside their values.
    /// </summary>
    /// <remarks>
    ///     Defaults to above.
    /// </remarks>
    public FieldOrientation? Orientation
    {
        set => _orientation = value;
    }

    /// <summary>
    ///     The visual variant of the list.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="DetailsVariant.Default" />.
    /// </remarks>
    public DetailsVariant? Variant
    {
        set => _variant = value;
    }

    internal ResourceDetailsBuilder(string title) => _title = title;

    /// <summary>
    ///     Adds a read-only field.
    /// </summary>
    public ResourceDisplayFieldBuilder Add<TProperty>(Expression<Func<TResource, TProperty>> field)
    {
        var fieldBuilder = ResourceDisplayFieldBuilder.Create(field, nameof(field));
        _fields.Add(fieldBuilder);

        return fieldBuilder;
    }

    /// <summary>
    ///     Adds and configures a read-only field.
    /// </summary>
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
    ///     Sets the number of columns the section spans in its parent.
    /// </summary>
    /// <remarks>
    ///     Defaults to 1.
    /// </remarks>
    public ResourceDetailsBuilder<TResource> ColumnSpan(int span)
    {
        _columnSpan = ColumnSpanBuilder.FromSpan(span);

        return this;
    }

    /// <summary>
    ///     Sets the number of columns the section spans in its parent at each breakpoint.
    /// </summary>
    public ResourceDetailsBuilder<TResource> ColumnSpan(Action<ColumnSpanBuilder> configure)
    {
        _columnSpan = ColumnSpanBuilder.Build(configure);

        return this;
    }

    /// <summary>
    ///     Spans all columns of the section's parent.
    /// </summary>
    public ResourceDetailsBuilder<TResource> ColumnSpanFull()
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
    public ResourceDetailsBuilder<TResource> ColumnStart(int start)
    {
        _columnStart = ColumnStartBuilder.FromStart(start);

        return this;
    }

    /// <summary>
    ///     Sets the column the section starts at in its parent, at each breakpoint.
    /// </summary>
    public ResourceDetailsBuilder<TResource> ColumnStart(Action<ColumnStartBuilder> configure)
    {
        _columnStart = ColumnStartBuilder.Build(configure);

        return this;
    }

    /// <summary>
    ///     Sets the number of columns, starting at the medium breakpoint.
    /// </summary>
    /// <remarks>
    ///     Defaults to 1. Breakpoints are measured against the section's width. Below the medium
    ///     breakpoint, the fields use one column.
    /// </remarks>
    public ResourceDetailsBuilder<TResource> Columns(int count)
    {
        _columns = FormGridColumnDefinitions.FromCount(count);

        return this;
    }

    /// <summary>
    ///     Sets the number of columns at each breakpoint.
    /// </summary>
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
            Variant = _variant,
        };
        options.MutableFields.AddRange(_fields.Select(field => field.Build()));

        return options;
    }
}
