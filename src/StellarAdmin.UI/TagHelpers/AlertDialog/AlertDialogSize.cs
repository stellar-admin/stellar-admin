namespace StellarAdmin.UI;

/// <summary>
///     Specifies the size of an alert dialog.
/// </summary>
[GenerateDataAttributeText]
public enum AlertDialogSize
{
    /// <summary>
    ///     The standard size.
    /// </summary>
    Default,

    /// <summary>
    ///     A compact size.
    /// </summary>
    [DataAttributeText("sm")]
    Small,
}
