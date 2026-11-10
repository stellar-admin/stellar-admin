using Microsoft.Extensions.Primitives;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources;

internal sealed class FieldBindingContext
{
    private readonly List<string> _errors = [];

    public FormFieldOptions Field { get; }

    public object Model { get; }

    public IServiceProvider Services { get; }

    // No values when the field's value is read
    public StringValues Values { get; }

    public IReadOnlyList<string> Errors => _errors;

    public FieldBindingContext(
        FormFieldOptions field,
        object model,
        StringValues values,
        IServiceProvider services
    )
    {
        Field = field;
        Model = model;
        Values = values;
        Services = services;
    }

    // The form shows the message under the field
    public void AddError(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        _errors.Add(message);
    }
}
