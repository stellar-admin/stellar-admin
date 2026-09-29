namespace StellarAdmin.TagHelpers;

/// <summary>
///     The kind of message a toast shows.
/// </summary>
public enum ToastType
{
    /// <summary>
    ///     A neutral message.
    /// </summary>
    Default,

    /// <summary>
    ///     An operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    ///     An informational message.
    /// </summary>
    Info,

    /// <summary>
    ///     A message that needs attention.
    /// </summary>
    Warning,

    /// <summary>
    ///     An operation failed.
    /// </summary>
    Error,
}
