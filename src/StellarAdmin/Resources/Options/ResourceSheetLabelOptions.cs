namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the default text of the sheet that pages load content into.
/// </summary>
public sealed class ResourceSheetLabelOptions
{
    /// <summary>
    ///     Gets or sets the description shown when the sheet's content can't be loaded.
    /// </summary>
    /// <value>
    ///     The text. The default is "The content could not be loaded.".
    /// </value>
    public string ErrorDescription { get; set; } = "The content could not be loaded.";

    /// <summary>
    ///     Gets or sets the title shown when the sheet's content can't be loaded.
    /// </summary>
    /// <value>
    ///     The text. The default is "Something went wrong".
    /// </value>
    public string ErrorTitle { get; set; } = "Something went wrong";

    /// <summary>
    ///     Gets or sets the label of the button that loads the sheet's content again.
    /// </summary>
    /// <value>
    ///     The text. The default is "Try again".
    /// </value>
    public string RetryLabel { get; set; } = "Try again";

    /// <summary>
    ///     Gets or sets the description shown when a form in the sheet can't be saved.
    /// </summary>
    /// <value>
    ///     The text. The default is "Your changes could not be saved.".
    /// </value>
    public string SaveErrorDescription { get; set; } = "Your changes could not be saved.";
}
