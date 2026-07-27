namespace StellarAdmin.TagHelpers;

/// <summary>
///     Root options for configuring StellarAdmin UI. Configure it via the <see cref="StellarAdminUIBuilder" />
///     returned from <c>AddStellarAdmin().AddUI()</c>
/// </summary>
public class StellarAdminUIOptions
{
    /// <summary>
    ///     Defaults for floating menu surfaces (Dropdown Menu, and future menu families).
    /// </summary>
    public StellarAdminUIMenuOptions Menu { get; } = new();
}
