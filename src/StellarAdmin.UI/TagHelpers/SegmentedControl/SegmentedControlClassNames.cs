namespace StellarAdmin.UI;

/// <summary>
///     Additional CSS classes for the parts of a segmented control.
/// </summary>
public class SegmentedControlClassNames : FieldClassNames
{
    /// <summary>
    ///     Additional CSS classes for the segmented control container.
    /// </summary>
    public string? Control { get; set; }

    /// <summary>
    ///     Additional CSS classes for each option.
    /// </summary>
    public string? Item { get; set; }
}
