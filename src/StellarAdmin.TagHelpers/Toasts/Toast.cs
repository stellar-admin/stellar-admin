namespace StellarAdmin.TagHelpers;

/// <summary>
///     A notification shown briefly in the toaster.
/// </summary>
public sealed class Toast
{
    /// <summary>
    ///     An optional action button.
    /// </summary>
    public ToastAction? Action { get; init; }

    /// <summary>
    ///     Optional text shown below the title.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    ///     How long the toast stays visible.
    /// </summary>
    /// <remarks>
    ///     Defaults to the toaster's duration. <see cref="TimeSpan.Zero" /> keeps the toast open until it is closed.
    /// </remarks>
    public TimeSpan? Duration { get; init; }

    /// <summary>
    ///     The toast title.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    ///     The kind of message.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="ToastType.Default" />.
    /// </remarks>
    public ToastType Type { get; init; }
}
