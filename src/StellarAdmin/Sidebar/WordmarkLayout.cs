namespace StellarAdmin.Sidebar;

/// <summary>
///     Specifies how a wordmark logo is placed in the sidebar header.
/// </summary>
public enum WordmarkLayout
{
    /// <summary>
    ///     Centers the wordmark at its standard height.
    /// </summary>
    Center,

    /// <summary>
    ///     Aligns the wordmark with the start of the header at its standard height.
    /// </summary>
    Start,

    /// <summary>
    ///     Stretches the wordmark to the header's width, up to twice its standard height.
    /// </summary>
    Fill,
}
