namespace StellarAdmin.Resources.Displays;

/// <summary>
///     Provides the field that a display handler prepares data for.
/// </summary>
/// <param name="fieldName">The property path of the field.</param>
/// <param name="model">The model that holds the field.</param>
/// <param name="value">The value of the field.</param>
public sealed class FieldDisplayContext(string fieldName, object model, object? value)
{
    /// <summary>
    ///     Gets the property path of the field.
    /// </summary>
    public string FieldName { get; } = fieldName;

    /// <summary>
    ///     Gets the model that holds the field.
    /// </summary>
    public object Model { get; } = model;

    /// <summary>
    ///     Gets the value of the field.
    /// </summary>
    public object? Value { get; } = value;
}
