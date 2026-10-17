namespace StellarAdmin.UI;

/// <summary>
///     Specifies additional CSS classes for the parts of a segmented control.
/// </summary>
public class SegmentedControlClassNames : FieldClassNames
{
    /// <summary>
    ///     Gets or sets additional CSS classes for the element that holds the options.
    /// </summary>
    public string? Control { get; set; }

    /// <summary>
    ///     Gets or sets additional CSS classes for each option.
    /// </summary>
    public string? Item { get; set; }
}
