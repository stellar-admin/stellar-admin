using Microsoft.Extensions.Primitives;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources;

/// <summary>
///     Describes the form field a <see cref="FieldBinder" /> reads or binds.
/// </summary>
public sealed class FieldBindingContext
{
    private readonly List<string> _errors = [];

    /// <summary>
    ///     Gets the field.
    /// </summary>
    public FormFieldOptions Field { get; }

    /// <summary>
    ///     Gets the model the form edits.
    /// </summary>
    public object Model { get; }

    /// <summary>
    ///     Gets the services of the current request.
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    ///     Gets the values posted for the field.
    /// </summary>
    /// <value>
    ///     The posted values, or no values when the field's value is read.
    /// </value>
    public StringValues Values { get; }

    internal IReadOnlyList<string> Errors => _errors;

    internal FieldBindingContext(
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

    /// <summary>
    ///     Reports a posted value that can't be applied. The form shows the message under the field.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="message" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="message" /> is empty or consists only of white space.
    /// </exception>
    public void AddError(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        _errors.Add(message);
    }
}
