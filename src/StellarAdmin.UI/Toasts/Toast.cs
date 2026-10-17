namespace StellarAdmin.UI.Toasts;

/// <summary>
///     Represents a notification shown briefly in the toaster.
/// </summary>
public sealed class Toast
{
    /// <summary>
    ///     Gets the button shown on the toast.
    /// </summary>
    /// <value>
    ///     The button, or <see langword="null" /> to show no button. The default is
    ///     <see langword="null" />.
    /// </value>
    public ToastAction? Action { get; init; }

    /// <summary>
    ///     Gets the text shown below the title.
    /// </summary>
    /// <value>
    ///     The text, or <see langword="null" /> to show only the title. The default is
    ///     <see langword="null" />.
    /// </value>
    public string? Description { get; init; }

    /// <summary>
    ///     Gets how long the toast stays visible.
    /// </summary>
    /// <value>
    ///     The time the toast stays visible, <see cref="TimeSpan.Zero" /> to keep it open until
    ///     it's closed, or <see langword="null" /> to use the toaster's duration. The default is
    ///     <see langword="null" />.
    /// </value>
    /// <remarks>
    ///     The value can't be negative.
    /// </remarks>
    public TimeSpan? Duration { get; init; }

    /// <summary>
    ///     Gets the title of the toast.
    /// </summary>
    /// <remarks>
    ///     The title can't be empty or white space.
    /// </remarks>
    public required string Title { get; init; }

    /// <summary>
    ///     Gets the kind of message the toast shows.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the kind of message. The default is
    ///     <see cref="ToastType.Default" />.
    /// </value>
    public ToastType Type { get; init; }
}
