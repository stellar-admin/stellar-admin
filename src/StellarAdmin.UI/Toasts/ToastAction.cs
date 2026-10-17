namespace StellarAdmin.UI.Toasts;

/// <summary>
///     Represents a button on a toast.
/// </summary>
public sealed class ToastAction
{
    /// <summary>
    ///     Gets the URL that the button navigates to.
    /// </summary>
    public string Href { get; }

    /// <summary>
    ///     Gets the text of the button.
    /// </summary>
    public string Label { get; }

    private ToastAction(string label, string href)
    {
        Label = label;
        Href = href;
    }

    /// <summary>
    ///     Returns a button that navigates to a URL.
    /// </summary>
    /// <param name="label">The text of the button.</param>
    /// <param name="href">The URL to navigate to.</param>
    /// <returns>The new button.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="label" /> or <paramref name="href" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="label" /> or <paramref name="href" /> is empty or white space.
    /// </exception>
    public static ToastAction Link(string label, string href)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        ArgumentException.ThrowIfNullOrWhiteSpace(href);

        return new ToastAction(label, href);
    }
}
