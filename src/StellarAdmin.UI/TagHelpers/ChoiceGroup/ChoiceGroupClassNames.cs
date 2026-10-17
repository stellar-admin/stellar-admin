namespace StellarAdmin.UI;

/// <summary>
///     Additional CSS classes for the parts of a radio or checkbox group.
/// </summary>
public class ChoiceGroupClassNames : FieldClassNames
{
    /// <summary>
    ///     Additional CSS classes for the group container.
    /// </summary>
    public string? Control { get; set; }

    /// <summary>
    ///     Additional CSS classes for each option.
    /// </summary>
    public string? Item { get; set; }
}
