namespace StellarAdmin.UI.Icons;

/// <summary>
///     Configures how an icon pack is added.
/// </summary>
public sealed class IconPackOptions
{
    /// <summary>
    ///     Gets or sets a value that indicates whether the pack's semantic icon mappings are added.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the pack's icons are mapped to the semantic roles the pack
    ///     specifies; otherwise, <see langword="false" />. The default is <see langword="true" />.
    /// </value>
    public bool ImportSemanticMappings { get; set; } = true;

    /// <summary>
    ///     Gets or sets the text added to the start of each icon name in the pack.
    /// </summary>
    /// <value>
    ///     The prefix, or <see langword="null" /> to keep the pack's icon names. The default is
    ///     <see langword="null" />.
    /// </value>
    public string? Prefix { get; set; }
}
