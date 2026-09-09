namespace StellarAdmin.TagHelpers;

/// <summary>
///     Additional CSS classes for the parts of an OTP input.
/// </summary>
public class InputOtpClassNames : FieldClassNames
{
    /// <summary>
    ///     Additional CSS classes for the OTP container.
    /// </summary>
    public string? Control { get; set; }

    /// <summary>
    ///     Additional CSS classes for each slot group.
    /// </summary>
    public string? Group { get; set; }

    /// <summary>
    ///     Additional CSS classes for each group separator.
    /// </summary>
    public string? Separator { get; set; }

    /// <summary>
    ///     Additional CSS classes for each character slot.
    /// </summary>
    public string? Slot { get; set; }
}
