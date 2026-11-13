namespace StellarAdmin.UI;

/// <summary>
///     Specifies the size of a checklist, which sets the size of its status icons and the spacing
///     between its rows.
/// </summary>
[GenerateDataAttributeText]
public enum ChecklistSize
{
    /// <summary>
    ///     The standard size.
    /// </summary>
    Default,

    /// <summary>
    ///     A compact size, for a checklist in a card or nested in another checklist.
    /// </summary>
    [DataAttributeText("sm")]
    Small,
}
