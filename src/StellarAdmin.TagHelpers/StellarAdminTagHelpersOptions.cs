namespace StellarAdmin.TagHelpers;

/// <summary>
///     Root options for configuring the StellarAdmin tag helpers. Configure it via the <see cref="StellarAdminTagHelpersBuilder" />
///     returned from <c>AddStellarAdmin().AddTagHelpers()</c>
/// </summary>
public class StellarAdminTagHelpersOptions
{
    /// <summary>
    ///     Defaults for floating menu surfaces (Dropdown Menu, and future menu families).
    /// </summary>
    public StellarAdminTagHelpersMenuOptions Menu { get; } = new();
}
