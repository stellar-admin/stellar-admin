namespace StellarAdmin.TagHelpers;

/// <summary>
///     Additional CSS classes for the parts of an input.
/// </summary>
public class InputClassNames : FieldClassNames
{
    /// <summary>
    ///     Additional CSS classes for the checkbox or radio container.
    /// </summary>
    public string? Control { get; set; }

    /// <summary>
    ///     Additional CSS classes for the checkbox or radio indicator icon.
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    ///     Additional CSS classes for the checkbox or radio indicator.
    /// </summary>
    public string? Indicator { get; set; }

    /// <summary>
    ///     Additional CSS classes for the input element.
    /// </summary>
    public string? Input { get; set; }
}
