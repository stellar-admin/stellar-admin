namespace StellarAdmin.UI;

/// <summary>
///     Root options for configuring the StellarAdmin tag helpers. Configure it via the <see cref="StellarAdminUIBuilder" />
///     returned from <c>AddStellarAdminUI()</c>
/// </summary>
public class StellarAdminUIOptions
{
    /// <summary>
    ///     Defaults for floating menu surfaces (Dropdown Menu, and future menu families).
    /// </summary>
    public StellarAdminUIMenuOptions Menu { get; } = new();
}
