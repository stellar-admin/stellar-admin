using StellarAdmin.Pro.Resources.Options;

namespace StellarAdmin.Pro.Resources.Builders;

/// <summary>
///     Configures a single column of the data grid.
/// </summary>
public class IndexPageColumnBuilder
{
    private readonly DataGridColumnOptions _column;

    internal IndexPageColumnBuilder(DataGridColumnOptions column)
    {
        _column = column;
    }

    /// <summary>
    ///     Sets the composite format string applied to the cell values, like <c>{0:d}</c>,
    ///     overriding a <c>[DisplayFormat]</c> on the property.
    /// </summary>
    public IndexPageColumnBuilder Format(string format)
    {
        ArgumentNullException.ThrowIfNull(format);

        _column.Format = format;

        return this;
    }

    /// <summary>
    ///     Makes the column sortable. Its header renders a sort link, and the page sorts by
    ///     the column when the request asks for it.
    /// </summary>
    public IndexPageColumnBuilder Sortable()
    {
        _column.Sortable = true;

        return this;
    }

    /// <summary>
    ///     Sets the display template that renders the cells, overriding a <c>[UIHint]</c> on
    ///     the property.
    /// </summary>
    public IndexPageColumnBuilder Template(string template)
    {
        ArgumentNullException.ThrowIfNull(template);

        _column.Template = template;

        return this;
    }

    /// <summary>
    ///     Sets the column header title, overriding the title derived from the property
    ///     metadata.
    /// </summary>
    public IndexPageColumnBuilder Title(string title)
    {
        ArgumentNullException.ThrowIfNull(title);

        _column.Title = title;

        return this;
    }
}
