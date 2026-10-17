namespace StellarAdmin.UI;

/// <summary>
///     Holds the CSS classes added to the parts of a one-time code input.
/// </summary>
public class InputOtpClassNames : FieldClassNames
{
    /// <summary>
    ///     Gets or sets the CSS classes added to the container of the slots.
    /// </summary>
    public string? Control { get; set; }

    /// <summary>
    ///     Gets or sets the CSS classes added to each group of slots.
    /// </summary>
    public string? Group { get; set; }

    /// <summary>
    ///     Gets or sets the CSS classes added to each separator between groups.
    /// </summary>
    public string? Separator { get; set; }

    /// <summary>
    ///     Gets or sets the CSS classes added to each character slot.
    /// </summary>
    public string? Slot { get; set; }
}
