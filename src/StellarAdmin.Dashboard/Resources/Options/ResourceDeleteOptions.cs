namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     Configures resource deletion.
/// </summary>
public sealed class ResourceDeleteOptions
{
    /// <summary>
    ///     The cancel button label.
    /// </summary>
    public string? CancelLabel { get; set; }

    /// <summary>
    ///     The delete confirmation button label.
    /// </summary>
    public string? ConfirmLabel { get; set; }

    /// <summary>
    ///     The delete confirmation message.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    ///     The delete confirmation title.
    /// </summary>
    public string? Title { get; set; }
}
