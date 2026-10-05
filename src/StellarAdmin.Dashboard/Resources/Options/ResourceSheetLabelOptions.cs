namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     Default text for the sheet that pages load content into.
/// </summary>
public sealed class ResourceSheetLabelOptions
{
    /// <summary>
    ///     The description shown when the sheet's content could not be loaded.
    /// </summary>
    public string ErrorDescription { get; set; } = "The content could not be loaded.";

    /// <summary>
    ///     The title shown when the sheet's content could not be loaded.
    /// </summary>
    public string ErrorTitle { get; set; } = "Something went wrong";

    /// <summary>
    ///     The label of the button that loads the sheet's content again.
    /// </summary>
    public string RetryLabel { get; set; } = "Try again";
}
