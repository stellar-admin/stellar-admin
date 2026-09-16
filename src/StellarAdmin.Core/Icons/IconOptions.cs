namespace StellarAdmin.Icons;

/// <summary>
///     Configures the icons available to StellarAdmin.
/// </summary>
public class IconOptions
{
    /// <summary>
    ///     The icon definitions keyed by name.
    /// </summary>
    /// <remarks>
    ///     Includes Lucide icons by default.
    /// </remarks>
    public IDictionary<string, IconDefinition> Icons { get; } =
        new Dictionary<string, IconDefinition>(
            LucideIcons.IconDefinitions,
            StringComparer.OrdinalIgnoreCase
        );
}
