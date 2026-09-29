namespace StellarAdmin.TagHelpers;

/// <summary>
///     An action button shown on a toast.
/// </summary>
public sealed class ToastAction
{
    /// <summary>
    ///     The URL the action navigates to.
    /// </summary>
    public string Href { get; }

    /// <summary>
    ///     The button label.
    /// </summary>
    public string Label { get; }

    private ToastAction(string label, string href)
    {
        Label = label;
        Href = href;
    }

    /// <summary>
    ///     Returns an action that navigates to a URL.
    /// </summary>
    /// <param name="label">The button label.</param>
    /// <param name="href">The URL to navigate to.</param>
    /// <returns>The <see cref="ToastAction" /> instance.</returns>
    /// <exception cref="ArgumentException"></exception>
    public static ToastAction Link(string label, string href)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        ArgumentException.ThrowIfNullOrWhiteSpace(href);

        return new ToastAction(label, href);
    }
}
