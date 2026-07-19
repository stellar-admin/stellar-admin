namespace StellarAdmin.UI;

/// <summary>
///     Root options for configuring StellarAdmin.UI. Configure it via the
///     <see cref="Builders.StellarAdminBuilder" /> returned from <c>AddStellarAdmin()</c> (for example
///     <see cref="Builders.StellarAdminBuilder.ConfigureMenu" />); components read the effective
///     defaults at render time via <c>IOptions&lt;StellarAdminOptions&gt;</c>.
/// </summary>
public class StellarAdminOptions
{
    /// <summary>Defaults for floating menu surfaces (Dropdown Menu, and future menu families).</summary>
    public StellarAdminMenuOptions Menu { get; } = new();
}
