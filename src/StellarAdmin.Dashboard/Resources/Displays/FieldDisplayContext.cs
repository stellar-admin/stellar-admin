namespace StellarAdmin.Dashboard.Resources.Displays;

/// <summary>
///     The field a display handler prepares data for.
/// </summary>
public sealed class FieldDisplayContext(string fieldName, object model, object? value)
{
    /// <summary>
    ///     The field's property path.
    /// </summary>
    public string FieldName { get; } = fieldName;

    /// <summary>
    ///     The form's model.
    /// </summary>
    public object Model { get; } = model;

    /// <summary>
    ///     The field's value.
    /// </summary>
    public object? Value { get; } = value;
}
