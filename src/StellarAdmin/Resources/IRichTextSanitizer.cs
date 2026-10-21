namespace StellarAdmin.Resources;

/// <summary>
///     Removes markup that isn't safe to store or show from the HTML of rich text fields.
/// </summary>
/// <remarks>
///     The admin panel sanitizes a rich text field's HTML when the form is saved and again when
///     the field is displayed. The default keeps the markup the rich text editor produces and
///     removes scripts, event handlers, styles and unsafe links. Register an implementation in the
///     service collection to replace it.
/// </remarks>
public interface IRichTextSanitizer
{
    /// <summary>
    ///     Returns the HTML without the markup that isn't allowed.
    /// </summary>
    /// <param name="html">The HTML to sanitize.</param>
    /// <returns>The sanitized HTML.</returns>
    string Sanitize(string html);
}
