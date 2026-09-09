namespace StellarAdmin.TagHelpers;

/// <summary>
///     Additional CSS classes for the parts of an input.
/// </summary>
public class InputClassNames : FieldClassNames
{
    /// <summary>
    ///     Additional CSS classes for the input element or checkbox or radio container.
    /// </summary>
    public string? Control { get; set; }
}
