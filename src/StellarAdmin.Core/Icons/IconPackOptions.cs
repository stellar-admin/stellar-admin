namespace StellarAdmin.Icons;

/// <summary>
///     Configures the registration of an icon pack.
/// </summary>
public class IconPackOptions
{
    /// <summary>
    ///     Whether to import the pack's semantic icon mappings.
    /// </summary>
    /// <remarks>
    ///     Defaults to true.
    /// </remarks>
    public bool ImportSemanticMappings { get; set; } = true;

    /// <summary>
    ///     The literal prefix added to each icon name.
    /// </summary>
    /// <remarks>
    ///     Defaults to null, which leaves names unchanged.
    /// </remarks>
    public string? Prefix { get; set; }
}
