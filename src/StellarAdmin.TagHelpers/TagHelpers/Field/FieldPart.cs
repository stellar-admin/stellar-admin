namespace StellarAdmin.TagHelpers;

/// <summary>
///     A supporting part that a field can automatically render around its control.
/// </summary>
public enum FieldPart
{
    /// <summary>
    ///     The field's label.
    /// </summary>
    Label,

    /// <summary>
    ///     The field's description text.
    /// </summary>
    Description,

    /// <summary>
    ///     The field's validation error message.
    /// </summary>
    Error,
}
