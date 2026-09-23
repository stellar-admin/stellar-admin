using System.Linq.Expressions;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures an index column.
/// </summary>
public sealed class ResourceColumnBuilder<TResource>
{
    private readonly List<Action<DataGridColumnOptions>> _configuration = [];
    private readonly LambdaExpression _field;

    /// <summary>
    ///     The composite format string for cell values.
    /// </summary>
    public string? Format
    {
        set => _configuration.Add(options => options.Format = value);
    }

    /// <summary>
    ///     The column title.
    /// </summary>
    public string? Title
    {
        set => _configuration.Add(options => options.Title = value);
    }

    internal ResourceColumnBuilder(LambdaExpression field) => _field = field;

    /// <summary>
    ///     Enables sorting by the column's field.
    /// </summary>
    public ResourceColumnBuilder<TResource> Sortable()
    {
        _configuration.Add(options =>
        {
            options.Sortable = true;
            options.SortExpression = null;
        });

        return this;
    }

    /// <summary>
    ///     Enables sorting by the selected expression.
    /// </summary>
    public ResourceColumnBuilder<TResource> Sortable<TSort>(
        Expression<Func<TResource, TSort>> selector
    )
    {
        ArgumentNullException.ThrowIfNull(selector);

        _configuration.Add(options =>
        {
            options.Sortable = true;
            options.SortExpression = selector;
        });

        return this;
    }

    internal DataGridColumnOptions Build()
    {
        var options = new DataGridColumnOptions(
            _field,
            ResourcePropertyPath.GetProperties(_field) is { } properties
                ? ResourcePropertyPath.GetName(properties)
                : null
        );
        foreach (var configure in _configuration)
        {
            configure(options);
        }

        return options;
    }
}
