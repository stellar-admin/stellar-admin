namespace StellarAdmin.TagHelpers;

/// <summary>
///     Additional CSS classes for the parts of a toggle group.
/// </summary>
public class ToggleGroupClassNames : FieldClassNames
{
    /// <summary>
    ///     Additional CSS classes for the toggle group container.
    /// </summary>
    public string? Control { get; set; }

    /// <summary>
    ///     Additional CSS classes for each toggle item.
    /// </summary>
    public string? Item { get; set; }
}
