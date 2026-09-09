namespace StellarAdmin.TagHelpers;

/// <summary>
///     Additional CSS classes for the parts of a select.
/// </summary>
public class SelectClassNames : FieldClassNames
{
    /// <summary>
    ///     Additional CSS classes for the select container.
    /// </summary>
    public string? Control { get; set; }

    /// <summary>
    ///     Additional CSS classes for the dropdown icon.
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    ///     Additional CSS classes for the select element.
    /// </summary>
    public string? Input { get; set; }
}
