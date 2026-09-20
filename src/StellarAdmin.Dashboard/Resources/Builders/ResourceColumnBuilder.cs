using System.Linq.Expressions;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures an index column.
/// </summary>
public sealed class ResourceColumnBuilder
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

    internal DataGridColumnOptions Build()
    {
        var options = new DataGridColumnOptions(
            _field,
            (_field.Body as MemberExpression)?.Member.Name
        );
        foreach (var configure in _configuration)
        {
            configure(options);
        }

        return options;
    }
}
