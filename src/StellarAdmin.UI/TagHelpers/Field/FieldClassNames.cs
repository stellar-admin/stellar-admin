namespace StellarAdmin.UI;

/// <summary>
///     Holds the CSS classes added to the shared parts of a field.
/// </summary>
public class FieldClassNames
{
    /// <summary>
    ///     Gets or sets the CSS classes added to the container that holds the label, description
    ///     and error beside the control.
    /// </summary>
    /// <remarks>
    ///     Applies only to a horizontal field, such as a checkbox with its label.
    /// </remarks>
    public string? Content { get; set; }

    /// <summary>
    ///     Gets or sets the CSS classes added to the description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     Gets or sets the CSS classes added to the error message.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    ///     Gets or sets the CSS classes added to the label.
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    ///     Gets or sets the CSS classes added to the field that wraps the control.
    /// </summary>
    /// <remarks>
    ///     Applies only when the control renders its own field.
    /// </remarks>
    public string? Root { get; set; }
}
