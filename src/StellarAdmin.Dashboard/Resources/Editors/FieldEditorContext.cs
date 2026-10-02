namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     The form field an editor handler prepares.
/// </summary>
public sealed class FieldEditorContext(string fieldName, object model, object? value)
{
    /// <summary>
    ///     The property path of the field.
    /// </summary>
    public string FieldName { get; } = fieldName;

    /// <summary>
    ///     The model the form renders.
    /// </summary>
    public object Model { get; } = model;

    /// <summary>
    ///     The field's current value.
    /// </summary>
    public object? Value { get; } = value;
}
