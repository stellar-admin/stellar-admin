namespace StellarAdmin.UI;

/// <summary>
///     Holds additional CSS classes for the parts of a toggle group.
/// </summary>
public class ToggleGroupClassNames : FieldClassNames
{
    /// <summary>
    ///     Gets or sets additional CSS classes for the element that holds the items.
    /// </summary>
    public string? Control { get; set; }

    /// <summary>
    ///     Gets or sets additional CSS classes for each item.
    /// </summary>
    public string? Item { get; set; }
}
