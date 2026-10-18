namespace StellarAdmin.UI;

/// <summary>
///     Holds the CSS classes added to the parts of an icon picker.
/// </summary>
public class IconPickerClassNames : FieldClassNames
{
    /// <summary>
    ///     Gets or sets the CSS classes added to the icon picker's container.
    /// </summary>
    public string? Control { get; set; }

    /// <summary>
    ///     Gets or sets the CSS classes added to each tile of an inline icon picker.
    /// </summary>
    public string? Item { get; set; }

    /// <summary>
    ///     Gets or sets the CSS classes added to the popover.
    /// </summary>
    public string? Popover { get; set; }

    /// <summary>
    ///     Gets or sets the CSS classes added to the button that opens the popover.
    /// </summary>
    public string? Trigger { get; set; }
}
