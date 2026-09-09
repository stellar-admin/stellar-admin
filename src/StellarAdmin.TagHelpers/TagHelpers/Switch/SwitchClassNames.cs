namespace StellarAdmin.TagHelpers;

/// <summary>
///     Additional CSS classes for the parts of a switch.
/// </summary>
public class SwitchClassNames : FieldClassNames
{
    /// <summary>
    ///     Additional CSS classes for the switch container.
    /// </summary>
    public string? Control { get; set; }

    /// <summary>
    ///     Additional CSS classes for the checkbox input.
    /// </summary>
    public string? Input { get; set; }

    /// <summary>
    ///     Additional CSS classes for the switch thumb.
    /// </summary>
    public string? Thumb { get; set; }
}
