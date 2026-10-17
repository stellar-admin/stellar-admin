namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Describes the form field an editor handler prepares.
/// </summary>
/// <param name="fieldName">The property path of the field.</param>
/// <param name="model">The model the form renders.</param>
/// <param name="value">The field's current value.</param>
public sealed class FieldEditorContext(string fieldName, object model, object? value)
{
    /// <summary>
    ///     Gets the property path of the field.
    /// </summary>
    public string FieldName { get; } = fieldName;

    /// <summary>
    ///     Gets the model the form renders.
    /// </summary>
    public object Model { get; } = model;

    /// <summary>
    ///     Gets the field's current value.
    /// </summary>
    public object? Value { get; } = value;
}
