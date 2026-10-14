namespace StellarAdmin.Resources.Displays;

/// <summary>
///     Displays a web address as a link. Other values display as text.
/// </summary>
/// <remarks>
///     The default display for properties marked <c>[Url]</c>.
/// </remarks>
public sealed class LinkDisplay : FieldDisplay
{
    /// <inheritdoc />
    public override string TemplateName => "Displays/Link";

    /// <summary>
    ///     Whether the link opens in a new tab.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>true</c>.
    /// </remarks>
    public bool OpenInNewTab { get; set; } = true;
}
