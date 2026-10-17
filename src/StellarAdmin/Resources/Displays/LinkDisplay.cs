namespace StellarAdmin.Resources.Displays;

/// <summary>
///     Shows a web address as a link.
/// </summary>
/// <remarks>
///     Only <c>http</c> and <c>https</c> addresses become links. Other values show as text. This
///     is the default display for properties marked <c>[Url]</c>.
/// </remarks>
public sealed class LinkDisplay : FieldDisplay
{
    /// <inheritdoc />
    public override string TemplateName => "Displays/Link";

    /// <summary>
    ///     Gets or sets a value that indicates whether the link opens in a new browser tab.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the link opens in a new tab; otherwise,
    ///     <see langword="false" />. The default is <see langword="true" />.
    /// </value>
    public bool OpenInNewTab { get; set; } = true;
}
