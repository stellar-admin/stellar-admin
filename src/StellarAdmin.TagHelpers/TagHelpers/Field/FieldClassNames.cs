namespace StellarAdmin.TagHelpers;

/// <summary>
///     Additional CSS classes for shared field parts.
/// </summary>
public class FieldClassNames
{
    /// <summary>
    ///     Additional CSS classes for the label and supporting text container, when rendered.
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    ///     Additional CSS classes for the help text.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     Additional CSS classes for the validation message.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    ///     Additional CSS classes for the field label.
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    ///     Additional CSS classes for the field wrapper, when rendered.
    /// </summary>
    public string? Root { get; set; }
}
