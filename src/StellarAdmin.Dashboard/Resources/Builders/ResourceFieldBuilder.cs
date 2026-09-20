using System.Linq.Expressions;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures a form field.
/// </summary>
public sealed class ResourceFieldBuilder
{
    private readonly List<Action<FormFieldOptions>> _configuration = [];
    private readonly LambdaExpression _field;

    /// <summary>
    ///     The field label.
    /// </summary>
    public string? Title
    {
        set => _configuration.Add(options => options.Title = value);
    }

    internal ResourceFieldBuilder(LambdaExpression field) => _field = field;

    internal FormFieldOptions Build()
    {
        var options = new FormFieldOptions(_field, ((MemberExpression)_field.Body).Member.Name);
        foreach (var configure in _configuration)
        {
            configure(options);
        }

        return options;
    }
}
