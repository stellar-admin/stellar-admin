namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the configuration of resource deletion.
/// </summary>
public sealed class ResourceDeleteOptions
{
    /// <summary>
    ///     Gets or sets the label of the cancel button.
    /// </summary>
    /// <value>
    ///     The text, or <see langword="null" /> to use
    ///     <see cref="ResourceDeleteLabelOptions.CancelLabel" />. The default is
    ///     <see langword="null" />.
    /// </value>
    public string? CancelLabel { get; set; }

    /// <summary>
    ///     Gets or sets the label of the button that confirms the deletion.
    /// </summary>
    /// <value>
    ///     The text, or <see langword="null" /> to use
    ///     <see cref="ResourceDeleteLabelOptions.ConfirmLabel" />. The default is
    ///     <see langword="null" />.
    /// </value>
    public string? ConfirmLabel { get; set; }

    /// <summary>
    ///     Gets or sets the confirmation message.
    /// </summary>
    /// <value>
    ///     The text, or <see langword="null" /> to use
    ///     <see cref="ResourceDeleteLabelOptions.Message" />. The default is
    ///     <see langword="null" />.
    /// </value>
    public string? Message { get; set; }

    /// <summary>
    ///     Gets or sets the confirmation title.
    /// </summary>
    /// <value>
    ///     The text, or <see langword="null" /> to use
    ///     <see cref="ResourceDeleteLabelOptions.Title" />. The default is <see langword="null" />.
    /// </value>
    public string? Title { get; set; }
}
