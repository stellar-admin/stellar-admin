namespace StellarAdmin.UI;

/// <summary>
///     Specifies the line drawn between the rows of a checklist.
/// </summary>
[GenerateDataAttributeText]
public enum ChecklistDivider
{
    /// <summary>
    ///     A line that starts at the text, leaving the status icons clear.
    /// </summary>
    Inset,

    /// <summary>
    ///     A line across the full width of the checklist.
    /// </summary>
    FullWidth,

    /// <summary>
    ///     No line.
    /// </summary>
    None,
}
